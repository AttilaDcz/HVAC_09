using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Structural
{
    public class MenuSubButton : Control, IThemeable
    {
        private bool _isHovered = false;
        private bool _isActive = false;
        private string _moduleName = "Modul neve";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        [Category("HVAC Properties")]
        public string ModuleName
        {
            get => _moduleName;
            set { _moduleName = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        [Category("HVAC Properties")]
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; Invalidate(); }
        }

        public MenuSubButton()
        {
            // Maximális pufferelés a villódzásmentes hover-effektushoz
            SetStyle(ControlStyles.AllPaintingInWmPaint |
             ControlStyles.UserPaint |
             ControlStyles.OptimizedDoubleBuffer |
             ControlStyles.ResizeRedraw |
             ControlStyles.SupportsTransparentBackColor, true);

            // A magasság igazodik a globális mérnöki TextBox/ComboBox magassághoz + kis puffer
            this.Height = (int)(ThemeMetrics.TextBoxHeight * 1.25);
            this.Dock = DockStyle.Top;
            this.Cursor = Cursors.Hand;
        }

        public void ApplyTheme(ThemePalette palette)
        {
            // Alapértelmezett háttér teljesen transzparens, beolvad a menüsávba
            this.BackColor = palette.Window;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate(); // Újrarajzolás a hover háttérfény miatt
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            ThemePalette currentPalette = ThemeManager.CurrentPalette;

            // 1. Háttérállapotok kezelése
            if (_isActive)
            {
                // Kijelölt/Aktív állapot: megkapja a stabil sötét Surface színt
                using (SolidBrush activeBrush = new SolidBrush(currentPalette.Surface))
                {
                    g.FillRectangle(activeBrush, this.ClientRectangle);
                }
            }
            else if (_isHovered)
            {
                // Föléhúzás (Hover) állapot: finom, áttetsző felderengés
                using (SolidBrush hoverBrush = new SolidBrush(currentPalette.SurfaceHover))
                {
                    g.FillRectangle(hoverBrush, this.ClientRectangle);
                }
            }

            // 2. Szöveg kirajzolása (ThemeMetrics és ThemeFonts alapján)
            Font textFont = ThemeFonts.Body;

            // Szövegszín meghatározása a hierarchia szerint
            Color textColor = currentPalette.TextSecondary; // Alapból lágy szürke
            if (_isActive) textColor = currentPalette.TextPrimary; // Aktívként tiszta fehér
            else if (_isHovered) textColor = currentPalette.TextPrimary;

            // Bal oldali behúzás (Margin), hogy szépen a szakág csempe szövege alá igazodjon
            int leftIndent = ThemeMetrics.MarginLarge * 2;

            TextRenderer.DrawText(
                g,
                _moduleName,
                textFont,
                new Rectangle(
                    leftIndent,
                    0,
                    Math.Max(0, Width - leftIndent - ThemeMetrics.MarginNormal),
                    Height),
                textColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding |
                TextFormatFlags.PreserveGraphicsClipping);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
