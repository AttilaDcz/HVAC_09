using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Structural
{
    public class HVACScrollableContainer : Panel, IThemeable
    {
        private readonly DoubleBufferedPanel _innerContentPanel;
        private readonly System.Windows.Forms.Timer _smoothScrollTimer;
        private int _scrollY;
        private int _targetScrollY;
        private bool _isThumbHovered;
        private bool _isThumbPressed;
        private int _mouseClickOffsetY;
        private Rectangle _trackRect;
        private Rectangle _thumbRect;
        private Rectangle _thumbHitRect;

        private const int ScrollBarWidth = 6;
        private const int ScrollHitWidth = 12;
        private const int MinThumbHeight = 24;
        private const int WsExComposited = 0x02000000;

        public Control.ControlCollection ContentControls => _innerContentPanel.Controls;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SmoothScrollEnabled { get; set; } = true;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CaptureChildMouseWheel { get; set; } = true;

        public HVACScrollableContainer()
        {
            AutoScroll = false;
            HorizontalScroll.Enabled = false;
            HorizontalScroll.Visible = false;
            VerticalScroll.Enabled = false;
            VerticalScroll.Visible = false;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            _innerContentPanel = new DoubleBufferedPanel
            {
                Location = Point.Empty,
                Width = 240,
                Height = 0,
                BackColor = ThemeManager.CurrentPalette.Window
            };

            Controls.Add(_innerContentPanel);

            _innerContentPanel.ControlAdded += InnerContentPanel_ControlAdded;
            _innerContentPanel.ControlRemoved += InnerContentPanel_ControlRemoved;
            Resize += (s, e) => RecalculateContentLayout(preserveScrollRatio: true);

            _smoothScrollTimer = new System.Windows.Forms.Timer { Interval = 15 };
            _smoothScrollTimer.Tick += SmoothScrollTimer_Tick;

            MouseWheel += HVACScrollableContainer_MouseWheel;
        }

        public void ApplyTheme(ThemePalette palette)
        {
            BackColor = palette.Window;
            _innerContentPanel.BackColor = palette.Window;
            Invalidate(true);
        }

        public void RecalculateGeometry()
        {
            RecalculateContentLayout();
        }

        public void RecalculateContentLayout(bool preserveScrollRatio = false)
        {
            double scrollRatio = GetScrollRatio();

            int contentWidth = Math.Max(0, Width - ScrollBarWidth);
            _innerContentPanel.Width = contentWidth;

            int totalHeight = 0;
            foreach (Control ctrl in _innerContentPanel.Controls)
            {
                if (!ctrl.Visible)
                {
                    continue;
                }

                ctrl.Width = Math.Max(0, contentWidth - ctrl.Left);
                totalHeight = Math.Max(totalHeight, ctrl.Bottom);
            }

            _innerContentPanel.Height = totalHeight;

            if (preserveScrollRatio)
            {
                int maxScroll = GetMaxScroll();
                SetScrollPosition(-(int)(maxScroll * scrollRatio), animated: false);
            }
            else
            {
                ClampScrollState();
                ApplyScrollPosition();
            }

            UpdateScrollBarGeometry();
            Invalidate();
        }

        public void ResetScroll()
        {
            ScrollToTop();
        }

        public void ScrollToTop(bool animated = false)
        {
            SetScrollPosition(0, animated);
        }

        public void ScrollToBottom(bool animated = false)
        {
            SetScrollPosition(-GetMaxScroll(), animated);
        }

        public void ScrollToControl(Control control, bool animated = true)
        {
            if (control == null || control.Parent != _innerContentPanel)
            {
                return;
            }

            int desiredTop = -control.Top;
            SetScrollPosition(desiredTop, animated);
        }

        public void ScrollByWheelDeltaFromChild(int delta)
        {
            ScrollByWheelDelta(delta);
        }

        public void SetScrollPosition(int value, bool animated = false)
        {
            int clampedValue = ClampScrollValue(value);
            _targetScrollY = clampedValue;

            if (animated && SmoothScrollEnabled)
            {
                _smoothScrollTimer.Start();
                return;
            }

            _smoothScrollTimer.Stop();
            _scrollY = clampedValue;
            ApplyScrollPosition();
            UpdateScrollBarGeometry();
            Invalidate();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WsExComposited;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_thumbRect.IsEmpty)
            {
                return;
            }

            ThemePalette palette = ThemeManager.CurrentPalette;
            Color thumbColor = palette.Border;

            if (_isThumbPressed)
            {
                thumbColor = palette.Accent;
            }
            else if (_isThumbHovered)
            {
                thumbColor = palette.SurfaceHover;
            }

            using SolidBrush thumbBrush = new SolidBrush(thumbColor);
            e.Graphics.FillRectangle(thumbBrush, _thumbRect);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (_thumbHitRect.Contains(e.Location))
            {
                _isThumbPressed = true;
                _mouseClickOffsetY = e.Y - _thumbRect.Y;
                Capture = true;
                Invalidate();
                return;
            }

            if (_trackRect.Contains(e.Location))
            {
                int direction = e.Y < _thumbRect.Y ? 1 : -1;
                SetScrollPosition(_targetScrollY + direction * Height, animated: true);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool isOverThumb = _thumbHitRect.Contains(e.Location);
            if (isOverThumb != _isThumbHovered)
            {
                _isThumbHovered = isOverThumb;
                Invalidate();
            }

            if (!_isThumbPressed || _thumbRect.IsEmpty)
            {
                return;
            }

            int maxScroll = GetMaxScroll();
            int maxThumbTop = Math.Max(1, Height - _thumbRect.Height);
            int newThumbTop = e.Y - _mouseClickOffsetY;

            newThumbTop = Math.Max(0, Math.Min(maxThumbTop, newThumbTop));

            double scrollPercent = (double)newThumbTop / maxThumbTop;
            SetScrollPosition(-(int)(maxScroll * scrollPercent), animated: false);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_isThumbPressed)
            {
                return;
            }

            if (_isThumbHovered)
            {
                _isThumbHovered = false;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_isThumbPressed)
            {
                _isThumbPressed = false;
                Capture = false;
                Invalidate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _smoothScrollTimer.Stop();
                _smoothScrollTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        private void HVACScrollableContainer_MouseWheel(object? sender, MouseEventArgs e)
        {
            ScrollByWheelDelta(e.Delta);
        }

        private void ScrollByWheelDelta(int delta)
        {
            if (GetMaxScroll() <= 0)
            {
                return;
            }

            int scrollStep = Math.Max(24, (int)(40 * (ThemeMetrics.ButtonHeight / 28.0)));
            int direction = delta > 0 ? 1 : -1;

            SetScrollPosition(_targetScrollY + direction * scrollStep, animated: true);
        }

        private void SmoothScrollTimer_Tick(object? sender, EventArgs e)
        {
            int diff = _targetScrollY - _scrollY;

            if (Math.Abs(diff) <= 1)
            {
                _scrollY = _targetScrollY;
                _smoothScrollTimer.Stop();
            }
            else
            {
                int step = (int)(diff * 0.28);
                if (step == 0)
                {
                    step = diff > 0 ? 1 : -1;
                }

                _scrollY += step;
            }

            ApplyScrollPosition();
            UpdateScrollBarGeometry();
            Invalidate();
        }

        private void InnerContentPanel_ControlAdded(object? sender, ControlEventArgs e)
        {
            if (e.Control != null)
            {
                RegisterMouseWheelForwarding(e.Control);
            }

            RecalculateContentLayout();
        }

        private void InnerContentPanel_ControlRemoved(object? sender, ControlEventArgs e)
        {
            if (e.Control != null)
            {
                UnregisterMouseWheelForwarding(e.Control);
            }

            RecalculateContentLayout();
        }

        private void RegisterMouseWheelForwarding(Control control)
        {
            control.MouseWheel += ChildControl_MouseWheel;

            foreach (Control child in control.Controls)
            {
                RegisterMouseWheelForwarding(child);
            }

            control.ControlAdded += ChildControl_ControlAdded;
            control.ControlRemoved += ChildControl_ControlRemoved;
        }

        private void UnregisterMouseWheelForwarding(Control control)
        {
            control.MouseWheel -= ChildControl_MouseWheel;
            control.ControlAdded -= ChildControl_ControlAdded;
            control.ControlRemoved -= ChildControl_ControlRemoved;

            foreach (Control child in control.Controls)
            {
                UnregisterMouseWheelForwarding(child);
            }
        }

        private void ChildControl_ControlAdded(object? sender, ControlEventArgs e)
        {
            if (e.Control != null)
            {
                RegisterMouseWheelForwarding(e.Control);
            }
        }

        private void ChildControl_ControlRemoved(object? sender, ControlEventArgs e)
        {
            if (e.Control != null)
            {
                UnregisterMouseWheelForwarding(e.Control);
            }
        }

        private void ChildControl_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (!CaptureChildMouseWheel)
            {
                return;
            }

            ScrollByWheelDelta(e.Delta);
        }

        private void ApplyScrollPosition()
        {
            ClampScrollState();
            _innerContentPanel.Top = _scrollY;
            Invalidate(true);

            if (_isThumbPressed)
            {
                RefreshContentDuringDrag();
            }
        }

        private void RefreshContentDuringDrag()
        {
            _innerContentPanel.Invalidate(true);
            _innerContentPanel.Update();
            Update();
        }

        private void UpdateScrollBarGeometry()
        {
            int totalHeight = _innerContentPanel.Height;
            int containerHeight = Height;

            _trackRect = Rectangle.Empty;
            _thumbRect = Rectangle.Empty;
            _thumbHitRect = Rectangle.Empty;

            if (totalHeight <= containerHeight || containerHeight <= 0)
            {
                return;
            }

            _trackRect = new Rectangle(
                Math.Max(0, Width - ScrollHitWidth),
                0,
                ScrollHitWidth,
                containerHeight);

            int thumbHeight = (int)((double)containerHeight / totalHeight * containerHeight);
            thumbHeight = Math.Max(MinThumbHeight, Math.Min(containerHeight, thumbHeight));

            int maxScroll = GetMaxScroll();
            double scrollPercent = maxScroll == 0 ? 0 : (double)Math.Abs(_scrollY) / maxScroll;
            int thumbTop = (int)((containerHeight - thumbHeight) * scrollPercent);

            _thumbRect = new Rectangle(
                Math.Max(0, Width - ScrollBarWidth),
                thumbTop,
                ScrollBarWidth,
                thumbHeight);

            _thumbHitRect = new Rectangle(
                Math.Max(0, Width - ScrollHitWidth),
                thumbTop,
                ScrollHitWidth,
                thumbHeight);
        }

        private int GetMaxScroll()
        {
            return Math.Max(0, _innerContentPanel.Height - Height);
        }

        private int ClampScrollValue(int value)
        {
            int maxScroll = GetMaxScroll();
            return Math.Max(-maxScroll, Math.Min(0, value));
        }

        private void ClampScrollState()
        {
            _scrollY = ClampScrollValue(_scrollY);
            _targetScrollY = ClampScrollValue(_targetScrollY);
        }

        private double GetScrollRatio()
        {
            int maxScroll = GetMaxScroll();
            if (maxScroll <= 0)
            {
                return 0;
            }

            return Math.Abs(_scrollY) / (double)maxScroll;
        }

        private sealed class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint,
                    true);
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= WsExComposited;
                    return cp;
                }
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                using SolidBrush backgroundBrush = new SolidBrush(BackColor);
                e.Graphics.FillRectangle(backgroundBrush, ClientRectangle);
            }
        }
    }
}
