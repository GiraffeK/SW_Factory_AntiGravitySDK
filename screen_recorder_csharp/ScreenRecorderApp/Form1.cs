using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScreenRecorderLib.Audio;
using ScreenRecorderLib.Capture;
using ScreenRecorderLib.Models;
using ScreenRecorderLib.Pipeline;

namespace ScreenRecorderApp
{
    public partial class Form1 : Form
    {
        private RecorderPipeline? _pipeline;
        private VideoCapturer? _videoCapturer;
        private AudioCapturer? _audioCapturer;
        private CancellationTokenSource? _cts;
        private Stopwatch _recordStopwatch = new();
        private System.Windows.Forms.Timer _uiTimer = new();

        private Rectangle _selectedRegion = Rectangle.Empty;
        private bool _isRecording = false;
        private long _videoFramesCount = 0;
        private long _audioPacketsCount = 0;
        private string _currentOutputFile = "";

        public Form1()
        {
            InitializeComponent();
            SetupCustomStyles();
            InitializeDefaults();
        }

        private void SetupCustomStyles()
        {
            Text = "🎥 高效能低延遲螢幕錄影系統 (C# WinForms / .NET 10)";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(720, 560);
            BackColor = Color.FromArgb(24, 24, 28);
            ForeColor = Color.White;

            _uiTimer.Interval = 100;
            _uiTimer.Tick += UiTimer_Tick;
        }

        private void InitializeDefaults()
        {
            // 預設為全螢幕
            var screen = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            _selectedRegion = screen;
            UpdateRegionLabel();

            cboFps.SelectedIndex = 0; // 60 FPS
            cboEncoder.SelectedIndex = 0; // GPU NVENC / QSV / AMF
            cboCodec.SelectedIndex = 0; // H.264
            cboContainer.SelectedIndex = 0; // MKV (防損毀)

            chkAudioLoopback.Checked = true;
            chkAudioMic.Checked = true;

            btnStop.Enabled = false;
            btnOpenFolder.Enabled = false;
        }

        private void UpdateRegionLabel()
        {
            lblRegionInfo.Text = $"當前錄製範圍: {_selectedRegion.Width} × {_selectedRegion.Height} (X: {_selectedRegion.X}, Y: {_selectedRegion.Y})";
        }

        private void btnSelectRegion_Click(object sender, EventArgs e)
        {
            using var overlay = new RegionSelectOverlay();
            overlay.ShowDialog(this);
            if (overlay.SelectionConfirmed && !overlay.SelectedRegion.IsEmpty)
            {
                _selectedRegion = overlay.SelectedRegion;
                UpdateRegionLabel();
            }
        }

        private void btnFullScreen_Click(object sender, EventArgs e)
        {
            var screen = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            _selectedRegion = screen;
            UpdateRegionLabel();
        }

        private async void btnRecord_Click(object sender, EventArgs e)
        {
            if (_isRecording) return;

            string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "ScreenRecordings");
            Directory.CreateDirectory(outputDir);

            string ext = cboContainer.SelectedItem?.ToString()?.Contains("MKV") == true ? ".mkv" : ".mp4";
            _currentOutputFile = Path.Combine(outputDir, $"Record_{DateTime.Now:yyyyMMdd_HHmmss}{ext}");

            int fps = cboFps.SelectedItem?.ToString()?.Contains("120") == true ? 120 : (cboFps.SelectedItem?.ToString()?.Contains("30") == true ? 30 : 60);

            var config = new RecorderConfig
            {
                Width = _selectedRegion.Width,
                Height = _selectedRegion.Height,
                Fps = fps,
                VideoCodec = cboCodec.SelectedItem?.ToString()?.Contains("HEVC") == true ? VideoCodecType.HEVC :
                             (cboCodec.SelectedItem?.ToString()?.Contains("AV1") == true ? VideoCodecType.AV1 : VideoCodecType.H264),
                Container = ext == ".mkv" ? ContainerFormat.Mvk : ContainerFormat.Mp4,
                OutputFilePath = _currentOutputFile,
                EnableSystemAudioLoopback = chkAudioLoopback.Checked,
                EnableMicrophone = chkAudioMic.Checked
            };

            _pipeline = new RecorderPipeline(config);
            _pipeline.Start();

