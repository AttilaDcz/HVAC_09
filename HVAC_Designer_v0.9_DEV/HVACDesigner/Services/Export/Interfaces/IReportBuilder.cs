using QuestPDF.Infrastructure;

namespace HVACDesigner.Services.Export.Interfaces
{
    public interface IReportBuilder<in TReportData>
    {
        IDocument Build(TReportData data);
    }
}
