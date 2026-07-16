using System;

namespace HVACDesigner.Data.Models.Duct
{
    public class DuctGeometry
    {
        public GeometryShape Shape { get; set; }

        public int InletDiameter { get; set; }
        public int InletWidth { get; set; }
        public int InletHeight { get; set; }

        public int OutletDiameter { get; set; }
        public int OutletWidth { get; set; }
        public int OutletHeight { get; set; }

        public int BranchDiameter { get; set; }
        public int BranchWidth { get; set; }
        public int BranchHeight { get; set; }

        public double FreeAreaPercent { get; set; } = 100;

        public double Length1 { get; set; }
        public double Length2 { get; set; }


        public double GetArea()
        {
            return GetInletArea();
        }


        /*public double GetEffectiveArea()
        {
            return GetEffectiveInletArea();
        }*/


        public double GetInletArea()
        {
            return GetArea(InletDiameter, InletWidth, InletHeight);
        }


        public double GetOutletArea()
        {
            int diameter = OutletDiameter > 0 ? OutletDiameter : InletDiameter;

            int width = OutletWidth > 0 ? OutletWidth : InletWidth;
            int height = OutletHeight > 0 ? OutletHeight : InletHeight;

            return GetArea(diameter, width, height);
        }


        public double GetBranchArea()
        {
            return GetArea(BranchDiameter, BranchWidth, BranchHeight);
        }


        public double GetEffectiveInletArea()
        {
            return GetInletArea() * FreeAreaPercent / 100.0;
        }


        public double GetEffectiveOutletArea()
        {
            return GetOutletArea() * FreeAreaPercent / 100.0;
        }


        public double GetEffectiveBranchArea()
        {
            return GetBranchArea() * FreeAreaPercent / 100.0;
        }


        public double GetInletHydraulicDiameter()
        {
            return GetHydraulicDiameter(
                InletDiameter,
                InletWidth,
                InletHeight);
        }


        public double GetOutletHydraulicDiameter()
        {
            return GetHydraulicDiameter(
                OutletDiameter > 0 ? OutletDiameter : InletDiameter,
                OutletWidth > 0 ? OutletWidth : InletWidth,
                OutletHeight > 0 ? OutletHeight : InletHeight);
        }


        public double GetBranchHydraulicDiameter()
        {
            return GetHydraulicDiameter(
                BranchDiameter,
                BranchWidth,
                BranchHeight);
        }


        private double GetArea(int diameter, int width, int height)
        {
            if (Shape == GeometryShape.Circular)
            {
                double d = diameter / 1000.0;

                if (d <= 0)
                    return 0;

                return Math.PI * d * d / 4.0;
            }

            double a = width / 1000.0;
            double b = height / 1000.0;

            if (a <= 0 || b <= 0)
                return 0;

            return a * b;
        }


        private double GetHydraulicDiameter(int diameter, int width, int height)
        {
            if (Shape == GeometryShape.Circular)
                return diameter / 1000.0;

            double a = width / 1000.0;
            double b = height / 1000.0;

            if (a + b <= 0)
                return 0;

            return 2 * a * b / (a + b);
        }


        public string InletLabel
        {
            get
            {
                if (Shape == GeometryShape.Circular)
                    return $"Ø{InletDiameter}";

                return $"{InletWidth}x{InletHeight}";
            }
        }


        public string OutletLabel
        {
            get
            {
                int diameter = OutletDiameter > 0 ? OutletDiameter : InletDiameter;
                int width = OutletWidth > 0 ? OutletWidth : InletWidth;
                int height = OutletHeight > 0 ? OutletHeight : InletHeight;

                if (Shape == GeometryShape.Circular)
                    return $"Ø{diameter}";

                return $"{width}x{height}";
            }
        }


        public string BranchLabel
        {
            get
            {
                if (Shape == GeometryShape.Circular)
                    return $"Ø{BranchDiameter}";

                return $"{BranchWidth}x{BranchHeight}";
            }
        }


        public static DuctGeometry Circular(int diameter)
        {
            return new DuctGeometry
            {
                Shape = GeometryShape.Circular,
                InletDiameter = diameter,
                OutletDiameter = diameter,
                BranchDiameter = diameter
            };
        }


        public static DuctGeometry Rectangular(int width, int height)
        {
            return new DuctGeometry
            {
                Shape = GeometryShape.Rectangular,
                InletWidth = width,
                InletHeight = height,
                OutletWidth = width,
                OutletHeight = height,
                BranchWidth = width,
                BranchHeight = height
            };
        }
    }
}