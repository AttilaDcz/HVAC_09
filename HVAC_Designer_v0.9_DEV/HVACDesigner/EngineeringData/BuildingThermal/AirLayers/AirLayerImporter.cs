using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using HVACDesigner.EngineeringData.Abstractions;
using HVACDesigner.EngineeringData.Importing;
using HVACDesigner.EngineeringData.Infrastructure.Xml;
using HVACDesigner.EngineeringData.Registry;

namespace HVACDesigner.EngineeringData.BuildingThermal.AirLayers
{
    public sealed class AirLayerImportOutcome
    {
        public AirLayerCatalog Catalog { get; }
        public ContentSetImportResult ImportResult { get; }

        public AirLayerImportOutcome(
            AirLayerCatalog catalog,
            ContentSetImportResult importResult)
        {
            Catalog = catalog;
            ImportResult = importResult ??
                throw new ArgumentNullException(nameof(importResult));
        }
    }

    public sealed class AirLayerImporter
    {
        private static readonly HashSet<string> KnownAttributes =
            new HashSet<string>(
                new[]
                {
                    "Id",
                    "Name",
                    "ThermalRes",
                    "ThermalResistance",
                    "Orientation",
                    "VentilationLevel"
                },
                StringComparer.OrdinalIgnoreCase);

        public AirLayerImportOutcome Import(
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
                        "AIR_LAYER_CONTENT_MISSING",
                        "A légréteg-katalógus nem található.",
                        descriptor.ContentSetId));

                return Failure(descriptor, diagnostics);
            }

            var imported = new List<AirLayerDefinition>();
            var knownIds = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            try
            {
                using (DataPackageContent content =
                    source.OpenContentSet(manifest, descriptor))
                {
                    XDocument document = XDocument.Load(content.Stream);
                    XElement root = XmlDataPackageSource.FindElement(
                        document,
                        XmlDataPackageSource.GetXmlPath(descriptor.SourcePath));

                    if (root == null)
                    {
                        diagnostics.Add(
                            new ImportDiagnostic(
                                ImportDiagnosticSeverity.Error,
                                ImportFailureScope.ContentSet,
                                "AIR_LAYER_SECTION_MISSING",
                                "A MaterialCatalog XML-szekció nem található.",
                                descriptor.ContentSetId));

                        return Failure(descriptor, diagnostics);
                    }

                    int recordIndex = 0;

                    foreach (XElement element in root.Descendants("AirLayer"))
                    {
                        recordIndex++;

                        string id = Attribute(element, "Id");
                        string name = Attribute(element, "Name");
                        string resistanceText =
                            FirstNonEmpty(
                                Attribute(element, "ThermalRes"),
                                Attribute(element, "ThermalResistance"));

                        string recordId =
                            string.IsNullOrWhiteSpace(id)
                                ? $"record-{recordIndex}"
                                : id;

                        if (string.IsNullOrWhiteSpace(id) ||
                            string.IsNullOrWhiteSpace(name))
                        {
                            diagnostics.Add(
                                RecordError(
                                    "AIR_LAYER_REQUIRED_VALUE",
                                    "A légréteg Id és Name attribútuma kötelező.",
                                    descriptor.ContentSetId,
                                    recordId));
                            continue;
                        }

                        if (!double.TryParse(
                                resistanceText,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out double resistance) ||
                            resistance <= 0.0 ||
                            double.IsNaN(resistance) ||
                            double.IsInfinity(resistance))
                        {
                            diagnostics.Add(
                                RecordError(
                                    "AIR_LAYER_RESISTANCE_INVALID",
                                    "A légréteg hőellenállása pozitív szám legyen.",
                                    descriptor.ContentSetId,
                                    recordId));
                            continue;
                        }

                        if (!knownIds.Add(id))
                        {
                            diagnostics.Add(
                                RecordError(
                                    "AIR_LAYER_DUPLICATE_ID",
                                    "Duplikált légréteg-azonosító.",
                                    descriptor.ContentSetId,
                                    recordId));
                            continue;
                        }

                        IDictionary<string, string> metadata =
                            element.Attributes()
                                .Where(attribute =>
                                    !KnownAttributes.Contains(
                                        attribute.Name.LocalName))
                                .ToDictionary(
                                    attribute => attribute.Name.LocalName,
                                    attribute => attribute.Value,
                                    StringComparer.OrdinalIgnoreCase);

                        imported.Add(
                            new AirLayerDefinition(
                                id,
                                name,
                                resistance,
                                ParseOrientation(
                                    Attribute(element, "Orientation")),
                                ParseVentilation(
                                    Attribute(element, "VentilationLevel")),
                                manifest.PackageId,
                                descriptor.ContentSetId,
                                descriptor.ContentVersion,
                                metadata));
                    }
                }
            }
            catch (Exception exception)
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        ImportDiagnosticSeverity.Error,
                        ImportFailureScope.ContentSet,
                        "AIR_LAYER_IMPORT_FAILED",
                        "A légrétegek importja meghiúsult.",
                        descriptor.ContentSetId,
                        exception: exception));

                return Failure(descriptor, diagnostics);
            }

            var catalog = new AirLayerCatalog(
                descriptor.ContentSetId,
                descriptor.ContentVersion,
                imported);

            var result = new ContentSetImportResult(
                descriptor.ContentSetId,
                imported.Count,
                diagnostics.Count(item =>
                    item.Severity == ImportDiagnosticSeverity.Error &&
                    item.FailureScope == ImportFailureScope.Record),
                true,
                diagnostics);

            return new AirLayerImportOutcome(catalog, result);
        }

        public AirLayerImportOutcome ImportAndRegister(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source,
            EngineeringDataRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            AirLayerImportOutcome outcome =
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

        private static AirLayerOrientation ParseOrientation(string value)
        {
            if (Enum.TryParse(
                value,
                true,
                out AirLayerOrientation parsed))
            {
                return parsed;
            }

            return AirLayerOrientation.Unspecified;
        }

        private static AirLayerVentilationLevel ParseVentilation(string value)
        {
            if (Enum.TryParse(
                value,
                true,
                out AirLayerVentilationLevel parsed))
            {
                return parsed;
            }

            return AirLayerVentilationLevel.Unspecified;
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

        private static AirLayerImportOutcome Failure(
            ContentSetDescriptor descriptor,
            IEnumerable<ImportDiagnostic> diagnostics) =>
            new AirLayerImportOutcome(
                null,
                new ContentSetImportResult(
                    descriptor.ContentSetId,
                    0,
                    0,
                    false,
                    diagnostics));
    }
}
