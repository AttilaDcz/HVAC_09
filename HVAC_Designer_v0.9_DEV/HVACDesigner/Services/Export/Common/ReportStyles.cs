using QuestPDF.Helpers;

namespace HVACDesigner.Services.Export.Common
{
    public static class ReportStyles
    {
        public const string FontFamily = "Segoe UI";
        public static readonly string Text = Colors.Grey.Darken4;
        public static readonly string Muted = Colors.Grey.Darken1;
        public static readonly string Border = Colors.Grey.Lighten2;
        public static readonly string HeaderBackground = Colors.BlueGrey.Lighten5;
        public static readonly string ResultBorder = Colors.Blue.Medium;
        public static readonly string ResultBackground = Colors.Blue.Lighten5;
        public static readonly string Warning = Colors.Orange.Medium;
    }
}
