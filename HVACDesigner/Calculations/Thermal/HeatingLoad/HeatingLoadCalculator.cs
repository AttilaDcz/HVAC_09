using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates;
using HVACDesigner.Calculations.Thermal.Common;

namespace HVACDesigner.Calculations.Thermal.HeatingLoad
{
    /// <summary>
    /// EN 12831-1:2017 szerinti fűtési hőterhelés kalkulátor.
    ///
    /// Algoritmus:
    ///   1. Transzmissziós hőveszteség Φ_T:
    ///      Minden elem esetén: Φ_T,i = U_i · A_i · ΔT_eff
    ///      ahol ΔT_eff a határfeltétel alapján korrigált hőmérséklet-különbség.
    ///      Ha a szomszédos tér fűtetlen, a b_u tényező csökkenti a különbséget.
    ///
    ///   2. Szellőzési hőveszteség Φ_V:
    ///      Φ_V = ρcp · max(n_vent, n_infil) · V · ΔT
    ///      ahol ρcp = 0.34 Wh/(m³K), V = helyiség légtérfogata
    ///
    ///   3. Hőhíd-pótlék Φ_TB:
    ///      Φ_TB = f_TB · Φ_T
    ///      ahol f_TB az alkalmazott pótlék (alapértelmezett vagy ajánlott)
    ///
    ///   4. Felfűtési pótlék Φ_RH:
    ///      Φ_RH = (reheatFactor - 1.0) · (Φ_T + Φ_V + Φ_TB)
    ///
    ///   5. Teljes tervezési hőterhelés:
    ///      Φ_HL = Φ_T + Φ_V + Φ_TB + Φ_RH
    ///
    /// Megjegyzés: talajon fekvő padlóknál (GroundFloor) az ISO 13370
    /// szerinti pontos számítás b_u tényezőre hagyatkozik. A részletes
    /// perimeter-módszer egy következő bővítmény.
    /// </summary>
    public sealed class HeatingLoadCalculator
    {
        private readonly ThermalCalculationContext _context;

