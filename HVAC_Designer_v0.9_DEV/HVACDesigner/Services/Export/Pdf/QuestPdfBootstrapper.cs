using QuestPDF.Infrastructure;

namespace HVACDesigner.Services.Export.Pdf
{
    public static class QuestPdfBootstrapper
    {
        public static void Initialize()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }
    }
}
