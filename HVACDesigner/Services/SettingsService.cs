using System;
using System.IO;
using System.Xml.Serialization;

namespace HVACDesigner.Services
{
    public enum SettingsOperationStatus
    {
        Success,
        DefaultCreated,
        FallbackToDefault,
        Failed
    }

    public sealed class SettingsOperationResult
    {
        private SettingsOperationResult(
            SettingsOperationStatus status,
            string message,
            UserSettings settings,
            string filePath,
            Exception? exception = null)
        {
            Status = status;
            Message = message ?? string.Empty;
            Settings = settings ?? new UserSettings();
            FilePath = filePath ?? string.Empty;
            Exception = exception;
        }

        public SettingsOperationStatus Status { get; }
        public string Message { get; }
        public UserSettings Settings { get; }
        public string FilePath { get; }
        public Exception? Exception { get; }
        public bool Succeeded =>
            Status == SettingsOperationStatus.Success ||
            Status == SettingsOperationStatus.DefaultCreated ||
            Status == SettingsOperationStatus.FallbackToDefault;

        public static SettingsOperationResult Success(
            SettingsOperationStatus status,
            string message,
            UserSettings settings,
            string filePath)
        {
            return new SettingsOperationResult(status, message, settings, filePath);
        }

        public static SettingsOperationResult Failure(
            string message,
            UserSettings settings,
            string filePath,
            Exception exception)
        {
            return new SettingsOperationResult(
                SettingsOperationStatus.Failed,
                message,
                settings,
                filePath,
                exception);
        }
    }

    public class SettingsService
    {
        private readonly XmlSerializer _serializer =
            new XmlSerializer(typeof(UserSettings));

        public SettingsService()
            : this(new ApplicationPaths())
        {
        }

        public SettingsService(ApplicationPaths paths)
        {
            Paths = paths ?? throw new ArgumentNullException(nameof(paths));
            SelectedSettings = new UserSettings();
            LastLoadResult = LoadSettings();
        }

        public ApplicationPaths Paths { get; }
        public UserSettings SelectedSettings { get; set; }
        public SettingsOperationResult? LastLoadResult { get; private set; }
        public SettingsOperationResult? LastSaveResult { get; private set; }

        public SettingsOperationResult LoadSettings()
        {
            try
            {
                Paths.EnsureUserDirectories();

                string loadPath = ResolveSettingsLoadPath();

                if (!File.Exists(loadPath))
                {
                    SelectedSettings = CreateDefaultSettings();
                    UnitSettingsMapper.ApplyToUnitContext(SelectedSettings.Units);

                    LastLoadResult = SettingsOperationResult.Success(
                        SettingsOperationStatus.DefaultCreated,
                        "Nem volt korábbi beállításfájl, alapértelmezett beállítások jöttek létre.",
                        SelectedSettings,
                        Paths.SettingsFilePath);
                    return LastLoadResult;
                }

                using FileStream stream = File.OpenRead(loadPath);
                SelectedSettings =
                    (UserSettings?)_serializer.Deserialize(stream) ??
                    CreateDefaultSettings();

                NormalizeLoadedSettings();
                UnitSettingsMapper.ApplyToUnitContext(SelectedSettings.Units);

                LastLoadResult = SettingsOperationResult.Success(
                    SettingsOperationStatus.Success,
                    loadPath == Paths.LegacySettingsFilePath
                        ? "A régi beállításfájl sikeresen betöltve, a következő mentés az új helyre kerül."
                        : "A beállítások sikeresen betöltve.",
                    SelectedSettings,
                    loadPath);
                return LastLoadResult;
            }
            catch (Exception exception)
            {
                SelectedSettings = CreateDefaultSettings();
                UnitSettingsMapper.ApplyToUnitContext(SelectedSettings.Units);

                LastLoadResult = SettingsOperationResult.Success(
                    SettingsOperationStatus.FallbackToDefault,
                    "A beállításfájl nem volt betölthető, alapértelmezett beállítások aktívak: " +
                    exception.Message,
                    SelectedSettings,
                    Paths.SettingsFilePath);
                return LastLoadResult;
            }
        }

