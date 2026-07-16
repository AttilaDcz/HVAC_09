using System;
using System.Collections.Generic;
using System.Globalization;
using HVACDesigner.EngineeringData.Importing;

namespace HVACDesigner.EngineeringData.Air.DuctElements
{
    public sealed class DuctElementDefinitionMapResult
    {
        public DuctElementDefinition Value { get; }
        public ImportDiagnostic Diagnostic { get; }
        public bool Succeeded => Value != null;

        private DuctElementDefinitionMapResult(
            DuctElementDefinition value,
            ImportDiagnostic diagnostic)
        {
            Value = value;
            Diagnostic = diagnostic;
        }

        public static DuctElementDefinitionMapResult Success(
            DuctElementDefinition value) =>
            new DuctElementDefinitionMapResult(
                value ?? throw new ArgumentNullException(nameof(value)),
                null);

        public static DuctElementDefinitionMapResult Failure(
            ImportDiagnostic diagnostic) =>
            new DuctElementDefinitionMapResult(
                null,
                diagnostic ?? throw new ArgumentNullException(nameof(diagnostic)));
    }

    public sealed class DuctElementDefinitionMapper
    {
        public DuctElementDefinitionMapResult Map(
            DuctElementDefinitionDto dto,
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
                    "DUCT_ELEMENT_ID_REQUIRED",
                    "Az elem Id attribútuma kötelező.",
                    contentSetId,
                    recordId,
                    "Id");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return Fail(
                    "DUCT_ELEMENT_NAME_REQUIRED",
                    "Az elem neve kötelező.",
                    contentSetId,
                    recordId,
                    "Name");

            if (!TryParseNonNegativeDouble(
                dto.DefaultZeta,
                out double defaultZeta))
            {
                return Fail(
                    "DUCT_ELEMENT_ZETA_INVALID",
                    "A DefaultZeta nemnegatív szám legyen.",
                    contentSetId,
                    recordId,
                    "DefaultZeta");
            }

            if (!TryParseOptionalBoolean(
                dto.AllowSizeChange,
                out bool allowSizeChange) ||
                !TryParseOptionalBoolean(
                    dto.AllowLength,
                    out bool allowLength) ||
                !TryParseOptionalBoolean(
                    dto.AllowRadius,
                    out bool allowRadius) ||
                !TryParseOptionalBoolean(
                    dto.AllowBranch,
                    out bool allowBranch))
            {
                return Fail(
                    "DUCT_ELEMENT_BOOLEAN_INVALID",
                    "Az Allow* attribútumok csak true vagy false értéket kaphatnak.",
                    contentSetId,
                    recordId,
                    "Allow*");
            }

            DuctElementCategory category =
                ParseCategory(
                    dto.Category,
                    dto.SourceSectionPath,
                    dto.SourceElementName);

            DuctGeometryKind geometry =
                ParseGeometry(
                    dto.GeometryType,
                    dto.SourceSectionPath);

            DuctFlowDirection flowDirection =
                ParseFlowDirection(dto.FlowDirection);

            DuctPressureModel pressureModel =
                ParsePressureModel(dto.PressureModel);

            return DuctElementDefinitionMapResult.Success(
                new DuctElementDefinition(
                    dto.Id.Trim(),
                    dto.Name.Trim(),
                    category,
                    geometry,
                    flowDirection,
                    pressureModel,
                    dto.Material,
                    defaultZeta,
                    allowSizeChange,
                    allowLength,
                    allowRadius,
                    allowBranch,
                    dto.SourceElementName,
                    dto.SourceSectionPath,
                    packageId,
                    contentSetId,
                    version,
                    dto.Metadata));
        }

