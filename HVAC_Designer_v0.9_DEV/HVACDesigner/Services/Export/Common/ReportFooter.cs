using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using HVACDesigner.Services.Export.Reports;

namespace HVACDesigner.Services.Export.Common
{
    public static class ReportFooter
    {
        public static void Compose(IContainer container, ReportContext context)
        {
            if (!context.Options.IncludeFooter)
                return;

            container.Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(ReportStyles.Border);
                column.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.DefaultTextStyle(style => style
                            .FontFamily(ReportStyles.FontFamily)
                            .FontSize(8)
                            .FontColor(ReportStyles.Muted));

                        text.Span(context.VersionService.ProductName);

                        if (context.Options.IncludeProgramVersion)
                            text.Span(" · " + context.VersionService.VersionText);

                        if (context.Options.IncludeDate)
                            text.Span(" · " + context.CreatedAt.ToString("yyyy.MM.dd"));
                    });

                    if (context.Options.IncludePageNumbers)
                    {
                        row.ConstantItem(70).AlignRight().Text(text =>
                        {
                            text.DefaultTextStyle(style => style
                                .FontFamily(ReportStyles.FontFamily)
                                .FontSize(8)
                                .FontColor(ReportStyles.Muted));
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                    }
                });
            });
        }
    }
}
