using System;

namespace HVACDesigner.CoreUI.Status
{
    public enum EngineeringStatusSeverity
    {
        Neutral,
        Info,
        Success,
        Warning,
        Danger
    }

    public sealed class EngineeringStatusState
    {
        public EngineeringStatusState(
            string projectName = "",
            string moduleName = "Munkatér",
            string message = "Készen áll",
            EngineeringStatusSeverity severity = EngineeringStatusSeverity.Neutral,
            string dataState = "",
            string unitSummary = "",
            bool recalculationSuggested = false,
            DateTime? updatedUtc = null)
        {
            ProjectName = projectName?.Trim() ?? string.Empty;
            ModuleName = string.IsNullOrWhiteSpace(moduleName)
                ? "Munkatér"
                : moduleName.Trim();
            Message = string.IsNullOrWhiteSpace(message)
                ? "Készen áll"
                : message.Trim();
            Severity = severity;
            DataState = dataState?.Trim() ?? string.Empty;
            UnitSummary = unitSummary?.Trim() ?? string.Empty;
            RecalculationSuggested = recalculationSuggested;
            UpdatedUtc = (updatedUtc ?? DateTime.UtcNow).ToUniversalTime();
        }

        public string ProjectName { get; }
        public string ModuleName { get; }
        public string Message { get; }
        public EngineeringStatusSeverity Severity { get; }
        public string DataState { get; }
        public string UnitSummary { get; }
        public bool RecalculationSuggested { get; }
        public DateTime UpdatedUtc { get; }

        public EngineeringStatusState With(
            string? projectName = null,
            string? moduleName = null,
            string? message = null,
            EngineeringStatusSeverity? severity = null,
            string? dataState = null,
            string? unitSummary = null,
            bool? recalculationSuggested = null)
        {
            return new EngineeringStatusState(
                projectName ?? ProjectName,
                moduleName ?? ModuleName,
                message ?? Message,
                severity ?? Severity,
                dataState ?? DataState,
                unitSummary ?? UnitSummary,
                recalculationSuggested ?? RecalculationSuggested);
        }
    }
}
