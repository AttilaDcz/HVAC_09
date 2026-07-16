using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HVACDesigner.Features.Water;
using HVACDesigner.Calculations.Water;
using HVACDesigner.CoreUI.Components.Data;
using HVACDesigner.CoreUI.Components.Dialogs;
using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Components.Help;
using HVACDesigner.CoreUI.Components.Results;
using HVACDesigner.CoreUI.Components.Structural;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Layout;
using HVACDesigner.CoreUI.Services;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.EngineeringData.SimpleCatalogs;
using HVACDesigner.Services;
using HVACDesigner.Services.Export.Pdf;
using HVACDesigner.Services.Export.Reports;
using HVACDesigner.Services.Export.Reports.Water;

namespace HVACDesigner.CoreUI.Workspace.Water
{
    public sealed class WaterDemandControl : UserControl, IThemeable
    {
        private const string ModuleDataKey = "Water.PeakDemand";

        private readonly WaterCalculationService calculationService;
        private readonly WaterResultPresentationAdapter presentation;
        private readonly List<FixtureUsage> fixtureUsages =
            new List<FixtureUsage>();
        private readonly List<EngineeringCardPanel> cards =
            new List<EngineeringCardPanel>();

        private readonly HVACScrollableContainer scrollContainer;
        private readonly Panel content;

        private Label projectName = null!;
        private Label buildingFunction = null!;
        private Label buildingProfile = null!;

        private EngineeringTextBox dwellingCount = null!;
        private EngineeringTextBox occupancy = null!;
        private EngineeringButton occupancySuggestButton = null!;
        private Label occupancySuggestion = null!;
        private WaterDailyDemandInputInfo dailyDemandInputInfo =
            new WaterDailyDemandInputInfo(
                true,
                false,
                "Létszám",
                "Létszám",
                "fő",
                null);

        private EngineeringComboBox fixtureSelector = null!;
        private EngineeringTextBox fixtureQuantity = null!;
        private EngineeringDataGridView fixtureGrid = null!;
        private EngineeringCheckBox greywaterEnabled = null!;
        private EngineeringCheckBox roofDrainageEnabled = null!;
        private EngineeringTextBox roofArea = null!;
        private EngineeringComboBox roofType = null!;
        private EngineeringTextBox rainfallIntensity = null!;
        private WaterRoofDrainageInputInfo roofDrainageInputInfo =
            new WaterRoofDrainageInputInfo(
                0.03,
                Array.Empty<WaterRoofDrainageOption>());

        private EngineeringResultCard dailyResult = null!;
        private EngineeringResultCard dhwResult = null!;
        private EngineeringResultCard peakResult = null!;
        private EngineeringResultCard wastewaterResult = null!;
        private EngineeringResultCard minimumDnResult = null!;
        private EngineeringResultCard greywaterResult = null!;
        private EngineeringResultCard roofDrainageResult = null!;
        private WaterCalculationResult? lastCalculationResult;

        private SimpleCatalog<FixtureDefinition> fixtureCatalog;
        private ThemePalette palette = ThemeManager.CurrentPalette;
        private bool resultsAreCurrent;
        private EngineeringToolTip toolTip = null!;

