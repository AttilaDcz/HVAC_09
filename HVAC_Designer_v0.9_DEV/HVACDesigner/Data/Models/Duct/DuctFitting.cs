using System;

namespace HVACDesigner.Data.Models.Duct
{
    public class DuctFitting : DuctElement
    {
        public double ShankLength1 { get; set; }

        public double ShankLength2 { get; set; }

        public DuctFitting()
        {
            ElementType = DuctElementType.Elbow;
            GeometryType = GeometryType.SizeChange;
            PressureLossType = PressureLossType.Zeta;
            Category = "Idom";
        }

        public double GetOutletVelocity()
        {
            if (Geometry == null)
                return 0;

            double area = Geometry.GetEffectiveOutletArea();

            if (area <= 0)
                return 0;

            return (Airflow / 3600.0) / area;
        }

        public double GetCalculationVelocity()
        {
            double vin = GetVelocity();

            if (GeometryType != GeometryType.SizeChange)
                return vin;

            double vout = GetOutletVelocity();

            if (vout <= 0)
                return vin;

            return (vin + vout) / 2.0;
        }
        public bool UseFixPressureDrop
        {
            get => PressureLossType == PressureLossType.FixedPressure;
            set
            {
                if (value)
                    PressureLossType = PressureLossType.FixedPressure;
            }
        }


        public double FixPressureDrop
        {
            get => FixedPressureDrop;
            set => FixedPressureDrop = value;
        }

        public override double CalculatePressureDrop(double airDensity = 1.204)
        {
            if (Airflow <= 0) return 0;

            // Átirányítjuk a számítást a központi fizikai motorba, 
            // ami a gépészetileg helyes vonatkoztatott sebességgel fog dolgozni.
            var tempCalc = new Calculations.Air.DuctCalculator { AirDensity = airDensity };
            return tempCalc.ComputeFittingLoss(this, Airflow);
        }
    }
}