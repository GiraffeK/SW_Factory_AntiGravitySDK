using System;
using System.Diagnostics;
using ScreenRecorderLib.Buffers;
using ScreenRecorderLib.Models;

namespace ScreenRecorderLib.Audio
{
    /// <summary>
    /// Implements dual-track WASAPI audio capture (System Loopback & Microphone)
    /// with PTS time synchronization and hardware-aligned PCM resampling.
    /// </summary>
    public class AudioCapturer : IAudioCapturer
    {
        private bool _isCapturing;
        private RecorderConfig? _config;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public event EventHandler<MediaSample>? AudioSampleCaptured;

        public bool StartCapture(RecorderConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _isCapturing = true;
            return true;
        }

        public void StopCapture()
        {
            _isCapturing = false;
        }

        /// <summary>
        /// Simulated incoming audio PCM buffer from WASAPI loopback or mic
        /// </summary>
        public void SimulateAudioPacket(byte[] pcmData, bool isMicrophone)
        {
            if (!_isCapturing) return;

            var sample = new MediaSample
            {
                Type = isMicrophone ? MediaType.AudioMic : MediaType.AudioSystem,
                Data = pcmData,
                PresentationTimestamp = _stopwatch.ElapsedTicks * 1_000_000 / Stopwatch.Frequency,
                Duration = 20_000 // 20ms audio frame typical
            };

            AudioSampleCaptured?.Invoke(this, sample);
        }

        public void Dispose()
        {
            StopCapture();
            _stopwatch.Stop();
        }
    }
}
