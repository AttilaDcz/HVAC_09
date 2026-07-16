using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Theme;
using System.ComponentModel;

namespace HVACDesigner.CoreUI.Components.Structural
{
    public class MenuSectionHeader : Control, IThemeable
    {
        private string _icon = string.Empty;
        private HvacIconKind? _iconKind = HvacIconKind.BuildingEnergy;
        private string _title = "SZAKÁG";
        private Color _accentColor = Color.FromArgb(0, 122, 204); // Alapértelmezett mérnöki kék

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        [Category("HVAC Properties")]
        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value ?? string.Empty;
                _iconKind = null;
                InvalidatedUpdate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        [Category("HVAC Properties")]
        public HvacIconKind? IconKind
        {
            get => _iconKind;
            set { _iconKind = value; InvalidatedUpdate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        [Category("HVAC Properties")]
        public string Title
        {
            get => _title;
            set { _title = value; InvalidatedUpdate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        [Category("HVAC Properties")]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; InvalidatedUpdate(); }
        }

        public MenuSectionHeader()
        {
            // Optimalizált kettős pufferelés a villódzás teljes kiküszöbölésére
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            // Biztosítjuk az intelligens ThemeMetrics szerinti alapértelmezett magasságot
            this.Height = (int)(42 * (ThemeMetrics.ButtonHeight / 28.0));
            this.Dock = DockStyle.Top;
        }

        public void ApplyTheme(ThemePalette palette)
        {
            // Lekérjük a központi paletta színeit
            this.BackColor = palette.SurfaceAlt; // Kicsit kiemelkedő sötét szürke csempealap
            this.ForeColor = palette.TextPrimary;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 1. Háttér kitöltése
            using (SolidBrush bgBrush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            ThemePalette currentPalette = ThemeManager.CurrentPalette;

            // 2. AZ ARCULATI EXTRA: Bal oldali függőleges hangsúlyos dizájn-csík kirajzolása
            int stripWidth = 4;
            using (SolidBrush accentBrush = new SolidBrush(_accentColor))
            {
                g.FillRectangle(accentBrush, 0, 0, stripWidth, this.Height);
            }

            // 3. AZ ARCULATI EXTRA 2: Halvány alsó elválasztó él (Mérnöki struktúra)
            using (Pen borderPen = new Pen(currentPalette.Border, 1))
            {
                g.DrawLine(borderPen, 0, this.Height - 1, this.Width, this.Height - 1);
            }

            // Mérések a szövegelrendezéshez a ThemeMetrics szerint
            int margin = ThemeMetrics.MarginNormal;
            int currentLeft = stripWidth + margin;

            if (_iconKind.HasValue)
            {
                int vectorSize = ThemeMetrics.IconSizeNormal;
                using Bitmap icon = HvacIconRenderer.Render(
                    _iconKind.Value,
                    ThemeManager.CurrentThemeMode,
                    vectorSize,
                    _accentColor);

                int iconTop = (Height - vectorSize) / 2;
                g.DrawImage(icon, currentLeft, iconTop, vectorSize, vectorSize);
                currentLeft += vectorSize + margin;
            }
            else if (!string.IsNullOrWhiteSpace(_icon))
            {
                Size iconSize = TextRenderer.MeasureText(
                    g,
                    _icon,
                    ThemeFonts.Section,
                    new Size(int.MaxValue, Height),
                    TextFormatFlags.NoPadding);

                TextRenderer.DrawText(
                    g,
                    _icon,
                    ThemeFonts.Section,
                    new Rectangle(currentLeft, 0, iconSize.Width, Height),
                    ForeColor,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.PreserveGraphicsClipping);

                currentLeft += iconSize.Width + margin;
            }

            TextRenderer.DrawText(
                g,
                _title,
                ThemeFonts.Section,
                new Rectangle(
                    currentLeft,
                    0,
                    Math.Max(0, Width - currentLeft - margin),
                    Height),
                ForeColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding |
                TextFormatFlags.PreserveGraphicsClipping);
        }

        private void InvalidatedUpdate()
        {
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
