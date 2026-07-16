using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Engineering
{
    public class EngineeringRadioButton : Control, IThemeable
    {
        private ThemePalette palette = ThemeManager.CurrentPalette;
        private bool isChecked;
        private bool isHovered;
        private bool isPressed;
        private bool isFocused;
        private ContentAlignment checkAlign = ContentAlignment.MiddleLeft;

        public EngineeringRadioButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint,
                true);

            Cursor = Cursors.Hand;
            TabStop = true;
            Text = "Választógomb";
            Size = new Size(170, Math.Max(28, ThemeMetrics.TextBoxHeight));
            MinimumSize = new Size(24, 24);

            ApplyTheme(palette);
        }

        public event EventHandler? CheckedChanged;

        [Category("Engineering")]
        [DefaultValue(false)]
        public bool Checked
        {
            get => isChecked;
            set
            {
                if (isChecked == value)
                    return;

                isChecked = value;

                if (isChecked)
                    UncheckSiblingRadioButtons();

                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("Engineering")]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment CheckAlign
        {
            get => checkAlign;
            set
            {
                if (checkAlign == value)
                    return;

                checkAlign = value;
                Invalidate();
            }
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = Color.Transparent;
            ForeColor = Enabled ? palette.TextPrimary : palette.TextDisabled;
            Font = ThemeFonts.Body;
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            ApplyTheme(palette);
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
            isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (Enabled && e.Button == MouseButtons.Left)
            {
                Focus();
                isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!Enabled || e.Button != MouseButtons.Left)
                return;

            bool wasPressed = isPressed;
            isPressed = false;

            if (wasPressed && ClientRectangle.Contains(e.Location))
                Checked = true;

            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            isFocused = true;
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            isFocused = false;
            isPressed = false;
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Enabled && (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter))
            {
                Checked = true;
                e.Handled = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle glyphBounds = GetGlyphBounds();
            Rectangle textBounds = GetTextBounds(glyphBounds);

            DrawGlyph(g, glyphBounds);
            DrawText(g, textBounds);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        private void UncheckSiblingRadioButtons()
        {
            if (Parent == null)
                return;

            foreach (Control sibling in Parent.Controls)
            {
                if (!ReferenceEquals(sibling, this) &&
                    sibling is EngineeringRadioButton radioButton &&
                    radioButton.Checked)
                {
                    radioButton.Checked = false;
                }
            }
        }

        private Rectangle GetGlyphBounds()
        {
            int glyphSize = Math.Min(16, Math.Max(14, Height - 10));
            int top = Math.Max(0, (Height - glyphSize) / 2);
            int left = IsRightAligned()
                ? Math.Max(0, Width - glyphSize - ThemeMetrics.MarginSmall)
                : ThemeMetrics.MarginSmall;

            return new Rectangle(left, top, glyphSize, glyphSize);
        }

        private Rectangle GetTextBounds(Rectangle glyphBounds)
        {
            int gap = ThemeMetrics.MarginSmall;

            if (IsRightAligned())
            {
                return new Rectangle(
                    0,
                    0,
                    Math.Max(0, glyphBounds.Left - gap),
                    Height);
            }

            int left = glyphBounds.Right + gap;
            return new Rectangle(
                left,
                0,
                Math.Max(0, Width - left),
                Height);
        }

        private bool IsRightAligned()
        {
            return checkAlign == ContentAlignment.TopRight ||
                   checkAlign == ContentAlignment.MiddleRight ||
                   checkAlign == ContentAlignment.BottomRight;
        }

        private void DrawGlyph(Graphics g, Rectangle bounds)
        {
            Color fillColor = Enabled
                ? isPressed
                    ? palette.SurfacePressed
                    : isHovered
                        ? palette.SurfaceAlt
                        : palette.Surface
                : palette.Surface;

            Color borderColor = Enabled
                ? isFocused
                    ? palette.BorderStrong
                    : isChecked
                        ? palette.Accent
                        : isHovered
                            ? palette.Border
                            : palette.BorderLight
                : palette.BorderLight;

            RectangleF softBounds = new RectangleF(
                bounds.Left + 0.5f,
                bounds.Top + 0.5f,
                bounds.Width - 1f,
                bounds.Height - 1f);

            using SolidBrush fillBrush = new SolidBrush(fillColor);
            g.FillEllipse(fillBrush, softBounds);

            using Pen borderPen = new Pen(borderColor, 1.25f);
            g.DrawEllipse(borderPen, softBounds);

            if (isFocused)
            {
                Rectangle focusBounds = Rectangle.Inflate(bounds, 2, 2);
                using Pen focusPen = new Pen(Color.FromArgb(90, palette.BorderStrong), 1f);
                g.DrawEllipse(focusPen, focusBounds);
            }

            if (!isChecked)
                return;

            Rectangle inner = Rectangle.Inflate(bounds, -5, -5);
            if (inner.Width <= 0 || inner.Height <= 0)
                return;

            using SolidBrush selectedBrush = new SolidBrush(Enabled ? palette.Accent : palette.TextDisabled);
            g.FillEllipse(selectedBrush, inner);
        }

        private void DrawText(Graphics g, Rectangle textBounds)
        {
            if (string.IsNullOrEmpty(Text))
                return;

            TextRenderer.DrawText(
                g,
                Text,
                Font,
                textBounds,
                ResolveTextColor(),
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding);
        }

        private Color ResolveTextColor()
        {
            if (!Enabled)
                return palette.TextDisabled;

            if (isChecked || isHovered || isFocused)
                return palette.TextPrimary;

            return palette.TextSecondary;
        }
    }
}
