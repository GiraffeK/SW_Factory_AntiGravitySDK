using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ScreenRecorderCore.Models;

namespace ScreenRecorderCore.Pipeline
{
    public class WindowsGraphicsCapturer : IVideoCapturer
    {
        private readonly RecorderConfiguration _config;
        private CancellationTokenSource? _cts;
        private bool _isRunning;

        public event Action<IntPtr, long, int, int>? FrameCaptured;

        public WindowsGraphicsCapturer(RecorderConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task StartCaptureAsync(CancellationToken cancellationToken)
        {
            if (_isRunning) return;
            _isRunning = true;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var stopwatch = Stopwatch.StartNew();
            int intervalMs = 1000 / Math.Max(1, _config.TargetFps);

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    long pts = stopwatch.ElapsedMilliseconds;

                    // Zero-copy simulation / integration point with Windows.Graphics.Capture
                    // In real production, GraphicsCaptureItem & Direct3D11CaptureFramePool are utilized here.
                    IntPtr dummySurfacePtr = new IntPtr(0xDEADBEEF);
                    FrameCaptured?.Invoke(dummySurfacePtr, pts, _config.Width, _config.Height);

                    await Task.Delay(intervalMs, _cts.Token).ConfigureAwait(false);
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