        public WaterDemandControl()
        {
            calculationService =
                new WaterCalculationService(
                    ServiceLocator.EngineeringDataRegistry,
                    ServiceLocator.EngineeringRuleRegistry);

            presentation =
                new WaterResultPresentationAdapter();

            fixtureCatalog =
                calculationService.GetFixtureCatalog();

            toolTip = new EngineeringToolTip();

            Dock = DockStyle.Fill;

            scrollContainer = new HVACScrollableContainer
            {
                Dock = DockStyle.Fill,
                CaptureChildMouseWheel = true
            };

            content = new Panel
            {
                Location = Point.Empty,
                Size = new Size(980, 1650)
            };

            scrollContainer.ContentControls.Add(content);
            Controls.Add(scrollContainer);

            BuildLayout();
            ApplyTheme(palette);
            LoadFixtureOptions();
            LoadRoofDrainageOptions();
            RefreshProjectData();
            LoadFromProjectData();
            SetPendingResults();

            ThemeManager.ThemeChanged +=
                ThemeManager_ThemeChanged;
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ??
                throw new ArgumentNullException(nameof(palette));

            BackColor = palette.Window;
            content.BackColor = palette.Window;
            scrollContainer.ApplyTheme(palette);
            toolTip?.ApplyTheme(palette);

            foreach (EngineeringCardPanel card in cards)
                card.ApplyTheme(palette);

            foreach (Control control in EnumerateControls(content))
            {
                if (control is IThemeable themeable)
                    themeable.ApplyTheme(palette);
            }

            occupancySuggestion.ForeColor =
                palette.TextSecondary;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.ThemeChanged -=
                    ThemeManager_ThemeChanged;
                toolTip?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void BuildLayout()
        {
            EngineeringCardPanel projectCard = CreateCard(
                "Projektadatok",
                "A globális projektadatokból átvett adatok.",
                HvacIconKind.Building,
                new Point(0, 0),
                new Size(940, 218));

            projectName = AddValueRow(
                projectCard.ContentPanel,
                "Projekt",
                14);

            buildingFunction = AddValueRow(
                projectCard.ContentPanel,
                "Épületfunkció",
                48);

            buildingProfile = AddValueRow(
                projectCard.ContentPanel,
                "Épületprofil",
                82);

            EngineeringButton editProject = new EngineeringButton
            {
                Text = "Projektadatok szerkesztése",
                Variant = EngineeringButtonVariant.Secondary,
                IconKind = HvacIconKind.ProjectProperties,
                IconPlacement = EngineeringButtonIconPlacement.Left,
                Location = new Point(650, 82),
                Size = new Size(240, ThemeMetrics.ButtonHeight)
            };
            editProject.Click += EditProject_Click;
            projectCard.ContentPanel.Controls.Add(editProject);

            EngineeringCardPanel designCard = CreateCard(
                "Tervezési alapadatok",
                "A lakók száma megadható, vagy profilalapú javaslat kérhető.",
                HvacIconKind.Info,
                new Point(0, 236),
                new Size(940, 196));

            dwellingCount = CreateIntegerInput(
                "Lakások száma",
                new Point(18, 34),
                170);

            occupancy = CreateIntegerInput(
                "Lakók száma",
                new Point(218, 34),
                170);

            occupancySuggestButton = new EngineeringButton
            {
                Text = "Javaslat kérése",
                Variant = EngineeringButtonVariant.Info,
                IconKind = HvacIconKind.Info,
                IconPlacement = EngineeringButtonIconPlacement.Left,
                Location = new Point(418, 34),
                Size = new Size(170, ThemeMetrics.ButtonHeight)
            };
            occupancySuggestButton.Click += SuggestOccupancy_Click;

            occupancySuggestion = new Label
            {
                Location = new Point(218, 100),
                Size = new Size(670, 24),
                Font = ThemeFonts.Caption,
                AutoEllipsis = true
            };

            dwellingCount.TextChanged += InputChanged;
            occupancy.TextChanged += InputChanged;

            designCard.ContentPanel.Controls.AddRange(
                new Control[]
                {
                    dwellingCount,
                    occupancy,
                    occupancySuggestButton,
                    occupancySuggestion
                });

            EngineeringCardPanel fixtureCard = CreateCard(
                "Szerelvények",
                "A katalógus és a vízügyi ruleset értékei automatikusan kerülnek a számításba.",
                HvacIconKind.WaterDemand,
                new Point(0, 450),
                new Size(940, 580));

            fixtureSelector = new EngineeringComboBox
            {
                LabelText = "Szerelvény",
                Location = new Point(18, 30),
                Size = new Size(280, 58),
                DisplayMember = nameof(FixtureOption.DisplayName),
                ValueMember = nameof(FixtureOption.Id)
            };

            fixtureQuantity = CreateIntegerInput(
                "Darabszám",
                new Point(310, 30),
                80);

            EngineeringButton add = new EngineeringButton
            {
                Text = "Hozzáadás",
                Variant = EngineeringButtonVariant.Primary,
                Location = new Point(440, 30),
                Size = new Size(130, ThemeMetrics.ButtonHeight)
            };
            add.Click += AddFixture_Click;

            EngineeringButton remove = new EngineeringButton
            {
                Text = "Elem törlése",
                Variant = EngineeringButtonVariant.Secondary,
                Location = new Point(590, 30),
                Size = new Size(130, ThemeMetrics.ButtonHeight)
            };
            remove.Click += RemoveFixture_Click;

            fixtureGrid = CreateFixtureGrid();

            greywaterEnabled = new EngineeringCheckBox
            {
                Text = "Szürkevíz mérleg számítása",
                Location = new Point(18, 360),
                Size = new Size(260, ThemeMetrics.TextBoxHeight)
            };
            greywaterEnabled.CheckedChanged += InputChanged;

            roofDrainageEnabled = new EngineeringCheckBox
            {
                Text = "Tetővíz számítása",
                Location = new Point(18, 414),
                Size = new Size(210, ThemeMetrics.TextBoxHeight)
            };
            roofDrainageEnabled.CheckedChanged += RoofDrainageEnabled_Changed;

            roofArea = CreateIntegerInput(
                "Tetőfelület [m²]",
                new Point(250, 398),
                130);

            roofType = new EngineeringComboBox
            {
                LabelText = "Tetőtípus",
                Location = new Point(410, 398),
                Size = new Size(170, 58),
                DisplayMember = nameof(WaterRoofDrainageOption.DisplayName),
                ValueMember = nameof(WaterRoofDrainageOption.Id)
            };

            rainfallIntensity = new EngineeringTextBox
            {
                LabelText = "Esőintenzitás [l/sm²]",
                QuantityKind = QuantityKind.None,
                UnitVisible = false,
                TextAlign = HorizontalAlignment.Left,
                Location = new Point(610, 398),
                Size = new Size(150, 58)
            };

            roofArea.TextChanged += InputChanged;
            roofType.SelectedIndexChanged += InputChanged;
            rainfallIntensity.TextChanged += InputChanged;

            fixtureCard.ContentPanel.Controls.AddRange(
                new Control[]
                {
                    fixtureSelector,
                    fixtureQuantity,
                    add,
                    remove,
                    fixtureGrid,
                    greywaterEnabled,
                    roofDrainageEnabled,
                    roofArea,
                    roofType,
                    rainfallIntensity
                });

            EngineeringButton calculate = new EngineeringButton
            {
                Text = "Ellenőrzés és számítás",
                Variant = EngineeringButtonVariant.Primary,
                Location = new Point(270, 1052),
                Size = new Size(250, ThemeMetrics.ButtonHeight)
            };
            calculate.Click += Calculate_Click;
            content.Controls.Add(calculate);

            EngineeringButton exportPdf = new EngineeringButton
            {
                Text = "PDF jegyzőkönyv",
                Variant = EngineeringButtonVariant.Secondary,
                IconKind = HvacIconKind.SaveProject,
                IconPlacement = EngineeringButtonIconPlacement.Left,
                Location = new Point(540, 1052),
                Size = new Size(190, ThemeMetrics.ButtonHeight)
            };
            exportPdf.Click += ExportPdf_Click;
            content.Controls.Add(exportPdf);

            EngineeringCardPanel resultsCard = CreateCard(
                "Eredmények",
                "A részszámítások külön állapotot és diagnosztikát kapnak.",
                HvacIconKind.Info,
                new Point(0, 1108),
                new Size(940, 760));

            dailyResult = CreateResultCard(new Point(18, 32));
            dhwResult = CreateResultCard(new Point(458, 32));
            peakResult = CreateResultCard(new Point(18, 206));
            wastewaterResult = CreateResultCard(new Point(458, 206));
            minimumDnResult = CreateResultCard(new Point(18, 380));
            greywaterResult = CreateResultCard(new Point(458, 380));
            roofDrainageResult = CreateResultCard(new Point(18, 554));

            resultsCard.ContentPanel.Controls.AddRange(
                new Control[]
                {
                    dailyResult,
                    dhwResult,
                    peakResult,
                    wastewaterResult,
                    minimumDnResult,
                    greywaterResult,
                    roofDrainageResult
                });

            content.Controls.AddRange(
                new Control[]
                {
                    projectCard,
                    designCard,
                    fixtureCard,
                    resultsCard
                });

            content.Height = resultsCard.Bottom + 20;
        }

        private EngineeringDataGridView CreateFixtureGrid()
        {
            var grid = new EngineeringDataGridView
            {
                Location = new Point(18, 112),
                Size = new Size(870, 230)
            };

            grid.AddTextColumn(
                "Fixture",
                "Szerelvény",
                190);

            grid.AddNumericColumn(
                "Quantity",
                "db",
                55);

            grid.AddNumericColumn(
                "LuPerUnit",
                "LU/db",
                70);

            grid.AddNumericColumn(
                "TotalLu",
                "ΣLU",
                70);

            grid.AddNumericColumn(
                "DuPerUnit",
                "DU/db",
                70);

            grid.AddNumericColumn(
                "TotalDu",
                "ΣDU",
                70);

            grid.AddNumericColumn(
                "MinimumDn",
                "Min. DN",
                75);

            return grid;
        }

        private void RefreshProjectData()
        {
            ProjectData project =
                ServiceLocator.Project.CurrentProject;

            project.NormalizeAfterLoad();

            projectName.Text =
                TextOrDash(project.Name);

            buildingFunction.Text =
                TextOrDash(
                    project.BuildingFunctionDisplayName);

            buildingProfile.Text =
                ResolveProfileDisplayName(
                    project.BuildingProfileId);

            RefreshDailyDemandInputs(project);
            InvalidateResults();
        }

        private void RefreshDailyDemandInputs(
            ProjectData project)
        {
            try
            {
                dailyDemandInputInfo =
                    calculationService.GetDailyDemandInputInfo(
                        project);
            }
            catch
            {
                dailyDemandInputInfo =
                    new WaterDailyDemandInputInfo(
                        true,
                        false,
                        "Létszám",
                        "Létszám",
                        "fő",
                        null);
            }

            dwellingCount.LabelText =
                dailyDemandInputInfo.PrimaryInputLabel;

            occupancy.LabelText =
                dailyDemandInputInfo.SecondaryInputLabel;

            bool showSecondary =
                dailyDemandInputInfo.IsPersonBased &&
                dailyDemandInputInfo.SupportsDwellingSuggestion;

            occupancy.Visible = showSecondary;
            occupancySuggestButton.Visible = showSecondary;

            if (!showSecondary)
                occupancy.Text = string.Empty;

            occupancySuggestion.Location = showSecondary
                ? new Point(218, 100)
                : new Point(18, 100);

            occupancySuggestion.Text =
                dailyDemandInputInfo.DailyWaterRate.HasValue
                    ? $"Profil szerinti fajlagos érték: {dailyDemandInputInfo.DailyWaterRate:0.###} L/{dailyDemandInputInfo.UnitLabel}/nap"
                    : "A profil nem tartalmaz napi vízigény fajlagos értéket.";

            // --- Tooltip / Súgó üzenetek beállítása ---
            if (dailyDemandInputInfo.SupportsDwellingSuggestion)
            {
                toolTip.SetHelpRecursive(
                    dwellingCount,
                    "A tervezett épületben található különálló lakások száma.",
                    "Lakások száma",
                    EngineeringToolTipKind.Info);

                toolTip.SetHelpRecursive(
                    occupancy,
                    "A lakások számából és a lakásonkénti átlagos létszámból számított, vagy manuálisan megadott összesített lakólétszám.",
                    "Létszám",
                    EngineeringToolTipKind.Info);
            }
            else
            {
                string label = dailyDemandInputInfo.PrimaryInputLabel;
                toolTip.SetHelpRecursive(
                    dwellingCount,
                    $"A napi vízigény számításának alapjául szolgáló tervezési mennyiség ({dailyDemandInputInfo.UnitLabel}-ben mérve).",
                    label,
                    EngineeringToolTipKind.Info);
            }

            toolTip.SetHelpRecursive(
                greywaterEnabled,
                "A zuhanyzókból, mosdókból és mosógépekből származó szürkevíz tisztítás utáni újrahasznosítása WC és vizelde öblítésre.",
                "Szürkevíz mérleg",
                EngineeringToolTipKind.Info);

            toolTip.SetHelpRecursive(
                roofDrainageEnabled,
                "A csapadékvíz lefolyási mértékadó hozamának számítása a tetőfelület és a helyi esőintenzitás alapján (MSZ EN 12056-3).",
                "Tetővíz hozamszámítás",
                EngineeringToolTipKind.Info);

            toolTip.SetHelpRecursive(
                roofArea,
                "A csapadék gyűjtésére szolgáló vízszintes vetületi tetőfelület alapterülete négyzetméterben.",
                "Tetőfelület",
                EngineeringToolTipKind.Info);

            toolTip.SetHelpRecursive(
                roofType,
                "A tető kialakítása, amely meghatározza a lefolyási tényezőt (pl. lapostető: 1.0, zöldtető: 0.3).",
                "Tetőtípus lefolyási tényezővel",
                EngineeringToolTipKind.Info);

            toolTip.SetHelpRecursive(
                rainfallIntensity,
                "A méretezéshez használt mértékadó esőintenzitás, l/(s*m²) egységben megadva.",
                "Mértékadó esőintenzitás",
                EngineeringToolTipKind.Info);
        }

        private void LoadFixtureOptions()
        {
            fixtureSelector.Items.Clear();

            foreach (FixtureDefinition fixture in
                fixtureCatalog.Items.Values.OrderBy(
                    item => item.DisplayName,
                    StringComparer.CurrentCultureIgnoreCase))
            {
                fixtureSelector.Items.Add(
                    new FixtureOption(
                        fixture.Id,
                        fixture.DisplayName));
            }

            if (fixtureSelector.Items.Count > 0)
                fixtureSelector.SelectedIndex = 0;
        }

        private void LoadRoofDrainageOptions()
        {
            roofDrainageInputInfo =
                calculationService.GetRoofDrainageInputInfo();

            roofType.Items.Clear();

            foreach (WaterRoofDrainageOption option in
                roofDrainageInputInfo.RoofTypes)
            {
                roofType.Items.Add(option);
            }

            if (roofType.Items.Count > 0)
                roofType.SelectedIndex = 0;

            rainfallIntensity.Text =
                roofDrainageInputInfo.DefaultRainfallIntensity.ToString(
                    "0.#####",
                    CultureInfo.CurrentCulture);

            SetRoofDrainageInputsEnabled();
        }

        private void RoofDrainageEnabled_Changed(
            object sender,
            EventArgs e)
        {
            SetRoofDrainageInputsEnabled();
            InputChanged(sender, e);
        }

        private void SetRoofDrainageInputsEnabled()
        {
            bool enabled = roofDrainageEnabled.Checked;
            roofArea.Enabled = enabled;
            roofType.Enabled = enabled;
            rainfallIntensity.Enabled = enabled;
        }

        private void SuggestOccupancy_Click(
            object sender,
            EventArgs e)
        {
            int? dwellings =
                TryReadPositiveInteger(
                    dwellingCount,
                    out int parsed)
                    ? parsed
                    : (int?)null;

            WaterOccupancySuggestion suggestion =
                calculationService.SuggestOccupancy(
                    ServiceLocator.Project.CurrentProject,
                    dwellings);

            occupancySuggestion.Text =
                suggestion.IsAvailable
                    ? $"Javaslat: {suggestion.SuggestedValue:0.##} fő · {suggestion.Explanation}"
                    : suggestion.Explanation;

            if (suggestion.IsAvailable)
            {
                occupancy.Text =
                    suggestion.SuggestedValue.ToString(
                        "0",
                        CultureInfo.CurrentCulture);

                InvalidateResults();
            }
        }

        private void AddFixture_Click(
            object sender,
            EventArgs e)
        {
            if (!(fixtureSelector.SelectedItem is
                FixtureOption option))
                return;

            if (!TryReadPositiveInteger(
                fixtureQuantity,
                out int quantity))
                return;

            FixtureUsage existing =
                fixtureUsages.FirstOrDefault(item =>
                    string.Equals(
                        item.FixtureId,
                        option.Id,
                        StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                fixtureUsages.Remove(existing);
                quantity += existing.Quantity;
            }

            fixtureUsages.Add(
                new FixtureUsage(
                    option.Id,
                    quantity));

            fixtureQuantity.Text = string.Empty;
            RefreshFixtureGrid();
            InvalidateResults();
        }

        private void RemoveFixture_Click(
            object sender,
            EventArgs e)
        {
            if (fixtureGrid.CurrentRow == null)
                return;

            WaterFixtureGridRow row =
                fixtureGrid.GetRowModel<
                    WaterFixtureGridRow>(
                    fixtureGrid.CurrentRow);

            if (row == null)
                return;

            fixtureUsages.RemoveAll(item =>
                string.Equals(
                    item.FixtureId,
                    row.FixtureId,
                    StringComparison.OrdinalIgnoreCase));

            RefreshFixtureGrid();
            InvalidateResults();
        }

        private void RefreshFixtureGrid()
        {
            fixtureGrid.Rows.Clear();

            foreach (FixtureUsage usage in fixtureUsages)
            {
                fixtureCatalog.TryGet(
                    usage.FixtureId,
                    out FixtureDefinition fixture);

                var row = new WaterFixtureGridRow(
                    usage.FixtureId,
                    fixture?.DisplayName ??
                        usage.FixtureId,
                    usage.Quantity,
                    fixture?.PotableLoadingUnit,
                    fixture?.WastewaterDu,
                    fixture?.MinimumWasteDn);

                int index = fixtureGrid.Rows.Add(
                    row.DisplayName,
                    row.Quantity,
                    Format(row.LoadingUnit),
                    row.TotalLoadingUnits.ToString("0.###"),
                    Format(row.DischargeUnit),
                    row.TotalDischargeUnits.ToString("0.###"),
                    row.MinimumDn?.ToString() ?? "-");

                fixtureGrid.SetRowModel(
                    fixtureGrid.Rows[index],
                    row);
            }
        }

        private void Calculate_Click(
            object sender,
            EventArgs e)
        {
            int? dwellings =
                TryReadPositiveInteger(
                    dwellingCount,
                    out int parsedDwellings)
                    ? parsedDwellings
                    : (int?)null;

            double? residents =
                TryReadOptionalPositiveInteger(
                    occupancy,
                    out int parsedResidents)
                    ? parsedResidents
                    : (double?)null;

            double? demandUnitCount =
                !dailyDemandInputInfo.IsPersonBased
                    ? dwellings
                    : (dailyDemandInputInfo.SupportsDwellingSuggestion
                        ? (double?)null
                        : residents ?? dwellings);

            double? roofAreaValue =
                roofDrainageEnabled.Checked &&
                TryReadPositiveDouble(
                    roofArea,
                    out double parsedRoofArea)
                    ? parsedRoofArea
                    : (double?)null;

            double? rainfallValue =
                roofDrainageEnabled.Checked &&
                TryReadPositiveDouble(
                    rainfallIntensity,
                    out double parsedRainfall)
                    ? parsedRainfall
                    : (double?)null;

            string roofTypeId =
                roofType.SelectedItem is WaterRoofDrainageOption option
                    ? option.Id
                    : string.Empty;

            try
            {
                var input = new WaterModuleInput(
                    dailyDemandInputInfo.SupportsDwellingSuggestion
                        ? dwellings
                        : null,
                    dailyDemandInputInfo.IsPersonBased
                        ? residents ?? (
                            dailyDemandInputInfo.SupportsDwellingSuggestion
                                ? null
                                : (double?)dwellings)
                        : null,
                    demandUnitCount,
                    fixtureUsages.ToArray(),
                    greywaterEnabled.Checked,
                    roofDrainageEnabled.Checked,
                    roofAreaValue,
                    roofTypeId,
                    rainfallValue);

                WaterCalculationResult result =
                    calculationService.Calculate(
                        ServiceLocator.Project.CurrentProject,
                        input);

                lastCalculationResult = result;

                dailyResult.Model =
                    presentation.CreateDailyDemand(
                        result.DailyDemand,
                        result.Rules.DailyRule);

                dhwResult.Model =
                    presentation.CreateDhwDemand(
                        result.DhwDemand,
                        result.Rules.DhwRule);

                peakResult.Model =
                    presentation.CreatePeakDemand(
                        result.PeakDemand,
                        result.Rules.PeakRule);

                wastewaterResult.Model =
                    presentation.CreateWastewater(
                        result.Wastewater,
                        result.Rules.WastewaterRule);

                minimumDnResult.Model =
                    presentation.CreateMinimumConnection(
                        result.Wastewater,
                        result.Rules.WastewaterRule);

                greywaterResult.Model =
                    presentation.CreateGreywater(
                        result.Greywater,
                        result.Rules.GreywaterRule);

                roofDrainageResult.Model =
                    presentation.CreateRoofDrainage(
                        result.RoofDrainage,
                        result.Rules.RoofDrainageRule);

                resultsAreCurrent = true;

                // Mentés: az adatok csak sikeres (vagy részlegesen sikeres) számítás után kerülnek a projektbe
                SaveToProjectData();

            }
            catch (Exception exception)
            {
                lastCalculationResult = null;

                dailyResult.Model =
                    presentation.CreateFailure(
                        "Napi vízigény",
                        exception.Message);

                dhwResult.Model =
                    presentation.CreateFailure(
                        "Használati melegvíz",
                        exception.Message);

                peakResult.Model =
                    presentation.CreateFailure(
                        "Mértékadó ivóvízhozam",
                        exception.Message);

                wastewaterResult.Model =
                    presentation.CreateFailure(
                        "Mértékadó szennyvízhozam",
                        exception.Message);

                minimumDnResult.Model =
                    presentation.CreateFailure(
                        "Bekötési minimum",
                        exception.Message);

                greywaterResult.Model =
                    presentation.CreateFailure(
                        "Szürkevíz mérleg",
                        exception.Message);

                roofDrainageResult.Model =
                    presentation.CreateFailure(
                        "Tetővíz hozam",
                        exception.Message);
            }
        }

        private void ExportPdf_Click(
            object sender,
            EventArgs e)
        {
            if (lastCalculationResult == null)
            {
                EngineeringDialog.ShowMessage(
                    FindForm(),
                    "PDF jegyzőkönyv",
                    "Előbb futtasd le a számítást, majd utána készíthető PDF jegyzőkönyv.",
                    EngineeringDialogSeverity.Warning,
                    HvacIconKind.Info);
                return;
            }

            PdfExportOptions options =
                PdfExportSettingsMapper.Load(
                    ServiceLocator.Settings.SelectedSettings,
                    ModuleKeys.WaterPeakDemand);

            using PdfExportOptionsDialog optionsDialog =
                new PdfExportOptionsDialog(options);

            if (optionsDialog.ShowDialog(FindForm()) != DialogResult.OK)
                return;

            options = optionsDialog.Options;

            using SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "PDF dokumentum (*.pdf)|*.pdf",
                Title = "Vízigény számítási jegyzőkönyv mentése",
                InitialDirectory = ResolveExportFolder(),
                FileName = BuildDefaultPdfFileName()
            };

            if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
                return;

            try
            {
                ReportContext context = new ReportContext(
                    ServiceLocator.Project.CurrentProject,
                    ServiceLocator.Settings.SelectedSettings,
                    options,
                    new ApplicationVersionService(),
                    DateTime.Now);

                WaterReportData reportData = new WaterReportData(
                    context,
                    lastCalculationResult,
                    BuildReportFixtureRows(),
                    TextOrDash(ServiceLocator.Project.CurrentProject.BuildingFunctionDisplayName),
                    ResolveProfileDisplayName(ServiceLocator.Project.CurrentProject.BuildingProfileId),
                    dwellingCount.LabelText,
                    dwellingCount.Text,
                    occupancy.Visible ? occupancy.LabelText : string.Empty,
                    occupancy.Visible ? occupancy.Text : string.Empty);

                var document = new ReportFactory()
                    .CreateWaterReportBuilder()
                    .Build(reportData);

                new PdfExportService().Export(document, dialog.FileName);

                PdfExportSettingsMapper.Save(
                    ServiceLocator.Settings.SelectedSettings,
                    ModuleKeys.WaterPeakDemand,
                    options);
                ServiceLocator.Settings.SaveSettings();

                EngineeringDialog.ShowMessage(
                    FindForm(),
                    "PDF jegyzőkönyv",
                    "A PDF jegyzőkönyv elkészült.",
                    EngineeringDialogSeverity.Success,
                    HvacIconKind.Certification);
            }
            catch (Exception exception)
            {
                EngineeringDialog.ShowMessage(
                    FindForm(),
                    "PDF jegyzőkönyv",
                    "A PDF létrehozása nem sikerült: " + exception.Message,
                    EngineeringDialogSeverity.Danger,
                    HvacIconKind.Safety);
            }
        }

        private void EditProject_Click(
            object sender,
            EventArgs e)
        {
            using var form = new ProjectPropertiesForm(
                ServiceLocator.Project.CurrentProject,
                isNewProject: false);

            if (form.ShowDialog(FindForm()) ==
                DialogResult.OK)
            {
                ProjectOperationResult saveResult =
                    ServiceLocator.Project.Save();

                if (!saveResult.Succeeded &&
                    saveResult.Status != ProjectOperationStatus.MissingFilePath)
                {
                    EngineeringDialog.ShowMessage(
                        FindForm(),
                        "Projektadatok",
                        saveResult.Message,
                        EngineeringDialogSeverity.Warning,
                        HvacIconKind.Info);
                }

                RefreshProjectData();
            }
        }

        private void InputChanged(
            object sender,
            EventArgs e)
        {
            InvalidateResults();
        }

        private void InvalidateResults()
        {
            if (resultsAreCurrent)
                SetPendingResults();
        }

        private void SetPendingResults()
        {
            dailyResult.Model =
                presentation.CreatePending(
                    "Napi vízigény");

            dhwResult.Model =
                presentation.CreatePending(
                    "Használati melegvíz");

            peakResult.Model =
                presentation.CreatePending(
                    "Mértékadó ivóvízhozam");

            wastewaterResult.Model =
                presentation.CreatePending(
                    "Mértékadó szennyvízhozam");

            minimumDnResult.Model =
                presentation.CreatePending(
                    "Bekötési minimum");

            greywaterResult.Model =
                presentation.CreatePending(
                    "Szürkevíz mérleg");

            roofDrainageResult.Model =
                presentation.CreatePending(
                    "Tetővíz hozam");

            resultsAreCurrent = false;
        }

        private string ResolveProfileDisplayName(
            string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId))
                return "-";

