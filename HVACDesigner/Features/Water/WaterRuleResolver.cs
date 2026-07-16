using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using HVACDesigner.EngineeringData.Rules.Common;
using HVACDesigner.EngineeringData.SimpleCatalogs;

namespace HVACDesigner.Features.Water
{
    public sealed class WaterRuleInfo
    {
        public string RuleSetId { get; }
        public string Version { get; }
        public string MethodId { get; }
        public string DataStatus { get; }
        public string DisplayName { get; }
        public IReadOnlyList<string> References { get; }
        public RuleParameterSet Parameters { get; }

        public bool UsesMvpData =>
            string.Equals(
                DataStatus,
                "MvpSampleOnly",
                StringComparison.OrdinalIgnoreCase);

        public WaterRuleInfo(
            string ruleSetId,
            string version,
            string methodId,
            string dataStatus,
            string displayName,
            IEnumerable<string> references,
            RuleParameterSet parameters)
        {
            RuleSetId = ruleSetId ?? string.Empty;
            Version = version ?? string.Empty;
            MethodId = methodId ?? string.Empty;
            DataStatus = dataStatus ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            References = new ReadOnlyCollection<string>(
                (references ?? Array.Empty<string>()).ToList());
            Parameters = parameters ?? new RuleParameterSet(new Dictionary<string, string>());
        }
    }

    public sealed class WaterResolvedRules
    {
        public WaterRuleInfo DailyRule { get; }
        public WaterRuleInfo DhwRule { get; }
        public WaterRuleInfo PeakRule { get; }
        public WaterRuleInfo WastewaterRule { get; }
        public WaterRuleInfo GreywaterRule { get; }
        public WaterRuleInfo RoofDrainageRule { get; }

        public double? DefaultPersonsPerDwelling { get; }
        public double? DailyWaterPerPerson { get; }
        public double? DailyWaterRate { get; }
        public string DailyWaterRateId { get; }
        public string DailyWaterUnitLabel { get; }
        public string DailyWaterInputLabel { get; }

        public double? DailyHotWaterRate { get; }
        public string DailyHotWaterRateId { get; }

        public double PeakConversionA { get; }
        public double PeakConversionExponent { get; }
        public double PeakConversionOffset { get; }

        public double WastewaterFrequencyFactor { get; }
        public double ContinuousWastewaterFlow { get; }
        public double PumpedWastewaterFlow { get; }

        public double DhwReferenceTemperature { get; }
        public double ColdWaterReferenceTemperature { get; }
        public double WaterSpecificHeatCapacity { get; }
        public double WaterDensity { get; }

        public WaterResolvedRules(
            WaterRuleInfo dailyRule,
            WaterRuleInfo dhwRule,
            WaterRuleInfo peakRule,
            WaterRuleInfo wastewaterRule,
            WaterRuleInfo greywaterRule,
            WaterRuleInfo roofDrainageRule,
            double? defaultPersonsPerDwelling,
            double? dailyWaterPerPerson,
            double? dailyWaterRate,
            string dailyWaterRateId,
            string dailyWaterUnitLabel,
            string dailyWaterInputLabel,
            double? dailyHotWaterRate,
            string dailyHotWaterRateId,
            double peakConversionA,
            double peakConversionExponent,
            double peakConversionOffset,
            double wastewaterFrequencyFactor,
            double continuousWastewaterFlow,
            double pumpedWastewaterFlow,
            double dhwReferenceTemperature,
            double coldWaterReferenceTemperature,
            double waterSpecificHeatCapacity,
            double waterDensity)
        {
            DailyRule = dailyRule;
            DhwRule = dhwRule;
            PeakRule = peakRule;
            WastewaterRule = wastewaterRule;
            GreywaterRule = greywaterRule;
            RoofDrainageRule = roofDrainageRule;
            DefaultPersonsPerDwelling = defaultPersonsPerDwelling;
            DailyWaterPerPerson = dailyWaterPerPerson;
            DailyWaterRate = dailyWaterRate;
            DailyWaterRateId = dailyWaterRateId ?? string.Empty;
            DailyWaterUnitLabel = dailyWaterUnitLabel ?? "egység";
            DailyWaterInputLabel = dailyWaterInputLabel ?? "Mennyiség";
            DailyHotWaterRate = dailyHotWaterRate;
            DailyHotWaterRateId = dailyHotWaterRateId ?? string.Empty;
            PeakConversionA = peakConversionA;
            PeakConversionExponent = peakConversionExponent;
            PeakConversionOffset = peakConversionOffset;
            WastewaterFrequencyFactor = wastewaterFrequencyFactor;
            ContinuousWastewaterFlow = continuousWastewaterFlow;
            PumpedWastewaterFlow = pumpedWastewaterFlow;
            DhwReferenceTemperature = dhwReferenceTemperature;
            ColdWaterReferenceTemperature = coldWaterReferenceTemperature;
            WaterSpecificHeatCapacity = waterSpecificHeatCapacity;
            WaterDensity = waterDensity;
        }
    }

