using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HVACDesigner.Calculations.Thermal.Common;

namespace HVACDesigner.Calculations.Thermal.CoolingLoad
{
    // ═══════════════════════════════════════════════════════════════
    // EGYSZERŰSÍTETT CSÚCSTERHELÉS MODELLEK
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Egy tömör (opaque) határoló szerkezet bemenete a
    /// hűtési hőterhelés számításhoz.
    /// </summary>
    public sealed class OpaqueCoolingElementInput
    {
        public string ElementId { get; }
        public double AreaM2 { get; }
        public double UValue { get; }
        public CardinalDirection Orientation { get; }

        public OpaqueCoolingElementInput(
            string elementId,
            double areaM2,
            double uValue,
            CardinalDirection orientation)
        {
            ElementId = elementId?.Trim() ?? string.Empty;
            AreaM2 = areaM2 > 0.0
                ? areaM2
                : throw new ArgumentOutOfRangeException(nameof(areaM2));
            UValue = uValue > 0.0
                ? uValue
                : throw new ArgumentOutOfRangeException(nameof(uValue));
            Orientation = orientation;
        }
    }

    /// <summary>
    /// Egy üveges felület (nyílászáró, tetőablak) bemenete
    /// a hűtési hőterhelés számításhoz.
    /// </summary>
    public sealed class GlazingCoolingInput
    {
        public string ElementId { get; }
        public double AreaM2 { get; }

        /// <summary>U-érték [W/(m²K)] – hőveszteség/nyereség számításhoz</summary>
        public double UValue { get; }

        /// <summary>Teljes energiaáteresztési tényező g [-] (SHGC)</summary>
        public double SolarHeatGainCoefficient { get; }

        public CardinalDirection Orientation { get; }

        /// <summary>Árnyékolás típusa (befolyásolja a g-értéket)</summary>
        public ShadingType Shading { get; }

        public GlazingCoolingInput(
            string elementId,
            double areaM2,
            double uValue,
            double solarHeatGainCoefficient,
            CardinalDirection orientation,
            ShadingType shading = ShadingType.None)
        {
            ElementId = elementId?.Trim() ?? string.Empty;
            AreaM2 = areaM2 > 0.0
                ? areaM2
                : throw new ArgumentOutOfRangeException(nameof(areaM2));
            UValue = uValue > 0.0
                ? uValue
                : throw new ArgumentOutOfRangeException(nameof(uValue));
            SolarHeatGainCoefficient = solarHeatGainCoefficient > 0.0
                ? solarHeatGainCoefficient
                : throw new ArgumentOutOfRangeException(
                    nameof(solarHeatGainCoefficient));
            Orientation = orientation;
            Shading = shading;
        }
    }

    /// <summary>
    /// Egy helyiség / zóna nyári hűtési csúcsterhelés számítási bemenete.
    /// Egyszerűsített módszer (CIBSE/WinWatt-szerű kvázi-stacionárius).
    /// </summary>
    public sealed class RoomCoolingInput
    {
        public string RoomId { get; }
        public string DisplayName { get; }
        public string ClimateRegionId { get; }

        public double IndoorTemperatureC { get; }
        public double FloorAreaM2 { get; }
        public double HeightM { get; }

        public IReadOnlyList<OpaqueCoolingElementInput> OpaqueElements { get; }
        public IReadOnlyList<GlazingCoolingInput> GlazedElements { get; }

        public int OccupancyPersons { get; }
        public double LightingW { get; }
        public double EquipmentW { get; }
        public double VentilationAch { get; }

        /// <summary>Foglaltság egyidejűségi tényező [-]</summary>
        public double OccupancyConcurrencyFactor { get; }

        public RoomCoolingInput(
            string roomId,
            string displayName,
            string climateRegionId,
            double indoorTemperatureC,
            double floorAreaM2,
            double heightM,
            IEnumerable<OpaqueCoolingElementInput> opaqueElements,
            IEnumerable<GlazingCoolingInput> glazedElements,
            int occupancyPersons,
            double lightingW,
            double equipmentW,
            double ventilationAch = 1.0,
            double occupancyConcurrencyFactor = 1.0)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                throw new ArgumentException("A RoomId nem lehet üres.", nameof(roomId));
            if (floorAreaM2 <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(floorAreaM2));
            if (heightM <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(heightM));

            RoomId = roomId.Trim();
            DisplayName = displayName?.Trim() ?? roomId;
            ClimateRegionId = climateRegionId?.Trim()
                ?? throw new ArgumentNullException(nameof(climateRegionId));
            IndoorTemperatureC = indoorTemperatureC;
            FloorAreaM2 = floorAreaM2;
            HeightM = heightM;
            OpaqueElements = new ReadOnlyCollection<OpaqueCoolingElementInput>(
                new List<OpaqueCoolingElementInput>(
                    opaqueElements ?? Array.Empty<OpaqueCoolingElementInput>()));
            GlazedElements = new ReadOnlyCollection<GlazingCoolingInput>(
                new List<GlazingCoolingInput>(
                    glazedElements ?? Array.Empty<GlazingCoolingInput>()));
            OccupancyPersons = Math.Max(0, occupancyPersons);
            LightingW = Math.Max(0.0, lightingW);
            EquipmentW = Math.Max(0.0, equipmentW);
            VentilationAch = Math.Max(0.0, ventilationAch);
            OccupancyConcurrencyFactor =
                Math.Max(0.0, Math.Min(1.0, occupancyConcurrencyFactor));
        }

