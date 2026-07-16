using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Structural;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Services;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.Services;

namespace HVACDesigner.CoreUI.Layout
{
    // A bal oldali navigációs sáv fizikai konténere
    public class NavigationHost : HostBase, IThemeable
    {
        private const int SectionAccentWidth = 3;
        private const int SectionIconLeft = 10;
        private const int SectionIconTextGap = 12;
        private const int SectionArrowWidth = 28;

        private HVACScrollableContainer _scrollContainer;
        private Button? _activeButton;
        private List<NavigationGroup> _menuItems = new List<NavigationGroup>();
        private readonly Dictionary<string, Button> _buttonsByModule =
            new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Color> _accentByModule =
            new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);

        public NavigationHost() : base("NavigationHostZone")
        {
            this.Width = LayoutMetrics.NavigationExpandedWidth;
            this.BackColor = ThemeManager.CurrentPalette.SurfaceAlt;

            _scrollContainer = new HVACScrollableContainer
            {
                Dock = DockStyle.Fill,
                Width = this.Width,
                Height = this.Height
            };
            this.Controls.Add(_scrollContainer);

            this.Resize += NavigationHost_Resize;
            ServiceLocator.Navigation.NavigationRequested += OnNavigationRequested;

            GenerateLogicalTree();
            BuildMenuUI();
        }

        public void ApplyTheme(ThemePalette palette)
        {
            BackColor = palette.SurfaceAlt;

            if (_scrollContainer != null)
            {
                _scrollContainer.ApplyTheme(palette);
            }

            BuildMenuUI();
            Invalidate(true);
        }

        private void NavigationHost_Resize(object? sender, EventArgs e)
        {
            if (_scrollContainer != null)
            {
                _scrollContainer.Size = this.Size;
                _scrollContainer.RecalculateContentLayout(preserveScrollRatio: true);
            }
        }

        // Összerakjuk a tiszta logikai fát, immár szakág-specifikus színekkel
        private void GenerateLogicalTree()
        {
            _menuItems.Clear();

            // =========================================================================
            // 1. ÉPÜLET ÉS HŐTECHNIKA -> Meleg vörös/narancs (Thermal)
            // =========================================================================
            var groupThermal = new NavigationGroup("BuildingThermal", "Épület és Hőtechnika", HvacIconKind.BuildingEnergy, Color.FromArgb(230, 90, 75));
            groupThermal.AddChild(new NavigationItem(ModuleKeys.BuildingUValue, "Épületszerkezetek (U)"));
            groupThermal.AddChild(new NavigationItem(ModuleKeys.BuildingInventory, "Épület leltár (Zónák)"));
            groupThermal.AddChild(new NavigationItem(ModuleKeys.BuildingHeatLoad, "Hőszükséglet (EN 12831)"));
            groupThermal.AddChild(new NavigationItem(ModuleKeys.BuildingCertification, "Tanúsítás (9/2023. ÉKM)"));
            _menuItems.Add(groupThermal);

            // =========================================================================
            // 2. FŰTÉS ÉS HŰTÉS -> Klasszikus hidraulika kék (Hydro)
            // =========================================================================
            var groupHydro = new NavigationGroup("ThermalHydraulics", "Fűtés és Hűtés", HvacIconKind.HeatingCooling, Color.FromArgb(52, 152, 219));
            groupHydro.AddChild(new NavigationItem(ModuleKeys.HeatingPipePressureDrop, "Hidraulikai méretezés"));
            groupHydro.AddChild(new NavigationItem(ModuleKeys.HeatingPumpSelection, "Szivattyú munkapont"));
            _menuItems.Add(groupHydro);

            // =========================================================================
            // 3. VÍZ-CSATORNA -> Elegáns türkiz (Sanitary)
            // =========================================================================
            var groupSanitary = new NavigationGroup("Sanitary", "Víz-Csatorna", HvacIconKind.SanitaryWater, Color.FromArgb(26, 188, 156));
            groupSanitary.AddChild(new NavigationItem(ModuleKeys.WaterPeakDemand, "Mértékadó vízigény (LU)"));
            groupSanitary.AddChild(new NavigationItem(ModuleKeys.WaterDailyDemand, "Napi vízigény számítás"));
            groupSanitary.AddChild(new NavigationItem(ModuleKeys.WaterDrainagePeak, "Szennyvíz lefolyók"));
            groupSanitary.AddChild(new NavigationItem(ModuleKeys.WaterDhwCirculation, "HMV cirkulációs körök"));
            groupSanitary.AddChild(new NavigationItem(ModuleKeys.WaterLongProfile, "Csatorna hosszszelvény"));
            _menuItems.Add(groupSanitary);

            // =========================================================================
            // 4. LÉGTECHNIKA -> Mérnöki mentazöld (Air)
            // =========================================================================
            var groupAir = new NavigationGroup("Air", "Légtechnika", HvacIconKind.Air, Color.FromArgb(46, 204, 113));
            groupAir.AddChild(new NavigationItem(ModuleKeys.AirVelocity, "Légsebesség számítás"));
            groupAir.AddChild(new NavigationItem(ModuleKeys.AirDuctSizing, "Csatorna méretező"));
            groupAir.AddChild(new NavigationItem(ModuleKeys.AirDuctNetwork, "Hálózati veszteségek"));
            _menuItems.Add(groupAir);

            // =========================================================================
            // 5. FÜSTGÁZELLÁTÁS -> Figyelmeztető gáz-sárga (FlueGas)
            // =========================================================================
            var groupGas = new NavigationGroup("FlueGas", "Füstgázellátás", HvacIconKind.FlueGas, Color.FromArgb(230, 126, 34));
            groupGas.AddChild(new NavigationItem(ModuleKeys.FlueGas, "Égéstermék elvezetés"));
            _menuItems.Add(groupGas);

            // =========================================================================
            // 6. ADATOK ÉS SZABVÁNYOK -> EngineeringData / XML diagnosztika
            // =========================================================================
            var groupData = new NavigationGroup("EngineeringData", "Adatok és szabványok", HvacIconKind.Info, Color.FromArgb(127, 140, 141));
            groupData.AddChild(new NavigationItem(ModuleKeys.EngineeringDataSandbox, "XML és registry teszt"));
            _menuItems.Add(groupData);
        }

