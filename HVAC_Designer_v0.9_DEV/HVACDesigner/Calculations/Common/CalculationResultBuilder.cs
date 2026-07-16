using System;
using System.Collections.Generic;
using System.Linq;
using HVACDesigner.EngineeringData.Rules.Common;

namespace HVACDesigner.Calculations.Common
{
    public sealed class CalculationResultBuilder<T>
    {
        private readonly Dictionary<string, string> _inputs =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly List<string> _ruleSetKeys = new List<string>();
        private readonly List<string> _profileKeys = new List<string>();
        private readonly List<CalculationAssumption> _assumptions =
            new List<CalculationAssumption>();
        private readonly List<CalculationDiagnostic> _diagnostics =
            new List<CalculationDiagnostic>();

        private CalculationTrace _trace;

        public CalculationResultBuilder<T> AddInput(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A bemeneti adat neve nem lehet üres.", nameof(name));

            _inputs[name.Trim()] = value?.Trim() ?? string.Empty;
            return this;
        }

        public CalculationResultBuilder<T> UseRuleSet(string versionedRuleSetKey)
        {
            AddUnique(_ruleSetKeys, versionedRuleSetKey);
            return this;
        }

        public CalculationResultBuilder<T> UseProfile(string versionedProfileKey)
        {
            AddUnique(_profileKeys, versionedProfileKey);
            return this;
        }

        public CalculationResultBuilder<T> AddAssumption(
            CalculationAssumption assumption)
        {
            _assumptions.Add(
                assumption ?? throw new ArgumentNullException(nameof(assumption)));
            return this;
        }

        public CalculationResultBuilder<T> AddDiagnostic(
            CalculationDiagnostic diagnostic)
        {
            _diagnostics.Add(
                diagnostic ?? throw new ArgumentNullException(nameof(diagnostic)));
            return this;
        }

        public CalculationResultBuilder<T> WithTrace(CalculationTrace trace)
        {
            _trace = trace;
            return this;
        }

        public CalculationResult<T> Build(T result)
        {
            return new CalculationResult<T>(
                result,
                ResolveStatus(),
                _inputs,
                _ruleSetKeys,
                _profileKeys,
                _assumptions,
                _diagnostics,
                _trace);
        }

        public CalculationResult<T> BuildWaitingForInput(T partialResult)
        {
            if (!_diagnostics.Any(item =>
                item.Severity == CalculationDiagnosticSeverity.Error))
            {
                _diagnostics.Add(
                    new CalculationDiagnostic(
                        "CALCULATION_WAITING_FOR_INPUT",
                        "A számításhoz szükséges legalább egy kötelező adat hiányzik.",
                        CalculationDiagnosticSeverity.Error));
            }

            return new CalculationResult<T>(
                partialResult,
                CalculationStatus.WaitingForInput,
                _inputs,
                _ruleSetKeys,
                _profileKeys,
                _assumptions,
                _diagnostics,
                _trace);
        }

        private CalculationStatus ResolveStatus()
        {
            if (_diagnostics.Any(item =>
                item.Severity == CalculationDiagnosticSeverity.Error))
                return CalculationStatus.Failed;

            if (_diagnostics.Any(item =>
                item.Severity == CalculationDiagnosticSeverity.Warning))
                return CalculationStatus.SuccessWithWarnings;

            return CalculationStatus.Success;
        }

        private static void AddUnique(ICollection<string> target, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A kulcs nem lehet üres.", nameof(value));

            string normalized = value.Trim();

            if (!target.Any(item =>
                string.Equals(item, normalized, StringComparison.OrdinalIgnoreCase)))
                target.Add(normalized);
        }
    }
}
