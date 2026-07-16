using System;
using System.Collections.Generic;
using HVACDesigner.Calculations.Common;
using HVACDesigner.Calculations.Water;
using HVACDesigner.EngineeringData.Rules.Common;

namespace HVACDesigner.Tests.Integration.Water
{
    public static class WaterThirtyApartmentScenarioTests
    {
        public static void Run()
        {
            var fixtures =
                new Dictionary<string, FixtureCatalogItem>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ["WashBasin"] = new FixtureCatalogItem(
                        "WashBasin", "Mosdó",
                        0.5, 0.5, 40, true, "true", false),

                    ["KitchenSink"] = new FixtureCatalogItem(
                        "KitchenSink", "Konyhai mosogató",
                        1.0, 0.8, 50, true, "conditional", false),

                    ["Shower"] = new FixtureCatalogItem(
                        "Shower", "Zuhany",
                        0.5, 0.6, 50, true, "true", false),

                    ["WCWithCistern"] = new FixtureCatalogItem(
                        "WCWithCistern", "Öblítőtartályos WC",
                        0.5, 2.0, 100, false, string.Empty, true),

                    ["WashingMachine"] = new FixtureCatalogItem(
                        "WashingMachine", "Mosógép",
                        1.0, 0.8, 50, false, "true", false),

                    ["Dishwasher"] = new FixtureCatalogItem(
                        "Dishwasher", "Mosogatógép",
                        1.0, 0.8, 50, false, "conditional", false)
                };

            var usages = new[]
            {
                new FixtureUsage("WashBasin", 30),
                new FixtureUsage("KitchenSink", 30),
                new FixtureUsage("Shower", 30),
                new FixtureUsage("WCWithCistern", 30),
                new FixtureUsage("WashingMachine", 30),
                new FixtureUsage("Dishwasher", 30)
            };

            var input = new WaterDemandInput(
                "ApartmentBuilding",
                "Residential.MultiApartment",
                30,
                null,
                null,
                usages,
                false);

            var calculator = new WaterDemandCalculator();

            CalculationResult<DailyWaterDemandResult> daily =
                calculator.CalculateDailyDemand(
                    input,
                    2.5,
                    105.0,
                    "DailyWaterPerPerson",
                    "fő",
                    "Residential.MultiApartment",
                    "HU.Water.DailyDemand@1.1");

            CalculationResult<PeakWaterDemandResult> peak =
                calculator.CalculatePeakWaterDemand(
                    input,
                    fixtures,
                    0.20,
                    0.50,
                    0.00,
                    "En806Compatible",
                    "HU.Water.PeakDemand@1.1");

            CalculationResult<WastewaterFlowResult> wastewater =
                calculator.CalculateWastewaterFlow(
                    input,
                    fixtures,
                    0.50,
                    0.00,
                    0.00,
                    "En12056DischargeUnits",
                    "HU.Water.Wastewater@1.1");

            AssertNear(
                daily.Result.DailyDemandCubicMetres,
                7.875,
                "Napi vízigény");

            AssertNear(
                peak.Result.TotalLoadingUnits,
                135.0,
                "Összes LU");

            AssertNear(
                peak.Result.DesignFlowLitresPerSecond,
                0.20 * Math.Sqrt(135.0),
                "Ivóvízhozam");

            AssertNear(
                wastewater.Result.TotalDischargeUnits,
                165.0,
                "Összes DU");

            AssertNear(
                wastewater.Result.DesignFlowLitresPerSecond,
                0.50 * Math.Sqrt(165.0),
                "Szennyvízhozam");

            if (wastewater.Result.MinimumRequiredDiameter != 100)
            {
                throw new InvalidOperationException(
                    "A szerelvénybekötési minimum nem DN 100.");
            }

            if (daily.Status !=
                CalculationStatus.SuccessWithWarnings)
            {
                throw new InvalidOperationException(
                    "A becsült lakószámnak figyelmeztetéses eredményt kell adnia.");
            }

