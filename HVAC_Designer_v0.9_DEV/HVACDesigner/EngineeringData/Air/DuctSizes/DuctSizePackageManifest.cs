using System;
using HVACDesigner.EngineeringData.Abstractions;

namespace HVACDesigner.EngineeringData.Air.DuctSizes
{
    public static class DuctSizePackageManifest
    {
        public const string PackageId = "hvacdesigner.builtin.air";
        public const string CircularContentSetId =
            "air.duct-sizes.circular";
        public const string RectangularContentSetId =
            "air.duct-sizes.rectangular";
        public const string ContentVersion = "1.0";

        public static EngineeringDataPackageManifest Create(
            string ductDataXmlPath)
        {
            if (string.IsNullOrWhiteSpace(ductDataXmlPath))
                throw new ArgumentException(
                    "Az XML útvonal nem lehet üres.",
                    nameof(ductDataXmlPath));

            return new EngineeringDataPackageManifest(
                PackageId,
                "HVAC Designer beépített légcsatorna méretek",
                "1.0.0",
                "1.0",
                "Air",
                DataPackageSourceType.Xml,
                ductDataXmlPath,
                new[]
                {
                    new ContentSetDescriptor(
                        CircularContentSetId,
                        EngineeringContentKind.StandardSizeCatalog,
                        typeof(CircularDuctSizeDefinition).FullName,
                        ContentVersion,
                        "/DuctDatabase/CircularSizes",
                        isRequired: false),
                    new ContentSetDescriptor(
                        RectangularContentSetId,
                        EngineeringContentKind.StandardSizeCatalog,
                        typeof(RectangularDuctSizeDefinition).FullName,
                        ContentVersion,
                        "/DuctDatabase/RectangularSizes",
                        isRequired: false)
                });
        }
    }
}
