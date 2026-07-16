using System;
using System.Collections.Generic;
using System.Linq;
using HVACDesigner.Calculations.Common;

namespace HVACDesigner.Calculations.Water
{
    public sealed class WaterDemandCalculator
    {
        public CalculationResult<DailyWaterDemandResult>
            CalculateDailyDemand(
                WaterDemandInput input,
                double? defaultPersonsPerDwelling,
                double? dailyWaterRate,
                string dailyWaterRateId,
                string demandUnitLabel,
                string profileKey,
                string ruleSetKey)
        {
            var diagnostics =
                WaterValidation.ValidateDailyDemand(
                    input,
                    defaultPersonsPerDwelling,
                    dailyWaterRate,
                    dailyWaterRateId);

            var builder =
                new CalculationResultBuilder<DailyWaterDemandResult>()
                .UseProfile(profileKey)
                .UseRuleSet(ruleSetKey);

            foreach (CalculationDiagnostic diagnostic in diagnostics)
                builder.AddDiagnostic(diagnostic);

            if (diagnostics.Any(item =>
                item.Severity ==
                CalculationDiagnosticSeverity.Error))
            {
                return builder.BuildWaitingForInput(null);
            }

            bool estimated = false;
            double occupancy;
            bool isPersonBased =
                string.IsNullOrWhiteSpace(dailyWaterRateId) ||
                string.Equals(
                    dailyWaterRateId,
                    "DailyWaterPerPerson",
                    StringComparison.OrdinalIgnoreCase);

            if (!isPersonBased)
            {
                occupancy = input.DemandUnitCount.Value;
                builder.AddInput(
                    dailyWaterRateId,
                    occupancy.ToString("0.###"));
            }
            else if (input.Occupancy.HasValue)
            {
                occupancy = input.Occupancy.Value;
                builder.AddInput(
                    "Occupancy",
                    occupancy.ToString("0.###"));
            }
            else
            {
                occupancy =
                    input.DwellingCount.Value *
                    defaultPersonsPerDwelling.Value;

                estimated = true;

                builder.AddInput(
                    "DwellingCount",
                    input.DwellingCount.Value.ToString());

                builder.AddAssumption(
                    new CalculationAssumption(
                        "Alapértelmezett lakószám",
                        defaultPersonsPerDwelling.Value.ToString("0.###"),
                        "fő/lakás",
                        profileKey,
                        false));

                builder.AddDiagnostic(
                    new CalculationDiagnostic(
                        "OCCUPANCY_ESTIMATED",
                        "A létszám a profil alapértelmezett lakószámából származik.",
                        CalculationDiagnosticSeverity.Warning));
            }

            double demand =
                occupancy *
                dailyWaterRate.Value;

            builder.AddInput(
                string.IsNullOrWhiteSpace(dailyWaterRateId)
                    ? "DailyWaterRate"
                    : dailyWaterRateId,
                dailyWaterRate.Value.ToString("0.###"));

            return builder.Build(
                new DailyWaterDemandResult(
                    occupancy,
                    demand,
                    estimated,
                    demandUnitLabel));
        }

        public CalculationResult<PeakWaterDemandResult>
            CalculatePeakWaterDemand(
                WaterDemandInput input,
                IReadOnlyDictionary<string, FixtureCatalogItem> catalog,
                double conversionA,
                double conversionExponent,
                double conversionOffset,
                string methodId,
                string ruleSetKey)
        {
            var diagnostics =
                WaterValidation.ValidateFixtures(
                    input,
                    catalog,
                    requirePotableData: true,
                    requireWastewaterData: false);

            var builder =
                new CalculationResultBuilder<PeakWaterDemandResult>()
                .UseRuleSet(ruleSetKey);

            foreach (CalculationDiagnostic diagnostic in diagnostics)
                builder.AddDiagnostic(diagnostic);

            if (diagnostics.Any(item =>
                item.Severity ==
                CalculationDiagnosticSeverity.Error))
            {
                return builder.BuildWaitingForInput(null);
            }

            double loadingUnits = 0.0;
            int potableFixtureCount = 0;

            foreach (FixtureUsage usage in input.FixtureUsages)
            {
                FixtureCatalogItem fixture =
                    catalog[usage.FixtureId];

                if (!fixture.PotableLoadingUnit.HasValue)
                {
                    builder.AddDiagnostic(
                        new CalculationDiagnostic(
                            "POTABLE_LOADING_UNIT_SKIPPED",
                            "A(z) „" + fixture.DisplayName +
                            "” szerelvénynek nincs ivóvíz-terhelési értéke, ezért az ivóvízhozam számításból kimaradt.",
                            CalculationDiagnosticSeverity.Warning));
                    continue;
                }

                loadingUnits +=
                    usage.Quantity *
                    fixture.PotableLoadingUnit.Value;

                potableFixtureCount++;
            }

            if (potableFixtureCount == 0 ||
                loadingUnits <= 0.0)
            {
                builder.AddDiagnostic(
                    new CalculationDiagnostic(
                        "POTABLE_FIXTURE_REQUIRED",
                        "A mértékadó ivóvízhozamhoz legalább egy ivóvíz-terhelési értékkel rendelkező szerelvény szükséges.",
                        CalculationDiagnosticSeverity.Error));

                return builder.BuildWaitingForInput(null);
            }

            double designFlow =
                conversionA *
                Math.Pow(
                    Math.Max(0.0, loadingUnits),
                    conversionExponent) +
                conversionOffset;

            builder.AddInput(
                "TotalLoadingUnits",
                loadingUnits.ToString("0.###"));

            return builder.Build(
                new PeakWaterDemandResult(
                    loadingUnits,
                    Math.Max(0.0, designFlow),
                    methodId));
        }

