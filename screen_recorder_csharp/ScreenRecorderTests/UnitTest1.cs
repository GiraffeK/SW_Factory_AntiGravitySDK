using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using ScreenRecorderLib.Audio;
using ScreenRecorderLib.Capture;
using ScreenRecorderLib.Models;
using ScreenRecorderLib.Pipeline;
using ScreenRecorderLib.Muxer;

namespace ScreenRecorderTests
{
    public class ScreenRecorderPipelineTests
    {
        [Fact]
        public void TestRecorderConfigDefaults()
        {
            var config = new RecorderConfig();
            Assert.Equal(1920, config.Width);
            Assert.Equal(1080, config.Height);
            Assert.Equal(60, config.Fps);
            Assert.Equal(VideoCodecType.H264, config.VideoCodec);
            Assert.Equal(ContainerFormat.Mvk, config.Container);
        }

        [Fact]
        public void TestLockFreeRingBufferManager()
        {
            using var bufferManager = new LockFreeRingBufferManager(10, 10);
            
            var sample = new MediaSample
            {
                Type = MediaType.Video,
                Data = new byte[] { 1, 2, 3, 4 },
                PresentationTimestamp = 1000,
                Duration = 16666,
                IsKeyFrame = true
            };

            bool written = bufferManager.TryWriteVideo(sample);
            Assert.True(written);

            bool readSuccess = bufferManager.VideoReader.TryRead(out var readSample);
            Assert.True(readSuccess);
            Assert.NotNull(readSample);
            Assert.Equal(1000, readSample.PresentationTimestamp);
            Assert.True(readSample.IsKeyFrame);
        }

        [Fact]
        public async Task TestEndToEndRecorderPipeline()
        {
            string testOutputFile = Path.Combine(Path.GetTempPath(), $"test_recording_{Guid.NewGuid()}.mvk");

            try
            {
                var config = new RecorderConfig
                {
                    Width = 1280,
                    Height = 720,
                    Fps = 30,
                    Container = ContainerFormat.Mvk,
                    OutputFilePath = testOutputFile
                };

                using var pipeline = new RecorderPipeline(config);
                pipeline.Start();

                using var videoCapturer = new VideoCapturer();
                using var audioCapturer = new AudioCapturer();

                videoCapturer.FrameCaptured += (sender, sample) =>
                {
                    pipeline.VideoWriter.TryWrite(sample);
                };

                audioCapturer.AudioSampleCaptured += (sender, sample) =>
                {
                    pipeline.AudioWriter.TryWrite(sample);
                };

                videoCapturer.StartCapture(IntPtr.Zero, config);
                audioCapturer.StartCapture(config);

                // Simulate frames and audio
                for (int i = 0; i < 15; i++)
                {
                    videoCapturer.SimulateFrameCapture(new byte[100], i == 0);
                    audioCapturer.SimulateAudioPacket(new byte[200], false);
                    await Task.Delay(10);
                }

                videoCapturer.StopCapture();
                audioCapturer.StopCapture();

                await Task.Delay(50);
                pipeline.Stop();

                // Verify output file exists and has content
                Assert.True(File.Exists(testOutputFile));
                var fileInfo = new FileInfo(testOutputFile);
                Assert.True(fileInfo.Length > 0);
            }
            finally
            {
                if (File.Exists(testOutputFile))
                {
                    try { File.Delete(testOutputFile); } catch { }
                }
            }
        }
    }
}
