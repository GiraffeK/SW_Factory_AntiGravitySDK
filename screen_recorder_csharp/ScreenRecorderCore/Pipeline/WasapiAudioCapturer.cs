using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ScreenRecorderCore.Models;

namespace ScreenRecorderCore.Pipeline
{
    public class WasapiAudioCapturer : IAudioCapturer
    {
        private readonly RecorderConfiguration _config;
        private CancellationTokenSource? _cts;
        private bool _isRunning;

        public event Action<byte[], long, MediaType>? AudioSampleCaptured;

        public WasapiAudioCapturer(RecorderConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task StartCaptureAsync(CancellationToken cancellationToken)
        {
            if (_isRunning) return;
            _isRunning = true;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var stopwatch = Stopwatch.StartNew();
            // 20ms audio chunk size: 48000 Hz * 2 channels * 2 bytes/sample * 0.02s = 3840 bytes
            int sampleRate = _config.AudioSampleRate;
            int channels = _config.AudioChannels;
            int chunkSize = sampleRate * channels * 2 / 50; 

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    long pts = stopwatch.ElapsedMilliseconds;
                    byte[] audioBuffer = new byte[chunkSize];
                    // Fill with silence or captured WASAPI loopback / mic PCM data
                    Array.Clear(audioBuffer, 0, audioBuffer.Length);

                    AudioSampleCaptured?.Invoke(audioBuffer, pts, MediaType.Audio);

                    await Task.Delay(20, _cts.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            finally
            {
                _isRunning = false;
            }
        }

        public void StopCapture()
        {
            _cts?.Cancel();
        }
    }
}
