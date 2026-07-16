using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Help
{
    public enum EngineeringToolTipKind
    {
        Neutral,
        Info,
        Success,
        Warning,
        Danger
    }

    public sealed class EngineeringToolTip : ToolTip, IThemeable
    {
        private const int MaxContentWidth = 320;
        private const int AccentWidth = 3;
        private readonly Dictionary<Control, ToolTipContent> contentByControl = new Dictionary<Control, ToolTipContent>();
        private ThemePalette palette = ThemeManager.CurrentPalette;

        public EngineeringToolTip()
        {
            OwnerDraw = true;
            InitialDelay = 350;
            ReshowDelay = 70;
            AutoPopDelay = 7000;
            ShowAlways = true;
            UseAnimation = true;
            UseFading = true;

            Popup += HandlePopup;
            Draw += HandleDraw;
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
        }

        public void SetHelp(
            Control control,
            string text,
            string title = "",
            EngineeringToolTipKind kind = EngineeringToolTipKind.Neutral)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            string safeText = text ?? string.Empty;
            string safeTitle = title ?? string.Empty;

            if (string.IsNullOrWhiteSpace(safeText) && string.IsNullOrWhiteSpace(safeTitle))
            {
                contentByControl.Remove(control);
                SetToolTip(control, string.Empty);
                Hide(control);
                return;
            }

            contentByControl[control] = new ToolTipContent(safeTitle, safeText, kind);
            SetToolTip(control, string.IsNullOrWhiteSpace(safeTitle) ? safeText : safeTitle);
        }

        public void SetHelpRecursive(
            Control control,
            string text,
            string title = "",
            EngineeringToolTipKind kind = EngineeringToolTipKind.Neutral)
        {
            SetHelp(control, text, title, kind);

            foreach (Control child in control.Controls)
                SetHelpRecursive(child, text, title, kind);
        }

        public void ClearHelp(Control control)
        {
            SetHelp(control, string.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
                Popup -= HandlePopup;
                Draw -= HandleDraw;
                contentByControl.Clear();
            }

            base.Dispose(disposing);
        }

        private void ThemeManager_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            ApplyTheme(e.Palette);
        }

        private void HandlePopup(object? sender, PopupEventArgs e)
        {
            ToolTipContent content = ResolveContent(e.AssociatedControl);
            e.ToolTipSize = MeasureContent(content);
        }

        private void HandleDraw(object? sender, DrawToolTipEventArgs e)
        {
            ToolTipContent content = ResolveContent(e.AssociatedControl);
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle bounds = e.Bounds;
            Color accent = ResolveKindColor(content.Kind);

            using SolidBrush backgroundBrush = new SolidBrush(palette.SurfaceAlt);
            using Pen borderPen = new Pen(palette.Border);
            using SolidBrush accentBrush = new SolidBrush(accent);

            g.FillRectangle(backgroundBrush, bounds);
            g.FillRectangle(accentBrush, bounds.Left, bounds.Top, AccentWidth, bounds.Height);
            g.DrawRectangle(borderPen, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1);

            int x = bounds.Left + 12;
            int y = bounds.Top + 8;
            int width = Math.Max(20, bounds.Width - 22);

            if (!string.IsNullOrWhiteSpace(content.Title))
            {
                TextRenderer.DrawText(
                    g,
                    content.Title,
                    ThemeFonts.BodyBold,
                    new Rectangle(x, y, width, 18),
                    palette.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                y += 22;
            }

            if (!string.IsNullOrWhiteSpace(content.Text))
            {
                TextRenderer.DrawText(
                    g,
                    content.Text,
                    ThemeFonts.Caption,
                    new Rectangle(x, y, width, Math.Max(18, bounds.Bottom - y - 7)),
                    palette.TextSecondary,
                    TextFormatFlags.Left |
                    TextFormatFlags.Top |
                    TextFormatFlags.WordBreak |
                    TextFormatFlags.NoPrefix);
            }
        }

        private ToolTipContent ResolveContent(Control? control)
        {
            if (control != null && contentByControl.TryGetValue(control, out ToolTipContent? content))
                return content;

            string text = control == null ? string.Empty : GetToolTip(control);
            return new ToolTipContent(string.Empty, text, EngineeringToolTipKind.Neutral);
        }

        private static Size MeasureContent(ToolTipContent content)
        {
            int titleWidth = 0;
            int titleHeight = 0;

            if (!string.IsNullOrWhiteSpace(content.Title))
            {
                Size titleSize = TextRenderer.MeasureText(
                    content.Title,
                    ThemeFonts.BodyBold,
                    new Size(MaxContentWidth, int.MaxValue),
                    TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                titleWidth = titleSize.Width;
                titleHeight = 22;
            }

            int bodyWidth = 0;
            int bodyHeight = 0;

            if (!string.IsNullOrWhiteSpace(content.Text))
            {
                Size bodySize = TextRenderer.MeasureText(
                    content.Text,
                    ThemeFonts.Caption,
                    new Size(MaxContentWidth, int.MaxValue),
                    TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                bodyWidth = bodySize.Width;
                bodyHeight = Math.Max(18, bodySize.Height);
            }

            int width = Math.Min(MaxContentWidth + 24, Math.Max(120, Math.Max(titleWidth, bodyWidth) + 24));
            int height = Math.Max(34, titleHeight + bodyHeight + 18);
            return new Size(width, height);
        }

        private Color ResolveKindColor(EngineeringToolTipKind kind)
        {
            return kind switch
            {
                EngineeringToolTipKind.Info => palette.Info,
                EngineeringToolTipKind.Success => palette.Success,
                EngineeringToolTipKind.Warning => palette.Warning,
                EngineeringToolTipKind.Danger => palette.Danger,
                _ => palette.Accent
            };
        }

        private sealed class ToolTipContent
        {
            public ToolTipContent(string title, string text, EngineeringToolTipKind kind)
            {
                Title = title;
                Text = text;
                Kind = kind;
            }

            public string Title { get; }
            public string Text { get; }
            public EngineeringToolTipKind Kind { get; }
        }
    }
}
