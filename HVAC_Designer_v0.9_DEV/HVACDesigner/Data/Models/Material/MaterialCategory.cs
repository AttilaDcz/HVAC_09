using System.Collections.Generic;

namespace HVACDesigner.Data.Models.Material
{
    public class MaterialCategory
    {
        public string Name { get; set; } = string.Empty;
        public List<Material> Materials { get; set; } = new List<Material>();
    }
}