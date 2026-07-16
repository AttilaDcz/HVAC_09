using System;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Dialogs;
using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Notifications;
using HVACDesigner.CoreUI.Status;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.Services;

namespace HVACDesigner.CoreUI.Layout
{
    public class StatusHost : HostBase, IThemeable
    {
        private const int WM_NCHITTEST = 0x0084;
        private const int HTBOTTOMRIGHT = 17;
        private const int GripHitBoxSize = 16;
        private const int UnitWidgetRightPadding = 30;

        private ThemePalette palette = ThemeManager.CurrentPalette;
        private EngineeringStatusState state = EngineeringStatusMessages.Current;
        private Rectangle unitRect;
        private bool isUnitHovered;

        public StatusHost() : base("StatusHostZone")
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            ApplyTheme(palette);

            MouseClick += StatusHost_MouseClick;
            MouseMove += StatusHost_MouseMove;
            MouseLeave += StatusHost_MouseLeave;

            EngineeringStatusMessages.StatusChanged += EngineeringStatusMessages_StatusChanged;
            UnitContext.UnitChanged += UnitContext_UnitChanged;
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;

            RefreshUnitSummary();
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.SurfaceAlt;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                EngineeringStatusMessages.StatusChanged -= EngineeringStatusMessages_StatusChanged;
                UnitContext.UnitChanged -= UnitContext_UnitChanged;
                ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            }

