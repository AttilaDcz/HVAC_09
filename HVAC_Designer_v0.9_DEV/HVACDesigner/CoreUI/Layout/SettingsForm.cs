using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Dialogs;
using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Components.Structural;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.Services;

namespace HVACDesigner.CoreUI.Layout
{
    public class SettingsForm : Form, IThemeable
    {
        private readonly EngineeringTabHost tabHost;
        private readonly EngineeringUnitSelector unitSelector;
        private readonly EngineeringButton btnSave;
        private readonly EngineeringButton btnCancel;
        private readonly EngineeringButton btnDefaults;
        private readonly EngineeringButton btnApply;
        private readonly List<EngineeringCardPanel> cards = new List<EngineeringCardPanel>();

        private EngineeringTextBox txtDefName = null!;
        private EngineeringTextBox txtDefChamber = null!;
        private EngineeringTextBox txtDefCompany = null!;
        private EngineeringTextBox txtDefPhone = null!;
        private EngineeringTextBox txtDefEmail = null!;
        private EngineeringTextBox txtDefZip = null!;
        private EngineeringTextBox txtDefCity = null!;
        private EngineeringTextBox txtDefStreet = null!;
        private EngineeringTextBox txtDefHouse = null!;
        private EngineeringComboBox cmbDefStreetType = null!;

        private EngineeringComboBox cmbTheme = null!;
        private EngineeringTextBox txtAutoSave = null!;
        private EngineeringCheckBox chkDebugViews = null!;
        private EngineeringCheckBox chkBootstrapDiagnostics = null!;
        private EngineeringCheckBox chkVerboseNotifications = null!;

        private EngineeringTextBox txtAutoSavePath = null!;
        private EngineeringTextBox txtLogFolder = null!;
        private EngineeringTextBox txtCacheFolder = null!;
        private EngineeringTextBox txtUserXmlFolder = null!;
        private EngineeringTextBox txtExportFolder = null!;

        private ThemePalette palette = ThemeManager.CurrentPalette;

