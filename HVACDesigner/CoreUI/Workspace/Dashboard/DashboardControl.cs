using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Workspace.Dashboard
{
    public class DashboardControl : UserControl, IThemeable
    {
        private ThemePalette _currentPalette = null!;

        public event Action<string>? ModuleClicked;

        public DashboardControl()
        {
            InitializeComponent();
            ApplyTheme(ThemeManager.CurrentPalette);
        }

        public void ApplyTheme(ThemePalette palette)
        {
            _currentPalette =
                palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = ThemeManager.CurrentPalette.Window;
            //BackColor = palette.Window;
            ForeColor = palette.TextPrimary;

            Invalidate(true);
        }

        private void InitializeComponent()
        {
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;
            Padding = new Padding(30);

            string version = "v1.0.0";

            try
            {
                Version? assemblyVersion =
                    Assembly.GetExecutingAssembly().GetName().Version;

                if (assemblyVersion != null)
                {
                    version = "v" + assemblyVersion.ToString(3);
                }
            }
            catch
            {
                // A verziószám csak megjelenítési információ.
                // Sikertelen kiolvasáskor az alapérték marad.
            }

            var mainLayout = new TransparentTableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6
            };

            // Feliratkozás a közvetlen, pixelpontos cellarajzolásra
            mainLayout.CellPaint += MainLayout_CellPaint;

            mainLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55F));

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 10F));

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 160F));

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 40F));

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 10F));

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            //--------------------------------------------------
            // Címsor
            //--------------------------------------------------

            var titlePanel = new TransparentPanel
            {
                Dock = DockStyle.Fill
            };

            var lblTitle = new Label
            {
                Text = $"🏢 HVAC DESIGNER  {version}",
                Font = new Font(
                    "Segoe UI Semibold",
                    18F,
                    FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };

            var lblSubtitle = new Label
            {
                Text =
                    "Üdvözlöm a rendszerben! " +
                    "Válasszon az alábbi modulok közül.  |  " +
                    DateTime.Now.ToString("yyyy. MM. dd. dddd"),
                Font = new Font(
                    "Segoe UI",
                    9.5F,
                    FontStyle.Regular),
                AutoSize = true,
                Location = new Point(3, 33),
                BackColor = Color.Transparent,
                Tag = "Secondary"
            };

            titlePanel.Controls.Add(lblTitle);
            titlePanel.Controls.Add(lblSubtitle);

            mainLayout.Controls.Add(titlePanel, 0, 0);

            //--------------------------------------------------
            // Modul-kártyák
            //--------------------------------------------------

            var cardsLayout = new TransparentTableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 3
            };

            cardsLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            cardsLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 33.33F));

            cardsLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 33.33F));

            cardsLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 33.33F));

            cardsLayout.Controls.Add(
                CreateModuleCard(
                    "🌍 Energetika",
                    "U-érték, szigetelés és globális\n" +
                    "teljesítményigények meghatározása."),
                0,
                0);

            cardsLayout.Controls.Add(
                CreateModuleCard(
                    "🔥 Fűtés & Hűtés",
                    "Csőhálózati nyomásesések számítása\n" +
                    "és szivattyú tömegáram választás."),
                1,
                0);

            cardsLayout.Controls.Add(
                CreateModuleCard(
                    "🌀 Légtechnika",
                    "Légsebesség, csatorna méretezés\n" +
                    "és komplex hálózati hidraulika."),
                2,
                0);

            mainLayout.Controls.Add(cardsLayout, 0, 2);

            //--------------------------------------------------
            // Legutóbbi projektek címsor
            //--------------------------------------------------

            var recentTitlePanel = new TransparentPanel
            {
                Dock = DockStyle.Fill
            };

            var lblRecentTitle = new Label
            {
                Text = "⏳ LEGUTÓBBI PROJEKTEK",
                Font = new Font(
                    "Segoe UI Semibold",
                    11F,
                    FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 15),
                BackColor = Color.Transparent
            };

            recentTitlePanel.Controls.Add(lblRecentTitle);

            mainLayout.Controls.Add(recentTitlePanel, 0, 3);

            //--------------------------------------------------
            // Legutóbbi projektek listája
            //--------------------------------------------------

            var recentFilesPanel =
                new TransparentFlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoScroll = false,
                    Padding = new Padding(0, 10, 0, 0)
                };

            recentFilesPanel.Controls.Add(
                CreateRecentFileRow(
                    "C:/Tervek/2026/" +
                    "CsaladiHaz_Energetika.hvac",
                    "Szakág: Épületenergetika  |  " +
                    "Módosítva: Ma, 18:24"));

            recentFilesPanel.Controls.Add(
                CreateRecentFileRow(
                    "D:/BME_Beadando/" +
                    "Legtechnika_Torony_B.hvac",
                    "Szakág: Légtechnika (24 szakasz)  |  " +
                    "Módosítva: Tegnap, 14:10"));

            recentFilesPanel.Controls.Add(
                CreateRecentFileRow(
                    "C:/Munkak/Kozmu/" +
                    "Csatorna_Hosszszelveny_V2.hvac",
                    "Szakág: Víz-Csatorna (145 m)  |  " +
                    "Módosítva: 2026. 07. 05."));

            mainLayout.Controls.Add(recentFilesPanel, 0, 5);

            Controls.Add(mainLayout);
        }

        private void MainLayout_CellPaint(object? sender, TableLayoutCellPaintEventArgs e)
        {
            // Csak az 1. (címsor alatti) és a 4. (projektek feletti) sorban rajzolunk elválasztót
            if (e.Row != 1 && e.Row != 4)
                return;

            if (_currentPalette == null)
                return;

            var g = e.Graphics;
            var r = e.CellBounds;

            // Kikapcsoljuk az élsimítást a tökéletes, egész pixelre illeszkedő 1px vastag vonalhoz
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.None;

            // A vonalat a cellasor függőleges közepére igazítjuk
            int y = r.Top + (r.Height / 2);

            using (var brush = new LinearGradientBrush(
                new Point(r.Left, y),
                new Point(r.Right, y),
                Color.Transparent,
                _currentPalette.Border))
            {
                brush.InterpolationColors = new ColorBlend
                {
                    Colors = new[]
                    {
                        Color.Transparent,
                        _currentPalette.Border,
                        _currentPalette.Border,
                        Color.Transparent
                    },
                    Positions = new[]
                    {
                        0.0F,
                        0.05F,
                        0.95F,
                        1.0F
                    }
                };

                using (var pen = new Pen(brush, 1F))
                {
                    g.DrawLine(pen, r.Left, y, r.Right, y);
                }
            }
        }

        private Control CreateModuleCard(
            string title,
            string description)
        {
            return new ModuleCard(
                title,
                description,
                () => ModuleClicked?.Invoke(title));
        }

        private static Control CreateRecentFileRow(
            string filePath,
            string details)
        {
            return new RecentFileRow(filePath, details);
        }

        private sealed class TransparentPanel :
            Panel,
            IThemeable
        {
            public TransparentPanel()
            {
                BackColor = Color.Transparent;
            }

            public void ApplyTheme(ThemePalette palette)
            {
                if (palette == null)
                    throw new ArgumentNullException(nameof(palette));

                BackColor = Color.Transparent;
                Invalidate();
            }
        }

        private sealed class TransparentTableLayoutPanel :
            TableLayoutPanel,
            IThemeable
        {
            public TransparentTableLayoutPanel()
            {
                BackColor = Color.Transparent;
            }

            public void ApplyTheme(ThemePalette palette)
            {
                if (palette == null)
                    throw new ArgumentNullException(nameof(palette));

                BackColor = Color.Transparent;
                Invalidate();
            }
        }

        private sealed class TransparentFlowLayoutPanel :
            FlowLayoutPanel,
            IThemeable
        {
            public TransparentFlowLayoutPanel()
            {
                BackColor = Color.Transparent;
            }

            public void ApplyTheme(ThemePalette palette)
            {
                if (palette == null)
                    throw new ArgumentNullException(nameof(palette));

                BackColor = Color.Transparent;
                Invalidate();
            }
        }

        private sealed class ModuleCard :
            Panel,
            IThemeable
        {
            private readonly Label _titleLabel;
            private readonly Label _descriptionLabel;
            private readonly Action _clickAction;

            private ThemePalette _palette = null!;
            private bool _isHovered;

            public ModuleCard(
                string title,
                string description,
                Action clickAction)
            {
                _clickAction =
                    clickAction ??
                    throw new ArgumentNullException(
                        nameof(clickAction));

                Dock = DockStyle.Fill;
                Margin = new Padding(0, 10, 15, 10);
                Cursor = Cursors.Hand;

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw,
                    true);

                _titleLabel = new Label
                {
                    Text = title,
                    Font = new Font(
                        "Segoe UI Semibold",
                        11.5F,
                        FontStyle.Bold),
                    Location = new Point(15, 15),
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };

                _descriptionLabel = new Label
                {
                    Text = description,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Regular),
                    Location = new Point(15, 42),
                    Size = new Size(260, 60),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand,
                    Tag = "Secondary"
                };

                Controls.Add(_titleLabel);
                Controls.Add(_descriptionLabel);

                Click += HandleClick;
                _titleLabel.Click += HandleClick;
                _descriptionLabel.Click += HandleClick;

                MouseEnter += HandleMouseEnter;
                MouseLeave += HandleMouseLeave;

                _titleLabel.MouseEnter += HandleMouseEnter;
                _titleLabel.MouseLeave += HandleMouseLeave;

                _descriptionLabel.MouseEnter += HandleMouseEnter;
                _descriptionLabel.MouseLeave += HandleMouseLeave;

                ApplyTheme(ThemeManager.CurrentPalette);
            }

            public void ApplyTheme(ThemePalette palette)
            {
                _palette =
                    palette ??
                    throw new ArgumentNullException(nameof(palette));

                _titleLabel.ForeColor = palette.TextPrimary;
                _descriptionLabel.ForeColor =
                    palette.TextSecondary;

                UpdateBackground();
                Invalidate(true);
            }

            private void HandleClick(object? sender, EventArgs e)
            {
                _clickAction();
            }

            private void HandleMouseEnter(
                object? sender,
                EventArgs e)
            {
                _isHovered = true;
                UpdateBackground();
            }

            private void HandleMouseLeave(
                object? sender,
                EventArgs e)
            {
                Point mousePosition =
                    PointToClient(Cursor.Position);

                if (ClientRectangle.Contains(mousePosition))
                    return;

                _isHovered = false;
                UpdateBackground();
            }

            private void UpdateBackground()
            {
                BackColor = _isHovered
                    ? _palette.SurfaceHover
                    : _palette.SurfaceAlt;

                Invalidate();
            }
        }

        private sealed class RecentFileRow :
            Panel,
            IThemeable
        {
            private readonly Label _pathLabel;
            private readonly Label _detailsLabel;

            private ThemePalette _palette = null!;
            private bool _isHovered;

            public RecentFileRow(
                string filePath,
                string details)
            {
                Size = new Size(1100, 50);
                Margin = new Padding(0, 0, 0, 2);
                Cursor = Cursors.Hand;

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw,
                    true);

                _pathLabel = new Label
                {
                    Text = $"📂  {filePath}",
                    Font = new Font(
                        "Segoe UI",
                        10F,
                        FontStyle.Regular),
                    Location = new Point(5, 4),
                    Width = 850,
                    AutoEllipsis = true,
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };

                _detailsLabel = new Label
                {
                    Text = details,
                    Font = new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Regular),
                    Location = new Point(32, 25),
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand,
                    Tag = "Secondary"
                };

                Controls.Add(_pathLabel);
                Controls.Add(_detailsLabel);

                MouseEnter += HandleMouseEnter;
                MouseLeave += HandleMouseLeave;

                _pathLabel.MouseEnter += HandleMouseEnter;
                _pathLabel.MouseLeave += HandleMouseLeave;

                _detailsLabel.MouseEnter += HandleMouseEnter;
                _detailsLabel.MouseLeave += HandleMouseLeave;

                ApplyTheme(ThemeManager.CurrentPalette);
            }

            public void ApplyTheme(ThemePalette palette)
            {
                _palette =
                    palette ??
                    throw new ArgumentNullException(nameof(palette));

                _pathLabel.ForeColor = palette.TextPrimary;
                _detailsLabel.ForeColor =
                    palette.TextSecondary;

                UpdateBackground();
                Invalidate(true);
            }

            private void HandleMouseEnter(
                object? sender,
                EventArgs e)
            {
                _isHovered = true;
                UpdateBackground();
            }

            private void HandleMouseLeave(
                object? sender,
                EventArgs e)
            {
                Point mousePosition =
                    PointToClient(Cursor.Position);

                if (ClientRectangle.Contains(mousePosition))
                    return;

                _isHovered = false;
                UpdateBackground();
            }

            private void UpdateBackground()
            {
                BackColor = _isHovered
                    ? _palette.SurfaceHover
                    : Color.Transparent;

                Invalidate();
            }
        }
    }
}
