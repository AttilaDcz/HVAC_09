using System;

namespace HVACDesigner.EngineeringData.Air.DuctSizes
{
    public sealed class CircularDuctSizeDefinition
    {
        public int DiameterMillimeters { get; }
        public double DiameterMeters => DiameterMillimeters / 1000.0;
        public double AreaSquareMeters =>
            Math.PI * DiameterMeters * DiameterMeters / 4.0;

        public string SourcePackageId { get; }
        public string SourceContentSetId { get; }
        public string SourceVersion { get; }

        public CircularDuctSizeDefinition(
            int diameterMillimeters,
            string sourcePackageId,
            string sourceContentSetId,
            string sourceVersion)
        {
            if (diameterMillimeters <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(diameterMillimeters));

            DiameterMillimeters = diameterMillimeters;
            SourcePackageId = RequireText(sourcePackageId, nameof(sourcePackageId));
            SourceContentSetId = RequireText(sourceContentSetId, nameof(sourceContentSetId));
            SourceVersion = RequireText(sourceVersion, nameof(sourceVersion));
        }

        private static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Az érték nem lehet üres.", parameterName);

            return value.Trim();
        }
    }

    public sealed class RectangularDuctSizeDefinition
    {
        public int WidthMillimeters { get; }
        public int HeightMillimeters { get; }

        public double WidthMeters => WidthMillimeters / 1000.0;
        public double HeightMeters => HeightMillimeters / 1000.0;
        public double AreaSquareMeters => WidthMeters * HeightMeters;
        public double HydraulicDiameterMeters =>
            2.0 * WidthMeters * HeightMeters / (WidthMeters + HeightMeters);

        public string SourcePackageId { get; }
        public string SourceContentSetId { get; }
        public string SourceVersion { get; }

        public RectangularDuctSizeDefinition(
            int widthMillimeters,
            int heightMillimeters,
            string sourcePackageId,
            string sourceContentSetId,
            string sourceVersion)
        {
            if (widthMillimeters <= 0)
                throw new ArgumentOutOfRangeException(nameof(widthMillimeters));

            if (heightMillimeters <= 0)
                throw new ArgumentOutOfRangeException(nameof(heightMillimeters));

            WidthMillimeters = widthMillimeters;
            HeightMillimeters = heightMillimeters;
            SourcePackageId = RequireText(sourcePackageId, nameof(sourcePackageId));
            SourceContentSetId = RequireText(sourceContentSetId, nameof(sourceContentSetId));
            SourceVersion = RequireText(sourceVersion, nameof(sourceVersion));
        }

        private static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Az érték nem lehet üres.", parameterName);

            return value.Trim();
        }
    }
}
