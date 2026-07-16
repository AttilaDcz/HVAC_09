using System;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace HVACDesigner.Services
{
    public enum ProjectOperationStatus
    {
        Success,
        Cancelled,
        NoActiveProject,
        MissingFilePath,
        FileNotFound,
        InvalidProjectFile,
        Failed
    }

    public sealed class ProjectOperationResult
    {
        private ProjectOperationResult(
            ProjectOperationStatus status,
            string message,
            string filePath = "",
            Exception? exception = null,
            ProjectData? project = null)
        {
            Status = status;
            Message = message ?? string.Empty;
            FilePath = filePath ?? string.Empty;
            Exception = exception;
            Project = project;
        }

        public ProjectOperationStatus Status { get; }
        public string Message { get; }
        public string FilePath { get; }
        public Exception? Exception { get; }
        public ProjectData? Project { get; }
        public bool Succeeded => Status == ProjectOperationStatus.Success;

        public static ProjectOperationResult Success(
            string message,
            string filePath,
            ProjectData? project = null)
        {
            return new ProjectOperationResult(
                ProjectOperationStatus.Success,
                message,
                filePath,
                project: project);
        }

        public static ProjectOperationResult Failure(
            ProjectOperationStatus status,
            string message,
            string filePath = "",
            Exception? exception = null)
        {
            return new ProjectOperationResult(
                status,
                message,
                filePath,
                exception);
        }
    }

    public class ProjectService : IDisposable
    {
        private readonly System.Threading.Timer _autoSaveTimer;
        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions { WriteIndented = true };

        private ProjectData _currentProject;
        private string? _currentFilePath;
        private bool _disposed;

        public ProjectService()
        {
            _currentProject = ProjectData.CreateNew("Új Projekt");
            _autoSaveTimer = new System.Threading.Timer(
                _ => AutoSave(),
                null,
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(5));
        }

        public event EventHandler? ProjectChanged;

        public ProjectData CurrentProject
        {
            get => _currentProject;
            set
            {
                _currentProject = value ?? ProjectData.CreateNew("Új Projekt");
                _currentProject.NormalizeAfterLoad();
                ProjectChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string CurrentFilePath => _currentFilePath ?? string.Empty;
        public string CurrentFileName => string.IsNullOrEmpty(_currentFilePath)
            ? "Névtelen"
            : Path.GetFileName(_currentFilePath);
        public bool IsProjectLoaded => _currentProject != null;

        public void NotifyProjectChanged()
        {
            _currentProject?.NormalizeAfterLoad();
            ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

        public ProjectOperationResult CreateNew(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.MissingFilePath,
                    "Nincs megadva projektfájl útvonal.");
            }

            try
            {
                string normalizedPath = Path.GetFullPath(filePath);
                EnsureDirectoryExists(normalizedPath);

                ProjectData project =
                    ProjectData.CreateNew(Path.GetFileNameWithoutExtension(normalizedPath));

                WriteProjectFile(normalizedPath, project);

                _currentProject = project;
                _currentFilePath = normalizedPath;
                ProjectChanged?.Invoke(this, EventArgs.Empty);

                return ProjectOperationResult.Success(
                    "A projekt sikeresen létrejött.",
                    normalizedPath,
                    project);
            }
            catch (Exception exception)
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.Failed,
                    "A projekt létrehozása nem sikerült: " + exception.Message,
                    filePath,
                    exception);
            }
        }

        public ProjectOperationResult Open(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.MissingFilePath,
                    "Nincs megadva megnyitandó projektfájl.");
            }

            try
            {
                string normalizedPath = Path.GetFullPath(filePath);
                if (!File.Exists(normalizedPath))
                {
                    return ProjectOperationResult.Failure(
                        ProjectOperationStatus.FileNotFound,
                        "A projektfájl nem található.",
                        normalizedPath);
                }

                string content = File.ReadAllText(normalizedPath);
                ProjectData? project =
                    JsonSerializer.Deserialize<ProjectData>(content, _jsonOptions);

                if (project == null)
                {
                    return ProjectOperationResult.Failure(
                        ProjectOperationStatus.InvalidProjectFile,
                        "A projektfájl nem tartalmaz érvényes projektadatot.",
                        normalizedPath);
                }

                project.NormalizeAfterLoad();
                _currentProject = project;
                _currentFilePath = normalizedPath;
                ProjectChanged?.Invoke(this, EventArgs.Empty);

                return ProjectOperationResult.Success(
                    "A projekt sikeresen betöltve.",
                    normalizedPath,
                    project);
            }
            catch (JsonException exception)
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.InvalidProjectFile,
                    "A projektfájl formátuma nem érvényes: " + exception.Message,
                    filePath,
                    exception);
            }
            catch (Exception exception)
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.Failed,
                    "A projekt megnyitása nem sikerült: " + exception.Message,
                    filePath,
                    exception);
            }
        }

        public ProjectOperationResult Save()
        {
            if (!IsProjectLoaded)
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.NoActiveProject,
                    "Nincs aktív projekt.");
            }

            if (string.IsNullOrWhiteSpace(_currentFilePath))
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.MissingFilePath,
                    "Az aktív projekthez még nincs mentési útvonal.");
            }

            return SaveAs(_currentFilePath);
        }

        public ProjectOperationResult SaveAs(string filePath)
        {
            if (!IsProjectLoaded)
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.NoActiveProject,
                    "Nincs aktív projekt.");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.MissingFilePath,
                    "Nincs megadva mentési útvonal.");
            }

            try
            {
                string normalizedPath = Path.GetFullPath(filePath);
                EnsureDirectoryExists(normalizedPath);
                WriteProjectFile(normalizedPath, _currentProject);

                _currentFilePath = normalizedPath;
                ProjectChanged?.Invoke(this, EventArgs.Empty);

                return ProjectOperationResult.Success(
                    "A projekt sikeresen mentve.",
                    normalizedPath,
                    _currentProject);
            }
            catch (Exception exception)
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.Failed,
                    "A projekt mentése nem sikerült: " + exception.Message,
                    filePath,
                    exception);
            }
        }

        public ProjectOperationResult AutoSave()
        {
            if (!IsProjectLoaded)
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.NoActiveProject,
                    "Nincs aktív projekt.");
            }

            if (string.IsNullOrWhiteSpace(_currentFilePath))
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.MissingFilePath,
                    "Az autosave csak mentett projektfájl mellett fut.");
            }

            try
            {
                string autoSavePath = _currentFilePath + ".bak";
                WriteProjectFile(autoSavePath, _currentProject);

                return ProjectOperationResult.Success(
                    "Automatikus mentés elkészült.",
                    autoSavePath,
                    _currentProject);
            }
            catch (Exception exception)
            {
                return ProjectOperationResult.Failure(
                    ProjectOperationStatus.Failed,
                    "Az automatikus mentés nem sikerült: " + exception.Message,
                    _currentFilePath + ".bak",
                    exception);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _autoSaveTimer.Dispose();
            _disposed = true;
        }

        private void WriteProjectFile(string filePath, ProjectData project)
        {
            project.PrepareForSave();
            string serializedData = JsonSerializer.Serialize(project, _jsonOptions);
            File.WriteAllText(filePath, serializedData);
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);
        }
    }
}
