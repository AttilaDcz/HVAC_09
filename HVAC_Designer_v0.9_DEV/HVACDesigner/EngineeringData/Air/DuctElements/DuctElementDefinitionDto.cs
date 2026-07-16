using System.Collections.Generic;

namespace HVACDesigner.EngineeringData.Air.DuctElements
{
    public sealed class DuctElementDefinitionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string GeometryType { get; set; } = string.Empty;
        public string FlowDirection { get; set; } = string.Empty;
        public string PressureModel { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string DefaultZeta { get; set; } = string.Empty;

        public string AllowSizeChange { get; set; } = string.Empty;
        public string AllowLength { get; set; } = string.Empty;
        public string AllowRadius { get; set; } = string.Empty;
        public string AllowBranch { get; set; } = string.Empty;

        public string SourceElementName { get; set; } = string.Empty;
        public string SourceSectionPath { get; set; } = string.Empty;
        public int SourceRecordIndex { get; set; }

        public IDictionary<string, string> Metadata { get; set; } =
            new Dictionary<string, string>();
    }
}
