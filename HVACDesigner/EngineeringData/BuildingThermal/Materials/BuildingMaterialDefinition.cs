using System;
using System.Collections.Generic;
using HVACDesigner.EngineeringData.Common;

namespace HVACDesigner.EngineeringData.BuildingThermal.Materials
{
    public enum BuildingMaterialValueSource
    {
        Unspecified,
        StandardTable,
        ManufacturerData,
        Measured,
        Estimated,
        UserDefined
    }

    public enum BuildingMaterialMoistureCondition
    {
        Unspecified,
        Dry,
        Normal,
        Moist,
        Wet
    }

    /// <summary>
    /// Homogén építőanyag technológiafüggetlen, változtathatatlan definíciója.
    ///
    /// A hővezetési tényezők W/(m*K), a sűrűség kg/m3,
    /// a fajhő J/(kg*K) SI-egységben tárolódik.
    /// </summary>
    public sealed class BuildingMaterialDefinition
    {
        public EngineeringDataHeader Header { get; }

        public string Id => Header.Id;
        public string Name => Header.Name;
        public string Category => Header.Category;

        /// <summary>
        /// Deklarált vagy alap hővezetési tényező [W/(m*K)].
        /// </summary>
        public double DeclaredThermalConductivity { get; }

        /// <summary>
        /// A jelenlegi katalógus LambdaCorrection szorzója.
        /// </summary>
        public double ThermalConductivityCorrectionFactor { get; }

        /// <summary>
        /// Tervezési hővezetési tényező [W/(m*K)].
        /// </summary>
        public double DesignThermalConductivity { get; }

        public double Density { get; }                    // [kg/m3]
        public double SpecificHeatCapacity { get; }       // [J/(kg*K)]
        public double VaporResistanceFactor { get; }      // [-]

        public BuildingMaterialValueSource ValueSource { get; }
        public BuildingMaterialMoistureCondition MoistureCondition { get; }

        public string StandardReference => Header.StandardReference;
        public string Description { get; }

        public string SourcePackageId => Header.SourcePackageId;
        public string SourceContentSetId => Header.SourceContentSetId;
        public string SourceVersion => Header.SourceVersion;

        public IReadOnlyDictionary<string, string> Metadata =>
            Header.Metadata;

        /// <summary>
        /// Térfogati hőkapacitás [J/(m3*K)].
        /// A későbbi órás/dinamikus terhelésszámítás közvetlenül használhatja.
        /// </summary>
        public double VolumetricHeatCapacity =>
            Density * SpecificHeatCapacity;

        public BuildingMaterialDefinition(
            string id,
            string name,
            string category,
            double declaredThermalConductivity,
            double thermalConductivityCorrectionFactor,
            double density,
            double specificHeatCapacity,
            double vaporResistanceFactor,
            BuildingMaterialValueSource valueSource,
            BuildingMaterialMoistureCondition moistureCondition,
            string standardReference,
            string description,
            string sourcePackageId,
            string sourceContentSetId,
            string sourceVersion,
            IDictionary<string, string>? metadata = null)
            : this(
                new EngineeringDataHeader(
                    id,
                    name,
                    category: RequireText(category, nameof(category)),
                    sourcePackageId: RequireText(
                        sourcePackageId,
                        nameof(sourcePackageId)),
                    sourceContentSetId: RequireText(
                        sourceContentSetId,
                        nameof(sourceContentSetId)),
                    sourceVersion: RequireText(
                        sourceVersion,
                        nameof(sourceVersion)),
                    standardReference: standardReference,
                    metadata: metadata),
                declaredThermalConductivity,
                thermalConductivityCorrectionFactor,
                density,
                specificHeatCapacity,
                vaporResistanceFactor,
                valueSource,
                moistureCondition,
                description)
        {
        }

        public BuildingMaterialDefinition(
            EngineeringDataHeader header,
            double declaredThermalConductivity,
            double thermalConductivityCorrectionFactor,
            double density,
            double specificHeatCapacity,
            double vaporResistanceFactor,
            BuildingMaterialValueSource valueSource,
            BuildingMaterialMoistureCondition moistureCondition,
            string description)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));

            EnsurePositiveFinite(
                declaredThermalConductivity,
                nameof(declaredThermalConductivity));

            EnsurePositiveFinite(
                thermalConductivityCorrectionFactor,
                nameof(thermalConductivityCorrectionFactor));

            EnsurePositiveFinite(density, nameof(density));
            EnsurePositiveFinite(
                specificHeatCapacity,
                nameof(specificHeatCapacity));

            EnsurePositiveFinite(
                vaporResistanceFactor,
                nameof(vaporResistanceFactor));

            DeclaredThermalConductivity =
                declaredThermalConductivity;

            ThermalConductivityCorrectionFactor =
                thermalConductivityCorrectionFactor;

            DesignThermalConductivity =
                declaredThermalConductivity *
                thermalConductivityCorrectionFactor;

            Density = density;
            SpecificHeatCapacity = specificHeatCapacity;
            VaporResistanceFactor = vaporResistanceFactor;
            ValueSource = valueSource;
            MoistureCondition = moistureCondition;
            Description =
                description?.Trim() ?? string.Empty;
        }

        private static void EnsurePositiveFinite(
            double value,
            string parameterName)
        {
            if (value <= 0.0 ||
                double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Az értéknek pozitív és véges számnak kell lennie.");
            }
        }

        private static string RequireText(
            string? value,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Az érték nem lehet üres.",
                    parameterName);
            }

            return value.Trim();
        }
    }
}
