using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using HVACDesigner.EngineeringData.Abstractions;
using HVACDesigner.EngineeringData.BuildingThermal.AirLayers;
using HVACDesigner.EngineeringData.BuildingThermal.Materials;
using HVACDesigner.EngineeringData.Importing;
using HVACDesigner.EngineeringData.Infrastructure.Xml;
using HVACDesigner.EngineeringData.Registry;

namespace HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates
{
    public sealed class ConstructionTemplateImportOutcome
    {
        public ConstructionTemplateCatalog Catalog { get; }
        public ContentSetImportResult ImportResult { get; }

        public ConstructionTemplateImportOutcome(
            ConstructionTemplateCatalog catalog,
            ContentSetImportResult importResult)
        {
            Catalog = catalog;
            ImportResult = importResult ??
                throw new ArgumentNullException(nameof(importResult));
        }
    }

    public sealed class ConstructionTemplateImporter
    {
        private readonly ConstructionTemplateValidator _validator =
            new ConstructionTemplateValidator();

        public ConstructionTemplateImportOutcome Import(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source,
            BuildingMaterialCatalog materialCatalog,
            AirLayerCatalog airLayerCatalog)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var diagnostics = new List<ImportDiagnostic>();

            if (!source.ContentSetExists(manifest, descriptor))
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        descriptor.IsRequired
                            ? ImportDiagnosticSeverity.Error
                            : ImportDiagnosticSeverity.Warning,
                        ImportFailureScope.ContentSet,
                        "CONSTRUCTION_CONTENT_MISSING",
                        "A szerkezetsablonok nem találhatók.",
                        descriptor.ContentSetId));

                return Failure(descriptor, diagnostics);
            }

            var imported = new List<ConstructionTemplateDefinition>();
            var knownIds = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

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
                                "CONSTRUCTION_SECTION_MISSING",
                                "A PredefinedStructures XML-szekció nem található.",
                                descriptor.ContentSetId));

                        return Failure(descriptor, diagnostics);
                    }

                    int recordIndex = 0;

                    foreach (XElement structure in
                        root.Elements("Structure"))
                    {
                        recordIndex++;

                        string id =
                            (string)structure.Attribute("Id") ??
                            string.Empty;

                        string name =
                            (string)structure.Attribute("Name") ??
                            string.Empty;

                        string recordId =
                            string.IsNullOrWhiteSpace(id)
                                ? $"record-{recordIndex}"
                                : id;

                        if (string.IsNullOrWhiteSpace(id) ||
                            string.IsNullOrWhiteSpace(name))
                        {
                            diagnostics.Add(
                                RecordError(
                                    "CONSTRUCTION_REQUIRED_VALUE",
                                    "A szerkezetsablon Id és Name attribútuma kötelező.",
                                    descriptor.ContentSetId,
                                    recordId));
                            continue;
                        }

                        if (!knownIds.Add(id))
                        {
                            diagnostics.Add(
                                RecordError(
                                    "CONSTRUCTION_DUPLICATE_ID",
                                    "Duplikált szerkezetsablon-azonosító.",
                                    descriptor.ContentSetId,
                                    recordId));
                            continue;
                        }

                        var layers =
                            ParseLayers(
                                structure.Elements("Layer"),
                                descriptor,
                                recordId,
                                diagnostics);

                        var paths =
                            ParsePaths(
                                structure.Elements("Path"),
                                descriptor,
                                recordId,
                                diagnostics);

                        ConstructionTemplateDefinition template;

                        try
                        {
                            template =
                                new ConstructionTemplateDefinition(
                                    id,
                                    name,
                                    ParseConstructionType(
                                        (string)structure.Attribute("Type")),
                                    layers,
                                    paths,
                                    (string)structure.Attribute("ValueSource"),
                                    (string)structure.Attribute("StandardReference"),
                                    (string)structure.Attribute("Notes"),
                                    manifest.PackageId,
                                    descriptor.ContentSetId,
                                    descriptor.ContentVersion);
                        }
                        catch (Exception exception)
                        {
                            diagnostics.Add(
                                new ImportDiagnostic(
                                    ImportDiagnosticSeverity.Error,
                                    ImportFailureScope.Record,
                                    "CONSTRUCTION_MODEL_INVALID",
                                    exception.Message,
                                    descriptor.ContentSetId,
                                    recordId,
                                    exception: exception));
                            continue;
                        }

                        ConstructionTemplateValidationResult validation =
                            _validator.Validate(
                                template,
                                materialCatalog,
                                airLayerCatalog);

                        if (!validation.IsValid)
                        {
                            foreach (string error in validation.Errors)
                            {
                                diagnostics.Add(
                                    RecordError(
                                        "CONSTRUCTION_REFERENCE_INVALID",
                                        error,
                                        descriptor.ContentSetId,
                                        recordId));
                            }
                            continue;
                        }

                        foreach (string warning in validation.Warnings)
                        {
                            diagnostics.Add(
                                new ImportDiagnostic(
                                    ImportDiagnosticSeverity.Warning,
                                    ImportFailureScope.Property,
                                    "CONSTRUCTION_WARNING",
                                    warning,
                                    descriptor.ContentSetId,
                                    recordId));
                        }

                        imported.Add(template);
                    }
                }
            }
            catch (Exception exception)
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        ImportDiagnosticSeverity.Error,
                        ImportFailureScope.ContentSet,
                        "CONSTRUCTION_IMPORT_FAILED",
                        "A szerkezetsablonok importja meghiúsult.",
                        descriptor.ContentSetId,
                        exception: exception));

                return Failure(descriptor, diagnostics);
            }

            var catalog =
                new ConstructionTemplateCatalog(
                    descriptor.ContentSetId,
                    descriptor.ContentVersion,
                    imported);

            var result =
                new ContentSetImportResult(
                    descriptor.ContentSetId,
                    imported.Count,
                    diagnostics.Count(item =>
                        item.Severity ==
                        ImportDiagnosticSeverity.Error &&
                        item.FailureScope ==
                        ImportFailureScope.Record),
                    true,
                    diagnostics);

            return new ConstructionTemplateImportOutcome(
                catalog,
                result);
        }

        public ConstructionTemplateImportOutcome ImportAndRegister(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source,
            BuildingMaterialCatalog materialCatalog,
            AirLayerCatalog airLayerCatalog,
            EngineeringDataRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            ConstructionTemplateImportOutcome outcome =
                Import(
                    manifest,
                    descriptor,
                    source,
                    materialCatalog,
                    airLayerCatalog);

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

        private static List<ConstructionLayerDefinition> ParseLayers(
            IEnumerable<XElement> elements,
            ContentSetDescriptor descriptor,
            string recordId,
            ICollection<ImportDiagnostic> diagnostics)
        {
            var layers = new List<ConstructionLayerDefinition>();
            int order = 0;

            foreach (XElement element in elements)
            {
                order++;

                ConstructionLayerDefinition layer =
                    ParseLayer(
                        element,
                        order,
                        descriptor,
                        recordId,
                        diagnostics);

                if (layer != null)
                    layers.Add(layer);
            }

            return layers;
        }

        private static List<ConstructionPathDefinition> ParsePaths(
            IEnumerable<XElement> elements,
            ContentSetDescriptor descriptor,
            string recordId,
            ICollection<ImportDiagnostic> diagnostics)
        {
            var paths = new List<ConstructionPathDefinition>();
            int pathIndex = 0;

            foreach (XElement pathElement in elements)
            {
                pathIndex++;

                string pathId =
                    (string)pathElement.Attribute("Id") ??
                    $"path-{pathIndex}";

                if (!TryPositiveFraction(
                    (string)pathElement.Attribute("AreaFraction"),
                    out double areaFraction))
                {
                    diagnostics.Add(
                        RecordError(
                            "CONSTRUCTION_PATH_AREA_INVALID",
                            "A párhuzamos útvonal AreaFraction értéke 0 és 1 közötti legyen.",
                            descriptor.ContentSetId,
                            recordId));
                    continue;
                }

                List<ConstructionLayerDefinition> layers =
                    ParseLayers(
                        pathElement.Elements("Layer"),
                        descriptor,
                        recordId,
                        diagnostics);

                if (layers.Count == 0)
                    continue;

                paths.Add(
                    new ConstructionPathDefinition(
                        pathId,
                        (string)pathElement.Attribute("Name"),
                        areaFraction,
                        layers));
            }

            return paths;
        }

        private static ConstructionLayerDefinition ParseLayer(
            XElement element,
            int order,
            ContentSetDescriptor descriptor,
            string recordId,
            ICollection<ImportDiagnostic> diagnostics)
        {
            string kindText =
                (string)element.Attribute("Kind") ??
                string.Empty;

            ConstructionLayerKind kind =
                ParseLayerKind(
                    kindText,
                    element);

            string referenceId =
                FirstNonEmpty(
                    (string)element.Attribute("MaterialId"),
                    (string)element.Attribute("AirLayerId"));

            double? thickness = null;

            string thicknessText =
                FirstNonEmpty(
                    (string)element.Attribute("ThicknessMm"),
                    (string)element.Attribute("Thickness"));

            if (!string.IsNullOrWhiteSpace(thicknessText))
            {
                if (!double.TryParse(
                        thicknessText,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double thicknessMillimeters) ||
                    thicknessMillimeters <= 0.0)
                {
                    diagnostics.Add(
                        RecordError(
                            "CONSTRUCTION_LAYER_THICKNESS_INVALID",
                            "A rétegvastagság pozitív milliméterérték legyen.",
                            descriptor.ContentSetId,
                            recordId));
                    return null;
                }

                thickness = thicknessMillimeters / 1000.0;
            }

            double? fixedResistance = null;

            string resistanceText =
                (string)element.Attribute("ThermalResistance");

            if (!string.IsNullOrWhiteSpace(resistanceText))
            {
                if (!double.TryParse(
                        resistanceText,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double parsedResistance) ||
                    parsedResistance <= 0.0)
                {
                    diagnostics.Add(
                        RecordError(
                            "CONSTRUCTION_LAYER_RESISTANCE_INVALID",
                            "A közvetlen hőellenállás pozitív szám legyen.",
                            descriptor.ContentSetId,
                            recordId));
                    return null;
                }

                fixedResistance = parsedResistance;
            }

            try
            {
                return new ConstructionLayerDefinition(
                    order,
                    kind,
                    referenceId,
                    thickness,
                    fixedResistance,
                    1.0,
                    (string)element.Attribute("Description"));
            }
            catch (Exception exception)
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        ImportDiagnosticSeverity.Error,
                        ImportFailureScope.Record,
                        "CONSTRUCTION_LAYER_INVALID",
                        exception.Message,
                        descriptor.ContentSetId,
                        recordId,
                        exception: exception));

                return null;
            }
        }

        private static ConstructionLayerKind ParseLayerKind(
            string value,
            XElement element)
        {
            if (Enum.TryParse(
                value,
                true,
                out ConstructionLayerKind parsed))
            {
                return parsed;
            }

            if (element.Attribute("AirLayerId") != null)
                return ConstructionLayerKind.AirLayer;

            if (element.Attribute("ThermalResistance") != null)
                return ConstructionLayerKind.FixedResistance;

            return ConstructionLayerKind.Material;
        }

        private static ConstructionType ParseConstructionType(
            string value)
        {
            if (Enum.TryParse(
                value,
                true,
                out ConstructionType parsed))
            {
                return parsed;
            }

            return ConstructionType.Custom;
        }

        private static bool TryPositiveFraction(
            string value,
            out double result)
        {
            return double.TryParse(
                       value,
                       NumberStyles.Float,
                       CultureInfo.InvariantCulture,
                       out result) &&
                   result > 0.0 &&
                   result <= 1.0 &&
                   !double.IsNaN(result) &&
                   !double.IsInfinity(result);
        }

        private static string FirstNonEmpty(
            string first,
            string second)
        {
            return !string.IsNullOrWhiteSpace(first)
                ? first
                : second ?? string.Empty;
        }

        private static ImportDiagnostic RecordError(
            string code,
            string message,
            string contentSetId,
            string recordId)
        {
            return new ImportDiagnostic(
                ImportDiagnosticSeverity.Error,
                ImportFailureScope.Record,
                code,
                message,
                contentSetId,
                recordId);
        }

        private static ConstructionTemplateImportOutcome Failure(
            ContentSetDescriptor descriptor,
            IEnumerable<ImportDiagnostic> diagnostics)
        {
            return new ConstructionTemplateImportOutcome(
                null,
                new ContentSetImportResult(
                    descriptor.ContentSetId,
                    0,
                    0,
                    false,
                    diagnostics));
        }
    }
}