    /// <summary>
    /// A Water modul által használt RuleSetek vékony feloldója.
    /// Nem olvas XML-t közvetlenül.
    /// </summary>
    public sealed class WaterRuleResolver
    {
        private const string RuleVersion = "1.1";

        public WaterResolvedRules Resolve(
            EngineeringRuleRegistry registry,
            SimpleCatalog<BuildingProfileDefinition> profiles,
            string profileId,
            string buildingFunctionId,
            string buildingCategory)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            if (profiles == null)
                throw new ArgumentNullException(nameof(profiles));

            RuleSetDescriptor daily = registry.GetRequiredRuleSet(
                "HU.Water.DailyDemand",
                RuleVersion);

            RuleSetDescriptor dhw = registry.GetRequiredRuleSet(
                "HU.Water.DhwDemand",
                RuleVersion);

            RuleSetDescriptor peak = registry.GetRequiredRuleSet(
                "HU.Water.PeakDemand",
                RuleVersion);

            RuleSetDescriptor wastewater = registry.GetRequiredRuleSet(
                "HU.Water.Wastewater",
                RuleVersion);

            RuleSetDescriptor greywater = registry.GetRequiredRuleSet(
                "HU.Water.Greywater",
                RuleVersion);

            RuleSetDescriptor roofDrainage = registry.GetRequiredRuleSet(
                "HU.Water.RoofDrainage",
                RuleVersion);

            double? personsPerDwelling = null;
            double? dailyWaterPerPerson = null;
            BuildingProfileValue dailyWaterBasis = null;
            BuildingProfileValue dailyHotWaterBasis = null;

            if (!string.IsNullOrWhiteSpace(profileId) &&
                profiles.TryGet(
                    profileId,
                    out BuildingProfileDefinition profile))
            {
                personsPerDwelling = FindProfileNumber(
                    profile,
                    "DefaultPersonsPerDwelling");

                dailyWaterPerPerson = FindProfileNumber(
                    profile,
                    "DailyWaterPerPerson");

                dailyWaterBasis = FindDailyWaterBasis(profile);
                dailyHotWaterBasis = FindDailyHotWaterBasis(profile);
            }

            string systemType = ResolveWastewaterSystemType(
                buildingFunctionId,
                buildingCategory);

            return new WaterResolvedRules(
                CreateRuleInfo(daily),
                CreateRuleInfo(dhw),
                CreateRuleInfo(peak),
                CreateRuleInfo(wastewater),
                CreateRuleInfo(greywater),
                CreateRuleInfo(roofDrainage),
                personsPerDwelling,
                dailyWaterPerPerson,
                ParseProfileNumber(dailyWaterBasis),
                dailyWaterBasis?.Id ?? string.Empty,
                ResolveDailyWaterUnitLabel(dailyWaterBasis?.Id),
                ResolveDailyWaterInputLabel(dailyWaterBasis?.Id),
                ParseProfileNumber(dailyHotWaterBasis),
                dailyHotWaterBasis?.Id ?? string.Empty,
                peak.Parameters.GetRequiredDouble("Conversion.A"),
                peak.Parameters.GetRequiredDouble("Conversion.Exponent"),
                peak.Parameters.GetRequiredDouble("Conversion.Offset"),
                wastewater.Parameters.GetRequiredDouble(
                    "SystemType." +
                    systemType +
                    ".FrequencyFactor"),
                wastewater.Parameters.GetDoubleOrDefault(
                    "ContinuousFlow.Default",
                    0.0),
                wastewater.Parameters.GetDoubleOrDefault(
                    "PumpedFlow.Default",
                    0.0),
                dhw.Parameters.GetRequiredDouble("DhwReferenceTemperature"),
                dhw.Parameters.GetRequiredDouble("ColdWaterReferenceTemperature"),
                dhw.Parameters.GetRequiredDouble("WaterSpecificHeatCapacity"),
                dhw.Parameters.GetRequiredDouble("WaterDensity"));
        }

