using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using HVACDesigner.EngineeringData.Abstractions;
using HVACDesigner.EngineeringData.Importing;
using HVACDesigner.EngineeringData.Infrastructure.Xml;
using HVACDesigner.EngineeringData.Registry;

namespace HVACDesigner.EngineeringData.BuildingThermal.Openings
{
    public sealed class BuildingOpeningImportOutcome
    {
        public BuildingOpeningCatalog Catalog { get; }
        public ContentSetImportResult ImportResult { get; }

        public BuildingOpeningImportOutcome(
            BuildingOpeningCatalog catalog,
            ContentSetImportResult importResult)
        {
            Catalog = catalog;
            ImportResult = importResult ??
                throw new ArgumentNullException(nameof(importResult));
        }
    }

    public sealed class BuildingOpeningImporter
    {
        public BuildingOpeningImportOutcome Import(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source)
        {
            if (manifest == null) throw new ArgumentNullException(nameof(manifest));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (source == null) throw new ArgumentNullException(nameof(source));

            var diagnostics = new List<ImportDiagnostic>();

            if (!source.ContentSetExists(manifest, descriptor))
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        descriptor.IsRequired
                            ? ImportDiagnosticSeverity.Error
                            : ImportDiagnosticSeverity.Warning,
                        ImportFailureScope.ContentSet,
                        "OPENING_CONTENT_MISSING",
                        "A nyílászáró-katalógus nem található.",
                        descriptor.ContentSetId));

