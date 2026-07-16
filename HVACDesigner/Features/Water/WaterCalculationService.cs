using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HVACDesigner.Calculations.Common;
using HVACDesigner.Calculations.Water;
using HVACDesigner.EngineeringData.Registry;
using HVACDesigner.EngineeringData.Rules.Common;
using HVACDesigner.EngineeringData.SimpleCatalogs;
using HVACDesigner.Services;

namespace HVACDesigner.Features.Water
{
    public sealed class WaterModuleInput
    {
        public int? DwellingCount { get; }
        public double? Occupancy { get; }
        public double? DemandUnitCount { get; }
        public bool GreywaterEnabled { get; }
        public bool RoofDrainageEnabled { get; }
        public double? RoofAreaSquareMetres { get; }
        public string RoofType { get; }
        public double? RainfallIntensity { get; }
        public IReadOnlyList<FixtureUsage> FixtureUsages { get; }

        public WaterModuleInput(
            int? dwellingCount,
            double? occupancy,
            double? demandUnitCount,
            IReadOnlyList<FixtureUsage> fixtureUsages,
            bool greywaterEnabled = false,
            bool roofDrainageEnabled = false,
            double? roofAreaSquareMetres = null,
            string roofType = null,
            double? rainfallIntensity = null)
        {
            DwellingCount = dwellingCount;
            Occupancy = occupancy;
            DemandUnitCount = demandUnitCount;
            GreywaterEnabled = greywaterEnabled;
            RoofDrainageEnabled = roofDrainageEnabled;
            RoofAreaSquareMetres = roofAreaSquareMetres;
            RoofType = roofType ?? string.Empty;
            RainfallIntensity = rainfallIntensity;
            FixtureUsages = fixtureUsages ??
                Array.Empty<FixtureUsage>();
        }
    }

    public sealed class WaterCalculationResult
    {
        public CalculationResult<DailyWaterDemandResult> DailyDemand { get; }
        public CalculationResult<DhwDemandResult> DhwDemand { get; }
        public CalculationResult<PeakWaterDemandResult> PeakDemand { get; }
        public CalculationResult<WastewaterFlowResult> Wastewater { get; }
        public CalculationResult<GreywaterBalanceResult> Greywater { get; }
        public CalculationResult<RoofDrainageResult> RoofDrainage { get; }
        public WaterResolvedRules Rules { get; }

        public WaterCalculationResult(
            CalculationResult<DailyWaterDemandResult> dailyDemand,
            CalculationResult<DhwDemandResult> dhwDemand,
            CalculationResult<PeakWaterDemandResult> peakDemand,
            CalculationResult<WastewaterFlowResult> wastewater,
            CalculationResult<GreywaterBalanceResult> greywater,
            CalculationResult<RoofDrainageResult> roofDrainage,
            WaterResolvedRules rules)
        {
            DailyDemand = dailyDemand;
            DhwDemand = dhwDemand;
            PeakDemand = peakDemand;
            Wastewater = wastewater;
            Greywater = greywater;
            RoofDrainage = roofDrainage;
            Rules = rules;
        }
    }

    public sealed class WaterRoofDrainageOption
    {
        public string Id { get; }
        public string DisplayName { get; }
        public double RunoffCoefficient { get; }

        public WaterRoofDrainageOption(
            string id,
            string displayName,
            double runoffCoefficient)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? Id;
            RunoffCoefficient = runoffCoefficient;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public sealed class WaterRoofDrainageInputInfo
    {
        public double DefaultRainfallIntensity { get; }
        public IReadOnlyList<WaterRoofDrainageOption> RoofTypes { get; }

        public WaterRoofDrainageInputInfo(
            double defaultRainfallIntensity,
            IEnumerable<WaterRoofDrainageOption> roofTypes)
        {
            DefaultRainfallIntensity = defaultRainfallIntensity;
            RoofTypes = new ReadOnlyCollection<WaterRoofDrainageOption>(
                (roofTypes ?? Enumerable.Empty<WaterRoofDrainageOption>())
                .ToList());
        }
    }

