using System;
using System.Collections.Generic;
using System.Linq;
using HVACDesigner.EngineeringData.Profiles;

namespace HVACDesigner.EngineeringData.Rules.DisciplineRules
{
    public sealed class BuildingRuleResolver
    {
        private readonly Dictionary<string, CalculationRequirement> _requirements;

        public BuildingRuleResolver(
            IEnumerable<CalculationRequirement> requirements)
        {
            _requirements =
                (requirements ?? throw new ArgumentNullException(nameof(requirements)))
                .ToDictionary(
                    item => item.CalculationId,
                    StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<CalculationResolution> Resolve(
            CalculationRequest request,
            ProfileResolutionMode resolutionMode)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var result = new List<CalculationResolution>();

            foreach (CalculationRequirement requirement in _requirements.Values)
            {
                if (!request.IsRequested(requirement.CalculationId))
                {
                    result.Add(new CalculationResolution(
                        requirement.CalculationId,
                        CalculationRequestStatus.NotRequested,
                        CalculationApplicability.Applicable,
                        requirement.MethodId,
                        Array.Empty<string>(),
                        Array.Empty<string>()));
                    continue;
                }

                if (!IsApplicable(requirement, resolutionMode))
                {
                    result.Add(new CalculationResolution(
                        requirement.CalculationId,
                        CalculationRequestStatus.Requested,
                        CalculationApplicability.NotApplicable,
                        requirement.MethodId,
                        Array.Empty<string>(),
                        Array.Empty<string>()));
                    continue;
                }

                var missingRequired = new List<string>();
                var missingOptional = new List<string>();

                foreach (CalculationInputRequirement input in requirement.Inputs)
                {
                    bool present =
                        request.AvailableInputs.TryGetValue(
                            input.InputId,
                            out string value) &&
                        !string.IsNullOrWhiteSpace(value);

                    if (present)
                        continue;

                    if (input.RequirementKind == InputRequirementKind.Required)
                        missingRequired.Add(input.InputId);
                    else
                        missingOptional.Add(input.InputId);
                }

                result.Add(new CalculationResolution(
                    requirement.CalculationId,
                    CalculationRequestStatus.Requested,
                    CalculationApplicability.Applicable,
                    requirement.MethodId,
                    missingRequired,
                    missingOptional));
            }

            return result;
        }

        private static bool IsApplicable(
            CalculationRequirement requirement,
            ProfileResolutionMode resolutionMode)
        {
            switch (resolutionMode)
            {
                case ProfileResolutionMode.BuildingLevel:
                    return requirement.SupportsBuildingLevel;
                case ProfileResolutionMode.SpaceLevel:
                    return requirement.SupportsSpaceLevel;
                case ProfileResolutionMode.Mixed:
                    return requirement.SupportsBuildingLevel ||
                           requirement.SupportsSpaceLevel;
                default:
                    return false;
            }
        }
    }
}