        public void BuildMenuUI()
        {
            Control[] oldControls = new Control[_scrollContainer.ContentControls.Count];
            _scrollContainer.ContentControls.CopyTo(oldControls, 0);
            _scrollContainer.ContentControls.Clear();
            _buttonsByModule.Clear();
            _accentByModule.Clear();

            foreach (Control control in oldControls)
            {
                DisposeControlImages(control);
                control.Dispose();
            }

            _activeButton = null;

            ThemePalette palette = ThemeManager.CurrentPalette;
            Color itemBack = palette.SurfaceAlt;
            Color itemHover = palette.SurfaceHover;
            Color headerBack = palette.Surface;
            Color itemText = palette.TextPrimary;

            for (int i = _menuItems.Count - 1; i >= 0; i--)
            {
                NavigationGroup group = _menuItems[i];

                Panel sectionPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Width = LayoutMetrics.NavigationExpandedWidth,
                    Tag = "NoTheme"
                };

                if (group.IsExpanded)
                {
                    for (int j = group.Children.Count - 1; j >= 0; j--)
                    {
                        NavigationItem item = group.Children[j];

                        Button btn = new Button
                        {
                            Text = $"    {item.DisplayText}",
                            Height = LayoutMetrics.MenuButtonHeight,
                            Dock = DockStyle.Top,
                            FlatStyle = FlatStyle.Flat,
                            BackColor = itemBack,
                            ForeColor = itemText,
                            Font = new Font("Segoe UI", 10F),
                            TextAlign = ContentAlignment.MiddleLeft,
                            Padding = new Padding(15, 0, 0, 0)
                        };
                        btn.FlatAppearance.BorderSize = 0;

                        btn.MouseEnter += (s, e) => { if (btn != _activeButton) btn.BackColor = ThemeManager.CurrentPalette.SurfaceHover; };
                        btn.MouseLeave += (s, e) => { if (btn != _activeButton) btn.BackColor = ThemeManager.CurrentPalette.SurfaceAlt; };

                        btn.Click += (s, e) =>
                        {
                            ServiceLocator.Navigation.NavigateTo(item.ModuleName);
                        };

                        _buttonsByModule[item.ModuleName] = btn;
                        _accentByModule[item.ModuleName] = group.AccentColor;
                        sectionPanel.Controls.Add(btn);
                    }
                }

                int sectionIconSize = GetSectionIconSize();
                int sectionTextLeft = SectionIconLeft + sectionIconSize + SectionIconTextGap;

                Label lblHeader = new Label
                {
                    Text = group.GroupName,
                    Dock = DockStyle.Top,
                    Height = LayoutMetrics.MenuSectionHeaderHeight,
                    ForeColor = palette.TextPrimary,
                    Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                    Padding = new Padding(sectionTextLeft, 0, SectionArrowWidth, 0),
                    BackColor = headerBack,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                };

                EventHandler toggleGroup = (s, e) =>
                {
                    group.IsExpanded = !group.IsExpanded;
                    BuildMenuUI();
                };
                lblHeader.Click += toggleGroup;

                sectionPanel.Controls.Add(lblHeader);

                Label arrowLabel = new Label
                {
                    Text = group.IsExpanded ? "▼" : "▶",
                    Dock = DockStyle.Right,
                    Width = SectionArrowWidth,
                    ForeColor = palette.TextSecondary,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Cursor = Cursors.Hand,
                    Tag = "NoTheme"
                };
                arrowLabel.Click += toggleGroup;
                lblHeader.Controls.Add(arrowLabel);

                PictureBox iconBox = new PictureBox
                {
                    Image = HvacIconRenderer.Render(
                        group.IconKind,
                        ThemeManager.CurrentThemeMode,
                        sectionIconSize,
                        group.AccentColor),
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Size = new Size(sectionIconSize, sectionIconSize),
                    Location = new Point(
                        SectionIconLeft,
                        (LayoutMetrics.MenuSectionHeaderHeight - sectionIconSize) / 2),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand,
                    Tag = "NoTheme"
                };
                iconBox.Click += toggleGroup;
                lblHeader.Controls.Add(iconBox);

                // JAVÍTVA: A fix kék helyett a logikai fából dinamikusan kiolvasott group.AccentColor-t használjuk!
                Panel accentLine = new Panel
                {
                    Width = SectionAccentWidth,
                    Dock = DockStyle.Left,
                    BackColor = group.AccentColor
                };
                lblHeader.Controls.Add(accentLine);

                _scrollContainer.ContentControls.Add(sectionPanel);
            }