    public sealed class WaterOccupancySuggestion
    {
        public bool IsAvailable { get; }
        public double SuggestedValue { get; }
        public string Explanation { get; }

        private WaterOccupancySuggestion(
            bool isAvailable,
            double suggestedValue,
            string explanation)
        {
            IsAvailable = isAvailable;
            SuggestedValue = suggestedValue;
            Explanation = explanation ?? string.Empty;
        }

        public static WaterOccupancySuggestion Missing(
            string explanation) =>
            new WaterOccupancySuggestion(
                false,
                0.0,
                explanation);

        public static WaterOccupancySuggestion Create(
            int dwellings,
            double personsPerDwelling) =>
            new WaterOccupancySuggestion(
                true,
                dwellings * personsPerDwelling,
                $"{dwellings} lakás × {personsPerDwelling:0.###} fő/lakás");
    }

    public sealed class WaterDailyDemandInputInfo
    {
        public bool IsPersonBased { get; }
        public bool SupportsDwellingSuggestion { get; }
        public string PrimaryInputLabel { get; }
        public string SecondaryInputLabel { get; }
        public string UnitLabel { get; }
        public double? DailyWaterRate { get; }

        public WaterDailyDemandInputInfo(
            bool isPersonBased,
            bool supportsDwellingSuggestion,
            string primaryInputLabel,
            string secondaryInputLabel,
            string unitLabel,
            double? dailyWaterRate)
        {
            IsPersonBased = isPersonBased;
            SupportsDwellingSuggestion = supportsDwellingSuggestion;
            PrimaryInputLabel = primaryInputLabel ?? "Mennyiség";
            SecondaryInputLabel = secondaryInputLabel ?? "Létszám";
            UnitLabel = unitLabel ?? "egység";
            DailyWaterRate = dailyWaterRate;
        }
    }

    /// <summary>
    /// A Water modul első vertikális szeletének alkalmazásszolgáltatása.
    /// </summary>
    public sealed class WaterCalculationService
    {
        private const string DataVersion = "1.0";

        /// <summary>Szürkevíz forrás XML értékkonstansai.</summary>
        private static class GreywaterSourceValues
        {
            public const string True        = "true";
            public const string Conditional = "conditional";
        }

        private readonly EngineeringDataRegistry dataRegistry;
        private readonly EngineeringRuleRegistry ruleRegistry;
        private readonly FixtureCatalogAdapter fixtureAdapter;
        private readonly WaterRuleResolver ruleResolver;
        private readonly WaterDemandCalculator calculator;
        private readonly RoofDrainageCalculator roofCalculator;

        public WaterCalculationService(
            EngineeringDataRegistry dataRegistry,
            EngineeringRuleRegistry ruleRegistry)
        {
            this.dataRegistry = dataRegistry ??
                throw new ArgumentNullException(nameof(dataRegistry));

            this.ruleRegistry = ruleRegistry ??
                throw new ArgumentNullException(nameof(ruleRegistry));

            fixtureAdapter = new FixtureCatalogAdapter();
            ruleResolver = new WaterRuleResolver();
            calculator = new WaterDemandCalculator();
            roofCalculator = new RoofDrainageCalculator();
        }

        public SimpleCatalog<FixtureDefinition> GetFixtureCatalog()
        {
            return dataRegistry.GetRequired<
                SimpleCatalog<FixtureDefinition>>(
                "Catalog.Fixtures",
                DataVersion);
        }

