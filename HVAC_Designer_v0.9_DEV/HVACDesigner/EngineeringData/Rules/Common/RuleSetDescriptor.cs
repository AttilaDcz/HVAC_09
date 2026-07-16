using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Rules.Common
{
    public enum EngineeringRuleDiscipline
    {
        Shared,
        BuildingPhysics,
        BuildingEnergy,
        Heating,
        Cooling,
        Air,
        WaterSupply,
        Drainage,
        DomesticHotWater,
        IndoorEnvironment,
        Documentation
    }

    public sealed class RuleSetDescriptor
    {
        private readonly ReadOnlyCollection<RuleReference> _references;

        public string RuleSetId { get; }
        public string Version { get; }
        public string Name { get; }
        public EngineeringRuleDiscipline Discipline { get; }
        public string MethodId { get; }
        public RuleApplicability Applicability { get; }
        public RuleParameterSet Parameters { get; }
        public IReadOnlyList<RuleReference> References => _references;

        public RuleSetDescriptor(
            string ruleSetId,
            string version,
            string name,
            EngineeringRuleDiscipline discipline,
            string methodId,
            RuleApplicability applicability,
            RuleParameterSet parameters,
            IEnumerable<RuleReference> references = null)
        {
            RuleSetId = RequireText(ruleSetId, nameof(ruleSetId));
            Version = RequireText(version, nameof(version));
            Name = RequireText(name, nameof(name));
            MethodId = RequireText(methodId, nameof(methodId));
            Discipline = discipline;
            Applicability = applicability ?? throw new ArgumentNullException(nameof(applicability));
            Parameters = parameters ?? new RuleParameterSet(new Dictionary<string, string>());
            _references = new ReadOnlyCollection<RuleReference>(
                (references ?? Enumerable.Empty<RuleReference>()).ToList());
        }

        public string GetVersionedKey() => RuleSetId + "@" + Version;

        private static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Az érték nem lehet üres.", parameterName);
            return value.Trim();
        }
    }
}
