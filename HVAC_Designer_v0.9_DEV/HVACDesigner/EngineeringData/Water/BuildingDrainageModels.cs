using System;

namespace HVACDesigner.EngineeringData.Water
{
    public enum ElevationCoordinateMode
    {
        Relative,
        Absolute
    }

    public enum ElevationDatum
    {
        ProjectZero,
        Baltic,
        Custom
    }

    public enum PipeElevationReference
    {
        InvertLevel,
        Centerline
    }

    public enum DrainageFlowDirection
    {
        Forward,
        Reverse
    }

    public sealed class ElevationValue
    {
        public double Value { get; }
        public ElevationCoordinateMode CoordinateMode { get; }
        public ElevationDatum Datum { get; }
        public string CustomDatumName { get; }

        public ElevationValue(
            double value,
            ElevationCoordinateMode coordinateMode,
            ElevationDatum datum,
            string customDatumName = "")
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (datum == ElevationDatum.Custom &&
                string.IsNullOrWhiteSpace(customDatumName))
            {
                throw new ArgumentException(
                    "Egyedi alapszinthez megnevezés szükséges.",
                    nameof(customDatumName));
            }

            Value = value;
            CoordinateMode = coordinateMode;
            Datum = datum;
            CustomDatumName =
                customDatumName?.Trim() ?? string.Empty;
        }
    }

    public sealed class BuildingDrainageSegment
    {
        public string Id { get; }
        public double Length { get; }
        public double Slope { get; }
        public int NominalDiameter { get; }
        public double InternalDiameter { get; }
        public double OuterDiameter { get; }
        public string PipeMaterialId { get; }

        public ElevationValue StartElevation { get; }
        public ElevationValue EndElevation { get; }
        public PipeElevationReference ElevationReference { get; }
        public DrainageFlowDirection FlowDirection { get; }

        public BuildingDrainageSegment(
            string id,
            double length,
            double slope,
            int nominalDiameter,
            double internalDiameter,
            double outerDiameter,
            string pipeMaterialId,
            ElevationValue startElevation,
            ElevationValue endElevation,
            PipeElevationReference elevationReference,
            DrainageFlowDirection flowDirection)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(
                    "A szakaszazonosító nem lehet üres.",
                    nameof(id));

            EnsurePositiveFinite(length, nameof(length));

            if (slope < 0.0 ||
                double.IsNaN(slope) ||
                double.IsInfinity(slope))
            {
                throw new ArgumentOutOfRangeException(nameof(slope));
            }

            if (nominalDiameter <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(nominalDiameter));

            EnsurePositiveFinite(
                internalDiameter,
                nameof(internalDiameter));

            EnsurePositiveFinite(
                outerDiameter,
                nameof(outerDiameter));

            if (outerDiameter < internalDiameter)
                throw new ArgumentException(
                    "A külső átmérő nem lehet kisebb a belső átmérőnél.");

            Id = id.Trim();
            Length = length;
            Slope = slope;
            NominalDiameter = nominalDiameter;
            InternalDiameter = internalDiameter;
            OuterDiameter = outerDiameter;
            PipeMaterialId =
                pipeMaterialId?.Trim() ?? string.Empty;
            StartElevation =
                startElevation ??
                throw new ArgumentNullException(
                    nameof(startElevation));
            EndElevation =
                endElevation ??
                throw new ArgumentNullException(
                    nameof(endElevation));
            ElevationReference = elevationReference;
            FlowDirection = flowDirection;
        }

        public double CalculateExpectedEndElevation()
        {
            double sign =
                FlowDirection == DrainageFlowDirection.Forward
                    ? -1.0
                    : 1.0;

            return StartElevation.Value +
                   sign * Length * Slope;
        }

        public double ConvertToInvertLevel(double elevation)
        {
            return ElevationReference ==
                   PipeElevationReference.InvertLevel
                ? elevation
                : elevation - OuterDiameter / 2.0;
        }

        public double ConvertToCenterline(double elevation)
        {
            return ElevationReference ==
                   PipeElevationReference.Centerline
                ? elevation
                : elevation + OuterDiameter / 2.0;
        }

        private static void EnsurePositiveFinite(
            double value,
            string parameterName)
        {
            if (value <= 0.0 ||
                double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName);
            }
        }
    }

    public sealed class BuildingDrainageOutlet
    {
        public string OutletId { get; }
        public double DesignFlow { get; }
        public int NominalDiameter { get; }
        public string PipeMaterialId { get; }
        public double MinimumRequiredSlope { get; }
        public double SelectedSlope { get; }

        public ElevationValue Elevation { get; }
        public PipeElevationReference ElevationReference { get; }

        public string ConnectionNote { get; }

        public BuildingDrainageOutlet(
            string outletId,
            double designFlow,
            int nominalDiameter,
            string pipeMaterialId,
            double minimumRequiredSlope,
            double selectedSlope,
            ElevationValue elevation,
            PipeElevationReference elevationReference,
            string connectionNote)
        {
            if (string.IsNullOrWhiteSpace(outletId))
                throw new ArgumentException(
                    "A kifolyás azonosítója nem lehet üres.",
                    nameof(outletId));

            if (designFlow < 0.0 ||
                double.IsNaN(designFlow) ||
                double.IsInfinity(designFlow))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(designFlow));
            }

            if (nominalDiameter <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(nominalDiameter));

            if (minimumRequiredSlope < 0.0 ||
                selectedSlope < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(selectedSlope));
            }

            OutletId = outletId.Trim();
            DesignFlow = designFlow;
            NominalDiameter = nominalDiameter;
            PipeMaterialId =
                pipeMaterialId?.Trim() ?? string.Empty;
            MinimumRequiredSlope = minimumRequiredSlope;
            SelectedSlope = selectedSlope;
            Elevation =
                elevation ??
                throw new ArgumentNullException(
                    nameof(elevation));
            ElevationReference = elevationReference;
            ConnectionNote =
                connectionNote?.Trim() ?? string.Empty;
        }

        public bool MeetsSlopeRequirement =>
            SelectedSlope >= MinimumRequiredSlope;
    }

    public sealed class SimpleDrainageProfileSegment
    {
        public string StartPointId { get; }
        public string EndPointId { get; }
        public double Length { get; }
        public double Slope { get; }
        public int NominalDiameter { get; }

        public ElevationValue StartInvertLevel { get; }
        public ElevationValue EndInvertLevel { get; }

        public double? GroundLevel { get; }
        public double? CoverDepth { get; }

        public SimpleDrainageProfileSegment(
            string startPointId,
            string endPointId,
            double length,
            double slope,
            int nominalDiameter,
            ElevationValue startInvertLevel,
            ElevationValue endInvertLevel,
            double? groundLevel,
            double? coverDepth)
        {
            if (string.IsNullOrWhiteSpace(startPointId) ||
                string.IsNullOrWhiteSpace(endPointId))
            {
                throw new ArgumentException(
                    "A kezdő- és végpont azonosítója kötelező.");
            }

            if (length <= 0.0 ||
                double.IsNaN(length) ||
                double.IsInfinity(length))
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (slope < 0.0 ||
                double.IsNaN(slope) ||
                double.IsInfinity(slope))
            {
                throw new ArgumentOutOfRangeException(nameof(slope));
            }

            if (nominalDiameter <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(nominalDiameter));

            StartPointId = startPointId.Trim();
            EndPointId = endPointId.Trim();
            Length = length;
            Slope = slope;
            NominalDiameter = nominalDiameter;
            StartInvertLevel =
                startInvertLevel ??
                throw new ArgumentNullException(
                    nameof(startInvertLevel));
            EndInvertLevel =
                endInvertLevel ??
                throw new ArgumentNullException(
                    nameof(endInvertLevel));
            GroundLevel = groundLevel;
            CoverDepth = coverDepth;
        }

        public double CalculateExpectedEndInvertLevel()
        {
            return StartInvertLevel.Value -
                   Length * Slope;
        }
    }
}
