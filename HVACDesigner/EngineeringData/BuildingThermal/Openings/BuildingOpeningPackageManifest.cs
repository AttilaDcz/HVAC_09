using System;
using HVACDesigner.EngineeringData.Abstractions;

namespace HVACDesigner.EngineeringData.BuildingThermal.Openings
{
    public static class BuildingOpeningPackageManifest
    {
        public const string PackageId =
            "hvacdesigner.builtin.building-thermal";

        public const string ContentSetId =
            "building.openings";

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
                "HVAC Designer beépített nyílászáró-adatok",
                "1.0.0",
                "1.0",
                "BuildingThermal",
                DataPackageSourceType.Xml,
                materialDataXmlPath,
                new[]
                {
                    new ContentSetDescriptor(
                        ContentSetId,
                        EngineeringContentKind.ComponentCatalog,
                        typeof(BuildingOpeningDefinition).FullName,
                        ContentVersion,
                        "/MaterialCatalog",
                        isRequired: false)
                });
        }
    }
}