            base.Dispose(disposing);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg != WM_NCHITTEST)
                return;

            int x = m.LParam.ToInt32() & 0xFFFF;
            int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
            Point clientPoint = PointToClient(new Point(x, y));

            if (clientPoint.X >= Width - GripHitBoxSize &&
                clientPoint.Y >= Height - GripHitBoxSize)
            {
                m.Result = (IntPtr)HTBOTTOMRIGHT;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using Pen borderPen = new Pen(palette.Border);
            g.DrawLine(borderPen, 0, 0, Width, 0);

            DrawLeftContext(g);
            DrawCenterMessage(g);
            DrawUnitWidget(g);
            DrawResizeGrip(g);
        }

        private void DrawLeftContext(Graphics g)
        {
            string project = ResolveProjectName();
            string module = state.ModuleName;
            string text = $"Projekt: {project}  |  Modul: {module}";

            Rectangle bounds = new Rectangle(14, 2, Math.Max(80, Width / 3), Height - 4);
            TextRenderer.DrawText(
                g,
                text,
                ThemeFonts.Caption,
                bounds,
                palette.TextSecondary,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);
        }

        private void DrawCenterMessage(Graphics g)
        {
            Color severityColor = ResolveSeverityColor(state.Severity);
            int left = Math.Max(220, Width / 3);
            int right = Math.Max(left + 120, unitRect.Left - 10);
            Rectangle accentBounds = new Rectangle(left, Math.Max(7, (Height - 8) / 2), 4, 8);
            Rectangle textBounds = new Rectangle(left + 10, 2, Math.Max(80, right - left - 10), Height - 4);

            using SolidBrush accentBrush = new SolidBrush(severityColor);
            g.FillRectangle(accentBrush, accentBounds);

            TextRenderer.DrawText(
                g,
                state.Message,
                ThemeFonts.Caption,
                textBounds,
                state.Severity == EngineeringStatusSeverity.Neutral
                    ? palette.TextSecondary
                    : palette.TextPrimary,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);
        }

        private void DrawUnitWidget(Graphics g)
        {
            string unitText = string.IsNullOrWhiteSpace(state.UnitSummary)
                ? BuildUnitSummary()
                : state.UnitSummary;

            string text = " " + unitText + " ";
            Size textSize = TextRenderer.MeasureText(text, ThemeFonts.Caption, Size.Empty, TextFormatFlags.NoPadding);
            int width = Math.Min(Math.Max(120, textSize.Width + 18), 260);
            int height = Math.Min(20, Height - 5);

            unitRect = new Rectangle(
                Width - UnitWidgetRightPadding - width,
                Math.Max(3, (Height - height) / 2),
                width,
                height);

            Color background = isUnitHovered ? palette.SurfaceHover : palette.Surface;
            Color border = isUnitHovered ? palette.BorderStrong : palette.Border;
            Color textColor = isUnitHovered ? palette.TextPrimary : palette.TextSecondary;

            using SolidBrush backgroundBrush = new SolidBrush(background);
            using Pen borderPen = new Pen(border);
            g.FillRectangle(backgroundBrush, unitRect);
            g.DrawRectangle(borderPen, unitRect.Left, unitRect.Top, unitRect.Width - 1, unitRect.Height - 1);

            TextRenderer.DrawText(
                g,
                text,
                ThemeFonts.Caption,
                unitRect,
                textColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);
        }

        private void DrawResizeGrip(Graphics g)
        {
            using SolidBrush gripBrush = new SolidBrush(palette.TextDisabled);
            int startX = Width - 14;
            int startY = Height - 14;
            g.FillRectangle(gripBrush, startX + 8, startY + 8, 2, 2);
            g.FillRectangle(gripBrush, startX + 4, startY + 8, 2, 2);
            g.FillRectangle(gripBrush, startX, startY + 8, 2, 2);
            g.FillRectangle(gripBrush, startX + 8, startY + 4, 2, 2);
            g.FillRectangle(gripBrush, startX + 4, startY + 4, 2, 2);
            g.FillRectangle(gripBrush, startX + 8, startY, 2, 2);
        }

        private void StatusHost_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || !unitRect.Contains(e.Location))
                return;

            using EngineeringDialog dialog = new EngineeringDialog
            {
                DialogTitle = "Mértékegységek",
                DialogSubtitle = "Szakági megjelenítési és beviteli preferenciák.",
                IconKind = HvacIconKind.Settings,
                Severity = EngineeringDialogSeverity.Info,
                ButtonSet = EngineeringDialogButtonSet.SaveCancel,
                DialogSize = EngineeringDialogSize.Custom,
                ClientSize = new Size(770, 520),
                ContentPadding = new Padding(20, 16, 20, 16)
            };

            EngineeringUnitSelector selector = new EngineeringUnitSelector
            {
                Dock = DockStyle.Fill
            };
            selector.ApplyTheme(palette);
            dialog.SetContent(selector);

            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                selector.ApplyChanges();
                EngineeringNotificationService.Success(
                    "Mértékegységek frissítve",
                    "A megjelenítési preferenciák érvénybe léptek.");
                RefreshUnitSummary();
            }
        }

        private void StatusHost_MouseMove(object? sender, MouseEventArgs e)
        {
            bool hovered = unitRect.Contains(e.Location);
            if (hovered == isUnitHovered)
                return;

            isUnitHovered = hovered;
            Cursor = hovered ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        private void StatusHost_MouseLeave(object? sender, EventArgs e)
        {
            isUnitHovered = false;
            Cursor = Cursors.Default;
            Invalidate();
        }

        private void EngineeringStatusMessages_StatusChanged(object? sender, EngineeringStatusState e)
        {
            string previousModule = state.ModuleName;
            state = e;

            if (!string.Equals(previousModule, state.ModuleName, StringComparison.OrdinalIgnoreCase))
            {
                RefreshUnitSummary();
                return;
            }

            Invalidate();
        }

        private void UnitContext_UnitChanged(object? sender, EventArgs e)
        {
            RefreshUnitSummary();
        }

        private void ThemeManager_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            ApplyTheme(e.Palette);
        }

        private void RefreshUnitSummary()
        {
            EngineeringStatusMessages.SetUnitSummary(BuildUnitSummary());
        }

        private string BuildUnitSummary()
        {
            string module = state.ModuleName;

            if (module.Contains("Lég", StringComparison.OrdinalIgnoreCase) ||
                module.Contains("Füst", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeUnitSummary($"SI: {UnitContext.Air.GetFlowUnitLabel()}, {UnitContext.Air.GetPressureUnitLabel()}, {UnitContext.Air.GetDimensionUnitLabel()}");
            }

            if (module.Contains("Hidraul", StringComparison.OrdinalIgnoreCase) ||
                module.Contains("Cső", StringComparison.OrdinalIgnoreCase) ||
                module.Contains("Szivattyú", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeUnitSummary($"SI: {UnitContext.Hydraulics.GetFlowUnitLabel()}, {UnitContext.Hydraulics.GetPressureUnitLabel()}, {UnitContext.Hydraulics.GetDimensionUnitLabel()}");
            }

            if (module.Contains("Víz", StringComparison.OrdinalIgnoreCase) ||
                module.Contains("Sanitary", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeUnitSummary($"SI: {UnitContext.Sanitary.GetFlowUnitLabel()}, {UnitContext.Sanitary.GetPressureUnitLabel()}, {UnitContext.Sanitary.GetDimensionUnitLabel()}");
            }

            if (module.Contains("U-", StringComparison.OrdinalIgnoreCase) ||
                module.Contains("Energet", StringComparison.OrdinalIgnoreCase) ||
                module.Contains("Hő", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeUnitSummary($"SI: {UnitContext.Energetics.GetUValueUnitLabel()}, {UnitContext.Energetics.GetThicknessUnitLabel()}, {UnitContext.Energetics.GetAreaUnitLabel()}");
            }

            return NormalizeUnitSummary($"SI: {UnitContext.General.GetLengthUnitLabel()}, {UnitContext.General.GetAreaUnitLabel()}, {UnitContext.General.GetPowerUnitLabel()}");
        }

        private static string NormalizeUnitSummary(string text)
        {
            return (text ?? string.Empty)
                .Replace("mÂł", "m³")
                .Replace("mÂ˛", "m²")
                .Replace("cmÂ˛", "cm²")
                .Replace("mmÂ˛", "mm²")
                .Replace("Â°C", "°C")
                .Replace("Â°F", "°F")
                .Replace("Â·", "·")
                .Replace("â‚‚", "₂");
        }

        private string ResolveProjectName()
        {
            if (!string.IsNullOrWhiteSpace(state.ProjectName))
                return state.ProjectName;

            try
            {
                return ServiceLocator.Project.IsProjectLoaded
                    ? ServiceLocator.Project.CurrentFileName
                    : "Nincs projekt";
            }
            catch
            {
                return "Nincs projekt";
            }
        }

        private Color ResolveSeverityColor(EngineeringStatusSeverity severity)
        {
            return severity switch
            {
                EngineeringStatusSeverity.Info => palette.Info,
                EngineeringStatusSeverity.Success => palette.Success,
                EngineeringStatusSeverity.Warning => palette.Warning,
                EngineeringStatusSeverity.Danger => palette.Danger,
                _ => palette.BorderStrong
            };
        }
    }
}