        public SettingsForm()
        {
            Text = "Beállítások";
            Size = new Size(900, 700);
            MinimumSize = new Size(820, 620);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = ThemeFonts.Body;

            tabHost = new EngineeringTabHost
            {
                Location = new Point(18, 18),
                Size = new Size(ClientSize.Width - 36, ClientSize.Height - 86),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ShowBorder = true
            };

            unitSelector = new EngineeringUnitSelector
            {
                Location = new Point(22, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            btnDefaults = CreateFooterButton("Alapértékek", EngineeringButtonVariant.Ghost, HvacIconKind.Settings, 142);
            btnDefaults.Location = new Point(18, ClientSize.Height - 50);
            btnDefaults.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            btnDefaults.Click += BtnDefaults_Click;

            btnCancel = CreateFooterButton("Mégse", EngineeringButtonVariant.Secondary, null, 104);
            btnCancel.Location = new Point(ClientSize.Width - 378, ClientSize.Height - 50);
            btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnCancel.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            btnApply = CreateFooterButton("Alkalmaz", EngineeringButtonVariant.Info, HvacIconKind.Certification, 112);
            btnApply.Location = new Point(ClientSize.Width - 266, ClientSize.Height - 50);
            btnApply.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnApply.Click += (_, _) => SaveSettings(closeAfterSave: false);

            btnSave = CreateFooterButton("Mentés", EngineeringButtonVariant.Primary, HvacIconKind.SaveProject, 132);
            btnSave.Location = new Point(ClientSize.Width - 142, ClientSize.Height - 50);
            btnSave.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnSave.Click += (_, _) => SaveSettings(closeAfterSave: true);

            Controls.AddRange(new Control[] { tabHost, btnDefaults, btnCancel, btnApply, btnSave });

            BuildTabs();
            ApplyTheme(palette);
            LoadSettingsIntoUi();

            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.Window;
            ForeColor = palette.TextPrimary;
            Font = ThemeFonts.Body;

            tabHost.ApplyTheme(palette);
            unitSelector.ApplyTheme(palette);
            btnSave.ApplyTheme(palette);
            btnCancel.ApplyTheme(palette);
            btnDefaults.ApplyTheme(palette);
            btnApply.ApplyTheme(palette);
            ApplySettingsContentTheme();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;

            base.Dispose(disposing);
        }

        private void BuildTabs()
        {
            tabHost.Pages.Add(new EngineeringTabPage
            {
                Key = "profile",
                Text = "Tervezői profil",
                IconKind = HvacIconKind.ProjectProperties,
                Content = BuildProfilePage()
            });

            tabHost.Pages.Add(new EngineeringTabPage
            {
                Key = "system",
                Text = "Rendszer",
                IconKind = HvacIconKind.Settings,
                Content = BuildSystemPage()
            });

            tabHost.Pages.Add(new EngineeringTabPage
            {
                Key = "units",
                Text = "Mértékegységek",
                IconKind = HvacIconKind.DuctSizing,
                Content = BuildUnitsPage()
            });

            tabHost.Pages.Add(new EngineeringTabPage
            {
                Key = "paths",
                Text = "Útvonalak",
                IconKind = HvacIconKind.OpenProject,
                Content = BuildPathsPage()
            });

            tabHost.SelectedKey = "profile";
        }

        private Control BuildProfilePage()
        {
            Panel page = CreatePagePanel();

            EngineeringCardPanel identityCard = CreateCard(
                "Tervezői törzsadatok",
                "Ezeket az adatokat a projektadatok ablak egy kattintással be tudja olvasni.",
                HvacIconKind.ProjectProperties,
                new Point(18, 18),
                new Size(806, 250));

            txtDefName = AddTextBox(identityCard.ContentPanel, "Név", 16, 34, 250);
            txtDefName.PlaceholderText = "Valaki Valaki";
            txtDefChamber = AddTextBox(identityCard.ContentPanel, "Kamarai szám", 286, 34, 160);
            txtDefChamber.PlaceholderText = "MMK-00-0000";
            txtDefCompany = AddTextBox(identityCard.ContentPanel, "Cég / tervezőiroda", 466, 34, 250);
            txtDefCompany.PlaceholderText = "HVAC Designer Kft.";
            txtDefPhone = AddTextBox(identityCard.ContentPanel, "Telefon", 16, 112, 210);
            txtDefPhone.PlaceholderText = "+36 30 123 4567";
            txtDefEmail = AddTextBox(identityCard.ContentPanel, "E-mail", 246, 112, 300);
            txtDefEmail.PlaceholderText = "valaki.valaki@hvacdesigner.hu";
            txtDefEmail.TextChanged += (_, _) => ValidateEmailField();

            EngineeringCardPanel addressCard = CreateCard(
                "Alapértelmezett cím",
                "A tanúsítási és tervdokumentációs modulok később ezt is tudják majd használni.",
                HvacIconKind.Building,
                new Point(18, 288),
                new Size(806, 210));

            txtDefZip = AddTextBox(addressCard.ContentPanel, "Irányítószám", 16, 34, 96);
            txtDefZip.PlaceholderText = "1111";
            txtDefCity = AddTextBox(addressCard.ContentPanel, "Település", 130, 34, 160);
            txtDefCity.PlaceholderText = "Budapest";
            txtDefStreet = AddTextBox(addressCard.ContentPanel, "Közterület neve", 308, 34, 210);
            txtDefStreet.PlaceholderText = "Minta";
            cmbDefStreetType = AddComboBox(addressCard.ContentPanel, "Jellege", 536, 34, 108);
            cmbDefStreetType.Items.AddRange(new object[] { "utca", "út", "tér", "körút", "krt", "sétány", "köz", "fasor" });
            txtDefHouse = AddTextBox(addressCard.ContentPanel, "Házszám", 662, 34, 78);
            txtDefHouse.PlaceholderText = "12/A";

            page.Controls.AddRange(new Control[] { identityCard, addressCard });
            return page;
        }

        private Control BuildSystemPage()
        {
            Panel page = CreatePagePanel();

            EngineeringCardPanel visualCard = CreateCard(
                "Megjelenés és működés",
                "Alap téma, automatikus mentés és a későbbi méretezési preferenciák helye.",
                HvacIconKind.ThemeToggle,
                new Point(18, 18),
                new Size(806, 190));

            cmbTheme = AddComboBox(visualCard.ContentPanel, "Téma", 16, 36, 220);
            cmbTheme.Items.AddRange(new object[] { "Sötét mérnöki", "Világos modern" });
            txtAutoSave = AddTextBox(visualCard.ContentPanel, "Automatikus mentés [perc]", 256, 36, 180);
            txtAutoSave.UnitVisible = false;
            txtAutoSave.PlaceholderText = "5";
            AddInfoLabel(
                visualCard.ContentPanel,
                "A téma mentés után azonnal frissül. Az automatikus mentés 1 és 120 perc között állítható.",
                new Point(16, 112),
                new Size(720, 42));

            EngineeringCardPanel diagnosticsCard = CreateCard(
                "Diagnosztika",
                "Fejlesztői és hibakeresési nézetek. Normál használatnál ezek kikapcsolva maradhatnak.",
                HvacIconKind.Safety,
                new Point(18, 232),
                new Size(806, 250));

            chkDebugViews = AddCheckBox(diagnosticsCard.ContentPanel, "Fejlesztői nézetek engedélyezése", 16, 34, 330);
            chkBootstrapDiagnostics = AddCheckBox(diagnosticsCard.ContentPanel, "Indítási diagnosztika megjelenítése", 16, 74, 330);
            chkVerboseNotifications = AddCheckBox(diagnosticsCard.ContentPanel, "Részletes rendszerüzenetek", 16, 114, 330);

            page.Controls.AddRange(new Control[] { visualCard, diagnosticsCard });
            return page;
        }

        private Control BuildUnitsPage()
        {
            Panel page = CreatePagePanel();

            EngineeringCardPanel card = CreateCard(
                "Mértékegység-preferenciák",
                "A számító motorok SI alapon dolgoznak, itt a bevitel és megjelenítés egységeit állítod.",
                HvacIconKind.DuctSizing,
                new Point(18, 18),
                new Size(806, 470));

            unitSelector.Location = new Point(22, 22);
            card.ContentPanel.Controls.Add(unitSelector);
            page.Controls.Add(card);
            return page;
        }

        private Control BuildPathsPage()
        {
            Panel page = CreatePagePanel();

            EngineeringCardPanel card = CreateCard(
                "Mappák és adatforrások",
                "A projekt saját mappáját a projekt mentése határozza meg. Itt csak a közös felhasználói adat- és kimeneti mappák állíthatók.",
                HvacIconKind.OpenProject,
                new Point(18, 18),
                new Size(806, 460));

            txtAutoSavePath = AddPathRow(card.ContentPanel, "Automatikus mentések", 16, 34, editable: true);
            txtAutoSavePath.PlaceholderText = @"C:\...\AutoSave";
            txtUserXmlFolder = AddPathRow(card.ContentPanel, "Felhasználói XML-ek", 16, 104, editable: true);
            txtUserXmlFolder.PlaceholderText = @"C:\...\UserXml";
            txtExportFolder = AddPathRow(card.ContentPanel, "Alapértelmezett exportok", 16, 174, editable: true);
            txtExportFolder.PlaceholderText = @"C:\...\Exports";
            txtLogFolder = AddPathRow(card.ContentPanel, "Naplók (rendszer által kezelt)", 16, 244, editable: false);
            txtCacheFolder = AddPathRow(card.ContentPanel, "Gyorsítótár (rendszer által kezelt)", 16, 314, editable: false);

            page.Controls.Add(card);
            return page;
        }

        private EngineeringTextBox AddPathRow(Control parent, string label, int left, int inputTop, bool editable)
        {
            EngineeringTextBox box = AddTextBox(parent, label, left, inputTop, 620);
            box.UnitVisible = false;
            box.ReadOnly = !editable;

            EngineeringButton browseButton = CreateFooterButton("...", EngineeringButtonVariant.Secondary, HvacIconKind.OpenProject, 44);
            browseButton.Location = new Point(left + 636, inputTop);
            browseButton.ButtonSize = EngineeringButtonSize.Normal;
            browseButton.Enabled = editable;
            browseButton.Click += (_, _) => BrowseFolder(box);
            parent.Controls.Add(browseButton);

            return box;
        }

        private EngineeringCardPanel CreateCard(
            string title,
            string subtitle,
            HvacIconKind icon,
            Point location,
            Size size)
        {
            EngineeringCardPanel card = new EngineeringCardPanel
            {
                Title = title,
                Subtitle = subtitle,
                IconKind = icon,
                ShowIcon = true,
                ShowAccentStrip = true,
                ShowSeparator = true,
                ShowStatusBadge = false,
                Location = location,
                Size = size
            };

            card.ContentPanel.Padding = new Padding(16, 16, 16, 14);
            card.ApplyTheme(palette);
            cards.Add(card);
            return card;
        }

        private static Panel CreatePagePanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
        }

        private EngineeringTextBox AddTextBox(Control parent, string label, int left, int inputTop, int width)
        {
            EngineeringTextBox textBox = new EngineeringTextBox
            {
                LabelText = label,
                QuantityKind = QuantityKind.None,
                UnitVisible = false,
                TextAlign = HorizontalAlignment.Left,
                Location = new Point(left, inputTop),
                Size = new Size(width, 58)
            };
            textBox.ApplyTheme(palette);
            parent.Controls.Add(textBox);
            return textBox;
        }

        private EngineeringComboBox AddComboBox(Control parent, string label, int left, int inputTop, int width)
        {
            EngineeringComboBox comboBox = new EngineeringComboBox
            {
                LabelText = label,
                Location = new Point(left, inputTop),
                Size = new Size(width, 58)
            };
            comboBox.ApplyTheme(palette);
            parent.Controls.Add(comboBox);
            return comboBox;
        }

        private EngineeringCheckBox AddCheckBox(Control parent, string text, int left, int top, int width)
        {
            EngineeringCheckBox checkBox = new EngineeringCheckBox
            {
                Text = text,
                Location = new Point(left, top),
                Size = new Size(width, 30)
            };
            checkBox.ApplyTheme(palette);
            parent.Controls.Add(checkBox);
            return checkBox;
        }

        private Label AddInfoLabel(Control parent, string text, Point location, Size size)
        {
            Label label = new Label
            {
                Text = text,
                Location = location,
                Size = size,
                AutoEllipsis = false,
                UseCompatibleTextRendering = false
            };
            parent.Controls.Add(label);
            return label;
        }

        private EngineeringButton CreateFooterButton(
            string text,
            EngineeringButtonVariant variant,
            HvacIconKind? icon,
            int width)
        {
            return new EngineeringButton
            {
                Text = text,
                Variant = variant,
                IconKind = icon,
                IconPlacement = icon.HasValue ? EngineeringButtonIconPlacement.Left : EngineeringButtonIconPlacement.None,
                ButtonSize = EngineeringButtonSize.Normal,
                Size = new Size(width, ThemeMetrics.ButtonHeight)
            };
        }

        private void LoadSettingsIntoUi()
        {
            UserSettings s = ServiceLocator.Settings.SelectedSettings;
            s.Normalize();

            cmbTheme.SelectedIndex = string.Equals(s.CurrentTheme, "Light", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            txtAutoSave.Text = Math.Max(1, s.AutoSaveIntervalMinutes).ToString();

            txtDefName.Text = s.DefaultDesignerName;
            txtDefChamber.Text = s.DefaultEligibilityNumber;
            txtDefCompany.Text = s.DefaultCompany;
            txtDefPhone.Text = s.DefaultDesignerPhone;
            txtDefEmail.Text = s.DefaultDesignerEmail;
            ValidateEmailField();

            txtDefZip.Text = s.DefaultDesZip;
            txtDefCity.Text = s.DefaultDesCity;
            txtDefStreet.Text = s.DefaultDesStreet;
            cmbDefStreetType.SelectedItem = cmbDefStreetType.Items.Contains(s.DefaultDesStreetType)
                ? s.DefaultDesStreetType
                : "utca";
            txtDefHouse.Text = s.DefaultDesHouse;

            chkDebugViews.Checked = s.Developer.EnableDebugViews;
            chkBootstrapDiagnostics.Checked = s.Developer.ShowBootstrapDiagnostics;
            chkVerboseNotifications.Checked = s.Developer.VerboseNotifications;

            txtAutoSavePath.Text = s.Paths.AutoSavePath;
            txtLogFolder.Text = s.Paths.LogFolder;
            txtCacheFolder.Text = s.Paths.CacheFolder;
            txtUserXmlFolder.Text = s.Paths.UserXmlFolder;
            txtExportFolder.Text = s.Paths.ExportFolder;
        }

        private void SaveSettings(bool closeAfterSave)
        {
            UserSettings s = ServiceLocator.Settings.SelectedSettings;
            SettingsFormSnapshot snapshot = CaptureSettingsSnapshot();

            if (!IsEmailValid(snapshot.DefaultDesignerEmail))
            {
                tabHost.SelectedKey = "profile";
                txtDefEmail.HasValidationError = true;
                EngineeringDialog.ShowMessage(
                    this,
                    "E-mail ellenőrzés",
                    "Az e-mail címben szerepelnie kell @ jelnek. Ha nem szeretnél e-mailt megadni, hagyd üresen a mezőt.",
                    EngineeringDialogSeverity.Warning,
                    HvacIconKind.Info);
                return;
            }

            s.CurrentTheme = snapshot.CurrentTheme;
            s.AutoSaveIntervalMinutes = snapshot.AutoSaveIntervalMinutes;

            s.DefaultDesignerName = snapshot.DefaultDesignerName;
            s.DefaultEligibilityNumber = snapshot.DefaultEligibilityNumber;
            s.DefaultCompany = snapshot.DefaultCompany;
            s.DefaultDesignerPhone = snapshot.DefaultDesignerPhone;
            s.DefaultDesignerEmail = snapshot.DefaultDesignerEmail;

            s.DefaultDesZip = snapshot.DefaultDesZip;
            s.DefaultDesCity = snapshot.DefaultDesCity;
            s.DefaultDesStreet = snapshot.DefaultDesStreet;
            s.DefaultDesStreetType = snapshot.DefaultDesStreetType;
            s.DefaultDesHouse = snapshot.DefaultDesHouse;

            s.Developer.EnableDebugViews = snapshot.EnableDebugViews;
            s.Developer.ShowBootstrapDiagnostics = snapshot.ShowBootstrapDiagnostics;
            s.Developer.VerboseNotifications = snapshot.VerboseNotifications;

            s.Paths.AutoSavePath = snapshot.AutoSavePath;
            s.Paths.LogFolder = snapshot.LogFolder;
            s.Paths.CacheFolder = snapshot.CacheFolder;
            s.Paths.UserXmlFolder = snapshot.UserXmlFolder;
            s.Paths.ExportFolder = snapshot.ExportFolder;

            unitSelector.ApplyChanges();
            SettingsOperationResult result = ServiceLocator.Settings.SaveSettings();

            if (result.Succeeded)
            {
                ServiceLocator.Settings.SelectedSettings = result.Settings;
                ThemeManager.CurrentThemeMode = s.CurrentTheme == "Light"
                    ? AppThemeMode.Light
                    : AppThemeMode.Dark;

                if (closeAfterSave)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    RestoreSettingsSnapshot(snapshot);
                    EngineeringDialog.ShowCompactMessage(
                        this,
                        "Beállítások",
                        "A módosítások mentve.",
                        EngineeringDialogSeverity.Success,
                        HvacIconKind.Certification);
                }

                return;
            }

            EngineeringDialog.ShowMessage(
                this,
                "Beállítások",
                result.Message,
                EngineeringDialogSeverity.Danger,
                HvacIconKind.Safety);
        }

        private SettingsFormSnapshot CaptureSettingsSnapshot()
        {
            return new SettingsFormSnapshot
            {
                CurrentTheme = cmbTheme.SelectedIndex == 1 ? "Light" : "Dark",
                AutoSaveIntervalMinutes = ParseBoundedInt(txtAutoSave.Text, 5, 1, 120),
                DefaultDesignerName = txtDefName.Text.Trim(),
                DefaultEligibilityNumber = txtDefChamber.Text.Trim(),
                DefaultCompany = txtDefCompany.Text.Trim(),
                DefaultDesignerPhone = txtDefPhone.Text.Trim(),
                DefaultDesignerEmail = txtDefEmail.Text.Trim(),
                DefaultDesZip = txtDefZip.Text.Trim(),
                DefaultDesCity = txtDefCity.Text.Trim(),
                DefaultDesStreet = txtDefStreet.Text.Trim(),
                DefaultDesStreetType = cmbDefStreetType.SelectedItem?.ToString() ?? "utca",
                DefaultDesHouse = txtDefHouse.Text.Trim(),
                EnableDebugViews = chkDebugViews.Checked,
                ShowBootstrapDiagnostics = chkBootstrapDiagnostics.Checked,
                VerboseNotifications = chkVerboseNotifications.Checked,
                AutoSavePath = txtAutoSavePath.Text.Trim(),
                LogFolder = txtLogFolder.Text.Trim(),
                CacheFolder = txtCacheFolder.Text.Trim(),
                UserXmlFolder = txtUserXmlFolder.Text.Trim(),
                ExportFolder = txtExportFolder.Text.Trim()
            };
        }

        private void RestoreSettingsSnapshot(SettingsFormSnapshot snapshot)
        {
            cmbTheme.SelectedIndex = snapshot.CurrentTheme == "Light" ? 1 : 0;
            txtAutoSave.Text = snapshot.AutoSaveIntervalMinutes.ToString();

            txtDefName.Text = snapshot.DefaultDesignerName;
            txtDefChamber.Text = snapshot.DefaultEligibilityNumber;
            txtDefCompany.Text = snapshot.DefaultCompany;
            txtDefPhone.Text = snapshot.DefaultDesignerPhone;
            txtDefEmail.Text = snapshot.DefaultDesignerEmail;

            txtDefZip.Text = snapshot.DefaultDesZip;
            txtDefCity.Text = snapshot.DefaultDesCity;
            txtDefStreet.Text = snapshot.DefaultDesStreet;
            cmbDefStreetType.SelectedItem = cmbDefStreetType.Items.Contains(snapshot.DefaultDesStreetType)
                ? snapshot.DefaultDesStreetType
                : "utca";
            txtDefHouse.Text = snapshot.DefaultDesHouse;

            chkDebugViews.Checked = snapshot.EnableDebugViews;
            chkBootstrapDiagnostics.Checked = snapshot.ShowBootstrapDiagnostics;
            chkVerboseNotifications.Checked = snapshot.VerboseNotifications;

            txtAutoSavePath.Text = snapshot.AutoSavePath;
            txtLogFolder.Text = snapshot.LogFolder;
            txtCacheFolder.Text = snapshot.CacheFolder;
            txtUserXmlFolder.Text = snapshot.UserXmlFolder;
            txtExportFolder.Text = snapshot.ExportFolder;

            ValidateEmailField();
        }

        private void BtnDefaults_Click(object? sender, EventArgs e)
        {
            DialogResult result = EngineeringDialog.ShowConfirmation(
                this,
                "Alapértékek visszaállítása",
                "A felhasználói beállítások alaphelyzetbe kerülnek. A projektfájlok nem módosulnak.",
                EngineeringDialogSeverity.Warning,
                HvacIconKind.Safety);

            if (result != DialogResult.Yes)
                return;

            SettingsOperationResult resetResult = ServiceLocator.Settings.ResetToDefaults();
            LoadSettingsIntoUi();
            ApplyTheme(ThemeManager.CurrentPalette);

            if (!resetResult.Succeeded)
            {
                EngineeringDialog.ShowMessage(
                    this,
                    "Beállítások",
                    resetResult.Message,
                    EngineeringDialogSeverity.Danger,
                    HvacIconKind.Safety);
            }
        }

        private void BrowseFolder(EngineeringTextBox target)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = target.LabelText,
                SelectedPath = string.IsNullOrWhiteSpace(target.Text) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : target.Text,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
                target.Text = dialog.SelectedPath;
        }

