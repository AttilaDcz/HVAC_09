using System;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Stylers
{
    public static class ControlStyler
    {
        public static void Style(Control ctrl, ThemePalette palette)
        {
            if (ctrl == null)
                return;

            if (palette == null)
                throw new ArgumentNullException(nameof(palette));

            switch (ctrl)
            {
                case DataGridView grid:
                    ApplyDataGridStyle(grid, palette);
                    break;

                case Label label:
                    label.BackColor = Color.Transparent;

                    bool isSecondary = string.Equals(
                        label.Tag?.ToString(),
                        "Secondary",
                        StringComparison.OrdinalIgnoreCase);

                    label.ForeColor = isSecondary
                        ? palette.TextSecondary
                        : palette.TextPrimary;

                    if (label.Font.Size <= 10 &&
                        label.Font.Style == FontStyle.Regular)
                    {
                        label.Font = ThemeFonts.Body;
                    }

                    break;

                case TextBox textBox:
                    textBox.Font = ThemeFonts.Body;
                    textBox.BorderStyle = BorderStyle.FixedSingle;

                    if (textBox.ReadOnly)
                    {
                        textBox.BackColor = palette.Surface;
                        textBox.ForeColor = palette.TextSecondary;
                    }
                    else
                    {
                        textBox.BackColor = palette.SurfaceAlt;
                        textBox.ForeColor = palette.TextPrimary;
                    }

                    break;

                case GroupBox groupBox:
                    groupBox.Font = ThemeFonts.Body;
                    groupBox.ForeColor = palette.TextPrimary;
                    break;

                case ComboBox comboBox:
                    comboBox.Font = ThemeFonts.Body;
                    comboBox.BackColor = palette.Surface;
                    comboBox.ForeColor = palette.TextPrimary;
                    comboBox.FlatStyle = FlatStyle.Flat;
                    break;

                case Button button:
                    button.Font = ThemeFonts.Body;
                    button.BackColor = palette.SurfaceAlt;
                    button.ForeColor = palette.TextPrimary;

                    button.FlatStyle = FlatStyle.Flat;
                    button.UseVisualStyleBackColor = false;

                    button.FlatAppearance.BorderSize = 1;
                    button.FlatAppearance.BorderColor = palette.Border;
                    button.FlatAppearance.MouseOverBackColor =
                        palette.SurfaceHover;
                    button.FlatAppearance.MouseDownBackColor =
                        palette.SurfacePressed;

                    break;

                case CheckBox checkBox:
                    checkBox.Font = ThemeFonts.Body;
                    checkBox.BackColor = Color.Transparent;
                    checkBox.ForeColor = palette.TextPrimary;
                    checkBox.FlatStyle = FlatStyle.Flat;
                    break;

                case RadioButton radioButton:
                    radioButton.Font = ThemeFonts.Body;
                    radioButton.BackColor = Color.Transparent;
                    radioButton.ForeColor = palette.TextPrimary;
                    radioButton.FlatStyle = FlatStyle.Flat;
                    break;

                case Panel panel:
                    panel.Font = ThemeFonts.Body;
                    panel.BackColor = palette.Window;
                    break;
            }
        }

        public static void ApplyDataGridStyle(
            DataGridView grid,
            ThemePalette palette)
        {
            if (grid == null)
                return;

            if (palette == null)
                throw new ArgumentNullException(nameof(palette));

            grid.SuspendLayout();

            try
            {
                grid.EnableHeadersVisualStyles = false;

                grid.BackgroundColor = palette.Window;
                grid.ForeColor = palette.TextPrimary;
                grid.GridColor = palette.Border;
                grid.Font = ThemeFonts.Body;

                grid.DefaultCellStyle.BackColor = palette.Surface;
                grid.DefaultCellStyle.ForeColor = palette.TextPrimary;
                grid.DefaultCellStyle.Font = ThemeFonts.Body;
                grid.DefaultCellStyle.SelectionBackColor =
                    palette.SurfaceSelected;
                grid.DefaultCellStyle.SelectionForeColor =
                    palette.TextPrimary;

                grid.AlternatingRowsDefaultCellStyle.BackColor =
                    palette.SurfaceAlt;
                grid.AlternatingRowsDefaultCellStyle.ForeColor =
                    palette.TextPrimary;
                grid.AlternatingRowsDefaultCellStyle.SelectionBackColor =
                    palette.SurfaceSelected;
                grid.AlternatingRowsDefaultCellStyle.SelectionForeColor =
                    palette.TextPrimary;

                grid.ColumnHeadersDefaultCellStyle.BackColor =
                    palette.SurfaceAlt;
                grid.ColumnHeadersDefaultCellStyle.ForeColor =
                    palette.TextPrimary;
                grid.ColumnHeadersDefaultCellStyle.Font =
                    ThemeFonts.BodyBold;
                grid.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                    palette.SurfaceAlt;
                grid.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                    palette.TextPrimary;

                grid.RowHeadersDefaultCellStyle.BackColor =
                    palette.SurfaceAlt;
                grid.RowHeadersDefaultCellStyle.ForeColor =
                    palette.TextPrimary;
                grid.RowHeadersDefaultCellStyle.SelectionBackColor =
                    palette.SurfaceSelected;
                grid.RowHeadersDefaultCellStyle.SelectionForeColor =
                    palette.TextPrimary;

                grid.ColumnHeadersBorderStyle =
                    DataGridViewHeaderBorderStyle.Single;
            }
            finally
            {
                grid.ResumeLayout();
                grid.Invalidate();
            }
        }
    }
}