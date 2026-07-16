using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using HVACDesigner.Data.Models.Material;

namespace HVACDesigner.Data.Providers
{
    public class XmlMaterialProvider : XmlDataProvider, IMaterialProvider
    {
        // 🏢 Belső szótár a jogszabályi határértékek tárolására (Type string -> UMax)
        private Dictionary<string, double> _uValueRequirements = new Dictionary<string, double>();

        public XmlMaterialProvider(string xmlPath) : base(xmlPath)
        {
            LoadRequirements();
        }

        // 🏢 JOGSZABÁLYI KÖVETELMÉNYEK BEOLVASÁSA
        private void LoadRequirements()
        {
            _uValueRequirements.Clear();
            var requirementsNode = Doc.Root?.Element("Requirements");

            if (requirementsNode != null)
            {
                foreach (var req in requirementsNode.Elements("StructureRequirement"))
                {
                    string? type = (string?)req.Attribute("Type");
                    XAttribute? uMaxAttr = req.Attribute("UMax");

                    if (!string.IsNullOrEmpty(type) && uMaxAttr != null)
                    {
                        _uValueRequirements[type] = ParseDouble(uMaxAttr);
                    }
                }
            }
        }

        public Dictionary<string, double> GetUValueRequirements()
        {
            return _uValueRequirements;
        }

        // 1. KATEGÓRIÁK ÉS HOMOGÉN ELEMEK BEOLVASÁSA
        public List<MaterialCategory> GetAllCategories()
        {
            var categories = new List<MaterialCategory>();
            var elements = Doc.Root?.Elements("Category");

            if (elements == null) return categories;

            foreach (var catXml in elements)
            {
                var category = new MaterialCategory
                {
                    Name = (string?)catXml.Attribute("Name") ?? "Névtelen kategória"
                };

                foreach (var matXml in catXml.Elements("Material"))
                {
                    category.Materials.Add(new Material
                    {
                        Id = (string?)matXml.Attribute("Id") ?? "",
                        Name = (string?)matXml.Attribute("Name") ?? "",
                        Lambda = ParseDouble(matXml.Attribute("Lambda")),
                        Density = ParseDouble(matXml.Attribute("Density")),
                        SpecificHeat = ParseDouble(matXml.Attribute("SpecificHeat")),
                        Mu = ParseDouble(matXml.Attribute("Mu")),
                        IsAirLayer = false
                    });
                }

                foreach (var airXml in catXml.Elements("AirLayer"))
                {
                    category.Materials.Add(new Material
                    {
                        Id = (string?)airXml.Attribute("Id") ?? "",
                        Name = (string?)airXml.Attribute("Name") ?? "",
                        ThermalResistanceOverride = ParseDouble(airXml.Attribute("ThermalRes")),
                        IsAirLayer = true
                    });
                }

                foreach (var defXml in catXml.Elements("Default"))
                {
                    category.Materials.Add(new Material
                    {
                        Id = (string?)defXml.Attribute("Id") ?? "",
                        Name = (string?)defXml.Attribute("Name") ?? "",
                        UwFix = ParseDouble(defXml.Attribute("Uw")),
                        GValue = ParseDouble(defXml.Attribute("GValue")),
                        IsWindowDefault = true
                    });
                }

                foreach (var glXml in catXml.Elements("Glazing"))
                {
                    category.Materials.Add(new Material { Id = (string?)glXml.Attribute("Id") ?? "", Name = (string?)glXml.Attribute("Name") ?? "", Ug = ParseDouble(glXml.Attribute("Ug")), GValue = ParseDouble(glXml.Attribute("GValue")), IsComponent = true });
                }
                foreach (var frXml in catXml.Elements("Frame"))
                {
                    category.Materials.Add(new Material { Id = (string?)frXml.Attribute("Id") ?? "", Name = (string?)frXml.Attribute("Name") ?? "", Uf = ParseDouble(frXml.Attribute("Uf")), IsComponent = true });
                }
                foreach (var spXml in catXml.Elements("Spacer"))
                {
                    category.Materials.Add(new Material { Id = (string?)spXml.Attribute("Id") ?? "", Name = (string?)spXml.Attribute("Name") ?? "", Psi = ParseDouble(spXml.Attribute("Psi")), IsComponent = true });
                }

                categories.Add(category);
            }

            return categories;
        }

        public List<string> GetCategoryNames()
        {
            return Doc.Root?.Elements("Category")
                .Select(x => (string?)x.Attribute("Name") ?? "")
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList() ?? new List<string>();
        }

        public List<Material> GetMaterialsByCategory(string categoryName)
        {
            var categories = GetAllCategories();
            var target = categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            return target?.Materials ?? new List<Material>();
        }

        // 2. ELŐRE DEFINIÁLT SZERKEZETEK BEOLVASÁSA (Tiszta string-alapon, ahogy a modelled kéri!)
        public List<PredefinedStructure> GetPredefinedStructures()
        {
            var structures = new List<PredefinedStructure>();
            var section = Doc.Root?.Element("PredefinedStructures");

            if (section == null) return structures;

            var allMaterials = GetAllCategories().SelectMany(c => c.Materials).ToList();

            foreach (var structXml in section.Elements("Structure"))
            {
                var structure = new PredefinedStructure
                {
                    Id = (string?)structXml.Attribute("Id") ?? Guid.NewGuid().ToString(),
                    Name = (string?)structXml.Attribute("Name") ?? "",
                    StructureType = (string?)structXml.Attribute("Type") ?? "OutsideWall" // Sima string átadás!
                };

                foreach (var layerXml in structXml.Elements("Layer"))
                {
                    string matId = (string?)layerXml.Attribute("MaterialId") ?? (string?)layerXml.Attribute("Id") ?? "";
                    var baseMaterial = allMaterials.FirstOrDefault(m => m.Id == matId);

                    var layer = new StructureLayer
                    {
                        Name = (string?)layerXml.Attribute("Name") ?? baseMaterial?.Name ?? "Ismeretlen réteg",
                        Thickness = ParseDouble(layerXml.Attribute("Thickness")),
                        BaseMaterial = baseMaterial
                    };

                    structure.Layers.Add(layer);
                }
                structures.Add(structure);
            }

            return structures;
        }

        private static double ParseDouble(XAttribute? attr)
        {
            if (attr == null) return 0;
            return double.TryParse(attr.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double res) ? res : 0;
        }
    }
}
