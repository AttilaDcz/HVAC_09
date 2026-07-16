using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Dialogs;
using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Components.Structural;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.EngineeringData.SimpleCatalogs;
using HVACDesigner.Services;

namespace HVACDesigner.CoreUI.Layout
{
    public class ProjectPropertiesForm : Form, IThemeable
    {
        private const string SourceCatalog = "Catalog";
        private const string SourceCustom = "Custom";
        private const string CustomFunctionId = "__custom__";

        private readonly ProjectData data;
        private readonly HVACScrollableContainer scrollContainer;
        private readonly Panel projectContent;
        private readonly EngineeringButton btnSave;
        private readonly EngineeringButton btnCancel;
        private readonly List<EngineeringCardPanel> cards = new List<EngineeringCardPanel>();

        private EngineeringTextBox txtProjName = null!;
        private EngineeringTextBox txtHrsz = null!;
        private EngineeringTextBox txtProjZip = null!;
        private EngineeringTextBox txtProjCity = null!;
        private EngineeringTextBox txtProjStreet = null!;
        private EngineeringComboBox cmbStreetType = null!;
        private EngineeringTextBox txtProjHouse = null!;
        private EngineeringTextBox txtProjBuilding = null!;
        private EngineeringTextBox txtProjFloor = null!;
        private EngineeringTextBox txtProjDoor = null!;

        private EngineeringComboBox cmbBuildingFunction = null!;
        private EngineeringTextBox txtCustomBuildingFunction = null!;
        private EngineeringComboBox cmbDesignPhase = null!;

        private EngineeringTextBox txtDesName = null!;
        private EngineeringTextBox txtChamber = null!;
        private EngineeringTextBox txtCoDesName = null!;
        private EngineeringTextBox txtCoChamber = null!;
        private EngineeringTextBox txtDesCompany = null!;
        private EngineeringTextBox txtDesAddress = null!;
        private EngineeringTextBox txtDesPhone = null!;
        private EngineeringTextBox txtDesEmail = null!;

        private EngineeringCheckBox cbIsCompany = null!;
        private EngineeringTextBox txtClientName = null!;
        private EngineeringTextBox txtClientAddress = null!;
        private EngineeringTextBox txtClientTax = null!;
        private EngineeringTextBox txtClientContact = null!;
        private EngineeringTextBox txtClientPhone = null!;
        private EngineeringTextBox txtClientEmail = null!;

        private SimpleCatalog<BuildingFunctionDefinition>? buildingFunctions;
        private SimpleCatalog<BuildingFunctionMapping>? buildingMappings;
        private ThemePalette palette = ThemeManager.CurrentPalette;

        public ProjectPropertiesForm(ProjectData projectData, bool isNewProject)
        {
            data = projectData ?? throw new ArgumentNullException(nameof(projectData));
            data.NormalizeAfterLoad();

            Text = isNewProject ? "Új projekt létrehozása" : "Projekt adatlap szerkesztése";
            Size = new Size(840, 780);
            MinimumSize = new Size(780, 660);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = ThemeFonts.Body;

            LoadCatalogs();

            scrollContainer = new HVACScrollableContainer
            {
                Location = new Point(18, 18),
                Size = new Size(ClientSize.Width - 36, ClientSize.Height - 88),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                CaptureChildMouseWheel = true
            };

            projectContent = new Panel
            {
                Location = Point.Empty,
                Size = new Size(760, 1242)
            };

            btnCancel = CreateFooterButton("Mégse", EngineeringButtonVariant.Secondary, null, 104);
            btnCancel.Location = new Point(ClientSize.Width - 278, ClientSize.Height - 50);
            btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnCancel.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            btnSave = CreateFooterButton("Mentés", EngineeringButtonVariant.Primary, HvacIconKind.SaveProject, 120);
            btnSave.Location = new Point(ClientSize.Width - 130, ClientSize.Height - 50);
            btnSave.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnSave.Click += BtnSave_Click;

            scrollContainer.ContentControls.Add(projectContent);
            Controls.AddRange(new Control[] { scrollContainer, btnCancel, btnSave });

            BuildLayout();
            ApplyTheme(palette);
            LoadDataIntoUi();

            Shown += (_, _) => scrollContainer.RecalculateContentLayout();
            Resize += (_, _) => scrollContainer.RecalculateContentLayout(preserveScrollRatio: true);
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.Window;
            ForeColor = palette.TextPrimary;
            Font = ThemeFonts.Body;

            projectContent.BackColor = palette.Window;
            scrollContainer.ApplyTheme(palette);
            btnSave.ApplyTheme(palette);
            btnCancel.ApplyTheme(palette);

            foreach (EngineeringCardPanel card in cards)
                card.ApplyTheme(palette);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;

            base.Dispose(disposing);
        }