        public WaterRoofDrainageInputInfo GetRoofDrainageInputInfo()
        {
            RuleSetDescriptor roof = ruleRegistry.GetRequiredRuleSet(
                "HU.Water.RoofDrainage",
                "1.1");

            double rainfall = roof.Parameters.GetRequiredDouble(
                "DefaultRainfallIntensity");

            List<WaterRoofDrainageOption> roofTypes =
                roof.Parameters.Values
                    .Where(pair => pair.Key.StartsWith(
                        "DefaultRunoffCoefficient.",
                        StringComparison.OrdinalIgnoreCase))
                    .Select(pair =>
                    {
                        string id = pair.Key.Substring(
                            "DefaultRunoffCoefficient.".Length);

                        // DisplayName az XML-ből jön (pl. DisplayName.FlatRoof = Lapostető)
                        // Ha nincs bejegyezve, az azonosítót használjuk fallbackként
                        string displayName = roof.Parameters
                            .GetStringOrDefault("DisplayName." + id, id);

                        return new WaterRoofDrainageOption(
                            id,
                            displayName,
                            roof.Parameters.GetDoubleOrDefault(
                                pair.Key,
                                1.0));
                    })
                    .OrderBy(item => item.DisplayName)
                    .ToList();

            return new WaterRoofDrainageInputInfo(
                rainfall,
                roofTypes);
        }

        public WaterDailyDemandInputInfo GetDailyDemandInputInfo(
            ProjectData project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            project.NormalizeAfterLoad();

            SimpleCatalog<BuildingProfileDefinition> profiles =
                GetProfiles();

            // Ha nincs épületfunkció, alapértelmezett inputinfot adunk vissza
            if (!TryResolveBuildingCategory(
                project.BuildingFunctionId,
                out string category))
            {
                return new WaterDailyDemandInputInfo(
                    true, false, "Lakások száma", "Lakók száma", "fő", null);
            }

            WaterResolvedRules rules = ruleResolver.Resolve(
                ruleRegistry,
                profiles,
                project.BuildingProfileId,
                project.BuildingFunctionId,
                category);

            bool isPersonBased =
                string.IsNullOrWhiteSpace(rules.DailyWaterRateId) ||
                string.Equals(
                    rules.DailyWaterRateId,
                    "DailyWaterPerPerson",
                    StringComparison.OrdinalIgnoreCase);

            bool isLowDensityResidential = string.Equals(
                project.BuildingProfileId,
                "Residential.LowDensity",
                StringComparison.OrdinalIgnoreCase);

            bool supportsDwellingSuggestion =
                isPersonBased &&
                rules.DefaultPersonsPerDwelling.HasValue &&
                !isLowDensityResidential;

            return new WaterDailyDemandInputInfo(
                isPersonBased,
                supportsDwellingSuggestion,
                supportsDwellingSuggestion
                    ? "Lakások száma"
                    : rules.DailyWaterInputLabel,
                "Létszám",
                rules.DailyWaterUnitLabel,
                rules.DailyWaterRate);
        }

        public WaterOccupancySuggestion SuggestOccupancy(
            ProjectData project,
            int? dwellingCount)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            if (!dwellingCount.HasValue ||
                dwellingCount.Value <= 0)
            {
                return WaterOccupancySuggestion.Missing(
                    "A javaslathoz add meg a lakások számát.");
            }

            if (string.IsNullOrWhiteSpace(
                project.BuildingProfileId))
            {
                return WaterOccupancySuggestion.Missing(
                    "A projekthez nem tartozik épületprofil.");
            }

            if (!TryResolveBuildingCategory(
                project.BuildingFunctionId,
                out string category))
            {
                return WaterOccupancySuggestion.Missing(
                    "A projekt épületfunkciója nincs megadva.");
            }

            SimpleCatalog<BuildingProfileDefinition> profiles =
                GetProfiles();

            WaterResolvedRules rules = ruleResolver.Resolve(
                ruleRegistry,
                profiles,
                project.BuildingProfileId,
                project.BuildingFunctionId,
                category);

            if (!rules.DefaultPersonsPerDwelling.HasValue)
            {
                return WaterOccupancySuggestion.Missing(
                    "A profil nem tartalmaz fő/lakás javaslati értéket.");
            }

