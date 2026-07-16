using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Abstractions
{
    public enum DataPackageSourceType
    {
        Xml,
        Json,
        Sql,
        Api,
        Embedded,
        Custom
    }

    /// <summary>
    /// Egy adatcsomag más csomagra vagy tartalomkészletre mutató függősége.
    /// </summary>
    public sealed class DataPackageDependency
    {
        public string TargetId { get; }
        public string MinimumVersion { get; }
        public bool IsRequired { get; }

        public DataPackageDependency(
            string targetId,
            string minimumVersion = "",
            bool isRequired = true)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException(
                    "A függőség célazonosítója nem lehet üres.",
                    nameof(targetId));
            }

            TargetId = targetId.Trim();
            MinimumVersion = minimumVersion?.Trim() ?? string.Empty;
            IsRequired = isRequired;
        }
    }

    /// <summary>
    /// Egy verziózott mérnöki adatcsomag deklaratív leírása.
    /// Nem végez adatbetöltést.
    /// </summary>
    public sealed class EngineeringDataPackageManifest
    {
        private readonly ReadOnlyCollection<ContentSetDescriptor> _contentSets;
        private readonly ReadOnlyCollection<DataPackageDependency> _dependencies;

        public string PackageId { get; }
        public string Name { get; }
        public string PackageVersion { get; }
        public string SchemaVersion { get; }
        public string Discipline { get; }
        public DataPackageSourceType SourceType { get; }
        public string SourceLocation { get; }
        public string Manufacturer { get; }
        public string Jurisdiction { get; }
        public DateTimeOffset? ValidFrom { get; }
        public DateTimeOffset? ValidTo { get; }

        public IReadOnlyList<ContentSetDescriptor> ContentSets => _contentSets;
        public IReadOnlyList<DataPackageDependency> Dependencies => _dependencies;

        public EngineeringDataPackageManifest(
            string packageId,
            string name,
            string packageVersion,
            string schemaVersion,
            string discipline,
            DataPackageSourceType sourceType,
            string sourceLocation,
            IEnumerable<ContentSetDescriptor> contentSets,
            IEnumerable<DataPackageDependency>? dependencies = null,
            string manufacturer = "",
            string jurisdiction = "",
            DateTimeOffset? validFrom = null,
            DateTimeOffset? validTo = null)
        {
            PackageId = RequireText(packageId, nameof(packageId));
            Name = RequireText(name, nameof(name));
            PackageVersion = RequireText(
                packageVersion,
                nameof(packageVersion));
            SchemaVersion = RequireText(
                schemaVersion,
                nameof(schemaVersion));
            Discipline = RequireText(discipline, nameof(discipline));
            SourceType = sourceType;
            SourceLocation = sourceLocation?.Trim() ?? string.Empty;
            Manufacturer = manufacturer?.Trim() ?? string.Empty;
            Jurisdiction = jurisdiction?.Trim() ?? string.Empty;
            ValidFrom = validFrom;
            ValidTo = validTo;

            if (validFrom.HasValue &&
                validTo.HasValue &&
                validTo.Value < validFrom.Value)
            {
                throw new ArgumentException(
                    "A ValidTo nem lehet korábbi a ValidFrom értékénél.");
            }

            var contentSetList =
                (contentSets ?? throw new ArgumentNullException(
                    nameof(contentSets)))
                .ToList();

            if (contentSetList.Count == 0)
            {
                throw new ArgumentException(
                    "Az adatcsomagnak legalább egy tartalomkészletet tartalmaznia kell.",
                    nameof(contentSets));
            }

            string duplicateContentSetId = contentSetList
                .GroupBy(
                    item => item.ContentSetId,
                    StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(duplicateContentSetId))
            {
                throw new ArgumentException(
                    $"Duplikált tartalomkészlet-azonosító: {duplicateContentSetId}.",
                    nameof(contentSets));
            }

            _contentSets =
                new ReadOnlyCollection<ContentSetDescriptor>(
                    contentSetList);

            _dependencies =
                new ReadOnlyCollection<DataPackageDependency>(
                    (dependencies ?? Enumerable.Empty<DataPackageDependency>())
                    .ToList());
        }

        public bool TryGetContentSet(
            string contentSetId,
            out ContentSetDescriptor? descriptor)
        {
            descriptor = _contentSets.FirstOrDefault(
                item => string.Equals(
                    item.ContentSetId,
                    contentSetId,
                    StringComparison.OrdinalIgnoreCase));

            return descriptor != null;
        }

        public bool IsValidAt(DateTimeOffset instant)
        {
            if (ValidFrom.HasValue && instant < ValidFrom.Value)
                return false;

            if (ValidTo.HasValue && instant > ValidTo.Value)
                return false;

            return true;
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
