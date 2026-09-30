using System;
using System.Drawing;
using System.Threading.Tasks;
using Xunit;
using ScreenRecorderApp.Core;
using ScreenRecorderLib.Buffers;
using ScreenRecorderLib.Models;
using ScreenRecorderLib.Pipeline;

namespace ScreenRecorderApp.Tests
{
    public class ScreenRecorderAppTests
    {
        [Fact]
        public void TestLockFreeMediaQueueDropOldestBehavior()
        {
            var queue = new LockFreeMediaQueue(maxCapacity: 3);

            // 放入 4 個 Frame，預期最舊的被丟棄
            for (int i = 1; i <= 4; i++)
            {
                var frame = new MediaFrame
                {
                    Type = Core.MediaType.Video,
                    PresentationTimestamp = i * 1000,
                    Data = new byte[] { (byte)i }
                };
                queue.Enqueue(frame);
            }

            Assert.Equal(3, queue.Count);
            Assert.Equal(1, queue.DroppedFrames);

            // 最舊的 1 應該已經被 Dequeue 丟棄，第一個取出的應該是 2
            bool success = queue.TryDequeue(out var firstOut);
            Assert.True(success);
            Assert.NotNull(firstOut);
            Assert.Equal(2000, firstOut.PresentationTimestamp);
        }

        [Fact]
        public void TestTimestampSynchronizerMonotonic()
        {
            var sync = new TimestampSynchronizer(timeScale: 1000000); // Microseconds

            long pts1 = sync.GetCurrentPts();
            System.Threading.Thread.Sleep(5);
            long pts2 = sync.GetCurrentPts();

            Assert.True(pts1 >= 0);
            Assert.True(pts2 > pts1, $"PTS2 ({pts2}) should be greater than PTS1 ({pts1})");
        }

        [Fact]
        public void TestRecorderConfigDefaults()
        {
            var config = new RecorderConfig();
            Assert.Equal(1920, config.Width);
            Assert.Equal(1080, config.Height);
            Assert.Equal(60, config.Fps);
            Assert.Equal(VideoCodecType.H264, config.VideoCodec);
            Assert.Equal(ContainerFormat.Mvk, config.Container);
            Assert.True(config.EnableSystemAudioLoopback);
            Assert.True(config.EnableMicrophone);
        }

        [Fact]
        public void TestLockFreeRingBufferManagerFlow()
        {
            using var ringBuffer = new LockFreeRingBufferManager(videoCapacity: 10, audioCapacity: 10);

            var videoSample = new MediaSample
            {
                Type = ScreenRecorderLib.Models.MediaType.Video,
                Data = new byte[] { 10, 20, 30 },
                PresentationTimestamp = 5000,
                Duration = 16666,
                IsKeyFrame = true
            };

            bool writeResult = ringBuffer.TryWriteVideo(videoSample);
            Assert.True(writeResult);

            bool readResult = ringBuffer.VideoReader.TryRead(out var readSample);
            Assert.True(readResult);
            Assert.NotNull(readSample);
            Assert.Equal(5000, readSample.PresentationTimestamp);
            Assert.True(readSample.IsKeyFrame);
            Assert.Equal(3, readSample.Data.Length);
        }

        [Theory]
        [InlineData(1921, 1081, 1920, 1080)]
        [InlineData(1280, 720, 1280, 720)]
        [InlineData(853, 480, 852, 480)]
        public void TestHardwareEncoderEvenDimensionsAlignment(int inW, int inH, int expectedW, int expectedH)
        {
            // 驗證長寬強制偶數對齊演算法（NVENC / QSV / AMF 要求）
            int alignedW = (inW / 2) * 2;
            int alignedH = (inH / 2) * 2;

            Assert.Equal(expectedW, alignedW);
            Assert.Equal(expectedH, alignedH);
        }
    }
}
