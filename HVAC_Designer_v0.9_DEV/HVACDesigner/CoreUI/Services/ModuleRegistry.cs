using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.Services;

namespace HVACDesigner.CoreUI.Services
{
    public sealed class ModuleRegistry
    {
        private readonly Dictionary<string, ModuleRegistration> registrations =
            new Dictionary<string, ModuleRegistration>(StringComparer.OrdinalIgnoreCase);

        private readonly ModuleService moduleCache;

        public ModuleRegistry(ModuleService moduleCache)
        {
            this.moduleCache = moduleCache ??
                throw new ArgumentNullException(nameof(moduleCache));
        }

        public void Register(
            string moduleKey,
            Func<ApplicationServices, UserControl> factory,
            bool cache = true)
        {
            if (string.IsNullOrWhiteSpace(moduleKey))
                throw new ArgumentException("A modul azonositoja nem lehet ures.", nameof(moduleKey));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            registrations[moduleKey.Trim()] = new ModuleRegistration(factory, cache);
        }

        public void RegisterPlaceholder(
            string moduleKey,
            string discipline,
            string description,
            bool cache = false)
        {
            Register(
                moduleKey,
                _ => CreatePlaceholder(
                    ModuleDisplayNames.Get(moduleKey),
                    discipline,
                    description),
                cache);
        }

        public UserControl? GetOrCreate(
            string moduleKey,
            ApplicationServices services)
        {
            if (string.IsNullOrWhiteSpace(moduleKey))
                return null;

            if (!registrations.TryGetValue(
                moduleKey.Trim(),
                out ModuleRegistration? registration))
            {
                return null;
            }

            if (!registration.Cache)
                return registration.Factory(services);

            return moduleCache.GetModule(
                moduleKey,
                () => registration.Factory(services));
        }

        public bool IsRegistered(string moduleKey)
        {
            return !string.IsNullOrWhiteSpace(moduleKey) &&
                registrations.ContainsKey(moduleKey.Trim());
        }

        public IReadOnlyCollection<string> GetRegisteredModuleNames()
        {
            return registrations.Keys;
        }

        private static UserControl CreatePlaceholder(
            string moduleName,
            string discipline,
            string description)
        {
            ThemePalette palette = ThemeManager.CurrentPalette;
            var module = new UserControl
            {
                BackColor = palette.Window
            };

            var label = new Label
            {
                Text = $"[{discipline}] {moduleName} - {description}",
                Font = ThemeFonts.Subtitle,
                ForeColor = palette.TextSecondary,
                AutoSize = true,
                Location = new Point(40, 40)
            };

            module.Controls.Add(label);
            return module;
        }

        private sealed class ModuleRegistration
        {
            public ModuleRegistration(
                Func<ApplicationServices, UserControl> factory,
                bool cache)
            {
                Factory = factory;
                Cache = cache;
            }

            public Func<ApplicationServices, UserControl> Factory { get; }
            public bool Cache { get; }
        }
    }
}
