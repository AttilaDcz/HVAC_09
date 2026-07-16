using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Rules.DisciplineRules
{
    public enum WaterCalculationId
    {
        DailyWaterDemand,
        PeakWaterDemand,
        PotableWaterPipeSizing,
        DailyHotWaterDemand,
        PeakHotWaterDemand,
        WastewaterLoad,
        RoofDrainage,
        GreywaterBalance,
        BuildingDrainageOutlet
    }

    public enum WaterInputRequirementKind
    {
        Required,
        Optional
    }

    public enum WaterResolutionStatus
    {
        NotRequested,
        NotApplicable,
        WaitingForInput,
        Ready
    }

    public sealed class WaterInputRequirement
    {
        public string InputId { get; }
        public WaterInputRequirementKind RequirementKind { get; }

        public WaterInputRequirement(
            string inputId,
            WaterInputRequirementKind requirementKind)
        {
            if (string.IsNullOrWhiteSpace(inputId))
                throw new ArgumentException(
                    "Az InputId nem lehet üres.",
                    nameof(inputId));

            InputId = inputId.Trim();
            RequirementKind = requirementKind;
        }
    }

    public sealed class WaterCalculationRule
    {
        private readonly ReadOnlyCollection<WaterInputRequirement> _inputs;

        public WaterCalculationId CalculationId { get; }
        public string MethodId { get; }
        public bool SupportsBuildingLevel { get; }
        public bool SupportsSpaceLevel { get; }
        public bool OptionalFeature { get; }
        public string FeatureFlag { get; }
        public IReadOnlyList<WaterInputRequirement> Inputs => _inputs;

        public WaterCalculationRule(
            WaterCalculationId calculationId,
            string methodId,
            bool supportsBuildingLevel,
            bool supportsSpaceLevel,
            bool optionalFeature,
            string featureFlag,
            IEnumerable<WaterInputRequirement> inputs)
        {
            if (string.IsNullOrWhiteSpace(methodId))
                throw new ArgumentException(
                    "A MethodId nem lehet üres.",
                    nameof(methodId));

            CalculationId = calculationId;
            MethodId = methodId.Trim();
            SupportsBuildingLevel = supportsBuildingLevel;
            SupportsSpaceLevel = supportsSpaceLevel;
            OptionalFeature = optionalFeature;
            FeatureFlag = featureFlag?.Trim() ?? string.Empty;

            _inputs = new ReadOnlyCollection<WaterInputRequirement>(
                (inputs ?? Enumerable.Empty<WaterInputRequirement>())
                .ToList());
        }
    }

    public sealed class WaterCalculationRequest
    {
        private readonly ReadOnlyCollection<WaterCalculationId>
            _requestedCalculations;

        private readonly ReadOnlyDictionary<string, string>
            _availableInputs;

        private readonly ReadOnlyDictionary<string, bool>
            _featureFlags;

        public IReadOnlyList<WaterCalculationId> RequestedCalculations =>
            _requestedCalculations;

        public IReadOnlyDictionary<string, string> AvailableInputs =>
            _availableInputs;

        public IReadOnlyDictionary<string, bool> FeatureFlags =>
            _featureFlags;

        public WaterCalculationRequest(
            IEnumerable<WaterCalculationId> requestedCalculations,
            IDictionary<string, string> availableInputs,
            IDictionary<string, bool> featureFlags)
        {
            _requestedCalculations =
                new ReadOnlyCollection<WaterCalculationId>(
                    (requestedCalculations ??
                     Enumerable.Empty<WaterCalculationId>())
                    .Distinct()
                    .ToList());

            _availableInputs =
                new ReadOnlyDictionary<string, string>(
                    new Dictionary<string, string>(
                        availableInputs ??
                        new Dictionary<string, string>(),
                        StringComparer.OrdinalIgnoreCase));

            _featureFlags =
                new ReadOnlyDictionary<string, bool>(
                    new Dictionary<string, bool>(
                        featureFlags ??
                        new Dictionary<string, bool>(),
                        StringComparer.OrdinalIgnoreCase));
        }

        public bool IsRequested(WaterCalculationId id) =>
            _requestedCalculations.Contains(id);

        public bool IsFeatureEnabled(string featureFlag)
        {
            if (string.IsNullOrWhiteSpace(featureFlag))
                return true;

            return _featureFlags.TryGetValue(
                       featureFlag.Trim(),
                       out bool enabled) &&
                   enabled;
        }
    }

    public sealed class WaterCalculationResolution
    {
        public WaterCalculationId CalculationId { get; }
        public WaterResolutionStatus Status { get; }
        public string MethodId { get; }
        public IReadOnlyList<string> MissingRequiredInputs { get; }
        public IReadOnlyList<string> MissingOptionalInputs { get; }

        public bool CanRun => Status == WaterResolutionStatus.Ready;

        public WaterCalculationResolution(
            WaterCalculationId calculationId,
            WaterResolutionStatus status,
            string methodId,
            IEnumerable<string> missingRequiredInputs,
            IEnumerable<string> missingOptionalInputs)
        {
            CalculationId = calculationId;
            Status = status;
            MethodId = methodId?.Trim() ?? string.Empty;

            MissingRequiredInputs =
                (missingRequiredInputs ??
                 Enumerable.Empty<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            MissingOptionalInputs =
                (missingOptionalInputs ??
                 Enumerable.Empty<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