            return WaterOccupancySuggestion.Create(
                dwellingCount.Value,
                rules.DefaultPersonsPerDwelling.Value);
        }

        public WaterCalculationResult Calculate(
            ProjectData project,
            WaterModuleInput input)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            if (input == null)
                throw new ArgumentNullException(nameof(input));

            project.NormalizeAfterLoad();

            SimpleCatalog<FixtureDefinition> fixtureSource =
                GetFixtureCatalog();

            SimpleCatalog<BuildingProfileDefinition> profiles =
                GetProfiles();

            // Ha az épületfunkció nincs beállítva vagy nem azonosítható, értelmes
            // diagnosztikát adunk vissza kivétel dobása helyett
            if (!TryResolveBuildingCategory(
                project.BuildingFunctionId,
                out string category))
            {
                return BuildMissingFunctionResult();
            }

            WaterResolvedRules rules = ruleResolver.Resolve(
                ruleRegistry,
                profiles,
                project.BuildingProfileId,
                project.BuildingFunctionId,
                category);

            IReadOnlyDictionary<string, FixtureCatalogItem> fixtures =
                fixtureAdapter.Adapt(fixtureSource);

            var calculationInput = new WaterDemandInput(
                project.BuildingFunctionId,
                project.BuildingProfileId,
                input.DwellingCount,
                input.Occupancy,
                input.DemandUnitCount,
                input.FixtureUsages,
                input.GreywaterEnabled);

            CalculationResult<DailyWaterDemandResult> daily =
                calculator.CalculateDailyDemand(
                    calculationInput,
                    rules.DefaultPersonsPerDwelling,
                    rules.DailyWaterRate,
                    rules.DailyWaterRateId,
                    rules.DailyWaterUnitLabel,
                    project.BuildingProfileId,
                    BuildRuleKey(rules.DailyRule));

            CalculationResult<DhwDemandResult> dhw =
                calculator.CalculateDhwDemand(
                    calculationInput,
                    rules.DailyHotWaterRate,
                    rules.DailyHotWaterRateId,
                    rules.DefaultPersonsPerDwelling,
                    rules.DhwReferenceTemperature,
                    rules.ColdWaterReferenceTemperature,
                    rules.WaterSpecificHeatCapacity,
                    rules.WaterDensity,
                    BuildRuleKey(rules.DhwRule));

            CalculationResult<PeakWaterDemandResult> peak =
                calculator.CalculatePeakWaterDemand(
                    calculationInput,
                    fixtures,
                    rules.PeakConversionA,
                    rules.PeakConversionExponent,
                    rules.PeakConversionOffset,
                    rules.PeakRule.MethodId,
                    BuildRuleKey(rules.PeakRule));

            CalculationResult<WastewaterFlowResult> wastewater =
                calculator.CalculateWastewaterFlow(
                    calculationInput,
                    fixtures,
                    rules.WastewaterFrequencyFactor,
                    rules.ContinuousWastewaterFlow,
                    rules.PumpedWastewaterFlow,
                    rules.WastewaterRule.MethodId,
                    BuildRuleKey(rules.WastewaterRule));

            CalculationResult<GreywaterBalanceResult> greywater =
                CalculateGreywater(
                    input,
                    fixtures,
                    daily,
                    BuildRuleKey(rules.GreywaterRule));

            CalculationResult<RoofDrainageResult> roofDrainage =
                CalculateRoofDrainage(
                    input,
                    rules);

            return new WaterCalculationResult(
                daily,
                dhw,
                peak,
                wastewater,
                greywater,
                roofDrainage,
                rules);
        }

