using System;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ScreenRecorderCore.Models;

namespace ScreenRecorderCore.Pipeline
{
    public class HardwareEncoder : IEncoder
    {
        private readonly RecorderConfiguration _config;
        private bool _disposed;

        public HardwareEncoder(RecorderConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task EncodeVideoAsync(IntPtr surfacePtr, long ptsMs, ChannelWriter<MediaPacket> outputWriter, CancellationToken cancellationToken)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HardwareEncoder));

            // Simulate GPU hardware encoding (NVENC / QSV / AMF) producing H.264/HEVC/AV1 NAL units
            byte[] encodedPayload = Encoding.UTF8.GetBytes($"ENC_VIDEO_FRAME_{ptsMs}_{_config.Encoder}");
            var packet = new MediaPacket
            {
                Type = MediaType.Video,
                Data = encodedPayload,
                TimestampMs = ptsMs,
                IsKeyFrame = (ptsMs % 1000 < 50) // Keyframe every ~1s
            };

            await outputWriter.WriteAsync(packet, cancellationToken).ConfigureAwait(false);
        }

        public async Task EncodeAudioAsync(byte[] pcmData, long ptsMs, ChannelWriter<MediaPacket> outputWriter, CancellationToken cancellationToken)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HardwareEncoder));

            // Simulate AAC / Opus audio encoding
            byte[] encodedPayload = new byte[pcmData.Length / 4]; // Compressed AAC simulation
            Array.Copy(pcmData, encodedPayload, Math.Min(pcmData.Length, encodedPayload.Length));

            var packet = new MediaPacket
            {
                Type = MediaType.Audio,
                Data = encodedPayload,
                TimestampMs = ptsMs,
                IsKeyFrame = true
            };

            await outputWriter.WriteAsync(packet, cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
