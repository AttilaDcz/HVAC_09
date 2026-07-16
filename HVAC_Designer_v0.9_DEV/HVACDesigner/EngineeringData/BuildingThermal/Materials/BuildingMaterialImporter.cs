using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using HVACDesigner.EngineeringData.Abstractions;
using HVACDesigner.EngineeringData.Importing;
using HVACDesigner.EngineeringData.Infrastructure.Xml;
using HVACDesigner.EngineeringData.Registry;

namespace HVACDesigner.EngineeringData.BuildingThermal.Materials
{
    public sealed class BuildingMaterialImportOutcome
    {
        public BuildingMaterialCatalog Catalog { get; }
        public ContentSetImportResult ImportResult { get; }

        public BuildingMaterialImportOutcome(
            BuildingMaterialCatalog catalog,
            ContentSetImportResult importResult)
        {
            Catalog = catalog;
            ImportResult =
                importResult ??
                throw new ArgumentNullException(nameof(importResult));
        }
    }

    public sealed class BuildingMaterialImporter
    {
        private static readonly HashSet<string> KnownAttributes =
            new HashSet<string>(
                new[]
                {
                    "Id",
                    "Name",
                    "Lambda",
                    "LambdaCorrection",
                    "Density",
                    "SpecificHeat",
                    "Mu",
                    "ValueSource",
                    "MoistureCondition",
                    "StandardReference",
                    "Description"
                },
                StringComparer.OrdinalIgnoreCase);

        private readonly BuildingMaterialMapper _mapper =
            new BuildingMaterialMapper();

        public BuildingMaterialImportOutcome Import(
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

            var diagnostics = new List<ImportDiagnostic>();

            if (descriptor.ContentKind !=
                EngineeringContentKind.ReferenceCatalog)
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        ImportDiagnosticSeverity.Error,
                        ImportFailureScope.ContentSet,
                        "BUILDING_MATERIAL_CONTENT_KIND_INVALID",
                        "Az építőanyag-import ReferenceCatalog tartalmat igényel.",
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
                        "BUILDING_MATERIAL_CONTENT_MISSING",
                        "Az építőanyag tartalomkészlet nem található.",
                        descriptor.ContentSetId));

                return Failure(descriptor, diagnostics);
            }

            var imported = new List<BuildingMaterialDefinition>();
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
                                "BUILDING_MATERIAL_SECTION_MISSING",
                                "A MaterialCatalog XML-szekció nem található.",
                                descriptor.ContentSetId));

                        return Failure(descriptor, diagnostics);
                    }

                    int recordIndex = 0;

                    foreach (XElement categoryElement in
                        root.Elements("Category"))
                    {
                        string category =
                            (string)categoryElement.Attribute("Name") ??
                            string.Empty;

                        foreach (XElement materialElement in
                            categoryElement.Elements("Material"))
                        {
                            recordIndex++;

                            BuildingMaterialDto dto =
                                CreateDto(
                                    materialElement,
                                    category,
                                    recordIndex);

                            BuildingMaterialMapResult mapped =
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
                                        "BUILDING_MATERIAL_DUPLICATE_ID",
                                        "Duplikált építőanyag-azonosító.",
                                        descriptor.ContentSetId,
                                        mapped.Value.Id,
                                        "Id"));

                                continue;
                            }

                            imported.Add(mapped.Value);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                diagnostics.Add(
                    new ImportDiagnostic(
                        ImportDiagnosticSeverity.Error,
                        ImportFailureScope.ContentSet,
                        "BUILDING_MATERIAL_IMPORT_FAILED",
                        "Az építőanyagok importja meghiúsult.",
                        descriptor.ContentSetId,
                        exception: exception));

                return Failure(descriptor, diagnostics);
            }

            var catalog = new BuildingMaterialCatalog(
                descriptor.ContentSetId,
                descriptor.ContentVersion,
                imported);

            var result = new ContentSetImportResult(
                descriptor.ContentSetId,
                imported.Count,
                diagnostics.Count(item =>
                    item.Severity ==
                    ImportDiagnosticSeverity.Error &&
                    item.FailureScope ==
                    ImportFailureScope.Record),
                true,
                diagnostics);

            return new BuildingMaterialImportOutcome(
                catalog,
                result);
        }

        public BuildingMaterialImportOutcome ImportAndRegister(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source,
            EngineeringDataRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            BuildingMaterialImportOutcome outcome =
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

        private static BuildingMaterialDto CreateDto(
            XElement element,
            string category,
            int recordIndex)
        {
            IDictionary<string, string> metadata =
                element.Attributes()
                    .Where(attribute =>
                        !KnownAttributes.Contains(
                            attribute.Name.LocalName))
                    .ToDictionary(
                        attribute => attribute.Name.LocalName,
                        attribute => attribute.Value,
                        StringComparer.OrdinalIgnoreCase);

            return new BuildingMaterialDto
            {
                Id = Attribute(element, "Id"),
                Name = Attribute(element, "Name"),
                Category = category,
                Lambda = Attribute(element, "Lambda"),
                LambdaCorrection =
                    Attribute(element, "LambdaCorrection"),
                Density = Attribute(element, "Density"),
                SpecificHeatKilojoules =
                    Attribute(element, "SpecificHeat"),
                Mu = Attribute(element, "Mu"),
                ValueSource = Attribute(element, "ValueSource"),
                MoistureCondition =
                    Attribute(element, "MoistureCondition"),
                StandardReference =
                    Attribute(element, "StandardReference"),
                Description =
                    Attribute(element, "Description"),
                SourceRecordIndex = recordIndex,
                Metadata = metadata
            };
        }

        private static string Attribute(
            XElement element,
            string name) =>
            (string)element.Attribute(name) ?? string.Empty;

        private static BuildingMaterialImportOutcome Failure(
            ContentSetDescriptor descriptor,
            IEnumerable<ImportDiagnostic> diagnostics) =>
            new BuildingMaterialImportOutcome(
                null,
                new ContentSetImportResult(
                    descriptor.ContentSetId,
                    0,
                    0,
                    false,
                    diagnostics));
    }
}
