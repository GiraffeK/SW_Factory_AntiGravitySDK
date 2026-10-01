using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScreenRecorder.Core.Forms
{
    public class RegionSelectorForm : Form
    {
        public Rectangle SelectedRegion { get; private set; }
        private Point _startPoint;
        private Rectangle _currentRectangle;
        private bool _isSelecting;

        public RegionSelectorForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.BackColor = Color.Black;
            this.Opacity = 0.3;
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Cross;
            this.TopMost = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;
                _startPoint = e.Location;
                _currentRectangle = new Rectangle(e.Location, new Size(0, 0));
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isSelecting)
            {
                int x = Math.Min(e.X, _startPoint.X);
                int y = Math.Min(e.Y, _startPoint.Y);
                int width = Math.Abs(e.X - _startPoint.X);
                int height = Math.Abs(e.Y - _startPoint.Y);
                _currentRectangle = new Rectangle(x, y, width, height);
                this.Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;
                SelectedRegion = _currentRectangle;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_currentRectangle.Width > 0 && _currentRectangle.Height > 0)
            {
                using var brush = new SolidBrush(Color.FromArgb(100, Color.DodgerBlue));
                e.Graphics.FillRectangle(brush, _currentRectangle);
                using var pen = new Pen(Color.Red, 2);
                e.Graphics.DrawRectangle(pen, _currentRectangle);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                SelectedRegion = Rectangle.Empty;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            base.OnKeyDown(e);
        }
    }
}
