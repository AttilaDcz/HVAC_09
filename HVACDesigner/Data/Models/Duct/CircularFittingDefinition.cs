namespace HVACDesigner.Data.Models.Duct
{
    public class CircularFittingDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = "";

        public DuctElementType ElementType { get; set; }

        public FittingAngle Angle { get; set; }

        public ElbowType ElbowType { get; set; }

        public bool AllowShankLengths { get; set; }

        public double DefaultZeta { get; set; }

        public FlowDirection FlowDirection { get; set; }

        public GeometryType GeometryType { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}