using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Common
{
    public enum EngineeringDataLifecycleState
    {
        Draft,
        Active,
        Deprecated,
        Obsolete
    }

    public sealed class EngineeringDataHeader
    {
        public string Id { get; }
        public string Name { get; }
        public string Category { get; }

        public string SourcePackageId { get; }
        public string SourceContentSetId { get; }
        public string SourceVersion { get; }

        public string Country { get; }
        public string StandardReference { get; }

        public EngineeringDataLifecycleState LifecycleState { get; }

        public IReadOnlyList<string> Tags { get; }
        public IReadOnlyDictionary<string, string> Metadata { get; }

        public EngineeringDataHeader(
            string id,
            string name,
            string category = "",
            string sourcePackageId = "",
            string sourceContentSetId = "",
            string sourceVersion = "",
            string country = "",
            string standardReference = "",
            EngineeringDataLifecycleState lifecycleState =
                EngineeringDataLifecycleState.Active,
            IEnumerable<string>? tags = null,
            IDictionary<string, string>? metadata = null)
        {
            Id = RequireText(id, nameof(id));
            Name = RequireText(name, nameof(name));
            Category = NormalizeOptionalText(category);
            SourcePackageId = NormalizeOptionalText(sourcePackageId);
            SourceContentSetId = NormalizeOptionalText(sourceContentSetId);
            SourceVersion = NormalizeOptionalText(sourceVersion);
            Country = NormalizeOptionalText(country);
            StandardReference = NormalizeOptionalText(standardReference);
            LifecycleState = lifecycleState;

            Tags = new ReadOnlyCollection<string>(
                (tags ?? Enumerable.Empty<string>())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .ToList());

            Metadata = new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(
                    metadata ?? new Dictionary<string, string>(),
                    StringComparer.OrdinalIgnoreCase));
        }

        private static string RequireText(string? value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Az érték nem lehet üres.",
                    parameterName);
            }

            return value.Trim();
        }

        private static string NormalizeOptionalText(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }
    }
}
