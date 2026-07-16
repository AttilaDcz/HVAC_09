using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates;
using HVACDesigner.Calculations.Thermal.Common;

namespace HVACDesigner.Calculations.Thermal.UValue
{
    /// <summary>
    /// U-érték számító ISO 6946:2017 módszer alapján.
    ///
    /// Algoritmus:
    ///   1. Rsi és Rse meghatározása szerkezettípus/hőáram-irány alapján
    ///   2. Minden rétegre:
    ///      - Material: R_i = d_i / λ_design
    ///      - AirLayer: R_i = katalógusból (ThermalResistance)
    ///      - FixedResistance: R_i = rögzített érték
    ///   3. R_T = Rsi + ΣR_i + Rse
    ///   4. U = 1 / R_T
    ///   5. ÉKM-limit ellenőrzés
    ///   6. Hőhíd-ajánlás és figyelmeztetés
    /// </summary>
    public sealed class UValueCalculator
    {
        private readonly ThermalCalculationContext _context;

        // ÉKM Umax határértékek szerkezettípusonként [W/(m²K)]
        // Forrás: ÉKM 9/2023. (V. 25.) rendelet
        private static readonly IReadOnlyDictionary<ConstructionType, double>
            EkmLimits = new ReadOnlyDictionary<ConstructionType, double>(
                new Dictionary<ConstructionType, double>
                {
                    { ConstructionType.ExternalWall,  0.20 },
                    { ConstructionType.Roof,          0.17 },
                    { ConstructionType.Ceiling,       0.17 },
                    { ConstructionType.GroundFloor,   0.30 },
                    { ConstructionType.BasementWall,  0.26 },
                    // Belső határolók fűtetlen tér felé (elválasztó fal)
                    { ConstructionType.InternalWall,  0.40 },
                });

        public UValueCalculator(ThermalCalculationContext context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Kiszámítja az U-értéket egy szerkezeti bemenet alapján.
        /// </summary>
        public UValueResult Calculate(UValueInput input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // 1. Felületi hőellenállások
            var (rsi, rse) = _context.PhysicsRules
                .GetSurfaceResistances(input.ConstructionType);

            // 2. Rétegek hőellenállásainak kiszámítása
            var layerResults = new List<LayerThermalResistance>();
            double sumLayerR = 0.0;
            int order = 0;

            foreach (var layer in input.Layers)
            {
                order++;
                double layerR = CalculateLayerResistance(layer, order);
                layerResults.Add(new LayerThermalResistance(
                    order,
                    layer.ReferenceId,
                    layer.Description,
                    layer.Kind,
                    layer.ThicknessM,
                    layerR));
                sumLayerR += layerR;
            }

            // 3. Teljes hőellenállás
            double rTotal = rsi + sumLayerR + rse;

            // 4. U-érték
            if (rTotal <= 0.0)
                throw new InvalidOperationException(
                    $"A teljes hőellenállás (R_T = {rTotal:F4} m²K/W) nem pozitív. " +
                    "Ellenőrizze a rétegadatokat.");

            double uValue = 1.0 / rTotal;

            // 5. ÉKM-limit ellenőrzés
            double? ekmLimit = null;
            bool exceedsLimit = false;
            if (EkmLimits.TryGetValue(input.ConstructionType, out double limit))
            {
                ekmLimit = limit;
                exceedsLimit = uValue > limit + 1e-6;
            }

            // 6. Hőhíd-ajánlás és figyelmeztetés
            var allowanceRule = _context.HeatingRules
                .FindAllowance(input.ConstructionType, uValue);

            string thermalBridgeWarning = null;
            if (uValue > allowanceRule.WarningThresholdU)
            {
                thermalBridgeWarning =
                    $"A szerkezet U-értéke ({uValue:F3} W/(m²K)) meghaladja " +
                    $"a {allowanceRule.WarningThresholdU:F2} W/(m²K) figyelmeztetési " +
                    $"küszöböt. Ajánlott hőhíd-pótlék: " +
                    $"{allowanceRule.RecommendedAllowance * 100:F0}% " +
                    $"(ÉKM alap: {allowanceRule.DefaultAllowance * 100:F0}%). " +
                    allowanceRule.Note;
            }

            return new UValueResult(
                input.ConstructionId,
                input.DisplayName,
                rsi,
                rse,
                new ReadOnlyCollection<LayerThermalResistance>(layerResults),
                rTotal,
                uValue,
                exceedsLimit,
                ekmLimit,
                allowanceRule.RecommendedAllowance,
                thermalBridgeWarning);
        }

        /// <summary>
        /// Kiszámítja egy réteg hőellenállását a réteg típusa alapján.
        /// </summary>
        private double CalculateLayerResistance(
            ConstructionLayerInput layer, int order)
        {
            switch (layer.Kind)
            {
                case ConstructionLayerKind.Material:
                    return CalculateMaterialResistance(layer, order);

                case ConstructionLayerKind.AirLayer:
                    return CalculateAirLayerResistance(layer, order);

                case ConstructionLayerKind.FixedResistance:
                    if (!layer.FixedResistance.HasValue ||
                        layer.FixedResistance.Value <= 0.0)
                        throw new InvalidOperationException(
                            $"A {order}. réteg (FixedResistance) értéke hiányzik " +
                            "vagy nem pozitív.");
                    return layer.FixedResistance.Value;

                default:
                    throw new NotSupportedException(
                        $"Ismeretlen réteg típus: {layer.Kind}");
            }
        }

        private double CalculateMaterialResistance(
            ConstructionLayerInput layer, int order)
        {
            if (string.IsNullOrWhiteSpace(layer.ReferenceId))
                throw new InvalidOperationException(
                    $"A {order}. anyagréteghez hiányzik a MaterialId.");

            if (!layer.ThicknessM.HasValue || layer.ThicknessM.Value <= 0.0)
                throw new InvalidOperationException(
                    $"A {order}. anyagréteg ({layer.ReferenceId}) " +
                    "vastagsága hiányzik vagy nem pozitív.");

            // Az anyag kikeresése – a BuildingMaterialCatalog.TryGet() módszerrel
            if (!_context.Materials.TryGet(
                    layer.ReferenceId,
                    out var material))
            {
                throw new InvalidOperationException(
                    $"A(z) '{layer.ReferenceId}' anyag nem található " +
                    "az anyagkatalógusban.");
            }

            double lambda = material.DesignThermalConductivity;
            if (lambda <= 0.0)
                throw new InvalidOperationException(
                    $"A(z) '{layer.ReferenceId}' anyag tervezési " +
                    "hővezetési tényezője nem pozitív.");

            // R_i = d / λ  [m²K/W]
            return layer.ThicknessM.Value / lambda;
        }

        private double CalculateAirLayerResistance(
            ConstructionLayerInput layer, int order)
        {
            if (string.IsNullOrWhiteSpace(layer.ReferenceId))
                throw new InvalidOperationException(
                    $"A {order}. légréteghez hiányzik az AirLayerId.");

            if (!_context.AirLayers.TryGet(
                    layer.ReferenceId,
                    out var airLayer))
            {
                throw new InvalidOperationException(
                    $"A(z) '{layer.ReferenceId}' légréteg nem található " +
                    "a légréteg katalógusban.");
            }

            if (airLayer.ThermalResistance <= 0.0)
                throw new InvalidOperationException(
                    $"A(z) '{layer.ReferenceId}' légréteg hőellenállása " +
                    "nem pozitív.");

            return airLayer.ThermalResistance;
        }
    }
}
