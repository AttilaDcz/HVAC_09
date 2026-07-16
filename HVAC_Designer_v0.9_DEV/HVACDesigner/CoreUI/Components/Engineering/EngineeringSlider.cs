using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Engineering
{
    public enum EngineeringSliderMode
    {
        Continuous,
        Stepped,
        Integer
    }

    public sealed class EngineeringSlider : Control, IThemeable
    {
        private ThemePalette palette = ThemeManager.CurrentPalette;
        private double minimum = 0d;
        private double maximum = 100d;
        private double value = 50d;
        private double step = 1d;
        private int decimals = 0;
        private string unitLabel = string.Empty;
        private string labelText = "Érték";
        private bool showLabel = true;
        private bool showValue = true;
        private bool showScaleLabels = true;
        private bool showTicks = true;
        private int majorTickCount = 5;
        private EngineeringSliderMode mode = EngineeringSliderMode.Stepped;
        private bool isHovered;
        private bool isDragging;
        private bool showFocusCue;

        public EngineeringSlider()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.UserPaint,
                true);

            Cursor = Cursors.Hand;
            TabStop = true;
            Size = new Size(280, 74);
            MinimumSize = new Size(140, 48);
            Font = ThemeFonts.Body;
            ApplyTheme(palette);
        }

        public event EventHandler? ValueChanged;

        [Category("Engineering")]
        [DefaultValue(0d)]
        public double Minimum
        {
            get => minimum;
            set
            {
                if (Math.Abs(minimum - value) < double.Epsilon)
                    return;

                minimum = value;
                if (maximum <= minimum)
                    maximum = minimum + 1d;

                Value = this.value;
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(100d)]
        public double Maximum
        {
            get => maximum;
            set
            {
                if (Math.Abs(maximum - value) < double.Epsilon)
                    return;

                maximum = value;
                if (maximum <= minimum)
                    minimum = maximum - 1d;

                Value = this.value;
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(50d)]
        public double Value
        {
            get => value;
            set => SetValue(value, false);
        }

        [Category("Engineering")]
        [DefaultValue(1d)]
        public double Step
        {
            get => step;
            set
            {
                step = value <= 0d ? 1d : value;
                if (mode != EngineeringSliderMode.Continuous)
                    Value = this.value;
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(0)]
        public int Decimals
        {
            get => decimals;
            set
            {
                decimals = Math.Max(0, Math.Min(6, value));
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue("")]
        public string UnitLabel
        {
            get => unitLabel;
            set
            {
                unitLabel = value ?? string.Empty;
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue("Érték")]
        public string LabelText
        {
            get => labelText;
            set
            {
                labelText = value ?? string.Empty;
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(true)]
        public bool ShowLabel
        {
            get => showLabel;
            set
            {
                showLabel = value;
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(true)]
        public bool ShowValue
        {
            get => showValue;
            set
            {
                showValue = value;
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(true)]
        public bool ShowScaleLabels
        {
            get => showScaleLabels;
            set
            {
                showScaleLabels = value;
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(true)]
        public bool ShowTicks
        {
            get => showTicks;
            set
            {
                showTicks = value;
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(5)]
        public int MajorTickCount
        {
            get => majorTickCount;
            set
            {
                majorTickCount = Math.Max(2, Math.Min(12, value));
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(EngineeringSliderMode.Stepped)]
        public EngineeringSliderMode SliderMode
        {
            get => mode;
            set
            {
                if (mode == value)
                    return;

                mode = value;
                Value = this.value;
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double DecimalValue => Value;

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.Surface;
            ForeColor = palette.TextPrimary;
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = false;
            if (!isDragging)
                Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!Enabled || e.Button != MouseButtons.Left)
                return;

            Focus();
            showFocusCue = false;
            isDragging = true;
            Capture = true;
            SetValueFromPoint(e.X);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isDragging)
                SetValueFromPoint(e.X);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!isDragging)
                return;

            isDragging = false;
            Capture = false;
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!Enabled)
                return;

            double keyboardStep = ResolveKeyboardStep(e);
            if (Math.Abs(keyboardStep) < double.Epsilon)
                return;

            showFocusCue = true;
            SetValue(Value + keyboardStep, true);
            e.Handled = true;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            showFocusCue = ShowFocusCues;
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            showFocusCue = false;
            isDragging = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using SolidBrush backgroundBrush = new SolidBrush(BackColor);
            g.FillRectangle(backgroundBrush, ClientRectangle);

            SliderLayout layout = CalculateLayout(g);
            DrawHeader(g, layout);
            DrawTrack(g, layout);
            DrawScale(g, layout);
        }

        private void DrawHeader(Graphics g, SliderLayout layout)
        {
            if (!ShowLabel && !ShowValue)
                return;

            if (ShowLabel && !string.IsNullOrWhiteSpace(LabelText))
            {
                TextRenderer.DrawText(
                    g,
                    LabelText,
                    ThemeFonts.Caption,
                    layout.LabelBounds,
                    Enabled ? palette.TextSecondary : palette.TextDisabled,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding);
            }

            if (ShowValue)
            {
                TextRenderer.DrawText(
                    g,
                    FormatValue(Value, true),
                    ThemeFonts.BodyBold,
                    layout.ValueBounds,
                    Enabled ? palette.TextPrimary : palette.TextDisabled,
                    TextFormatFlags.Right |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding);
            }
        }

        private void DrawTrack(Graphics g, SliderLayout layout)
        {
            Color trackColor = Enabled ? palette.Border : palette.BorderLight;
            Color fillColor = Enabled ? palette.Accent : palette.TextDisabled;
            if (isHovered || isDragging)
                fillColor = palette.AccentHover;

            Rectangle track = new Rectangle(
                layout.TrackBounds.Left,
                layout.TrackBounds.Top + (layout.TrackBounds.Height / 2) - 2,
                layout.TrackBounds.Width,
                4);

            using SolidBrush trackBrush = new SolidBrush(trackColor);
            g.FillRectangle(trackBrush, track);

            int thumbCenterX = ValueToX(Value, layout.TrackBounds);
            Rectangle fill = new Rectangle(
                track.Left,
                track.Top,
                Math.Max(0, thumbCenterX - track.Left),
                track.Height);
            using SolidBrush fillBrush = new SolidBrush(fillColor);
            g.FillRectangle(fillBrush, fill);

            int thumbHeight = Math.Max(18, layout.TrackBounds.Height - 4);
            int trackCenterY = track.Top + track.Height / 2;
            Rectangle thumb = new Rectangle(
                thumbCenterX - 4,
                trackCenterY - thumbHeight / 2,
                8,
                thumbHeight);
            using SolidBrush thumbBrush = new SolidBrush(fillColor);
            g.FillRectangle(thumbBrush, thumb);

            Color thumbBorderColor = showFocusCue && Focused ? palette.BorderStrong : palette.Surface;
            using Pen thumbBorderPen = new Pen(thumbBorderColor, 1f);
            g.DrawRectangle(thumbBorderPen, thumb);

            if (showFocusCue && Focused)
            {
                Rectangle focus = Rectangle.Inflate(layout.TrackBounds, 3, 2);
                using Pen focusPen = new Pen(palette.BorderStrong, 1f);
                g.DrawRectangle(focusPen, focus);
            }
        }

        private void DrawScale(Graphics g, SliderLayout layout)
        {
            if (!ShowTicks && !ShowScaleLabels)
                return;

            for (int i = 0; i < MajorTickCount; i++)
            {
                double ratio = MajorTickCount == 1 ? 0d : i / (double)(MajorTickCount - 1);
                double tickValue = Minimum + ((Maximum - Minimum) * ratio);
                int x = layout.TrackBounds.Left + (int)Math.Round(layout.TrackBounds.Width * ratio);

                if (ShowTicks)
                {
                    using Pen tickPen = new Pen(Enabled ? palette.Border : palette.BorderLight, 1f);
                    g.DrawLine(tickPen, x, layout.TrackBounds.Bottom + 3, x, Math.Min(layout.ScaleTop - 2, layout.TrackBounds.Bottom + layout.TickHeight));
                }

                if (!ShowScaleLabels)
                    continue;

                string text = FormatValue(tickValue, false);
                Size labelSize = TextRenderer.MeasureText(
                    text,
                    ThemeFonts.Caption,
                    new Size(int.MaxValue, layout.ScaleLabelHeight),
                    TextFormatFlags.NoPadding);

                Rectangle labelBounds = new Rectangle(
                    x - labelSize.Width / 2,
                    layout.ScaleTop,
                    labelSize.Width + 2,
                    layout.ScaleLabelHeight);

                TextRenderer.DrawText(
                    g,
                    text,
                    ThemeFonts.Caption,
                    labelBounds,
                    Enabled ? palette.TextDisabled : Blend(palette.TextDisabled, BackColor, 0.55),
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.Top |
                    TextFormatFlags.NoPadding);
            }
        }

        private SliderLayout CalculateLayout(Graphics graphics)
        {
            int headerHeight = ShowLabel || ShowValue ? 22 : 2;
            int scaleHeight = ShowScaleLabels ? 18 : 2;
            int tickHeight = ShowTicks ? 10 : 2;
            int trackHeight = 22;

            string minText = FormatValue(Minimum, false);
            string maxText = FormatValue(Maximum, false);
            Size minSize = TextRenderer.MeasureText(graphics, minText, ThemeFonts.Caption, Size.Empty, TextFormatFlags.NoPadding);
            Size maxSize = TextRenderer.MeasureText(graphics, maxText, ThemeFonts.Caption, Size.Empty, TextFormatFlags.NoPadding);

            int leftPadding = Math.Max(10, minSize.Width / 2 + 4);
            int rightPadding = Math.Max(10, maxSize.Width / 2 + 4);
            int scaleTop = Height - scaleHeight;
            int trackBottom = scaleTop - tickHeight - 1;
            int trackTop = Math.Max(headerHeight + 2, trackBottom - trackHeight);
            if (trackTop + trackHeight > trackBottom)
                trackHeight = Math.Max(16, trackBottom - trackTop);
            int usableWidth = Math.Max(32, Width - leftPadding - rightPadding);

            Rectangle trackBounds = new Rectangle(leftPadding, trackTop, usableWidth, trackHeight);
            Rectangle labelBounds = new Rectangle(0, 0, Math.Max(0, Width / 2), headerHeight);
            Rectangle valueBounds = new Rectangle(Width / 2, 0, Math.Max(0, Width / 2), headerHeight);

            return new SliderLayout(
                labelBounds,
                valueBounds,
                trackBounds,
                tickHeight,
                scaleTop,
                scaleHeight);
        }

        private void SetValueFromPoint(int x)
        {
            using Graphics graphics = CreateGraphics();
            SliderLayout layout = CalculateLayout(graphics);
            double ratio = (x - layout.TrackBounds.Left) / (double)Math.Max(1, layout.TrackBounds.Width);
            double next = Minimum + ((Maximum - Minimum) * Math.Max(0d, Math.Min(1d, ratio)));
            SetValue(next, true);
        }

        private void SetValue(double nextValue, bool raiseEvent)
        {
            double normalized = NormalizeValue(nextValue);
            if (Math.Abs(value - normalized) < Math.Pow(10, -(Math.Min(6, Decimals) + 3)))
                return;

            value = normalized;
            Invalidate();

            if (raiseEvent)
                ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private double NormalizeValue(double input)
        {
            double clamped = Math.Max(Minimum, Math.Min(Maximum, input));

            if (SliderMode == EngineeringSliderMode.Integer)
                clamped = Math.Round(clamped);
            else if (SliderMode == EngineeringSliderMode.Stepped)
                clamped = Minimum + (Math.Round((clamped - Minimum) / Step) * Step);

            clamped = Math.Max(Minimum, Math.Min(Maximum, clamped));
            if (SliderMode == EngineeringSliderMode.Continuous)
                return clamped;

            return Decimals > 0 ? Math.Round(clamped, Decimals + 2) : Math.Round(clamped, 0);
        }

        private int ValueToX(double currentValue, Rectangle trackBounds)
        {
            double range = Maximum - Minimum;
            if (range <= 0d)
                return trackBounds.Left;

            double ratio = (currentValue - Minimum) / range;
            return trackBounds.Left + (int)Math.Round(trackBounds.Width * Math.Max(0d, Math.Min(1d, ratio)));
        }

        private string FormatValue(double currentValue, bool includeUnit)
        {
            string text = currentValue.ToString("F" + Decimals);
            if (includeUnit && !string.IsNullOrWhiteSpace(UnitLabel))
                text += " " + UnitLabel;
            return text;
        }

        private double ResolveKeyboardStep(KeyEventArgs e)
        {
            double increment = SliderMode == EngineeringSliderMode.Continuous
                ? Math.Max((Maximum - Minimum) / 100d, Step)
                : Step;

            if (SliderMode == EngineeringSliderMode.Integer)
                increment = Math.Max(1d, Math.Round(increment));

            return e.KeyCode switch
            {
                Keys.Left or Keys.Down => -increment,
                Keys.Right or Keys.Up => increment,
                Keys.PageDown => -increment * 10d,
                Keys.PageUp => increment * 10d,
                Keys.Home => Minimum - Value,
                Keys.End => Maximum - Value,
                _ => 0d
            };
        }

        private static Color Blend(Color foreground, Color background, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            int r = (int)(background.R + ((foreground.R - background.R) * amount));
            int g = (int)(background.G + ((foreground.G - background.G) * amount));
            int b = (int)(background.B + ((foreground.B - background.B) * amount));
            return Color.FromArgb(r, g, b);
        }

        private readonly struct SliderLayout
        {
            public SliderLayout(
                Rectangle labelBounds,
                Rectangle valueBounds,
                Rectangle trackBounds,
                int tickHeight,
                int scaleTop,
                int scaleLabelHeight)
            {
                LabelBounds = labelBounds;
                ValueBounds = valueBounds;
                TrackBounds = trackBounds;
                TickHeight = tickHeight;
                ScaleTop = scaleTop;
                ScaleLabelHeight = scaleLabelHeight;
            }

            public Rectangle LabelBounds { get; }
            public Rectangle ValueBounds { get; }
            public Rectangle TrackBounds { get; }
            public int TickHeight { get; }
            public int ScaleTop { get; }
            public int ScaleLabelHeight { get; }
        }
    }
}