        public CalculationResult<WastewaterFlowResult>
            CalculateWastewaterFlow(
                WaterDemandInput input,
                IReadOnlyDictionary<string, FixtureCatalogItem> catalog,
                double frequencyFactor,
                double continuousFlowLitresPerSecond,
                double pumpedFlowLitresPerSecond,
                string methodId,
                string ruleSetKey)
        {
            var diagnostics =
                WaterValidation.ValidateFixtures(
                    input,
                    catalog,
                    requirePotableData: false,
                    requireWastewaterData: true);

            var builder =
                new CalculationResultBuilder<WastewaterFlowResult>()
                .UseRuleSet(ruleSetKey);

            foreach (CalculationDiagnostic diagnostic in diagnostics)
                builder.AddDiagnostic(diagnostic);

            if (diagnostics.Any(item =>
                item.Severity ==
                CalculationDiagnosticSeverity.Error))
            {
                return builder.BuildWaitingForInput(null);
            }

            double totalDu = 0.0;
            int minimumDn = 0;

            foreach (FixtureUsage usage in input.FixtureUsages)
            {
                FixtureCatalogItem fixture =
                    catalog[usage.FixtureId];

                if (!fixture.WastewaterDischargeUnit.HasValue)
                {
                    builder.AddDiagnostic(
                        new CalculationDiagnostic(
                            "WASTEWATER_DU_SKIPPED",
                            "A(z) „" + fixture.DisplayName +
                            "” szerelvénynek nincs szennyvíz-lefolyási értéke, ezért a szennyvízhozam számításból kimaradt.",
                            CalculationDiagnosticSeverity.Warning));
                    continue;
                }

                totalDu +=
                    usage.Quantity *
                    fixture.WastewaterDischargeUnit.Value;

                if (fixture.MinimumWasteDiameter.HasValue)
                {
                    minimumDn =
                        Math.Max(
                            minimumDn,
                            fixture.MinimumWasteDiameter.Value);
                }
            }

            double qww =
                frequencyFactor *
                Math.Sqrt(Math.Max(0.0, totalDu));

            double totalFlow =
                qww +
                Math.Max(0.0, continuousFlowLitresPerSecond) +
                Math.Max(0.0, pumpedFlowLitresPerSecond);

            builder.AddInput(
                "TotalDischargeUnits",
                totalDu.ToString("0.###"));

            builder.AddInput(
                "FrequencyFactor",
                frequencyFactor.ToString("0.###"));

            return builder.Build(
                new WastewaterFlowResult(
                    totalDu,
                    totalFlow,
                    minimumDn,
                    methodId));
        }

