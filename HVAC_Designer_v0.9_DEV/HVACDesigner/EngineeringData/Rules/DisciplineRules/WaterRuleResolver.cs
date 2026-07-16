using System;
using System.Collections.Generic;
using System.Linq;
using HVACDesigner.EngineeringData.Profiles;

namespace HVACDesigner.EngineeringData.Rules.DisciplineRules
{
    public sealed class WaterRuleResolver
    {
        private readonly Dictionary<WaterCalculationId, WaterCalculationRule>
            _rules;

        public WaterRuleResolver(
            IEnumerable<WaterCalculationRule> rules)
        {
            _rules =
                (rules ??
                 throw new ArgumentNullException(nameof(rules)))
                .ToDictionary(item => item.CalculationId);
        }

        public IReadOnlyList<WaterCalculationResolution> Resolve(
            WaterCalculationRequest request,
            ProfileResolutionMode resolutionMode)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var result =
                new List<WaterCalculationResolution>();

            foreach (WaterCalculationRule rule in _rules.Values)
            {
                if (!request.IsRequested(rule.CalculationId))
                {
                    result.Add(
                        new WaterCalculationResolution(
                            rule.CalculationId,
                            WaterResolutionStatus.NotRequested,
                            rule.MethodId,
                            Array.Empty<string>(),
                            Array.Empty<string>()));
                    continue;
                }

                if (!IsApplicable(rule, resolutionMode) ||
                    (rule.OptionalFeature &&
                     !request.IsFeatureEnabled(rule.FeatureFlag)))
                {
                    result.Add(
                        new WaterCalculationResolution(
                            rule.CalculationId,
                            WaterResolutionStatus.NotApplicable,
                            rule.MethodId,
                            Array.Empty<string>(),
                            Array.Empty<string>()));
                    continue;
                }

                var missingRequired = new List<string>();
                var missingOptional = new List<string>();

                foreach (WaterInputRequirement input in rule.Inputs)
                {
                    bool present =
                        request.AvailableInputs.TryGetValue(
                            input.InputId,
                            out string value) &&
                        !string.IsNullOrWhiteSpace(value);

                    if (present)
                        continue;

                    if (input.RequirementKind ==
                        WaterInputRequirementKind.Required)
                    {
                        missingRequired.Add(input.InputId);
                    }
                    else
                    {
                        missingOptional.Add(input.InputId);
                    }
                }

                result.Add(
                    new WaterCalculationResolution(
                        rule.CalculationId,
                        missingRequired.Count == 0
                            ? WaterResolutionStatus.Ready
                            : WaterResolutionStatus.WaitingForInput,
                        rule.MethodId,
                        missingRequired,
                        missingOptional));
            }

            return result;
        }

        private static bool IsApplicable(
            WaterCalculationRule rule,
            ProfileResolutionMode resolutionMode)
        {
            switch (resolutionMode)
            {
                case ProfileResolutionMode.BuildingLevel:
                    return rule.SupportsBuildingLevel;

                case ProfileResolutionMode.SpaceLevel:
                    return rule.SupportsSpaceLevel;

                case ProfileResolutionMode.Mixed:
                    return rule.SupportsBuildingLevel ||
                           rule.SupportsSpaceLevel;

                default:
                    return false;
            }
        }
    }
}
