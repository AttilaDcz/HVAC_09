using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using HVACDesigner.Data.Providers;
using HVACDesigner.CoreUI.Services;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.Services;
using HVACDesigner.CoreUI.Components.Structural;
using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Components.Results;
using HVACDesigner.CoreUI.Components.Data;
using HVACDesigner.CoreUI.Components.Dialogs;
using HVACDesigner.Calculations.Thermal.UValue;
using HVACDesigner.Calculations.Thermal.Common;
using HVACDesigner.EngineeringData.BuildingThermal.Materials;
using HVACDesigner.EngineeringData.BuildingThermal.AirLayers;
using HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates;
using HVACDesigner.EngineeringData.BuildingThermal.Openings;
using HVACDesigner.EngineeringData.SimpleCatalogs;
using HVACDesigner.Features.BuildingThermal;
using HVACDesigner.Calculations.Common;
using HVACDesigner.CoreUI.Notifications;

namespace HVACDesigner.CoreUI.Workspace.BuildingThermal
{
    public partial class UValueCalculationControl : UserControl
    {
        private readonly IMaterialProvider _materialProvider;
        private readonly List<UserStructure> _structures = new List<UserStructure>();
        private UserStructure _selectedStructure;
        private bool _isEditingDraft = false;
        private bool _isUpdatingDisplay = false;

        // Katalógusok és kalkulációs szolgáltatás
        private SimpleCatalog<MaterialDefinition> _materialCatalog;
        private SimpleCatalog<AirLayerSimpleDefinition> _airLayerCatalog;
        private SimpleCatalog<HVACDesigner.EngineeringData.SimpleCatalogs.ConstructionTemplateDefinition> _templatesCatalog;
        private SimpleCatalog<OpeningDefinition> _openingsCatalog;
        private readonly UValueCalculationService _calculationService = new UValueCalculationService();
        private readonly List<ThermalBridgeCorrectionOption> _thermalBridgeOptions = new List<ThermalBridgeCorrectionOption>();

        // UI vezérlők
        private TableLayoutPanel mainLayout;
        
        // Bal hasáb
        private EngineeringCardPanel cardLeft;
        private FlowLayoutPanel pnlStructuresList;
        private EngineeringButton btnNewStructure;
        private EngineeringButton btnDeleteStructure;

        // Középső hasáb
        private EngineeringCardPanel cardCenter;
        private EngineeringComboBox cmbCategory;
        private EngineeringComboBox cmbMaterial;
        private EngineeringTextBox txtThickness;
        private EngineeringButton btnInsertLayer;

        // Jobb hasáb (Rétegrend szerkesztő)
        private EngineeringCardPanel cardRight;
        private Panel pnlRightHeader;
        private EngineeringCheckBox chkUseTemplate;
        private EngineeringComboBox cmbTemplate;
        private EngineeringComboBox cmbThermalBridgeType; // ÚJ: Hőhíd kialakítás ComboBox
        private EngineeringTextBox txtThermalBridge;
        private Label lblSideDirection;
        private EngineeringDataGridView gridLayers;
        private Panel pnlRightFooter;
        private EngineeringButton btnDeleteLayer;
        private EngineeringButton btnSaveStructure;
        private Label lblResultU;
        private Label lblResultLimit;
        private Label lblResultStatus;

        // Jobb hasáb (Nyílászáró szerkesztő panel)
        private Panel pnlOpeningEditor;
        private EngineeringRadioButton rbOpeningCatalog;
        private EngineeringRadioButton rbOpeningCalculate;
        private EngineeringComboBox cmbOpeningCatalogItem;
        private Panel pnlOpeningCalculationDetails;
        private EngineeringComboBox cmbGlazing;
        private EngineeringComboBox cmbFrame;
        private EngineeringTextBox txtFrameWidth;
        private EngineeringTextBox txtOpeningWidth;
        private EngineeringTextBox txtOpeningHeight;
        private EngineeringComboBox cmbSpacer;
        private EngineeringButton btnSaveOpeningStructure;
        private Label lblOpeningResultU;
        private Label lblOpeningResultLimit;
        private Label lblOpeningResultStatus;

        public UValueCalculationControl(IMaterialProvider materialProvider)
        {
            _materialProvider = materialProvider ?? throw new ArgumentNullException(nameof(materialProvider));

            InitializeComponent();
            
            BuildInterface();
            LoadCatalogData();
            
            // Alapértelmezett kezdő szerkezet a sablonból
            var defaultStruct = new UserStructure
            {
                Id = "Structure.Wall.1",
                Name = "Külső ETICS falazat",
                Type = ConstructionType.ExternalWall,
                ThermalBridgeCorrectionFactor = 0.10,
                ThermalBridgeOptionIndex = 0
            };
            
            // Sablon rétegek betöltése a catalog-ból a C#-ba égetett adatok helyett
            var defaultTemplate = _templatesCatalog.Items.Values
                .FirstOrDefault(t => string.Equals(t.Id, "Wall.External.CeramicBlock.200.EPS.120", StringComparison.OrdinalIgnoreCase))
                ?? _templatesCatalog.Items.Values.FirstOrDefault();
            
            if (defaultTemplate != null)
            {
                foreach (var templateLayer in defaultTemplate.Layers)
                {
                    bool isAir = !string.IsNullOrEmpty(templateLayer.AirLayerId);
                    if (isAir)
                    {
                        var airDef = _airLayerCatalog.Items.Values.FirstOrDefault(a => a.Id == templateLayer.AirLayerId);
                        if (airDef != null)
                        {
                            defaultStruct.Layers.Add(new UserLayer
                            {
                                ReferenceId = airDef.Id,
                                DisplayName = airDef.DisplayName,
                                IsAirLayer = true,
                                ThicknessM = 0.0,
                                DesignLambda = airDef.ThermalResistance
                            });
                        }
                    }
                    else
                    {
                        var matDef = _materialCatalog.Items.Values.FirstOrDefault(m => m.Id == templateLayer.MaterialId);
                        if (matDef != null)
                        {
                            defaultStruct.Layers.Add(new UserLayer
                            {
                                ReferenceId = matDef.Id,
                                DisplayName = matDef.DisplayName,
                                IsAirLayer = false,
                                ThicknessM = templateLayer.ThicknessM ?? 0.0,
                                DesignLambda = matDef.Lambda * (matDef.LambdaCorrection ?? 1.0)
                            });
                        }
                    }
                }
            }
            
            _structures.Add(defaultStruct);
            _selectedStructure = defaultStruct;
            _isEditingDraft = false;

            CalculateUValue(defaultStruct);
            RefreshDisplay();
        }

