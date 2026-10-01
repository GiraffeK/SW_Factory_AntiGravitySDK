using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ScreenRecorder.Core.Models;

namespace ScreenRecorder.Core.Capture
{
    public interface IScreenCaptureEngine
    {
        event EventHandler<VideoFrame>? FrameCaptured;
        bool IsCapturing { get; }
        void StartCapture(int fps, System.Drawing.Rectangle region);
        void StopCapture();
    }

    public class ScreenCaptureEngine : IScreenCaptureEngine
    {
        public event EventHandler<VideoFrame>? FrameCaptured;
        public bool IsCapturing { get; private set; }

        private CancellationTokenSource? _cts;
        private Task? _captureLoopTask;
        private readonly Stopwatch _stopwatch = new();

        public void StartCapture(int fps, System.Drawing.Rectangle region)
        {
            if (IsCapturing) return;
            IsCapturing = true;
            _cts = new CancellationTokenSource();
            _stopwatch.Restart();

            int intervalMs = 1000 / Math.Max(1, fps);

            _captureLoopTask = Task.Run(async () =>
            {
                long frameIndex = 0;
                while (!_cts.Token.IsCancellationRequested)
                {
                    long currentTimestamp = _stopwatch.ElapsedMilliseconds;

                    // Simulate zero-copy frame capture (In production: Windows.Graphics.Capture API)
                    var frame = new VideoFrame
                    {
                        Width = region.Width > 0 ? region.Width : 1920,
                        Height = region.Height > 0 ? region.Height : 1080,
                        TimestampMs = currentTimestamp,
                        IsKeyFrame = frameIndex % (fps * 2) == 0, // Keyframe every 2 seconds
                        Buffer = new byte[1024] // Mock encoded / raw surface pointer
                    };

                    FrameCaptured?.Invoke(this, frame);
                    frameIndex++;

                    try
                    {
                        await Task.Delay(intervalMs, _cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, _cts.Token);
        }

        public void StopCapture()
        {
            if (!IsCapturing) return;
            IsCapturing = false;
            _cts?.Cancel();
            try
            {
                _captureLoopTask?.Wait(2000);
            }
            catch { }
            _cts?.Dispose();
            _cts = null;
            _stopwatch.Stop();
        }
    }
}
