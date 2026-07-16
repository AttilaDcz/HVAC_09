using System;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Services;
using HVACDesigner.CoreUI.Status;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.Services;

namespace HVACDesigner.CoreUI.Layout
{
    public class ContentHost : HostBase, IThemeable
    {
        private UserControl? currentActiveModule;

        public ContentHost()
            : base("ContentHostZone")
        {
            ApplyTheme(ThemeManager.CurrentPalette);
            Resize += ContentHost_Resize;
            ServiceLocator.Navigation.NavigationRequested += OnModuleNavigation;
            ServiceLocator.Project.ProjectChanged += OnProjectChanged;
        }

        public void ApplyTheme(ThemePalette palette)
        {
            BackColor = palette.Window;

            if (currentActiveModule != null)
                ThemeApplicator.ApplyTheme(currentActiveModule, palette);

            Invalidate(true);
        }

        private void OnProjectChanged(object? sender, EventArgs e)
        {
            // Töröljük a modul gyorsítótárat (ez meghívja a Dispose-t rajtuk)
            ServiceLocator.Modules.ClearCache();

            // Újratöltjük a jelenleg aktív modult
            string currentModule = ServiceLocator.Navigation.CurrentModuleName;
            if (!string.IsNullOrEmpty(currentModule))
            {
                OnModuleNavigation(currentModule);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServiceLocator.Navigation.NavigationRequested -= OnModuleNavigation;
                ServiceLocator.Project.ProjectChanged -= OnProjectChanged;
            }
            base.Dispose(disposing);
        }

        private void ContentHost_Resize(object? sender, EventArgs e)
        {
            if (currentActiveModule == null)
                return;

            currentActiveModule.Location = new Point(0, 0);
            currentActiveModule.Size = Size;
        }

        private void OnModuleNavigation(string moduleName)
        {
            EngineeringStatusMessages.SetModule(ModuleDisplayNames.Get(moduleName));
            SuspendLayout();

            Controls.Clear();
            currentActiveModule = ServiceLocator.ModuleRegistry.GetOrCreate(
                moduleName,
                ServiceLocator.Current);

            if (currentActiveModule == null)
                currentActiveModule = CreateMissingModulePlaceholder(moduleName);

            if (currentActiveModule != null)
            {
                currentActiveModule.Location = new Point(0, 0);
                currentActiveModule.Size = Size;
                Controls.Add(currentActiveModule);
                ThemeApplicator.ApplyTheme(
                    currentActiveModule,
                    ThemeManager.CurrentPalette);
            }

            ResumeLayout(true);
        }

        private static UserControl CreateMissingModulePlaceholder(string moduleName)
        {
            ThemePalette palette = ThemeManager.CurrentPalette;
            var module = new UserControl
            {
                BackColor = palette.Window
            };

            var label = new Label
            {
                Text = $"Nincs regisztralt modul: {moduleName}",
                Font = ThemeFonts.Subtitle,
                ForeColor = palette.Warning,
                AutoSize = true,
                Location = new Point(40, 40)
            };

            module.Controls.Add(label);
            return module;
        }
    }
}