        private void ThemeManager_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            ApplyTheme(e.Palette);
        }

        private void ApplySettingsContentTheme()
        {
            foreach (EngineeringCardPanel card in cards)
                card.ApplyTheme(palette);

            foreach (EngineeringTabPage page in tabHost.Pages)
            {
                if (page.Content == null)
                    continue;

                ApplyThemeRecursive(page.Content);
            }
        }

        private void ApplyThemeRecursive(Control parent)
        {
            if (parent is IThemeable rootThemeable)
            {
                rootThemeable.ApplyTheme(palette);
                return;
            }

            parent.BackColor = palette.Window;
            parent.ForeColor = palette.TextPrimary;
            parent.Font = parent is Label ? ThemeFonts.Caption : ThemeFonts.Body;

            foreach (Control child in parent.Controls)
            {
                if (child is IThemeable themeable)
                {
                    themeable.ApplyTheme(palette);
                    continue;
                }

                child.BackColor = palette.Window;
                child.ForeColor = palette.TextPrimary;
                child.Font = child is Label ? ThemeFonts.Caption : ThemeFonts.Body;

                if (child.HasChildren)
                    ApplyThemeRecursive(child);
            }
        }

        private static int ParseBoundedInt(string value, int fallback, int min, int max)
        {
            if (!int.TryParse(value.Trim(), out int parsed))
                parsed = fallback;

            return Math.Max(min, Math.Min(max, parsed));
        }