        public HeatingLoadCalculator(ThermalCalculationContext context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Egy helyiség fűtési hőterhelésének számítása.
        /// </summary>
        public RoomHeatingResult CalculateRoom(RoomHeatingInput input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            double deltaT = input.TemperatureDifference;

            // Ellenőrzés: fűtési periódusban kell az outdoor < indoor lennie
            if (deltaT <= 0.0)
            {
                throw new InvalidOperationException(
                    $"A hőmérsékleti különbség nem pozitív " +
                    $"(Ti={input.IndoorTemperatureC}°C, Te={input.OutdoorTemperatureC}°C). " +
                    "Fűtési hőterhelés számításhoz a belső hőmérsékletnek " +
                    "nagyobbnak kell lennie a külsőnél.");
            }

            // 1. Transzmissziós hőveszteség
            var elementDetails = new List<ElementHeatLossDetail>();
            var thermalBridgeWarnings = new List<string>();
            double sumTransmissionW = 0.0;

            foreach (var element in input.Elements)
            {
                double effectiveDeltaT = GetEffectiveDeltaT(
                    element.BoundaryCondition,
                    deltaT,
                    input.IndoorTemperatureC,
                    input.OutdoorTemperatureC);

                double heatLossW =
                    element.UValue * element.AreaM2 * effectiveDeltaT;

                elementDetails.Add(new ElementHeatLossDetail(
                    element.ElementId,
                    element.ConstructionType,
                    element.AreaM2,
                    element.UValue,
                    effectiveDeltaT,
                    heatLossW));

                sumTransmissionW += heatLossW;

                // Hőhíd-figyelmeztetés ellenőrzése
                var allowanceRule = _context.HeatingRules
                    .FindAllowance(element.ConstructionType, element.UValue);

                if (element.UValue > allowanceRule.WarningThresholdU)
                {
                    thermalBridgeWarnings.Add(
                        $"[{element.ElementId ?? element.ConstructionType.ToString()}] " +
                        $"U={element.UValue:F3} W/(m²K) > küszöb " +
                        $"{allowanceRule.WarningThresholdU:F2} W/(m²K). " +
                        $"Ajánlott hőhíd-pótlék: " +
                        $"{allowanceRule.RecommendedAllowance * 100:F0}%. " +
                        allowanceRule.Note);
                }
            }

            // 2. Szellőzési hőveszteség
            // EN 12831: max(n_vent, n_infil) – a mérvadó értéket vesszük
            double dominantAch =
                Math.Max(input.VentilationAch, input.InfiltrationAch);
            double ventilationW =
                _context.HeatingRules.AirVolumetricHeatCapacity
                * dominantAch
                * input.AirVolumeM3
                * deltaT;

            // 3. Hőhíd-pótlék
            double thermalBridgeCorrectionW =
                input.ThermalBridgeAllowance * sumTransmissionW;

            double designBaseW = sumTransmissionW
                                + ventilationW
                                + thermalBridgeCorrectionW;

            // 4. Felfűtési pótlék
            double reheatW = (input.ReheatFactor - 1.0) * designBaseW;

            return new RoomHeatingResult(
                input.RoomId,
                input.DisplayName,
                sumTransmissionW,
                ventilationW,
                thermalBridgeCorrectionW,
                reheatW,
                input.ThermalBridgeAllowance,
                new ReadOnlyCollection<ElementHeatLossDetail>(elementDetails),
                new ReadOnlyCollection<string>(thermalBridgeWarnings));
        }

        /// <summary>
        /// Épület összes helyiségének fűtési hőterhelése.
        /// </summary>
        public BuildingHeatingResult CalculateBuilding(
            IEnumerable<RoomHeatingInput> rooms,
            string climateRegionId)
        {
            if (rooms == null)
                throw new ArgumentNullException(nameof(rooms));

            var results = new List<RoomHeatingResult>();
            foreach (var room in rooms)
                results.Add(CalculateRoom(room));

            return new BuildingHeatingResult(
                new ReadOnlyCollection<RoomHeatingResult>(results),
                climateRegionId);
        }

        /// <summary>
        /// Meghatározza a tényleges hőmérsékleti különbséget
        /// a határfeltétel alapján.
        ///
        /// - Outdoor: ΔT = Ti - Te
        /// - Ground: ΔT = b_u · (Ti - Te), ahol b_u ≈ 0.45-0.6
        ///   (egyszerűsített; pontos ISO 13370 módszer következő fázis)
        /// - AdjacentUnconditioned: ΔT = b_u · (Ti - Te), b_u ≈ 0.5
        ///   (fűtetlen szomszédos tér: padlástér, garázs stb.)
        /// - AdjacentConditionedZone: ΔT = 0 (nincs veszteség)
        /// - Adiabatic: ΔT = 0
        /// - UserDefinedTemperature: az adott hőmérsékleti különbséget vesszük
        ///   (a ConstructionBoundaryCondition FixedAdjacentTemperature mezőjéből)
        /// </summary>
        private static double GetEffectiveDeltaT(
            ConstructionBoundaryCondition bc,
            double deltaTNominal,
            double indoorT,
            double outdoorT)
        {
            // Ha a bc már megadja a ResolveTemperatureDifference-t, azt részesítjük
            // előnyben (ha a mód TemperatureReductionFactor vagy FixedTemperature).
            switch (bc.BoundaryKind)
            {
                case AdjacentBoundaryKind.Outdoor:
                    return deltaTNominal;

                case AdjacentBoundaryKind.Ground:
                    // ISO 13370 egyszerűsített: b_u ≈ 0.45 alacsony szintű épületeknél
                    double buGround = bc.TemperatureReductionFactor ?? 0.45;
                    return buGround * deltaTNominal;

                case AdjacentBoundaryKind.AdjacentUnconditionedSpace:
                    // Fűtetlen tér: padlástér, garázs, pince
                    double buUncond = bc.TemperatureReductionFactor ?? 0.50;
                    return buUncond * deltaTNominal;

                case AdjacentBoundaryKind.AdjacentConditionedZone:
                case AdjacentBoundaryKind.Adiabatic:
                    return 0.0;

                case AdjacentBoundaryKind.UserDefinedTemperature:
                    if (bc.FixedAdjacentTemperature.HasValue)
                        return indoorT - bc.FixedAdjacentTemperature.Value;
                    return deltaTNominal;

                default:
                    return deltaTNominal;
            }
        }
    }
}
