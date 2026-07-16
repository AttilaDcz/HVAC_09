using System;
using System.Collections.Generic;
using System.Globalization;
using HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates;
using HVACDesigner.EngineeringData.Rules.Common;

namespace HVACDesigner.Features.BuildingThermal
{
    public sealed class ThermalBridgeCorrectionOption
    {
        public string Id { get; }
        public string DisplayName { get; }
        public ConstructionType ConstructionType { get; }
        public double CorrectionFactor { get; }
        public string SourceReference { get; }

        public ThermalBridgeCorrectionOption(
            string id,
            string displayName,
            ConstructionType constructionType,
            double correctionFactor,
            string sourceReference)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            ConstructionType = constructionType;
            CorrectionFactor = correctionFactor;
            SourceReference = sourceReference ?? string.Empty;
        }

        public override string ToString() => DisplayName;
    }

    public sealed class ThermalBridgeRuleResolver
    {
        public IReadOnlyList<ThermalBridgeCorrectionOption> ResolveOptions(EngineeringRuleRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            var options = new List<ThermalBridgeCorrectionOption>();

            // TNM szabálycsomag lekérése
            var tnmEnvelope = registry.GetRequiredRuleSet("HU.TNM.Legacy.Envelope", "2023-10-31");
            string sourceRef = tnmEnvelope.Name; // "TNM legacy szerkezeti követelmények"

            double GetParamValue(string name, double fallback)
            {
                string val = tnmEnvelope.Parameters.GetStringOrDefault(name, string.Empty);
                return double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) ? d : fallback;
            }

            // Külső fal - megszakítatlan hőszigetelés
            double continuousWeak = GetParamValue("ThermalBridge.ExternalWall.Continuous.Weak", 0.15);
            double continuousMedium = GetParamValue("ThermalBridge.ExternalWall.Continuous.Medium", 0.20);
            double continuousStrong = GetParamValue("ThermalBridge.ExternalWall.Continuous.Strong", 0.30);

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.ExternalWall.Continuous.Weak",
                $"Külső fal - megszakítatlan, gyenge ({(continuousWeak * 100):F0}%)",
                ConstructionType.ExternalWall,
                continuousWeak,
                sourceRef));

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.ExternalWall.Continuous.Medium",
                $"Külső fal - megszakítatlan, közepes ({(continuousMedium * 100):F0}%)",
                ConstructionType.ExternalWall,
                continuousMedium,
                sourceRef));

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.ExternalWall.Continuous.Strong",
                $"Külső fal - megszakítatlan, erős ({(continuousStrong * 100):F0}%)",
                ConstructionType.ExternalWall,
                continuousStrong,
                sourceRef));

            // Egyéb külső fal
            double otherWeak = GetParamValue("ThermalBridge.ExternalWall.Other.Weak", 0.25);
            double otherMedium = GetParamValue("ThermalBridge.ExternalWall.Other.Medium", 0.30);
            double otherStrong = GetParamValue("ThermalBridge.ExternalWall.Other.Strong", 0.40);

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.ExternalWall.Other.Weak",
                $"Egyéb külső fal - gyenge ({(otherWeak * 100):F0}%)",
                ConstructionType.ExternalWall,
                otherWeak,
                sourceRef));

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.ExternalWall.Other.Medium",
                $"Egyéb külső fal - közepes ({(otherMedium * 100):F0}%)",
                ConstructionType.ExternalWall,
                otherMedium,
                sourceRef));

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.ExternalWall.Other.Strong",
                $"Egyéb külső fal - erős ({(otherStrong * 100):F0}%)",
                ConstructionType.ExternalWall,
                otherStrong,
                sourceRef));

            // Lapostető
            double flatWeak = GetParamValue("ThermalBridge.Roof.Flat.Weak", 0.10);
            double flatMedium = GetParamValue("ThermalBridge.Roof.Flat.Medium", 0.15);
            double flatStrong = GetParamValue("ThermalBridge.Roof.Flat.Strong", 0.20);

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.Roof.Flat.Weak",
                $"Lapostető - gyenge ({(flatWeak * 100):F0}%)",
                ConstructionType.Roof,
                flatWeak,
                sourceRef));

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.Roof.Flat.Medium",
                $"Lapostető - közepes ({(flatMedium * 100):F0}%)",
                ConstructionType.Roof,
                flatMedium,
                sourceRef));

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.Roof.Flat.Strong",
                $"Lapostető - erős ({(flatStrong * 100):F0}%)",
                ConstructionType.Roof,
                flatStrong,
                sourceRef));

            // Beépített tetőteret határoló szerkezet
            double atticWeak = GetParamValue("ThermalBridge.Roof.Attic.Weak", 0.10);
            double atticMedium = GetParamValue("ThermalBridge.Roof.Attic.Medium", 0.15);
            double atticStrong = GetParamValue("ThermalBridge.Roof.Attic.Strong", 0.20);

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.Roof.Attic.Weak",
                $"Beépített tetőtér - gyenge ({(atticWeak * 100):F0}%)",
                ConstructionType.Roof,
                atticWeak,
                sourceRef));

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.Roof.Attic.Medium",
                $"Beépített tetőtér - közepes ({(atticMedium * 100):F0}%)",
                ConstructionType.Roof,
                atticMedium,
                sourceRef));

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.Roof.Attic.Strong",
                $"Beépített tetőtér - erős ({(atticStrong * 100):F0}%)",
                ConstructionType.Roof,
                atticStrong,
                sourceRef));

            // Padlásfödém (egyszerűsített)
            double ceilingAttic = GetParamValue("ThermalBridge.Ceiling.Attic.Default", 0.10);
            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.Ceiling.Attic.Default",
                $"Padlásfödém - általános ({(ceilingAttic * 100):F0}%)",
                ConstructionType.Ceiling,
                ceilingAttic,
                sourceRef));

            // Árkád vagy áthajtó feletti födém
            double ceilingArcade = GetParamValue("ThermalBridge.Ceiling.Arcade.Default", 0.10);
            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.Ceiling.Arcade.Default",
                $"Árkád feletti födém - általános ({(ceilingArcade * 100):F0}%)",
                ConstructionType.Ceiling,
                ceilingArcade,
                sourceRef));

            // Pincefödém
            double ceilingBasementInternal = GetParamValue("ThermalBridge.Ceiling.Basement.InternalInsulation", 0.20);
            double ceilingBasementExternal = GetParamValue("ThermalBridge.Ceiling.Basement.ExternalInsulation", 0.10);

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.Ceiling.Basement.InternalInsulation",
                $"Pincefödém - belső hőszig. ({(ceilingBasementInternal * 100):F0}%)",
                ConstructionType.Ceiling,
                ceilingBasementInternal,
                sourceRef));

            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.Ceiling.Basement.ExternalInsulation",
                $"Pincefödém - alsó hőszig. ({(ceilingBasementExternal * 100):F0}%)",
                ConstructionType.Ceiling,
                ceilingBasementExternal,
                sourceRef));

            // Fűtött-fűtetlen tér közötti fal
            double wallHeatedUnheated = GetParamValue("ThermalBridge.Wall.BetweenHeatedUnheated.Default", 0.05);
            options.Add(new ThermalBridgeCorrectionOption(
                "ThermalBridge.Wall.BetweenHeatedUnheated.Default",
                $"Fűtött-fűtetlen tér közötti fal ({(wallHeatedUnheated * 100):F0}%)",
                ConstructionType.ExternalWall,
                wallHeatedUnheated,
                sourceRef));

            return options;
        }
    }
}
