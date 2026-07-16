using System;
using System.IO;

namespace HVACDesigner.Services
{
    public sealed class ApplicationPaths
    {
        public ApplicationPaths()
            : this(AppContext.BaseDirectory)
        {
        }

        public ApplicationPaths(string installRoot)
        {
            InstallRoot = Path.GetFullPath(
                string.IsNullOrWhiteSpace(installRoot)
                    ? AppContext.BaseDirectory
                    : installRoot);

            FactoryDataXmlRoot = Path.Combine(InstallRoot, "Data", "Xml");
            ResourcesRoot = Path.Combine(InstallRoot, "Resources");

            LocalAppDataRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HVAC Designer");

            SettingsRoot = Path.Combine(LocalAppDataRoot, "Settings");
            AutoSaveRoot = Path.Combine(LocalAppDataRoot, "Autosave");
            LogsRoot = Path.Combine(LocalAppDataRoot, "Logs");
            CacheRoot = Path.Combine(LocalAppDataRoot, "Cache");
            UserDataRoot = Path.Combine(LocalAppDataRoot, "UserData");
            UserXmlRoot = Path.Combine(UserDataRoot, "Xml");
            ProjectTemplatesRoot = Path.Combine(UserDataRoot, "ProjectTemplates");
            ExportsRoot = Path.Combine(UserDataRoot, "Exports");
            SettingsFilePath = Path.Combine(SettingsRoot, "usersettings.config");
            LegacySettingsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HVACDesigner",
                "usersettings.config");
        }

        public string InstallRoot { get; }
        public string FactoryDataXmlRoot { get; }
        public string ResourcesRoot { get; }
        public string LocalAppDataRoot { get; }
        public string SettingsRoot { get; }
        public string AutoSaveRoot { get; }
        public string LogsRoot { get; }
        public string CacheRoot { get; }
        public string UserDataRoot { get; }
        public string UserXmlRoot { get; }
        public string ProjectTemplatesRoot { get; }
        public string ExportsRoot { get; }
        public string SettingsFilePath { get; }
        public string LegacySettingsFilePath { get; }

        public string GetModuleDataPath(string moduleId)
        {
            return Path.Combine(UserDataRoot, "Modules", NormalizeModuleId(moduleId));
        }

        public string GetModuleCachePath(string moduleId)
        {
            return Path.Combine(CacheRoot, "Modules", NormalizeModuleId(moduleId));
        }

        public string GetModuleAutoSavePath(string moduleId)
        {
            return Path.Combine(AutoSaveRoot, "Modules", NormalizeModuleId(moduleId));
        }

        public void EnsureUserDirectories()
        {
            Directory.CreateDirectory(LocalAppDataRoot);
            Directory.CreateDirectory(SettingsRoot);
            Directory.CreateDirectory(AutoSaveRoot);
            Directory.CreateDirectory(LogsRoot);
            Directory.CreateDirectory(CacheRoot);
            Directory.CreateDirectory(UserDataRoot);
            Directory.CreateDirectory(UserXmlRoot);
            Directory.CreateDirectory(ProjectTemplatesRoot);
            Directory.CreateDirectory(ExportsRoot);
        }

        private static string NormalizeModuleId(string moduleId)
        {
            string value = string.IsNullOrWhiteSpace(moduleId)
                ? "general"
                : moduleId.Trim().ToLowerInvariant();

            foreach (char invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '_');

            return value;
        }
    }
}
