using System.Reflection;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Stylers;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Data
{
    public class HVACDataGridView : DataGridView, IThemeable
    {
        public HVACDataGridView()
        {
            InitializeBehavior();
            ApplyTheme(ThemeManager.CurrentPalette);
        }

        private void InitializeBehavior()
        {
            SuspendLayout();

            try
            {
                //-----------------------------------
                // Általános viselkedés
                //-----------------------------------

                BorderStyle = BorderStyle.None;

                EnableDoubleBuffering();

                //-----------------------------------
                // Sorok
                //-----------------------------------

                RowHeadersVisible = false;

                AllowUserToAddRows = false;
                AllowUserToDeleteRows = false;
                AllowUserToResizeRows = false;

                MultiSelect = false;

                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect;

                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill;

                //-----------------------------------
                // Oszlopfejlécek
                //-----------------------------------

                ColumnHeadersHeight = 34;

                ColumnHeadersBorderStyle =
                    DataGridViewHeaderBorderStyle.Single;

                ColumnHeadersDefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;

                //-----------------------------------
                // Cellák
                //-----------------------------------

                DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleLeft;

                CellBorderStyle =
                    DataGridViewCellBorderStyle.SingleHorizontal;

                RowTemplate.Height = 30;
            }
            finally
            {
                ResumeLayout();
            }
        }

        public void ApplyTheme(ThemePalette palette)
        {
            ControlStyler.ApplyDataGridStyle(this, palette);
        }

        /// <summary>
        /// Bekapcsolja a DataGridView belső kettős
        /// pufferelését a villódzás csökkentésére.
        /// </summary>
        private void EnableDoubleBuffering()
        {
            typeof(DataGridView)
                .GetProperty(
                    "DoubleBuffered",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic)
                ?.SetValue(this, true);
        }
    }
}
