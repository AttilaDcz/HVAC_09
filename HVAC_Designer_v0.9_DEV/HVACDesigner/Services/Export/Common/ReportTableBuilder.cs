using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HVACDesigner.Services.Export.Common
{
    public static class ReportTableBuilder
    {
        public static IContainer HeaderCell(this IContainer container)
        {
            return container
                .Background(ReportStyles.HeaderBackground)
                .Border(1)
                .BorderColor(ReportStyles.Border)
                .PaddingVertical(5)
                .PaddingHorizontal(6);
        }

        public static IContainer BodyCell(this IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3)
                .PaddingVertical(4)
                .PaddingHorizontal(6);
        }
    }
}