            if (ServiceLocator.EngineeringDataRegistry.TryGet(
                "Profiles.Building",
                "1.0",
                out SimpleCatalog<
                    BuildingProfileDefinition> profiles) &&
                profiles.TryGet(
                    profileId,
                    out BuildingProfileDefinition profile))
            {
                return profile.DisplayName;
            }

            return profileId;
        }

        private IEnumerable<WaterReportFixtureRow> BuildReportFixtureRows()
        {
            foreach (FixtureUsage usage in fixtureUsages)
            {
                fixtureCatalog.TryGet(
                    usage.FixtureId,
                    out FixtureDefinition fixture);

                double potable = fixture?.PotableLoadingUnit ?? 0.0;
                double wastewater = fixture?.WastewaterDu ?? 0.0;

                yield return new WaterReportFixtureRow(
                    fixture?.DisplayName ?? usage.FixtureId,
                    usage.Quantity,
                    fixture?.PotableLoadingUnit,
                    potable * usage.Quantity,
                    fixture?.WastewaterDu,
                    wastewater * usage.Quantity,
                    fixture?.MinimumWasteDn);
            }
        }

        private static string ResolveExportFolder()
        {
            string folder =
                ServiceLocator.Settings.SelectedSettings.Paths.ExportFolder;

            if (string.IsNullOrWhiteSpace(folder))
                folder = Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments);