        private CalculationResult<GreywaterBalanceResult> CalculateGreywater(
            WaterModuleInput input,
            IReadOnlyDictionary<string, FixtureCatalogItem> fixtures,
            CalculationResult<DailyWaterDemandResult> daily,
            string ruleSetKey)
        {
            if (!input.GreywaterEnabled)
            {
                return calculator.CalculateGreywaterBalance(
                    false,
                    0.0,
                    0.0,
                    ruleSetKey);
            }

            double dailyLitres =
                daily.Result?.DailyDemandLitres ?? 0.0;

            double totalWeight = 0.0;
            double sourceWeight = 0.0;
            double demandWeight = 0.0;

            foreach (FixtureUsage usage in input.FixtureUsages)
            {
                if (!fixtures.TryGetValue(
                    usage.FixtureId,
                    out FixtureCatalogItem fixture))
                {
                    continue;
                }

                double weight =
                    Math.Max(
                        0.0,
                        fixture.PotableLoadingUnit ?? 0.0) *
                    usage.Quantity;

                totalWeight += weight;

                if (IsGreywaterSource(fixture))
                    sourceWeight += weight;

                if (fixture.GreywaterDemand)
                    demandWeight += weight;
            }

            double sourceLitres = totalWeight > 0.0
                ? dailyLitres * sourceWeight / totalWeight
                : 0.0;

            double demandLitres = totalWeight > 0.0
                ? dailyLitres * demandWeight / totalWeight
                : 0.0;

            return calculator.CalculateGreywaterBalance(
                true,
                sourceLitres,
                demandLitres,
                ruleSetKey);
        }

        private CalculationResult<RoofDrainageResult> CalculateRoofDrainage(
            WaterModuleInput input,
            WaterResolvedRules rules)
        {
            if (!input.RoofDrainageEnabled)
            {
                return new CalculationResult<RoofDrainageResult>(
                    null,
                    CalculationStatus.NotApplicable,
                    null,
                    new[] { BuildRuleKey(rules.RoofDrainageRule) },
                    null,
                    null,
                    null);
            }

            if (!input.RoofAreaSquareMetres.HasValue ||
                !input.RainfallIntensity.HasValue)
            {
                return roofCalculator.Calculate(
                    new RoofDrainageInput(
                        Array.Empty<RoofCatchment>(),
                        1.0),
                    BuildRuleKey(rules.RoofDrainageRule));
            }

            WaterRoofDrainageInputInfo roofInfo =
                GetRoofDrainageInputInfo();

            WaterRoofDrainageOption option =
                roofInfo.RoofTypes.FirstOrDefault(item =>
                    string.Equals(
                        item.Id,
                        input.RoofType,
                        StringComparison.OrdinalIgnoreCase)) ??
                roofInfo.RoofTypes.FirstOrDefault();

            double runoff =
                option?.RunoffCoefficient ?? 1.0;

            return roofCalculator.Calculate(
                new RoofDrainageInput(
                    new[]
                    {
                        new RoofCatchment(
                            "Roof",
                            input.RoofAreaSquareMetres.Value,
                            runoff)
                    },
                    input.RainfallIntensity.Value),
                BuildRuleKey(rules.RoofDrainageRule));
        }

        private static bool IsGreywaterSource(
            FixtureCatalogItem fixture)
        {
            return string.Equals(
                    fixture.GreywaterSource,
                    GreywaterSourceValues.True,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    fixture.GreywaterSource,
                    GreywaterSourceValues.Conditional,
                    StringComparison.OrdinalIgnoreCase);
        }

        private SimpleCatalog<BuildingProfileDefinition> GetProfiles()
        {
            return dataRegistry.GetRequired<
                SimpleCatalog<BuildingProfileDefinition>>(
                "Profiles.Building",
                DataVersion);
        }

        private bool TryResolveBuildingCategory(
            string functionId,
            out string category)
        {
            category = string.Empty;

            if (string.IsNullOrWhiteSpace(functionId))
                return false;

            SimpleCatalog<BuildingFunctionDefinition> functions =
                dataRegistry.GetRequired<
                    SimpleCatalog<BuildingFunctionDefinition>>(
                    "Functions.Building",
                    DataVersion);

            if (!functions.TryGet(
                functionId,
                out BuildingFunctionDefinition function))
            {
                return false;
            }

            category = function.Category;
            return true;
        }

