using HVACDesigner.Services.Export.Interfaces;
using HVACDesigner.Services.Export.Reports.Water;

namespace HVACDesigner.Services.Export.Reports
{
    public sealed class ReportFactory
    {
        public IReportBuilder<WaterReportData> CreateWaterReportBuilder()
        {
            return new WaterReportBuilder();
        }
    }
}
