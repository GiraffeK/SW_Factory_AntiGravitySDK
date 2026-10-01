using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ScreenRecorder.Core.Models;

namespace ScreenRecorder.Core.Audio
{
    public interface IAudioMixerEngine
    {
        event EventHandler<AudioSample>? AudioSampleCaptured;
        bool IsRecording { get; }
        void StartRecording();
        void StopRecording();
    }

    public class AudioMixerEngine : IAudioMixerEngine
    {
        public event EventHandler<AudioSample>? AudioSampleCaptured;
        public bool IsRecording { get; private set; }

        private CancellationTokenSource? _cts;
        private Task? _audioLoopTask;
        private readonly Stopwatch _stopwatch = new();

        public void StartRecording()
        {
            if (IsRecording) return;
            IsRecording = true;
            _cts = new CancellationTokenSource();
            _stopwatch.Restart();

            // WASAPI Loopback & Mic sampling simulation (10ms chunks)
            const int sampleIntervalMs = 10;
            const int bufferSize = 960; // e.g. 48kHz stereo 10ms chunk

            _audioLoopTask = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    long currentTimestamp = _stopwatch.ElapsedMilliseconds;

                    // Produce Loopback sample (System audio)
                    var loopbackSample = new AudioSample
                    {
                        Data = new byte[bufferSize],
                        Length = bufferSize,
                        TimestampMs = currentTimestamp,
                        IsLoopback = true
                    };
                    AudioSampleCaptured?.Invoke(this, loopbackSample);

                    // Produce Microphone sample
                    var micSample = new AudioSample
                    {
                        Data = new byte[bufferSize],
                        Length = bufferSize,
                        TimestampMs = currentTimestamp,
                        IsLoopback = false
                    };
                    AudioSampleCaptured?.Invoke(this, micSample);

                    try
                    {
                        await Task.Delay(sampleIntervalMs, _cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, _cts.Token);
        }

        public void StopRecording()
        {
            if (!IsRecording) return;
            IsRecording = false;
            _cts?.Cancel();
            try
            {
                _audioLoopTask?.Wait(2000);
            }
            catch { }
            _cts?.Dispose();
            _cts = null;
            _stopwatch.Stop();
        }
    }
}
