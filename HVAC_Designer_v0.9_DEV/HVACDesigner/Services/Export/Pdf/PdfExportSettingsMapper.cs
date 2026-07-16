using System;
using System.Linq;

namespace HVACDesigner.Services.Export.Pdf
{
    public static class PdfExportSettingsMapper
    {
        public static PdfExportOptions Load(UserSettings settings, string moduleId)
        {
            settings ??= new UserSettings();
            settings.Normalize();

            PdfModuleExportSettingsData? stored =
                settings.PdfExportSettings.FirstOrDefault(item =>
                    string.Equals(
                        item.ModuleId,
                        moduleId,
                        StringComparison.OrdinalIgnoreCase));

            if (stored == null)
                return new PdfExportOptions();

            return new PdfExportOptions
            {
                IncludeProjectData = stored.IncludeProjectData,
                IncludeDesignerData = stored.IncludeDesignerData,
                IncludeFixtures = stored.IncludeFixtures,
                IncludeCalculationInputs = stored.IncludeCalculationInputs,
                IncludeResults = stored.IncludeResults,
                IncludeStandards = stored.IncludeStandards,
                IncludeNotes = stored.IncludeNotes,
                IncludeSignature = stored.IncludeSignature,
                IncludeFooter = stored.IncludeFooter,
                IncludePageNumbers = stored.IncludePageNumbers,
                IncludeDate = stored.IncludeDate,
                IncludeProgramVersion = stored.IncludeProgramVersion,
                PaperSize = Enum.TryParse(stored.PaperSize, out PdfPaperSize paperSize)
                    ? paperSize
                    : PdfPaperSize.A4,
                Orientation = Enum.TryParse(stored.Orientation, out PdfPageOrientation orientation)
                    ? orientation
                    : PdfPageOrientation.Portrait
            };
        }

        public static void Save(
            UserSettings settings,
            string moduleId,
            PdfExportOptions options)
        {
            if (settings == null || string.IsNullOrWhiteSpace(moduleId) || options == null)
                return;

            settings.Normalize();

            PdfModuleExportSettingsData? stored =
                settings.PdfExportSettings.FirstOrDefault(item =>
                    string.Equals(
                        item.ModuleId,
                        moduleId,
                        StringComparison.OrdinalIgnoreCase));

            if (stored == null)
            {
                stored = new PdfModuleExportSettingsData
                {
                    ModuleId = moduleId.Trim()
                };
                settings.PdfExportSettings.Add(stored);
            }

            stored.IncludeProjectData = options.IncludeProjectData;
            stored.IncludeDesignerData = options.IncludeDesignerData;
            stored.IncludeFixtures = options.IncludeFixtures;
            stored.IncludeCalculationInputs = options.IncludeCalculationInputs;
            stored.IncludeResults = options.IncludeResults;
            stored.IncludeStandards = options.IncludeStandards;
            stored.IncludeNotes = options.IncludeNotes;
            stored.IncludeSignature = options.IncludeSignature;
            stored.IncludeFooter = options.IncludeFooter;
            stored.IncludePageNumbers = options.IncludePageNumbers;
            stored.IncludeDate = options.IncludeDate;
            stored.IncludeProgramVersion = options.IncludeProgramVersion;
            stored.PaperSize = options.PaperSize.ToString();
            stored.Orientation = options.Orientation.ToString();
        }
    }
}
