using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Abstractions
{
    public enum EngineeringContentKind
    {
        ReferenceCatalog,
        StandardSizeCatalog,
        ElementDefinitionLibrary,
        ComponentCatalog,
        TemplateLibrary,
        RulePackage
    }

    /// <summary>
    /// Egy adatcsomagon belüli önálló, típusos tartalomkészlet leírása.
    /// </summary>
    public sealed class ContentSetDescriptor
    {
        private readonly ReadOnlyCollection<string> _dependencies;

        public string ContentSetId { get; }
        public EngineeringContentKind ContentKind { get; }
        public string RecordType { get; }
        public string ContentVersion { get; }
        public string SourcePath { get; }
        public string MappingSchema { get; }
        public bool IsRequired { get; }
        public IReadOnlyList<string> Dependencies => _dependencies;

        public ContentSetDescriptor(
            string contentSetId,
            EngineeringContentKind contentKind,
            string recordType,
            string contentVersion,
            string sourcePath,
            string mappingSchema = "",
            bool isRequired = false,
            IEnumerable<string>? dependencies = null)
        {
            ContentSetId = RequireText(
                contentSetId,
                nameof(contentSetId));

            ContentKind = contentKind;

            RecordType = RequireText(
                recordType,
                nameof(recordType));

            ContentVersion = RequireText(
                contentVersion,
                nameof(contentVersion));

            SourcePath = sourcePath?.Trim() ?? string.Empty;
            MappingSchema = mappingSchema?.Trim() ?? string.Empty;
            IsRequired = isRequired;

            var dependencyList =
                (dependencies ?? Enumerable.Empty<string>())
                .Select(item => item?.Trim() ?? string.Empty)
                .Where(item => item.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (dependencyList.Any(
                item => string.Equals(
                    item,
                    ContentSetId,
                    StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(
                    "Egy tartalomkészlet nem függhet saját magától.",
                    nameof(dependencies));
            }

            _dependencies =
                new ReadOnlyCollection<string>(
                    dependencyList);
        }

        private static string RequireText(
            string? value,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Az érték nem lehet üres.",
                    parameterName);
            }

            return value.Trim();
        }
    }
}
