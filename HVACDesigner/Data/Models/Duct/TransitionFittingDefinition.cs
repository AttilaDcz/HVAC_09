namespace HVACDesigner.Data.Models.Duct
{
    public class TransitionFittingDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = "";

        public double DefaultZeta { get; set; }

        public FlowDirection FlowDirection { get; set; } = FlowDirection.Both;

        public GeometryType GeometryType { get; set; } = GeometryType.SizeChange;

        public bool AllowLength { get; set; } = true;

        public bool AllowSizeChange { get; set; } = true;

        public override string ToString()
        {
            return Name;
        }
    }
}