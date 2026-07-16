using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates;
using HVACDesigner.EngineeringData.BuildingThermal.Materials;
using HVACDesigner.EngineeringData.BuildingThermal.AirLayers;
using HVACDesigner.EngineeringData.BuildingThermal.Openings;

namespace HVACDesigner.Calculations.Thermal.Common
{
    /// <summary>
    /// A termikus számítások (U-érték, fűtési hőszükséglet, hűtési hőterhelés)
    /// közös adatforrása. Összefogja az összes szükséges katalógust és
    /// szabálykészletet; a kalkulátorok ebből olvasnak.
    ///
    /// Példányosítás az EngineeringDataBootstrapService eredményéből történik.
    /// </summary>
    public sealed class ThermalCalculationContext
    {
        /// <summary>Anyagkatalógus (catalog-materials.xml)</summary>
        public BuildingMaterialCatalog Materials { get; }

        /// <summary>Légréteg katalógus (catalog-air-layers.xml)</summary>
        public AirLayerCatalog AirLayers { get; }

        /// <summary>Nyílászáró katalógus (catalog-openings.xml)</summary>
        public BuildingOpeningCatalog Openings { get; }

        /// <summary>Felületi ellenállások és alapparaméterek (rules-building-physics.xml)</summary>
        public BuildingPhysicsRules PhysicsRules { get; }

        /// <summary>EN 12831 módszerparaméterek és hőhíd-ajánlások (rules-heating-load.xml)</summary>
        public HeatingLoadRules HeatingRules { get; }

        /// <summary>Nyári hőterhelés paraméterek (rules-cooling-load.xml)</summary>
        public CoolingLoadRules CoolingRules { get; }

        /// <summary>Tervezési klímaadatok (design-climate.xml)</summary>
        public IReadOnlyDictionary<string, DesignClimateRegion> Climate { get; }

        public ThermalCalculationContext(
            BuildingMaterialCatalog materials,
            AirLayerCatalog airLayers,
            BuildingOpeningCatalog openings,
            BuildingPhysicsRules physicsRules,
            HeatingLoadRules heatingRules,
            CoolingLoadRules coolingRules,
            IDictionary<string, DesignClimateRegion> climate)
        {
            Materials = materials
                ?? throw new ArgumentNullException(nameof(materials));
            AirLayers = airLayers
                ?? throw new ArgumentNullException(nameof(airLayers));
            Openings = openings
                ?? throw new ArgumentNullException(nameof(openings));
            PhysicsRules = physicsRules
                ?? throw new ArgumentNullException(nameof(physicsRules));
            HeatingRules = heatingRules
                ?? throw new ArgumentNullException(nameof(heatingRules));
            CoolingRules = coolingRules
                ?? throw new ArgumentNullException(nameof(coolingRules));
            Climate = new ReadOnlyDictionary<string, DesignClimateRegion>(
                climate
                ?? throw new ArgumentNullException(nameof(climate)));
        }

        /// <summary>
        /// Visszaadja a klímarégió adatait, vagy kivételt dob ha nem található.
        /// </summary>
        public DesignClimateRegion RequireClimateRegion(string regionId)
        {
            if (string.IsNullOrWhiteSpace(regionId))
                throw new ArgumentException(
                    "A klímarégió azonosító nem lehet üres.",
                    nameof(regionId));

            if (!Climate.TryGetValue(regionId, out DesignClimateRegion region))
                throw new InvalidOperationException(
                    $"A(z) '{regionId}' klímarégió nem található a katalógusban.");

            return region;
        }
    }

    /// <summary>
    /// Egy klímarégió tervezési adatai (design-climate.xml beolvasott értékek).
    /// </summary>
    public sealed class DesignClimateRegion
    {
        public string Id { get; }
        public string DisplayName { get; }

        // Téli méretezés
        public double HeatingOutdoorTemperatureC { get; }

        // Nyári csúcsterhelés (egyszerűsített módszer)
        public double CoolingOutdoorDryBulbC { get; }
        public double CoolingOutdoorWetBulbC { get; }
        public double DailyTemperatureRange { get; }

        // Napbesugárzás [W/m²] csúcsnapon – tájolás szerint
        public double SolarSouthWm2 { get; }
        public double SolarEastWm2 { get; }
        public double SolarWestWm2 { get; }
        public double SolarNorthWm2 { get; }
        public double SolarHorizontalWm2 { get; }

        // Órasoros klímafájl hivatkozó azonosítója (ISO 52016-1 hookhoz)
        public string HourlyClimateSeriesId { get; }

        public DesignClimateRegion(
            string id,
            string displayName,
            double heatingOutdoorTemperatureC,
            double coolingOutdoorDryBulbC,
            double coolingOutdoorWetBulbC,
            double dailyTemperatureRange,
            double solarSouthWm2,
            double solarEastWm2,
            double solarWestWm2,
            double solarNorthWm2,
            double solarHorizontalWm2,
            string hourlyClimateSeriesId = "")
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Az Id nem lehet üres.", nameof(id));

            Id = id.Trim();
            DisplayName = displayName?.Trim() ?? id;
            HeatingOutdoorTemperatureC = heatingOutdoorTemperatureC;
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

        /// <summary>
        /// Visszaadja a napbesugárzást egy adott tájoláshoz.
        /// </summary>
        public double GetSolarIrradiance(CardinalDirection orientation)
        {
            switch (orientation)
            {
                case CardinalDirection.South:      return SolarSouthWm2;
                case CardinalDirection.East:       return SolarEastWm2;
                case CardinalDirection.West:       return SolarWestWm2;
                case CardinalDirection.North:      return SolarNorthWm2;
                case CardinalDirection.Horizontal: return SolarHorizontalWm2;
                default:
                    return (SolarSouthWm2 + SolarEastWm2 +
                            SolarWestWm2 + SolarNorthWm2) / 4.0;
            }
        }
    }

    /// <summary>
    /// Égtájak a napbesugárzás meghatározásához.
    /// </summary>
    public enum CardinalDirection
    {
        South,
        East,
        West,
        North,
        Horizontal,
        Unspecified
    }
}
