using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Results
{
    public enum EngineeringResultStatus
    {
        Neutral,
        Info,
        Success,
        Warning,
        Danger
    }

    public enum EngineeringAiSupportLevel
    {
        None,
        RuleCheck,
        Recommendation,
        Assistant
    }

    public enum EngineeringResultDiagnosticSeverity
    {
        Info,
        Warning,
        Error
    }

    public sealed class EngineeringResultDiagnostic
    {
        public EngineeringResultDiagnostic(
            string code,
            string message,
            EngineeringResultDiagnosticSeverity severity)
        {
            Code = code?.Trim() ?? string.Empty;
            Message = message?.Trim() ?? string.Empty;
            Severity = severity;
        }

        public string Code { get; }
        public string Message { get; }
        public EngineeringResultDiagnosticSeverity Severity { get; }
    }

    public sealed class EngineeringResultReference
    {
        public EngineeringResultReference(string title, string clause = "")
        {
            Title = title?.Trim() ?? string.Empty;
            Clause = clause?.Trim() ?? string.Empty;
        }

        public string Title { get; }
        public string Clause { get; }

        public string ToDisplayText()
        {
            return string.IsNullOrWhiteSpace(Clause)
                ? Title
                : $"{Title}, {Clause}";
        }
    }

    public sealed class EngineeringResultCardModel
    {
        private readonly ReadOnlyCollection<EngineeringResultDiagnostic> diagnostics;
        private readonly ReadOnlyCollection<EngineeringResultReference> references;

        public EngineeringResultCardModel(
            string title,
            string primaryValue,
            string primaryUnit,
            EngineeringResultStatus status,
            string subtitle = "",
            string limitText = "",
            string sourceText = "",
            string recommendationText = "",
            EngineeringAiSupportLevel aiLevel = EngineeringAiSupportLevel.None,
            IEnumerable<EngineeringResultDiagnostic>? diagnostics = null,
            IEnumerable<EngineeringResultReference>? references = null)
        {
            Title = title?.Trim() ?? string.Empty;
            PrimaryValue = primaryValue?.Trim() ?? string.Empty;
            PrimaryUnit = primaryUnit?.Trim() ?? string.Empty;
            Status = status;
            Subtitle = subtitle?.Trim() ?? string.Empty;
            LimitText = limitText?.Trim() ?? string.Empty;
            SourceText = sourceText?.Trim() ?? string.Empty;
            RecommendationText = recommendationText?.Trim() ?? string.Empty;
            AiLevel = aiLevel;

            this.diagnostics = new ReadOnlyCollection<EngineeringResultDiagnostic>(
                (diagnostics ?? Enumerable.Empty<EngineeringResultDiagnostic>()).ToList());
            this.references = new ReadOnlyCollection<EngineeringResultReference>(
                (references ?? Enumerable.Empty<EngineeringResultReference>()).ToList());
        }

        public string Title { get; }
        public string PrimaryValue { get; }
        public string PrimaryUnit { get; }
        public EngineeringResultStatus Status { get; }
        public string Subtitle { get; }
        public string LimitText { get; }
        public string SourceText { get; }
        public string RecommendationText { get; }
        public EngineeringAiSupportLevel AiLevel { get; }
        public IReadOnlyList<EngineeringResultDiagnostic> Diagnostics => diagnostics;
        public IReadOnlyList<EngineeringResultReference> References => references;
    }

    public sealed class EngineeringResultCard : Control, IThemeable
    {
        private ThemePalette palette = ThemeManager.CurrentPalette;
        private EngineeringResultCardModel model = new EngineeringResultCardModel(
            "Eredmény",
            "-",
            string.Empty,
            EngineeringResultStatus.Neutral);
        private bool isHovered;
        private bool showDetails;
        private const int CompactHeight = 164;
        private const int MinimumExpandedHeight = 238;

        public EngineeringResultCard()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint,
                true);

            Size = new Size(250, CompactHeight);
            MinimumSize = new Size(190, 118);
            Cursor = Cursors.Hand;
            TabStop = true;
            ApplyTheme(palette);
        }

        public event EventHandler? DetailsToggled;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EngineeringResultCardModel Model
        {
            get => model;
            set
            {
                model = value ?? throw new ArgumentNullException(nameof(value));
                Height = showDetails
                    ? Math.Max(Height, CalculateExpandedHeight())
                    : CompactHeight;
                Invalidate();
            }
        }

        [DefaultValue(false)]
        public bool ShowDetails
        {
            get => showDetails;
            set
            {
                if (showDetails == value)
                    return;

                showDetails = value;
                Height = showDetails
                    ? Math.Max(Height, CalculateExpandedHeight())
                    : CompactHeight;
                Invalidate();
                DetailsToggled?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.Surface;
            ForeColor = palette.TextPrimary;
            Font = ThemeFonts.Body;
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
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            ShowDetails = !ShowDetails;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                ShowDetails = !ShowDetails;
                e.Handled = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            Color statusColor = ResolveStatusColor(model.Status);
            Color background = isHovered
                ? Blend(palette.SurfaceHover, palette.Surface, 0.32)
                : palette.Surface;

            using SolidBrush backgroundBrush = new SolidBrush(background);
            using Pen borderPen = new Pen(isHovered ? statusColor : palette.Border);
            g.FillRectangle(backgroundBrush, bounds);
            g.DrawRectangle(borderPen, bounds);

            using SolidBrush accentBrush = new SolidBrush(statusColor);
            g.FillRectangle(accentBrush, 0, 0, 4, Height);

            DrawHeader(g, statusColor);
            DrawMainValue(g);
            DrawSummary(g, statusColor);

            if (showDetails)
                DrawDetails(g);
        }

        private void DrawHeader(Graphics g, Color statusColor)
        {
            TextRenderer.DrawText(
                g,
                model.Title,
                ThemeFonts.Caption,
                new Rectangle(14, 10, Width - 112, 18),
                palette.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            string badge = ResolveAiBadgeText(model.AiLevel);
            Rectangle badgeBounds = new Rectangle(Width - 92, 9, 78, 20);
            using SolidBrush badgeBrush = new SolidBrush(Blend(statusColor, palette.Surface, 0.14));
            using Pen badgePen = new Pen(Blend(statusColor, palette.Border, 0.55));
            g.FillRectangle(badgeBrush, badgeBounds);
            g.DrawRectangle(badgePen, badgeBounds);

            TextRenderer.DrawText(
                g,
                badge,
                ThemeFonts.Tiny,
                badgeBounds,
                statusColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void DrawMainValue(Graphics g)
        {
            Rectangle valueBounds = new Rectangle(14, 34, Width - 28, 36);
            TextRenderer.DrawText(
                g,
                model.PrimaryValue,
                ThemeFonts.Title,
                valueBounds,
                palette.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            if (!string.IsNullOrWhiteSpace(model.PrimaryUnit))
            {
                Size valueSize = TextRenderer.MeasureText(model.PrimaryValue, ThemeFonts.Title);
                Rectangle unitBounds = new Rectangle(
                    Math.Min(Width - 84, 14 + valueSize.Width + 4),
                    45,
                    70,
                    18);

                TextRenderer.DrawText(
                    g,
                    model.PrimaryUnit,
                    ThemeFonts.Caption,
                    unitBounds,
                    palette.TextSecondary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private void DrawSummary(Graphics g, Color statusColor)
        {
            string subtitle = model.Subtitle;
            if (!string.IsNullOrWhiteSpace(model.LimitText))
                subtitle = string.IsNullOrWhiteSpace(subtitle)
                    ? model.LimitText
                    : $"{subtitle} | {model.LimitText}";

            TextRenderer.DrawText(
                g,
                subtitle,
                ThemeFonts.Caption,
                new Rectangle(14, 72, Width - 28, 38),
                palette.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);

            string statusText = ResolveStatusText(model.Status);
            TextRenderer.DrawText(
                g,
                statusText,
                ThemeFonts.BodyBold,
                new Rectangle(14, 113, Width - 28, 21),
                statusColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            string detailsHint = showDetails ? "Részletek elrejtése" : "Részletek";
            TextRenderer.DrawText(
                g,
                detailsHint,
                ThemeFonts.Tiny,
                new Rectangle(Width - 90, Height - 22, 76, 16),
                palette.TextDisabled,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void DrawDetails(Graphics g)
        {
            int y = 141;
            Color background = isHovered ? Blend(palette.SurfaceHover, palette.Surface, 0.32) : palette.Surface;
            using (SolidBrush cleanupBrush = new SolidBrush(background))
            {
                g.FillRectangle(cleanupBrush, Width - 118, Height - 25, 112, 21);
            }

            DrawDetailLine(g, "Forrás", model.SourceText, ref y);
            DrawDetailLine(g, "Javaslat", model.RecommendationText, ref y);

            if (model.Diagnostics.Count > 0)
            {
                EngineeringResultDiagnostic diagnostic = model.Diagnostics[0];
                DrawDetailLine(g, "Diagn.", diagnostic.Message, ref y);
            }

            if (model.References.Count > 0)
                DrawDetailLine(g, "Ref.", model.References[0].ToDisplayText(), ref y);

            TextRenderer.DrawText(
                g,
                "Részletek elrejtése",
                ThemeFonts.Tiny,
                new Rectangle(Width - 112, Height - 22, 98, 16),
                palette.TextDisabled,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void DrawDetailLine(Graphics g, string label, string value, ref int y)
        {
            int rowHeight = MeasureDetailHeight(value);

            if (string.IsNullOrWhiteSpace(value) || y + rowHeight > Height - 34)
                return;

            TextRenderer.DrawText(
                g,
                label,
                ThemeFonts.Tiny,
                new Rectangle(14, y, 44, rowHeight),
                palette.TextDisabled,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);

            TextRenderer.DrawText(
                g,
                value,
                ThemeFonts.Tiny,
                new Rectangle(60, y, Width - 74, rowHeight),
                palette.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);

            y += rowHeight + 4;
        }

        private int CalculateExpandedHeight()
        {
            int detailHeight = 0;
            detailHeight += MeasureDetailHeight(model.SourceText);
            detailHeight += MeasureDetailHeight(model.RecommendationText);

            if (model.Diagnostics.Count > 0)
                detailHeight += MeasureDetailHeight(model.Diagnostics[0].Message);

            if (model.References.Count > 0)
                detailHeight += MeasureDetailHeight(model.References[0].ToDisplayText());

            return Math.Max(MinimumExpandedHeight, 151 + detailHeight + 46);
        }

        private int MeasureDetailHeight(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            int textWidth = Math.Max(80, Width - 74);
            Size measured = TextRenderer.MeasureText(
                value,
                ThemeFonts.Tiny,
                new Size(textWidth, int.MaxValue),
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak);

            return Math.Max(17, measured.Height + 2);
        }

        private Color ResolveStatusColor(EngineeringResultStatus status)
        {
            return status switch
            {
                EngineeringResultStatus.Info => palette.Info,
                EngineeringResultStatus.Success => palette.Success,
                EngineeringResultStatus.Warning => palette.Warning,
                EngineeringResultStatus.Danger => palette.Danger,
                _ => palette.TextSecondary
            };
        }

        private static string ResolveStatusText(EngineeringResultStatus status)
        {
            return status switch
            {
                EngineeringResultStatus.Info => "Információ",
                EngineeringResultStatus.Success => "Megfelelő",
                EngineeringResultStatus.Warning => "Figyelés",
                EngineeringResultStatus.Danger => "Kritikus",
                _ => "Semleges"
            };
        }

        private static string ResolveAiBadgeText(EngineeringAiSupportLevel level)
        {
            return level switch
            {
                EngineeringAiSupportLevel.RuleCheck => "AI L1",
                EngineeringAiSupportLevel.Recommendation => "AI L2",
                EngineeringAiSupportLevel.Assistant => "AI L3",
                _ => "Kézi"
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
    }
}
