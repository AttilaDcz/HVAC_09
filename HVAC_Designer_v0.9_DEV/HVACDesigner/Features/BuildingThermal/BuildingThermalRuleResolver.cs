using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using HVACDesigner.EngineeringData.Rules.Common;
using HVACDesigner.EngineeringData.SimpleCatalogs;

namespace HVACDesigner.Features.BuildingThermal
{
    public sealed class BuildingThermalRuleInfo
    {
        public string RuleSetId { get; }
        public string Version { get; }
        public string MethodId { get; }
        public string DisplayName { get; }
        public IReadOnlyList<string> References { get; }
        public RuleParameterSet Parameters { get; }

        public BuildingThermalRuleInfo(
            string ruleSetId,
            string version,
            string methodId,
            string displayName,
            IEnumerable<string> references,
            RuleParameterSet parameters)
        {
            RuleSetId = ruleSetId ?? string.Empty;
            Version = version ?? string.Empty;
            MethodId = methodId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            References = new ReadOnlyCollection<string>(
                (references ?? Array.Empty<string>()).ToList());
            Parameters = parameters ?? new RuleParameterSet(new Dictionary<string, string>());
        }
    }

    public sealed class BuildingThermalResolvedRules
    {
        public BuildingThermalRuleInfo EnvelopeRule { get; }

        // Követelmény U-értékek [W/m²K]
        public double ExternalWallMaxU { get; }
        public double FlatRoofMaxU { get; }
        public double FloorOnGroundMaxU { get; }
        public double BasementFloorMaxU { get; }
        public double WindowMaxU { get; }
        public double EntranceDoorMaxU { get; }
        public double SeparatingWallMaxU { get; }

        public BuildingThermalResolvedRules(
            BuildingThermalRuleInfo envelopeRule,
            double externalWallMaxU,
            double flatRoofMaxU,
            double floorOnGroundMaxU,
            double basementFloorMaxU,
            double windowMaxU,
            double entranceDoorMaxU,
            double separatingWallMaxU)
        {
            EnvelopeRule = envelopeRule;
            ExternalWallMaxU = externalWallMaxU;
            FlatRoofMaxU = flatRoofMaxU;
            FloorOnGroundMaxU = floorOnGroundMaxU;
            BasementFloorMaxU = basementFloorMaxU;
            WindowMaxU = windowMaxU;
            EntranceDoorMaxU = entranceDoorMaxU;
            SeparatingWallMaxU = separatingWallMaxU;
        }
    }

    /// <summary>
    /// Az épületszerkezeti U-értékek és szabályozások (ÉKM vs TNM) feloldó osztálya.
    /// A WaterRuleResolver mintájára készült.
    /// </summary>
    public sealed class BuildingThermalRuleResolver
    {
        public BuildingThermalResolvedRules Resolve(
            EngineeringRuleRegistry registry,
            string methodId) // "Hungary.EKM" vagy "Hungary.TNM.Legacy"
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            RuleSetDescriptor envelope = null;

            if (string.Equals(methodId, "Hungary.EKM", StringComparison.OrdinalIgnoreCase))
            {
                envelope = registry.GetRequiredRuleSet("HU.EKM.Envelope", "2023.1");
            }
            else if (string.Equals(methodId, "Hungary.TNM.Legacy", StringComparison.OrdinalIgnoreCase))
            {
                envelope = registry.GetRequiredRuleSet("HU.TNM.Legacy.Envelope", "2023-10-31");
            }
            else
            {
                // Fallback az ÉKM-re, ha a megadott módszer ismeretlen
                envelope = registry.GetRequiredRuleSet("HU.EKM.Envelope", "2023.1");
            }

            var envelopeRuleInfo = CreateRuleInfo(envelope);

            // U-érték határok kiolvasása a szabály paraméterekből
            double extWallU = GetDoubleParam(envelope.Parameters, "ExternalWall.MaximumU", 0.20);
            double flatRoofU = GetDoubleParam(envelope.Parameters, "FlatRoof.MaximumU", 0.17);
            double floorOnGroundU = GetDoubleParam(envelope.Parameters, "FloorOnGround.MaximumU", 0.30);
            double basementFloorU = GetDoubleParam(envelope.Parameters, "BasementFloor.MaximumU", 0.26);
            double windowU = GetDoubleParam(envelope.Parameters, "Window.MaximumU", 1.10);
            double entranceDoorU = GetDoubleParam(envelope.Parameters, "EntranceDoor.MaximumU", 1.40);
            double separatingWallU = GetDoubleParam(envelope.Parameters, "SeparatingWall.MaximumU", 0.40);

            return new BuildingThermalResolvedRules(
                envelopeRuleInfo,
                extWallU,
                flatRoofU,
                floorOnGroundU,
                basementFloorU,
                windowU,
                entranceDoorU,
                separatingWallU);
        }

        private static BuildingThermalRuleInfo CreateRuleInfo(RuleSetDescriptor rule)
        {
            return new BuildingThermalRuleInfo(
                rule.RuleSetId,
                rule.Version,
                rule.MethodId,
                rule.Name,
                rule.References.Select(item => item.Designation),
                rule.Parameters);
        }

        private static double GetDoubleParam(RuleParameterSet parameters, string key, double defaultValue)
        {
            string val = parameters.GetStringOrDefault(key, string.Empty);
            if (string.IsNullOrWhiteSpace(val))
                return defaultValue;

            if (double.TryParse(
                val,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double parsed))
            {
                return parsed;
            }

            return defaultValue;
        }
    }
}
