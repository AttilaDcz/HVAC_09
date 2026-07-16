using System;
using System.Collections.Generic;
using System.Linq;
using HVACDesigner.Calculations.Common;

namespace HVACDesigner.Calculations.Water
{
    public static class WaterValidation
    {
        public static IReadOnlyList<CalculationDiagnostic>
            ValidateDailyDemand(
                WaterDemandInput input,
                double? defaultPersonsPerDwelling,
                double? dailyWaterRate,
                string dailyWaterRateId)
        {
            var diagnostics =
                new List<CalculationDiagnostic>();

            if (input == null)
            {
                diagnostics.Add(
                    Error(
                        "WATER_INPUT_REQUIRED",
                        "A vízigény-számításhoz nincs megadva bemenet."));
                return diagnostics;
            }

            bool isPersonBased =
                string.IsNullOrWhiteSpace(dailyWaterRateId) ||
                string.Equals(
                    dailyWaterRateId,
                    "DailyWaterPerPerson",
                    StringComparison.OrdinalIgnoreCase);

            if (isPersonBased &&
                !input.Occupancy.HasValue &&
                !input.DwellingCount.HasValue)
            {
                diagnostics.Add(
                    Error(
                        "OCCUPANCY_OR_DWELLINGS_REQUIRED",
                        "Adja meg a létszámot vagy a lakások számát."));
            }

            if (isPersonBased &&
                !input.Occupancy.HasValue &&
                input.DwellingCount.HasValue &&
                !defaultPersonsPerDwelling.HasValue)
            {
                diagnostics.Add(
                    Error(
                        "DEFAULT_OCCUPANCY_REQUIRED",
                        "A profil nem tartalmaz alapértelmezett lakószámot."));
            }

            if (!isPersonBased &&
                !input.DemandUnitCount.HasValue)
            {
                diagnostics.Add(
                    Error(
                        "DAILY_WATER_BASIS_REQUIRED",
                        "Adja meg a profil fajlagos vízigényéhez tartozó mennyiséget."));
            }

            if (!dailyWaterRate.HasValue)
            {
                diagnostics.Add(
                    Error(
                        "DAILY_WATER_RATE_REQUIRED",
                        "A profil nem tartalmaz napi fajlagos ivóvízigényt."));
            }

            return diagnostics;
        }

        public static IReadOnlyList<CalculationDiagnostic>
            ValidateFixtures(
                WaterDemandInput input,
                IReadOnlyDictionary<string, FixtureCatalogItem> catalog,
                bool requirePotableData,
                bool requireWastewaterData)
        {
            var diagnostics =
                new List<CalculationDiagnostic>();

            if (input == null)
            {
                diagnostics.Add(
                    Error(
                        "WATER_INPUT_REQUIRED",
                        "A számításhoz nincs megadva bemenet."));
                return diagnostics;
            }

            if (input.FixtureUsages.Count == 0)
            {
                diagnostics.Add(
                    Error(
                        "FIXTURE_LIST_REQUIRED",
                        "A számításhoz szerelvénylista szükséges."));
                return diagnostics;
            }

            foreach (FixtureUsage usage in input.FixtureUsages)
            {
                if (!catalog.TryGetValue(
                    usage.FixtureId,
                    out FixtureCatalogItem fixture))
                {
                    diagnostics.Add(
                        Error(
                            "FIXTURE_NOT_FOUND",
                            "A(z) „" + usage.FixtureId +
                            "” szerelvény nem található a katalógusban."));
                    continue;
                }

                if (requirePotableData &&
                    !fixture.PotableLoadingUnit.HasValue)
                {
                    diagnostics.Add(
                        new CalculationDiagnostic(
                            "POTABLE_LOADING_UNIT_MISSING",
                            "A(z) „" + fixture.DisplayName +
                            "” szerelvényhez nincs ivóvíz-terhelési érték.",
                            CalculationDiagnosticSeverity.Warning));
                }

                if (requireWastewaterData &&
                    !fixture.WastewaterDischargeUnit.HasValue)
                {
                    diagnostics.Add(
                        new CalculationDiagnostic(
                            "WASTEWATER_DU_MISSING",
                            "A(z) „" + fixture.DisplayName +
                            "” szerelvényhez nincs szennyvíz-lefolyási egység.",
                            CalculationDiagnosticSeverity.Warning));
                }
            }

            return diagnostics;
        }

        public static IReadOnlyList<CalculationDiagnostic>
            ValidateRoofDrainage(
                RoofDrainageInput input)
        {
            var diagnostics =
                new List<CalculationDiagnostic>();

            if (input == null)
            {
                diagnostics.Add(
                    Error(
                        "ROOF_INPUT_REQUIRED",
                        "A tetővíz-számításhoz nincs megadva bemenet."));
                return diagnostics;
            }

            if (input.Catchments.Count == 0)
            {
                diagnostics.Add(
                    Error(
                        "ROOF_CATCHMENT_REQUIRED",
                        "Legalább egy tetőfelületet meg kell adni."));
            }

            return diagnostics;
        }

        private static CalculationDiagnostic Error(
            string code,
            string message)
        {
            return new CalculationDiagnostic(
                code,
                message,
                CalculationDiagnosticSeverity.Error);
        }
    }
}
