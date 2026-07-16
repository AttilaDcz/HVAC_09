using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Rules.Common
{
    public sealed class DesignMethodProfile
    {
        private readonly ReadOnlyCollection<string> _ruleSetKeys;
        private readonly ReadOnlyCollection<RuleReference> _references;

        public string DesignMethodId { get; }
        public string Version { get; }
        public string Name { get; }
        public string Jurisdiction { get; }
        public DateTime? ValidFrom { get; }
        public DateTime? ValidTo { get; }
        public bool IsLegacy { get; }
        public IReadOnlyList<string> RuleSetKeys => _ruleSetKeys;
        public IReadOnlyList<RuleReference> References => _references;

        public DesignMethodProfile(
            string designMethodId,
            string version,
            string name,
            string jurisdiction,
            DateTime? validFrom,
            DateTime? validTo,
            bool isLegacy,
            IEnumerable<string> ruleSetKeys,
            IEnumerable<RuleReference> references = null)
        {
            if (validFrom.HasValue && validTo.HasValue && validTo.Value.Date < validFrom.Value.Date)
                throw new ArgumentException("A ValidTo nem lehet korábbi a ValidFrom értéknél.");

            DesignMethodId = RequireText(designMethodId, nameof(designMethodId));
            Version = RequireText(version, nameof(version));
            Name = RequireText(name, nameof(name));
            Jurisdiction = jurisdiction?.Trim() ?? string.Empty;
            ValidFrom = validFrom?.Date;
            ValidTo = validTo?.Date;
            IsLegacy = isLegacy;

            var keys = (ruleSetKeys ?? throw new ArgumentNullException(nameof(ruleSetKeys)))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (keys.Count == 0)
                throw new ArgumentException("A profilnak legalább egy RuleSet hivatkozást tartalmaznia kell.", nameof(ruleSetKeys));

            _ruleSetKeys = new ReadOnlyCollection<string>(keys);
            _references = new ReadOnlyCollection<RuleReference>(
                (references ?? Enumerable.Empty<RuleReference>()).ToList());
        }

        public string GetVersionedKey() => DesignMethodId + "@" + Version;

        public bool IsValidOn(DateTime date)
        {
            DateTime value = date.Date;
            if (ValidFrom.HasValue && value < ValidFrom.Value) return false;
            if (ValidTo.HasValue && value > ValidTo.Value) return false;
            return true;
        }

        private static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Az érték nem lehet üres.", parameterName);
            return value.Trim();
        }
    }
}
