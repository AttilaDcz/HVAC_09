using System;

namespace HVACDesigner.Data.Models.Duct
{
    public class DuctDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public GeometryType GeometryType { get; set; }

        public string Material { get; set; } = string.Empty;

        public FlowDirection FlowDirection { get; set; } = FlowDirection.Both;

        public string PressureModel { get; set; } = "Friction";

        public bool IsFlexible { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}