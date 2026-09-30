namespace ScreenRecorderApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.GroupBox grpRegion;
        private System.Windows.Forms.Label lblRegionInfo;
        private System.Windows.Forms.Button btnSelectRegion;
        private System.Windows.Forms.Button btnFullScreen;

        private System.Windows.Forms.GroupBox grpSettings;
        private System.Windows.Forms.Label lblFps;
        private System.Windows.Forms.ComboBox cboFps;
        private System.Windows.Forms.Label lblEncoder;
        private System.Windows.Forms.ComboBox cboEncoder;
        private System.Windows.Forms.Label lblCodec;
        private System.Windows.Forms.ComboBox cboCodec;
        private System.Windows.Forms.Label lblContainer;
        private System.Windows.Forms.ComboBox cboContainer;

        private System.Windows.Forms.GroupBox grpAudio;
        private System.Windows.Forms.CheckBox chkAudioLoopback;
        private System.Windows.Forms.CheckBox chkAudioMic;

        private System.Windows.Forms.GroupBox grpControl;
        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Label lblTimer;
        private System.Windows.Forms.Label lblStats;
        private System.Windows.Forms.Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.grpRegion = new System.Windows.Forms.GroupBox();
            this.lblRegionInfo = new System.Windows.Forms.Label();
            this.btnSelectRegion = new System.Windows.Forms.Button();
            this.btnFullScreen = new System.Windows.Forms.Button();

            this.grpSettings = new System.Windows.Forms.GroupBox();
            this.lblFps = new System.Windows.Forms.Label();
            this.cboFps = new System.Windows.Forms.ComboBox();
            this.lblEncoder = new System.Windows.Forms.Label();
            this.cboEncoder = new System.Windows.Forms.ComboBox();
            this.lblCodec = new System.Windows.Forms.Label();
            this.cboCodec = new System.Windows.Forms.ComboBox();
            this.lblContainer = new System.Windows.Forms.Label();
            this.cboContainer = new System.Windows.Forms.ComboBox();

            this.grpAudio = new System.Windows.Forms.GroupBox();
            this.chkAudioLoopback = new System.Windows.Forms.CheckBox();
            this.chkAudioMic = new System.Windows.Forms.CheckBox();

            this.grpControl = new System.Windows.Forms.GroupBox();
            this.btnRecord = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.lblTimer = new System.Windows.Forms.Label();
            this.lblStats = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();

            this.SuspendLayout();

            // grpRegion
            this.grpRegion.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.grpRegion.Location = new System.Drawing.Point(20, 15);
            this.grpRegion.Size = new System.Drawing.Size(680, 80);
            this.grpRegion.Text = "📐 錄影範圍 (Region Selection)";

            this.lblRegionInfo.AutoSize = true;
            this.lblRegionInfo.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblRegionInfo.Location = new System.Drawing.Point(20, 32);
            this.lblRegionInfo.Text = "當前錄製範圍: 1920 × 1080 (全螢幕)";

            this.btnSelectRegion.BackColor = System.Drawing.Color.FromArgb(45, 95, 165);
            this.btnSelectRegion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectRegion.ForeColor = System.Drawing.Color.White;
            this.btnSelectRegion.Location = new System.Drawing.Point(400, 25);
            this.btnSelectRegion.Size = new System.Drawing.Size(160, 35);
            this.btnSelectRegion.Text = "🎯 滑鼠框選區域";
            this.btnSelectRegion.UseVisualStyleBackColor = false;
            this.btnSelectRegion.Click += new System.EventHandler(this.btnSelectRegion_Click);

            this.btnFullScreen.BackColor = System.Drawing.Color.FromArgb(50, 50, 58);
            this.btnFullScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFullScreen.ForeColor = System.Drawing.Color.White;
            this.btnFullScreen.Location = new System.Drawing.Point(570, 25);
            this.btnFullScreen.Size = new System.Drawing.Size(95, 35);
            this.btnFullScreen.Text = "全螢幕";
            this.btnFullScreen.UseVisualStyleBackColor = false;
            this.btnFullScreen.Click += new System.EventHandler(this.btnFullScreen_Click);

            this.grpRegion.Controls.Add(this.lblRegionInfo);
            this.grpRegion.Controls.Add(this.btnSelectRegion);
            this.grpRegion.Controls.Add(this.btnFullScreen);

            // grpSettings
            this.grpSettings.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.grpSettings.Location = new System.Drawing.Point(20, 105);
            this.grpSettings.Size = new System.Drawing.Size(680, 110);
            this.grpSettings.Text = "⚙️ 硬體編碼與影音管線設定";

            this.lblFps.Location = new System.Drawing.Point(20, 30);
            this.lblFps.Size = new System.Drawing.Size(80, 23);
            this.lblFps.Text = "幀率 (FPS):";
            this.lblFps.ForeColor = System.Drawing.Color.WhiteSmoke;

            this.cboFps.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFps.Items.AddRange(new object[] { "60 FPS (流暢推薦)", "120 FPS (極速電競)", "30 FPS (節省空間)" });
            this.cboFps.Location = new System.Drawing.Point(100, 27);
            this.cboFps.Size = new System.Drawing.Size(200, 27);

            this.lblEncoder.Location = new System.Drawing.Point(340, 30);
            this.lblEncoder.Size = new System.Drawing.Size(100, 23);
            this.lblEncoder.Text = "硬體加速引擎:";
            this.lblEncoder.ForeColor = System.Drawing.Color.WhiteSmoke;

            this.cboEncoder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEncoder.Items.AddRange(new object[] { "GPU 硬體加速 (NVENC / QSV / AMF)", "CPU 軟體編碼 (相容性模式)" });
            this.cboEncoder.Location = new System.Drawing.Point(450, 27);
            this.cboEncoder.Size = new System.Drawing.Size(215, 27);

            this.lblCodec.Location = new System.Drawing.Point(20, 70);
            this.lblCodec.Size = new System.Drawing.Size(80, 23);
            this.lblCodec.Text = "編碼格式:";
            this.lblCodec.ForeColor = System.Drawing.Color.WhiteSmoke;

            this.cboCodec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCodec.Items.AddRange(new object[] { "H.264 / AVC (最通用)", "H.265 / HEVC (高效壓縮)", "AV1 (次世代低頻寬)" });
            this.cboCodec.Location = new System.Drawing.Point(100, 67);
            this.cboCodec.Size = new System.Drawing.Size(200, 27);

            this.lblContainer.Location = new System.Drawing.Point(340, 70);
            this.lblContainer.Size = new System.Drawing.Size(100, 23);
            this.lblContainer.Text = "封裝容器:";
            this.lblContainer.ForeColor = System.Drawing.Color.WhiteSmoke;

            this.cboContainer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboContainer.Items.AddRange(new object[] { "MKV (抗崩潰 / 推薦)", "MP4 (常規格式)" });
            this.cboContainer.Location = new System.Drawing.Point(450, 67);
            this.cboContainer.Size = new System.Drawing.Size(215, 27);

            this.grpSettings.Controls.Add(this.lblFps);
            this.grpSettings.Controls.Add(this.cboFps);
            this.grpSettings.Controls.Add(this.lblEncoder);
            this.grpSettings.Controls.Add(this.cboEncoder);
            this.grpSettings.Controls.Add(this.lblCodec);
            this.grpSettings.Controls.Add(this.cboCodec);
            this.grpSettings.Controls.Add(this.lblContainer);
            this.grpSettings.Controls.Add(this.cboContainer);

            // grpAudio
            this.grpAudio.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.grpAudio.Location = new System.Drawing.Point(20, 225);
            this.grpAudio.Size = new System.Drawing.Size(680, 65);
            this.grpAudio.Text = "🎙️ 雙軌音訊採集 (WASAPI PTS 嚴格時間戳同步)";

            this.chkAudioLoopback.AutoSize = true;
            this.chkAudioLoopback.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.chkAudioLoopback.Location = new System.Drawing.Point(25, 28);
            this.chkAudioLoopback.Text = "捕捉系統內部音訊 (WASAPI Loopback)";

            this.chkAudioMic.AutoSize = true;
            this.chkAudioMic.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.chkAudioMic.Location = new System.Drawing.Point(350, 28);
            this.chkAudioMic.Text = "捕捉麥克風聲音 (Microphone Input)";

            this.grpAudio.Controls.Add(this.chkAudioLoopback);
            this.grpAudio.Controls.Add(this.chkAudioMic);

            // grpControl
            this.grpControl.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.grpControl.Location = new System.Drawing.Point(20, 300);
            this.grpControl.Size = new System.Drawing.Size(680, 235);
            this.grpControl.Text = "🎛️ 錄製控制與即時指標 (Zero-Blocking Lock-Free Buffer)";

            this.btnRecord.BackColor = System.Drawing.Color.FromArgb(200, 35, 51);
            this.btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecord.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnRecord.ForeColor = System.Drawing.Color.White;
            this.btnRecord.Location = new System.Drawing.Point(25, 30);
            this.btnRecord.Size = new System.Drawing.Size(180, 50);
            this.btnRecord.Text = "⏺️ 開始錄製";
            this.btnRecord.UseVisualStyleBackColor = false;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);

            this.btnStop.BackColor = System.Drawing.Color.FromArgb(60, 60, 68);
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Location = new System.Drawing.Point(220, 30);
            this.btnStop.Size = new System.Drawing.Size(180, 50);
            this.btnStop.Text = "⏹️ 停止錄製";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);

            this.btnOpenFolder.BackColor = System.Drawing.Color.FromArgb(40, 110, 80);
            this.btnOpenFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenFolder.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnOpenFolder.ForeColor = System.Drawing.Color.White;
            this.btnOpenFolder.Location = new System.Drawing.Point(420, 30);
            this.btnOpenFolder.Size = new System.Drawing.Size(150, 50);
            this.btnOpenFolder.Text = "📂 開啟影片資料夾";
            this.btnOpenFolder.UseVisualStyleBackColor = false;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);

            this.lblTimer.AutoSize = true;
            this.lblTimer.Font = new System.Drawing.Font("Consolas", 18F, System.Drawing.FontStyle.Bold);
            this.lblTimer.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.lblTimer.Location = new System.Drawing.Point(25, 100);
            this.lblTimer.Text = "錄製時間: 00:00:00.00";

            this.lblStats.AutoSize = true;
            this.lblStats.Font = new System.Drawing.Font("Consolas", 10.5F);
            this.lblStats.ForeColor = System.Drawing.Color.LightGray;
            this.lblStats.Location = new System.Drawing.Point(25, 145);
            this.lblStats.Text = "視訊:     0 幀 (0.0 FPS)  |  音訊:     0 包 (PTS 同步中)";

            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblStatus.Location = new System.Drawing.Point(25, 185);
            this.lblStatus.Text = "💡 就緒: 準備錄影中 (請選擇全螢幕或點選「滑鼠框選區域」)";

            this.grpControl.Controls.Add(this.btnRecord);
            this.grpControl.Controls.Add(this.btnStop);
            this.grpControl.Controls.Add(this.btnOpenFolder);
            this.grpControl.Controls.Add(this.lblTimer);
            this.grpControl.Controls.Add(this.lblStats);
            this.grpControl.Controls.Add(this.lblStatus);

            // Form1
            this.Controls.Add(this.grpRegion);
            this.Controls.Add(this.grpSettings);
            this.Controls.Add(this.grpAudio);
            this.Controls.Add(this.grpControl);
            this.Name = "Form1";
            this.ResumeLayout(false);
        }

        #endregion
    }
}