        public CalculationResult<DhwDemandResult>
            CalculateDhwDemand(
                WaterDemandInput input,
                double? dailyHotWaterRate,
                string dailyHotWaterRateId,
                double? defaultPersonsPerDwelling,
                double referenceDhwTemp,
                double referenceColdWaterTemp,
                double waterSpecificHeatCapacity,
                double waterDensity,
                string ruleSetKey)
        {
            var builder =
                new CalculationResultBuilder<DhwDemandResult>()
                .UseRuleSet(ruleSetKey);

            if (input == null)
            {
                builder.AddDiagnostic(
                    new CalculationDiagnostic(
                        "DHW_INPUT_REQUIRED",
                        "A melegvíz-igény számításhoz nincs megadva bemenet.",
                        CalculationDiagnosticSeverity.Error));
                return builder.BuildWaitingForInput(null);
            }

            bool isPersonBased =
                string.IsNullOrWhiteSpace(dailyHotWaterRateId) ||
                string.Equals(
                    dailyHotWaterRateId,
                    "DailyHotWaterPerPerson",
                    StringComparison.OrdinalIgnoreCase);

            if (isPersonBased &&
                !input.Occupancy.HasValue &&
                !input.DwellingCount.HasValue)
            {
                builder.AddDiagnostic(
                    new CalculationDiagnostic(
                        "DHW_OCCUPANCY_OR_DWELLINGS_REQUIRED",
                        "A melegvíz-számításhoz adja meg a létszámot vagy a lakások számát.",
                        CalculationDiagnosticSeverity.Error));
                return builder.BuildWaitingForInput(null);
            }

            if (isPersonBased &&
                !input.Occupancy.HasValue &&
                input.DwellingCount.HasValue &&
                !defaultPersonsPerDwelling.HasValue)
            {
                builder.AddDiagnostic(
                    new CalculationDiagnostic(
                        "DHW_DEFAULT_OCCUPANCY_REQUIRED",
                        "A melegvíz-számításhoz a profil nem tartalmaz alapértelmezett lakószámot.",
                        CalculationDiagnosticSeverity.Error));
                return builder.BuildWaitingForInput(null);
            }

            if (!isPersonBased &&
                !input.DemandUnitCount.HasValue)
            {
                builder.AddDiagnostic(
                    new CalculationDiagnostic(
                        "DHW_BASIS_REQUIRED",
                        "A melegvíz-számításhoz adja meg a profil fajlagos igényéhez tartozó mennyiséget.",
                        CalculationDiagnosticSeverity.Error));
                return builder.BuildWaitingForInput(null);
            }

            if (!dailyHotWaterRate.HasValue)
            {
                builder.AddDiagnostic(
                    new CalculationDiagnostic(
                        "DHW_RATE_REQUIRED",
                        "A profil nem tartalmaz napi fajlagos melegvíz-igényt.",
                        CalculationDiagnosticSeverity.Error));
                return builder.BuildWaitingForInput(null);
            }

            double basisCount = 0.0;

            if (isPersonBased)
            {
                if (input.Occupancy.HasValue)
                {
                    basisCount = input.Occupancy.Value;
                }
                else if (input.DwellingCount.HasValue &&
                         defaultPersonsPerDwelling.HasValue)
                {
                    basisCount =
                        input.DwellingCount.Value *
                        defaultPersonsPerDwelling.Value;
                }
            }
            else
            {
                basisCount = input.DemandUnitCount.Value;
            }

            double dailyVolumeLitres =
                basisCount *
                dailyHotWaterRate.Value;

            double dT = Math.Max(0.0, referenceDhwTemp - referenceColdWaterTemp);
            double dailyEnergyKwh =
                (dailyVolumeLitres *
                 waterDensity *
                 waterSpecificHeatCapacity *
                 dT) / 3600.0;

            builder.AddInput(
                "Occupancy",
                basisCount.ToString("0.###"));

            builder.AddInput(
                "DailyHotWaterRate",
                dailyHotWaterRate.Value.ToString("0.###"));

            builder.AddInput(
                "DhwReferenceTemperature",
                referenceDhwTemp.ToString("0.#"));

            builder.AddInput(
                "ColdWaterReferenceTemperature",
                referenceColdWaterTemp.ToString("0.#"));

            string unitLabel = "fő";
            if (string.Equals(dailyHotWaterRateId, "DailyHotWaterPerBed", StringComparison.OrdinalIgnoreCase))
                unitLabel = "ágy";
            else if (string.Equals(dailyHotWaterRateId, "DailyHotWaterPerGuest", StringComparison.OrdinalIgnoreCase))
                unitLabel = "vendég";
            else if (string.Equals(dailyHotWaterRateId, "DailyHotWaterPerMeal", StringComparison.OrdinalIgnoreCase))
                unitLabel = "adag";
            else if (string.Equals(dailyHotWaterRateId, "DailyHotWaterPerUser", StringComparison.OrdinalIgnoreCase))
                unitLabel = "használó";

            return builder.Build(
                new DhwDemandResult(
                    basisCount,
                    dailyVolumeLitres,
                    dailyEnergyKwh,
                    unitLabel));
        }

        public CalculationResult<GreywaterBalanceResult>
            CalculateGreywaterBalance(
                bool enabled,
                double eligibleSupplyLitresPerDay,
                double eligibleDemandLitresPerDay,
                string ruleSetKey)
        {
            var builder =
                new CalculationResultBuilder<GreywaterBalanceResult>()
                .UseRuleSet(ruleSetKey);

            if (!enabled)
            {
                return new CalculationResult<GreywaterBalanceResult>(
                    null,
                    CalculationStatus.NotApplicable,
                    null,
                    new[] { ruleSetKey },
                    null,
                    null,
                    null);
            }

            builder.AddInput(
                "EligibleSupplyLitresPerDay",
                eligibleSupplyLitresPerDay.ToString("0.###"));

            builder.AddInput(
                "EligibleDemandLitresPerDay",
                eligibleDemandLitresPerDay.ToString("0.###"));

            return builder.Build(
                new GreywaterBalanceResult(
                    eligibleSupplyLitresPerDay,
                    eligibleDemandLitresPerDay));
        }
    }
}
