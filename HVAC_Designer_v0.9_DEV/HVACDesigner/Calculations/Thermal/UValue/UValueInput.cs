using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates;
using HVACDesigner.Calculations.Thermal.Common;

namespace HVACDesigner.Calculations.Thermal.UValue
{
    // ═══════════════════════════════════════════════════════════════
    // BEMENETI MODELLEK
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Egy réteg bemenete az U-érték számításhoz.
    /// </summary>
    public sealed class ConstructionLayerInput
    {
        /// <summary>Réteg típusa: anyag, légréteg vagy rögzített ellenállás</summary>
        public ConstructionLayerKind Kind { get; }

        /// <summary>Hivatkozás MaterialId-ra vagy AirLayerId-ra</summary>
        public string ReferenceId { get; }

        /// <summary>Vastagság [m] (anyagrétegnél kötelező)</summary>
        public double? ThicknessM { get; }

        /// <summary>Rögzített hőellenállás [m²K/W] (FixedResistance típusnál)</summary>
        public double? FixedResistance { get; }

        /// <summary>Terület-arány [-] (inhomogén rétegek esetén, alapértelmezett 1.0)</summary>
        public double AreaFraction { get; }

        /// <summary>Leíró megjegyzés</summary>
        public string Description { get; }

        public ConstructionLayerInput(
            ConstructionLayerKind kind,
            string referenceId,
            double? thicknessM = null,
            double? fixedResistance = null,
            double areaFraction = 1.0,
            string description = "")
        {
            Kind = kind;
            ReferenceId = referenceId?.Trim() ?? string.Empty;
            ThicknessM = thicknessM;
            FixedResistance = fixedResistance;
            AreaFraction = areaFraction;
            Description = description?.Trim() ?? string.Empty;
        }

        public static ConstructionLayerInput Material(
            string materialId, double thicknessM, string description = "")
            => new ConstructionLayerInput(
                ConstructionLayerKind.Material, materialId,
                thicknessM, null, 1.0, description);

        public static ConstructionLayerInput AirLayer(
            string airLayerId, string description = "")
            => new ConstructionLayerInput(
                ConstructionLayerKind.AirLayer, airLayerId,
                null, null, 1.0, description);

        public static ConstructionLayerInput Fixed(
            double resistance, string description = "")
            => new ConstructionLayerInput(
                ConstructionLayerKind.FixedResistance, string.Empty,
                null, resistance, 1.0, description);
    }

    /// <summary>
    /// Egy épületszerkezet U-érték számítási bemenete.
    /// </summary>
    public sealed class UValueInput
    {
        public string ConstructionId { get; }
        public string DisplayName { get; }
        public ConstructionType ConstructionType { get; }
        public AdjacentBoundaryKind BoundaryKind { get; }
        public IReadOnlyList<ConstructionLayerInput> Layers { get; }

        public UValueInput(
            string constructionId,
            string displayName,
            ConstructionType constructionType,
            AdjacentBoundaryKind boundaryKind,
            IEnumerable<ConstructionLayerInput> layers)
        {
            if (string.IsNullOrWhiteSpace(constructionId))
                throw new ArgumentException(
                    "A ConstructionId nem lehet üres.", nameof(constructionId));

            ConstructionId = constructionId.Trim();
            DisplayName = displayName?.Trim() ?? constructionId;
            ConstructionType = constructionType;
            BoundaryKind = boundaryKind;
            Layers = new ReadOnlyCollection<ConstructionLayerInput>(
                new List<ConstructionLayerInput>(
                    layers
                    ?? throw new ArgumentNullException(nameof(layers))));

            if (Layers.Count == 0)
                throw new ArgumentException(
                    "Legalább egy réteget meg kell adni.", nameof(layers));
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // EREDMÉNY MODELLEK
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Egy réteg számított hőellenállása az U-érték breakdown-ban.
    /// </summary>
    public sealed class LayerThermalResistance
    {
        public int Order { get; }
        public string ReferenceId { get; }
        public string Description { get; }
        public ConstructionLayerKind Kind { get; }
        public double? ThicknessM { get; }
        public double ThermalResistance { get; }   // [m²K/W]

        public LayerThermalResistance(
            int order,
            string referenceId,
            string description,
            ConstructionLayerKind kind,
            double? thicknessM,
            double thermalResistance)
        {
            Order = order;
            ReferenceId = referenceId?.Trim() ?? string.Empty;
            Description = description?.Trim() ?? string.Empty;
            Kind = kind;
            ThicknessM = thicknessM;
            ThermalResistance = thermalResistance;
        }
    }

    /// <summary>
    /// U-érték számítás eredménye (ISO 6946:2017 módszer).
    /// </summary>
    public sealed class UValueResult
    {
        public string ConstructionId { get; }
        public string DisplayName { get; }

        /// <summary>Belső felületi hőellenállás [m²K/W]</summary>
        public double Rsi { get; }

        /// <summary>Külső felületi hőellenállás [m²K/W]</summary>
        public double Rse { get; }

        /// <summary>Rétegek hőellenállásai (breakdown)</summary>
        public IReadOnlyList<LayerThermalResistance> Layers { get; }

        /// <summary>Összes hőellenállás R_T = Rsi + ΣRi + Rse [m²K/W]</summary>
        public double TotalThermalResistance { get; }

        /// <summary>Hőátbocsátási tényező U = 1/R_T [W/(m²K)]</summary>
        public double UValue { get; }

        /// <summary>Meghaladja-e az ÉKM Umax korlátot?</summary>
        public bool ExceedsEkmLimit { get; }

        /// <summary>ÉKM Umax értéke [W/(m²K)], ha elérhető</summary>
        public double? EkmLimit { get; }

        /// <summary>
        /// Hőhíd-figyelmeztetés szövege, ha az U-érték meghaladja a threshold-ot.
        /// Null, ha nincs figyelmeztetés szükséges.
        /// </summary>
        public string ThermalBridgeWarning { get; }

        /// <summary>Ajánlott hőhíd-pótlék a szerkezettípushoz [-]</summary>
        public double RecommendedThermalBridgeAllowance { get; }

        public UValueResult(
            string constructionId,
            string displayName,
            double rsi,
            double rse,
            IReadOnlyList<LayerThermalResistance> layers,
            double totalThermalResistance,
            double uValue,
            bool exceedsEkmLimit,
            double? ekmLimit,
            double recommendedThermalBridgeAllowance,
            string thermalBridgeWarning = null)
        {
            ConstructionId = constructionId;
            DisplayName = displayName;
            Rsi = rsi;
            Rse = rse;
            Layers = layers;
            TotalThermalResistance = totalThermalResistance;
            UValue = uValue;
            ExceedsEkmLimit = exceedsEkmLimit;
            EkmLimit = ekmLimit;
            RecommendedThermalBridgeAllowance = recommendedThermalBridgeAllowance;
            ThermalBridgeWarning = thermalBridgeWarning;
        }

        /// <summary>Igaz, ha hőhíd-figyelmeztetés van</summary>
        public bool HasThermalBridgeWarning =>
            !string.IsNullOrEmpty(ThermalBridgeWarning);
    }
}
