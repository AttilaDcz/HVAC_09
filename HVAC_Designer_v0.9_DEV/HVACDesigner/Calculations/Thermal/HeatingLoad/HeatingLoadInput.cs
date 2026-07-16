using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates;
using HVACDesigner.Calculations.Thermal.Common;
using HVACDesigner.Calculations.Thermal.UValue;

namespace HVACDesigner.Calculations.Thermal.HeatingLoad
{
    // ═══════════════════════════════════════════════════════════════
    // BEMENETI MODELLEK
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Egy határoló szerkezet (fal, tető, padló, nyílászáró) bemenete
    /// a fűtési hőterhelés számításhoz.
    /// </summary>
    public sealed class BuildingElementHeatInput
    {
        /// <summary>Szerkezet típusa (pl. ExternalWall, Roof, stb.)</summary>
        public ConstructionType ConstructionType { get; }

        /// <summary>Bruttó felület (nyílászárókkal együtt) vagy nettó felület [m²]</summary>
        public double AreaM2 { get; }

        /// <summary>U-érték [W/(m²K)] – az UValueCalculator eredményéből vehető</summary>
        public double UValue { get; }

        /// <summary>Határfeltétel (Outdoor, Ground, AdjacentUnconditioned, stb.)</summary>
        public ConstructionBoundaryCondition BoundaryCondition { get; }

        /// <summary>Opcionális leíró azonosító (pl. "ÉszakiFal", "Tető")</summary>
        public string ElementId { get; }

        public BuildingElementHeatInput(
            ConstructionType constructionType,
            double areaM2,
            double uValue,
            ConstructionBoundaryCondition boundaryCondition,
            string elementId = "")
        {
            if (areaM2 <= 0.0 || double.IsNaN(areaM2) || double.IsInfinity(areaM2))
                throw new ArgumentOutOfRangeException(
                    nameof(areaM2), "A felületek területe pozitív kell legyen.");

            if (uValue <= 0.0 || double.IsNaN(uValue) || double.IsInfinity(uValue))
                throw new ArgumentOutOfRangeException(
                    nameof(uValue), "Az U-érték pozitív kell legyen.");

            ConstructionType = constructionType;
            AreaM2 = areaM2;
            UValue = uValue;
            BoundaryCondition =
                boundaryCondition
                ?? throw new ArgumentNullException(nameof(boundaryCondition));
            ElementId = elementId?.Trim() ?? string.Empty;
        }
    }

    /// <summary>
    /// Egy helyiség / zóna fűtési hőterhelés számítási bemenete.
    /// EN 12831-1:2017 szerinti helyiségszintű számítás.
    /// </summary>
    public sealed class RoomHeatingInput
    {
        public string RoomId { get; }
        public string DisplayName { get; }

        /// <summary>Belső tervezési hőmérséklet [°C]</summary>
        public double IndoorTemperatureC { get; }

        /// <summary>Külső tervezési hőmérséklet [°C] (klímarégióból)</summary>
        public double OutdoorTemperatureC { get; }

        /// <summary>Alapterület [m²]</summary>
        public double FloorAreaM2 { get; }

        /// <summary>Belső légmagasság [m]</summary>
        public double HeightM { get; }

        /// <summary>Határoló szerkezetek listája</summary>
        public IReadOnlyList<BuildingElementHeatInput> Elements { get; }

        /// <summary>Szellőzési légcsere-szám [h⁻¹] (mechanikus vagy természetes)</summary>
        public double VentilationAch { get; }

        /// <summary>Infiltrációs légcsere-szám [h⁻¹]</summary>
        public double InfiltrationAch { get; }

        /// <summary>Felfűtési pótlék tényező [-] (1.0 = nincs pótlék)</summary>
        public double ReheatFactor { get; }

        /// <summary>Hőhíd-pótlék tényező [-] (pl. 0.10 = 10%)</summary>
        public double ThermalBridgeAllowance { get; }

        public RoomHeatingInput(
            string roomId,
            string displayName,
            double indoorTemperatureC,
            double outdoorTemperatureC,
            double floorAreaM2,
            double heightM,
            IEnumerable<BuildingElementHeatInput> elements,
            double ventilationAch = 0.5,
            double infiltrationAch = 0.05,
            double reheatFactor = 1.0,
            double thermalBridgeAllowance = 0.10)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                throw new ArgumentException(
                    "A RoomId nem lehet üres.", nameof(roomId));

            if (floorAreaM2 <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(floorAreaM2));
            if (heightM <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(heightM));

            RoomId = roomId.Trim();
            DisplayName = displayName?.Trim() ?? roomId;
            IndoorTemperatureC = indoorTemperatureC;
            OutdoorTemperatureC = outdoorTemperatureC;
            FloorAreaM2 = floorAreaM2;
            HeightM = heightM;
            Elements = new ReadOnlyCollection<BuildingElementHeatInput>(
                new List<BuildingElementHeatInput>(
                    elements
                    ?? throw new ArgumentNullException(nameof(elements))));
            VentilationAch = Math.Max(0.0, ventilationAch);
            InfiltrationAch = Math.Max(0.0, infiltrationAch);
            ReheatFactor = Math.Max(1.0, reheatFactor);
            ThermalBridgeAllowance = Math.Max(0.0, thermalBridgeAllowance);
        }

