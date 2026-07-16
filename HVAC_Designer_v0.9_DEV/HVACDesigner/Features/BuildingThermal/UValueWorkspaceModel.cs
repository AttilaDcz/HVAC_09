using System;
using System.Collections.Generic;
using HVACDesigner.Calculations.Thermal.UValue;
using HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates;

namespace HVACDesigner.Features.BuildingThermal
{
    public interface IThermalTransmittanceResult
    {
        string StructureId { get; }
        string StructureName { get; }
        double UValue { get; } // A korrigált vagy eredő U-érték [W/m²K]
        double? EkmLimit { get; }
        bool ExceedsEkmLimit { get; }
        bool HasWarning { get; }
        string WarningMessage { get; }
    }

    public sealed class OpaqueConstructionUValueResult : IThermalTransmittanceResult
    {
        public string StructureId { get; }
        public string StructureName { get; }
        public double BaseUValue { get; }
        public double ThermalBridgeCorrectionFactor { get; }
        public double UValue => BaseUValue * (1.0 + ThermalBridgeCorrectionFactor);
        public double? EkmLimit { get; }
        public bool ExceedsEkmLimit => EkmLimit.HasValue && UValue > EkmLimit.Value + 1e-6;
        public bool HasWarning { get; }
        public string WarningMessage { get; }
        public IReadOnlyList<LayerThermalResistance> Layers { get; }

        public OpaqueConstructionUValueResult(
            string structureId,
            string structureName,
            double baseUValue,
            double thermalBridgeCorrectionFactor,
            double? ekmLimit,
            bool hasWarning,
            string warningMessage,
            IEnumerable<LayerThermalResistance> layers)
        {
            StructureId = structureId ?? string.Empty;
            StructureName = structureName ?? string.Empty;
            BaseUValue = baseUValue;
            ThermalBridgeCorrectionFactor = thermalBridgeCorrectionFactor;
            EkmLimit = ekmLimit;
            HasWarning = hasWarning;
            WarningMessage = warningMessage ?? string.Empty;
            Layers = new List<LayerThermalResistance>(layers ?? Array.Empty<LayerThermalResistance>());
        }
    }

    public sealed class OpeningUValueResult : IThermalTransmittanceResult
    {
        public string StructureId { get; }
        public string StructureName { get; }
        public double BaseUValue { get; } // Az eredő ablak Uw érték
        public double UValue => BaseUValue; // Nyílászáróknál nincs extra χ korrekció
        public double? EkmLimit { get; }
        public bool ExceedsEkmLimit => EkmLimit.HasValue && UValue > EkmLimit.Value + 1e-6;
        public bool HasWarning { get; }
        public string WarningMessage { get; }

        public double WidthM { get; }
        public double HeightM { get; }
        public double FrameVisibleWidthM { get; }

        public OpeningUValueResult(
            string structureId,
            string structureName,
            double uw,
            double? ekmLimit,
            bool hasWarning,
            string warningMessage,
            double widthM,
            double heightM,
            double frameVisibleWidthM)
        {
            StructureId = structureId ?? string.Empty;
            StructureName = structureName ?? string.Empty;
            BaseUValue = uw;
            EkmLimit = ekmLimit;
            HasWarning = hasWarning;
            WarningMessage = warningMessage ?? string.Empty;
            WidthM = widthM;
            HeightM = heightM;
            FrameVisibleWidthM = frameVisibleWidthM;
        }
    }

    public sealed class UserLayer
    {
        public string ReferenceId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsAirLayer { get; set; }
        public double ThicknessM { get; set; }
        public double DesignLambda { get; set; }
    }

    public sealed class UserStructure
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ConstructionType Type { get; set; }
        public List<UserLayer> Layers { get; } = new List<UserLayer>();
        public double ThermalBridgeCorrectionFactor { get; set; } = 0.10;
        public int ThermalBridgeOptionIndex { get; set; } = 0;
        public IThermalTransmittanceResult LastResult { get; set; }

        // Nyílászáró tulajdonságok
        public bool IsOpening { get; set; }
        public string OpeningType { get; set; } = "Window";
        public bool IsOpeningCalculated { get; set; }
        public string OpeningCatalogId { get; set; } = string.Empty;
        public string GlazingId { get; set; } = string.Empty;
        public string FrameId { get; set; } = string.Empty;
        public string SpacerId { get; set; } = string.Empty;
        
        // Geometriai méretek (alapértelmezésekkel)
        public double OpeningWidthM { get; set; } = 1.23;
        public double OpeningHeightM { get; set; } = 1.48;
        public double FrameWidthMm { get; set; } = 80.0;
    }
}
