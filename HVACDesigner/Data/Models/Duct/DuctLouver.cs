using System;

namespace HVACDesigner.Data.Models.Duct
{
    public class DuctLouver : DuctElement
    {
        public TerminalType TerminalType { get; set; }

        public double FreeAreaPercent { get; set; } = 70;

        public DuctLouver()
        {
            ElementType = DuctElementType.Louver;
            GeometryType = GeometryType.SingleSize;
            PressureLossType = PressureLossType.FreeArea;
            Category = "Terminál";
        }

        public override double CalculatePressureDrop(double airDensity = 1.2)
        {
            if (PressureLossType == PressureLossType.FixedPressure)
                return FixedPressureDrop;

            double velocity = GetFaceVelocity();

            if (velocity <= 0)
                return 0;

            double dynamicPressure =
                airDensity * Math.Pow(velocity, 2) / 2.0;

            if (PressureLossType == PressureLossType.FreeArea)
            {
                PressureDrop = Zeta * dynamicPressure;
                return PressureDrop;
            }

            PressureDrop = Zeta * dynamicPressure;

            return PressureDrop;
        }

        public override double GetVelocity()
        {
            return GetFaceVelocity();
        }

        private double GetFaceVelocity()
        {
            if (Geometry == null)
                return 0;

            double area = Geometry.GetArea();

            if (area <= 0)
                return 0;

            double freeArea =
                area * (FreeAreaPercent / 100.0);

            if (freeArea <= 0)
                return 0;

            return (Airflow / 3600.0) / freeArea;
        }

        public override string SizeLabel
        {
            get
            {
                if (Geometry == null)
                    return "";

                return Geometry.InletLabel;
            }
        }
    }
}