        private void BuildLayout()
        {
            EngineeringCardPanel projectCard = CreateCard(
                "Projekt és helyszín",
                "A projekt azonosító és ingatlanadatok dokumentációs alapként kerülnek mentésre.",
                HvacIconKind.Building,
                new Point(0, 0),
                new Size(740, 360));

            txtProjName = AddTextBox(projectCard.ContentPanel, "Projekt megnevezése", 16, 34, 320, "Családi ház gépészeti terve");
            txtProjName.TextChanged += (_, _) => txtProjName.HasValidationError = string.IsNullOrWhiteSpace(txtProjName.Text);
            txtHrsz = AddTextBox(projectCard.ContentPanel, "HRSZ", 348, 34, 118, "1234/5");
            cmbDesignPhase = AddComboBox(projectCard.ContentPanel, "Tervfázis", 486, 34, 180);
            cmbDesignPhase.Items.AddRange(new object[]
            {
                "Koncepcióterv",
                "Engedélyezési terv",
                "Kiviteli terv",
                "Megvalósulási terv",
                "Tanúsítás / felülvizsgálat"
            });

            txtProjZip = AddTextBox(projectCard.ContentPanel, "Irsz.", 16, 112, 84, "1111");
            txtProjCity = AddTextBox(projectCard.ContentPanel, "Település", 116, 112, 150, "Budapest");
            txtProjStreet = AddTextBox(projectCard.ContentPanel, "Közterület", 282, 112, 190, "Minta");
            cmbStreetType = AddComboBox(projectCard.ContentPanel, "Jellege", 488, 112, 100);
            cmbStreetType.Items.AddRange(new object[] { "utca", "út", "tér", "körút", "krt", "sétány", "dűlő", "köz", "fasor", "rakpart" });
            txtProjHouse = AddTextBox(projectCard.ContentPanel, "Házszám", 604, 112, 70, "12");
            txtProjBuilding = AddTextBox(projectCard.ContentPanel, "Épület / lépcsőház", 16, 190, 130, "A");
            txtProjFloor = AddTextBox(projectCard.ContentPanel, "Emelet", 166, 190, 78, "2");
            txtProjDoor = AddTextBox(projectCard.ContentPanel, "Ajtó", 264, 190, 78, "5");

            EngineeringCardPanel functionCard = CreateCard(
                "Épületfunkció és szabványprofil",
                "A projekt rendeltetése. Ha nincs megfelelő katalóguselem, választható egyedi megadás.",
                HvacIconKind.BuildingEnergy,
                new Point(0, 378),
                new Size(740, 170));

            cmbBuildingFunction = AddComboBox(functionCard.ContentPanel, "Épületfunkció", 16, 34, 250);
            cmbBuildingFunction.DisplayMember = nameof(BuildingFunctionOption.Display);
            cmbBuildingFunction.ValueMember = nameof(BuildingFunctionOption.Id);
            cmbBuildingFunction.SelectedIndexChanged += (_, _) => BuildingFunctionChanged();
            txtCustomBuildingFunction = AddTextBox(functionCard.ContentPanel, "Egyedi funkció", 286, 34, 260, "Családi ház gépészeti átalakítás");
            txtCustomBuildingFunction.TextChanged += (_, _) =>
            {
                if (IsCustomBuildingFunctionSelected())
                    txtCustomBuildingFunction.HasValidationError = string.IsNullOrWhiteSpace(txtCustomBuildingFunction.Text);
            };

            EngineeringCardPanel designerCard = CreateCard(
                "Tervezői adatok",
                "A felhasználói profil gombbal betölthető, de a projekt saját másolatot tárol.",
                HvacIconKind.ProjectProperties,
                new Point(0, 566),
                new Size(740, 340));

            EngineeringButton btnLoadProfile = CreateFooterButton("Tervezői profil betöltése", EngineeringButtonVariant.Info, HvacIconKind.Import, 206);
            btnLoadProfile.Location = new Point(16, 20);
            btnLoadProfile.Click += BtnLoadProfile_Click;
            designerCard.ContentPanel.Controls.Add(btnLoadProfile);

            txtDesName = AddTextBox(designerCard.ContentPanel, "Felelős tervező", 16, 76, 240, "Valaki Valaki");
            txtChamber = AddTextBox(designerCard.ContentPanel, "Kamarai szám", 276, 76, 170, "MMK-00-0000");
            txtDesCompany = AddTextBox(designerCard.ContentPanel, "Tervezőiroda / cég", 456, 76, 240, "HVAC Designer Kft.");
            txtCoDesName = AddTextBox(designerCard.ContentPanel, "Munkatárs / segédtervező", 16, 154, 240, "");
            txtCoChamber = AddTextBox(designerCard.ContentPanel, "Munkatárs kamarai száma", 276, 154, 170, "");
            txtDesAddress = AddTextBox(designerCard.ContentPanel, "Iroda címe", 456, 154, 240, "1111 Budapest, Minta utca 12/A");
            txtDesPhone = AddTextBox(designerCard.ContentPanel, "Telefon", 16, 216, 210, "+36 30 123 4567");
            txtDesEmail = AddTextBox(designerCard.ContentPanel, "E-mail", 246, 216, 260, "valaki.valaki@hvacdesigner.hu");
            txtDesEmail.TextChanged += (_, _) => txtDesEmail.HasValidationError = !IsEmailValid(txtDesEmail.Text);

            EngineeringCardPanel clientCard = CreateCard(
                "Megbízó / tulajdonos",
                "A megbízói adatok tervlapokhoz, számítási dokumentációhoz és későbbi exporthoz használhatók.",
                HvacIconKind.Info,
                new Point(0, 924),
                new Size(740, 300));

            cbIsCompany = new EngineeringCheckBox
            {
                Text = "A megbízó jogi személy / társasház / cég",
                Location = new Point(16, 34),
                Size = new Size(340, 30)
            };
            cbIsCompany.CheckedChanged += (_, _) => ToggleClientType(cbIsCompany.Checked);
            clientCard.ContentPanel.Controls.Add(cbIsCompany);

            txtClientName = AddTextBox(clientCard.ContentPanel, "Megbízó neve", 16, 86, 220, "Kovács Péter");
            txtClientAddress = AddTextBox(clientCard.ContentPanel, "Lakcím / székhely", 256, 86, 250, "1111 Budapest, Minta utca 12/A");
            txtClientTax = AddTextBox(clientCard.ContentPanel, "Adószám", 526, 86, 130, "12345678-1-42");
            txtClientContact = AddTextBox(clientCard.ContentPanel, "Kapcsolattartó", 16, 164, 240, "Kovács Péter");
            txtClientPhone = AddTextBox(clientCard.ContentPanel, "Telefon", 266, 164, 190, "+36 30 123 4567");
            txtClientEmail = AddTextBox(clientCard.ContentPanel, "E-mail", 476, 164, 220, "megbizo@example.hu");
            txtClientEmail.TextChanged += (_, _) => txtClientEmail.HasValidationError = !IsEmailValid(txtClientEmail.Text);

            projectContent.Controls.AddRange(new Control[] { projectCard, functionCard, designerCard, clientCard });
            projectContent.Height = clientCard.Bottom + 18;
            scrollContainer.RecalculateContentLayout();
        }

