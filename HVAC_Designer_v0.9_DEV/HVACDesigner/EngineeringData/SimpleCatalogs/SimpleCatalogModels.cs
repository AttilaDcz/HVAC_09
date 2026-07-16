using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.SimpleCatalogs
{
    public sealed class EngineeringDataHeaderInfo
    {
        public string Id { get; }
        public string Version { get; }
        public string SchemaVersion { get; }
        public string DataType { get; }
        public string Country { get; }
        public string DisplayName { get; }

        public EngineeringDataHeaderInfo(
            string id,
            string version,
            string schemaVersion,
            string dataType,
            string country,
            string displayName)
        {
            Id = Require(id, nameof(id));
            Version = Require(version, nameof(version));
            SchemaVersion = Require(schemaVersion, nameof(schemaVersion));
            DataType = Require(dataType, nameof(dataType));
            Country = country?.Trim() ?? string.Empty;
            DisplayName = displayName?.Trim() ?? string.Empty;
        }

        private static string Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Az érték nem lehet üres.", name);

            return value.Trim();
        }
    }

    public sealed class FixtureDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public double? PotableLoadingUnit { get; }
        public double? WastewaterDu { get; }
        public int? MinimumWasteDn { get; }
        public bool HotWaterRelevant { get; }
        public string GreywaterSource { get; }
        public bool GreywaterDemand { get; }

        public FixtureDefinition(
            string id,
            string displayName,
            string category,
            double? potableLoadingUnit,
            double? wastewaterDu,
            int? minimumWasteDn,
            bool hotWaterRelevant,
            string greywaterSource,
            bool greywaterDemand)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            PotableLoadingUnit = potableLoadingUnit;
            WastewaterDu = wastewaterDu;
            MinimumWasteDn = minimumWasteDn;
            HotWaterRelevant = hotWaterRelevant;
            GreywaterSource = greywaterSource;
            GreywaterDemand = greywaterDemand;
        }
    }

    public sealed class MaterialDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public double Lambda { get; }
        public double? Density { get; }
        public string Category { get; }
        public double? Mu { get; }
        public double? SpecificHeat { get; }
        public double? LambdaCorrection { get; }

        public MaterialDefinition(
            string id,
            string displayName,
            double lambda,
            double? density,
            string category = null,
            double? mu = null,
            double? specificHeat = null,
            double? lambdaCorrection = null)
        {
            Id = id;
            DisplayName = displayName;
            Lambda = lambda;
            Density = density;
            Category = category ?? string.Empty;
            Mu = mu;
            SpecificHeat = specificHeat;
            LambdaCorrection = lambdaCorrection;
        }
    }

    public sealed class OpeningDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Type { get; }
        public double UValue { get; }
        public double? GValue { get; }

        public OpeningDefinition(
            string id,
            string displayName,
            string type,
            double uValue,
            double? gValue)
        {
            Id = id;
            DisplayName = displayName;
            Type = type;
            UValue = uValue;
            GValue = gValue;
        }
    }

    public sealed class BuildingFunctionDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Category { get; }

        public BuildingFunctionDefinition(
            string id,
            string displayName,
            string category)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
        }
    }

    public sealed class BuildingProfileValue
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Value { get; }
        public string Unit { get; }
        public string Source { get; }
        public string RequirementLevel { get; }

        public BuildingProfileValue(
            string id,
            string displayName,
            string value,
            string unit,
            string source,
            string requirementLevel)
        {
            Id = id;
            DisplayName = displayName;
            Value = value;
            Unit = unit;
            Source = source;
            RequirementLevel = requirementLevel;
        }
    }

    public sealed class BuildingProfileSection
    {
        public string Name { get; }
        public IReadOnlyList<BuildingProfileValue> Values { get; }

        public BuildingProfileSection(
            string name,
            IEnumerable<BuildingProfileValue> values)
        {
            Name = name;
            Values = new ReadOnlyCollection<BuildingProfileValue>(
                (values ?? Enumerable.Empty<BuildingProfileValue>()).ToList());
        }
    }

    public sealed class BuildingProfileDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<BuildingProfileSection> Sections { get; }

        public BuildingProfileDefinition(
            string id,
            string displayName,
            IEnumerable<BuildingProfileSection> sections)
        {
            Id = id;
            DisplayName = displayName;
            Sections = new ReadOnlyCollection<BuildingProfileSection>(
                (sections ?? Enumerable.Empty<BuildingProfileSection>()).ToList());
        }
    }

    public sealed class BuildingFunctionMapping
    {
        public string FunctionId { get; }
        public string ProfileId { get; }

        public BuildingFunctionMapping(string functionId, string profileId)
        {
            FunctionId = functionId;
            ProfileId = profileId;
        }
    }

    public sealed class ClimateRegionDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public double HeatingOutdoorTemperatureCelsius { get; }
        public string Notes { get; }

        // ─── Nyári adatok (design-climate.xml v2.0) ───────────────────────
        public double? CoolingOutdoorDryBulbC { get; }
        public double? CoolingOutdoorWetBulbC { get; }
        public double? DailyTemperatureRange { get; }
        public double? SolarSouthWm2 { get; }
        public double? SolarEastWm2 { get; }
        public double? SolarWestWm2 { get; }
        public double? SolarNorthWm2 { get; }
        public double? SolarHorizontalWm2 { get; }
        public string HourlyClimateSeriesId { get; }

        public ClimateRegionDefinition(
            string id,
            string displayName,
            double heatingOutdoorTemperatureCelsius,
            string notes,
            double? coolingOutdoorDryBulbC = null,
            double? coolingOutdoorWetBulbC = null,
            double? dailyTemperatureRange = null,
            double? solarSouthWm2 = null,
            double? solarEastWm2 = null,
            double? solarWestWm2 = null,
            double? solarNorthWm2 = null,
            double? solarHorizontalWm2 = null,
            string hourlyClimateSeriesId = "")
        {
            Id = id;
            DisplayName = displayName;
            HeatingOutdoorTemperatureCelsius = heatingOutdoorTemperatureCelsius;
            Notes = notes;
            CoolingOutdoorDryBulbC = coolingOutdoorDryBulbC;
            CoolingOutdoorWetBulbC = coolingOutdoorWetBulbC;
            DailyTemperatureRange = dailyTemperatureRange;
            SolarSouthWm2 = solarSouthWm2;
            SolarEastWm2 = solarEastWm2;
            SolarWestWm2 = solarWestWm2;
            SolarNorthWm2 = solarNorthWm2;
            SolarHorizontalWm2 = solarHorizontalWm2;
            HourlyClimateSeriesId = hourlyClimateSeriesId?.Trim() ?? string.Empty;
        }
    }

    public sealed class EngineeringDictionaryEntry
    {
        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<string> Aliases { get; }

        public EngineeringDictionaryEntry(
            string id,
            string displayName,
            IEnumerable<string> aliases)
        {
            Id = id;
            DisplayName = displayName;
            Aliases = new ReadOnlyCollection<string>(
                (aliases ?? Enumerable.Empty<string>()).ToList());
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // ÚJ TERMIKUS KATALÓGUS MODELLEK
    // Forrás: catalog-air-layers.xml, catalog-construction-templates.xml,
    //         rules-building-physics.xml, rules-heating-load.xml,
    //         rules-cooling-load.xml
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Légréteg bejegyzés (catalog-air-layers.xml).
    /// ISO 6946:2017 Annex B alapján.
    /// </summary>
    public sealed class AirLayerSimpleDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Orientation { get; }
        public string HeatFlowDirection { get; }
        public string VentilationLevel { get; }
        public string EmissivityClass { get; }
        public double ThermalResistance { get; }  // [m²K/W]
        public string Source { get; }
        public string Notes { get; }

        public AirLayerSimpleDefinition(
            string id,
            string displayName,
            string orientation,
            string heatFlowDirection,
            string ventilationLevel,
            string emissivityClass,
            double thermalResistance,
            string source,
            string notes)
        {
            Id = id;
            DisplayName = displayName;
            Orientation = orientation;
            HeatFlowDirection = heatFlowDirection;
            VentilationLevel = ventilationLevel;
            EmissivityClass = emissivityClass;
            ThermalResistance = thermalResistance;
            Source = source;
            Notes = notes;
        }
    }

    /// <summary>
    /// Egy réteg bejegyzés egy szerkezetrétegrend-sablonban.
    /// </summary>
    public sealed class ConstructionLayerSimpleDefinition
    {
        public int Order { get; }
        public string MaterialId { get; }
        public string AirLayerId { get; }
        public double? ThicknessM { get; }
        public string Description { get; }

        public ConstructionLayerSimpleDefinition(
            int order,
            string materialId,
            string airLayerId,
            double? thicknessM,
            string description)
        {
            Order = order;
            MaterialId = materialId?.Trim() ?? string.Empty;
            AirLayerId = airLayerId?.Trim() ?? string.Empty;
            ThicknessM = thicknessM;
            Description = description?.Trim() ?? string.Empty;
        }
    }

    /// <summary>
    /// Épületszerkezeti rétegrendsablon (catalog-construction-templates.xml).
    /// </summary>
    public sealed class ConstructionTemplateDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Type { get; }
        public string DataStatus { get; }
        public IReadOnlyList<ConstructionLayerSimpleDefinition> Layers { get; }

        public ConstructionTemplateDefinition(
            string id,
            string displayName,
            string type,
            string dataStatus,
            IEnumerable<ConstructionLayerSimpleDefinition> layers)
        {
            Id = id;
            DisplayName = displayName;
            Type = type;
            DataStatus = dataStatus;
            Layers = new ReadOnlyCollection<ConstructionLayerSimpleDefinition>(
                (layers ?? Enumerable.Empty<ConstructionLayerSimpleDefinition>())
                .ToList());
        }
    }

    /// <summary>
    /// Épületfizikai felületi hőellenállások és hőhíd-alap paraméter
    /// (rules-building-physics.xml).
    /// </summary>
    public sealed class BuildingPhysicsRulesDefinition
    {
        public string Id { get; }
        public double RsiHorizontal { get; }
        public double RseHorizontal { get; }
        public double RsiUpward { get; }
        public double RseUpward { get; }
        public double RsiDownward { get; }
        public double RseDownward { get; }
        public double DefaultThermalBridgeAllowance { get; }

        public BuildingPhysicsRulesDefinition(
            string id,
            double rsiHorizontal,
            double rseHorizontal,
            double rsiUpward,
            double rseUpward,
            double rsiDownward,
            double rseDownward,
            double defaultThermalBridgeAllowance)
        {
            Id = id;
            RsiHorizontal = rsiHorizontal;
            RseHorizontal = rseHorizontal;
            RsiUpward = rsiUpward;
            RseUpward = rseUpward;
            RsiDownward = rsiDownward;
            RseDownward = rseDownward;
            DefaultThermalBridgeAllowance = defaultThermalBridgeAllowance;
        }
    }

    /// <summary>
    /// Egy szerkezettípusra vonatkozó hőhíd-ajánlás (rules-heating-load.xml).
    /// </summary>
    public sealed class ThermalBridgeAllowanceSimpleDefinition
    {
        public string ConstructionType { get; }
        public string InsulationLevel { get; }
        public double RecommendedAllowance { get; }
        public double DefaultAllowance { get; }
        public double WarningThresholdU { get; }
        public string Note { get; }

        public ThermalBridgeAllowanceSimpleDefinition(
            string constructionType,
            string insulationLevel,
            double recommendedAllowance,
            double defaultAllowance,
            double warningThresholdU,
            string note)
        {
            ConstructionType = constructionType;
            InsulationLevel = insulationLevel;
            RecommendedAllowance = recommendedAllowance;
            DefaultAllowance = defaultAllowance;
            WarningThresholdU = warningThresholdU;
            Note = note;
        }
    }

    /// <summary>
    /// EN 12831 fűtési hőterhelés módszerparaméterek összesítője
    /// (rules-heating-load.xml).
    /// </summary>
    public sealed class HeatingLoadRulesDefinition
    {
        public string Id { get; }
        public double AirVolumetricHeatCapacity { get; }
        public double InfiltrationAchNew { get; }
        public double InfiltrationAchRecent { get; }
        public double InfiltrationAchLegacy { get; }
        public double ReheatFactorResidential8h { get; }
        public double ReheatFactorOffice12h { get; }
        public double ReheatFactorSchoolWeekend { get; }
        public IReadOnlyList<ThermalBridgeAllowanceSimpleDefinition> ThermalBridgeAllowances { get; }

        public HeatingLoadRulesDefinition(
            string id,
            double airVolumetricHeatCapacity,
            double infiltrationAchNew,
            double infiltrationAchRecent,
            double infiltrationAchLegacy,
            double reheatFactorResidential8h,
            double reheatFactorOffice12h,
            double reheatFactorSchoolWeekend,
            IEnumerable<ThermalBridgeAllowanceSimpleDefinition> thermalBridgeAllowances)
        {
            Id = id;
            AirVolumetricHeatCapacity = airVolumetricHeatCapacity;
            InfiltrationAchNew = infiltrationAchNew;
            InfiltrationAchRecent = infiltrationAchRecent;
            InfiltrationAchLegacy = infiltrationAchLegacy;
            ReheatFactorResidential8h = reheatFactorResidential8h;
            ReheatFactorOffice12h = reheatFactorOffice12h;
            ReheatFactorSchoolWeekend = reheatFactorSchoolWeekend;
            ThermalBridgeAllowances =
                new ReadOnlyCollection<ThermalBridgeAllowanceSimpleDefinition>(
                    (thermalBridgeAllowances
                     ?? Enumerable.Empty<ThermalBridgeAllowanceSimpleDefinition>())
                    .ToList());
        }
    }

    /// <summary>
    /// Nyári hűtési hőterhelés módszerparaméterek összesítője
    /// (rules-cooling-load.xml).
    /// </summary>
    public sealed class CoolingLoadRulesDefinition
    {
        public string Id { get; }
        public double AirVolumetricHeatCapacity { get; }
        public double ShadingFactorNone { get; }
        public double ShadingFactorInternalLightBlind { get; }
        public double ShadingFactorInternalDarkBlind { get; }
        public double ShadingFactorExternalLightBlind { get; }
        public double ShadingFactorExternalDarkBlind { get; }
        public double ShadingFactorExternalOverhang { get; }
        public double ConcurrencyFactorResidential { get; }
        public double ConcurrencyFactorOffice { get; }
        public double ConcurrencyFactorSchool { get; }
        public double ConcurrencyFactorRetail { get; }
        public double SensibleHeatPerPersonSedentary { get; }
        public double LatentHeatPerPersonSedentary { get; }
        public double LightingPowerDensityOffice { get; }
        public double LightingPowerDensityResidential { get; }
        public double EquipmentPowerDensityOffice { get; }
        public double EquipmentPowerDensityResidential { get; }

        public CoolingLoadRulesDefinition(
            string id,
            double airVolumetricHeatCapacity,
            double shadingFactorNone,
            double shadingFactorInternalLightBlind,
            double shadingFactorInternalDarkBlind,
            double shadingFactorExternalLightBlind,
            double shadingFactorExternalDarkBlind,
            double shadingFactorExternalOverhang,
            double concurrencyFactorResidential,
            double concurrencyFactorOffice,
            double concurrencyFactorSchool,
            double concurrencyFactorRetail,
            double sensibleHeatPerPersonSedentary,
            double latentHeatPerPersonSedentary,
            double lightingPowerDensityOffice,
            double lightingPowerDensityResidential,
            double equipmentPowerDensityOffice,
            double equipmentPowerDensityResidential)
        {
            Id = id;
            AirVolumetricHeatCapacity = airVolumetricHeatCapacity;
            ShadingFactorNone = shadingFactorNone;
            ShadingFactorInternalLightBlind = shadingFactorInternalLightBlind;
            ShadingFactorInternalDarkBlind = shadingFactorInternalDarkBlind;
            ShadingFactorExternalLightBlind = shadingFactorExternalLightBlind;
            ShadingFactorExternalDarkBlind = shadingFactorExternalDarkBlind;
            ShadingFactorExternalOverhang = shadingFactorExternalOverhang;
            ConcurrencyFactorResidential = concurrencyFactorResidential;
            ConcurrencyFactorOffice = concurrencyFactorOffice;
            ConcurrencyFactorSchool = concurrencyFactorSchool;
            ConcurrencyFactorRetail = concurrencyFactorRetail;
            SensibleHeatPerPersonSedentary = sensibleHeatPerPersonSedentary;
            LatentHeatPerPersonSedentary = latentHeatPerPersonSedentary;
            LightingPowerDensityOffice = lightingPowerDensityOffice;
            LightingPowerDensityResidential = lightingPowerDensityResidential;
            EquipmentPowerDensityOffice = equipmentPowerDensityOffice;
            EquipmentPowerDensityResidential = equipmentPowerDensityResidential;
        }
    }

    public sealed class SimpleCatalog<T>
    {
        private readonly ReadOnlyDictionary<string, T> _items;

        public EngineeringDataHeaderInfo Header { get; }
        public IReadOnlyDictionary<string, T> Items => _items;

        public SimpleCatalog(
            EngineeringDataHeaderInfo header,
            IEnumerable<T> items,
            Func<T, string> idSelector)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (idSelector == null) throw new ArgumentNullException(nameof(idSelector));

            var dictionary = new Dictionary<string, T>(
                StringComparer.OrdinalIgnoreCase);

            foreach (T item in items)
            {
                string id = idSelector(item);

                if (string.IsNullOrWhiteSpace(id))
                    throw new InvalidOperationException(
                        "A katalóguselem azonosítója nem lehet üres.");

                if (dictionary.ContainsKey(id))
                    throw new InvalidOperationException(
                        "Duplikált katalóguselem-azonosító: " + id + ".");

                dictionary.Add(id, item);
            }

            _items = new ReadOnlyDictionary<string, T>(dictionary);
        }

        public bool TryGet(string id, out T value)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                value = default(T);
                return false;
            }

            return _items.TryGetValue(id.Trim(), out value);
        }
    }

    public sealed class GlazingSimpleDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public double Ug { get; }
        public double SolarTransmittance { get; }
        public int? PaneCount { get; }
        public string GasFill { get; }
        public string CoatingType { get; }

        public GlazingSimpleDefinition(string id, string displayName, double ug, double solarTransmittance, int? paneCount, string gasFill, string coatingType)
        {
            Id = id;
            DisplayName = displayName;
            Ug = ug;
            SolarTransmittance = solarTransmittance;
            PaneCount = paneCount;
            GasFill = gasFill;
            CoatingType = coatingType;
        }
    }

    public sealed class FrameSimpleDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public double Uf { get; }
        public string MaterialKind { get; }
        public double? ProfileDepth { get; }
        public int? ChamberCount { get; }
        public double? DefaultWidth { get; }

        public FrameSimpleDefinition(string id, string displayName, double uf, string materialKind, double? profileDepth, int? chamberCount, double? defaultWidth)
        {
            Id = id;
            DisplayName = displayName;
            Uf = uf;
            MaterialKind = materialKind;
            ProfileDepth = profileDepth;
            ChamberCount = chamberCount;
            DefaultWidth = defaultWidth;
        }
    }

    public sealed class SpacerSimpleDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public double Psi { get; }
        public string SpacerType { get; }

        public SpacerSimpleDefinition(string id, string displayName, double psi, string spacerType)
        {
            Id = id;
            DisplayName = displayName;
            Psi = psi;
            SpacerType = spacerType;
        }
    }
}
