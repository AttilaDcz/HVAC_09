using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Rules.Common
{
    public sealed class RuleApplicabilityContext
    {
        public string Jurisdiction { get; }
        public DateTime ReferenceDate { get; }
        public string BuildingUse { get; }
        public IReadOnlyCollection<string> Tags { get; }

        public RuleApplicabilityContext(
            string jurisdiction,
            DateTime referenceDate,
            string buildingUse,
            IEnumerable<string> tags = null)
        {
            Jurisdiction = jurisdiction?.Trim() ?? string.Empty;
            ReferenceDate = referenceDate.Date;
            BuildingUse = buildingUse?.Trim() ?? string.Empty;
            Tags = new ReadOnlyCollection<string>(
                (tags ?? Enumerable.Empty<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList());
        }
    }

    public sealed class RuleApplicability
    {
        private readonly ReadOnlyCollection<string> _buildingUses;
        private readonly ReadOnlyCollection<string> _requiredTags;

        public string Jurisdiction { get; }
        public DateTime? ValidFrom { get; }
        public DateTime? ValidTo { get; }
        public IReadOnlyCollection<string> BuildingUses => _buildingUses;
        public IReadOnlyCollection<string> RequiredTags => _requiredTags;

        public RuleApplicability(
            string jurisdiction,
            DateTime? validFrom,
            DateTime? validTo,
            IEnumerable<string> buildingUses = null,
            IEnumerable<string> requiredTags = null)
        {
            if (validFrom.HasValue && validTo.HasValue && validTo.Value.Date < validFrom.Value.Date)
                throw new ArgumentException("A ValidTo nem lehet korábbi a ValidFrom értéknél.");

            Jurisdiction = jurisdiction?.Trim() ?? string.Empty;
            ValidFrom = validFrom?.Date;
            ValidTo = validTo?.Date;
            _buildingUses = CreateCollection(buildingUses);
            _requiredTags = CreateCollection(requiredTags);
        }

        public bool AppliesTo(RuleApplicabilityContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!string.IsNullOrWhiteSpace(Jurisdiction) &&
                !string.Equals(Jurisdiction, context.Jurisdiction, StringComparison.OrdinalIgnoreCase))
                return false;

            if (ValidFrom.HasValue && context.ReferenceDate < ValidFrom.Value)
                return false;
            if (ValidTo.HasValue && context.ReferenceDate > ValidTo.Value)
                return false;

            if (_buildingUses.Count > 0 &&
                !_buildingUses.Contains(context.BuildingUse, StringComparer.OrdinalIgnoreCase))
                return false;

            foreach (string tag in _requiredTags)
            {
                if (!context.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private static ReadOnlyCollection<string> CreateCollection(IEnumerable<string> values)
        {
            return new ReadOnlyCollection<string>(
                (values ?? Enumerable.Empty<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList());
        }
    }
}
