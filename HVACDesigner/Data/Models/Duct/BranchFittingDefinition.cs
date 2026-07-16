namespace HVACDesigner.Data.Models.Duct
{
    public class BranchFittingDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = "";

        public BranchType BranchType { get; set; }

        public double DefaultZeta { get; set; }

        public FlowDirection FlowDirection { get; set; } = FlowDirection.Both;

        public GeometryType GeometryType { get; set; } = GeometryType.Branch;

        public bool AllowBranchAirflow { get; set; } = true;

        public override string ToString()
        {
            return Name;
        }
    }
}