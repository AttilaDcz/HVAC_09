using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Rules.DisciplineRules
{
    public enum CalculationRequestStatus { Requested, NotRequested }
    public enum CalculationApplicability { Applicable, NotApplicable }
    public enum InputRequirementKind { Required, Optional }

    public sealed class CalculationInputRequirement
    {
        public string InputId { get; }
        public InputRequirementKind RequirementKind { get; }

        public CalculationInputRequirement(
            string inputId,
            InputRequirementKind requirementKind)
        {
            if (string.IsNullOrWhiteSpace(inputId))
                throw new ArgumentException(
                    "Az InputId nem lehet üres.",
                    nameof(inputId));

            InputId = inputId.Trim();
            RequirementKind = requirementKind;
        }
    }

    public sealed class CalculationRequirement
    {
        private readonly ReadOnlyCollection<CalculationInputRequirement> _inputs;

        public string CalculationId { get; }
        public string MethodId { get; }
        public string SectionName { get; }
        public bool SupportsBuildingLevel { get; }
        public bool SupportsSpaceLevel { get; }
        public IReadOnlyList<CalculationInputRequirement> Inputs => _inputs;

        public CalculationRequirement(
            string calculationId,
            string methodId,
            string sectionName,
            bool supportsBuildingLevel,
            bool supportsSpaceLevel,
            IEnumerable<CalculationInputRequirement> inputs)
        {
            if (string.IsNullOrWhiteSpace(calculationId))
                throw new ArgumentException(
                    "A CalculationId nem lehet üres.",
                    nameof(calculationId));

            if (string.IsNullOrWhiteSpace(methodId))
                throw new ArgumentException(
                    "A MethodId nem lehet üres.",
                    nameof(methodId));

            CalculationId = calculationId.Trim();
            MethodId = methodId.Trim();
            SectionName = sectionName?.Trim() ?? string.Empty;
            SupportsBuildingLevel = supportsBuildingLevel;
            SupportsSpaceLevel = supportsSpaceLevel;
            _inputs = new ReadOnlyCollection<CalculationInputRequirement>(
                (inputs ?? Enumerable.Empty<CalculationInputRequirement>()).ToList());
        }
    }

    public sealed class CalculationRequest
    {
        private readonly ReadOnlyCollection<string> _requestedCalculationIds;
        private readonly ReadOnlyDictionary<string, string> _availableInputs;

        public string BuildingProfileKey { get; }
        public string SpaceProfileKey { get; }
        public IReadOnlyList<string> RequestedCalculationIds =>
            _requestedCalculationIds;
        public IReadOnlyDictionary<string, string> AvailableInputs =>
            _availableInputs;

        public CalculationRequest(
            string buildingProfileKey,
            string spaceProfileKey,
            IEnumerable<string> requestedCalculationIds,
            IDictionary<string, string> availableInputs)
        {
            BuildingProfileKey = buildingProfileKey?.Trim() ?? string.Empty;
            SpaceProfileKey = spaceProfileKey?.Trim() ?? string.Empty;

            _requestedCalculationIds = new ReadOnlyCollection<string>(
                (requestedCalculationIds ?? Enumerable.Empty<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList());

            _availableInputs = new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(
                    availableInputs ?? new Dictionary<string, string>(),
                    StringComparer.OrdinalIgnoreCase));
        }

        public bool IsRequested(string calculationId) =>
            !string.IsNullOrWhiteSpace(calculationId) &&
            _requestedCalculationIds.Contains(
                calculationId.Trim(),
                StringComparer.OrdinalIgnoreCase);
    }

    public sealed class CalculationResolution
    {
        public string CalculationId { get; }
        public CalculationRequestStatus RequestStatus { get; }
        public CalculationApplicability Applicability { get; }
        public string MethodId { get; }
        public IReadOnlyList<string> MissingRequiredInputs { get; }
        public IReadOnlyList<string> MissingOptionalInputs { get; }

        public bool CanRun =>
            RequestStatus == CalculationRequestStatus.Requested &&
            Applicability == CalculationApplicability.Applicable &&
            MissingRequiredInputs.Count == 0;

        public CalculationResolution(
            string calculationId,
            CalculationRequestStatus requestStatus,
            CalculationApplicability applicability,
            string methodId,
            IEnumerable<string> missingRequiredInputs,
            IEnumerable<string> missingOptionalInputs)
        {
            CalculationId = calculationId;
            RequestStatus = requestStatus;
            Applicability = applicability;
            MethodId = methodId?.Trim() ?? string.Empty;
            MissingRequiredInputs =
                (missingRequiredInputs ?? Enumerable.Empty<string>()).ToList();
            MissingOptionalInputs =
                (missingOptionalInputs ?? Enumerable.Empty<string>()).ToList();
        }
    }
}
