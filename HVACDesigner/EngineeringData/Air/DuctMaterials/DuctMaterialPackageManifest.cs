using System;
using HVACDesigner.EngineeringData.Abstractions;

namespace HVACDesigner.EngineeringData.Air.DuctMaterials
{
    public static class DuctMaterialPackageManifest
    {
        public const string PackageId =
            "hvacdesigner.builtin.air";

        public const string ContentSetId =
            "air.duct-materials";

        public const string ContentVersion =
            "1.0";

        public static EngineeringDataPackageManifest Create(
            string ductDataXmlPath)
        {
            if (string.IsNullOrWhiteSpace(ductDataXmlPath))
            {
                throw new ArgumentException(
                    "Az XML útvonal nem lehet üres.",
                    nameof(ductDataXmlPath));
            }

            var descriptor =
                new ContentSetDescriptor(
                    ContentSetId,
                    EngineeringContentKind.ReferenceCatalog,
                    typeof(DuctMaterialDefinition).FullName,
                    ContentVersion,
                    "/DuctDatabase/Materials",
                    isRequired: false);

            return new EngineeringDataPackageManifest(
                PackageId,
                "HVAC Designer beépített légtechnikai adatok",
                "1.0.0",
                "1.0",
                "Air",
                DataPackageSourceType.Xml,
                ductDataXmlPath,
                new[] { descriptor });
        }
    }
}
