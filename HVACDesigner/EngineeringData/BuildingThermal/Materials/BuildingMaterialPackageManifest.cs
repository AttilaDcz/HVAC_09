using System;
using HVACDesigner.EngineeringData.Abstractions;

namespace HVACDesigner.EngineeringData.BuildingThermal.Materials
{
    public static class BuildingMaterialPackageManifest
    {
        public const string PackageId =
            "hvacdesigner.builtin.building-thermal";

        public const string ContentSetId =
            "building.materials";

        public const string ContentVersion =
            "1.0";

        public static EngineeringDataPackageManifest Create(
            string materialDataXmlPath)
        {
            if (string.IsNullOrWhiteSpace(materialDataXmlPath))
            {
                throw new ArgumentException(
                    "Az XML útvonal nem lehet üres.",
                    nameof(materialDataXmlPath));
            }

            ContentSetDescriptor descriptor =
                new ContentSetDescriptor(
                    ContentSetId,
                    EngineeringContentKind.ReferenceCatalog,
                    typeof(BuildingMaterialDefinition).FullName,
                    ContentVersion,
                    "/MaterialCatalog",
                    isRequired: false);

            return new EngineeringDataPackageManifest(
                PackageId,
                "HVAC Designer beépített építőanyagok",
                "1.0.0",
                "1.0",
                "BuildingThermal",
                DataPackageSourceType.Xml,
                materialDataXmlPath,
                new[] { descriptor });
        }
    }
}
