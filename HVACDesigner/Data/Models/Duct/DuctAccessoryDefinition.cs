using System;

namespace HVACDesigner.Data.Models.Duct
{
    public class DuctAccessoryDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = "";

        public string Category { get; set; } = "";

        public DuctElementType ElementType { get; set; }

        public GeometryType GeometryType { get; set; } = GeometryType.SingleSize;

        public FlowDirection FlowDirection { get; set; } = FlowDirection.Both;

        public double DefaultZeta { get; set; }

        public double? FixedPressureDrop { get; set; }

        public double DefaultFreeArea { get; set; } = 100;

        public bool AllowSizeChange { get; set; }

        public bool AllowLength { get; set; }

        public bool AllowBranch { get; set; }

        public PressureLossType PressureLossType { get; set; } = PressureLossType.Zeta;

        public override string ToString()
        {
            return $"{Name} ({Category})";
        }
    }
}