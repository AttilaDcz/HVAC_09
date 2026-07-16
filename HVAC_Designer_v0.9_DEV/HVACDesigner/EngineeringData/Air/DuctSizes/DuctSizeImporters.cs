using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using HVACDesigner.EngineeringData.Abstractions;
using HVACDesigner.EngineeringData.Importing;
using HVACDesigner.EngineeringData.Infrastructure.Xml;
using HVACDesigner.EngineeringData.Registry;

namespace HVACDesigner.EngineeringData.Air.DuctSizes
{
    public sealed class DuctSizeImportOutcome<TCatalog> where TCatalog : class
    {
        public TCatalog Catalog { get; }
        public ContentSetImportResult ImportResult { get; }

        public DuctSizeImportOutcome(
            TCatalog catalog,
            ContentSetImportResult importResult)
        {
            Catalog = catalog;
            ImportResult = importResult ??
                throw new ArgumentNullException(nameof(importResult));
        }
    }

    public sealed class CircularDuctSizeImporter
    {
        public DuctSizeImportOutcome<CircularDuctSizeCatalog> Import(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source)
        {
            if (manifest == null) throw new ArgumentNullException(nameof(manifest));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (source == null) throw new ArgumentNullException(nameof(source));

            var diagnostics = new List<ImportDiagnostic>();

            if (!source.ContentSetExists(manifest, descriptor))
                return Missing<CircularDuctSizeCatalog>(descriptor);

            var mapper = new CircularDuctSizeMapper();
            var imported = new List<CircularDuctSizeDefinition>();
            var known = new HashSet<int>();

            using (DataPackageContent content =
                source.OpenContentSet(manifest, descriptor))
            {
                XDocument document = XDocument.Load(content.Stream);
                XElement section = XmlDataPackageSource.FindElement(
                    document,
                    XmlDataPackageSource.GetXmlPath(descriptor.SourcePath));

                int index = 0;

                foreach (XElement element in section.Elements("Size"))
                {
                    index++;

                    var result = mapper.Map(
                        new CircularDuctSizeDto
                        {
                            DiameterMillimeters =
                                (string)element.Attribute("Diameter") ??
                                string.Empty,
                            SourceRecordIndex = index
                        },
                        manifest.PackageId,
                        descriptor.ContentSetId,
                        descriptor.ContentVersion);

                    if (!result.Succeeded)
                    {
                        diagnostics.Add(result.Diagnostic);
                        continue;
                    }

                    if (!known.Add(result.Value.DiameterMillimeters))
                    {
                        diagnostics.Add(Duplicate(
                            descriptor.ContentSetId,
                            index,
                            result.Value.DiameterMillimeters.ToString()));
                        continue;
                    }

                    imported.Add(result.Value);
                }
            }

            return new DuctSizeImportOutcome<CircularDuctSizeCatalog>(
                new CircularDuctSizeCatalog(
                    descriptor.ContentSetId,
                    descriptor.ContentVersion,
                    imported),
                Result(descriptor, imported.Count, diagnostics));
        }

        public DuctSizeImportOutcome<CircularDuctSizeCatalog> ImportAndRegister(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source,
            EngineeringDataRegistry registry)
        {
            var outcome = Import(manifest, descriptor, source);

            if (outcome.Catalog != null)
                registry.Register(
                    manifest.PackageId,
                    descriptor.ContentSetId,
                    descriptor.ContentVersion,
                    descriptor.ContentKind,
                    outcome.Catalog);

            return outcome;
        }

        internal static DuctSizeImportOutcome<TCatalog> Missing<TCatalog>(
            ContentSetDescriptor descriptor)
            where TCatalog : class
        {
            var diagnostic = new ImportDiagnostic(
                descriptor.IsRequired
                    ? ImportDiagnosticSeverity.Error
                    : ImportDiagnosticSeverity.Warning,
                ImportFailureScope.ContentSet,
                "DUCT_SIZE_CONTENT_MISSING",
                "A légcsatorna-méret tartalomkészlet nem található.",
                descriptor.ContentSetId);

            return new DuctSizeImportOutcome<TCatalog>(
                null,
                new ContentSetImportResult(
                    descriptor.ContentSetId,
                    0,
                    0,
                    false,
                    new[] { diagnostic }));
        }

