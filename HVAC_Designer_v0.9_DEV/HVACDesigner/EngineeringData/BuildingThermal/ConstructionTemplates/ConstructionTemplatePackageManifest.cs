using System;
using HVACDesigner.EngineeringData.Abstractions;

namespace HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates
{
    public static class ConstructionTemplatePackageManifest
    {
        public const string PackageId =
            "hvacdesigner.builtin.building-thermal";

        public const string ContentSetId =
            "building.construction-templates";

        public const string ContentVersion =
            "1.0";

        public static EngineeringDataPackageManifest Create(
            string materialDataXmlPath)
        {
            if (string.IsNullOrWhiteSpace(materialDataXmlPath))
                throw new ArgumentException(
                    "Az XML útvonal nem lehet üres.",
                    nameof(materialDataXmlPath));

            return new EngineeringDataPackageManifest(
                PackageId,
                "HVAC Designer beépített szerkezetsablonok",
                "1.0.0",
                "1.0",
                "BuildingThermal",
                DataPackageSourceType.Xml,
                materialDataXmlPath,
                new[]
                {
                    new ContentSetDescriptor(
                        ContentSetId,
                        EngineeringContentKind.TemplateLibrary,
                        typeof(ConstructionTemplateDefinition).FullName,
                        ContentVersion,
                        "/MaterialCatalog/PredefinedStructures",
                        isRequired: false,
                        dependencies: new[]
                        {
                            "building.materials",
                            "building.air-layers"
                        })
                });
        }
    }
}
