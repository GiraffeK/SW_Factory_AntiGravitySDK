using System;
using System.Drawing;
using System.Windows.Forms;
using ScreenRecorder.Core.Audio;
using ScreenRecorder.Core.Capture;
using ScreenRecorder.Core.Forms;
using ScreenRecorder.Core.Pipeline;

namespace ScreenRecorder.Core
{
    public partial class MainForm : Form
    {
        private Button _btnSelectRegion;
        private Button _btnStartStop;
        private Label _lblStatus;
        private Rectangle _selectedRegion = Rectangle.Empty;

        private readonly IScreenCaptureEngine _videoCapture = new ScreenCaptureEngine();
        private readonly IAudioMixerEngine _audioMixer = new AudioMixerEngine();
        private RecorderPipelineManager? _pipelineManager;

        public MainForm()
        {
            InitializeComponent();
            _pipelineManager = new RecorderPipelineManager(_videoCapture, _audioMixer);
        }

        private void InitializeComponent()
        {
            this.Text = "AntiGravity High-Performance Screen Recorder (.NET 10)";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            _btnSelectRegion = new Button
            {
                Text = "框定錄影區域 (Select Region)",
                Location = new Point(30, 30),
                Size = new Size(200, 40)
            };
            _btnSelectRegion.Click += BtnSelectRegion_Click;

            _btnStartStop = new Button
            {
                Text = "開始錄影 (Start Recording)",
                Location = new Point(250, 30),
                Size = new Size(200, 40),
                BackColor = Color.LightGreen
            };
            _btnStartStop.Click += BtnStartStop_Click;

            _lblStatus = new Label
            {
                Text = "狀態: 就緒 (Ready) | 區域: 全螢幕 (Full Screen)",
                Location = new Point(30, 100),
                Size = new Size(420, 30),
                Font = new Font("Microsoft JhengHei", 10F, FontStyle.Regular)
            };

            this.Controls.Add(_btnSelectRegion);
            this.Controls.Add(_btnStartStop);
            this.Controls.Add(_lblStatus);
        }

        private void BtnSelectRegion_Click(object? sender, EventArgs e)
        {
            using var selector = new RegionSelectorForm();
            if (selector.ShowDialog() == DialogResult.OK && selector.SelectedRegion.Width > 0)
            {
                _selectedRegion = selector.SelectedRegion;
                _lblStatus.Text = $"狀態: 已選取區域 X={_selectedRegion.X}, Y={_selectedRegion.Y}, W={_selectedRegion.Width}, H={_selectedRegion.Height}";
            }
        }

        private async void BtnStartStop_Click(object? sender, EventArgs e)
        {
            if (_pipelineManager == null) return;

            if (!_pipelineManager.IsRecording)
            {
                string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.mkv");
                int fps = 60;
                var region = _selectedRegion == Rectangle.Empty ? Screen.PrimaryScreen.Bounds : _selectedRegion;

                _pipelineManager.Start(outputPath, fps, region);
                _btnStartStop.Text = "停止錄影 (Stop Recording)";
                _btnStartStop.BackColor = Color.LightCoral;
                _btnSelectRegion.Enabled = false;
                _lblStatus.Text = $"正在錄影中... 輸出檔: {Path.GetFileName(outputPath)}";
            }
            else
            {
                _btnStartStop.Enabled = false;
                _lblStatus.Text = "正在停止並封裝影音...";
                await _pipelineManager.StopAsync();
                _btnStartStop.Text = "開始錄影 (Start Recording)";
                _btnStartStop.BackColor = Color.LightGreen;
                _btnSelectRegion.Enabled = true;
                _lblStatus.Text = "狀態: 錄影完成並儲存成功！";
            }
        }
    }
}
