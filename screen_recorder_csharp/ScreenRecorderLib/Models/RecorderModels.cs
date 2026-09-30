using System;

namespace ScreenRecorderLib.Models
{
    public enum VideoCodecType
    {
        H264,
        HEVC,
        AV1
    }

    public enum AudioCodecType
    {
        AAC,
        Opus
    }

    public enum ContainerFormat
    {
        Mp4,
        Mvk,
        FragmentedMp4
    }

    public class RecorderConfig
    {
        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;
        public int Fps { get; set; } = 60;
        public int BitrateKbps { get; set; } = 8000;
        public VideoCodecType VideoCodec { get; set; } = VideoCodecType.H264;
        public AudioCodecType AudioCodec { get; set; } = AudioCodecType.AAC;
        public ContainerFormat Container { get; set; } = ContainerFormat.Mvk;
        public bool EnableSystemAudioLoopback { get; set; } = true;
        public bool EnableMicrophone { get; set; } = true;
        public string OutputFilePath { get; set; } = "output.mkv";
    }

    public class MediaSample
    {
        public MediaType Type { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public long PresentationTimestamp { get; set; } // Microseconds or ticks
        public long Duration { get; set; }
        public bool IsKeyFrame { get; set; }
    }

    public enum MediaType
    {
        Video,
        AudioSystem,
        AudioMic,
        AudioMixed
    }

    public enum RecorderState
    {
        Idle,
        Preparing,
        Recording,
        Paused,
        Stopping,
        Stopped,
        Error
    }
}
