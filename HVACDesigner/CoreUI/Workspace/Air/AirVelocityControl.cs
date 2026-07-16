using System;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.Calculations.Air;
using HVACDesigner.CoreUI.Components.Results;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.Data.Models.Duct;
using HVACDesigner.Data.Providers;

namespace HVACDesigner.CoreUI.Workspace.Air
{
    public partial class AirVelocityControl : UserControl, IThemeable
    {
        private const double MinRecommendedVelocity = 1.5;
        private const double MaxRecommendedVelocity = 5.0;

        private DuctElement? _currentElement;
        private IDuctDataProvider? _dataProvider;

        public AirVelocityControl()
        {
            InitializeComponent();
            Tag = "ThemeBoundary";

            rbCircular.Checked = true;
            ArrangeModuleLayout();
            UpdateInputFieldsVisibility();
            LoadFallbackSizes();
            ApplyTheme(ThemeManager.CurrentPalette);
            ShowInitialResult();
        }

        public void ApplyTheme(ThemePalette palette)
        {
            SuspendLayout();
            try
            {
                BackColor = palette.Window;
                ForeColor = palette.TextPrimary;

                inputCard.Tag = "NoTheme";
                geometryCard.Tag = "NoTheme";
                btnCalculate.Tag = "NoTheme";
                resultCard.Tag = "NoTheme";

                ArrangeModuleLayout();

                inputCard.ApplyTheme(palette);
                geometryCard.ApplyTheme(palette);
                txtFlow.ApplyTheme(palette);
                rbCircular.ApplyTheme(palette);
                rbRectangular.ApplyTheme(palette);
                cmbDiameter.ApplyTheme(palette);
                cmbWidth.ApplyTheme(palette);
                cmbHeight.ApplyTheme(palette);
                btnCalculate.ApplyTheme(palette);
                resultCard.ApplyTheme(palette);

                UpdateInputFieldsVisibility();
                inputCard.RefreshContentLayout();
                geometryCard.RefreshContentLayout();
            }
            finally
            {
                ResumeLayout(true);
            }

            Invalidate(true);
        }

        public void InitializeDataProvider(IDuctDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            LoadStandardSizesFromXml();
        }

