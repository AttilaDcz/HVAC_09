using System;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.Services.Export.Pdf;

namespace HVACDesigner.CoreUI.Components.Dialogs
{
    public sealed class PdfExportOptionsDialog : Form, IThemeable
    {
        private readonly EngineeringCheckBox cbProject;
        private readonly EngineeringCheckBox cbDesigner;
        private readonly EngineeringCheckBox cbFixtures;
        private readonly EngineeringCheckBox cbInputs;
        private readonly EngineeringCheckBox cbResults;
        private readonly EngineeringCheckBox cbStandards;
        private readonly EngineeringCheckBox cbNotes;
        private readonly EngineeringCheckBox cbSignature;
        private readonly EngineeringCheckBox cbFooter;
        private readonly EngineeringCheckBox cbPageNumbers;
        private readonly EngineeringCheckBox cbDate;
        private readonly EngineeringCheckBox cbVersion;
        private readonly RadioButton rbA4;
        private readonly RadioButton rbA3;
        private readonly RadioButton rbPortrait;
        private readonly RadioButton rbLandscape;
        private readonly TextBox notesBox;
        private readonly EngineeringButton createButton;
        private readonly EngineeringButton cancelButton;

        private ThemePalette palette = ThemeManager.CurrentPalette;

        public PdfExportOptionsDialog(PdfExportOptions options)
        {
            Options = (options ?? new PdfExportOptions()).Clone();

            Text = "PDF beállítások";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(660, 620);
            Padding = new Padding(1);

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                Tag = "NoTheme"
            };

            Label title = new Label
            {
                Text = "PDF beállítások",
                Location = new Point(58, 13),
                Size = new Size(560, 24),
                Font = ThemeFonts.Section
            };

            Label subtitle = new Label
            {
                Text = "Válaszd ki, mely részek kerüljenek a számítási jegyzőkönyvbe.",
                Location = new Point(58, 39),
                Size = new Size(560, 24),
                Font = ThemeFonts.Caption
            };

            PictureBox icon = new PictureBox
            {
                Location = new Point(20, 22),
                Size = new Size(24, 24),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = HvacIconRenderer.RenderOutline(
                    HvacIconKind.SaveProject,
                    ThemeManager.CurrentThemeMode,
                    24,
                    palette.Info)
            };

            header.Controls.AddRange(new Control[] { icon, title, subtitle });

            Panel body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                Tag = "NoTheme"
            };

            cbProject = AddCheck(body, "Projektadatok", 20, 20, Options.IncludeProjectData);
            cbDesigner = AddCheck(body, "Tervező adatai", 20, 54, Options.IncludeDesignerData);
            cbFixtures = AddCheck(body, "Berendezések", 20, 88, Options.IncludeFixtures);
            cbInputs = AddCheck(body, "Számítási adatok", 20, 122, Options.IncludeCalculationInputs);
            cbResults = AddCheck(body, "Eredmények", 20, 156, Options.IncludeResults);
            cbStandards = AddCheck(body, "Alkalmazott szabványok", 20, 190, Options.IncludeStandards);
            cbNotes = AddCheck(body, "Megjegyzések", 20, 224, Options.IncludeNotes);
            cbSignature = AddCheck(body, "Aláírás", 20, 258, Options.IncludeSignature);

            cbFooter = AddCheck(body, "Lábléc", 330, 20, Options.IncludeFooter);
            cbPageNumbers = AddCheck(body, "Oldalszám", 330, 54, Options.IncludePageNumbers);
            cbDate = AddCheck(body, "Dátum", 330, 88, Options.IncludeDate);
            cbVersion = AddCheck(body, "Program verzió", 330, 122, Options.IncludeProgramVersion);

            body.Controls.Add(AddLabel("Papírméret", 330, 170));
            rbA4 = AddRadio(body, "A4", 330, 198, Options.PaperSize == PdfPaperSize.A4);
            rbA3 = AddRadio(body, "A3", 400, 198, Options.PaperSize == PdfPaperSize.A3);

            body.Controls.Add(AddLabel("Tájolás", 330, 244));
            rbPortrait = AddRadio(body, "Álló", 330, 272, Options.Orientation == PdfPageOrientation.Portrait);
            rbLandscape = AddRadio(body, "Fekvő", 400, 272, Options.Orientation == PdfPageOrientation.Landscape);

