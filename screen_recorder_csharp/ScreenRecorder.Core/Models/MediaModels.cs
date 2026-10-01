using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ScreenRecorder.Core.Models
{
    public class VideoFrame : IDisposable
    {
        public IntPtr SurfacePointer { get; set; }
        public byte[]? Buffer { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long TimestampMs { get; set; }
        public bool IsKeyFrame { get; set; }

        public void Dispose()
        {
            Buffer = null;
        }
    }

    public class AudioSample : IDisposable
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public int Length { get; set; }
        public long TimestampMs { get; set; }
        public bool IsLoopback { get; set; } // true = system loopback, false = mic

        public void Dispose()
        {
            Data = Array.Empty<byte>();
        }
    }

    public class LockFreeQueue<T> where T : class
    {
        private readonly ConcurrentQueue<T> _queue = new();
        private readonly int _maxCapacity;

        public LockFreeQueue(int maxCapacity = 300)
        {
            _maxCapacity = maxCapacity;
        }

        public bool Enqueue(T item)
        {
            if (_queue.Count >= _maxCapacity)
            {
                // Drop oldest if full to prevent latency buildup (Drop-oldest policy)
                _queue.TryDequeue(out _);
            }
            _queue.Enqueue(item);
            return true;
        }

        public bool TryDequeue(out T? item)
        {
            return _queue.TryDequeue(out item);
        }

        public int Count => _queue.Count;

        public void Clear()
        {
            while (_queue.TryDequeue(out var item))
            {
                if (item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
