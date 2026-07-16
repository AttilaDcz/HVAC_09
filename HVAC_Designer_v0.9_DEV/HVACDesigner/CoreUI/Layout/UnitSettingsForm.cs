using System;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.Services;

namespace HVACDesigner.CoreUI.Layout
{
    public class UnitSettingsForm : Form
    {
        // UI Kezelőelemek
        private RadioButton rbSI = null!;
        private RadioButton rbImperial = null!;

        private GroupBox gbSiDetails = null!;
        private ComboBox cmbAirFlow = null!;
        private ComboBox cmbHydroPressure = null!;
        private ComboBox cmbHydroFlowType = null!;
        private ComboBox cmbSanitaryPressure = null!;

        private Button btnSave = null!;
        private Button btnCancel = null!;

        public UnitSettingsForm()
        {
            InitializeComponent();
            LoadCurrentContextValues();
            UpdateUiState();
        }

        private void InitializeComponent()
        {
            // Ablak alapbeállításai (Modern, Keret nélküli / Fix párbeszédablak)
            this.Text = "Mértékegység Rendszer Beállításai";
            this.Size = new Size(420, 480);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(28, 32, 40); // Shell sötét háttér
            this.ForeColor = Color.White;

            Font mainFont = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            Font boldFont = new Font("Segoe UI", 10F, FontStyle.Bold);

            // 1. Globális rendszer választó (SI vs Imperial)
            GroupBox gbGlobal = new GroupBox
            {
                Text = " Globális Alaprendszer ",
                Location = new Point(20, 20),
                Size = new Size(365, 80),
                ForeColor = Color.FromArgb(0, 122, 204), // Márka kék
                Font = boldFont
            };

            rbSI = new RadioButton { Text = "SI Alapú mértékegységek", Location = new Point(20, 35), Size = new Size(160, 25), Font = mainFont, ForeColor = Color.White, Checked = true };
            rbImperial = new RadioButton { Text = "Imperial (USA)", Location = new Point(190, 35), Size = new Size(150, 25), Font = mainFont, ForeColor = Color.White };

            rbSI.CheckedChanged += (s, e) => UpdateUiState();
            gbGlobal.Controls.AddRange(new Control[] { rbSI, rbImperial });
            this.Controls.Add(gbGlobal);

            // 2. Részletes SI szakági beállítások
            gbSiDetails = new GroupBox
            {
                Text = " Részletes SI Szakági Preferenciák ",
                Location = new Point(20, 120),
                Size = new Size(365, 260),
                ForeColor = Color.FromArgb(26, 188, 156), // Sanitary türkiz
                Font = boldFont
            };

            // Légtechnikai áramlás preferenciák
            Label lblAir = new Label { Text = "Légtechnikai légáram:", Location = new Point(20, 35), Size = new Size(150, 20), Font = mainFont, ForeColor = Color.FromArgb(200, 200, 200) };
            cmbAirFlow = new ComboBox { Location = new Point(180, 32), Size = new Size(160, 25), Font = mainFont, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAirFlow.Items.AddRange(new object[] { "m³/h (Köbméter / óra)", "l/s (Liter / szekundum)" });

            // Fűtési/Hidraulikai nyomás preferenciák
            Label lblHydroPres = new Label { Text = "Hidraulikai nyomás:", Location = new Point(20, 90), Size = new Size(150, 20), Font = mainFont, ForeColor = Color.FromArgb(200, 200, 200) };
            cmbHydroPressure = new ComboBox { Location = new Point(180, 87), Size = new Size(160, 25), Font = mainFont, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbHydroPressure.Items.AddRange(new object[] { "Pa (Pascal)", "kPa (KiloPascal)", "bar (Bár)" });

            // Fűtési/Hidraulikai tömeg vs térfogatáram
            Label lblHydroFlow = new Label { Text = "Csőhálózati áramlás:", Location = new Point(20, 145), Size = new Size(150, 20), Font = mainFont, ForeColor = Color.FromArgb(200, 200, 200) };
            cmbHydroFlowType = new ComboBox { Location = new Point(180, 142), Size = new Size(160, 25), Font = mainFont, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbHydroFlowType.Items.AddRange(new object[] { "Térfogatáram (m³/h)", "Tömegáram (kg/s)" });

            // Víz-csatorna nyomás preferenciák
            Label lblSanitaryPres = new Label { Text = "Víz-Csatorna nyomás:", Location = new Point(20, 200), Size = new Size(150, 20), Font = mainFont, ForeColor = Color.FromArgb(200, 200, 200) };
            cmbSanitaryPressure = new ComboBox { Location = new Point(180, 197), Size = new Size(160, 25), Font = mainFont, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSanitaryPressure.Items.AddRange(new object[] { "bar (Mértékadó)", "mH2O (Vízoszlop méter)" });

            gbSiDetails.Controls.AddRange(new Control[] { lblAir, cmbAirFlow, lblHydroPres, cmbHydroPressure, lblHydroFlow, cmbHydroFlowType, lblSanitaryPres, cmbSanitaryPressure });
            this.Controls.Add(gbSiDetails);

            // 3. Akció gombok (Mentés / Mégse)
            btnSave = new Button
            {
                Text = "✓ Beállítások Mentése",
                Location = new Point(110, 400),
                Size = new Size(160, 32),
                Font = boldFont,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Mégse",
                Location = new Point(280, 400),
                Size = new Size(105, 32),
                Font = mainFont,
                BackColor = Color.FromArgb(50, 55, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        // Aktuális szoftverállapot beolvasása nyitáskor
        private void LoadCurrentContextValues()
        {
            try
            {
                // Beállítjuk a kiválasztott combobox indexeket a meglévő UnitContext adatok alapján
                cmbAirFlow.SelectedIndex = (UnitContext.Air.Flow == AirFlowUnit.CubicMeterPerHour) ? 0 : 1;
                cmbHydroPressure.SelectedIndex = (UnitContext.Hydraulics.Pressure == FluidPressureUnit.Pascal) ? 0 : 1;
                cmbHydroFlowType.SelectedIndex = 0; // Alapértelmezett m3/h
                cmbSanitaryPressure.SelectedIndex = 0; // Alapértelmezett bar
            }
            catch
            {
                // Fallback indexek ha üres a kontextus
                cmbAirFlow.SelectedIndex = 0;
                cmbHydroPressure.SelectedIndex = 0;
                cmbHydroFlowType.SelectedIndex = 0;
                cmbSanitaryPressure.SelectedIndex = 0;
            }
        }

        // Intelligens felületvezérlés: Ha Imperial van jelölve, az SI finomhangolást letiltjuk
        private void UpdateUiState()
        {
            bool isSiSelected = rbSI.Checked;
            gbSiDetails.Enabled = isSiSelected;

            if (isSiSelected)
            {
                gbSiDetails.ForeColor = Color.FromArgb(26, 188, 156);
            }
            else
            {
                gbSiDetails.ForeColor = Color.Gray;
            }
        }

        // Adatok visszaírása és a globális számítási lánc értesítése
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (rbImperial.Checked)
                {
                    // Ha a gépész az Imperialt választja, kényszerítjük az USA szabványokat
                    UnitContext.Air.Flow = AirFlowUnit.LitersPerSecond; // vagy CFM ha van
                    UnitContext.Hydraulics.Flow = FluidFlowUnit.LitersPerSecond;
                }
                else
                {
                    // Szakágankénti precíz SI szinkronizáció
                    UnitContext.Air.Flow = (cmbAirFlow.SelectedIndex == 0)
                        ? AirFlowUnit.CubicMeterPerHour
                        : AirFlowUnit.LitersPerSecond;

                    UnitContext.Hydraulics.Flow = (cmbHydroFlowType.SelectedIndex == 0)
                        ? FluidFlowUnit.CubicMeterPerHour
                        : FluidFlowUnit.LitersPerSecond;
                }

                // Elsütjük a globális eseményt, hogy az ÖSSZES megnyitott gépészeti modul (DuctNetwork, U-érték, stb.) azonnal frissüljön!
                UnitContext.TriggerUnitChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a mértékegységek mentése közben: " + ex.Message);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

    }
}
