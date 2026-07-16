using HVACDesigner.Calculations.Air;
using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Theme; // Az új téma névtér
using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace HVACDesigner.CoreUI.Workspace.Air
{
    public partial class AirDuctsizeControl : UserControl
    {

        #region Konstruktor

        /// <summary>
        /// A légcsatorna méretező felület létrehozása.
        /// </summary>
        public AirDuctsizeControl()
        {
            InitializeComponent();
            this.BackColor = ThemeManager.CurrentPalette.Window;
            InitializeControl();
        }

        #endregion

        #region Inicializálás

        /// <summary>
        /// A kezelőfelület alapállapotának beállítása.
        /// Ez egyszer fut le a panel betöltésekor.
        /// </summary>
        private void InitializeControl()
        {
            rbCircular.Checked = true;
            rbRoundUp.Checked = true;
            rbHeight.Checked = true;

            tbVelocity.ApplyTheme(ThemeManager.CurrentPalette);
            UpdateVelocityLabel();
            UpdateInterface();
            UpdateRectangularInput();

        }

        #endregion

        #region Felület frissítése

        /// <summary>
        /// A kiválasztott keresztmetszetnek megfelelő kezelőelemek megjelenítése.
        /// </summary>
        private void UpdateInterface()
        {
            bool circular = rbCircular.Checked;

            // --- Téglalap beviteli rész (CardPanel) ---
            var rectPanel = this.Controls.Find("gbRectangularInput_card", true);
            if (rectPanel.Length > 0)
                rectPanel[0].Visible = !circular;

            // --- Kör eredmények ---
            lblDiameter.Visible = circular;
            txtDiameter.Visible = circular;

            // --- Téglalap eredmények ---
            lblWidth.Visible = !circular;
            txtWidthReal.Visible = !circular;

            lblHeight.Visible = !circular;
            txtHeightReal.Visible = !circular;
        }

        private void UpdateRectangularInput()
        {
            bool widthSelected = rbWidth.Checked;

            // Szélesség megadása
            txtWidth.Visible = widthSelected;
            txtHeight.Visible = !widthSelected;

            // Feliratok
            //label3.Visible = widthSelected;
            //label2.Visible = !widthSelected;
        }

        #endregion

        #region Felület frissítése

        /// <summary>
        /// A TrackBar aktuális értékének kiírása.
        /// </summary>
        private void UpdateVelocityLabel()
        {
            lblVelocityTitle.Text =
                tbVelocity.DecimalValue.ToString("F1") + " m/s";
        }


        #endregion

        #region Adatok beolvasása

        /// <summary>
        /// Térfogatáram beolvasása.
        /// </summary>
        private double ReadFlow()
        {
            return double.Parse(txtFlow.Text);
        }

        /// <summary>
        /// A TrackBar alapján kiválasztott sebesség.
        /// </summary>
        private double ReadVelocity()
        {
            return tbVelocity.Value;
        }

        /// <summary>
        /// A megadott szélesség beolvasása.
        /// </summary>
        private double ReadWidth()
        {
            return double.Parse(txtWidth.Text);
        }

        /// <summary>
        /// A megadott magasság beolvasása.
        /// </summary>
        private double ReadHeight()
        {
            return double.Parse(txtHeight.Text);
        }

        #endregion

        #region Számítás

        /// <summary>
        /// Kör légcsatorna méretezése.
        /// </summary>
        private void CalculateCircular()
        {
            double flow = ReadFlow();
            double velocity = ReadVelocity();

            // Elméleti átmérő
            double diameter =
                AirCalculations.CalculateCircularDiameter(flow, velocity);

            // Kerekítés
            diameter = AirCalculations.RoundCircular(
                diameter,
                rbRoundUp.Checked,
                rbRoundDown.Checked);

            // Valós sebesség
            double realVelocity =
                AirCalculations.CalculateVelocityCircular(
                    diameter,
                    flow);

            ShowCircularResult(diameter, realVelocity);
        }

        /// <summary>
        /// Téglalap légcsatorna méretezése.
        /// </summary>
        private void CalculateRectangular()
        {
            double flow = ReadFlow();
            double velocity = ReadVelocity();

            double width;
            double height;

            if (rbWidth.Checked)
            {
                width = ReadWidth();

                height =
                    AirCalculations.CalculateRectangularHeight(
                        flow,
                        velocity,
                        width);

                height = AirCalculations.RoundRectangular(
                    height,
                    rbRoundUp.Checked,
                    rbRoundDown.Checked);
            }
            else
            {
                height = ReadHeight();

                width =
                    AirCalculations.CalculateRectangularWidth(
                        flow,
                        velocity,
                        height);

                width = AirCalculations.RoundRectangular(
                    width,
                    rbRoundUp.Checked,
                    rbRoundDown.Checked);
            }

            double realVelocity =
                AirCalculations.CalculateVelocityRectangular(
                    width,
                    height,
                    flow);

            ShowRectangularResult(width, height, realVelocity);
        }

        #endregion

        #region Eredmények

        /// <summary>
        /// Kör légcsatorna eredményeinek megjelenítése.
        /// </summary>
        private void ShowCircularResult(
            double diameter,
            double velocity)
        {
            txtDiameter.Text = diameter.ToString("F0");

            txtVelocityReal.Text = velocity.ToString("F2");
        }

        /// <summary>
        /// Téglalap légcsatorna eredményeinek megjelenítése.
        /// </summary>
        private void ShowRectangularResult(
            double width,
            double height,
            double velocity)
        {
            txtWidthReal.Text = width.ToString("F0");

            txtHeightReal.Text = height.ToString("F0");

            txtVelocityReal.Text = velocity.ToString("F2");
        }

        #endregion

        #region Eseménykezelők

        /// <summary>
        /// Kör keresztmetszet kiválasztása.
        /// </summary>
        private void rbCircular_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateInterface();
        }

        /// <summary>
        /// Téglalap keresztmetszet kiválasztása.
        /// </summary>
        private void rbRectangular_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateInterface();
        }

        /// <summary>
        /// Sebesség csúszka mozgatása.
        /// </summary>
        private void tbVelocity_Scroll(object? sender, EventArgs e)
        {
            UpdateVelocityLabel();
        }
        /// <summary>
        /// Szélesség megadása.
        /// </summary>
        private void rbWidht_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateRectangularInput();
        }

        /// <summary>
        /// Magasság megadása.
        /// </summary>
        private void rbHeight_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateRectangularInput();
        }
        #endregion


        private void btnCalculate_Click(object? sender, EventArgs e)
        {
            try
            {
                if (rbCircular.Checked)
                {
                    CalculateCircular();
                }
                else
                {
                    CalculateRectangular();
                }
            }
            catch
            {
                MessageBox.Show(
                    "Hibás adat!",
                    "Figyelmeztetés",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void gbInput_Enter(object? sender, EventArgs e)
        {

        }
    }
}
