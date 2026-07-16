using HVACDesigner.Data.Models.Material;
using System.Collections.Generic;

namespace HVACDesigner.Data.Models.Material
{
    public class PredefinedStructure
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string StructureType { get; set; } = "OutsideWall"; // OutsideWall, RoofUpward, FloorDownward
        public List<StructureLayer> Layers { get; set; } = new List<StructureLayer>();
    }
}