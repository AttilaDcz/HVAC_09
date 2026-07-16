using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.Calculations.Water
{
    public sealed class FixtureCatalogItem
    {
        public string Id { get; }
        public string DisplayName { get; }
        public double? PotableLoadingUnit { get; }
        public double? WastewaterDischargeUnit { get; }
        public int? MinimumWasteDiameter { get; }
        public bool HotWaterRelevant { get; }
        public string GreywaterSource { get; }
        public bool GreywaterDemand { get; }

        public FixtureCatalogItem(
            string id,
            string displayName,
            double? potableLoadingUnit,
            double? wastewaterDischargeUnit,
            int? minimumWasteDiameter,
            bool hotWaterRelevant,
            string greywaterSource,
            bool greywaterDemand)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(
                    "A szerelvény azonosítója nem lehet üres.",
                    nameof(id));

            Id = id.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? Id
                : displayName.Trim();

            PotableLoadingUnit = potableLoadingUnit;
            WastewaterDischargeUnit = wastewaterDischargeUnit;
            MinimumWasteDiameter = minimumWasteDiameter;
            HotWaterRelevant = hotWaterRelevant;
            GreywaterSource = greywaterSource?.Trim() ?? string.Empty;
            GreywaterDemand = greywaterDemand;
        }
    }

    public sealed class FixtureUsage
    {
        public string FixtureId { get; }
        public int Quantity { get; }

        public FixtureUsage(
            string fixtureId,
            int quantity)
        {
            if (string.IsNullOrWhiteSpace(fixtureId))
                throw new ArgumentException(
                    "A szerelvény azonosítója nem lehet üres.",
                    nameof(fixtureId));

            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(quantity));

            FixtureId = fixtureId.Trim();
            Quantity = quantity;
        }
    }

    public sealed class WaterDemandInput
    {
        private readonly ReadOnlyCollection<FixtureUsage> _fixtureUsages;

        public string BuildingFunctionId { get; }
        public string BuildingProfileId { get; }
        public int? DwellingCount { get; }
        public double? Occupancy { get; }
        public double? DemandUnitCount { get; }
        public bool GreywaterEnabled { get; }

        public IReadOnlyList<FixtureUsage> FixtureUsages =>
            _fixtureUsages;

        public WaterDemandInput(
            string buildingFunctionId,
            string buildingProfileId,
            int? dwellingCount,
            double? occupancy,
            double? demandUnitCount,
            IEnumerable<FixtureUsage> fixtureUsages,
            bool greywaterEnabled)
        {
            BuildingFunctionId =
                buildingFunctionId?.Trim() ?? string.Empty;

            BuildingProfileId =
                buildingProfileId?.Trim() ?? string.Empty;

            if (dwellingCount.HasValue &&
                dwellingCount.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dwellingCount));
            }

            if (occupancy.HasValue &&
                occupancy.Value <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(occupancy));
            }

            if (demandUnitCount.HasValue &&
                demandUnitCount.Value <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(demandUnitCount));
            }

            DwellingCount = dwellingCount;
            Occupancy = occupancy;
            DemandUnitCount = demandUnitCount;
            GreywaterEnabled = greywaterEnabled;

            _fixtureUsages =
                new ReadOnlyCollection<FixtureUsage>(
                    (fixtureUsages ??
                     Enumerable.Empty<FixtureUsage>())
                    .ToList());
        }
    }

    public sealed class DailyWaterDemandResult
    {
        public double Occupancy { get; }
        public double DailyDemandLitres { get; }
        public double DailyDemandCubicMetres =>
            DailyDemandLitres / 1000.0;
        public bool OccupancyWasEstimated { get; }
        public string DemandUnitLabel { get; }

        public DailyWaterDemandResult(
            double occupancy,
            double dailyDemandLitres,
            bool occupancyWasEstimated,
            string demandUnitLabel = "fő")
        {
            Occupancy = occupancy;
            DailyDemandLitres = dailyDemandLitres;
            OccupancyWasEstimated = occupancyWasEstimated;
            DemandUnitLabel = demandUnitLabel ?? "fő";
        }
    }

    public sealed class DhwDemandResult
    {
        public double Occupancy { get; }
        public double DailyDhwVolumeLitres { get; }
        public double DailyDhwVolumeCubicMetres => DailyDhwVolumeLitres / 1000.0;
        public double DailyDhwEnergyKwh { get; }
        public string DemandUnitLabel { get; }

        public DhwDemandResult(
            double occupancy,
            double dailyDhwVolumeLitres,
            double dailyDhwEnergyKwh,
            string demandUnitLabel = "fő")
        {
            Occupancy = occupancy;
            DailyDhwVolumeLitres = dailyDhwVolumeLitres;
            DailyDhwEnergyKwh = dailyDhwEnergyKwh;
            DemandUnitLabel = demandUnitLabel ?? "fő";
        }
    }


    public sealed class PeakWaterDemandResult
    {
        public double TotalLoadingUnits { get; }
        public double DesignFlowLitresPerSecond { get; }
        public string MethodId { get; }

        public PeakWaterDemandResult(
            double totalLoadingUnits,
            double designFlowLitresPerSecond,
            string methodId)
        {
            TotalLoadingUnits = totalLoadingUnits;
            DesignFlowLitresPerSecond =
                designFlowLitresPerSecond;
            MethodId = methodId?.Trim() ?? string.Empty;
        }
    }

    public sealed class WastewaterFlowResult
    {
        public double TotalDischargeUnits { get; }
        public double DesignFlowLitresPerSecond { get; }
        public int MinimumRequiredDiameter { get; }
        public string MethodId { get; }

        public WastewaterFlowResult(
            double totalDischargeUnits,
            double designFlowLitresPerSecond,
            int minimumRequiredDiameter,
            string methodId)
        {
            TotalDischargeUnits = totalDischargeUnits;
            DesignFlowLitresPerSecond =
                designFlowLitresPerSecond;
            MinimumRequiredDiameter =
                minimumRequiredDiameter;
            MethodId = methodId?.Trim() ?? string.Empty;
        }
    }

    public sealed class GreywaterBalanceResult
    {
        public double EligibleSupplyLitresPerDay { get; }
        public double EligibleDemandLitresPerDay { get; }
        public double ReusableGreywaterLitresPerDay { get; }
        public double PotableBackupLitresPerDay { get; }
        public double OverflowLitresPerDay { get; }

        public GreywaterBalanceResult(
            double eligibleSupplyLitresPerDay,
            double eligibleDemandLitresPerDay)
        {
            EligibleSupplyLitresPerDay =
                Math.Max(0.0, eligibleSupplyLitresPerDay);

            EligibleDemandLitresPerDay =
                Math.Max(0.0, eligibleDemandLitresPerDay);

            ReusableGreywaterLitresPerDay =
                Math.Min(
                    EligibleSupplyLitresPerDay,
                    EligibleDemandLitresPerDay);

            PotableBackupLitresPerDay =
                Math.Max(
                    0.0,
                    EligibleDemandLitresPerDay -
                    EligibleSupplyLitresPerDay);

            OverflowLitresPerDay =
                Math.Max(
                    0.0,
                    EligibleSupplyLitresPerDay -
                    EligibleDemandLitresPerDay);
        }
    }

    public sealed class RoofCatchment
    {
        public string Id { get; }
        public double EffectiveAreaSquareMetres { get; }
        public double RunoffCoefficient { get; }

        public RoofCatchment(
            string id,
            double effectiveAreaSquareMetres,
            double runoffCoefficient)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(
                    "A tetőfelület azonosítója nem lehet üres.",
                    nameof(id));

            if (effectiveAreaSquareMetres <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(effectiveAreaSquareMetres));

            if (runoffCoefficient < 0.0 ||
                runoffCoefficient > 1.5)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(runoffCoefficient));
            }

            Id = id.Trim();
            EffectiveAreaSquareMetres =
                effectiveAreaSquareMetres;
            RunoffCoefficient = runoffCoefficient;
        }
    }

    public sealed class RoofDrainageInput
    {
        private readonly ReadOnlyCollection<RoofCatchment> _catchments;

        public IReadOnlyList<RoofCatchment> Catchments =>
            _catchments;

        public double RainfallIntensityLitresPerSecondSquareMetre { get; }

        public RoofDrainageInput(
            IEnumerable<RoofCatchment> catchments,
            double rainfallIntensityLitresPerSecondSquareMetre)
        {
            if (rainfallIntensityLitresPerSecondSquareMetre <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(rainfallIntensityLitresPerSecondSquareMetre));

            _catchments =
                new ReadOnlyCollection<RoofCatchment>(
                    (catchments ??
                     Enumerable.Empty<RoofCatchment>())
                    .ToList());

            RainfallIntensityLitresPerSecondSquareMetre =
                rainfallIntensityLitresPerSecondSquareMetre;
        }
    }

    public sealed class RoofDrainageResult
    {
        public double TotalEffectiveAreaSquareMetres { get; }
        public double DesignFlowLitresPerSecond { get; }

        public RoofDrainageResult(
            double totalEffectiveAreaSquareMetres,
            double designFlowLitresPerSecond)
        {
            TotalEffectiveAreaSquareMetres =
                totalEffectiveAreaSquareMetres;
            DesignFlowLitresPerSecond =
                designFlowLitresPerSecond;
        }
    }
}