        private static string BuildRuleKey(
            WaterRuleInfo rule)
        {
            return rule.RuleSetId +
                "@" +
                rule.Version;
        }

        // A tetőtípus megjelenítési nevét az XML rules-water.xml fájl
        // DisplayName.{id} paraméterei adják meg (pl. DisplayName.FlatRoof = Lapostető).
        // A GetRoofDrainageInputInfo() közvetlenül olvassa azokat, erre a metódusra nincs szükség.

        /// <summary>
        /// Épületfunkció hiányakor minden aleredményre WaitingForInput állapotú eredményt épít.
        /// </summary>
        private static WaterCalculationResult BuildMissingFunctionResult()
        {
            string ruleKey = string.Empty;

            CalculationResult<DailyWaterDemandResult> daily =
                new CalculationResultBuilder<DailyWaterDemandResult>()
                .AddDiagnostic(new CalculationDiagnostic(
                    "MISSING_BUILDING_FUNCTION",
                    "Az épületfunkció nincs megadva. Állítsd be a projektadatoknál.",
                    CalculationDiagnosticSeverity.Error))
                .BuildWaitingForInput(null);

            CalculationResult<PeakWaterDemandResult> peak =
                new CalculationResultBuilder<PeakWaterDemandResult>()
                .AddDiagnostic(new CalculationDiagnostic(
                    "MISSING_BUILDING_FUNCTION",
                    "Az épületfunkció nincs megadva.",
                    CalculationDiagnosticSeverity.Error))
                .BuildWaitingForInput(null);

            CalculationResult<WastewaterFlowResult> wastewater =
                new CalculationResultBuilder<WastewaterFlowResult>()
                .AddDiagnostic(new CalculationDiagnostic(
                    "MISSING_BUILDING_FUNCTION",
                    "Az épületfunkció nincs megadva.",
                    CalculationDiagnosticSeverity.Error))
                .BuildWaitingForInput(null);

            CalculationResult<GreywaterBalanceResult> greywater =
                new CalculationResultBuilder<GreywaterBalanceResult>()
                .AddDiagnostic(new CalculationDiagnostic(
                    "MISSING_BUILDING_FUNCTION",
                    "Az épületfunkció nincs megadva.",
                    CalculationDiagnosticSeverity.Error))
                .BuildWaitingForInput(null);

            CalculationResult<DhwDemandResult> dhw =
                new CalculationResultBuilder<DhwDemandResult>()
                .AddDiagnostic(new CalculationDiagnostic(
                    "MISSING_BUILDING_FUNCTION",
                    "Az épületfunkció nincs megadva.",
                    CalculationDiagnosticSeverity.Error))
                .BuildWaitingForInput(null);

            CalculationResult<RoofDrainageResult> roofDrainage =
                new CalculationResultBuilder<RoofDrainageResult>()
                .AddDiagnostic(new CalculationDiagnostic(
                    "MISSING_BUILDING_FUNCTION",
                    "Az épületfunkció nincs megadva.",
                    CalculationDiagnosticSeverity.Error))
                .BuildWaitingForInput(null);

            // Semleges placeholder szabályok a hibaállapotú eredményhez
            var emptyRuleInfo = new WaterRuleInfo(
                string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty, null, null);

            var emptyRules = new WaterResolvedRules(
                emptyRuleInfo, emptyRuleInfo, emptyRuleInfo, emptyRuleInfo, emptyRuleInfo, emptyRuleInfo,
                null, null, null,
                string.Empty, string.Empty, string.Empty,
                null, string.Empty,
                0.20, 0.50, 0.00, 0.50, 0.00, 0.00,
                60.0, 10.0, 4.187, 1.0);

            return new WaterCalculationResult(
                daily, dhw, peak, wastewater, greywater, roofDrainage, emptyRules);
        }
    }
}


