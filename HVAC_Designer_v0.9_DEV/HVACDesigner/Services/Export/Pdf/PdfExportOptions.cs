using System;

namespace HVACDesigner.Services.Export.Pdf
{
    public enum PdfPaperSize
    {
        A4,
        A3
    }

    public enum PdfPageOrientation
    {
        Portrait,
        Landscape
    }

    [Serializable]
    public sealed class PdfExportOptions
    {
        public bool IncludeProjectData { get; set; } = true;
        public bool IncludeDesignerData { get; set; } = true;
        public bool IncludeFixtures { get; set; } = true;
        public bool IncludeCalculationInputs { get; set; } = true;
        public bool IncludeResults { get; set; } = true;
        public bool IncludeStandards { get; set; } = true;
        public bool IncludeNotes { get; set; } = true;
        public bool IncludeSignature { get; set; } = true;
        public bool IncludeFooter { get; set; } = true;
        public bool IncludePageNumbers { get; set; } = true;
        public bool IncludeDate { get; set; } = true;
        public bool IncludeProgramVersion { get; set; } = true;
        public PdfPaperSize PaperSize { get; set; } = PdfPaperSize.A4;
        public PdfPageOrientation Orientation { get; set; } = PdfPageOrientation.Portrait;
        public string Notes { get; set; } = "";

        public PdfExportOptions Clone()
        {
            return (PdfExportOptions)MemberwiseClone();
        }
    }
}
