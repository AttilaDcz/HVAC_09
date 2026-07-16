using System;
using System.Globalization;
using HVACDesigner.EngineeringData.Importing;

namespace HVACDesigner.EngineeringData.Air.DuctSizes
{
    public sealed class DuctSizeMapResult<T> where T : class
    {
        public T Value { get; }
        public ImportDiagnostic Diagnostic { get; }
        public bool Succeeded => Value != null;

        private DuctSizeMapResult(T value, ImportDiagnostic diagnostic)
        {
            Value = value;
            Diagnostic = diagnostic;
        }

        public static DuctSizeMapResult<T> Success(T value) =>
            new DuctSizeMapResult<T>(
                value ?? throw new ArgumentNullException(nameof(value)),
                null);

        public static DuctSizeMapResult<T> Failure(ImportDiagnostic diagnostic) =>
            new DuctSizeMapResult<T>(
                null,
                diagnostic ?? throw new ArgumentNullException(nameof(diagnostic)));
    }

    public sealed class CircularDuctSizeMapper
    {
        public DuctSizeMapResult<CircularDuctSizeDefinition> Map(
            CircularDuctSizeDto dto,
            string packageId,
            string contentSetId,
            string version)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (!int.TryParse(
                    dto.DiameterMillimeters,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int diameter) ||
                diameter <= 0)
            {
                return DuctSizeMapResult<CircularDuctSizeDefinition>.Failure(
                    Invalid(
                        "DUCT_SIZE_DIAMETER_INVALID",
                        "A kör légcsatorna átmérője pozitív egész milliméterérték legyen.",
                        contentSetId,
                        dto.SourceRecordIndex,
                        "Diameter"));
            }

            return DuctSizeMapResult<CircularDuctSizeDefinition>.Success(
                new CircularDuctSizeDefinition(
                    diameter,
                    packageId,
                    contentSetId,
                    version));
        }

        private static ImportDiagnostic Invalid(
            string code,
            string message,
            string contentSetId,
            int recordIndex,
            string propertyName) =>
            new ImportDiagnostic(
                ImportDiagnosticSeverity.Error,
                ImportFailureScope.Record,
                code,
                message,
                contentSetId,
                $"record-{recordIndex}",
                propertyName);
    }

    public sealed class RectangularDuctSizeMapper
    {
        public DuctSizeMapResult<RectangularDuctSizeDefinition> Map(
            RectangularDuctSizeDto dto,
            string packageId,
            string contentSetId,
            string version)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (!TryPositive(dto.WidthMillimeters, out int width))
                return Failure(
                    "DUCT_SIZE_WIDTH_INVALID",
                    "A szélesség pozitív egész milliméterérték legyen.",
                    contentSetId,
                    dto.SourceRecordIndex,
                    "Width");

            if (!TryPositive(dto.HeightMillimeters, out int height))
                return Failure(
                    "DUCT_SIZE_HEIGHT_INVALID",
                    "A magasság pozitív egész milliméterérték legyen.",
                    contentSetId,
                    dto.SourceRecordIndex,
                    "Height");

            return DuctSizeMapResult<RectangularDuctSizeDefinition>.Success(
                new RectangularDuctSizeDefinition(
                    width,
                    height,
                    packageId,
                    contentSetId,
                    version));
        }

        private static bool TryPositive(string value, out int result) =>
            int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out result) &&
            result > 0;

        private static DuctSizeMapResult<RectangularDuctSizeDefinition> Failure(
            string code,
            string message,
            string contentSetId,
            int recordIndex,
            string propertyName) =>
            DuctSizeMapResult<RectangularDuctSizeDefinition>.Failure(
                new ImportDiagnostic(
                    ImportDiagnosticSeverity.Error,
                    ImportFailureScope.Record,
                    code,
                    message,
                    contentSetId,
                    $"record-{recordIndex}",
                    propertyName));
    }
}
