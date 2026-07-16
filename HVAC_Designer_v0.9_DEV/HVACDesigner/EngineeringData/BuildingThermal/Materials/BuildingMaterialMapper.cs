using System;
using System.Globalization;
using HVACDesigner.EngineeringData.Importing;

namespace HVACDesigner.EngineeringData.BuildingThermal.Materials
{
    public sealed class BuildingMaterialMapResult
    {
        public BuildingMaterialDefinition Value { get; }
        public ImportDiagnostic Diagnostic { get; }
        public bool Succeeded => Value != null;

        private BuildingMaterialMapResult(
            BuildingMaterialDefinition value,
            ImportDiagnostic diagnostic)
        {
            Value = value;
            Diagnostic = diagnostic;
        }

        public static BuildingMaterialMapResult Success(
            BuildingMaterialDefinition value) =>
            new BuildingMaterialMapResult(
                value ?? throw new ArgumentNullException(nameof(value)),
                null);

        public static BuildingMaterialMapResult Failure(
            ImportDiagnostic diagnostic) =>
            new BuildingMaterialMapResult(
                null,
                diagnostic ??
                throw new ArgumentNullException(nameof(diagnostic)));
    }

    public sealed class BuildingMaterialMapper
    {
        private const double KilojouleToJoule = 1000.0;

        public BuildingMaterialMapResult Map(
            BuildingMaterialDto dto,
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
                return Fail(
                    "BUILDING_MATERIAL_ID_REQUIRED",
                    "Az építőanyag Id attribútuma kötelező.",
                    contentSetId,
                    recordId,
                    "Id");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return Fail(
                    "BUILDING_MATERIAL_NAME_REQUIRED",
                    "Az építőanyag neve kötelező.",
                    contentSetId,
                    recordId,
                    "Name");

            if (string.IsNullOrWhiteSpace(dto.Category))
                return Fail(
                    "BUILDING_MATERIAL_CATEGORY_REQUIRED",
                    "Az építőanyag kategóriája kötelező.",
                    contentSetId,
                    recordId,
                    "Category");

            if (!TryPositiveDouble(dto.Lambda, out double lambda))
                return InvalidNumber(
                    "BUILDING_MATERIAL_LAMBDA_INVALID",
                    "A Lambda pozitív szám legyen.",
                    contentSetId,
                    recordId,
                    "Lambda");

            double correction = 1.0;

            if (!string.IsNullOrWhiteSpace(dto.LambdaCorrection) &&
                !TryPositiveDouble(
                    dto.LambdaCorrection,
                    out correction))
            {
                return InvalidNumber(
                    "BUILDING_MATERIAL_LAMBDA_CORRECTION_INVALID",
                    "A LambdaCorrection pozitív szám legyen.",
                    contentSetId,
                    recordId,
                    "LambdaCorrection");
            }

            if (!TryPositiveDouble(dto.Density, out double density))
                return InvalidNumber(
                    "BUILDING_MATERIAL_DENSITY_INVALID",
                    "A Density pozitív szám legyen.",
                    contentSetId,
                    recordId,
                    "Density");

            if (!TryPositiveDouble(
                dto.SpecificHeatKilojoules,
                out double specificHeatKilojoules))
            {
                return InvalidNumber(
                    "BUILDING_MATERIAL_SPECIFIC_HEAT_INVALID",
                    "A SpecificHeat pozitív szám legyen.",
                    contentSetId,
                    recordId,
                    "SpecificHeat");
            }

            if (!TryPositiveDouble(dto.Mu, out double mu))
                return InvalidNumber(
                    "BUILDING_MATERIAL_MU_INVALID",
                    "A Mu pozitív szám legyen.",
                    contentSetId,
                    recordId,
                    "Mu");

            BuildingMaterialValueSource valueSource =
                ParseValueSource(dto.ValueSource);

            BuildingMaterialMoistureCondition moistureCondition =
                ParseMoistureCondition(dto.MoistureCondition);

            return BuildingMaterialMapResult.Success(
                new BuildingMaterialDefinition(
                    dto.Id.Trim(),
                    dto.Name.Trim(),
                    dto.Category.Trim(),
                    lambda,
                    correction,
                    density,
                    specificHeatKilojoules * KilojouleToJoule,
                    mu,
                    valueSource,
                    moistureCondition,
                    dto.StandardReference,
                    dto.Description,
                    packageId,
                    contentSetId,
                    version,
                    dto.Metadata));
        }

        private static BuildingMaterialValueSource ParseValueSource(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return BuildingMaterialValueSource.Unspecified;

            if (Enum.TryParse(
                value,
                true,
                out BuildingMaterialValueSource parsed))
            {
                return parsed;
            }

            return BuildingMaterialValueSource.Unspecified;
        }

        private static BuildingMaterialMoistureCondition
            ParseMoistureCondition(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return BuildingMaterialMoistureCondition.Normal;

            if (Enum.TryParse(
                value,
                true,
                out BuildingMaterialMoistureCondition parsed))
            {
                return parsed;
            }

            return BuildingMaterialMoistureCondition.Unspecified;
        }

        private static bool TryPositiveDouble(
            string value,
            out double result)
        {
            return double.TryParse(
                       value,
                       NumberStyles.Float,
                       CultureInfo.InvariantCulture,
                       out result) &&
                   result > 0.0 &&
                   !double.IsNaN(result) &&
                   !double.IsInfinity(result);
        }

        private static BuildingMaterialMapResult InvalidNumber(
            string code,
            string message,
            string contentSetId,
            string recordId,
            string propertyName) =>
            Fail(
                code,
                message,
                contentSetId,
                recordId,
                propertyName);

        private static BuildingMaterialMapResult Fail(
            string code,
            string message,
            string contentSetId,
            string recordId,
            string propertyName) =>
            BuildingMaterialMapResult.Failure(
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
