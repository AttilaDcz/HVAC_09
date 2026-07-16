using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HVACDesigner.Features.Water;

namespace HVACDesigner.Services.Export.Reports.Water
{
    public sealed class WaterReportFixtureRow
    {
        public WaterReportFixtureRow(
            string name,
            int quantity,
            double? potableLoadingUnit,
            double totalPotableLoadingUnit,
            double? wastewaterUnit,
            double totalWastewaterUnit,
            int? minimumDn)
        {
            Name = name ?? string.Empty;
            Quantity = quantity;
            PotableLoadingUnit = potableLoadingUnit;
            TotalPotableLoadingUnit = totalPotableLoadingUnit;
            WastewaterUnit = wastewaterUnit;
            TotalWastewaterUnit = totalWastewaterUnit;
            MinimumDn = minimumDn;
        }

        public string Name { get; }
        public int Quantity { get; }
        public double? PotableLoadingUnit { get; }
        public double TotalPotableLoadingUnit { get; }
        public double? WastewaterUnit { get; }
        public double TotalWastewaterUnit { get; }
        public int? MinimumDn { get; }
    }

    public sealed class WaterReportData
    {
        public WaterReportData(
            ReportContext context,
            WaterCalculationResult result,
            IEnumerable<WaterReportFixtureRow> fixtures,
            string buildingFunction,
            string buildingProfile,
            string primaryInputLabel,
            string primaryInputValue,
            string secondaryInputLabel,
            string secondaryInputValue)
        {
            Context = context;
            Result = result;
            Fixtures = new ReadOnlyCollection<WaterReportFixtureRow>(
                (fixtures ?? Enumerable.Empty<WaterReportFixtureRow>()).ToList());
            BuildingFunction = buildingFunction ?? string.Empty;
            BuildingProfile = buildingProfile ?? string.Empty;
            PrimaryInputLabel = primaryInputLabel ?? string.Empty;
            PrimaryInputValue = primaryInputValue ?? string.Empty;
            SecondaryInputLabel = secondaryInputLabel ?? string.Empty;
            SecondaryInputValue = secondaryInputValue ?? string.Empty;
        }

        public ReportContext Context { get; }
        public WaterCalculationResult Result { get; }
        public IReadOnlyList<WaterReportFixtureRow> Fixtures { get; }
        public string BuildingFunction { get; }
        public string BuildingProfile { get; }
        public string PrimaryInputLabel { get; }
        public string PrimaryInputValue { get; }
        public string SecondaryInputLabel { get; }
        public string SecondaryInputValue { get; }
    }
}
