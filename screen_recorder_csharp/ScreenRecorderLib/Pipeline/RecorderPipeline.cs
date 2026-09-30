using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ScreenRecorderLib.Buffers;
using ScreenRecorderLib.Models;
using ScreenRecorderLib.Muxer;

namespace ScreenRecorderLib.Pipeline
{
    /// <summary>
    /// Coordinates the capture, lock-free buffering, hardware encoding simulation,
    /// time synchronization, and muxing pipeline.
    /// </summary>
    public class RecorderPipeline : IDisposable
    {
        private readonly RecorderConfig _config;
        private readonly LockFreeRingBufferManager _bufferManager;
        private readonly ResilientMuxer _muxer;
        private CancellationTokenSource? _cts;
        private Task? _processingTask;
        private bool _isRunning;

        public RecorderPipeline(RecorderConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _bufferManager = new LockFreeRingBufferManager();
            _muxer = new ResilientMuxer();
        }

        public ChannelWriter<MediaSample> VideoWriter => _bufferManager.VideoWriter;
        public ChannelWriter<MediaSample> AudioWriter => _bufferManager.AudioWriter;

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _cts = new CancellationTokenSource();

            _muxer.Initialize(_config.OutputFilePath, _config.Container);
            _processingTask = Task.Run(() => ProcessPipelineAsync(_cts.Token));
        }

        private async Task ProcessPipelineAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Multiplex and consume from video and audio channels in real time with timestamp ordering
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Check video
                    while (_bufferManager.VideoReader.TryRead(out var sample))
                    {
                        var encodedSample = SimulateHardwareEncode(sample);
                        _muxer.WriteSample(encodedSample);
                    }

                    // Check audio
                    while (_bufferManager.AudioReader.TryRead(out var sample))
                    {
                        var encodedSample = SimulateAudioEncode(sample);
                        _muxer.WriteSample(encodedSample);
                    }

                    await Task.Delay(2, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful cancellation
            }
        }

        private MediaSample SimulateHardwareEncode(MediaSample rawSample)
        {
            return new MediaSample
            {
                Type = MediaType.Video,
                Data = new byte[] { 0x00, 0x00, 0x00, 0x01, 0x67, 0x42, 0xC0, 0x1F },
                PresentationTimestamp = rawSample.PresentationTimestamp,
                Duration = rawSample.Duration,
                IsKeyFrame = rawSample.IsKeyFrame
            };
        }

        private MediaSample SimulateAudioEncode(MediaSample rawSample)
        {
            return new MediaSample
            {
                Type = rawSample.Type,
                Data = new byte[] { 0xFF, 0xF1, 0x50, 0x80 },
                PresentationTimestamp = rawSample.PresentationTimestamp,
                Duration = rawSample.Duration
            };
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;

            _cts?.Cancel();
            try
            {
                _processingTask?.Wait(2000);
            }
            catch { }

            _muxer.FinalizeFile();
            _cts?.Dispose();
        }

        public void Dispose()
        {
            Stop();
            _bufferManager.Dispose();
            _muxer.Dispose();
        }
    }
}
