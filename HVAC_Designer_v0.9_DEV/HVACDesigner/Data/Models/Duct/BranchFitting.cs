using System;

namespace HVACDesigner.Data.Models.Duct
{
    public class BranchFitting : DuctElement
    {
        public BranchType BranchType { get; set; }

        public double BranchAirflow { get; set; }

        public double MainOutletAirflow { get; set; }

        public double BranchPressureDrop { get; set; }

        public double BranchZeta { get; set; }

        public double Length { get; set; }
        public bool IsBranchDirection { get; set; } = false;

        public BranchFitting()
        {
            ElementType = DuctElementType.Branch;
            GeometryType = GeometryType.Branch;
            PressureLossType = PressureLossType.Zeta;
            Category = "Elágazás";
        }

        public override double CalculatePressureDrop(double airDensity = 1.2)
        {
            if (PressureLossType == PressureLossType.FixedPressure)
                return FixedPressureDrop;

            double velocity = GetVelocity();

            if (velocity <= 0)
                return 0;

            double dynamicPressure =
                airDensity * Math.Pow(velocity, 2) / 2.0;

            PressureDrop = Zeta * dynamicPressure;

            return PressureDrop;
        }

        public double CalculateBranchPressureDrop(double airDensity = 1.2)
        {
            if (Geometry == null)
                return 0;

            double area = Geometry.GetEffectiveBranchArea();

            if (area <= 0 || BranchAirflow <= 0)
                return 0;

            double velocity =
                (BranchAirflow / 3600.0) / area;

            double dynamicPressure =
                airDensity * Math.Pow(velocity, 2) / 2.0;

            BranchPressureDrop =
                BranchZeta * dynamicPressure;

            return BranchPressureDrop;
        }

        public void UpdateMainAirflow()
        {
            MainOutletAirflow =
                Math.Max(0, Airflow - BranchAirflow);
        }
        public double AirflowBranch1
        {
            get => BranchAirflow;
            set => BranchAirflow = value;
        }


        public double AirflowMainOut
        {
            get => MainOutletAirflow;
            set => MainOutletAirflow = value;
        }


        public double ZetaMain
        {
            get => Zeta;
            set => Zeta = value;
        }


        public double ZetaBranch1
        {
            get => BranchZeta;
            set => BranchZeta = value;
        }

        public override string SizeLabel
        {
            get
            {
                if (Geometry == null)
                    return "";

                return $"{Geometry.InletLabel} + {Geometry.BranchLabel}";
            }
        }
    }
}