        /// <summary>Légtérfogat [m³]</summary>
        public double AirVolumeM3 => FloorAreaM2 * HeightM;

        /// <summary>Hőmérsékleti különbség [K]</summary>
        public double TemperatureDifference =>
            IndoorTemperatureC - OutdoorTemperatureC;
    }

    // ═══════════════════════════════════════════════════════════════
    // EREDMÉNY MODELLEK
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Egy határoló szerkezet transzmissziós hőveszteségének részletezése.
    /// </summary>
    public sealed class ElementHeatLossDetail
    {
        public string ElementId { get; }
        public ConstructionType ConstructionType { get; }
        public double AreaM2 { get; }
        public double UValue { get; }
        public double EffectiveTemperatureDifference { get; }
        public double HeatLossW { get; }

        public ElementHeatLossDetail(
            string elementId,
            ConstructionType constructionType,
            double areaM2,
            double uValue,
            double effectiveTemperatureDifference,
            double heatLossW)
        {
            ElementId = elementId;
            ConstructionType = constructionType;
            AreaM2 = areaM2;
            UValue = uValue;
            EffectiveTemperatureDifference = effectiveTemperatureDifference;
            HeatLossW = heatLossW;
        }
    }

    /// <summary>
    /// Egy helyiség EN 12831-1:2017 szerinti fűtési hőterhelés-számítás eredménye.
    /// </summary>
    public sealed class RoomHeatingResult
    {
        public string RoomId { get; }
        public string DisplayName { get; }

        /// <summary>Transzmissziós hőveszteség Φ_T [W]</summary>
        public double TransmissionHeatLossW { get; }

        /// <summary>Szellőzési hőveszteség Φ_V [W]</summary>
        public double VentilationHeatLossW { get; }

        /// <summary>Alap fűtési hőszükséglet (hőhíd nélkül) [W]</summary>
        public double BaseHeatLoadW { get; }

        /// <summary>Hőhíd-pótlék [W]</summary>
        public double ThermalBridgeCorrectionW { get; }

        /// <summary>Tervezési fűtési hőszükséglet (hőhíddal) Φ_HL [W]</summary>
        public double DesignHeatLoadW { get; }

        /// <summary>Felfűtési pótlék Φ_RH [W] (0 ha nincs pótlék)</summary>
        public double ReheatLoadW { get; }

        /// <summary>Teljes tervezési hőterhelés felfűtési pótlékkal [W]</summary>
        public double TotalDesignHeatLoadW { get; }

        /// <summary>Alkalmazott hőhíd-pótlék arány [-]</summary>
        public double AppliedThermalBridgeAllowance { get; }

        /// <summary>Elemenkénti veszteségek bontása</summary>
        public IReadOnlyList<ElementHeatLossDetail> ElementBreakdown { get; }

        /// <summary>
        /// Hőhíd-figyelmeztetések listája (ha egyes szerkezetek meghaladják
        /// a WarningThresholdU értéket)
        /// </summary>
        public IReadOnlyList<string> ThermalBridgeWarnings { get; }

        public RoomHeatingResult(
            string roomId,
            string displayName,
            double transmissionHeatLossW,
            double ventilationHeatLossW,
            double thermalBridgeCorrectionW,
            double reheatLoadW,
            double appliedThermalBridgeAllowance,
            IReadOnlyList<ElementHeatLossDetail> elementBreakdown,
            IReadOnlyList<string> thermalBridgeWarnings)
        {
            RoomId = roomId;
            DisplayName = displayName;
            TransmissionHeatLossW = transmissionHeatLossW;
            VentilationHeatLossW = ventilationHeatLossW;
            BaseHeatLoadW = transmissionHeatLossW + ventilationHeatLossW;
            ThermalBridgeCorrectionW = thermalBridgeCorrectionW;
            DesignHeatLoadW = BaseHeatLoadW + thermalBridgeCorrectionW;
            ReheatLoadW = reheatLoadW;
            TotalDesignHeatLoadW = DesignHeatLoadW + reheatLoadW;
            AppliedThermalBridgeAllowance = appliedThermalBridgeAllowance;
            ElementBreakdown = elementBreakdown;
            ThermalBridgeWarnings = thermalBridgeWarnings;
        }

        public bool HasThermalBridgeWarnings =>
            ThermalBridgeWarnings?.Count > 0;
    }

    /// <summary>
    /// Épületszintű fűtési hőterhelés-összesítő.
    /// </summary>
    public sealed class BuildingHeatingResult
    {
        public IReadOnlyList<RoomHeatingResult> Rooms { get; }
        public double TotalDesignHeatLoadW { get; }
        public string ClimateRegionId { get; }

        public BuildingHeatingResult(
            IReadOnlyList<RoomHeatingResult> rooms,
            string climateRegionId)
        {
            Rooms = rooms
                ?? throw new ArgumentNullException(nameof(rooms));
            ClimateRegionId = climateRegionId?.Trim() ?? string.Empty;

            double total = 0.0;
            foreach (var room in rooms)
                total += room.TotalDesignHeatLoadW;
            TotalDesignHeatLoadW = total;
        }

        public double TotalDesignHeatLoadKw =>
            TotalDesignHeatLoadW / 1000.0;
    }
}
