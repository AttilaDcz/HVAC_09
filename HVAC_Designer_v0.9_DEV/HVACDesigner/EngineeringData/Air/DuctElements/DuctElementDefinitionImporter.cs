using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using HVACDesigner.EngineeringData.Abstractions;
using HVACDesigner.EngineeringData.Air.DuctMaterials;
using HVACDesigner.EngineeringData.Importing;
using HVACDesigner.EngineeringData.Infrastructure.Xml;
using HVACDesigner.EngineeringData.Registry;

namespace HVACDesigner.EngineeringData.Air.DuctElements
{
    public sealed class DuctElementDefinitionImportOutcome
    {
        public DuctElementDefinitionCatalog Catalog { get; }
        public ContentSetImportResult ImportResult { get; }

        public DuctElementDefinitionImportOutcome(
            DuctElementDefinitionCatalog catalog,
            ContentSetImportResult importResult)
        {
            Catalog = catalog;
            ImportResult = importResult ??
                throw new ArgumentNullException(nameof(importResult));
        }
    }

    public sealed class DuctElementDefinitionImporter
    {
        private static readonly HashSet<string> RecordElementNames =
            new HashSet<string>(
                new[]
                {
                    "Duct",
                    "Fitting",
                    "Damper",
                    "Accessory",
                    "Louver",
                    "Terminal",
                    "Element"
                },
                StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> KnownAttributes =
            new HashSet<string>(
                new[]
                {
                    "Id",
                    "Name",
                    "Category",
                    "GeometryType",
                    "FlowDirection",
                    "PressureModel",
                    "Material",
                    "DefaultZeta",
                    "AllowSizeChange",
                    "AllowLength",
                    "AllowRadius",
                    "AllowBranch"
                },
                StringComparer.OrdinalIgnoreCase);

        private readonly DuctElementDefinitionMapper _mapper =
            new DuctElementDefinitionMapper();

        public DuctElementDefinitionImportOutcome Import(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source,
            DuctMaterialCatalog materialCatalog = null)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var diagnostics = new List<ImportDiagnostic>();

            if (descriptor.ContentKind !=
                EngineeringContentKind.ElementDefinitionLibrary)
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        ImportDiagnosticSeverity.Error,
                        ImportFailureScope.ContentSet,
                        "DUCT_ELEMENT_CONTENT_KIND_INVALID",
                        "Az elemimport ElementDefinitionLibrary tartalmat igényel.",
                        descriptor.ContentSetId));

