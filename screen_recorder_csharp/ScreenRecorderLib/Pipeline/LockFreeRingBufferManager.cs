using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ScreenRecorderLib.Buffers;
using ScreenRecorderLib.Models;

namespace ScreenRecorderLib.Pipeline
{
    public class LockFreeRingBufferManager : IDisposable
    {
        private readonly Channel<MediaSample> _videoChannel;
        private readonly Channel<MediaSample> _audioChannel;

        public LockFreeRingBufferManager(int videoCapacity = 120, int audioCapacity = 500)
        {
            // Configure with DropOldest behavior to prevent capture blocking (Zero-blocking backpressure)
            var videoOptions = new BoundedChannelOptions(videoCapacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = false,
                SingleReader = true
            };

            var audioOptions = new BoundedChannelOptions(audioCapacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = false,
                SingleReader = true
            };

            _videoChannel = Channel.CreateBounded<MediaSample>(videoOptions);
            _audioChannel = Channel.CreateBounded<MediaSample>(audioOptions);
        }

        public ChannelWriter<MediaSample> VideoWriter => _videoChannel.Writer;
        public ChannelReader<MediaSample> VideoReader => _videoChannel.Reader;

        public ChannelWriter<MediaSample> AudioWriter => _audioChannel.Writer;
        public ChannelReader<MediaSample> AudioReader => _audioChannel.Reader;

        public bool TryWriteVideo(MediaSample sample)
        {
            return _videoChannel.Writer.TryWrite(sample);
        }

        public bool TryWriteAudio(MediaSample sample)
        {
            return _audioChannel.Writer.TryWrite(sample);
        }

        public void CompleteWriters()
        {
            _videoChannel.Writer.Complete();
            _audioChannel.Writer.Complete();
        }

        public void Dispose()
        {
            CompleteWriters();
        }
    }
}
