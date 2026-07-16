using System;

namespace HVACDesigner.Data.Models.Duct
{
    public abstract class DuctElement
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = string.Empty;

        public DuctElementType ElementType { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Subtype { get; set; } = string.Empty;

        public FlowDirection Direction { get; set; } = FlowDirection.Both;

        public GeometryType GeometryType { get; set; }

        public DuctGeometry Geometry { get; set; } = new DuctGeometry();

        public double Airflow { get; set; }

        public DuctMaterial? MaterialOverride { get; set; }

        public PressureLossType PressureLossType { get; set; }

        public double Zeta { get; set; }

        public double FixedPressureDrop { get; set; }

        public double PressureDrop { get; set; }

        public double Velocity { get; set; }

        public int Index { get; set; }
        public bool IsLocked { get; set; }
        public string Description { get; set; } = "";

        public virtual string SizeLabel
        {
            get
            {
                if (Geometry == null)
                    return "";

                if (Geometry.Shape == GeometryShape.Circular)
                    return $"Ø{Geometry.InletDiameter}";

                return $"{Geometry.InletWidth}x{Geometry.InletHeight}";
            }
        }

        public virtual double GetVelocity()
        {
            if (Geometry == null)
                return 0;

            double area = Geometry.GetEffectiveInletArea();

            if (area <= 0)
                return 0;

            return (Airflow / 3600.0) / area;
        }

        public virtual double GetHydraulicDiameter()
        {
            if (Geometry == null)
                return 0;

            return Geometry.GetInletHydraulicDiameter();
        }

        public abstract double CalculatePressureDrop(double airDensity = 1.2);
    }
}