        private void BuildInterface()
        {
            // Fő elrendezés (3 hasáb)
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(8)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            Controls.Add(mainLayout);

            // ==========================================
            // BAL HASÁB - Szerkezetlista
            // ==========================================
            cardLeft = new EngineeringCardPanel
            {
                Title = "Szerkezetlista",
                Dock = DockStyle.Fill,
                Margin = new Padding(4)
            };
            mainLayout.Controls.Add(cardLeft, 0, 0);

            pnlStructuresList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(4)
            };
            cardLeft.ContentPanel.Controls.Add(pnlStructuresList);

            Panel pnlLeftActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(4)
            };
            
            TableLayoutPanel pnlLeftButtons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            pnlLeftButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlLeftButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            btnNewStructure = new EngineeringButton
            {
                Text = "Új szerkezet",
                Dock = DockStyle.Fill
            };
            btnNewStructure.Click += BtnNewStructure_Click;

            btnDeleteStructure = new EngineeringButton
            {
                Text = "Törlés",
                Dock = DockStyle.Fill
            };
            btnDeleteStructure.Click += BtnDeleteStructure_Click;

            pnlLeftButtons.Controls.Add(btnNewStructure, 0, 0);
            pnlLeftButtons.Controls.Add(btnDeleteStructure, 1, 0);
            pnlLeftActions.Controls.Add(pnlLeftButtons);

            cardLeft.ContentPanel.Controls.Add(pnlLeftActions);
            pnlLeftActions.BringToFront();

            // ==========================================
            // KÖZÉPSŐ HASÁB - Anyagválaszték
            // ==========================================
            cardCenter = new EngineeringCardPanel
            {
                Title = "Anyagválaszték",
                Dock = DockStyle.Fill,
                Margin = new Padding(4)
            };
            mainLayout.Controls.Add(cardCenter, 1, 0);

            cmbCategory = new EngineeringComboBox
            {
                LabelText = "Kategória",
                Dock = DockStyle.Top,
                Height = 60
            };
            cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;

            cmbMaterial = new EngineeringComboBox
            {
                LabelText = "Anyag / Légréteg",
                Dock = DockStyle.Top,
                Height = 60,
                Margin = new Padding(0, 10, 0, 0)
            };

            txtThickness = new EngineeringTextBox
            {
                LabelText = "Vastagság [m]",
                Dock = DockStyle.Top,
                Height = 60,
                Text = "0.10",
                Margin = new Padding(0, 10, 0, 0)
            };

            btnInsertLayer = new EngineeringButton
            {
                Text = "Réteg hozzáadása",
                Dock = DockStyle.Top,
                Height = 36,
                Margin = new Padding(0, 15, 0, 0)
            };
            btnInsertLayer.Click += BtnInsertLayer_Click;

            cardCenter.ContentPanel.Controls.Add(btnInsertLayer);
            cardCenter.ContentPanel.Controls.Add(txtThickness);
            cardCenter.ContentPanel.Controls.Add(cmbMaterial);
            cardCenter.ContentPanel.Controls.Add(cmbCategory);
            
            btnInsertLayer.BringToFront();
            txtThickness.BringToFront();
            cmbMaterial.BringToFront();
            cmbCategory.BringToFront();

            // ==========================================
            // JOBB HASÁB - Szerkezet és Nyílászáró szerkesztő
            // ==========================================
            cardRight = new EngineeringCardPanel
            {
                Title = "Szerkezet",
                Dock = DockStyle.Fill,
                Margin = new Padding(4)
            };
            mainLayout.Controls.Add(cardRight, 2, 0);

