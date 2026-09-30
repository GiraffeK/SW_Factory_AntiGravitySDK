using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScreenRecorderApp
{
    /// <summary>
    /// 半透明全螢幕覆蓋層，讓使用者使用滑鼠拖曳框選錄影區域。
    /// 支援多螢幕 (VirtualScreen)、即時大小標籤、雙緩衝繪製與 ESC 取消。
    /// </summary>
    public class RegionSelectOverlay : Form
    {
        private Point _startPoint;
        private Rectangle _selectedRect;
        private bool _isSelecting;
        private readonly Pen _borderPen = new Pen(Color.FromArgb(0, 168, 255), 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Solid };
        private readonly Brush _fillBrush = new SolidBrush(Color.FromArgb(40, 0, 168, 255));
        private readonly Brush _overlayBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
        private readonly Font _font = new Font("Segoe UI", 11, FontStyle.Bold);
        private readonly Brush _textBrush = Brushes.White;
        private readonly Brush _textBgBrush = new SolidBrush(Color.FromArgb(200, 20, 20, 20));

        public Rectangle SelectedRegion => _selectedRect;
        public bool SelectionConfirmed { get; private set; }

        public RegionSelectOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            DoubleBuffered = true;
            Cursor = Cursors.Cross;

            // 涵蓋所有螢幕區域
            var virtualScreen = SystemInformation.VirtualScreen;
            Bounds = virtualScreen;
            BackColor = Color.Black;
            Opacity = 0.85;

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    SelectionConfirmed = false;
                    Close();
                }
            };
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;
                _startPoint = e.Location;
                _selectedRect = new Rectangle(e.Location, Size.Empty);
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isSelecting)
            {
                int x = Math.Min(_startPoint.X, e.X);
                int y = Math.Min(_startPoint.Y, e.Y);
                int w = Math.Abs(e.X - _startPoint.X);
                int h = Math.Abs(e.Y - _startPoint.Y);

                // 偶數校正（H.264 / HEVC 硬體編碼要求長寬為偶數）
                w = (w / 2) * 2;
                h = (h / 2) * 2;

                _selectedRect = new Rectangle(x, y, w, h);
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && _isSelecting)
            {
                _isSelecting = false;
                // 確保寬高大於最小限制 (e.g. 64x64)
                if (_selectedRect.Width >= 64 && _selectedRect.Height >= 64)
                {
                    SelectionConfirmed = true;
                }
                Close();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            // 繪製暗色遮罩
            g.FillRectangle(_overlayBrush, ClientRectangle);

            if (_selectedRect.Width > 0 && _selectedRect.Height > 0)
            {
                // 清空選取區域並塗上淺色半透明
                g.FillRectangle(_fillBrush, _selectedRect);
                g.DrawRectangle(_borderPen, _selectedRect);

                // 繪製即時尺寸指示牌
                string info = $"{_selectedRect.Width} × {_selectedRect.Height} (X: {_selectedRect.X}, Y: {_selectedRect.Y})";
                var textSize = g.MeasureString(info, _font);
                int badgeX = Math.Max(10, _selectedRect.Left);
                int badgeY = _selectedRect.Top > 40 ? _selectedRect.Top - 32 : _selectedRect.Bottom + 10;
                var badgeRect = new RectangleF(badgeX, badgeY, textSize.Width + 16, textSize.Height + 8);

                g.FillRectangle(_textBgBrush, badgeRect);
                g.DrawRectangle(Pens.CornflowerBlue, Rectangle.Round(badgeRect));
                g.DrawString(info, _font, _textBrush, badgeX + 8, badgeY + 4);
            }
            else
            {
                // 提示文字
                string tip = "請使用滑鼠左鍵拖曳框選錄影區域 (按 ESC 鍵取消)";
                var tipSize = g.MeasureString(tip, _font);
                var tipRect = new RectangleF((Width - tipSize.Width) / 2 - 16, 40, tipSize.Width + 32, tipSize.Height + 16);
                g.FillRectangle(_textBgBrush, tipRect);
                g.DrawRectangle(Pens.SkyBlue, Rectangle.Round(tipRect));
                g.DrawString(tip, _font, _textBrush, (Width - tipSize.Width) / 2, 48);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _borderPen.Dispose();
                _fillBrush.Dispose();
                _overlayBrush.Dispose();
                _font.Dispose();
                _textBgBrush.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