            _scrollContainer.RecalculateContentLayout();
            _scrollContainer.ScrollToTop();
            SetActiveModule(ServiceLocator.Navigation.CurrentModuleName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ServiceLocator.Navigation.NavigationRequested -= OnNavigationRequested;

            base.Dispose(disposing);
        }

        private void OnNavigationRequested(string moduleName)
        {
            SetActiveModule(moduleName);
        }

        private void SetActiveModule(string moduleName)
        {
            if (_activeButton != null)
            {
                _activeButton.BackColor = ThemeManager.CurrentPalette.SurfaceAlt;
                _activeButton.ForeColor = ThemeManager.CurrentPalette.TextPrimary;
            }

            _activeButton = null;

            if (string.IsNullOrWhiteSpace(moduleName) ||
                !_buttonsByModule.TryGetValue(moduleName, out Button? button))
            {
                return;
            }

            _activeButton = button;
            button.BackColor = _accentByModule.TryGetValue(moduleName, out Color accent)
                ? accent
                : ThemeManager.CurrentPalette.Accent;
            button.ForeColor = Color.White;
        }

        private static int GetSectionIconSize()
        {
            int scaledSize = (int)Math.Round(ThemeMetrics.IconSizeNormal * 1.5);
            int maxSize = Math.Max(
                ThemeMetrics.IconSizeNormal,
                LayoutMetrics.MenuSectionHeaderHeight - 6);

            return Math.Min(scaledSize, maxSize);
        }

        private static void DisposeControlImages(Control control)
        {
            switch (control)
            {
                case Button button:
                    DisposeAndDetachImages(button);
                    break;
                case Label label:
                    DisposeAndDetachImages(label);
                    break;
                case PictureBox pictureBox:
                    DisposeAndDetachImages(pictureBox);
                    break;
                default:
                    Image? backgroundImage = control.BackgroundImage;
                    control.BackgroundImage = null;
                    backgroundImage?.Dispose();
                    break;
            }

            foreach (Control child in control.Controls)
            {
                DisposeControlImages(child);
            }
        }

        private static void DisposeAndDetachImages(ButtonBase button)
        {
            Image? image = button.Image;
            Image? backgroundImage = button.BackgroundImage;
            button.Image = null;
            button.BackgroundImage = null;
            image?.Dispose();
            backgroundImage?.Dispose();
        }

        private static void DisposeAndDetachImages(Label label)
        {
            Image? image = label.Image;
            Image? backgroundImage = label.BackgroundImage;
            label.Image = null;
            label.BackgroundImage = null;
            image?.Dispose();
            backgroundImage?.Dispose();
        }

        private static void DisposeAndDetachImages(PictureBox pictureBox)
        {
            Image? image = pictureBox.Image;
            Image? backgroundImage = pictureBox.BackgroundImage;
            pictureBox.Image = null;
            pictureBox.BackgroundImage = null;
            image?.Dispose();
            backgroundImage?.Dispose();
        }

        public class NavigationItem
        {
            public string ModuleName { get; }
            public string DisplayText { get; }

            public NavigationItem(string moduleName, string displayText)
            {
                ModuleName = moduleName;
                DisplayText = displayText;
            }
        }

        public class NavigationGroup
        {
            public string GroupId { get; }
            public string GroupName { get; }
            public HvacIconKind IconKind { get; }
            public Color AccentColor { get; } // ÚJ: A szakág egyedi azonosító színe
            public List<NavigationItem> Children { get; } = new List<NavigationItem>();
            public bool IsExpanded { get; set; } = false;

            // JAVÍTVA: A konstruktor most már kötelezően bekéri a szakági színt is
            public NavigationGroup(string groupId, string groupName, HvacIconKind iconKind, Color accentColor)
            {
                GroupId = groupId;
                GroupName = groupName;
                IconKind = iconKind;
                AccentColor = accentColor;

                if (groupId == "BuildingThermal") IsExpanded = true;
            }

            public void AddChild(NavigationItem child)
            {
                Children.Add(child);
            }
        }
    }
}