        public double AirVolumeM3 => FloorAreaM2 * HeightM;
    }

    // ═══════════════════════════════════════════════════════════════
    // EREDMÉNY MODELLEK
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Egy helyiség nyári hűtési csúcsterhelés eredménye.
    /// </summary>
    public sealed class RoomCoolingResult
    {
        public string RoomId { get; }
        public string DisplayName { get; }

        /// <summary>Napsugárzáson alapuló hőnyereség [W]</summary>
        public double SolarGainW { get; }

        /// <summary>Transzmissziós hőnyereség üvegen és tömör falszerkezeteken [W]</summary>
        public double TransmissionGainW { get; }

        /// <summary>Személyek érzékelhető hőleadása [W]</summary>
        public double PeopleGainSensibleW { get; }

        /// <summary>Személyek látens hőleadása [W]</summary>
        public double PeopleGainLatentW { get; }

        /// <summary>Megvilágítás hőleadása [W]</summary>
        public double LightingGainW { get; }

        /// <summary>Berendezések hőleadása [W]</summary>
        public double EquipmentGainW { get; }

        /// <summary>Szellőzési hőnyereség [W]</summary>
        public double VentilationGainW { get; }

        /// <summary>Összes érzékelhető hőterhelés [W]</summary>
        public double TotalSensibleLoadW { get; }

        /// <summary>Látens hőterhelés [W]</summary>
        public double TotalLatentLoadW { get; }

        /// <summary>Teljes csúcsterhelés [W]</summary>
        public double TotalCoolingLoadW { get; }

        public RoomCoolingResult(
            string roomId,
            string displayName,
            double solarGainW,
            double transmissionGainW,
            double peopleSensibleW,
            double peopleLatentW,
            double lightingGainW,
            double equipmentGainW,
            double ventilationGainW)
        {
            RoomId = roomId;
            DisplayName = displayName;
            SolarGainW = solarGainW;
            TransmissionGainW = transmissionGainW;
            PeopleGainSensibleW = peopleSensibleW;
            PeopleGainLatentW = peopleLatentW;
            LightingGainW = lightingGainW;
            EquipmentGainW = equipmentGainW;
            VentilationGainW = ventilationGainW;

            TotalSensibleLoadW =
                solarGainW + transmissionGainW + peopleSensibleW
                + lightingGainW + equipmentGainW + ventilationGainW;
            TotalLatentLoadW = peopleLatentW;
            TotalCoolingLoadW = TotalSensibleLoadW + TotalLatentLoadW;
        }
    }

    /// <summary>
    /// Épületszintű hűtési csúcsterhelés összesítő.
    /// </summary>
    public sealed class BuildingCoolingResult
    {
        public IReadOnlyList<RoomCoolingResult> Rooms { get; }
        public double TotalCoolingLoadW { get; }
        public string ClimateRegionId { get; }

        public BuildingCoolingResult(
            IReadOnlyList<RoomCoolingResult> rooms,
            string climateRegionId)
        {
            Rooms = rooms ?? throw new ArgumentNullException(nameof(rooms));
            ClimateRegionId = climateRegionId?.Trim() ?? string.Empty;
            double total = 0.0;
            foreach (var r in rooms) total += r.TotalCoolingLoadW;
            TotalCoolingLoadW = total;
        }

        public double TotalCoolingLoadKw => TotalCoolingLoadW / 1000.0;
    }

    // ═══════════════════════════════════════════════════════════════
    // ISO 52016-1 HOOK – ÓRÁNKÉNTI MODELLEK
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Egy zóna ISO 52016-1:2017 szerinti RC-hálózati leírása.
    /// 5R1C modell: H_tr (külső, ablak, fal, padló, tető) + C_m (belső hőkapacitás).
    /// Az óránkénti szimulációs logika az IHourlyCoolingLoadCalculator-ban lesz.
    /// </summary>
    public sealed class ZoneRcModelInput
    {
        public string ZoneId { get; }

        /// <summary>Effektív tömegfelület A_m [m²] (ISO 52016-1 §6.6.5)</summary>
        public double EffectiveMassAreaM2 { get; }

        /// <summary>Belső hőkapacitás C_m [J/K]</summary>
        public double InternalHeatCapacityJK { get; }

        /// <summary>Korrigált hőátadási tényező H_tr,adj [W/K]</summary>
        public double HeatTransferCoefficientW_K { get; }

        /// <summary>Órasoros klímafájl hivatkozó azonosítója</summary>
        public string HourlyClimateSeriesId { get; }

        /// <summary>Órasoros belső terhelés profil hivatkozó azonosítója</summary>
        public string HourlyInternalGainProfileId { get; }

        public ZoneRcModelInput(
            string zoneId,
            double effectiveMassAreaM2,
            double internalHeatCapacityJK,
            double heatTransferCoefficientW_K,
            string hourlyClimateSeriesId = "",
            string hourlyInternalGainProfileId = "")
        {
            if (string.IsNullOrWhiteSpace(zoneId))
                throw new ArgumentException("A ZoneId nem lehet üres.", nameof(zoneId));

            ZoneId = zoneId.Trim();
            EffectiveMassAreaM2 = effectiveMassAreaM2;
            InternalHeatCapacityJK = internalHeatCapacityJK;
            HeatTransferCoefficientW_K = heatTransferCoefficientW_K;
            HourlyClimateSeriesId = hourlyClimateSeriesId?.Trim() ?? string.Empty;
            HourlyInternalGainProfileId =
                hourlyInternalGainProfileId?.Trim() ?? string.Empty;
        }
    }

    /// <summary>
    /// ISO 52016-1 §6.5.6 szerinti 5 hőtároló tömeg osztály.
    /// Az épület szerkezeteinek hőtároló képességét írja le.
    /// </summary>
    public enum ThermalMassClass
    {
        VeryLight,  // &lt; 80 kJ/(m²K)
        Light,      // 80–110 kJ/(m²K)
        Medium,     // 110–165 kJ/(m²K)
        Heavy,      // 165–260 kJ/(m²K)
        VeryHeavy   // &gt; 260 kJ/(m²K)
    }

    /// <summary>
    /// Egyetlen óra klímaadatai az ISO 52016-1 órasoros számításhoz.
    /// Első körben a design-climate.xml reprezentatív csúcsóráit tartalmazza.
    /// Teljes TMY adatsor az óránkénti számításhoz majd külön XML/CSV fájlban.
    /// </summary>
    public sealed class HourlyClimateData
    {
        /// <summary>Az év órájának sorszáma (0–8759)</summary>
        public int HourOfYear { get; }
        public double OutdoorTemperatureC { get; }
        public double SolarIrradianceSouthWm2 { get; }
        public double SolarIrradianceEastWm2 { get; }
        public double SolarIrradianceWestWm2 { get; }
        public double SolarIrradianceNorthWm2 { get; }
        public double SolarIrradianceHorizontalWm2 { get; }
        public double RelativeHumidityPercent { get; }

        public HourlyClimateData(
            int hourOfYear,
            double outdoorTemperatureC,
            double solarSouthWm2,
            double solarEastWm2,
            double solarWestWm2,
            double solarNorthWm2,
            double solarHorizontalWm2,
            double relativeHumidityPercent)
        {
            if (hourOfYear < 0 || hourOfYear > 8759)
                throw new ArgumentOutOfRangeException(
                    nameof(hourOfYear), "Az óra sorszáma 0 és 8759 közé kell essen.");

            HourOfYear = hourOfYear;
            OutdoorTemperatureC = outdoorTemperatureC;
            SolarIrradianceSouthWm2 = solarSouthWm2;
            SolarIrradianceEastWm2 = solarEastWm2;
            SolarIrradianceWestWm2 = solarWestWm2;
            SolarIrradianceNorthWm2 = solarNorthWm2;
            SolarIrradianceHorizontalWm2 = solarHorizontalWm2;
            RelativeHumidityPercent = relativeHumidityPercent;
        }
    }

    /// <summary>
    /// ISO 52016-1 óránkénti számítás eredménye.
    /// Fázis 2: adatszerkezet kész.
    /// Fázis 3+: az IHourlyCoolingLoadCalculator.Calculate() metódus tölti fel.
    /// </summary>
    public sealed class HourlyCoolingResult
    {
        public string ZoneId { get; }

        /// <summary>Óránkénti hűtési teljesítmény [W] – 8760 elem</summary>
        public IReadOnlyList<double> HourlyCoolingLoadW { get; }

        public double PeakCoolingLoadW { get; }
        public int PeakHourOfYear { get; }
        public double AnnualCoolingEnergyKwh { get; }
        public double AnnualHeatingEnergyKwh { get; }

        /// <summary>Óránkénti belső hőmérséklet [°C] – 8760 elem</summary>
        public IReadOnlyList<double> HourlyIndoorTemperatureC { get; }

        public HourlyCoolingResult(
            string zoneId,
            IReadOnlyList<double> hourlyCoolingLoadW,
            double peakCoolingLoadW,
            int peakHourOfYear,
            double annualCoolingEnergyKwh,
            double annualHeatingEnergyKwh,
            IReadOnlyList<double> hourlyIndoorTemperatureC)
        {
            ZoneId = zoneId;
            HourlyCoolingLoadW = hourlyCoolingLoadW;
            PeakCoolingLoadW = peakCoolingLoadW;
            PeakHourOfYear = peakHourOfYear;
            AnnualCoolingEnergyKwh = annualCoolingEnergyKwh;
            AnnualHeatingEnergyKwh = annualHeatingEnergyKwh;
            HourlyIndoorTemperatureC = hourlyIndoorTemperatureC;
        }
    }
}