        private bool ValidateEmailField()
        {
            bool valid = IsEmailValid(txtDefEmail.Text);
            txtDefEmail.HasValidationError = !valid;
            return valid;
        }

        private static bool IsEmailValid(string value)
        {
            string trimmed = value.Trim();
            return trimmed.Length == 0 || trimmed.Contains('@', StringComparison.Ordinal);
        }

        private sealed class SettingsFormSnapshot
        {
            public string CurrentTheme { get; set; } = "Dark";
            public int AutoSaveIntervalMinutes { get; set; }
            public string DefaultDesignerName { get; set; } = string.Empty;
            public string DefaultEligibilityNumber { get; set; } = string.Empty;
            public string DefaultCompany { get; set; } = string.Empty;
            public string DefaultDesignerPhone { get; set; } = string.Empty;
            public string DefaultDesignerEmail { get; set; } = string.Empty;
            public string DefaultDesZip { get; set; } = string.Empty;
            public string DefaultDesCity { get; set; } = string.Empty;
            public string DefaultDesStreet { get; set; } = string.Empty;
            public string DefaultDesStreetType { get; set; } = "utca";
            public string DefaultDesHouse { get; set; } = string.Empty;
            public bool EnableDebugViews { get; set; }
            public bool ShowBootstrapDiagnostics { get; set; }
            public bool VerboseNotifications { get; set; }
            public string AutoSavePath { get; set; } = string.Empty;
            public string LogFolder { get; set; } = string.Empty;
            public string CacheFolder { get; set; } = string.Empty;
            public string UserXmlFolder { get; set; } = string.Empty;
            public string ExportFolder { get; set; } = string.Empty;
        }
    }
}
