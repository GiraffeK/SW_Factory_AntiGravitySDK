using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenRecorderApp.Core
{
    /// <summary>
    /// Represents a media frame (audio or video) with presentation timestamp (PTS) and payload data.
    /// </summary>
    public class MediaFrame : IDisposable
    {
        public MediaType Type { get; set; }
        public long PresentationTimestamp { get; set; } // in microseconds or ticks
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public int Width { get; set; }
        public int Height { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }

        public void Dispose()
        {
            Data = Array.Empty<byte>();
        }
    }

    public enum MediaType
    {
        Video,
        AudioSystem,
        AudioMic
    }

    /// <summary>
    /// Lock-free / low-latency ring buffer / queue for audio and video pipeline isolation.
    /// </summary>
    public class LockFreeMediaQueue
    {
        private readonly ConcurrentQueue<MediaFrame> _queue = new();
        private readonly int _maxCapacity;
        private int _droppedFramesCount;

        public LockFreeMediaQueue(int maxCapacity = 300)
        {
            _maxCapacity = maxCapacity;
        }

        public bool Enqueue(MediaFrame frame)
        {
            if (_queue.Count >= _maxCapacity)
            {
                // Drop oldest or current to prevent memory explosion & backpressure lag
                if (_queue.TryDequeue(out var oldFrame))
                {
                    oldFrame.Dispose();
                    Interlocked.Increment(ref _droppedFramesCount);
                }
            }
            _queue.Enqueue(frame);
            return true;
        }

        public bool TryDequeue(out MediaFrame frame)
        {
            return _queue.TryDequeue(out frame!);
        }

        public int Count => _queue.Count;
        public int DroppedFrames => _droppedFramesCount;

        public void Clear()
        {
            while (_queue.TryDequeue(out var frame))
            {
                frame.Dispose();
            }
        }
    }

    /// <summary>
    /// PTS Timestamp Synchronizer and Alignment Engine to prevent audio/video drift.
    /// </summary>
    public class TimestampSynchronizer
    {
        private long _baseSystemTicks = -1;
        private readonly long _timeScale;

        public TimestampSynchronizer(long timeScale = 1000000) // Default microseconds
        {
            _timeScale = timeScale;
        }

        public long GetCurrentPts()
        {
            long ticks = DateTime.UtcNow.Ticks;
            if (_baseSystemTicks == -1)
            {
                Interlocked.CompareExchange(ref _baseSystemTicks, ticks, -1);
            }
            // Convert C# Ticks (100-nanosecond intervals) to target timescale (e.g. microseconds)
            return (ticks - _baseSystemTicks) * _timeScale / TimeSpan.TicksPerSecond;
        }

        public void Reset()
        {
            _baseSystemTicks = -1;
        }
    }
}