        internal static ImportDiagnostic Duplicate(
            string contentSetId,
            int index,
            string size) =>
            new ImportDiagnostic(
                ImportDiagnosticSeverity.Error,
                ImportFailureScope.Record,
                "DUCT_SIZE_DUPLICATE",
                $"Duplikált légcsatorna-méret: {size}.",
                contentSetId,
                $"record-{index}");

        internal static ContentSetImportResult Result(
            ContentSetDescriptor descriptor,
            int importedCount,
            List<ImportDiagnostic> diagnostics) =>
            new ContentSetImportResult(
                descriptor.ContentSetId,
                importedCount,
                diagnostics.Count(item =>
                    item.FailureScope == ImportFailureScope.Record),
                true,
                diagnostics);
    }

    public sealed class RectangularDuctSizeImporter
    {
        public DuctSizeImportOutcome<RectangularDuctSizeCatalog> Import(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor,
            IDataPackageSource source)
        {
            if (manifest == null) throw new ArgumentNullException(nameof(manifest));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (!source.ContentSetExists(manifest, descriptor))
                return CircularDuctSizeImporter
                    .Missing<RectangularDuctSizeCatalog>(descriptor);

            var mapper = new RectangularDuctSizeMapper();
            var diagnostics = new List<ImportDiagnostic>();
            var imported = new List<RectangularDuctSizeDefinition>();
            var known = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            using (DataPackageContent content =
                source.OpenContentSet(manifest, descriptor))
            {
                XDocument document = XDocument.Load(content.Stream);
                XElement section = XmlDataPackageSource.FindElement(
                    document,
                    XmlDataPackageSource.GetXmlPath(descriptor.SourcePath));

                int index = 0;

                foreach (XElement element in section.Elements("Size"))
                {
                    index++;

                    var result = mapper.Map(
                        new RectangularDuctSizeDto
                        {
                            WidthMillimeters =
                                (string)element.Attribute("Width") ??
                                string.Empty,
                            HeightMillimeters =
                                (string)element.Attribute("Height") ??
                                string.Empty,
                            SourceRecordIndex = index
                        },
                        manifest.PackageId,
                        descriptor.ContentSetId,
                        descriptor.ContentVersion);

                    if (!result.Succeeded)
                    {
                        diagnostics.Add(result.Diagnostic);
                        continue;
                    }

                    string key =
                        $"{result.Value.WidthMillimeters}x" +
                        $"{result.Value.HeightMillimeters}";

                    if (!known.Add(key))
                    {
                        diagnostics.Add(
                            CircularDuctSizeImporter.Duplicate(
                                descriptor.ContentSetId,
                                index,
                                key));
                        continue;
                    }

                    imported.Add(result.Value);
                }
            }

            return new DuctSizeImportOutcome<RectangularDuctSizeCatalog>(
                new RectangularDuctSizeCatalog(
                    descriptor.ContentSetId,
                    descriptor.ContentVersion,
                    imported),
                CircularDuctSizeImporter.Result(
                    descriptor,
                    imported.Count,
                    diagnostics));
        }

        public DuctSizeImportOutcome<RectangularDuctSizeCatalog>
            ImportAndRegister(
                EngineeringDataPackageManifest manifest,
                ContentSetDescriptor descriptor,
                IDataPackageSource source,
                EngineeringDataRegistry registry)
        {
            var outcome = Import(manifest, descriptor, source);

            if (outcome.Catalog != null)
                registry.Register(
                    manifest.PackageId,
                    descriptor.ContentSetId,
                    descriptor.ContentVersion,
                    descriptor.ContentKind,
                    outcome.Catalog);

            return outcome;
        }
    }
}
