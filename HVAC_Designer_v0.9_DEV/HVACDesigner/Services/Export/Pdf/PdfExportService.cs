using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HVACDesigner.Services.Export.Pdf
{
    public sealed class PdfExportService
    {
        public void Export(IDocument document, string filePath)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A PDF fájl útvonala nem lehet üres.", nameof(filePath));

            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            string targetDirectory = string.IsNullOrWhiteSpace(directory)
                ? Directory.GetCurrentDirectory()
                : directory;

            string temporaryPath = Path.Combine(
                targetDirectory,
                Path.GetFileNameWithoutExtension(filePath) +
                "." +
                Guid.NewGuid().ToString("N") +
                ".tmp.pdf");

            try
            {
                document.GeneratePdf(temporaryPath);

                FileInfo generated = new FileInfo(temporaryPath);
                if (!generated.Exists || generated.Length == 0)
                {
                    throw new InvalidOperationException(
                        "A PDF motor nem hozott létre érvényes dokumentumot.");
                }

                if (File.Exists(filePath))
                    File.Delete(filePath);

                File.Move(temporaryPath, filePath);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }
    }
}