        public SettingsOperationResult SaveSettings()
        {
            try
            {
                Paths.EnsureUserDirectories();
                SelectedSettings ??= CreateDefaultSettings();
                SelectedSettings.Units = UnitSettingsMapper.CaptureFromUnitContext();
                NormalizeLoadedSettings();

                using FileStream stream = File.Create(Paths.SettingsFilePath);
                _serializer.Serialize(stream, SelectedSettings);

                LastSaveResult = SettingsOperationResult.Success(
                    SettingsOperationStatus.Success,
                    "A beállítások sikeresen mentve.",
                    SelectedSettings,
                    Paths.SettingsFilePath);
                return LastSaveResult;
            }
            catch (Exception exception)
            {
                LastSaveResult = SettingsOperationResult.Failure(
                    "A beállítások mentése nem sikerült: " + exception.Message,
                    SelectedSettings ?? new UserSettings(),
                    Paths.SettingsFilePath,
                    exception);
                return LastSaveResult;
            }
        }

        public SettingsOperationResult ResetToDefaults()
        {
            SelectedSettings = CreateDefaultSettings();
            UnitSettingsMapper.ApplyToUnitContext(SelectedSettings.Units);
            return SaveSettings();
        }

        public void AddRecentProject(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            SelectedSettings ??= CreateDefaultSettings();
            SelectedSettings.RecentProjects.RemoveAll(item =>
                string.Equals(item, filePath, StringComparison.OrdinalIgnoreCase));
            SelectedSettings.RecentProjects.Insert(0, filePath);

            if (SelectedSettings.RecentProjects.Count > 10)
                SelectedSettings.RecentProjects.RemoveRange(
                    10,
                    SelectedSettings.RecentProjects.Count - 10);
        }

        private UserSettings CreateDefaultSettings()
        {
            UserSettings settings = new UserSettings();
            settings.Paths.AutoSavePath = Paths.AutoSaveRoot;
            settings.Paths.LogFolder = Paths.LogsRoot;
            settings.Paths.CacheFolder = Paths.CacheRoot;
            settings.Paths.UserXmlFolder = Paths.UserXmlRoot;
            settings.Paths.ExportFolder = Paths.ExportsRoot;
            settings.Units = UnitSettingsMapper.CaptureFromUnitContext();
            settings.Normalize();
            return settings;
        }

        private string ResolveSettingsLoadPath()
        {
            if (File.Exists(Paths.SettingsFilePath))
                return Paths.SettingsFilePath;

            if (File.Exists(Paths.LegacySettingsFilePath))
                return Paths.LegacySettingsFilePath;

            return Paths.SettingsFilePath;
        }

        private void NormalizeLoadedSettings()
        {
            SelectedSettings ??= CreateDefaultSettings();
            SelectedSettings.Normalize();

            if (string.IsNullOrWhiteSpace(SelectedSettings.Paths.AutoSavePath))
                SelectedSettings.Paths.AutoSavePath = Paths.AutoSaveRoot;
            if (string.IsNullOrWhiteSpace(SelectedSettings.Paths.LogFolder))
                SelectedSettings.Paths.LogFolder = Paths.LogsRoot;
            if (string.IsNullOrWhiteSpace(SelectedSettings.Paths.CacheFolder))
                SelectedSettings.Paths.CacheFolder = Paths.CacheRoot;
            if (string.IsNullOrWhiteSpace(SelectedSettings.Paths.UserXmlFolder))
                SelectedSettings.Paths.UserXmlFolder = Paths.UserXmlRoot;
            if (string.IsNullOrWhiteSpace(SelectedSettings.Paths.ExportFolder))
                SelectedSettings.Paths.ExportFolder = Paths.ExportsRoot;
        }
    }
}
