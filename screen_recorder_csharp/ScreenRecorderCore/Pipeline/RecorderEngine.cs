using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ScreenRecorderCore.Models;

namespace ScreenRecorderCore.Pipeline
{
    public class RecorderEngine : IDisposable
    {
        private readonly RecorderConfiguration _config;
        private readonly IVideoCapturer _videoCapturer;
        private readonly IAudioCapturer _audioCapturer;
        private readonly IEncoder _encoder;
        private readonly IMuxer _muxer;

        private readonly Channel<MediaPacket> _packetChannel;
        private CancellationTokenSource? _cts;
        private Task? _muxerTask;
        private bool _isRecording;

        public RecorderEngine(RecorderConfiguration config, IVideoCapturer videoCapturer, IAudioCapturer audioCapturer, IEncoder encoder, IMuxer muxer)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _videoCapturer = videoCapturer ?? throw new ArgumentNullException(nameof(videoCapturer));
            _audioCapturer = audioCapturer ?? throw new ArgumentNullException(nameof(audioCapturer));
            _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
            _muxer = muxer ?? throw new ArgumentNullException(nameof(muxer));

            // Bounded channel acting as lock-free ring buffer with backpressure
            var options = new BoundedChannelOptions(120)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = false,
                SingleReader = true
            };
            _packetChannel = Channel.CreateBounded<MediaPacket>(options);
        }

        public async Task StartAsync(string outputPath, CancellationToken cancellationToken)
        {
            if (_isRecording) return;
            _isRecording = true;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _muxer.Initialize(outputPath, _config.Container);

            // Start Muxer Consumer loop
            _muxerTask = Task.Run(() => MuxerConsumerLoopAsync(_cts.Token), _cts.Token);

            // Hook up capturers to encoder & queue
            _videoCapturer.FrameCaptured += async (surfacePtr, pts, w, h) =>
            {
                try
                {
                    await _encoder.EncodeVideoAsync(surfacePtr, pts, _packetChannel.Writer, _cts.Token).ConfigureAwait(false);
                }
                catch
                {
                    // Ignore dropped frame or cancellation
                }
            };

            _audioCapturer.AudioSampleCaptured += async (pcm, pts, type) =>
            {
                try
                {
                    await _encoder.EncodeAudioAsync(pcm, pts, _packetChannel.Writer, _cts.Token).ConfigureAwait(false);
                }
                catch
                {
                    // Ignore dropped sample or cancellation
                }
            };

            // Start capturing concurrently
            _ = _videoCapturer.StartCaptureAsync(_cts.Token);
            if (_config.EnableAudioLoopback || _config.EnableMicrophone)
            {
                _ = _audioCapturer.StartCaptureAsync(_cts.Token);
            }

            await Task.CompletedTask;
        }

        private async Task MuxerConsumerLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var packet in _packetChannel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
                {
                    await _muxer.WritePacketAsync(packet, cancellationToken).ConfigureAwait(false);
                    packet.Dispose();
                }
            }
            catch (OperationCanceledException)
            {
                // Normal exit
            }
        }

        public async Task StopAsync()
        {
            if (!_isRecording) return;

            _videoCapturer.StopCapture();
            _audioCapturer.StopCapture();

            _cts?.Cancel();
            _packetChannel.Writer.Complete();

            if (_muxerTask != null)
            {
                try
                {
                    await _muxerTask.ConfigureAwait(false);
                }
                catch
                {
                    // Suppress
                }
            }

            await _muxer.FinalizeAsync().ConfigureAwait(false);
            _isRecording = false;
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
            _encoder.Dispose();
            _muxer.Dispose();
        }
    }
}