        private void LoadStandardSizesFromXml()
        {
            try
            {
                if (_dataProvider == null)
                    return;

                var circSizes = _dataProvider.GetCircularDuctSizes();
                var diameters = circSizes.Select(c => c.Diameter).Distinct().OrderBy(d => d).ToList();

                cmbDiameter.Items.Clear();
                foreach (var d in diameters)
                    cmbDiameter.Items.Add(d);

                var rectSizes = _dataProvider.GetRectangularDuctSizes();
                var widths = rectSizes.Select(r => r.Width).Distinct().OrderBy(w => w).ToList();
                var heights = rectSizes.Select(r => r.Height).Distinct().OrderBy(h => h).ToList();

                cmbWidth.Items.Clear();
                foreach (var w in widths)
                    cmbWidth.Items.Add(w);

                cmbHeight.Items.Clear();
                foreach (var h in heights)
                    cmbHeight.Items.Add(h);

                if (cmbDiameter.Items.Count > 0)
                    cmbDiameter.SelectedIndex = Math.Min(2, cmbDiameter.Items.Count - 1);
                if (cmbWidth.Items.Count > 0)
                    cmbWidth.SelectedIndex = Math.Min(2, cmbWidth.Items.Count - 1);
                if (cmbHeight.Items.Count > 0)
                    cmbHeight.SelectedIndex = Math.Min(1, cmbHeight.Items.Count - 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("AirVelocity XML size load error: " + ex.Message);
                LoadFallbackSizes();
            }
        }

        private void LoadFallbackSizes()
        {
            int[] fallbackDiameters = { 80, 100, 125, 150, 160, 200, 250, 315, 400 };
            int[] fallbackWidths = { 200, 250, 300, 400, 500, 600, 800 };
            int[] fallbackHeights = { 150, 200, 250, 300, 400, 500 };

            cmbDiameter.Items.Clear();
            foreach (var d in fallbackDiameters)
                cmbDiameter.Items.Add(d);

            cmbWidth.Items.Clear();
            foreach (var w in fallbackWidths)
                cmbWidth.Items.Add(w);

            cmbHeight.Items.Clear();
            foreach (var h in fallbackHeights)
                cmbHeight.Items.Add(h);

            if (cmbDiameter.Items.Count > 0)
                cmbDiameter.SelectedIndex = 2;
            if (cmbWidth.Items.Count > 0)
                cmbWidth.SelectedIndex = 2;
            if (cmbHeight.Items.Count > 0)
                cmbHeight.SelectedIndex = 1;
        }

        public void SetElement(DuctElement element)
        {
            _currentElement = element;
            if (_currentElement == null)
                return;

            if (_currentElement.Airflow > 0)
                txtFlow.ValueSi = _currentElement.Airflow / 3600.0;
        }

        private void rbCircular_CheckedChanged_1(object? sender, EventArgs e) => UpdateInputFieldsVisibility();

        private void rbRectangular_CheckedChanged_1(object? sender, EventArgs e) => UpdateInputFieldsVisibility();

        private void UpdateInputFieldsVisibility()
        {
            bool isCircular = rbCircular.Checked;

            rbCircular.Visible = true;
            rbRectangular.Visible = true;
            cmbDiameter.Visible = isCircular;
            cmbWidth.Visible = !isCircular;
            cmbHeight.Visible = !isCircular;

            rbCircular.BringToFront();
            rbRectangular.BringToFront();

            if (isCircular)
                cmbDiameter.BringToFront();
            else
            {
                cmbWidth.BringToFront();
                cmbHeight.BringToFront();
            }

            geometryCard.RefreshContentLayout();
        }

        private void ArrangeModuleLayout()
        {
            if (inputCard == null || geometryCard == null || resultCard == null)
                return;

            inputCard.Bounds = new Rectangle(24, 20, 300, 165);
            txtFlow.Bounds = new Rectangle(8, 16, 264, 58);

            geometryCard.Bounds = new Rectangle(24, 205, 300, 280);
            rbCircular.Bounds = new Rectangle(8, 16, 230, 28);
            rbRectangular.Bounds = new Rectangle(8, 52, 250, 28);
            cmbDiameter.Bounds = new Rectangle(8, 112, 180, 58);
            cmbWidth.Bounds = new Rectangle(8, 112, 128, 58);
            cmbHeight.Bounds = new Rectangle(150, 112, 128, 58);

            btnCalculate.Bounds = new Rectangle(24, 508, 300, 40);
            resultCard.Bounds = new Rectangle(350, 20, 310, resultCard.Height);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ArrangeModuleLayout();
        }

        private void btnCalculate_Click_1(object? sender, EventArgs e)
        {
            try
            {
                double? flowM3s = txtFlow.ValueSi;
                if (!flowM3s.HasValue || flowM3s.Value <= 0)
                {
                    MessageBox.Show(
                        "Kérlek, adj meg egy érvényes térfogatáramot!",
                        "Hibás adat",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                double flowM3h = flowM3s.Value * 3600.0;
                double velocity;

                if (rbCircular.Checked)
                {
                    if (cmbDiameter.SelectedItem == null)
                        return;

                    double diameterMm = Convert.ToDouble(cmbDiameter.SelectedItem);
                    velocity = AirCalculations.CalculateVelocityCircular(diameterMm, flowM3h);
                }
                else
                {
                    if (cmbWidth.SelectedItem == null || cmbHeight.SelectedItem == null)
                        return;

                    double widthMm = Convert.ToDouble(cmbWidth.SelectedItem);
                    double heightMm = Convert.ToDouble(cmbHeight.SelectedItem);
                    velocity = AirCalculations.CalculateVelocityRectangular(widthMm, heightMm, flowM3h);
                }

                if (_currentElement != null)
                {
                    _currentElement.Airflow = flowM3h;
                    _currentElement.Velocity = velocity;
                }

                UpdateResultCard(velocity);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowInitialResult()
        {
            resultCard.Model = new EngineeringResultCardModel(
                "Légsebesség",
                "-",
                "m/s",
                EngineeringResultStatus.Neutral,
                subtitle: "Válassz geometriát és légmennyiséget.",
                limitText: "Ajánlott tartomány: 1,5-5,0 m/s",
                sourceText: "Egyszerű előzetes sebességellenőrzés",
                recommendationText: "A számítás után itt jelenik meg a mérnöki visszajelzés.",
                aiLevel: EngineeringAiSupportLevel.RuleCheck);
        }

        private void UpdateResultCard(double velocity)
        {
            EngineeringResultStatus status;
            string subtitle;
            string recommendation;
            EngineeringResultDiagnosticSeverity severity;
            string diagnosticCode;

            if (velocity < MinRecommendedVelocity)
            {
                status = EngineeringResultStatus.Warning;
                subtitle = "Alacsony légsebesség";
                recommendation = "A légcsatorna a megadott légmennyiséghez nagy lehet. Ellenőrizd, nem indokolatlanul nagy-e a választott keresztmetszet.";
                severity = EngineeringResultDiagnosticSeverity.Warning;
                diagnosticCode = "LOW";
            }
            else if (velocity > MaxRecommendedVelocity)
            {
                status = EngineeringResultStatus.Danger;
                subtitle = "Magas légsebesség";
                recommendation = "A sebesség meghaladja az előzetes tartományt. Érdemes nagyobb keresztmetszetet választani vagy ellenőrizni a zaj- és nyomásveszteség-hatást.";
                severity = EngineeringResultDiagnosticSeverity.Error;
                diagnosticCode = "HIGH";
            }
            else
            {
                status = EngineeringResultStatus.Success;
                subtitle = "Megfelelő tartomány";
                recommendation = "A sebesség az előzetes általános légcsatorna-tartományon belül van. A végleges döntésnél a zajt és a nyomásveszteséget is ellenőrizni kell.";
                severity = EngineeringResultDiagnosticSeverity.Info;
                diagnosticCode = "OK";
            }

            resultCard.Model = new EngineeringResultCardModel(
                "Légsebesség",
                velocity.ToString("F2"),
                "m/s",
                status,
                subtitle: subtitle,
                limitText: "Ajánlott tartomány: 1,5-5,0 m/s",
                sourceText: rbCircular.Checked ? "Kör keresztmetszet alapján" : "Szögletes keresztmetszet alapján",
                recommendationText: recommendation,
                aiLevel: EngineeringAiSupportLevel.RuleCheck,
                diagnostics: new[]
                {
                    new EngineeringResultDiagnostic(diagnosticCode, subtitle, severity)
                },
                references: new[]
                {
                    new EngineeringResultReference("Előzetes légtechnikai ökölszabály", "sebességtartomány")
                });
        }
    }
}
