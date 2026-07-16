using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Structural
{
    public class HVACSectionPanel : Panel, IThemeable
    {
        private string _sectionTitle = "Szekció";
        private ThemePalette _currentPalette = null!;

        [Category("Appearance")]
        [DefaultValue("Szekció")]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Visible)]
        public string SectionTitle
        {
            get => _sectionTitle;
            set
            {
                _sectionTitle = value;
                Invalidate();
            }
        }

        public HVACSectionPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            Padding = new Padding(15, 50, 15, 15);
            Size = new Size(250, 150);

            ApplyTheme(ThemeManager.CurrentPalette);
        }

        public void ApplyTheme(ThemePalette palette)
        {
            _currentPalette =
                palette ?? throw new ArgumentNullException(nameof(palette));

            BackColor = palette.Surface;
            ForeColor = palette.TextPrimary;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;

            graphics.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.TextRenderingHint =
                System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var backgroundBrush = new SolidBrush(BackColor))
            {
                graphics.FillRectangle(
                    backgroundBrush,
                    ClientRectangle);
            }

            TextRenderer.DrawText(
                graphics,
                _sectionTitle,
                ThemeFonts.Section,
                new Rectangle(12, 10, Math.Max(0, Width - 24), 22),
                ForeColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding |
                TextFormatFlags.PreserveGraphicsClipping);

            using (var linePen = new Pen(_currentPalette.Border, 1))
            {
                graphics.DrawLine(
                    linePen,
                    12,
                    36,
                    Width - 12,
                    36);
            }

            using (var borderPen = new Pen(_currentPalette.Border, 1))
            {
                graphics.DrawRectangle(
                    borderPen,
                    0,
                    0,
                    Width - 1,
                    Height - 1);
            }
        }
    }
}
