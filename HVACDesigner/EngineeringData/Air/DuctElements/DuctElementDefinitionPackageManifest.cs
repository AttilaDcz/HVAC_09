using System;
using HVACDesigner.EngineeringData.Abstractions;

namespace HVACDesigner.EngineeringData.Air.DuctElements
{
    public static class DuctElementDefinitionPackageManifest
    {
        public const string PackageId =
            "hvacdesigner.builtin.air";

        public const string ContentSetId =
            "air.duct-elements";

        public const string ContentVersion =
            "1.0";

        public static EngineeringDataPackageManifest Create(
            string ductDataXmlPath)
        {
            if (string.IsNullOrWhiteSpace(ductDataXmlPath))
                throw new ArgumentException(
                    "Az XML útvonal nem lehet üres.",
                    nameof(ductDataXmlPath));

            var descriptor = new ContentSetDescriptor(
                ContentSetId,
                EngineeringContentKind.ElementDefinitionLibrary,
                typeof(DuctElementDefinition).FullName,
                ContentVersion,
                "/DuctDatabase",
                isRequired: false,
                dependencies: new[]
                {
                    "air.duct-materials"
                });

            return new EngineeringDataPackageManifest(
                PackageId,
                "HVAC Designer beépített légtechnikai elemek",
                "1.0.0",
                "1.0",
                "Air",
                DataPackageSourceType.Xml,
                ductDataXmlPath,
                new[] { descriptor });
        }
    }
}
