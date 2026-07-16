using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HVACDesigner.EngineeringData.Rules.Common;

namespace HVACDesigner.Calculations.Common
{
    public enum CalculationStatus
    {
        NotRequested,
        NotApplicable,
        WaitingForInput,
        Success,
        SuccessWithWarnings,
        Failed
    }

    public enum CalculationDiagnosticSeverity
    {
        Information,
        Warning,
        Error
    }

    public sealed class CalculationDiagnostic
    {
        public string Code { get; }
        public string Message { get; }
        public CalculationDiagnosticSeverity Severity { get; }
        public string FieldName { get; }

        public CalculationDiagnostic(
            string code,
            string message,
            CalculationDiagnosticSeverity severity,
            string fieldName = "")
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("A diagnosztikai kód nem lehet üres.", nameof(code));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("A diagnosztikai üzenet nem lehet üres.", nameof(message));

            Code = code.Trim();
            Message = message.Trim();
            Severity = severity;
            FieldName = fieldName?.Trim() ?? string.Empty;
        }
    }

    public sealed class CalculationAssumption
    {
        public string Name { get; }
        public string Value { get; }
        public string Unit { get; }
        public string Source { get; }
        public bool IsUserConfirmed { get; }

        public CalculationAssumption(
            string name,
            string value,
            string unit,
            string source,
            bool isUserConfirmed)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A feltételezés neve nem lehet üres.", nameof(name));

            Name = name.Trim();
            Value = value?.Trim() ?? string.Empty;
            Unit = unit?.Trim() ?? string.Empty;
            Source = source?.Trim() ?? string.Empty;
            IsUserConfirmed = isUserConfirmed;
        }
    }

    public sealed class CalculationResult<T>
    {
        private readonly ReadOnlyDictionary<string, string> _inputs;
        private readonly ReadOnlyCollection<string> _usedRuleSetKeys;
        private readonly ReadOnlyCollection<string> _usedProfileKeys;
        private readonly ReadOnlyCollection<CalculationAssumption> _assumptions;
        private readonly ReadOnlyCollection<CalculationDiagnostic> _diagnostics;

        public T Result { get; }
        public CalculationStatus Status { get; }
        public IReadOnlyDictionary<string, string> Inputs => _inputs;
        public IReadOnlyList<string> UsedRuleSetKeys => _usedRuleSetKeys;
        public IReadOnlyList<string> UsedProfileKeys => _usedProfileKeys;
        public IReadOnlyList<CalculationAssumption> Assumptions => _assumptions;
        public IReadOnlyList<CalculationDiagnostic> Diagnostics => _diagnostics;
        public CalculationTrace Trace { get; }

        public bool Succeeded =>
            Status == CalculationStatus.Success ||
            Status == CalculationStatus.SuccessWithWarnings;

        public CalculationResult(
            T result,
            CalculationStatus status,
            IDictionary<string, string> inputs,
            IEnumerable<string> usedRuleSetKeys,
            IEnumerable<string> usedProfileKeys,
            IEnumerable<CalculationAssumption> assumptions,
            IEnumerable<CalculationDiagnostic> diagnostics,
            CalculationTrace trace = null)
        {
            Result = result;
            Status = status;

            _inputs = new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(
                    inputs ?? new Dictionary<string, string>(),
                    StringComparer.OrdinalIgnoreCase));

            _usedRuleSetKeys = new ReadOnlyCollection<string>(
                NormalizeKeys(usedRuleSetKeys));

            _usedProfileKeys = new ReadOnlyCollection<string>(
                NormalizeKeys(usedProfileKeys));

            _assumptions = new ReadOnlyCollection<CalculationAssumption>(
                (assumptions ?? Enumerable.Empty<CalculationAssumption>()).ToList());

            _diagnostics = new ReadOnlyCollection<CalculationDiagnostic>(
                (diagnostics ?? Enumerable.Empty<CalculationDiagnostic>()).ToList());

            Trace = trace;
            ValidateStatus();
        }

        private void ValidateStatus()
        {
            bool hasErrors = _diagnostics.Any(item =>
                item.Severity == CalculationDiagnosticSeverity.Error);

            bool hasWarnings = _diagnostics.Any(item =>
                item.Severity == CalculationDiagnosticSeverity.Warning);

            if (Status == CalculationStatus.Success && (hasErrors || hasWarnings))
                throw new ArgumentException(
                    "Success állapot nem tartalmazhat warning vagy error diagnosztikát.");

            if (Status == CalculationStatus.SuccessWithWarnings && hasErrors)
                throw new ArgumentException(
                    "SuccessWithWarnings állapot nem tartalmazhat error diagnosztikát.");

            if (Status == CalculationStatus.Failed && !hasErrors)
                throw new ArgumentException(
                    "Failed állapothoz legalább egy error diagnosztika szükséges.");
        }

        private static List<string> NormalizeKeys(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
