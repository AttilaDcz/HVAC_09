using System;
using HVACDesigner.Services.Export.Pdf;

namespace HVACDesigner.Services.Export.Reports
{
    public sealed class ReportContext
    {
        public ReportContext(
            ProjectData project,
            UserSettings settings,
            PdfExportOptions options,
            ApplicationVersionService versionService,
            DateTime createdAt)
        {
            Project = project ?? new ProjectData();
            Settings = settings ?? new UserSettings();
            Options = options ?? new PdfExportOptions();
            VersionService = versionService ?? new ApplicationVersionService();
            CreatedAt = createdAt;
        }

        public ProjectData Project { get; }
        public UserSettings Settings { get; }
        public PdfExportOptions Options { get; }
        public ApplicationVersionService VersionService { get; }
        public DateTime CreatedAt { get; }
    }
}