                return Failure(descriptor, diagnostics);
            }

            if (!source.ContentSetExists(manifest, descriptor))
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        descriptor.IsRequired
                            ? ImportDiagnosticSeverity.Error
                            : ImportDiagnosticSeverity.Warning,
                        ImportFailureScope.ContentSet,
                        "DUCT_ELEMENT_CONTENT_MISSING",
                        "A légtechnikai elemdefiníciók nem találhatók.",
                        descriptor.ContentSetId));

                return Failure(descriptor, diagnostics);
            }

            var imported = new List<DuctElementDefinition>();
            var knownIds = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            try
            {
                using (DataPackageContent content =
                    source.OpenContentSet(manifest, descriptor))
                {
                    XDocument document = XDocument.Load(content.Stream);

                    XElement section =
                        XmlDataPackageSource.FindElement(
                            document,
                            XmlDataPackageSource.GetXmlPath(
                                descriptor.SourcePath));

                    if (section == null)
                    {
                        diagnostics.Add(
                            new ImportDiagnostic(
                                ImportDiagnosticSeverity.Error,
                                ImportFailureScope.ContentSet,
                                "DUCT_ELEMENT_SECTION_MISSING",
                                "Az elemdefiníciós XML-szekció nem található.",
                                descriptor.ContentSetId));

                        return Failure(descriptor, diagnostics);
                    }

                    int recordIndex = 0;

                    foreach (XElement element in
                        section.DescendantsAndSelf()
                            .Where(item =>
                                RecordElementNames.Contains(
                                    item.Name.LocalName)))
                    {
                        if (element.Attribute("Id") == null)
                            continue;

                        recordIndex++;

                        var dto = CreateDto(
                            element,
                            section,
                            recordIndex);

                        DuctElementDefinitionMapResult mapped =
                            _mapper.Map(
                                dto,
                                manifest.PackageId,
                                descriptor.ContentSetId,
                                descriptor.ContentVersion);

                        if (!mapped.Succeeded)
                        {
                            diagnostics.Add(mapped.Diagnostic);
                            continue;
                        }

                        if (!knownIds.Add(mapped.Value.Id))
                        {
                            diagnostics.Add(
                                new ImportDiagnostic(
                                    ImportDiagnosticSeverity.Error,
                                    ImportFailureScope.Record,
                                    "DUCT_ELEMENT_DUPLICATE_ID",
                                    "Duplikált légtechnikai elemazonosító.",
                                    descriptor.ContentSetId,
                                    mapped.Value.Id,
                                    "Id"));

                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(
                                mapped.Value.DefaultMaterialId) &&
                            materialCatalog != null &&
                            !materialCatalog.TryGet(
                                mapped.Value.DefaultMaterialId,
                                out _))
                        {
                            diagnostics.Add(
                                new ImportDiagnostic(
                                    ImportDiagnosticSeverity.Warning,
                                    ImportFailureScope.Property,
                                    "DUCT_ELEMENT_MATERIAL_MISSING",
                                    "A hivatkozott alapanyag nem található; az elem anyagfeloldással még használható.",
                                    descriptor.ContentSetId,
                                    mapped.Value.Id,
                                    "Material"));
                        }

                        imported.Add(mapped.Value);
                    }
                }
            }
            catch (Exception exception)
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        ImportDiagnosticSeverity.Error,
                        ImportFailureScope.ContentSet,
                        "DUCT_ELEMENT_IMPORT_FAILED",
                        "A légtechnikai elemdefiníciók importja meghiúsult.",
                        descriptor.ContentSetId,
                        exception: exception));

                return Failure(descriptor, diagnostics);
            }

            var catalog = new DuctElementDefinitionCatalog(
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

            return new DuctElementDefinitionImportOutcome(
                catalog,
                result);
        }

        public DuctElementDefinitionImportOutcome ImportAndRegister(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source,
            EngineeringDataRegistry registry,
            DuctMaterialCatalog materialCatalog = null)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            var outcome = Import(
                manifest,
                descriptor,
                source,
                materialCatalog);

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

        private static DuctElementDefinitionDto CreateDto(
            XElement element,
            XElement rootSection,
            int recordIndex)
        {
            var metadata =
                element.Attributes()
                    .Where(attribute =>
                        !KnownAttributes.Contains(
                            attribute.Name.LocalName))
                    .ToDictionary(
                        attribute =>
                            attribute.Name.LocalName,
                        attribute =>
                            attribute.Value,
                        StringComparer.OrdinalIgnoreCase);

            return new DuctElementDefinitionDto
            {
                Id = Attribute(element, "Id"),
                Name = Attribute(element, "Name"),
                Category = Attribute(element, "Category"),
                GeometryType = Attribute(element, "GeometryType"),
                FlowDirection = Attribute(element, "FlowDirection"),
                PressureModel = Attribute(element, "PressureModel"),
                Material = Attribute(element, "Material"),
                DefaultZeta = Attribute(element, "DefaultZeta"),
                AllowSizeChange = Attribute(element, "AllowSizeChange"),
                AllowLength = Attribute(element, "AllowLength"),
                AllowRadius = Attribute(element, "AllowRadius"),
                AllowBranch = Attribute(element, "AllowBranch"),
                SourceElementName = element.Name.LocalName,
                SourceSectionPath =
                    BuildRelativePath(
                        rootSection,
                        element),
                SourceRecordIndex = recordIndex,
                Metadata = metadata
            };
        }

        private static string BuildRelativePath(
            XElement root,
            XElement element)
        {
            var names = element
                .AncestorsAndSelf()
                .TakeWhile(item => item != root.Parent)
                .Reverse()
                .Select(item => item.Name.LocalName);

            return "/" + string.Join("/", names);
        }

        private static string Attribute(
            XElement element,
            string name) =>
            (string)element.Attribute(name) ?? string.Empty;

        private static DuctElementDefinitionImportOutcome Failure(
            ContentSetDescriptor descriptor,
            IEnumerable<ImportDiagnostic> diagnostics) =>
            new DuctElementDefinitionImportOutcome(
                null,
                new ContentSetImportResult(
                    descriptor.ContentSetId,
                    0,
                    0,
                    false,
                    diagnostics));
    }
}