            return folder;
        }

        private static string BuildDefaultPdfFileName()
        {
            string projectName =
                ServiceLocator.Project.CurrentProject.Name;

            if (string.IsNullOrWhiteSpace(projectName))
                projectName = "vizigeny";

            foreach (char invalid in Path.GetInvalidFileNameChars())
                projectName = projectName.Replace(invalid, '_');

            return projectName.Trim() +
                "_vizigeny_jegyzokonyv.pdf";
        }

        private EngineeringCardPanel CreateCard(
            string title,
            string subtitle,
            HvacIconKind icon,
            Point location,
            Size size)
        {
            var card = new EngineeringCardPanel
            {
                Title = title,
                Subtitle = subtitle,
                IconKind = icon,
                ShowIcon = true,
                ShowAccentStrip = true,
                ShowSeparator = true,
                Location = location,
                Size = size
            };

            cards.Add(card);
            return card;
        }

        private EngineeringTextBox CreateIntegerInput(
            string label,
            Point location,
            int width)
        {
            return new EngineeringTextBox
            {
                LabelText = label,
                QuantityKind = QuantityKind.None,
                UnitVisible = false,
                TextAlign = HorizontalAlignment.Left,
                Location = location,
                Size = new Size(width, 58)
            };
        }

        private EngineeringResultCard CreateResultCard(
            Point location)
        {
            return new EngineeringResultCard
            {
                Location = location,
                Size = new Size(410, 164)
            };
        }

        private Label AddValueRow(
            Control parent,
            string caption,
            int top)
        {
            var captionLabel = new Label
            {
                Text = caption,
                Location = new Point(18, top),
                Size = new Size(160, 24),
                Font = ThemeFonts.Caption
            };

            var valueLabel = new Label
            {
                Location = new Point(190, top),
                Size = new Size(430, 24),
                Font = ThemeFonts.Body,
                AutoEllipsis = true
            };

            parent.Controls.Add(captionLabel);
            parent.Controls.Add(valueLabel);
            return valueLabel;
        }

        private static bool TryReadPositiveInteger(
            EngineeringTextBox textBox,
            out int value)
        {
            value = 0;

            string rawText = textBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(rawText))
            {
                textBox.HasValidationError = true;
                return false;
            }

            bool valid =
                int.TryParse(
                    rawText,
                    NumberStyles.Integer,
                    CultureInfo.CurrentCulture,
                    out int parsed) ||
                int.TryParse(
                    rawText,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out parsed);

            valid = valid && parsed > 0;

            textBox.HasValidationError = !valid;

            if (!valid)
                return false;

            value = parsed;
            return true;
        }

        private static bool TryReadOptionalPositiveInteger(
            EngineeringTextBox textBox,
            out int value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.HasValidationError = false;
                return false;
            }

            return TryReadPositiveInteger(textBox, out value);
        }

        private static bool TryReadPositiveDouble(
            EngineeringTextBox textBox,
            out double value)
        {
            value = 0.0;

            string rawText = textBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(rawText))
            {
                textBox.HasValidationError = true;
                return false;
            }

            bool valid =
                double.TryParse(
                    rawText,
                    NumberStyles.Float,
                    CultureInfo.CurrentCulture,
                    out double parsed) ||
                double.TryParse(
                    rawText,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out parsed);

            valid = valid && parsed > 0.0;
            textBox.HasValidationError = !valid;

            if (!valid)
                return false;

            value = parsed;
            return true;
        }

        private static string Format(double? value)
        {
            return value.HasValue
                ? value.Value.ToString(
                    "0.###",
                    CultureInfo.CurrentCulture)
                : "-";
        }

        private static string TextOrDash(
            string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "-"
                : value;
        }

        private void ThemeManager_ThemeChanged(
            object sender,
            ThemeChangedEventArgs e)
        {
            ApplyTheme(e.Palette);
        }

        private static IEnumerable<Control>
            EnumerateControls(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;

                foreach (Control descendant in
                    EnumerateControls(child))
                {
                    yield return descendant;
                }
            }
        }

        // =====================================================================
        // PROJEKTADAT MENTÉS ÉS VISSZATÖLTÉS
        // =====================================================================

        private const string SchemaVersion  = "1.0";

        /// <summary>
        /// Elmenti a Water modul UI-állapotát a projektbe.
        /// Csak sikeres számítás után hívódik meg.
        /// </summary>
        private void SaveToProjectData()
        {
            ProjectData project = ServiceLocator.Project.CurrentProject;
            project.Modules ??= new System.Collections.Generic.Dictionary<string, ModuleProjectData>(
                StringComparer.OrdinalIgnoreCase);

            var data = new ModuleProjectData
            {
                ModuleId            = ModuleDataKey,
                ModuleSchemaVersion = SchemaVersion
            };

            // --- Bemeneti értékek ---
            data.Inputs["DwellingCount"] = dwellingCount.Text;
            data.Inputs["Occupancy"]     = occupancy.Text;

            // --- Beállítások ---
            data.Settings["GreywaterEnabled"]    = greywaterEnabled.Checked ? "true" : "false";
            data.Settings["RoofDrainageEnabled"] = roofDrainageEnabled.Checked ? "true" : "false";
            data.Settings["RoofArea"]            = roofArea.Text;
            data.Settings["RainfallIntensity"]   = rainfallIntensity.Text;

            if (roofType.SelectedItem is WaterRoofDrainageOption roofOption)
                data.Settings["RoofTypeId"] = roofOption.Id;

            // --- Szerelvénylista ("FixtureId|qty" soronként) ---
            var fixtureLines = new System.Text.StringBuilder();
            foreach (FixtureUsage usage in fixtureUsages)
                fixtureLines.AppendLine(usage.FixtureId + "|" + usage.Quantity.ToString(CultureInfo.InvariantCulture));
            data.Settings["Fixtures"] = fixtureLines.ToString().Trim();

            project.Modules[ModuleDataKey] = data;
        }

        /// <summary>
        /// Visszatölti a Water modul UI-állapotát a projektből (ha volt mentés korábban).
        /// </summary>
        private void LoadFromProjectData()
        {
            ProjectData project = ServiceLocator.Project.CurrentProject;

            if (project.Modules == null ||
                !project.Modules.TryGetValue(ModuleDataKey, out ModuleProjectData data))
                return;

            // --- Bemeneti értékek ---
            if (data.Inputs.TryGetValue("DwellingCount", out string dwellings) &&
                !string.IsNullOrWhiteSpace(dwellings))
                dwellingCount.Text = dwellings;

            if (data.Inputs.TryGetValue("Occupancy", out string occ) &&
                !string.IsNullOrWhiteSpace(occ))
                occupancy.Text = occ;

            // --- Beállítások ---
            if (data.Settings.TryGetValue("GreywaterEnabled", out string gw))
                greywaterEnabled.Checked = string.Equals(gw, "true", StringComparison.OrdinalIgnoreCase);

            if (data.Settings.TryGetValue("RoofDrainageEnabled", out string rd))
            {
                roofDrainageEnabled.Checked = string.Equals(rd, "true", StringComparison.OrdinalIgnoreCase);
                SetRoofDrainageInputsEnabled();
            }

            if (data.Settings.TryGetValue("RoofArea", out string area) &&
                !string.IsNullOrWhiteSpace(area))
                roofArea.Text = area;

            if (data.Settings.TryGetValue("RainfallIntensity", out string rain) &&
                !string.IsNullOrWhiteSpace(rain))
                rainfallIntensity.Text = rain;

            if (data.Settings.TryGetValue("RoofTypeId", out string roofId) &&
                !string.IsNullOrWhiteSpace(roofId))
            {
                for (int i = 0; i < roofType.Items.Count; i++)
                {
                    if (roofType.Items[i] is WaterRoofDrainageOption opt &&
                        string.Equals(opt.Id, roofId, StringComparison.OrdinalIgnoreCase))
                    {
                        roofType.SelectedIndex = i;
                        break;
                    }
                }
            }

            // --- Szerelvénylista visszatöltése ---
            if (data.Settings.TryGetValue("Fixtures", out string fixtureData) &&
                !string.IsNullOrWhiteSpace(fixtureData))
            {
                fixtureUsages.Clear();

                foreach (string line in fixtureData.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] parts = line.Split('|');
                    if (parts.Length != 2)
                        continue;

                    string fixtureId = parts[0].Trim();
                    if (!int.TryParse(
                        parts[1].Trim(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int qty) || qty <= 0)
                        continue;

                    if (fixtureCatalog.TryGet(fixtureId, out _))
                        fixtureUsages.Add(new FixtureUsage(fixtureId, qty));
                }

                RefreshFixtureGrid();
            }
        }

        private sealed class FixtureOption
        {
            public string Id { get; }
            public string DisplayName { get; }

            public FixtureOption(
                string id,
                string displayName)
            {
                Id = id;
                DisplayName = displayName;
            }

            public override string ToString()
            {
                return DisplayName;
            }
        }
    }
}
