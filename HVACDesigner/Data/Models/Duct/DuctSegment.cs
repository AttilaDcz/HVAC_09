using System;

namespace HVACDesigner.Data.Models.Duct
{
    public class DuctSegment : DuctElement
    {
        public double Length { get; set; }

        public bool IsFlexible { get; set; }

        public double? RoughnessOverride { get; set; }

        public DuctSegment()
        {
            ElementType = DuctElementType.StraightDuct;
            GeometryType = GeometryType.LengthOnly;
            PressureLossType = PressureLossType.Friction;
            Category = "Csőszakasz";
        }

        public double GetRoughness(double globalRoughness)
        {
            if (RoughnessOverride.HasValue)
                return RoughnessOverride.Value;

            if (MaterialOverride != null)
                return MaterialOverride.Roughness;

            return globalRoughness;
        }

        public override double CalculatePressureDrop(double airDensity = 1.204)
        {
            if (Length <= 0 || Airflow <= 0) return 0;

            // Ha közvetlenül hívják, egy alapértelmezett kalkulátorral számol,
            // de hálózati szinten a DuctCalculator fogja futtatni.
            var tempCalc = new Calculations.Air.DuctCalculator { AirDensity = airDensity };
            return tempCalc.ComputeSegmentLoss(this, Airflow);
        }
    }
}