            _videoCapturer = new VideoCapturer();
            _audioCapturer = new AudioCapturer();
            _cts = new CancellationTokenSource();
            _videoFramesCount = 0;
            _audioPacketsCount = 0;

            _videoCapturer.FrameCaptured += (s, sample) =>
            {
                if (_pipeline.VideoWriter.TryWrite(sample))
                {
                    Interlocked.Increment(ref _videoFramesCount);
                }
            };

            _audioCapturer.AudioSampleCaptured += (s, sample) =>
            {
                if (_pipeline.AudioWriter.TryWrite(sample))
                {
                    Interlocked.Increment(ref _audioPacketsCount);
                }
            };

            _videoCapturer.StartCapture(IntPtr.Zero, config);
            _audioCapturer.StartCapture(config);

            // 啟動零拷貝採樣模擬
            StartSimulationWorker(config, _cts.Token);

            _recordStopwatch.Restart();
            _uiTimer.Start();

            _isRecording = true;
            btnRecord.Enabled = false;
            btnStop.Enabled = true;
            btnSelectRegion.Enabled = false;
            btnFullScreen.Enabled = false;
            btnOpenFolder.Enabled = false;
            lblStatus.Text = "⏺️ 狀態: 錄製中 (Recording)...";
            lblStatus.ForeColor = Color.Crimson;
        }

        private void StartSimulationWorker(RecorderConfig config, CancellationToken token)
        {
            Task.Run(async () =>
            {
                var frameInterval = TimeSpan.FromMilliseconds(1000.0 / config.Fps);
                while (!token.IsCancellationRequested)
                {
                    _videoCapturer?.SimulateFrameCapture(new byte[1024 * 32], _videoFramesCount % config.Fps == 0);
                    try { await Task.Delay(frameInterval, token); } catch { break; }
                }
            }, token);

            if (config.EnableSystemAudioLoopback || config.EnableMicrophone)
            {
                Task.Run(async () =>
                {
                    var audioInterval = TimeSpan.FromMilliseconds(20);
                    while (!token.IsCancellationRequested)
                    {
                        if (config.EnableSystemAudioLoopback)
                            _audioCapturer?.SimulateAudioPacket(new byte[1920], false);
                        if (config.EnableMicrophone)
                            _audioCapturer?.SimulateAudioPacket(new byte[1920], true);
                        try { await Task.Delay(audioInterval, token); } catch { break; }
                    }
                }, token);
            }
        }

        private async void btnStop_Click(object sender, EventArgs e)
        {
            if (!_isRecording) return;

            btnStop.Enabled = false;
            lblStatus.Text = "⏳ 正在 Flush 影音無鎖隊列並封裝寫入磁碟...";
            lblStatus.ForeColor = Color.Orange;

            _cts?.Cancel();
            _videoCapturer?.StopCapture();
            _audioCapturer?.StopCapture();

            await Task.Delay(200);

            _pipeline?.Stop();
            _recordStopwatch.Stop();
            _uiTimer.Stop();

            _videoCapturer?.Dispose();
            _audioCapturer?.Dispose();
            _pipeline?.Dispose();

            _isRecording = false;
            btnRecord.Enabled = true;
            btnSelectRegion.Enabled = true;
            btnFullScreen.Enabled = true;
            btnOpenFolder.Enabled = true;

            lblStatus.Text = $"✅ 錄影完成！檔案已保存 ({new FileInfo(_currentOutputFile).Length / 1024.0:F1} KB)";
            lblStatus.ForeColor = Color.LimeGreen;
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentOutputFile) && File.Exists(_currentOutputFile))
            {
                Process.Start("explorer.exe", $"/select,\"{_currentOutputFile}\"");
            }
            else
            {
                string dir = Path.GetDirectoryName(_currentOutputFile) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                Process.Start("explorer.exe", dir);
            }
        }

        private void UiTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isRecording) return;

            var elapsed = _recordStopwatch.Elapsed;
            lblTimer.Text = $"錄製時間: {elapsed:hh\\:mm\\:ss\\.ff}";
            double fps = _recordStopwatch.Elapsed.TotalSeconds > 0 ? _videoFramesCount / _recordStopwatch.Elapsed.TotalSeconds : 0;
            lblStats.Text = $"視訊: {_videoFramesCount,5} 幀 ({fps:F1} FPS)  |  音訊: {_audioPacketsCount,5} 包 (PTS 同步中)";
        }
    }
}
