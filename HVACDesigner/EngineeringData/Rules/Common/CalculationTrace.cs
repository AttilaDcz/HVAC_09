using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Rules.Common
{
    public sealed class AppliedRuleParameter
    {
        public string Name { get; }
        public string Value { get; }
        public string Unit { get; }
        public string SourceRuleSetKey { get; }

        public AppliedRuleParameter(string name, string value, string unit, string sourceRuleSetKey)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A paraméternév nem lehet üres.", nameof(name));
            Name = name.Trim();
            Value = value?.Trim() ?? string.Empty;
            Unit = unit?.Trim() ?? string.Empty;
            SourceRuleSetKey = sourceRuleSetKey?.Trim() ?? string.Empty;
        }
    }

    public sealed class UserRuleOverride
    {
        public string ParameterName { get; }
        public string OriginalValue { get; }
        public string OverrideValue { get; }
        public string Reason { get; }

        public UserRuleOverride(string parameterName, string originalValue, string overrideValue, string reason)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                throw new ArgumentException("A paraméternév nem lehet üres.", nameof(parameterName));
            ParameterName = parameterName.Trim();
            OriginalValue = originalValue?.Trim() ?? string.Empty;
            OverrideValue = overrideValue?.Trim() ?? string.Empty;
            Reason = reason?.Trim() ?? string.Empty;
        }
    }

    public sealed class CalculationTrace
    {
        private readonly ReadOnlyCollection<string> _ruleSetKeys;
        private readonly ReadOnlyCollection<RuleReference> _references;
        private readonly ReadOnlyCollection<AppliedRuleParameter> _appliedParameters;
        private readonly ReadOnlyCollection<UserRuleOverride> _userOverrides;

        public string DesignMethodKey { get; }
        public string MethodId { get; }
        public DateTime CreatedUtc { get; }
        public IReadOnlyList<string> RuleSetKeys => _ruleSetKeys;
        public IReadOnlyList<RuleReference> References => _references;
        public IReadOnlyList<AppliedRuleParameter> AppliedParameters => _appliedParameters;
        public IReadOnlyList<UserRuleOverride> UserOverrides => _userOverrides;

        public CalculationTrace(
            string designMethodKey,
            string methodId,
            IEnumerable<string> ruleSetKeys,
            IEnumerable<RuleReference> references,
            IEnumerable<AppliedRuleParameter> appliedParameters,
            IEnumerable<UserRuleOverride> userOverrides,
            DateTime? createdUtc = null)
        {
            if (string.IsNullOrWhiteSpace(designMethodKey))
                throw new ArgumentException("A DesignMethodKey nem lehet üres.", nameof(designMethodKey));
            if (string.IsNullOrWhiteSpace(methodId))
                throw new ArgumentException("A MethodId nem lehet üres.", nameof(methodId));

            DesignMethodKey = designMethodKey.Trim();
            MethodId = methodId.Trim();
            CreatedUtc = (createdUtc ?? DateTime.UtcNow).ToUniversalTime();
            _ruleSetKeys = new ReadOnlyCollection<string>(
                (ruleSetKeys ?? Enumerable.Empty<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList());
            _references = new ReadOnlyCollection<RuleReference>(
                (references ?? Enumerable.Empty<RuleReference>()).ToList());
            _appliedParameters = new ReadOnlyCollection<AppliedRuleParameter>(
                (appliedParameters ?? Enumerable.Empty<AppliedRuleParameter>()).ToList());
            _userOverrides = new ReadOnlyCollection<UserRuleOverride>(
                (userOverrides ?? Enumerable.Empty<UserRuleOverride>()).ToList());
        }
    }
}
