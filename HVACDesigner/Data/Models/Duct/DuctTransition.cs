using System;

namespace HVACDesigner.Data.Models.Duct
{
    public class DuctTransition : DuctElement
    {
        public TransitionType TransitionType { get; set; }

        public double Length { get; set; }

        public DuctTransition()
        {
            ElementType = DuctElementType.Transition;
            GeometryType = GeometryType.SizeChange;
            PressureLossType = PressureLossType.Zeta;
            Category = "Átmenet";
        }

        public override double CalculatePressureDrop(double airDensity = 1.2)
        {
            if (PressureLossType == PressureLossType.FixedPressure)
                return FixedPressureDrop;

            double velocity = GetCalculationVelocity();

            if (velocity <= 0)
                return 0;

            double dynamicPressure = airDensity * Math.Pow(velocity, 2) / 2.0;

            PressureDrop = Zeta * dynamicPressure;

            return PressureDrop;
        }

        private double GetCalculationVelocity()
        {
            double inletVelocity = GetVelocity();

            if (Geometry == null)
                return inletVelocity;

            double outletArea = Geometry.GetEffectiveOutletArea();

            if (outletArea <= 0)
                return inletVelocity;

            double outletVelocity = (Airflow / 3600.0) / outletArea;

            return (inletVelocity + outletVelocity) / 2.0;
        }

        public override string SizeLabel
        {
            get
            {
                if (Geometry == null)
                    return "";

                return $"{Geometry.InletLabel} → {Geometry.OutletLabel}";
            }
        }
    }
}