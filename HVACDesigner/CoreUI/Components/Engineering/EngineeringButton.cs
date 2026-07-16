using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Engineering
{
    public enum EngineeringButtonVariant
    {
        Primary,
        Secondary,
        Ghost,
        Danger,
        Success,
        Warning,
        Info
    }

    public enum EngineeringButtonSize
    {
        Compact,
        Normal,
        Large
    }

    public enum EngineeringButtonIconPlacement
    {
        None,
        Left,
        Right,
        IconOnly
    }

    public sealed class EngineeringButton : Control, IThemeable
    {
        private ThemePalette palette = ThemeManager.CurrentPalette;
        private EngineeringButtonVariant variant = EngineeringButtonVariant.Secondary;
        private EngineeringButtonSize buttonSize = EngineeringButtonSize.Normal;
        private EngineeringButtonIconPlacement iconPlacement = EngineeringButtonIconPlacement.None;
        private HvacIconKind? iconKind;
        private bool showBorder = true;
        private bool isLoading;
        private bool autoWidth;
        private bool isHovered;
        private bool isPressed;
        private bool showFocusCue;
        private bool useThemeFont = true;
        private bool applyingThemeFont;

        public EngineeringButton()
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
            Size = new Size(120, ThemeMetrics.ButtonHeight);
            Font = ThemeFonts.Body;
            ApplyTheme(palette);
        }

        [DefaultValue(EngineeringButtonVariant.Secondary)]
        public EngineeringButtonVariant Variant
        {
            get => variant;
            set
            {
                if (variant == value)
                    return;

                variant = value;
                Invalidate();
            }
        }

        [DefaultValue(EngineeringButtonSize.Normal)]
        public EngineeringButtonSize ButtonSize
        {
            get => buttonSize;
            set
            {
                if (buttonSize == value)
                    return;

                buttonSize = value;
                ApplySize();
                Invalidate();
            }
        }

        [DefaultValue(EngineeringButtonIconPlacement.None)]
        public EngineeringButtonIconPlacement IconPlacement
        {
            get => iconPlacement;
            set
            {
                if (iconPlacement == value)
                    return;

                iconPlacement = value;
                UpdateAutoWidth();
                Invalidate();
            }
        }

        [DefaultValue(null)]
        public HvacIconKind? IconKind
        {
            get => iconKind;
            set
            {
                if (iconKind == value)
                    return;

                iconKind = value;
                UpdateAutoWidth();
                Invalidate();
            }
        }

        [DefaultValue(true)]
        public bool ShowBorder
        {
            get => showBorder;
            set
            {
                if (showBorder == value)
                    return;

                showBorder = value;
                Invalidate();
            }
        }

        [DefaultValue(false)]
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (isLoading == value)
                    return;

                isLoading = value;
                Cursor = isLoading ? Cursors.WaitCursor : Cursors.Hand;
                Invalidate();
            }
        }

        [DefaultValue(false)]
        public bool AutoWidth
        {
            get => autoWidth;
            set
            {
                if (autoWidth == value)
                    return;

                autoWidth = value;
                UpdateAutoWidth();
            }
        }

        [DefaultValue(true)]
        public bool UseThemeFont
        {
            get => useThemeFont;
            set
            {
                if (useThemeFont == value)
                    return;

                useThemeFont = value;
                ApplySize();
                Invalidate();
            }
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            ForeColor = palette.TextPrimary;
            ApplySize();
            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            UpdateAutoWidth();
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? (isLoading ? Cursors.WaitCursor : Cursors.Hand) : Cursors.Default;
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            if (!applyingThemeFont)
                useThemeFont = false;

            UpdateAutoWidth();
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
            isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (Enabled && e.Button == MouseButtons.Left)
            {
                showFocusCue = false;
                Focus();
                isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (isPressed)
            {
                isPressed = false;
                Invalidate();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Enabled && (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter))
            {
                showFocusCue = true;
                isPressed = true;
                Invalidate();
                e.Handled = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (Enabled && (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter))
            {
                isPressed = false;
                Invalidate();
                OnClick(EventArgs.Empty);
                e.Handled = true;
            }
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
            isPressed = false;
            showFocusCue = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            ButtonColors colors = ResolveColors();
            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);

            using SolidBrush backgroundBrush = new SolidBrush(colors.Background);
            using Pen borderPen = new Pen(colors.Border);
            g.FillRectangle(backgroundBrush, bounds);

            bool drawFocusCue = showFocusCue && Focused && Enabled;

            if (showBorder || drawFocusCue || isHovered)
                g.DrawRectangle(borderPen, bounds);

            if (drawFocusCue)
            {
                using Pen focusPen = new Pen(colors.Focus);
                Rectangle focusBounds = new Rectangle(2, 2, Math.Max(0, Width - 5), Math.Max(0, Height - 5));
                g.DrawRectangle(focusPen, focusBounds);
            }

            DrawContent(g, colors);
        }

        private void DrawContent(Graphics g, ButtonColors colors)
        {
            string text = isLoading ? "Folyamatban..." : Text;
            bool hasIcon = iconKind.HasValue && iconPlacement != EngineeringButtonIconPlacement.None;
            bool iconOnly = hasIcon && iconPlacement == EngineeringButtonIconPlacement.IconOnly;
            int iconSize = ResolveIconSize();
            int gap = iconOnly ? 0 : ThemeMetrics.MarginSmall;
            Padding padding = ResolvePadding();

            Size textSize = iconOnly || string.IsNullOrWhiteSpace(text)
                ? Size.Empty
                : TextRenderer.MeasureText(text, Font, new Size(int.MaxValue, Height), TextFormatFlags.NoPadding);

            int contentWidth = (hasIcon ? iconSize : 0) +
                (hasIcon && textSize.Width > 0 ? gap : 0) +
                textSize.Width;

            int left = iconOnly
                ? (Width - iconSize) / 2
                : Math.Max(padding.Left, (Width - contentWidth) / 2);

            int centerY = Height / 2;
            Rectangle iconBounds = Rectangle.Empty;
            Rectangle textBounds = Rectangle.Empty;

            if (hasIcon && (iconPlacement == EngineeringButtonIconPlacement.Left || iconOnly))
            {
                iconBounds = new Rectangle(left, centerY - iconSize / 2, iconSize, iconSize);
                left += iconSize + gap;
            }

            if (!iconOnly && textSize.Width > 0)
            {
                int textOffsetY = ResolveTextOffsetY();
                textBounds = new Rectangle(
                    left,
                    textOffsetY,
                    Math.Max(0, Width - left - padding.Right),
                    Math.Max(0, Height - textOffsetY));
                left += textSize.Width + gap;
            }

            if (hasIcon && iconPlacement == EngineeringButtonIconPlacement.Right)
            {
                iconBounds = new Rectangle(left, centerY - iconSize / 2, iconSize, iconSize);
            }

            if (hasIcon && iconBounds.Width > 0)
            {
                using Bitmap icon = HvacIconRenderer.Render(
                    iconKind!.Value,
                    ThemeManager.CurrentThemeMode,
                    iconSize,
                    colors.Icon);
                g.DrawImage(icon, iconBounds);
            }

            if (!iconOnly && textBounds.Width > 0)
            {
                TextRenderer.DrawText(
                    g,
                    text,
                    Font,
                    textBounds,
                    colors.Text,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.PreserveGraphicsClipping);
            }
        }

        private int ResolveTextOffsetY()
        {
            return ButtonSize == EngineeringButtonSize.Compact ? -1 : -2;
        }

        private ButtonColors ResolveColors()
        {
            Color baseColor = ResolveVariantBaseColor();
            Color background;
            Color border;
            Color text;
            Color icon;

            if (!Enabled)
            {
                return new ButtonColors(
                    Blend(palette.SurfaceAlt, palette.Surface, 0.45),
                    palette.BorderLight,
                    palette.TextDisabled,
                    palette.TextDisabled,
                    ResolveFocusColor(baseColor));
            }

            if (variant == EngineeringButtonVariant.Ghost)
            {
                background = isPressed
                    ? palette.SurfacePressed
                    : isHovered ? palette.SurfaceHover : Color.Transparent;
                border = isHovered || (showFocusCue && Focused) ? palette.Border : Color.Transparent;
                text = palette.TextPrimary;
                icon = baseColor;
            }
            else if (variant == EngineeringButtonVariant.Secondary)
            {
                background = isPressed
                    ? palette.SurfacePressed
                    : isHovered ? palette.SurfaceHover : palette.SurfaceAlt;
                border = showFocusCue && Focused ? ResolveFocusColor(baseColor) : palette.Border;
                text = palette.TextPrimary;
                icon = palette.TextSecondary;
            }
            else
            {
                background = isPressed
                    ? Blend(baseColor, Color.Black, 0.78)
                    : isHovered ? Blend(baseColor, Color.White, 0.88) : baseColor;
                border = Blend(baseColor, palette.Border, 0.75);
                text = Color.White;
                icon = Color.White;
            }

            return new ButtonColors(background, border, text, icon, ResolveFocusColor(baseColor));
        }

        private Color ResolveFocusColor(Color baseColor)
        {
            return variant switch
            {
                EngineeringButtonVariant.Primary or
                EngineeringButtonVariant.Danger or
                EngineeringButtonVariant.Success or
                EngineeringButtonVariant.Warning or
                EngineeringButtonVariant.Info
                    => Color.White,
                EngineeringButtonVariant.Ghost
                    => palette.BorderStrong,
                _ => palette.BorderStrong
            };
        }

        private Color ResolveVariantBaseColor()
        {
            return variant switch
            {
                EngineeringButtonVariant.Primary => palette.Accent,
                EngineeringButtonVariant.Danger => palette.Danger,
                EngineeringButtonVariant.Success => palette.Success,
                EngineeringButtonVariant.Warning => palette.Warning,
                EngineeringButtonVariant.Info => palette.Info,
                EngineeringButtonVariant.Ghost => palette.TextSecondary,
                _ => palette.SurfaceAlt
            };
        }

        private Font ResolveFont()
        {
            return variant == EngineeringButtonVariant.Primary ||
                variant == EngineeringButtonVariant.Danger
                ? ThemeFonts.BodyBold
                : ThemeFonts.Body;
        }

        private int ResolveHeight()
        {
            return buttonSize switch
            {
                EngineeringButtonSize.Compact => Math.Max(22, ThemeMetrics.ButtonHeight - 4),
                EngineeringButtonSize.Large => ThemeMetrics.ButtonHeight + 8,
                _ => ThemeMetrics.ButtonHeight + 2
            };
        }

        private int ResolveIconSize()
        {
            return buttonSize switch
            {
                EngineeringButtonSize.Compact => ThemeMetrics.IconSizeSmall,
                EngineeringButtonSize.Large => ThemeMetrics.IconSizeLarge,
                _ => ThemeMetrics.IconSizeNormal
            };
        }

        private Padding ResolvePadding()
        {
            return buttonSize switch
            {
                EngineeringButtonSize.Compact => new Padding(8, 0, 8, 0),
                EngineeringButtonSize.Large => new Padding(16, 0, 16, 0),
                _ => new Padding(12, 0, 12, 0)
            };
        }

        private void ApplySize()
        {
            Height = ResolveHeight();
            if (useThemeFont)
                SetThemeFont(ResolveFont());
            UpdateAutoWidth();
        }

        private void UpdateAutoWidth()
        {
            if (!autoWidth)
                return;

            int iconSize = iconKind.HasValue && iconPlacement != EngineeringButtonIconPlacement.None
                ? ResolveIconSize()
                : 0;
            bool iconOnly = iconPlacement == EngineeringButtonIconPlacement.IconOnly && iconSize > 0;
            Padding padding = ResolvePadding();

            if (iconOnly)
            {
                Width = Math.Max(MinimumSize.Width, iconSize + padding.Left + padding.Right);
                return;
            }

            Size textSize = string.IsNullOrWhiteSpace(Text)
                ? Size.Empty
                : TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, Height), TextFormatFlags.NoPadding);

            int width = padding.Left + padding.Right + textSize.Width;
            if (iconSize > 0)
                width += iconSize + ThemeMetrics.MarginSmall;

            Width = Math.Max(Math.Max(32, MinimumSize.Width), width);
        }

        private void SetThemeFont(Font font)
        {
            applyingThemeFont = true;
            try
            {
                Font = font;
            }
            finally
            {
                applyingThemeFont = false;
            }
        }

        private static Color Blend(Color foreground, Color background, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            int r = (int)(background.R + ((foreground.R - background.R) * amount));
            int g = (int)(background.G + ((foreground.G - background.G) * amount));
            int b = (int)(background.B + ((foreground.B - background.B) * amount));
            return Color.FromArgb(r, g, b);
        }

        private readonly struct ButtonColors
        {
            public ButtonColors(
                Color background,
                Color border,
                Color text,
                Color icon,
                Color focus)
            {
                Background = background;
                Border = border;
                Text = text;
                Icon = icon;
                Focus = focus;
            }

            public Color Background { get; }
            public Color Border { get; }
            public Color Text { get; }
            public Color Icon { get; }
            public Color Focus { get; }
        }
    }
}
