using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Dialogs;
using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Components.Help;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Services;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.Services;

namespace HVACDesigner.CoreUI.Layout
{
    public class HeaderHost : HostBase, IThemeable
    {
        private readonly EngineeringToolTip headerToolTip = new EngineeringToolTip();
        private readonly Dictionary<string, EngineeringButton> buttonsByKey =
            new Dictionary<string, EngineeringButton>(StringComparer.OrdinalIgnoreCase);
        private const bool ShowTemporaryThemeToggle = true;
        private const int BrandBlockWidth = 286;

        private Panel topPanel = null!;
        private Panel modulePanel = null!;
        private PictureBox appIconBox = null!;
        private Label lblAppName = null!;
        private Label lblProjectName = null!;
        private Label lblModuleScope = null!;
        private FlowLayoutPanel globalButtonFlow = null!;
        private FlowLayoutPanel moduleButtonFlow = null!;
        private ThemePalette palette = ThemeManager.CurrentPalette;

        public HeaderHost()
            : base("HeaderHostZone")
        {
            Height = LayoutMetrics.HeaderHeight;

            InitBasicStructure();
            BuildHeaderUI();
            ApplyTheme(palette);

            ServiceLocator.Navigation.NavigationStateChanged += Navigation_NavigationStateChanged;
            ServiceLocator.Project.ProjectChanged += Project_ProjectChanged;
            ThemeManager.ThemeChanged += HeaderHost_ThemeChanged;
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));

            BackColor = palette.Surface;
            topPanel.BackColor = palette.Surface;
            modulePanel.BackColor = palette.SurfaceAlt;
            globalButtonFlow.BackColor = Color.Transparent;
            moduleButtonFlow.BackColor = Color.Transparent;

            lblAppName.BackColor = Color.Transparent;
            lblAppName.ForeColor = palette.TextPrimary;
            lblAppName.Font = ThemeFonts.Section;

            lblProjectName.BackColor = Color.Transparent;
            lblProjectName.ForeColor = palette.TextSecondary;
            lblProjectName.Font = ThemeFonts.Caption;

            lblModuleScope.BackColor = Color.Transparent;
            lblModuleScope.ForeColor = palette.TextSecondary;
            lblModuleScope.Font = ThemeFonts.Caption;

            appIconBox.BackColor = Color.Transparent;
            ReplaceAppIcon();