        private EngineeringCardPanel CreateCard(string title, string subtitle, HvacIconKind icon, Point location, Size size)
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

        private EngineeringTextBox AddTextBox(Control parent, string label, int left, int inputTop, int width, string placeholder)
        {
            EngineeringTextBox textBox = new EngineeringTextBox
            {
                LabelText = label,
                QuantityKind = QuantityKind.None,
                UnitVisible = false,
                TextAlign = HorizontalAlignment.Left,
                PlaceholderText = placeholder,
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

        private EngineeringButton CreateFooterButton(string text, EngineeringButtonVariant variant, HvacIconKind? icon, int width)
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

        private void LoadCatalogs()
        {
            ServiceLocator.EngineeringDataRegistry.TryGet(
                "Functions.Building",
                "1.0",
                out buildingFunctions);

            ServiceLocator.EngineeringDataRegistry.TryGet(
                "Mappings.Building",
                "1.0",
                out buildingMappings);

        }

        private void LoadBuildingFunctionOptions()
        {
            cmbBuildingFunction.Items.Clear();

            if (buildingFunctions != null)
            {
                foreach (BuildingFunctionDefinition function in buildingFunctions.Items.Values.OrderBy(item => item.DisplayName, StringComparer.CurrentCultureIgnoreCase))
                {
                    cmbBuildingFunction.Items.Add(new BuildingFunctionOption(function.Id, function.DisplayName, false));
                }
            }

            cmbBuildingFunction.Items.Add(new BuildingFunctionOption(CustomFunctionId, "Egyedi / kézi megadás", true));
        }

        private void LoadDataIntoUi()
        {
            LoadBuildingFunctionOptions();

            txtProjName.Text = data.Name;
            txtHrsz.Text = data.TopographicalNumber;
            txtProjZip.Text = data.ProjZipCode;
            txtProjCity.Text = data.ProjSettlementName;
            txtProjStreet.Text = data.ProjStreetName;
            cmbStreetType.SelectedItem = cmbStreetType.Items.Contains(data.ProjStreetType) ? data.ProjStreetType : "utca";
            txtProjHouse.Text = data.ProjHouseNumber;
            txtProjBuilding.Text = data.ProjBuilding;
            txtProjFloor.Text = data.ProjFloor;
            txtProjDoor.Text = data.ProjDoorNumber;
            cmbDesignPhase.SelectedItem = cmbDesignPhase.Items.Contains(data.DesignPhase) ? data.DesignPhase : "Kiviteli terv";

            bool customFunction = string.Equals(data.BuildingFunctionSource, SourceCustom, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(data.BuildingFunctionId) && !string.IsNullOrWhiteSpace(data.CustomBuildingFunctionName);

            if (customFunction)
                cmbBuildingFunction.SelectedValue = CustomFunctionId;
            else if (!string.IsNullOrWhiteSpace(data.BuildingFunctionId))
                cmbBuildingFunction.SelectedValue = data.BuildingFunctionId;
            else
                cmbBuildingFunction.SelectedValue = "DetachedHouse";

            if (cmbBuildingFunction.SelectedIndex < 0 && cmbBuildingFunction.Items.Count > 0)
                cmbBuildingFunction.SelectedIndex = 0;

            txtCustomBuildingFunction.Text = data.CustomBuildingFunctionName;

            txtDesName.Text = data.DesignerName;
            txtChamber.Text = data.EligibilityNumber;
            txtCoDesName.Text = data.CoDesignerName;
            txtCoChamber.Text = data.CoEligibilityNumber;
            txtDesCompany.Text = data.DesignerCompany;
            txtDesAddress.Text = data.DesignerAddress;
            txtDesPhone.Text = data.DesignerPhone;
            txtDesEmail.Text = data.DesignerEmail;

            cbIsCompany.Checked = data.ClientIsCompany;
            txtClientName.Text = data.ClientName;
            txtClientAddress.Text = data.ClientAddress;
            txtClientTax.Text = data.ClientTaxNumber;
            txtClientContact.Text = data.ClientContactPerson;
            txtClientPhone.Text = data.ClientPhone;
            txtClientEmail.Text = data.ClientEmail;

            ToggleClientType(data.ClientIsCompany);
            ValidateEmailFields();
        }

        private void BtnLoadProfile_Click(object? sender, EventArgs e)
        {
            if (HasDesignerData())
            {
                DialogResult result = EngineeringDialog.ShowConfirmation(
                    this,
                    "Tervezői profil betöltése",
                    "A projektben már vannak tervezői adatok. Felülírjam őket a felhasználói profil adataival?",
                    EngineeringDialogSeverity.Warning,
                    HvacIconKind.ProjectProperties);

                if (result != DialogResult.Yes)
                    return;
            }

            SettingsOperationResult settingsResult = ServiceLocator.Settings.LoadSettings();
            UserSettings profile = settingsResult.Settings;
            txtDesName.Text = profile.DefaultDesignerName;
            txtChamber.Text = profile.DefaultEligibilityNumber;
            txtDesCompany.Text = profile.DefaultCompany;
            txtDesPhone.Text = profile.DefaultDesignerPhone;
            txtDesEmail.Text = profile.DefaultDesignerEmail;
            txtDesAddress.Text = FormatAddress(
                profile.DefaultDesZip,
                profile.DefaultDesCity,
                profile.DefaultDesStreet,
                profile.DefaultDesStreetType,
                profile.DefaultDesHouse);

            ValidateEmailFields();
        }

        private void BuildingFunctionChanged()
        {
            bool custom = IsCustomBuildingFunctionSelected();
            txtCustomBuildingFunction.ReadOnly = !custom;
            txtCustomBuildingFunction.Enabled = true;
            txtCustomBuildingFunction.HasValidationError = custom && string.IsNullOrWhiteSpace(txtCustomBuildingFunction.Text);
        }

        private void ToggleClientType(bool isCompany)
        {
            txtClientTax.Visible = isCompany;
            txtClientContact.Visible = isCompany;

            if (isCompany)
            {
                txtClientPhone.Location = new Point(266, 164);
                txtClientEmail.Location = new Point(476, 164);
            }
            else
            {
                txtClientPhone.Location = new Point(16, 164);
                txtClientEmail.Location = new Point(226, 164);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            CommitPendingTextInputs();
            ProjectPropertiesSnapshot snapshot = CaptureProjectSnapshot();

            if (!ValidateRequiredFields(snapshot) || !ValidateBuildingFunction() || !ValidateEmailFields(snapshot))
                return;

            ApplySnapshotToProject(snapshot);
            ServiceLocator.Project.NotifyProjectChanged();

            DialogResult = DialogResult.OK;
            Close();
        }

        private ProjectPropertiesSnapshot CaptureProjectSnapshot()
        {
            ProjectPropertiesSnapshot snapshot = new ProjectPropertiesSnapshot
            {
                Name = txtProjName.Text.Trim(),
                TopographicalNumber = txtHrsz.Text.Trim(),
                ProjZipCode = txtProjZip.Text.Trim(),
                ProjSettlementName = txtProjCity.Text.Trim(),
                ProjStreetName = txtProjStreet.Text.Trim(),
                ProjStreetType = cmbStreetType.SelectedItem?.ToString() ?? "utca",
                ProjHouseNumber = txtProjHouse.Text.Trim(),
                ProjBuilding = txtProjBuilding.Text.Trim(),
                ProjFloor = txtProjFloor.Text.Trim(),
                ProjDoorNumber = txtProjDoor.Text.Trim(),
                DesignPhase = cmbDesignPhase.SelectedItem?.ToString() ?? string.Empty,
                DesignerName = txtDesName.Text.Trim(),
                EligibilityNumber = txtChamber.Text.Trim(),
                CoDesignerName = txtCoDesName.Text.Trim(),
                CoEligibilityNumber = txtCoChamber.Text.Trim(),
                DesignerCompany = txtDesCompany.Text.Trim(),
                DesignerAddress = txtDesAddress.Text.Trim(),
                DesignerPhone = txtDesPhone.Text.Trim(),
                DesignerEmail = txtDesEmail.Text.Trim(),
                ClientIsCompany = cbIsCompany.Checked,
                ClientName = txtClientName.Text.Trim(),
                ClientAddress = txtClientAddress.Text.Trim(),
                ClientTaxNumber = cbIsCompany.Checked ? txtClientTax.Text.Trim() : string.Empty,
                ClientContactPerson = cbIsCompany.Checked ? txtClientContact.Text.Trim() : string.Empty,
                ClientPhone = txtClientPhone.Text.Trim(),
                ClientEmail = txtClientEmail.Text.Trim()
            };

            CaptureBuildingFunctionSelection(snapshot);
            return snapshot;
        }

        private void CaptureBuildingFunctionSelection(ProjectPropertiesSnapshot snapshot)
        {
            if (IsCustomBuildingFunctionSelected())
            {
                string customName = txtCustomBuildingFunction.Text.Trim();
                snapshot.BuildingFunctionSource = SourceCustom;
                snapshot.BuildingFunctionId = string.Empty;
                snapshot.BuildingFunctionDisplayName = customName;
                snapshot.CustomBuildingFunctionName = customName;
                snapshot.BuildingProfileId = string.Empty;
                return;
            }

            if (cmbBuildingFunction.SelectedItem is BuildingFunctionOption option)
            {
                snapshot.BuildingFunctionSource = SourceCatalog;
                snapshot.BuildingFunctionId = option.Id;
                snapshot.BuildingFunctionDisplayName = option.Display;
                snapshot.CustomBuildingFunctionName = string.Empty;
                snapshot.BuildingProfileId = ResolveProfileId(option.Id);
                return;
            }

            snapshot.BuildingFunctionSource = SourceCatalog;
            snapshot.BuildingFunctionId = string.Empty;
            snapshot.BuildingFunctionDisplayName = string.Empty;
            snapshot.CustomBuildingFunctionName = string.Empty;
            snapshot.BuildingProfileId = string.Empty;
        }

        private void ApplySnapshotToProject(ProjectPropertiesSnapshot snapshot)
        {
            data.Name = snapshot.Name;
            data.TopographicalNumber = snapshot.TopographicalNumber;
            data.ProjZipCode = snapshot.ProjZipCode;
            data.ProjSettlementName = snapshot.ProjSettlementName;
            data.ProjStreetName = snapshot.ProjStreetName;
            data.ProjStreetType = snapshot.ProjStreetType;
            data.ProjHouseNumber = snapshot.ProjHouseNumber;
            data.ProjBuilding = snapshot.ProjBuilding;
            data.ProjFloor = snapshot.ProjFloor;
            data.ProjDoorNumber = snapshot.ProjDoorNumber;
            data.DesignPhase = snapshot.DesignPhase;

            data.BuildingFunctionSource = snapshot.BuildingFunctionSource;
            data.BuildingFunctionId = snapshot.BuildingFunctionId;
            data.BuildingFunctionDisplayName = snapshot.BuildingFunctionDisplayName;
            data.CustomBuildingFunctionName = snapshot.CustomBuildingFunctionName;
            data.BuildingProfileId = snapshot.BuildingProfileId;

            data.DesignerName = snapshot.DesignerName;
            data.EligibilityNumber = snapshot.EligibilityNumber;
            data.CoDesignerName = snapshot.CoDesignerName;
            data.CoEligibilityNumber = snapshot.CoEligibilityNumber;
            data.DesignerCompany = snapshot.DesignerCompany;
            data.DesignerAddress = snapshot.DesignerAddress;
            data.DesignerPhone = snapshot.DesignerPhone;
            data.DesignerEmail = snapshot.DesignerEmail;

            data.ClientIsCompany = snapshot.ClientIsCompany;
            data.ClientName = snapshot.ClientName;
            data.ClientAddress = snapshot.ClientAddress;
            data.ClientTaxNumber = snapshot.ClientTaxNumber;
            data.ClientContactPerson = snapshot.ClientContactPerson;
            data.ClientPhone = snapshot.ClientPhone;
            data.ClientEmail = snapshot.ClientEmail;
        }

        private bool ValidateRequiredFields(ProjectPropertiesSnapshot snapshot)
        {
            bool valid = !string.IsNullOrWhiteSpace(snapshot.Name);
            txtProjName.HasValidationError = !valid;

            if (!valid)
            {
                EngineeringDialog.ShowMessage(
                    this,
                    "Hiányzó projektadat",
                    "A projekt megnevezése kötelező.",
                    EngineeringDialogSeverity.Warning,
                    HvacIconKind.Info);
            }

            return valid;
        }

        private bool ValidateBuildingFunction()
        {
            bool valid = !IsCustomBuildingFunctionSelected() || !string.IsNullOrWhiteSpace(txtCustomBuildingFunction.Text);
            txtCustomBuildingFunction.HasValidationError = !valid;

            if (!valid)
            {
                EngineeringDialog.ShowMessage(
                    this,
                    "Hiányzó épületfunkció",
                    "Egyedi épületfunkció választásakor add meg a funkció nevét.",
                    EngineeringDialogSeverity.Warning,
                    HvacIconKind.BuildingEnergy);
            }

            return valid;
        }

        private bool ValidateEmailFields()
        {
            return ValidateEmailFields(CaptureProjectSnapshot());
        }

        private bool ValidateEmailFields(ProjectPropertiesSnapshot snapshot)
        {
            bool designerValid = IsEmailValid(snapshot.DesignerEmail);
            bool clientValid = IsEmailValid(snapshot.ClientEmail);

            txtDesEmail.HasValidationError = !designerValid;
            txtClientEmail.HasValidationError = !clientValid;

            if (designerValid && clientValid)
                return true;

            EngineeringDialog.ShowMessage(
                this,
                "E-mail ellenőrzés",
                "Az e-mail mezőkben szerepelnie kell @ jelnek. Ha nem szeretnél e-mailt megadni, hagyd üresen a mezőt.",
                EngineeringDialogSeverity.Warning,
                HvacIconKind.Info);

            return false;
        }

        private void CommitPendingTextInputs()
        {
            foreach (EngineeringTextBox textBox in EnumerateControls(projectContent).OfType<EngineeringTextBox>())
                textBox.CommitPendingText();
        }

        private static IEnumerable<Control> EnumerateControls(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;

                foreach (Control descendant in EnumerateControls(child))
                    yield return descendant;
            }
        }

        private string ResolveProfileId(string functionId)
        {
            return buildingMappings != null &&
                buildingMappings.TryGet(functionId, out BuildingFunctionMapping mapping)
                    ? mapping.ProfileId
                    : string.Empty;
        }

        private bool IsCustomBuildingFunctionSelected()
        {
            return cmbBuildingFunction.SelectedItem is BuildingFunctionOption option && option.IsCustom;
        }

        private bool HasDesignerData()
        {
            return !string.IsNullOrWhiteSpace(txtDesName.Text) ||
                !string.IsNullOrWhiteSpace(txtChamber.Text) ||
                !string.IsNullOrWhiteSpace(txtDesCompany.Text) ||
                !string.IsNullOrWhiteSpace(txtDesAddress.Text) ||
                !string.IsNullOrWhiteSpace(txtDesPhone.Text) ||
                !string.IsNullOrWhiteSpace(txtDesEmail.Text);
        }

        private void ThemeManager_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            ApplyTheme(e.Palette);
        }

        private static string FormatAddress(string zip, string city, string street, string streetType, string house)
        {
            string settlement = string.Join(" ", new[] { zip, city }.Where(item => !string.IsNullOrWhiteSpace(item)));
            string streetLine = string.Join(" ", new[] { street, streetType, house }.Where(item => !string.IsNullOrWhiteSpace(item)));

            if (settlement.Length == 0)
                return streetLine;

            if (streetLine.Length == 0)
                return settlement;

            return settlement + ", " + streetLine;
        }

        private static bool IsEmailValid(string value)
        {
            string trimmed = value.Trim();
            return trimmed.Length == 0 || trimmed.Contains('@', StringComparison.Ordinal);
        }

        private sealed class BuildingFunctionOption
        {
            public BuildingFunctionOption(string id, string display, bool isCustom)
            {
                Id = id;
                Display = display;
                IsCustom = isCustom;
            }

            public string Id { get; }
            public string Display { get; }
            public bool IsCustom { get; }

            public override string ToString()
            {
                return Display;
            }
        }

        private sealed class ProjectPropertiesSnapshot
        {
            public string Name { get; set; } = string.Empty;
            public string TopographicalNumber { get; set; } = string.Empty;
            public string ProjZipCode { get; set; } = string.Empty;
            public string ProjSettlementName { get; set; } = string.Empty;
            public string ProjStreetName { get; set; } = string.Empty;
            public string ProjStreetType { get; set; } = "utca";
            public string ProjHouseNumber { get; set; } = string.Empty;
            public string ProjBuilding { get; set; } = string.Empty;
            public string ProjFloor { get; set; } = string.Empty;
            public string ProjDoorNumber { get; set; } = string.Empty;
            public string DesignPhase { get; set; } = string.Empty;
            public string BuildingFunctionSource { get; set; } = SourceCatalog;
            public string BuildingFunctionId { get; set; } = string.Empty;
            public string BuildingFunctionDisplayName { get; set; } = string.Empty;
            public string CustomBuildingFunctionName { get; set; } = string.Empty;
            public string BuildingProfileId { get; set; } = string.Empty;
            public string DesignerName { get; set; } = string.Empty;
            public string EligibilityNumber { get; set; } = string.Empty;
            public string CoDesignerName { get; set; } = string.Empty;
            public string CoEligibilityNumber { get; set; } = string.Empty;
            public string DesignerCompany { get; set; } = string.Empty;
            public string DesignerAddress { get; set; } = string.Empty;
            public string DesignerPhone { get; set; } = string.Empty;
            public string DesignerEmail { get; set; } = string.Empty;
            public bool ClientIsCompany { get; set; }
            public string ClientName { get; set; } = string.Empty;
            public string ClientAddress { get; set; } = string.Empty;
            public string ClientTaxNumber { get; set; } = string.Empty;
            public string ClientContactPerson { get; set; } = string.Empty;
            public string ClientPhone { get; set; } = string.Empty;
            public string ClientEmail { get; set; } = string.Empty;
        }
    }
}
