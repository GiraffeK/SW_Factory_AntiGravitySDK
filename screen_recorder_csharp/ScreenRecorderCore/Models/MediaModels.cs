using System;

namespace ScreenRecorderCore.Models
{
    public enum EncoderType
    {
        SoftwareH264,
        HardwareNVENC,
        HardwareQSV,
        HardwareAMF,
        HardwareAV1
    }

    public enum ContainerFormat
    {
        Mp4,
        Mkv
    }

    public class RecorderConfiguration
    {
        public int TargetFps { get; set; } = 60;
        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;
        public int BitrateKbps { get; set; } = 8000;
        public EncoderType Encoder { get; set; } = EncoderType.HardwareNVENC;
        public ContainerFormat Container { get; set; } = ContainerFormat.Mkv; // MKV防損壞
        public bool EnableAudioLoopback { get; set; } = true;
        public bool EnableMicrophone { get; set; } = true;
        public int AudioSampleRate { get; set; } = 48000;
        public int AudioChannels { get; set; } = 2;
    }

    public enum MediaType
    {
        Video,
        Audio
    }

    public class MediaPacket : IDisposable
    {
        public MediaType Type { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public long TimestampMs { get; set; }
        public bool IsKeyFrame { get; set; }

        public void Dispose()
        {
            Data = Array.Empty<byte>();
        }
    }
}