            CalculationResult<DhwDemandResult> dhw =
                calculator.CalculateDhwDemand(
                    input,
                    35.0,
                    "DailyHotWaterPerPerson",
                    2.5,
                    60.0,
                    10.0,
                    4.187,
                    1.0,
                    "HU.Water.DhwDemand@1.1");

            AssertNear(
                dhw.Result.Occupancy,
                75.0,
                "HMV létszám");

            AssertNear(
                dhw.Result.DailyDhwVolumeLitres,
                2625.0,
                "HMV napi térfogat liter");

            AssertNear(
                dhw.Result.DailyDhwVolumeCubicMetres,
                2.625,
                "HMV napi térfogat m3");

            AssertNear(
                 dhw.Result.DailyDhwEnergyKwh,
                 152.65104167,
                 "HMV napi hőenergia kWh");

            // --- EngineeringAdvisor tesztek ---
            var parametersDict = new Dictionary<string, string>
            {
                ["Limit.DailyWaterPerPerson.Max"] = "150",
                ["Limit.DailyWaterPerPerson.Min"] = "60",
                ["Description.DailyWaterPerPerson"] = "Napi fajlagos ivóvízigény",
                ["Recommendation.DailyWaterPerPerson.Max"] = "Túl nagy vízfogyasztás!",
                ["Recommendation.DailyWaterPerPerson.Min"] = "Túl kicsi vízfogyasztás!"
            };
            var paramSet = new RuleParameterSet(parametersDict);

            // Teszt eset 1: Határérték felett (170 L)
            var evalHigh = EngineeringAdvisor.Evaluate("DailyWaterPerPerson", 170.0, paramSet, "L/fő/nap");
            if (evalHigh.AiLevel != AdvisorAiLevel.Recommendation)
                throw new InvalidOperationException("High value test failed: AiLevel should be Recommendation.");
            if (evalHigh.Diagnostics.Count != 1 || evalHigh.Diagnostics[0].Code != "DAILYWATERPERPERSON_TOO_HIGH")
                throw new InvalidOperationException("High value test failed: expected DAILYWATERPERPERSON_TOO_HIGH diagnostic.");
            if (evalHigh.RecommendationText != "Túl nagy vízfogyasztás!")
                throw new InvalidOperationException("High value test failed: incorrect recommendation text.");

            // Teszt eset 2: Határértékek között (100 L)
            var evalOk = EngineeringAdvisor.Evaluate("DailyWaterPerPerson", 100.0, paramSet, "L/fő/nap");
            if (evalOk.AiLevel != AdvisorAiLevel.RuleCheck)
                throw new InvalidOperationException("Ok value test failed: AiLevel should be RuleCheck.");
            if (evalOk.Diagnostics.Count != 0)
                throw new InvalidOperationException("Ok value test failed: diagnostics list should be empty.");
            if (evalOk.RecommendationText != "Megfelel a határértékeknek.")
                throw new InvalidOperationException("Ok value test failed: incorrect recommendation text.");

            // Teszt eset 3: Határérték alatt (50 L)
            var evalLow = EngineeringAdvisor.Evaluate("DailyWaterPerPerson", 50.0, paramSet, "L/fő/nap");
            if (evalLow.AiLevel != AdvisorAiLevel.Recommendation)
                throw new InvalidOperationException("Low value test failed: AiLevel should be Recommendation.");
            if (evalLow.Diagnostics.Count != 1 || evalLow.Diagnostics[0].Code != "DAILYWATERPERPERSON_TOO_LOW")
                throw new InvalidOperationException("Low value test failed: expected DAILYWATERPERPERSON_TOO_LOW diagnostic.");
            if (evalLow.RecommendationText != "Túl kicsi vízfogyasztás!")
                throw new InvalidOperationException("Low value test failed: incorrect recommendation text.");
        }

        private static void AssertNear(
            double actual,
            double expected,
            string name)
        {
            if (Math.Abs(actual - expected) > 0.0001)
            {
                throw new InvalidOperationException(
                    name +
                    " eltérés. Várt: " +
                    expected +
                    ", tényleges: " +
                    actual +
                    ".");
            }
        }
    }
}