        private static WaterRuleInfo CreateRuleInfo(
            RuleSetDescriptor rule)
        {
            return new WaterRuleInfo(
                rule.RuleSetId,
                rule.Version,
                rule.MethodId,
                rule.Parameters.GetStringOrDefault(
                    "DataStatus",
                    string.Empty),
                rule.Name,
                rule.References.Select(item => item.Designation),
                rule.Parameters);
        }

        private static double? FindProfileNumber(
            BuildingProfileDefinition profile,
            string valueId)
        {
            BuildingProfileValue value = profile.Sections
                .SelectMany(section => section.Values)
                .FirstOrDefault(item =>
                    string.Equals(
                        item.Id,
                        valueId,
                        StringComparison.OrdinalIgnoreCase));

            if (value == null)
                return null;

            if (!double.TryParse(
                value.Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double parsed))
            {
                return null;
            }

            return parsed;
        }

        private static BuildingProfileValue FindDailyWaterBasis(
            BuildingProfileDefinition profile)
        {
            string[] priority =
            {
                "DailyWaterPerPerson",
                "DailyWaterPerBed",
                "DailyWaterPerGuest",
                "DailyWaterPerMeal",
                "DailyWaterPerUser"
            };

            foreach (string id in priority)
            {
                BuildingProfileValue value = profile.Sections
                    .Where(section => string.Equals(
                        section.Name,
                        "Water",
                        StringComparison.OrdinalIgnoreCase))
                    .SelectMany(section => section.Values)
                    .FirstOrDefault(item => string.Equals(
                        item.Id,
                        id,
                        StringComparison.OrdinalIgnoreCase));

                if (value != null)
                    return value;
            }

            return null;
        }

        private static BuildingProfileValue FindDailyHotWaterBasis(
            BuildingProfileDefinition profile)
        {
            string[] priority =
            {
                "DailyHotWaterPerPerson",
                "DailyHotWaterPerBed",
                "DailyHotWaterPerGuest",
                "DailyHotWaterPerMeal",
                "DailyHotWaterPerUser"
            };

            foreach (string id in priority)
            {
                BuildingProfileValue value = profile.Sections
                    .Where(section => string.Equals(
                        section.Name,
                        "Water",
                        StringComparison.OrdinalIgnoreCase))
                    .SelectMany(section => section.Values)
                    .FirstOrDefault(item => string.Equals(
                        item.Id,
                        id,
                        StringComparison.OrdinalIgnoreCase));

                if (value != null)
                    return value;
            }

            return null;
        }

        private static double? ParseProfileNumber(
            BuildingProfileValue value)
        {
            if (value == null)
                return null;

            if (!double.TryParse(
                value.Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double parsed))
            {
                return null;
            }

            return parsed;
        }

        private static string ResolveDailyWaterUnitLabel(
            string rateId)
        {
            if (string.Equals(rateId, "DailyWaterPerBed", StringComparison.OrdinalIgnoreCase))
                return "férőhely";

            if (string.Equals(rateId, "DailyWaterPerGuest", StringComparison.OrdinalIgnoreCase))
                return "vendég";

            if (string.Equals(rateId, "DailyWaterPerMeal", StringComparison.OrdinalIgnoreCase))
                return "adag";

            if (string.Equals(rateId, "DailyWaterPerUser", StringComparison.OrdinalIgnoreCase))
                return "használó";

            return "fő";
        }

        private static string ResolveDailyWaterInputLabel(
            string rateId)
        {
            if (string.Equals(rateId, "DailyWaterPerBed", StringComparison.OrdinalIgnoreCase))
                return "Férőhelyek száma";

            if (string.Equals(rateId, "DailyWaterPerGuest", StringComparison.OrdinalIgnoreCase))
                return "Vendégek száma";

            if (string.Equals(rateId, "DailyWaterPerMeal", StringComparison.OrdinalIgnoreCase))
                return "Adagok száma/nap";

            if (string.Equals(rateId, "DailyWaterPerUser", StringComparison.OrdinalIgnoreCase))
                return "Használók száma";

            return "Létszám";
        }

        private static string ResolveWastewaterSystemType(
            string functionId,
            string category)
        {
            string[] supportedCategories =
            {
                "Residential",
                "Office",
                "Childcare",
                "Education",
                "Retail",
                "Accommodation",
                "Healthcare",
                "Hospitality",
                "Sports",
                "Industrial",
                "Storage"
            };

            string resolvedCategory = supportedCategories
                .FirstOrDefault(item => string.Equals(
                    item,
                    category,
                    StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(resolvedCategory))
                return resolvedCategory;

            throw new InvalidOperationException(
                "Az épületfunkcióhoz nem oldható fel szennyvíz gyakorisági tényező.");
        }
    }
}