            headerToolTip.ApplyTheme(palette);
            UpdateProjectCaption();
            UpdateModuleCaption();
            RefreshCommandStates();
            Invalidate(true);
        }

        public void BuildHeaderUI()
        {
            ClearButtonFlow(globalButtonFlow);
            ClearButtonFlow(moduleButtonFlow);
            buttonsByKey.Clear();

            if (ShowTemporaryThemeToggle)
                AddGlobalButton("ThemeToggle", HvacIconKind.ThemeToggle, "Téma váltása", ToggleTheme);
            AddGlobalButton("NavigateBack", HvacIconKind.NavigateBack, "Vissza", () => ServiceLocator.Navigation.GoBack());
            AddGlobalButton("NavigateForward", HvacIconKind.NavigateForward, "Előre", () => ServiceLocator.Navigation.GoForward());
            AddGlobalButton("Undo", HvacIconKind.Undo, "Visszavonás", ShowUndoPlaceholder);
            AddGlobalButton("Redo", HvacIconKind.Redo, "Újra", ShowRedoPlaceholder);
            AddGlobalButton("NewProject", HvacIconKind.NewProject, "Új projekt", CreateNewProject);
            AddGlobalButton("OpenProject", HvacIconKind.OpenProject, "Projekt megnyitása", OpenProject);
            AddGlobalButton("SaveProject", HvacIconKind.SaveProject, "Projekt mentése", () => SaveProjectFromHeader("Mentés"));
            AddGlobalButton("ProjectProperties", HvacIconKind.ProjectProperties, "Projektadatok", EditProjectProperties);
            AddGlobalButton("Settings", HvacIconKind.Settings, "Beállítások", OpenSettings);
            AddGlobalButton("Search", HvacIconKind.Search, "Keresés", ShowSearchPlaceholder);
            AddGlobalButton("Help", HvacIconKind.Help, "Súgó", ShowHelpPlaceholder);
            AddGlobalButton("Info", HvacIconKind.Info, "Névjegy", ShowAboutDialog);

            AddModulePlaceholderButton("ModuleActions", HvacIconKind.Info, "Modul műveletek");

            RefreshCommandStates();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using Pen topLine = new Pen(palette.Border);
            e.Graphics.DrawLine(topLine, 0, LayoutMetrics.TopBarHeight - 1, Width, LayoutMetrics.TopBarHeight - 1);
            e.Graphics.DrawLine(topLine, 0, Height - 1, Width, Height - 1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServiceLocator.Navigation.NavigationStateChanged -= Navigation_NavigationStateChanged;
                ServiceLocator.Project.ProjectChanged -= Project_ProjectChanged;
                ThemeManager.ThemeChanged -= HeaderHost_ThemeChanged;
                headerToolTip.Dispose();
                appIconBox.Image?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitBasicStructure()
        {
            topPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(Width, LayoutMetrics.TopBarHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Tag = "NoTheme"
            };

            modulePanel = new Panel
            {
                Location = new Point(0, LayoutMetrics.TopBarHeight),
                Size = new Size(Width, LayoutMetrics.BottomBarHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Tag = "NoTheme"
            };

            appIconBox = new PictureBox
            {
                Location = new Point(18, 11),
                Size = new Size(32, 32),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Hand,
                Tag = "NoTheme"
            };
            appIconBox.Click += (_, _) => ServiceLocator.Navigation.NavigateTo(ModuleKeys.Dashboard);

            lblAppName = new Label
            {
                Text = "HVAC Designer",
                AutoSize = false,
                Location = new Point(58, 8),
                Size = new Size(BrandBlockWidth - 72, 22),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft
            };
            lblAppName.Click += (_, _) => ServiceLocator.Navigation.NavigateTo(ModuleKeys.Dashboard);

            lblProjectName = new Label
            {
                AutoSize = false,
                Location = new Point(58, 30),
                Size = new Size(BrandBlockWidth - 72, 18),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            globalButtonFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Location = new Point(BrandBlockWidth + 12, 8),
                Tag = "NoTheme"
            };

            lblModuleScope = new Label
            {
                AutoSize = false,
                Location = new Point(LayoutMetrics.NavigationExpandedWidth + 20, 9),
                Size = new Size(210, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            moduleButtonFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Location = new Point(LayoutMetrics.NavigationExpandedWidth + 238, 4),
                Tag = "NoTheme"
            };

            topPanel.Controls.AddRange(new Control[] { appIconBox, lblAppName, lblProjectName, globalButtonFlow });
            modulePanel.Controls.AddRange(new Control[] { lblModuleScope, moduleButtonFlow });
            Controls.Add(modulePanel);
            Controls.Add(topPanel);
        }

        private void AddGlobalButton(
            string key,
            HvacIconKind iconKind,
            string toolTip,
            Action execute)
        {
            EngineeringButton button = CreateHeaderButton(key, iconKind, toolTip, execute);
            globalButtonFlow.Controls.Add(button);
        }

        private void AddModulePlaceholderButton(
            string key,
            HvacIconKind iconKind,
            string toolTip)
        {
            EngineeringButton button = CreateHeaderButton(key, iconKind, toolTip, ShowModuleActionsPlaceholder);
            button.Text = "Modul műveletek";
            button.IconPlacement = EngineeringButtonIconPlacement.Left;
            button.AutoWidth = true;
            button.ShowBorder = true;
            moduleButtonFlow.Controls.Add(button);
        }

        private EngineeringButton CreateHeaderButton(
            string key,
            HvacIconKind iconKind,
            string toolTip,
            Action execute)
        {
            EngineeringButton button = new EngineeringButton
            {
                Text = string.Empty,
                IconKind = iconKind,
                IconPlacement = EngineeringButtonIconPlacement.IconOnly,
                Variant = EngineeringButtonVariant.Ghost,
                ButtonSize = EngineeringButtonSize.Large,
                ShowBorder = false,
                Size = new Size(LayoutMetrics.GlobalButtonSize, LayoutMetrics.GlobalButtonSize),
                BackColor = Color.Transparent,
                ForeColor = palette.TextSecondary,
                Cursor = Cursors.Hand,
                Margin = new Padding(4, 0, 4, 0),
                Tag = "NoTheme"
            };

            button.Click += (_, _) =>
            {
                if (button.Enabled)
                    execute();
            };

            button.ApplyTheme(palette);
            headerToolTip.SetHelp(button, toolTip, kind: EngineeringToolTipKind.Info);
            buttonsByKey[key] = button;
            return button;
        }

        private void ClearButtonFlow(FlowLayoutPanel flow)
        {
            Control[] controls = new Control[flow.Controls.Count];
            flow.Controls.CopyTo(controls, 0);
            flow.Controls.Clear();

            foreach (Control control in controls)
            {
                control.BackgroundImage?.Dispose();
                control.Dispose();
            }
        }

        private void RefreshCommandStates()
        {
            SetButtonEnabled("NavigateBack", ServiceLocator.Navigation.CanGoBack);
            SetButtonEnabled("NavigateForward", ServiceLocator.Navigation.CanGoForward);
            SetButtonEnabled("Undo", false);
            SetButtonEnabled("Redo", false);
            SetButtonEnabled("SaveProject", ServiceLocator.Project.IsProjectLoaded);
            SetButtonEnabled("ProjectProperties", ServiceLocator.Project.IsProjectLoaded);
        }

        private void SetButtonEnabled(string key, bool enabled)
        {
            if (!buttonsByKey.TryGetValue(key, out EngineeringButton? button))
                return;

            button.Enabled = enabled;
            button.Cursor = enabled ? Cursors.Hand : Cursors.Default;
            button.ApplyTheme(palette);
        }

        private void ReplaceAppIcon()
        {
            Image? oldImage = appIconBox.Image;
            appIconBox.Image = HvacIconRenderer.Render(
                HvacIconKind.AppLogo,
                ThemeManager.CurrentThemeMode,
                32,
                palette.Accent);
            oldImage?.Dispose();
        }

        private void UpdateProjectCaption()
        {
            lblProjectName.Text = ServiceLocator.Project.IsProjectLoaded &&
                !string.IsNullOrWhiteSpace(ServiceLocator.Project.CurrentFilePath)
                    ? ServiceLocator.Project.CurrentFileName
                    : "Projekt nincs mentve";
        }

        private void UpdateModuleCaption()
        {
            string module = ServiceLocator.Navigation.CurrentModuleName;
            lblModuleScope.Text = string.IsNullOrWhiteSpace(module)
                ? "Modul műveletek"
                : ModuleDisplayNames.Get(module);
        }

        private void ToggleTheme()
        {
            ThemeManager.CurrentThemeMode = ThemeManager.CurrentThemeMode == AppThemeMode.Dark
                ? AppThemeMode.Light
                : AppThemeMode.Dark;
        }

        private void CreateNewProject()
        {
            using SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "HVAC Projektfájl (*.hvc)|*.hvc",
                Title = "Új gépészeti projektfájl létrehozása",
                FileName = "uj_projekt.hvc"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            ProjectOperationResult result = ServiceLocator.Project.CreateNew(dialog.FileName);
            if (!result.Succeeded)
            {
                ShowProjectOperationResult(result, "Projektkezelő");
                return;
            }

            using ProjectPropertiesForm propsForm =
                new ProjectPropertiesForm(ServiceLocator.Project.CurrentProject, true);
            if (propsForm.ShowDialog() == DialogResult.OK)
            {
                ShowProjectOperationResult(
                    ServiceLocator.Project.Save(),
                    "Projektkezelő");
            }
        }

        private void OpenProject()
        {
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "HVAC Projektfájl (*.hvc)|*.hvc",
                Title = "Mérnöki projektfájl betöltése"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ShowProjectOperationResult(
                    ServiceLocator.Project.Open(dialog.FileName),
                    "Projektkezelő");
            }
        }

        private void EditProjectProperties()
        {
            if (!ServiceLocator.Project.IsProjectLoaded)
            {
                EngineeringDialog.ShowMessage(
                    FindForm(),
                    "Projektadatok",
                    "Nincs aktív betöltött projekt, amit szerkeszteni lehetne.",
                    EngineeringDialogSeverity.Warning,
                    HvacIconKind.Info);
                return;
            }

            using ProjectPropertiesForm propsForm =
                new ProjectPropertiesForm(ServiceLocator.Project.CurrentProject, false);
            if (propsForm.ShowDialog() == DialogResult.OK)
                SaveProjectFromHeader("Projektkezelő");
        }

        private static void OpenSettings()
        {
            using SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
        }

        private void ShowSearchPlaceholder()
        {
            EngineeringDialog.ShowMessage(
                FindForm(),
                "Keresés",
                "A globális kereső alapja bekerült a fejlécbe. A keresőmotor későbbi réteg lesz.",
                EngineeringDialogSeverity.Info,
                HvacIconKind.Search);
        }

        private void ShowHelpPlaceholder()
        {
            EngineeringDialog.ShowMessage(
                FindForm(),
                "Súgó",
                "A súgó központ később modulfüggő leírásokat és gyors magyarázatokat jelenít meg.",
                EngineeringDialogSeverity.Info,
                HvacIconKind.Help);
        }

        private void ShowModuleActionsPlaceholder()
        {
            EngineeringDialog.ShowMessage(
                FindForm(),
                "Modul műveletek",
                "Ide kerülnek majd az aktív modul saját parancsai.",
                EngineeringDialogSeverity.Info,
                HvacIconKind.Info);
        }

        private void ShowUndoPlaceholder()
        {
            EngineeringDialog.ShowMessage(
                FindForm(),
                "Visszavonás",
                "A globális command history szolgáltatás még nincs bekötve.",
                EngineeringDialogSeverity.Info,
                HvacIconKind.Undo);
        }

        private void ShowRedoPlaceholder()
        {
            EngineeringDialog.ShowMessage(
                FindForm(),
                "Újra",
                "A globális command history szolgáltatás még nincs bekötve.",
                EngineeringDialogSeverity.Info,
                HvacIconKind.Redo);
        }

        private void HeaderHost_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            if (IsDisposed)
                return;

            ApplyTheme(e.Palette);
            BuildHeaderUI();
        }

        private void Navigation_NavigationStateChanged(object? sender, EventArgs e)
        {
            UpdateModuleCaption();
            RefreshCommandStates();
        }

        private void Project_ProjectChanged(object? sender, EventArgs e)
        {
            UpdateProjectCaption();
            RefreshCommandStates();
        }

        private void ShowAboutDialog()
        {
            using EngineeringDialog dialog = new EngineeringDialog
            {
                DialogTitle = "HVAC Designer Suite",
                DialogSubtitle = "Verzió: 1.0.0 Pro (2026. 07. 12.)",
                IconKind = HvacIconKind.Info,
                Severity = EngineeringDialogSeverity.Info,
                ButtonSet = EngineeringDialogButtonSet.Ok,
                DialogSize = EngineeringDialogSize.Small
            };

            Label description = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Fejlesztve gépészmérnöki tervezőirodák részére.\nMegfelelőség: MMK gépészeti és energetikai irányelvek.",
                ForeColor = ThemeManager.CurrentPalette.TextSecondary,
                Font = ThemeFonts.Body,
                TextAlign = ContentAlignment.MiddleLeft
            };
            dialog.ContentPanel.Controls.Add(description);
            dialog.ShowDialog(FindForm());
        }

        private void ShowProjectOperationResult(
            ProjectOperationResult result,
            string title)
        {
            EngineeringDialog.ShowMessage(
                FindForm(),
                title,
                result.Message,
                result.Succeeded ? EngineeringDialogSeverity.Success : EngineeringDialogSeverity.Danger,
                result.Succeeded ? HvacIconKind.Certification : HvacIconKind.Safety);
        }

        private void SaveProjectFromHeader(string title)
        {
            ProjectOperationResult result = ServiceLocator.Project.Save();
            if (result.Status != ProjectOperationStatus.MissingFilePath)
            {
                ShowProjectOperationResult(result, title);
                return;
            }

            using SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "HVAC Projektfájl (*.hvc)|*.hvc",
                Title = "Projekt mentése",
                FileName = ServiceLocator.Project.CurrentProject.Name + ".hvc"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ShowProjectOperationResult(
                    ServiceLocator.Project.SaveAs(dialog.FileName),
                    title);
            }
        }
    }
}
