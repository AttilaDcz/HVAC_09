using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using HVACDesigner.EngineeringData.Abstractions;
using HVACDesigner.EngineeringData.Importing;
using HVACDesigner.EngineeringData.Infrastructure.Xml;
using HVACDesigner.EngineeringData.Registry;

namespace HVACDesigner.EngineeringData.Air.DuctMaterials
{
    public sealed class DuctMaterialImportOutcome
    {
        public DuctMaterialCatalog Catalog { get; }
        public ContentSetImportResult ImportResult { get; }

        public DuctMaterialImportOutcome(
            DuctMaterialCatalog catalog,
            ContentSetImportResult importResult)
        {
            Catalog = catalog;
            ImportResult =
                importResult ??
                throw new ArgumentNullException(nameof(importResult));
        }
    }

    /// <summary>
    /// A ductdata.xml Materials tartalomkészletének első
    /// referenciaimportere.
    /// </summary>
    public sealed class DuctMaterialImporter
    {
        private readonly DuctMaterialMapper _mapper;

        public DuctMaterialImporter()
            : this(new DuctMaterialMapper())
        {
        }

        public DuctMaterialImporter(
            DuctMaterialMapper mapper)
        {
            _mapper =
                mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        public DuctMaterialImportOutcome Import(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var diagnostics =
                new List<ImportDiagnostic>();

            if (descriptor.ContentKind !=
                EngineeringContentKind.ReferenceCatalog)
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        ImportDiagnosticSeverity.Error,
                        ImportFailureScope.ContentSet,
                        "DUCT_MATERIAL_CONTENT_KIND_INVALID",
                        "A légcsatorna-anyag import csak ReferenceCatalog tartalmat fogad.",
                        descriptor.ContentSetId));

                return Failure(
                    descriptor,
                    diagnostics);
            }

            if (!source.ContentSetExists(
                manifest,
                descriptor))
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        descriptor.IsRequired
                            ? ImportDiagnosticSeverity.Error
                            : ImportDiagnosticSeverity.Warning,
                        ImportFailureScope.ContentSet,
                        "DUCT_MATERIAL_CONTENT_MISSING",
                        "A légcsatorna-anyag tartalomkészlet nem található.",
                        descriptor.ContentSetId));

                return Failure(
                    descriptor,
                    diagnostics);
            }

            var imported =
                new List<DuctMaterialDefinition>();

            var knownIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            try
            {
                using (DataPackageContent content =
                    source.OpenContentSet(
                        manifest,
                        descriptor))
                {
                    XDocument document =
                        XDocument.Load(
                            content.Stream,
                            LoadOptions.None);

                    string xmlPath =
                        XmlDataPackageSource.GetXmlPath(
                            descriptor.SourcePath);

                    XElement section =
                        XmlDataPackageSource.FindElement(
                            document,
                            xmlPath);

                    if (section == null)
                    {
                        diagnostics.Add(
                            new ImportDiagnostic(
                                ImportDiagnosticSeverity.Error,
                                ImportFailureScope.ContentSet,
                                "DUCT_MATERIAL_SECTION_MISSING",
                                $"Az XML szekció nem található: {xmlPath}.",
                                descriptor.ContentSetId));

                        return Failure(
                            descriptor,
                            diagnostics);
                    }

                    int recordIndex = 0;

                    foreach (XElement element in
                        section.Elements("Material"))
                    {
                        recordIndex++;

                        var dto =
                            new DuctMaterialDto
                            {
                                Id =
                                    (string)element.Attribute("Id") ??
                                    string.Empty,
                                Name =
                                    (string)element.Attribute("Name") ??
                                    string.Empty,
                                RoughnessMillimeters =
                                    (string)element.Attribute("Roughness") ??
                                    string.Empty,
                                Flexible =
                                    (string)element.Attribute("Flexible") ??
                                    string.Empty,
                                SourceRecordIndex =
                                    recordIndex
                            };

                        DuctMaterialMapResult map =
                            _mapper.Map(
                                dto,
                                manifest.PackageId,
                                descriptor.ContentSetId,
                                descriptor.ContentVersion);

                        if (!map.Succeeded)
                        {
                            diagnostics.Add(map.Diagnostic);
                            continue;
                        }

                        if (!knownIds.Add(map.Value.Id))
                        {
                            diagnostics.Add(
                                new ImportDiagnostic(
                                    ImportDiagnosticSeverity.Error,
                                    ImportFailureScope.Record,
                                    "DUCT_MATERIAL_DUPLICATE_ID",
                                    "Duplikált légcsatorna-anyag azonosító.",
                                    descriptor.ContentSetId,
                                    map.Value.Id,
                                    "Id"));

                            continue;
                        }

                        imported.Add(map.Value);
                    }
                }
            }
            catch (Exception exception)
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        ImportDiagnosticSeverity.Error,
                        ImportFailureScope.ContentSet,
                        "DUCT_MATERIAL_IMPORT_FAILED",
                        "A légcsatorna-anyagok importja meghiúsult.",
                        descriptor.ContentSetId,
                        exception: exception));

                return Failure(
                    descriptor,
                    diagnostics);
            }

            var catalog =
                new DuctMaterialCatalog(
                    descriptor.ContentSetId,
                    descriptor.ContentVersion,
                    imported);

            var result =
                new ContentSetImportResult(
                    descriptor.ContentSetId,
                    imported.Count,
                    diagnostics.Count(
                        item =>
                            item.FailureScope ==
                            ImportFailureScope.Record &&
                            item.Severity ==
                            ImportDiagnosticSeverity.Error),
                    true,
                    diagnostics);

            return new DuctMaterialImportOutcome(
                catalog,
                result);
        }

        public DuctMaterialImportOutcome ImportAndRegister(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source,
            EngineeringDataRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            DuctMaterialImportOutcome outcome =
                Import(
                    manifest,
                    descriptor,
                    source);

            if (outcome.Catalog != null &&
                outcome.ImportResult.IsAvailable)
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

        private static DuctMaterialImportOutcome Failure(
            ContentSetDescriptor descriptor,
            IEnumerable<ImportDiagnostic> diagnostics)
        {
            return new DuctMaterialImportOutcome(
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
