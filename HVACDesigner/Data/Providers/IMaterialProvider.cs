using System.Collections.Generic;
using HVACDesigner.Data.Models.Material;

namespace HVACDesigner.Data.Providers
{
    public interface IMaterialProvider
    {
        // Kategóriák és homogén anyagok elérése
        List<MaterialCategory> GetAllCategories();
        List<string> GetCategoryNames();
        List<Material> GetMaterialsByCategory(string categoryName);

        // BME-TABULA Típustár elérése
        List<PredefinedStructure> GetPredefinedStructures();
        Dictionary<string, double> GetUValueRequirements();
    }
}