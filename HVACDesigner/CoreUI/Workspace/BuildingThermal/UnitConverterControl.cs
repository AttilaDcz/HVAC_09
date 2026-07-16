using System;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.Data.Providers; // Szükséges az anyagadat-szolgáltató eléréséhez
using HVACDesigner.CoreUI.Services; // Szükséges a globális ServiceLocator eléréséhez 
using HVACDesigner.CoreUI.Theme; // Szükséges a központi palettákhoz és betűkhöz

namespace HVACDesigner.CoreUI.Workspace.BuildingThermal
{
    // A : UserControl kötelező, hogy a WinForms vizuális elemként kezelje!
    public class UnitConverterControl : UserControl
    {
        private ComboBox cmbCategory = null!;
        private TextBox txtInput = null!;
        private ComboBox cmbFromUnit = null!;
        private Label lblArrow = null!;
        private TextBox txtOutput = null!;
        private ComboBox cmbToUnit = null!;

        public UnitConverterControl()
        {
            InitializeComponent();
            SetupLayout();
            PopulateCategories();

            // Valós idejű gépelés-figyelés
            txtInput.TextChanged += (s, e) => PerformConversion();
            cmbCategory.SelectedIndexChanged += cmbCategory_SelectedIndexChanged;
            cmbFromUnit.SelectedIndexChanged += (s, e) => PerformConversion();
            cmbToUnit.SelectedIndexChanged += (s, e) => PerformConversion();
        }

        private void InitializeComponent()
        {
            // Kivesszük a fix 750-es szélességet, hogy a Form1 mondhassa meg mekkora legyen
            this.Height = 32;
            this.BackColor = Color.Transparent;
        }

        private void SetupLayout()
        {
            Font modernFont = new Font("Segoe UI", 10F, FontStyle.Regular);

            cmbCategory = new ComboBox { Size = new Size(150, 28), Location = new Point(5, 2), Font = modernFont, DropDownStyle = ComboBoxStyle.DropDownList };
            txtInput = new TextBox { Size = new Size(100, 28), Location = new Point(165, 2), Font = modernFont };
            cmbFromUnit = new ComboBox { Size = new Size(80, 28), Location = new Point(270, 2), Font = modernFont, DropDownStyle = ComboBoxStyle.DropDownList };

            lblArrow = new Label { Text = "➔", Size = new Size(30, 28), Location = new Point(355, 5), Font = new Font("Segoe UI", 11F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };

            txtOutput = new TextBox { Size = new Size(100, 28), Location = new Point(390, 2), Font = modernFont, ReadOnly = true };
            cmbToUnit = new ComboBox { Size = new Size(80, 28), Location = new Point(495, 2), Font = modernFont, DropDownStyle = ComboBoxStyle.DropDownList };

            this.Controls.AddRange(new Control[] { cmbCategory, txtInput, cmbFromUnit, lblArrow, txtOutput, cmbToUnit });

        }

        private void ApplyThemeToControl(Control c)
        {
            // AppColors helyett a gyári sötét színt vagy az új palettát használjuk
            c.BackColor = Color.FromArgb(45, 45, 48);
            c.ForeColor = Color.White;

            this.BackColor = ThemeManager.CurrentPalette.Window;
            if (c is TextBox tb) tb.BorderStyle = BorderStyle.FixedSingle;
        }

        private void PopulateCategories()
        {
            cmbCategory.Items.Add("Nyomás");
            cmbCategory.Items.Add("Légmennyiség / Hozam");
            cmbCategory.Items.Add("Teljesítmény");
            cmbCategory.SelectedIndex = 0;
        }

        private void cmbCategory_SelectedIndexChanged(object? sender, EventArgs e)
        {
            cmbFromUnit.Items.Clear();
            cmbToUnit.Items.Clear();

            switch (cmbCategory.SelectedIndex)
            {
                case 0: // Nyomás
                    string[] pressUnits = { "Pa", "kPa", "bar", "mH2O" };
                    cmbFromUnit.Items.AddRange(pressUnits);
                    cmbToUnit.Items.AddRange(pressUnits);
                    break;
                case 1: // Hozam
                    string[] flowUnits = { "m³/h", "l/s" };
                    cmbFromUnit.Items.AddRange(flowUnits);
                    cmbToUnit.Items.AddRange(flowUnits);
                    break;
                case 2: // Teljesítmény
                    string[] powerUnits = { "kW", "W", "kcal/h" };
                    cmbFromUnit.Items.AddRange(powerUnits);
                    cmbToUnit.Items.AddRange(powerUnits);
                    break;
            }

            if (cmbFromUnit.Items.Count > 0) cmbFromUnit.SelectedIndex = 0;
            if (cmbToUnit.Items.Count > 1) cmbToUnit.SelectedIndex = 1;

            PerformConversion();
        }

        private void PerformConversion()
        {
            if (string.IsNullOrEmpty(txtInput.Text))
            {
                txtOutput.Text = "";
                return;
            }

            string cleanInput = txtInput.Text.Replace(',', '.');
            if (!double.TryParse(cleanInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double inputVal))
            {
                txtOutput.Text = "Hiba";
                return;
            }

            if (cmbFromUnit.SelectedItem == null || cmbToUnit.SelectedItem == null) return;

            string fromUnit = cmbFromUnit.SelectedItem?.ToString() ?? string.Empty;
            string toUnit = cmbToUnit.SelectedItem?.ToString() ?? string.Empty;
            double result = 0;

            switch (cmbCategory.SelectedIndex)
            {
                case 0: // Nyomás (Alap: Pa)
                    double pa = fromUnit switch
                    {
                        "Pa" => inputVal,
                        "kPa" => inputVal * 1000.0,
                        "bar" => inputVal * 100000.0,
                        "mH2O" => inputVal * 9806.65,
                        _ => inputVal
                    };
                    result = toUnit switch
                    {
                        "Pa" => pa,
                        "kPa" => pa / 1000.0,
                        "bar" => pa / 100000.0,
                        "mH2O" => pa / 9806.65,
                        _ => pa
                    };
                    break;

                case 1: // Hozam (Alap: m3/h)
                    double m3h = fromUnit switch
                    {
                        "m³/h" => inputVal,
                        "l/s" => inputVal * 3.6,
                        _ => inputVal
                    };
                    result = toUnit switch
                    {
                        "m³/h" => m3h,
                        "l/s" => m3h / 3.6,
                        _ => m3h
                    };
                    break;

                case 2: // Teljesítmény (Alap: kW)
                    double kw = fromUnit switch
                    {
                        "kW" => inputVal,
                        "W" => inputVal / 1000.0,
                        "kcal/h" => inputVal * 0.001163,
                        _ => inputVal
                    };
                    result = toUnit switch
                    {
                        "kW" => kw,
                        "W" => kw * 1000.0,
                        "kcal/h" => kw / 0.001163,
                        _ => kw
                    };
                    break;
            }

            txtOutput.Text = result.ToString("G5", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
