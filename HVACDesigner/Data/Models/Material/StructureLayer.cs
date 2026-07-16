namespace HVACDesigner.Data.Models.Material
{
    public class StructureLayer
    {
        public string Name { get; set; } = string.Empty;
        public double Thickness { get; set; } // Rétegvastagság méterben [m]
        public Material? BaseMaterial { get; set; } // Referencia a fizikai anyagra (Lambda, Mu, stb.)
    }
}
