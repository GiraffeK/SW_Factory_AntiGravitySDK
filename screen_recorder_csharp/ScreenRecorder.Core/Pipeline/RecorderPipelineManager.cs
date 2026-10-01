using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ScreenRecorder.Core.Audio;
using ScreenRecorder.Core.Capture;
using ScreenRecorder.Core.Models;

namespace ScreenRecorder.Core.Pipeline
{
    public class RecorderPipelineManager
    {
        private readonly IScreenCaptureEngine _videoCapture;
        private readonly IAudioMixerEngine _audioMixer;
        private readonly LockFreeQueue<VideoFrame> _videoQueue = new(300);
        private readonly LockFreeQueue<AudioSample> _audioQueue = new(600);

        private CancellationTokenSource? _pipelineCts;
        private Task? _muxerTask;
        private string? _outputPath;

        public bool IsRecording { get; private set; }

        public RecorderPipelineManager(IScreenCaptureEngine videoCapture, IAudioMixerEngine audioMixer)
        {
            _videoCapture = videoCapture;
            _audioMixer = audioMixer;

            _videoCapture.FrameCaptured += OnFrameCaptured;
            _audioMixer.AudioSampleCaptured += OnAudioCaptured;
        }

        private void OnFrameCaptured(object? sender, VideoFrame frame)
        {
            _videoQueue.Enqueue(frame);
        }

        private void OnAudioCaptured(object? sender, AudioSample sample)
        {
            _audioQueue.Enqueue(sample);
        }

        public void Start(string outputPath, int fps, System.Drawing.Rectangle region)
        {
            if (IsRecording) return;
            IsRecording = true;
            _outputPath = outputPath;
            _pipelineCts = new CancellationTokenSource();

            _videoQueue.Clear();
            _audioQueue.Clear();

            // Start hardware capture & audio
            _videoCapture.StartCapture(fps, region);
            _audioMixer.StartRecording();

            // Start Muxer / Disk Writer loop
            _muxerTask = Task.Run(async () =>
            {
                using var fs = new FileStream(_outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, true);
                
                // Write simple header signature for MP4/MKV container simulation
                byte[] header = System.Text.Encoding.ASCII.GetBytes("ANTIGRAVITY_REC_CONTAINER_V1");
                await fs.WriteAsync(header, 0, header.Length, _pipelineCts.Token);

                while (!_pipelineCts.Token.IsCancellationRequested || _videoQueue.Count > 0 || _audioQueue.Count > 0)
                {
                    bool processedAny = false;

                    // Drain and multiplex video frames
                    while (_videoQueue.TryDequeue(out var vFrame))
                    {
                        if (vFrame != null)
                        {
                            processedAny = true;
                            // Simulate muxing packet
                            vFrame.Dispose();
                        }
                    }

                    // Drain and multiplex audio samples
                    while (_audioQueue.TryDequeue(out var aSample))
                    {
                        if (aSample != null)
                        {
                            processedAny = true;
                            // Simulate muxing packet
                            aSample.Dispose();
                        }
                    }

                    if (!processedAny)
                    {
                        try
                        {
                            await Task.Delay(5, _pipelineCts.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                }

                await fs.FlushAsync();
            }, _pipelineCts.Token);
        }

        public async Task StopAsync()
        {
            if (!IsRecording) return;
            IsRecording = false;

            _videoCapture.StopCapture();
            _audioMixer.StopRecording();

            _pipelineCts?.Cancel();
            try
            {
                if (_muxerTask != null)
                {
                    await _muxerTask;
                }
            }
            catch { }

            _pipelineCts?.Dispose();
            _pipelineCts = null;
            _videoQueue.Clear();
            _audioQueue.Clear();
        }
    }
}
