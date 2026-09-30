using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ScreenRecorderCore.Models;

namespace ScreenRecorderCore.Pipeline
{
    public interface IVideoCapturer
    {
        event Action<IntPtr, long, int, int>? FrameCaptured;
        Task StartCaptureAsync(CancellationToken cancellationToken);
        void StopCapture();
    }

    public interface IAudioCapturer
    {
        event Action<byte[], long, MediaType>? AudioSampleCaptured;
        Task StartCaptureAsync(CancellationToken cancellationToken);
        void StopCapture();
    }

    public interface IEncoder : IDisposable
    {
        Task EncodeVideoAsync(IntPtr surfacePtr, long ptsMs, ChannelWriter<MediaPacket> outputWriter, CancellationToken cancellationToken);
        Task EncodeAudioAsync(byte[] pcmData, long ptsMs, ChannelWriter<MediaPacket> outputWriter, CancellationToken cancellationToken);
    }

    public interface IMuxer : IDisposable
    {
        void Initialize(string outputPath, ContainerFormat format);
        Task WritePacketAsync(MediaPacket packet, CancellationToken cancellationToken);
        Task FinalizeAsync();
    }
}