                return Failure(descriptor, diagnostics);
            }

            var glazings = new List<GlazingDefinition>();
            var frames = new List<FrameDefinition>();
            var spacers = new List<SpacerDefinition>();
            var openings = new List<BuildingOpeningDefinition>();

            var glazingIds = NewIdSet();
            var frameIds = NewIdSet();
            var spacerIds = NewIdSet();
            var openingIds = NewIdSet();

            try
            {
                using (DataPackageContent content =
                    source.OpenContentSet(manifest, descriptor))
                {
                    XDocument document = XDocument.Load(content.Stream);

                    XElement root =
                        XmlDataPackageSource.FindElement(
                            document,
                            XmlDataPackageSource.GetXmlPath(
                                descriptor.SourcePath));

                    if (root == null)
                    {
                        diagnostics.Add(
                            new ImportDiagnostic(
                                ImportDiagnosticSeverity.Error,
                                ImportFailureScope.ContentSet,
                                "OPENING_SECTION_MISSING",
                                "A MaterialCatalog XML-szekció nem található.",
                                descriptor.ContentSetId));

                        return Failure(descriptor, diagnostics);
                    }

                    foreach (XElement element in root.Descendants("Glazing"))
                        ImportGlazing(element, glazings, glazingIds, diagnostics, descriptor);

                    foreach (XElement element in root.Descendants("Frame"))
                        ImportFrame(element, frames, frameIds, diagnostics, descriptor);

                    foreach (XElement element in root.Descendants("Spacer"))
                        ImportSpacer(element, spacers, spacerIds, diagnostics, descriptor);

                    foreach (XElement element in root.Descendants("Default"))
                        ImportOpening(element, openings, openingIds, diagnostics, descriptor);
                }
            }
            catch (Exception exception)
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        ImportDiagnosticSeverity.Error,
                        ImportFailureScope.ContentSet,
                        "OPENING_IMPORT_FAILED",
                        "A nyílászáró-katalógus importja meghiúsult.",
                        descriptor.ContentSetId,
                        exception: exception));

                return Failure(descriptor, diagnostics);
            }

            var catalog = new BuildingOpeningCatalog(
                descriptor.ContentSetId,
                descriptor.ContentVersion,
                glazings,
                frames,
                spacers,
                openings);

            var result = new ContentSetImportResult(
                descriptor.ContentSetId,
                glazings.Count +
                frames.Count +
                spacers.Count +
                openings.Count,
                diagnostics.Count(item =>
                    item.Severity == ImportDiagnosticSeverity.Error &&
                    item.FailureScope == ImportFailureScope.Record),
                true,
                diagnostics);

            return new BuildingOpeningImportOutcome(catalog, result);
        }

        public BuildingOpeningImportOutcome ImportAndRegister(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source,
            EngineeringDataRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            BuildingOpeningImportOutcome outcome =
                Import(manifest, descriptor, source);

            if (outcome.Catalog != null)
            {
                registry.Register(
                    manifest.PackageId,
                    descriptor.ContentSetId,
                    descriptor.ContentVersion,
                    descriptor.ContentKind,
                    outcome.Catalog);
            }

            return outcome;
        }

        private static void ImportGlazing(
            XElement element,
            ICollection<GlazingDefinition> target,
            ISet<string> ids,
            ICollection<ImportDiagnostic> diagnostics,
            ContentSetDescriptor descriptor)
        {
            string id = Attribute(element, "Id");
            string name = Attribute(element, "Name");

            if (!TryPositive(Attribute(element, "Ug"), out double ug) ||
                !TryFraction(
                    FirstNonEmpty(
                        Attribute(element, "GValue"),
                        Attribute(element, "g")),
                    out double g))
            {
                diagnostics.Add(RecordError(
                    "GLAZING_VALUE_INVALID",
                    "Az üvegezés Ug és g értéke érvénytelen.",
                    descriptor.ContentSetId,
                    id));
                return;
            }

            if (!TryAddId(ids, id, diagnostics, descriptor, "GLAZING"))
                return;

            double? visible =
                TryOptionalFraction(
                    Attribute(element, "VisibleTransmittance"));

            int? paneCount =
                TryOptionalPositiveInteger(
                    Attribute(element, "PaneCount"));

            target.Add(
                new GlazingDefinition(
                    id,
                    name,
                    ug,
                    g,
                    visible,
                    paneCount,
                    Attribute(element, "GasFill"),
                    Attribute(element, "CoatingType")));
        }

        private static void ImportFrame(
            XElement element,
            ICollection<FrameDefinition> target,
            ISet<string> ids,
            ICollection<ImportDiagnostic> diagnostics,
            ContentSetDescriptor descriptor)
        {
            string id = Attribute(element, "Id");

            if (!TryPositive(Attribute(element, "Uf"), out double uf))
            {
                diagnostics.Add(RecordError(
                    "FRAME_VALUE_INVALID",
                    "A keret Uf értéke érvénytelen.",
                    descriptor.ContentSetId,
                    id));
                return;
            }

            if (!TryAddId(ids, id, diagnostics, descriptor, "FRAME"))
                return;

            target.Add(
                new FrameDefinition(
                    id,
                    Attribute(element, "Name"),
                    uf,
                    ParseFrameMaterial(
                        Attribute(element, "Material")),
                    TryOptionalPositiveDouble(
                        Attribute(element, "ProfileDepthMm"),
                        0.001),
                    TryOptionalPositiveInteger(
                        Attribute(element, "ChamberCount")),
                    TryOptionalPositiveDouble(
                        Attribute(element, "VisibleWidthMm"),
                        0.001)));
        }

        private static void ImportSpacer(
            XElement element,
            ICollection<SpacerDefinition> target,
            ISet<string> ids,
            ICollection<ImportDiagnostic> diagnostics,
            ContentSetDescriptor descriptor)
        {
            string id = Attribute(element, "Id");

            if (!TryNonNegative(
                FirstNonEmpty(
                    Attribute(element, "Psi"),
                    Attribute(element, "PsiValue")),
                out double psi))
            {
                diagnostics.Add(RecordError(
                    "SPACER_VALUE_INVALID",
                    "A távtartó Ψ értéke érvénytelen.",
                    descriptor.ContentSetId,
                    id));
                return;
            }

            if (!TryAddId(ids, id, diagnostics, descriptor, "SPACER"))
                return;

            target.Add(
                new SpacerDefinition(
                    id,
                    Attribute(element, "Name"),
                    psi,
                    Attribute(element, "SpacerType")));
        }

        private static void ImportOpening(
            XElement element,
            ICollection<BuildingOpeningDefinition> target,
            ISet<string> ids,
            ICollection<ImportDiagnostic> diagnostics,
            ContentSetDescriptor descriptor)
        {
            string id = Attribute(element, "Id");

            if (!TryPositive(
                FirstNonEmpty(
                    Attribute(element, "Uw"),
                    Attribute(element, "UValue")),
                out double uValue))
            {
                diagnostics.Add(RecordError(
                    "OPENING_UVALUE_INVALID",
                    "A nyílászáró U-értéke érvénytelen.",
                    descriptor.ContentSetId,
                    id));
                return;
            }

            double? gValue = null;
            string gText =
                FirstNonEmpty(
                    Attribute(element, "GValue"),
                    Attribute(element, "g"));

            if (!string.IsNullOrWhiteSpace(gText))
            {
                if (!TryFraction(gText, out double parsedG))
                {
                    diagnostics.Add(RecordError(
                        "OPENING_GVALUE_INVALID",
                        "A nyílászáró g-értéke érvénytelen.",
                        descriptor.ContentSetId,
                        id));
                    return;
                }

                gValue = parsedG;
            }

            if (!TryAddId(ids, id, diagnostics, descriptor, "OPENING"))
                return;

            IDictionary<string, string> metadata =
                element.Attributes().ToDictionary(
                    attribute => attribute.Name.LocalName,
                    attribute => attribute.Value,
                    StringComparer.OrdinalIgnoreCase);

            target.Add(
                new BuildingOpeningDefinition(
                    id,
                    Attribute(element, "Name"),
                    ParseOpeningType(
                        FirstNonEmpty(
                            Attribute(element, "OpeningType"),
                            Attribute(element, "Type"))),
                    ParseCalculationMode(
                        Attribute(element, "CalculationMode")),
                    uValue,
                    gValue,
                    Attribute(element, "GlazingId"),
                    Attribute(element, "FrameId"),
                    Attribute(element, "SpacerId"),
                    TryOptionalPositiveDouble(
                        Attribute(element, "FrameWidthMm"),
                        0.001),
                    Attribute(element, "AirLeakageClass"),
                    metadata));
        }

        private static bool TryAddId(
            ISet<string> ids,
            string id,
            ICollection<ImportDiagnostic> diagnostics,
            ContentSetDescriptor descriptor,
            string prefix)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                diagnostics.Add(RecordError(
                    prefix + "_ID_REQUIRED",
                    "Az Id attribútum kötelező.",
                    descriptor.ContentSetId,
                    "unknown"));
                return false;
            }

            if (!ids.Add(id))
            {
                diagnostics.Add(RecordError(
                    prefix + "_DUPLICATE_ID",
                    "Duplikált azonosító.",
                    descriptor.ContentSetId,
                    id));
                return false;
            }

            return true;
        }

        private static HashSet<string> NewIdSet() =>
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static BuildingOpeningType ParseOpeningType(string value)
        {
            if (Enum.TryParse(
                value,
                true,
                out BuildingOpeningType parsed))
            {
                return parsed;
            }

            return BuildingOpeningType.Window;
        }

        private static BuildingOpeningCalculationMode
            ParseCalculationMode(string value)
        {
            if (Enum.TryParse(
                value,
                true,
                out BuildingOpeningCalculationMode parsed))
            {
                return parsed;
            }

            return BuildingOpeningCalculationMode.DirectValue;
        }

        private static FrameMaterialKind ParseFrameMaterial(string value)
        {
            if (Enum.TryParse(
                value,
                true,
                out FrameMaterialKind parsed))
            {
                return parsed;
            }

            return FrameMaterialKind.Unspecified;
        }

        private static bool TryPositive(string value, out double result) =>
            double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out result) &&
            result > 0.0 &&
            !double.IsNaN(result) &&
            !double.IsInfinity(result);

        private static bool TryNonNegative(string value, out double result) =>
            double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out result) &&
            result >= 0.0 &&
            !double.IsNaN(result) &&
            !double.IsInfinity(result);

        private static bool TryFraction(string value, out double result) =>
            double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out result) &&
            result >= 0.0 &&
            result <= 1.0 &&
            !double.IsNaN(result) &&
            !double.IsInfinity(result);

        private static double? TryOptionalFraction(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return TryFraction(value, out double parsed)
                ? parsed
                : (double?)null;
        }

        private static int? TryOptionalPositiveInteger(string value)
        {
            if (int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int parsed) &&
                parsed > 0)
            {
                return parsed;
            }

            return null;
        }

        private static double? TryOptionalPositiveDouble(
            string value,
            double multiplier)
        {
            if (TryPositive(value, out double parsed))
                return parsed * multiplier;

            return null;
        }

        private static ImportDiagnostic RecordError(
            string code,
            string message,
            string contentSetId,
            string recordId) =>
            new ImportDiagnostic(
                ImportDiagnosticSeverity.Error,
                ImportFailureScope.Record,
                code,
                message,
                contentSetId,
                recordId);

        private static string Attribute(
            XElement element,
            string name) =>
            (string)element.Attribute(name) ?? string.Empty;

        private static string FirstNonEmpty(
            string first,
            string second) =>
            !string.IsNullOrWhiteSpace(first) ? first : second;

        private static BuildingOpeningImportOutcome Failure(
            ContentSetDescriptor descriptor,
            IEnumerable<ImportDiagnostic> diagnostics) =>
            new BuildingOpeningImportOutcome(
                null,
                new ContentSetImportResult(
                    descriptor.ContentSetId,
                    0,
                    0,
                    false,
                    diagnostics));
    }
}