            // A. Rétegrend szerkesztő (Fejléc)
            pnlRightHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                Padding = new Padding(4)
            };

            chkUseTemplate = new EngineeringCheckBox
            {
                Text = "Sablonból választok",
                Location = new Point(10, 10),
                Size = new Size(160, 24)
            };
            chkUseTemplate.CheckedChanged += ChkUseTemplate_CheckedChanged;

            cmbTemplate = new EngineeringComboBox
            {
                LabelText = "Sablonok",
                Location = new Point(180, 2),
                Width = 220,
                Height = 45,
                Visible = false
            };
            cmbTemplate.SelectedIndexChanged += CmbTemplate_SelectedIndexChanged;

            cmbThermalBridgeType = new EngineeringComboBox
            {
                LabelText = "Hőhíd-kialakítás (TNM)",
                Location = new Point(10, 50),
                Width = 260,
                Height = 45
            };
            cmbThermalBridgeType.SelectedIndexChanged += CmbThermalBridgeType_SelectedIndexChanged;

            txtThermalBridge = new EngineeringTextBox
            {
                LabelText = "Hőhíd-tényező (χ)",
                Location = new Point(280, 50),
                Width = 120,
                Height = 50,
                Text = "0.10"
            };
            txtThermalBridge.TextChanged += TxtThermalBridge_TextChanged;

            lblSideDirection = new Label
            {
                Text = "← Belső oldal | Külső oldal →",
                Location = new Point(180, 80),
                Width = 220,
                Font = ThemeFonts.Caption,
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleRight
            };

            pnlRightHeader.Controls.Add(chkUseTemplate);
            pnlRightHeader.Controls.Add(cmbTemplate);
            pnlRightHeader.Controls.Add(cmbThermalBridgeType);
            pnlRightHeader.Controls.Add(txtThermalBridge);
            pnlRightHeader.Controls.Add(lblSideDirection);
            cardRight.ContentPanel.Controls.Add(pnlRightHeader);

            // Rétegtáblázat
            gridLayers = new EngineeringDataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = false
            };
            gridLayers.Columns.Clear();
            gridLayers.AutoGenerateColumns = false;
            gridLayers.AllowUserToAddRows = false;
            gridLayers.AllowUserToDeleteRows = false;
            gridLayers.MultiSelect = false;
            gridLayers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            gridLayers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Order", HeaderText = "Sorrend", Width = 85, ReadOnly = true });
            gridLayers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Megnevezés", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            gridLayers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Thickness", HeaderText = "Vastagság [m]", Width = 110, ReadOnly = false });
            gridLayers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Resistance", HeaderText = "Ellenállás [m²K/W]", Width = 130, ReadOnly = true });
            
            gridLayers.CellEndEdit += GridLayers_CellEndEdit;

            cardRight.ContentPanel.Controls.Add(gridLayers);
            gridLayers.BringToFront();

            // Rétegrend szerkesztő (Lábléc)
            pnlRightFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 140,
                Padding = new Padding(4)
            };

            btnDeleteLayer = new EngineeringButton
            {
                Text = "Kijelölt réteg törlése",
                Location = new Point(10, 10),
                Width = 160,
                Height = 32
            };
            btnDeleteLayer.Click += BtnDeleteLayer_Click;

            btnSaveStructure = new EngineeringButton
            {
                Text = "Szerkezet mentése",
                Location = new Point(10, 50),
                Width = 160,
                Height = 32
            };
            btnSaveStructure.Click += BtnSaveStructure_Click;

            lblResultU = new Label
            {
                Text = "Alap U-érték: -\nHőhídkorrekció: -\nKorrigált U-érték: - W/m²K",
                Location = new Point(180, 10),
                Width = 240,
                Height = 55,
                Font = ThemeFonts.BodyBold,
                ForeColor = ThemeManager.CurrentPalette.TextPrimary
            };

            lblResultLimit = new Label
            {
                Text = "ÉKM határérték: -",
                Location = new Point(180, 75),
                Width = 240,
                Font = ThemeFonts.Caption,
                ForeColor = ThemeManager.CurrentPalette.TextSecondary
            };

            lblResultStatus = new Label
            {
                Text = "Státusz: -",
                Location = new Point(180, 100),
                Width = 240,
                Font = ThemeFonts.BodyBold,
                ForeColor = Color.Gray
            };

            pnlRightFooter.Controls.Add(btnDeleteLayer);
            pnlRightFooter.Controls.Add(btnSaveStructure);
            pnlRightFooter.Controls.Add(lblResultU);
            pnlRightFooter.Controls.Add(lblResultLimit);
            pnlRightFooter.Controls.Add(lblResultStatus);
            cardRight.ContentPanel.Controls.Add(pnlRightFooter);
            pnlRightFooter.BringToFront();

            // B. Nyílászáró szerkesztő panel
            pnlOpeningEditor = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                Padding = new Padding(8)
            };
            cardRight.ContentPanel.Controls.Add(pnlOpeningEditor);

            rbOpeningCatalog = new EngineeringRadioButton
            {
                Text = "Egyszerű megadás a katalógusból",
                Location = new Point(10, 10),
                Size = new Size(250, 24),
                Checked = true
            };
            rbOpeningCatalog.CheckedChanged += RbOpeningCatalog_CheckedChanged;

            rbOpeningCalculate = new EngineeringRadioButton
            {
                Text = "Részletes számítás",
                Location = new Point(10, 35),
                Size = new Size(250, 24)
            };
            rbOpeningCalculate.CheckedChanged += RbOpeningCatalog_CheckedChanged;

            cmbOpeningCatalogItem = new EngineeringComboBox
            {
                LabelText = "Katalógusbeli nyílászáró",
                Location = new Point(10, 70),
                Width = 380,
                Height = 60
            };
            cmbOpeningCatalogItem.SelectedIndexChanged += CmbOpeningCatalogItem_SelectedIndexChanged;

            // Részletes számítási vezérlők
            pnlOpeningCalculationDetails = new Panel
            {
                Location = new Point(10, 70),
                Size = new Size(390, 360),
                Visible = false
            };

            cmbGlazing = new EngineeringComboBox
            {
                LabelText = "Üveg típusa",
                Dock = DockStyle.Top,
                Height = 55
            };
            cmbGlazing.SelectedIndexChanged += CmbOpeningCalculation_Changed;

            cmbFrame = new EngineeringComboBox
            {
                LabelText = "Keret típusa",
                Dock = DockStyle.Top,
                Height = 55,
                Margin = new Padding(0, 5, 0, 0)
            };
            cmbFrame.SelectedIndexChanged += CmbOpeningCalculation_Changed;

            txtFrameWidth = new EngineeringTextBox
            {
                LabelText = "Keret vastagsága [mm]",
                Dock = DockStyle.Top,
                Height = 55,
                Text = "80",
                Margin = new Padding(0, 5, 0, 0)
            };
            txtFrameWidth.TextChanged += TxtFrameWidth_TextChanged;

            txtOpeningWidth = new EngineeringTextBox
            {
                LabelText = "Szélesség [m]",
                Dock = DockStyle.Top,
                Height = 55,
                Text = "1.23",
                Margin = new Padding(0, 5, 0, 0)
            };
            txtOpeningWidth.TextChanged += TxtOpeningGeometry_TextChanged;

            txtOpeningHeight = new EngineeringTextBox
            {
                LabelText = "Magasság [m]",
                Dock = DockStyle.Top,
                Height = 55,
                Text = "1.48",
                Margin = new Padding(0, 5, 0, 0)
            };
            txtOpeningHeight.TextChanged += TxtOpeningGeometry_TextChanged;

            cmbSpacer = new EngineeringComboBox
            {
                LabelText = "Üveg peremezése / távtartó",
                Dock = DockStyle.Top,
                Height = 55,
                Margin = new Padding(0, 5, 0, 0)
            };
            cmbSpacer.SelectedIndexChanged += CmbOpeningCalculation_Changed;

            pnlOpeningCalculationDetails.Controls.Add(cmbSpacer);
            pnlOpeningCalculationDetails.Controls.Add(txtOpeningHeight);
            pnlOpeningCalculationDetails.Controls.Add(txtOpeningWidth);
            pnlOpeningCalculationDetails.Controls.Add(txtFrameWidth);
            pnlOpeningCalculationDetails.Controls.Add(cmbFrame);
            pnlOpeningCalculationDetails.Controls.Add(cmbGlazing);
            cmbSpacer.BringToFront();
            txtOpeningHeight.BringToFront();
            txtOpeningWidth.BringToFront();
            txtFrameWidth.BringToFront();
            cmbFrame.BringToFront();
            cmbGlazing.BringToFront();

            // Mentés gomb és Eredmények a nyílászáró panelen
            btnSaveOpeningStructure = new EngineeringButton
            {
                Text = "Szerkezet mentése",
                Location = new Point(10, 440),
                Width = 160,
                Height = 32
            };
            btnSaveOpeningStructure.Click += BtnSaveStructure_Click;

            lblOpeningResultU = new Label
            {
                Text = "Eredő U-érték: - W/m²K",
                Location = new Point(180, 440),
                Width = 240,
                Font = ThemeFonts.BodyBold,
                ForeColor = ThemeManager.CurrentPalette.TextPrimary
            };

            lblOpeningResultLimit = new Label
            {
                Text = "ÉKM határérték: -",
                Location = new Point(180, 470),
                Width = 240,
                Font = ThemeFonts.Caption,
                ForeColor = ThemeManager.CurrentPalette.TextSecondary
            };

            lblOpeningResultStatus = new Label
            {
                Text = "Státusz: -",
                Location = new Point(180, 495),
                Width = 240,
                Font = ThemeFonts.BodyBold,
                ForeColor = Color.Gray
            };

            pnlOpeningEditor.Controls.Add(btnSaveOpeningStructure);
            pnlOpeningEditor.Controls.Add(pnlOpeningCalculationDetails);
            pnlOpeningEditor.Controls.Add(cmbOpeningCatalogItem);
            pnlOpeningEditor.Controls.Add(rbOpeningCatalog);
            pnlOpeningEditor.Controls.Add(rbOpeningCalculate);
            pnlOpeningEditor.Controls.Add(lblOpeningResultU);
            pnlOpeningEditor.Controls.Add(lblOpeningResultLimit);
            pnlOpeningEditor.Controls.Add(lblOpeningResultStatus);
        }

        private void LoadCatalogData()
        {
            try
            {
                // Számítási szolgáltatás inicializálása
                _calculationService.Initialize(ServiceLocator.EngineeringDataRegistry);

                // UI katalógusok lekérése
                _materialCatalog = ServiceLocator.EngineeringDataRegistry.GetRequired<SimpleCatalog<MaterialDefinition>>("Catalog.Materials", "1.0");
                _airLayerCatalog = ServiceLocator.EngineeringDataRegistry.GetRequired<SimpleCatalog<AirLayerSimpleDefinition>>("Catalog.AirLayers", "1.0");
                _templatesCatalog = ServiceLocator.EngineeringDataRegistry.GetRequired<SimpleCatalog<HVACDesigner.EngineeringData.SimpleCatalogs.ConstructionTemplateDefinition>>("Catalog.ConstructionTemplates", "1.0");
                _openingsCatalog = _calculationService.OpeningsCatalog;

                // TNM Legacy szabályok beolvasása a hőhíd korrekciókhoz a resolver segítségével
                var thermalBridgeResolver = new ThermalBridgeRuleResolver();
                var resolvedOptions = thermalBridgeResolver.ResolveOptions(ServiceLocator.EngineeringRuleRegistry);

                // Hőhíd kialakítási opciók összeállítása
                _thermalBridgeOptions.Clear();
                _thermalBridgeOptions.Add(new ThermalBridgeCorrectionOption("Custom", "Egyedi (kézi megadás)", ConstructionType.Custom, 0.10, ""));
                foreach (var opt in resolvedOptions)
                {
                    _thermalBridgeOptions.Add(opt);
                }

                // Hőhíd legördülő feltöltése
                cmbThermalBridgeType.Items.Clear();
                foreach (var opt in _thermalBridgeOptions)
                {
                    cmbThermalBridgeType.Items.Add(opt);
                }
                cmbThermalBridgeType.SelectedIndex = 0;

                // Anyagkategóriák feltöltése
                cmbCategory.Items.Clear();
                var categories = _materialCatalog.Items.Values
                    .Select(m => m.Category)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                foreach (var cat in categories)
                {
                    cmbCategory.Items.Add(cat);
                }
                cmbCategory.Items.Add("Légrétegek");
                
                if (cmbCategory.Items.Count > 0)
                {
                    cmbCategory.SelectedIndex = 0;
                }

                // Sablonok feltöltése
                cmbTemplate.Items.Clear();
                var templates = _templatesCatalog.Items.Values
                    .OrderBy(t => t.DisplayName)
                    .ToList();

                foreach (var temp in templates)
                {
                    cmbTemplate.Items.Add(temp.DisplayName);
                }
                if (cmbTemplate.Items.Count > 0)
                {
                    cmbTemplate.SelectedIndex = 0;
                }

                // Nyílászáró katalógus feltöltése
                cmbOpeningCatalogItem.Items.Clear();
                foreach (var op in _openingsCatalog.Items.Values)
                {
                    cmbOpeningCatalogItem.Items.Add(op.DisplayName);
                }
                if (cmbOpeningCatalogItem.Items.Count > 0)
                {
                    cmbOpeningCatalogItem.SelectedIndex = 0;
                }

                // Nyílászáró számítás legördülők feltöltése a szolgáltatásból
                cmbGlazing.Items.Clear();
                foreach (var g in _calculationService.Glazings) cmbGlazing.Items.Add(g.Name);
                if (cmbGlazing.Items.Count > 0) cmbGlazing.SelectedIndex = 0;

                cmbFrame.Items.Clear();
                foreach (var f in _calculationService.Frames) cmbFrame.Items.Add(f.Name);
                if (cmbFrame.Items.Count > 0) cmbFrame.SelectedIndex = 0;

                cmbSpacer.Items.Clear();
                foreach (var s in _calculationService.Spacers) cmbSpacer.Items.Add(s.Name);
                if (cmbSpacer.Items.Count > 0) cmbSpacer.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                EngineeringNotificationService.Danger("Adatbetöltési hiba", $"Hiba a katalógusadatok betöltésekor: {ex.Message}");
            }
        }

        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCat = cmbCategory.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedCat)) return;

            cmbMaterial.Items.Clear();

            if (selectedCat == "Légrétegek")
            {
                var airLayers = _airLayerCatalog.Items.Values
                    .OrderBy(a => a.DisplayName)
                    .ToList();

                foreach (var air in airLayers)
                {
                    cmbMaterial.Items.Add(air.DisplayName);
                }
                
                txtThickness.Enabled = false;
                txtThickness.Text = "-";
            }
            else
            {
                var materials = _materialCatalog.Items.Values
                    .Where(m => string.Equals(m.Category, selectedCat, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(m => m.DisplayName)
                    .ToList();

                foreach (var mat in materials)
                {
                    cmbMaterial.Items.Add(mat.DisplayName);
                }

                txtThickness.Enabled = true;
                txtThickness.Text = "0.10";
            }

            if (cmbMaterial.Items.Count > 0)
            {
                cmbMaterial.SelectedIndex = 0;
            }
        }

        private void ChkUseTemplate_CheckedChanged(object sender, EventArgs e)
        {
            cmbTemplate.Visible = chkUseTemplate.Checked;
            if (chkUseTemplate.Checked)
            {
                CmbTemplate_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        private void CmbTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingDisplay) return;
            if (!chkUseTemplate.Checked || _selectedStructure == null) return;

            string selectedTempName = cmbTemplate.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTempName)) return;

            var template = _templatesCatalog.Items.Values
                .FirstOrDefault(t => string.Equals(t.DisplayName, selectedTempName, StringComparison.OrdinalIgnoreCase));
            
            if (template == null) return;

            _selectedStructure.Layers.Clear();
            foreach (var templLayer in template.Layers)
            {
                bool isAir = !string.IsNullOrEmpty(templLayer.AirLayerId);
                string refId = isAir ? templLayer.AirLayerId : templLayer.MaterialId;
                string displayName = "";
                double lambda = 0.0;

                if (isAir)
                {
                    if (_airLayerCatalog.Items.TryGetValue(refId, out var airLayer))
                    {
                        displayName = airLayer.DisplayName;
                        lambda = airLayer.ThermalResistance;
                    }
                    else
                    {
                        displayName = refId;
                    }
                }
                else
                {
                    if (_materialCatalog.Items.TryGetValue(refId, out var mat))
                    {
                        displayName = mat.DisplayName;
                        lambda = mat.Lambda * (mat.LambdaCorrection ?? 1.0);
                    }
                    else
                    {
                        displayName = refId;
                    }
                }

                _selectedStructure.Layers.Add(new UserLayer
                {
                    ReferenceId = refId,
                    DisplayName = displayName,
                    IsAirLayer = isAir,
                    ThicknessM = templLayer.ThicknessM ?? 0.0,
                    DesignLambda = lambda
                });
            }

            CalculateUValue(_selectedStructure);
            RefreshDisplay();
        }

        private void BtnInsertLayer_Click(object sender, EventArgs e)
        {
            if (_selectedStructure == null || _selectedStructure.IsOpening) return;

            string selectedCat = cmbCategory.SelectedItem?.ToString();
            string selectedMatName = cmbMaterial.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedCat) || string.IsNullOrEmpty(selectedMatName)) return;

            if (selectedCat == "Légrétegek")
            {
                var airLayer = _airLayerCatalog.Items.Values
                    .FirstOrDefault(a => string.Equals(a.DisplayName, selectedMatName, StringComparison.OrdinalIgnoreCase));
                
                if (airLayer == null) return;

                _selectedStructure.Layers.Add(new UserLayer
                {
                    ReferenceId = airLayer.Id,
                    DisplayName = airLayer.DisplayName,
                    IsAirLayer = true,
                    ThicknessM = 0.0,
                    DesignLambda = airLayer.ThermalResistance
                });
            }
            else
            {
                var mat = _materialCatalog.Items.Values
                    .FirstOrDefault(m => string.Equals(m.DisplayName, selectedMatName, StringComparison.OrdinalIgnoreCase));
                
                if (mat == null) return;

                if (!double.TryParse(txtThickness.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double thickness) || thickness <= 0)
                {
                    if (!double.TryParse(txtThickness.Text.Replace(".", ","), NumberStyles.Float, new CultureInfo("hu-HU"), out thickness) || thickness <= 0)
                    {
                        MessageBox.Show("Kérjük, adjon meg egy érvényes pozitív vastagságot méterben (pl. 0.12)!", "Érvénytelen vastagság", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                _selectedStructure.Layers.Add(new UserLayer
                {
                    ReferenceId = mat.Id,
                    DisplayName = mat.DisplayName,
                    IsAirLayer = false,
                    ThicknessM = thickness,
                    DesignLambda = mat.Lambda * (mat.LambdaCorrection ?? 1.0)
                });
            }

            chkUseTemplate.Checked = false;

            CalculateUValue(_selectedStructure);
            RefreshDisplay();
        }

        private void BtnDeleteLayer_Click(object sender, EventArgs e)
        {
            if (_selectedStructure == null || _selectedStructure.IsOpening || gridLayers.CurrentRow == null) return;

            int index = gridLayers.CurrentRow.Index;
            if (index >= 0 && index < _selectedStructure.Layers.Count)
            {
                _selectedStructure.Layers.RemoveAt(index);
                chkUseTemplate.Checked = false;
                
                CalculateUValue(_selectedStructure);
                RefreshDisplay();
            }
        }

        private void GridLayers_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_selectedStructure == null || _selectedStructure.IsOpening || e.RowIndex < 0) return;
            
            if (e.ColumnIndex == 2)
            {
                var cellVal = gridLayers.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                if (double.TryParse(cellVal, NumberStyles.Float, CultureInfo.InvariantCulture, out double thickness) && thickness > 0)
                {
                    _selectedStructure.Layers[e.RowIndex].ThicknessM = thickness;
                }
                else if (double.TryParse(cellVal?.Replace(".", ","), NumberStyles.Float, new CultureInfo("hu-HU"), out thickness) && thickness > 0)
                {
                    _selectedStructure.Layers[e.RowIndex].ThicknessM = thickness;
                }
                else
                {
                    MessageBox.Show("Kérjük, adjon meg egy érvényes pozitív vastagságot méterben (pl. 0.20)!", "Érvénytelen vastagság", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                CalculateUValue(_selectedStructure);
                RefreshDisplay();
            }
        }

        private void CmbThermalBridgeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingDisplay) return;
            if (_selectedStructure == null || _selectedStructure.IsOpening) return;

            var selectedOpt = cmbThermalBridgeType.SelectedItem as ThermalBridgeCorrectionOption;
            if (selectedOpt == null) return;

            _selectedStructure.ThermalBridgeOptionIndex = cmbThermalBridgeType.SelectedIndex;

            if (selectedOpt.Id == "Custom")
            {
                txtThermalBridge.Enabled = true;
            }
            else
            {
                txtThermalBridge.Enabled = false;
                txtThermalBridge.Text = selectedOpt.CorrectionFactor.ToString("F2", CultureInfo.InvariantCulture);
                _selectedStructure.ThermalBridgeCorrectionFactor = selectedOpt.CorrectionFactor;
            }

            CalculateUValue(_selectedStructure);
            RefreshDisplay();
        }

        private void TxtThermalBridge_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingDisplay) return;
            if (_selectedStructure == null || _selectedStructure.IsOpening || !txtThermalBridge.Enabled) return;

            if (double.TryParse(txtThermalBridge.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double allowance))
            {
                _selectedStructure.ThermalBridgeCorrectionFactor = allowance;
            }
            else if (double.TryParse(txtThermalBridge.Text.Replace(".", ","), NumberStyles.Float, new CultureInfo("hu-HU"), out allowance))
            {
                _selectedStructure.ThermalBridgeCorrectionFactor = allowance;
            }
            else
            {
                return;
            }

            CalculateUValue(_selectedStructure);
            RefreshDisplay();
        }

        private void BtnNewStructure_Click(object sender, EventArgs e)
        {
            using (var dialog = new EngineeringDialog())
            {
                dialog.DialogTitle = "Szerkezet típusa";
                dialog.DialogSubtitle = "Válassza ki a kívánt szerkezetet.";
                dialog.DialogSize = EngineeringDialogSize.Small;
                dialog.ButtonSet = EngineeringDialogButtonSet.OkCancel;

                var txtName = new EngineeringTextBox
                {
                    LabelText = "Szerkezet megnevezése",
                    Text = "Új szerkezet",
                    Dock = DockStyle.Top,
                    Height = 60
                };

                var cmbOpeningType = new EngineeringComboBox
                {
                    LabelText = "Nyílászáró típusa (ha ablak/ajtó)",
                    Dock = DockStyle.Top,
                    Height = 60,
                    Margin = new Padding(0, 10, 0, 0),
                    Visible = false
                };
                cmbOpeningType.Items.Add("Ablak");
                cmbOpeningType.Items.Add("Bejárati ajtó");
                cmbOpeningType.Items.Add("Teraszajtó");
                cmbOpeningType.Items.Add("Garázskapu");
                cmbOpeningType.Items.Add("Ipari kapu");
                cmbOpeningType.SelectedIndex = 0;

                var cmbType = new EngineeringComboBox
                {
                    LabelText = "Szerkezet típusa",
                    Dock = DockStyle.Top,
                    Height = 60,
                    Margin = new Padding(0, 10, 0, 0)
                };
                cmbType.Items.Add("Külső fal");
                cmbType.Items.Add("Tető / Lapostető");
                cmbType.Items.Add("Födém / Mennyezet");
                cmbType.Items.Add("Padló (talajon fekvő)");
                cmbType.Items.Add("Pincefal");
                cmbType.Items.Add("Belső fal (elválasztó)");
                cmbType.Items.Add("Ablak / Ajtó (Nyílászáró)");
                cmbType.SelectedIndex = 0;

                cmbType.SelectedIndexChanged += (s, ev) =>
                {
                    cmbOpeningType.Visible = cmbType.SelectedItem?.ToString() == "Ablak / Ajtó (Nyílászáró)";
                };

                dialog.ContentPanel.Controls.Add(cmbOpeningType);
                dialog.ContentPanel.Controls.Add(cmbType);
                dialog.ContentPanel.Controls.Add(txtName);
                txtName.BringToFront();

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string name = txtName.Text.Trim();
                    if (string.IsNullOrEmpty(name)) name = "Új szerkezet";
                    
                    bool isOpening = cmbType.SelectedItem?.ToString() == "Ablak / Ajtó (Nyílászáró)";
                    
                    string opType = "Window";
                    if (isOpening)
                    {
                        string sel = cmbOpeningType.SelectedItem?.ToString() ?? "Ablak";
                        if (sel == "Bejárati ajtó") opType = "EntranceDoor";
                        else if (sel == "Teraszajtó") opType = "TerraceDoor";
                        else if (sel == "Garázskapu") opType = "GarageGate";
                        else if (sel == "Ipari kapu") opType = "IndustrialGate";
                    }

                    var newStruct = new UserStructure
                    {
                        Id = "Structure." + Guid.NewGuid().ToString("N").Substring(0, 8),
                        Name = name,
                        Type = isOpening ? ConstructionType.Custom : TranslateType(cmbType.SelectedItem?.ToString() ?? "Külső fal"),
                        ThermalBridgeCorrectionFactor = isOpening ? 0.0 : 0.10,
                        IsOpening = isOpening,
                        OpeningType = opType,
                        ThermalBridgeOptionIndex = 0
                    };

                    if (isOpening)
                    {
                        if (_openingsCatalog.Items.Count > 0)
                            newStruct.OpeningCatalogId = _openingsCatalog.Items.Values.First().Id;
                        if (_calculationService.Glazings.Count > 0)
                            newStruct.GlazingId = _calculationService.Glazings[0].Id;
                        if (_calculationService.Frames.Count > 0)
                            newStruct.FrameId = _calculationService.Frames[0].Id;
                        if (_calculationService.Spacers.Count > 0)
                            newStruct.SpacerId = _calculationService.Spacers[0].Id;
                    }
                    
                    _selectedStructure = newStruct;
                    _isEditingDraft = true;
                    
                    CalculateUValue(newStruct);
                    RefreshDisplay();
                }
            }
        }

        private void BtnDeleteStructure_Click(object sender, EventArgs e)
        {
            if (_selectedStructure == null) return;

            using (var dialog = new EngineeringDialog())
            {
                dialog.DialogTitle = "Szerkezet törlése";
                dialog.DialogSubtitle = $"Biztosan törölni szeretné a(z) '{_selectedStructure.Name}' szerkezetet?";
                dialog.DialogSize = EngineeringDialogSize.Small;
                dialog.ButtonSet = EngineeringDialogButtonSet.YesNo;

                if (dialog.ShowDialog(this) == DialogResult.Yes)
                {
                    if (!_isEditingDraft)
                    {
                        _structures.Remove(_selectedStructure);
                    }
                    
                    _selectedStructure = _structures.FirstOrDefault();
                    _isEditingDraft = false;
                    RefreshDisplay();
                }
            }
        }

        private void BtnSaveStructure_Click(object sender, EventArgs e)
        {
            if (_selectedStructure == null) return;

            if (_isEditingDraft)
            {
                if (!_selectedStructure.IsOpening && _selectedStructure.Layers.Count == 0)
                {
                    EngineeringNotificationService.Warning("Üres rétegrend", "Mentés előtt adjon hozzá legalább egy réteget a szerkezethez!");
                    return;
                }

                _structures.Add(_selectedStructure);
                _isEditingDraft = false;
            }

            CalculateUValue(_selectedStructure);
            RefreshDisplay();
            
            EngineeringNotificationService.Success("Sikeres mentés", "A szerkezet adatai sikeresen mentve lettek.");
        }

        private void RbOpeningCatalog_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingDisplay) return;
            if (_selectedStructure == null || !_selectedStructure.IsOpening) return;

            _selectedStructure.IsOpeningCalculated = rbOpeningCalculate.Checked;
            cmbOpeningCatalogItem.Visible = rbOpeningCatalog.Checked;
            pnlOpeningCalculationDetails.Visible = rbOpeningCalculate.Checked;

            CalculateUValue(_selectedStructure);
            RefreshDisplay();
        }

        private void CmbOpeningCatalogItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingDisplay) return;
            if (_selectedStructure == null || !_selectedStructure.IsOpening) return;

            string selectedDisplayName = cmbOpeningCatalogItem.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedDisplayName)) return;

            var op = _openingsCatalog.Items.Values
                .FirstOrDefault(o => string.Equals(o.DisplayName, selectedDisplayName, StringComparison.OrdinalIgnoreCase));

            if (op != null)
            {
                _selectedStructure.OpeningCatalogId = op.Id;
                CalculateUValue(_selectedStructure);
                RefreshDisplay();
            }
        }

        private void CmbOpeningCalculation_Changed(object sender, EventArgs e)
        {
            if (_isUpdatingDisplay) return;
            if (_selectedStructure == null || !_selectedStructure.IsOpening) return;

            string gName = cmbGlazing.SelectedItem?.ToString();
            string fName = cmbFrame.SelectedItem?.ToString();
            string sName = cmbSpacer.SelectedItem?.ToString();

            var glazing = _calculationService.Glazings.FirstOrDefault(g => g.Name == gName);
            var frame = _calculationService.Frames.FirstOrDefault(f => f.Name == fName);
            var spacer = _calculationService.Spacers.FirstOrDefault(s => s.Name == sName);

            if (glazing != null) _selectedStructure.GlazingId = glazing.Id;
            if (frame != null) _selectedStructure.FrameId = frame.Id;
            if (spacer != null) _selectedStructure.SpacerId = spacer.Id;

            CalculateUValue(_selectedStructure);
            RefreshDisplay();
        }

        private void TxtFrameWidth_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingDisplay) return;
            if (_selectedStructure == null || !_selectedStructure.IsOpening) return;

            if (double.TryParse(txtFrameWidth.Text, out double val) && val > 0)
            {
                _selectedStructure.FrameWidthMm = val;
                CalculateUValue(_selectedStructure);
                RefreshDisplay();
            }
        }

        private void TxtOpeningGeometry_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingDisplay) return;
            if (_selectedStructure == null || !_selectedStructure.IsOpening) return;

            if (double.TryParse(txtOpeningWidth.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double w) && w > 0)
            {
                _selectedStructure.OpeningWidthM = w;
            }
            if (double.TryParse(txtOpeningHeight.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double h) && h > 0)
            {
                _selectedStructure.OpeningHeightM = h;
            }

            CalculateUValue(_selectedStructure);
            RefreshDisplay();
        }

        private void CalculateUValue(UserStructure structure)
        {
            var registry = ServiceLocator.EngineeringRuleRegistry;
            var result = _calculationService.Calculate(structure, registry, "Hungary.EKM");
            
            structure.LastResult = result.Result;

            if (!result.Succeeded)
            {
                foreach (var diag in result.Diagnostics)
                {
                    if (diag.Severity == CalculationDiagnosticSeverity.Error)
                    {
                        EngineeringNotificationService.Danger("Számítási hiba", diag.Message);
                    }
                    else if (diag.Severity == CalculationDiagnosticSeverity.Warning)
                    {
                        EngineeringNotificationService.Warning("Számítási figyelmeztetés", diag.Message);
                    }
                }
            }
        }

        private void RefreshDisplay()
        {
            _isUpdatingDisplay = true;
            if (_selectedStructure == null)
            {
                cardCenter.Enabled = false;
                cardRight.Enabled = false;
                pnlOpeningEditor.Visible = false;
                pnlRightHeader.Visible = true;
                gridLayers.Visible = true;
                pnlRightFooter.Visible = true;
                
                gridLayers.Rows.Clear();
                lblResultU.Text = "Alap U-érték: -\nHőhídkorrekció: -\nKorrigált U-érték: - W/m²K";
                lblResultLimit.Text = "ÉKM határérték: -";
                lblResultStatus.Text = "Státusz: -";
                lblResultStatus.ForeColor = Color.Gray;
                
                UpdateStructureCards();
                _isUpdatingDisplay = false;
                return;
            }

            // Gombok szövege a piszkozat státusztól függően
            string saveBtnText = _isEditingDraft ? "Szerkezet hozzáadása" : "Módosítás mentése";
            btnSaveStructure.Text = saveBtnText;
            btnSaveOpeningStructure.Text = saveBtnText;

            // Kapcsoljuk a megfelelő hasábszerkesztőt
            if (_selectedStructure.IsOpening)
            {
                cardCenter.Enabled = false;
                
                pnlRightHeader.Visible = false;
                gridLayers.Visible = false;
                pnlRightFooter.Visible = false;
                pnlOpeningEditor.Visible = true;

                rbOpeningCatalog.Checked = !_selectedStructure.IsOpeningCalculated;
                rbOpeningCalculate.Checked = _selectedStructure.IsOpeningCalculated;
                cmbOpeningCatalogItem.Visible = !_selectedStructure.IsOpeningCalculated;
                pnlOpeningCalculationDetails.Visible = _selectedStructure.IsOpeningCalculated;

                if (!_selectedStructure.IsOpeningCalculated)
                {
                    var op = _openingsCatalog.Items.Values.FirstOrDefault(o => o.Id == _selectedStructure.OpeningCatalogId);
                    if (op != null)
                    {
                        cmbOpeningCatalogItem.SelectedItem = op.DisplayName;
                    }
                }
                else
                {
                    var glazing = _calculationService.Glazings.FirstOrDefault(g => g.Id == _selectedStructure.GlazingId);
                    var frame = _calculationService.Frames.FirstOrDefault(f => f.Id == _selectedStructure.FrameId);
                    var spacer = _calculationService.Spacers.FirstOrDefault(s => s.Id == _selectedStructure.SpacerId);

                    if (glazing != null) cmbGlazing.SelectedItem = glazing.Name;
                    if (frame != null) cmbFrame.SelectedItem = frame.Name;
                    if (spacer != null) cmbSpacer.SelectedItem = spacer.Name;
                    txtFrameWidth.Text = _selectedStructure.FrameWidthMm.ToString("F0");
                    txtOpeningWidth.Text = _selectedStructure.OpeningWidthM.ToString("F2", CultureInfo.InvariantCulture);
                    txtOpeningHeight.Text = _selectedStructure.OpeningHeightM.ToString("F2", CultureInfo.InvariantCulture);
                }

                // Nyílászáró eredmények kiírása
                var result = _selectedStructure.LastResult as OpeningUValueResult;
                if (result != null)
                {
                    lblOpeningResultU.Text = $"Eredő U-érték: {result.UValue:F3} W/m²K";
                    if (result.EkmLimit.HasValue)
                    {
                        lblOpeningResultLimit.Text = $"ÉKM határérték: {result.EkmLimit.Value:F2} W/m²K";
                        if (result.ExceedsEkmLimit)
                        {
                            lblOpeningResultStatus.Text = "Státusz: NEM MEGFELELŐ";
                            lblOpeningResultStatus.ForeColor = Color.Red;
                        }
                        else
                        {
                            lblOpeningResultStatus.Text = "Státusz: MEGFELELŐ";
                            lblOpeningResultStatus.ForeColor = Color.Green;
                        }
                    }
                    else
                    {
                        lblOpeningResultLimit.Text = "ÉKM határérték: Nincs előírva";
                        lblOpeningResultStatus.Text = "Státusz: -";
                        lblOpeningResultStatus.ForeColor = Color.Gray;
                    }
                }
                else
                {
                    lblOpeningResultU.Text = "Eredő U-érték: - W/m²K";
                    lblOpeningResultLimit.Text = "ÉKM határérték: -";
                    lblOpeningResultStatus.Text = "Státusz: -";
                    lblOpeningResultStatus.ForeColor = Color.Gray;
                }
            }
            else
            {
                cardCenter.Enabled = true;
                
                pnlOpeningEditor.Visible = false;
                pnlRightHeader.Visible = true;
                gridLayers.Visible = true;
                pnlRightFooter.Visible = true;

                // 1. Táblázat kitöltése
                gridLayers.Rows.Clear();
                int count = _selectedStructure.Layers.Count;
                for (int i = 0; i < count; i++)
                {
                    var layer = _selectedStructure.Layers[i];
                    
                    string orderStr = (i + 1).ToString();
                    if (i == 0) orderStr += " (belső)";
                    if (i == count - 1) orderStr += " (külső)";

                    string thicknessStr = layer.IsAirLayer ? "-" : layer.ThicknessM.ToString("F3");
                    
                    double r = layer.IsAirLayer 
                        ? layer.DesignLambda
                        : (layer.ThicknessM / layer.DesignLambda);

                    gridLayers.Rows.Add(orderStr, layer.DisplayName, thicknessStr, r.ToString("F4"));
                }

                // 2. Értékek betöltése a szerkesztőbe
                if (_selectedStructure.ThermalBridgeOptionIndex >= 0 && _selectedStructure.ThermalBridgeOptionIndex < cmbThermalBridgeType.Items.Count)
                {
                    cmbThermalBridgeType.SelectedIndex = _selectedStructure.ThermalBridgeOptionIndex;
                }
                else
                {
                    cmbThermalBridgeType.SelectedIndex = 0;
                }
                
                txtThermalBridge.Text = _selectedStructure.ThermalBridgeCorrectionFactor.ToString("F2");

                // 3. Eredmények kijelzése
                var result = _selectedStructure.LastResult as OpaqueConstructionUValueResult;
                if (result != null)
                {
                    double finalU = result.UValue;

                    lblResultU.Text = $"Alap U-érték: {result.BaseUValue:F3} W/m²K\nHőhídkorrekció: {(result.ThermalBridgeCorrectionFactor * 100):F0}%\nKorrigált U-érték: {finalU:F3} W/m²K";
                    
                    if (result.EkmLimit.HasValue)
                    {
                        lblResultLimit.Text = $"ÉKM határérték: {result.EkmLimit.Value:F2} W/m²K";
                        
                        if (result.ExceedsEkmLimit)
                        {
                            lblResultStatus.Text = "Státusz: NEM MEGFELELŐ";
                            lblResultStatus.ForeColor = Color.Red;
                        }
                        else
                        {
                            lblResultStatus.Text = "Státusz: MEGFELELŐ";
                            lblResultStatus.ForeColor = Color.Green;
                        }
                    }
                    else
                    {
                        lblResultLimit.Text = "ÉKM határérték: Nincs előírva";
                        lblResultStatus.Text = "Státusz: -";
                        lblResultStatus.ForeColor = Color.Gray;
                    }
                }
                else
                {
                    lblResultU.Text = "Alap U-érték: -\nHőhídkorrekció: -\nKorrigált U-érték: - W/m²K";
                    lblResultLimit.Text = "ÉKM határérték: -";
                    lblResultStatus.Text = "Státusz: -";
                    lblResultStatus.ForeColor = Color.Gray;
                }
            }

            // 4. Bal oldali lista frissítése
            UpdateStructureCards();
            _isUpdatingDisplay = false;
        }

        private void UpdateStructureCards()
        {
            pnlStructuresList.SuspendLayout();
            pnlStructuresList.Controls.Clear();

            foreach (var structModel in _structures)
            {
                var result = structModel.LastResult;
                string valStr = "-";
                string limitStr = "";
                EngineeringResultStatus status = EngineeringResultStatus.Neutral;
                EngineeringAiSupportLevel aiLevel = EngineeringAiSupportLevel.RuleCheck;

                if (result != null)
                {
                    double finalU = result.UValue;
                        
                    valStr = finalU.ToString("F3");
                    
                    if (result.EkmLimit.HasValue)
                    {
                        limitStr = $"Limit: {result.EkmLimit.Value:F2}";
                        status = result.ExceedsEkmLimit ? EngineeringResultStatus.Danger : EngineeringResultStatus.Success;
                    }

                    if (result.HasWarning || status == EngineeringResultStatus.Danger)
                    {
                        aiLevel = EngineeringAiSupportLevel.Recommendation;
                    }
                }

                var cardModel = new EngineeringResultCardModel(
                    structModel.Name,
                    valStr,
                    "W/m²K",
                    status,
                    structModel.IsOpening ? "Nyílászáró" : GetTypeNameHu(structModel.Type),
                    limitStr,
                    "ISO 6946 / ÉKM",
                    result?.WarningMessage ?? "",
                    aiLevel
                );

                var card = new EngineeringResultCard
                {
                    Model = cardModel,
                    Width = pnlStructuresList.Width - 10,
                    Margin = new Padding(0, 0, 0, 8)
                };

                if (structModel == _selectedStructure && !_isEditingDraft)
                {
                    card.BackColor = ThemeManager.CurrentPalette.SurfaceAlt;
                }

                card.Click += (s, e) => {
                    _selectedStructure = structModel;
                    _isEditingDraft = false;
                    RefreshDisplay();
                };

                pnlStructuresList.Controls.Add(card);
            }
            pnlStructuresList.ResumeLayout();
        }

        private ConstructionType TranslateType(string text)
        {
            switch (text)
            {
                case "Külső fal": return ConstructionType.ExternalWall;
                case "Tető / Lapostető": return ConstructionType.Roof;
                case "Födém / Mennyezet": return ConstructionType.Ceiling;
                case "Padló (talajon fekvő)": return ConstructionType.GroundFloor;
                case "Pincefal": return ConstructionType.BasementWall;
                case "Belső fal (elválasztó)": return ConstructionType.InternalWall;
                default: return ConstructionType.Custom;
            }
        }

        private string GetTypeNameHu(ConstructionType type)
        {
            switch (type)
            {
                case ConstructionType.ExternalWall: return "Külső fal";
                case ConstructionType.Roof: return "Tető / Lapostető";
                case ConstructionType.Ceiling: return "Födém / Mennyezet";
                case ConstructionType.GroundFloor: return "Padló (talajon fekvő)";
                case ConstructionType.BasementWall: return "Pincefal";
                case ConstructionType.InternalWall: return "Belső fal";
                default: return "Egyedi szerkezet";
            }
        }

    }
}