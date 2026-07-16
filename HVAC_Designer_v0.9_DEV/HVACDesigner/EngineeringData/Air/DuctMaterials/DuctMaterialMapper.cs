using System;
using System.Globalization;
using HVACDesigner.EngineeringData.Importing;

namespace HVACDesigner.EngineeringData.Air.DuctMaterials
{
    public sealed class DuctMaterialMapResult
    {
        public DuctMaterialDefinition Value { get; }
        public ImportDiagnostic Diagnostic { get; }
        public bool Succeeded => Value != null;

        private DuctMaterialMapResult(
            DuctMaterialDefinition value,
            ImportDiagnostic diagnostic)
        {
            Value = value;
            Diagnostic = diagnostic;
        }

        public static DuctMaterialMapResult Success(
            DuctMaterialDefinition value)
        {
            return new DuctMaterialMapResult(
                value ??
                throw new ArgumentNullException(nameof(value)),
                null);
        }

        public static DuctMaterialMapResult Failure(
            ImportDiagnostic diagnostic)
        {
            return new DuctMaterialMapResult(
                null,
                diagnostic ??
                throw new ArgumentNullException(nameof(diagnostic)));
        }
    }

    /// <summary>
    /// DuctMaterialDto → DuctMaterialDefinition típusos mapper.
    /// </summary>
    public sealed class DuctMaterialMapper
    {
        public DuctMaterialMapResult Map(
            DuctMaterialDto dto,
            string packageId,
            string contentSetId,
            string version)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            string recordId =
                string.IsNullOrWhiteSpace(dto.Id)
                    ? $"record-{dto.SourceRecordIndex}"
                    : dto.Id.Trim();

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                return Fail(
                    "DUCT_MATERIAL_ID_REQUIRED",
                    "A légcsatorna-anyag Id attribútuma kötelező.",
                    contentSetId,
                    recordId,
                    "Id");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return Fail(
                    "DUCT_MATERIAL_NAME_REQUIRED",
                    "A légcsatorna-anyag neve kötelező.",
                    contentSetId,
                    recordId,
                    "Name");
            }

            if (!double.TryParse(
                dto.RoughnessMillimeters,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double roughnessMillimeters))
            {
                return Fail(
                    "DUCT_MATERIAL_ROUGHNESS_INVALID",
                    "Az érdesség nem értelmezhető számként.",
                    contentSetId,
                    recordId,
                    "Roughness");
            }

            if (roughnessMillimeters < 0.0 ||
                double.IsNaN(roughnessMillimeters) ||
                double.IsInfinity(roughnessMillimeters))
            {
                return Fail(
                    "DUCT_MATERIAL_ROUGHNESS_RANGE",
                    "Az érdesség nem lehet negatív vagy nem véges.",
                    contentSetId,
                    recordId,
                    "Roughness");
            }

            if (!bool.TryParse(
                dto.Flexible,
                out bool isFlexible))
            {
                return Fail(
                    "DUCT_MATERIAL_FLEXIBLE_INVALID",
                    "A Flexible attribútum csak true vagy false lehet.",
                    contentSetId,
                    recordId,
                    "Flexible");
            }

            double roughnessMeters =
                roughnessMillimeters / 1000.0;

            return DuctMaterialMapResult.Success(
                new DuctMaterialDefinition(
                    dto.Id.Trim(),
                    dto.Name.Trim(),
                    roughnessMeters,
                    isFlexible,
                    packageId,
                    contentSetId,
                    version));
        }

        private static DuctMaterialMapResult Fail(
            string code,
            string message,
            string contentSetId,
            string recordId,
            string propertyName)
        {
            return DuctMaterialMapResult.Failure(
                new ImportDiagnostic(
                    ImportDiagnosticSeverity.Error,
                    ImportFailureScope.Record,
                    code,
                    message,
                    contentSetId,
                    recordId,
                    propertyName));
        }
    }
}
