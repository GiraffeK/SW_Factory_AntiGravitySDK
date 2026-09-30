using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ScreenRecorderLib.Models;

namespace ScreenRecorderLib.Buffers
{
    public interface IVideoCapturer : IDisposable
    {
        event EventHandler<MediaSample>? FrameCaptured;
        bool StartCapture(nint targetWindowOrDisplayHandle, RecorderConfig config);
        void StopCapture();
        void NotifyDpiChanged(int newWidth, int newHeight);
        void NotifyWindowMinimized(bool isMinimized);
    }

    public interface IAudioCapturer : IDisposable
    {
        event EventHandler<MediaSample>? AudioSampleCaptured;
        bool StartCapture(RecorderConfig config);
        void StopCapture();
    }

    public interface IEncoderPipeline : IDisposable
    {
        ChannelWriter<MediaSample> VideoInputWriter { get; }
        ChannelWriter<MediaSample> AudioInputWriter { get; }
        Task StartAsync(CancellationToken cancellationToken);
        void Stop();
    }

    public interface IMuxer : IDisposable
    {
        void Initialize(string filePath, ContainerFormat format);
        void WriteSample(MediaSample sample);
        void FinalizeFile();
    }
}
