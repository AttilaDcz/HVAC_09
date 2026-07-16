using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HVACDesigner.Services.Export.Reports;

namespace HVACDesigner.Services.Export.Common
{
    public static class ReportHeader
    {
        public static void Compose(IContainer container, ReportContext context, string title)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(text =>
                    {
                        text.Item().Text(title)
                            .FontFamily(ReportStyles.FontFamily)
                            .FontSize(17)
                            .Bold()
                            .FontColor(ReportStyles.Text);

                        string projectName = ReportText.Clean(context.Project.Name);
                        if (!string.IsNullOrWhiteSpace(projectName))
                        {
                            text.Item().PaddingTop(4).Text(projectName)
                                .FontFamily(ReportStyles.FontFamily)
                                .FontSize(10)
                                .FontColor(ReportStyles.Muted);
                        }
                    });

                    row.ConstantItem(120).AlignRight().Text(context.VersionService.ProductName)
                        .FontFamily(ReportStyles.FontFamily)
                        .FontSize(10)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);
                });

                column.Item().PaddingTop(10).LineHorizontal(1).LineColor(ReportStyles.Border);
            });
        }
    }
}