        private static DuctElementCategory ParseCategory(
            string value,
            string sectionPath,
            string elementName)
        {
            string normalized = Normalize(value);

            switch (normalized)
            {
                case "ELBOW": return DuctElementCategory.Elbow;
                case "REDUCER": return DuctElementCategory.Reducer;
                case "EXPANDER": return DuctElementCategory.Expander;
                case "TRANSITION": return DuctElementCategory.Transition;
                case "BRANCH": return DuctElementCategory.Branch;
                case "DAMPER": return DuctElementCategory.Damper;
                case "ACCESSORY": return DuctElementCategory.Accessory;
                case "LOUVER": return DuctElementCategory.Louver;
                case "AIRTERMINAL": return DuctElementCategory.AirTerminal;
                case "FLEXIBLEDUCT": return DuctElementCategory.FlexibleDuct;
                case "CUSTOM": return DuctElementCategory.Custom;
            }

            string path = Normalize(sectionPath);

            if (path.Contains("STRAIGHTDUCTS"))
                return DuctElementCategory.StraightDuct;
            if (path.Contains("FLEXIBLEDUCTS"))
                return DuctElementCategory.FlexibleDuct;
            if (path.Contains("DAMPERS"))
                return DuctElementCategory.Damper;
            if (path.Contains("ACCESSORIES"))
                return DuctElementCategory.Accessory;
            if (path.Contains("LOUVERS"))
                return DuctElementCategory.Louver;
            if (path.Contains("CUSTOM"))
                return DuctElementCategory.Custom;

            if (Normalize(elementName) == "DUCT")
                return DuctElementCategory.StraightDuct;

            return DuctElementCategory.Other;
        }

        private static DuctGeometryKind ParseGeometry(
            string value,
            string sectionPath)
        {
            switch (Normalize(value))
            {
                case "CIRCULAR": return DuctGeometryKind.Circular;
                case "RECTANGULAR": return DuctGeometryKind.Rectangular;
                case "SINGLESIZE": return DuctGeometryKind.SingleSize;
                case "SIZECHANGE": return DuctGeometryKind.SizeChange;
                case "SHAPECHANGE": return DuctGeometryKind.ShapeChange;
                case "THREEWAY": return DuctGeometryKind.ThreeWay;
                case "FOURWAY": return DuctGeometryKind.FourWay;
                case "FREEAREA": return DuctGeometryKind.FreeArea;
                case "CUSTOM": return DuctGeometryKind.Custom;
            }

            string path = Normalize(sectionPath);

            if (path.Contains("CIRCULAR"))
                return DuctGeometryKind.Circular;

            if (path.Contains("RECTANGULAR"))
                return DuctGeometryKind.Rectangular;

            return DuctGeometryKind.Unspecified;
        }

        private static DuctFlowDirection ParseFlowDirection(
            string value)
        {
            switch (Normalize(value))
            {
                case "FORWARD": return DuctFlowDirection.Forward;
                case "REVERSE": return DuctFlowDirection.Reverse;
                case "BOTH": return DuctFlowDirection.Both;
                default: return DuctFlowDirection.Unspecified;
            }
        }

        private static DuctPressureModel ParsePressureModel(
            string value)
        {
            switch (Normalize(value))
            {
                case "FRICTION": return DuctPressureModel.Friction;
                case "ZETA": return DuctPressureModel.Zeta;
                case "FIXEDPRESSURE":
                case "FIXED":
                    return DuctPressureModel.FixedPressure;
                case "MANUFACTURERDATA":
                    return DuctPressureModel.ManufacturerData;
                default:
                    return DuctPressureModel.Unspecified;
            }
        }

        private static bool TryParseNonNegativeDouble(
            string value,
            out double result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = 0.0;
                return true;
            }

            return double.TryParse(
                       value,
                       NumberStyles.Float,
                       CultureInfo.InvariantCulture,
                       out result) &&
                   result >= 0.0 &&
                   !double.IsNaN(result) &&
                   !double.IsInfinity(result);
        }

        private static bool TryParseOptionalBoolean(
            string value,
            out bool result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = false;
                return true;
            }

            return bool.TryParse(value, out result);
        }

        private static string Normalize(string value) =>
            (value ?? string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace(" ", string.Empty)
                .ToUpperInvariant();

        private static DuctElementDefinitionMapResult Fail(
            string code,
            string message,
            string contentSetId,
            string recordId,
            string propertyName) =>
            DuctElementDefinitionMapResult.Failure(
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
