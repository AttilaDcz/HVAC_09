using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Rules.Common
{
    public sealed class EngineeringRuleRegistry
    {
        private readonly Dictionary<string, RuleSetDescriptor> _ruleSets =
            new Dictionary<string, RuleSetDescriptor>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, DesignMethodProfile> _profiles =
            new Dictionary<string, DesignMethodProfile>(StringComparer.OrdinalIgnoreCase);

        public void RegisterRuleSet(RuleSetDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            string key = descriptor.GetVersionedKey();
            if (_ruleSets.ContainsKey(key))
                throw new InvalidOperationException("A RuleSet már regisztrálva van: " + key + ".");
            _ruleSets.Add(key, descriptor);
        }

        public void RegisterDesignMethod(DesignMethodProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            foreach (string key in profile.RuleSetKeys)
            {
                if (!_ruleSets.ContainsKey(key))
                    throw new InvalidOperationException("A DesignMethod ismeretlen RuleSetre hivatkozik: " + key + ".");
            }

            string profileKey = profile.GetVersionedKey();
            if (_profiles.ContainsKey(profileKey))
                throw new InvalidOperationException("A DesignMethod már regisztrálva van: " + profileKey + ".");
            _profiles.Add(profileKey, profile);
        }

        public bool TryGetRuleSet(string ruleSetId, string version, out RuleSetDescriptor descriptor) =>
            _ruleSets.TryGetValue(CreateKey(ruleSetId, version), out descriptor);

        public RuleSetDescriptor GetRequiredRuleSet(string ruleSetId, string version)
        {
            if (TryGetRuleSet(ruleSetId, version, out RuleSetDescriptor descriptor))
                return descriptor;
            throw new KeyNotFoundException("A RuleSet nem található: " + CreateKey(ruleSetId, version) + ".");
        }

        public bool TryGetDesignMethod(string designMethodId, string version, out DesignMethodProfile profile) =>
            _profiles.TryGetValue(CreateKey(designMethodId, version), out profile);

        public DesignMethodProfile GetRequiredDesignMethod(string designMethodId, string version)
        {
            if (TryGetDesignMethod(designMethodId, version, out DesignMethodProfile profile))
                return profile;
            throw new KeyNotFoundException("A DesignMethod nem található: " + CreateKey(designMethodId, version) + ".");
        }

        public IReadOnlyList<RuleSetDescriptor> ResolveRuleSets(
            DesignMethodProfile profile,
            EngineeringRuleDiscipline? discipline = null)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var result = new List<RuleSetDescriptor>();

            foreach (string key in profile.RuleSetKeys)
            {
                if (!_ruleSets.TryGetValue(key, out RuleSetDescriptor descriptor))
                    throw new InvalidOperationException("A profilhoz tartozó RuleSet hiányzik: " + key + ".");
                if (!discipline.HasValue || descriptor.Discipline == discipline.Value)
                    result.Add(descriptor);
            }

            return new ReadOnlyCollection<RuleSetDescriptor>(result);
        }

        public IReadOnlyList<DesignMethodProfile> GetApplicableDesignMethods(RuleApplicabilityContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return new ReadOnlyCollection<DesignMethodProfile>(
                _profiles.Values
                    .Where(x => string.IsNullOrWhiteSpace(x.Jurisdiction) ||
                                string.Equals(x.Jurisdiction, context.Jurisdiction, StringComparison.OrdinalIgnoreCase))
                    .Where(x => x.IsValidOn(context.ReferenceDate))
                    .OrderBy(x => x.Name)
                    .ToList());
        }

        private static string CreateKey(string id, string version)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Az azonosító nem lehet üres.", nameof(id));
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("A verzió nem lehet üres.", nameof(version));
            return id.Trim() + "@" + version.Trim();
        }
    }
}