            body.Controls.Add(AddLabel("Megjegyzés", 20, 332));
            notesBox = new TextBox
            {
                Location = new Point(20, 358),
                Size = new Size(590, 96),
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                Text = Options.Notes
            };
            body.Controls.Add(notesBox);

            Panel footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                Tag = "NoTheme"
            };

            createButton = new EngineeringButton
            {
                Text = "PDF létrehozása",
                Variant = EngineeringButtonVariant.Primary,
                Location = new Point(408, 14),
                Size = new Size(140, ThemeMetrics.ButtonHeight)
            };
            createButton.Click += (_, _) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            cancelButton = new EngineeringButton
            {
                Text = "Mégse",
                Variant = EngineeringButtonVariant.Secondary,
                Location = new Point(554, 14),
                Size = new Size(86, ThemeMetrics.ButtonHeight)
            };
            cancelButton.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            footer.Controls.AddRange(new Control[] { createButton, cancelButton });

            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);

            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            ApplyTheme(palette);
        }

        public PdfExportOptions Options { get; private set; }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.Border;

            foreach (Control control in Controls)
                ApplyThemeTo(control);

            createButton.ApplyTheme(palette);
            cancelButton.ApplyTheme(palette);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
                CaptureOptions();

            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;

            base.Dispose(disposing);
        }

        private void CaptureOptions()
        {
            Options.IncludeProjectData = cbProject.Checked;
            Options.IncludeDesignerData = cbDesigner.Checked;
            Options.IncludeFixtures = cbFixtures.Checked;
            Options.IncludeCalculationInputs = cbInputs.Checked;
            Options.IncludeResults = cbResults.Checked;
            Options.IncludeStandards = cbStandards.Checked;
            Options.IncludeNotes = cbNotes.Checked;
            Options.IncludeSignature = cbSignature.Checked;
            Options.IncludeFooter = cbFooter.Checked;
            Options.IncludePageNumbers = cbPageNumbers.Checked;
            Options.IncludeDate = cbDate.Checked;
            Options.IncludeProgramVersion = cbVersion.Checked;
            Options.PaperSize = rbA3.Checked ? PdfPaperSize.A3 : PdfPaperSize.A4;
            Options.Orientation = rbLandscape.Checked
                ? PdfPageOrientation.Landscape
                : PdfPageOrientation.Portrait;
            Options.Notes = notesBox.Text.Trim();
        }

        private void ThemeManager_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            if (!IsDisposed)
                ApplyTheme(e.Palette);
        }

        private void ApplyThemeTo(Control control)
        {
            if (control is IThemeable themeable)
            {
                themeable.ApplyTheme(palette);
            }
            else if (control is Label label)
            {
                label.BackColor = Color.Transparent;
                label.ForeColor = palette.TextPrimary;
            }
            else if (control is RadioButton radio)
            {
                radio.BackColor = palette.Surface;
                radio.ForeColor = palette.TextPrimary;
            }
            else if (control is TextBox textBox)
            {
                textBox.BackColor = palette.SurfaceAlt;
                textBox.ForeColor = palette.TextPrimary;
            }
            else if (control is Panel panel)
            {
                panel.BackColor = palette.Surface;
            }

            foreach (Control child in control.Controls)
                ApplyThemeTo(child);
        }

        private static EngineeringCheckBox AddCheck(
            Control parent,
            string text,
            int left,
            int top,
            bool isChecked)
        {
            var check = new EngineeringCheckBox
            {
                Text = text,
                Checked = isChecked,
                Location = new Point(left, top),
                Size = new Size(260, 28)
            };
            parent.Controls.Add(check);
            return check;
        }

        private static Label AddLabel(string text, int left, int top)
        {
            return new Label
            {
                Text = text,
                Location = new Point(left, top),
                Size = new Size(240, 22),
                Font = ThemeFonts.Caption
            };
        }

        private static RadioButton AddRadio(
            Control parent,
            string text,
            int left,
            int top,
            bool isChecked)
        {
            var radio = new RadioButton
            {
                Text = text,
                Checked = isChecked,
                Location = new Point(left, top),
                Size = new Size(80, 24)
            };
            parent.Controls.Add(radio);
            return radio;
        }
    }
}
