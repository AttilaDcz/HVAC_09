using System;

namespace HVACDesigner.EngineeringData.Rules.Common
{
    public enum RuleReferenceStatus
    {
        Active,
        Superseded,
        Withdrawn,
        LegacyApplicable,
        HistoricalReference,
        Draft
    }

    public sealed class RuleReference
    {
        public string Designation { get; }
        public string Title { get; }
        public string Edition { get; }
        public RuleReferenceStatus Status { get; }
        public string Jurisdiction { get; }
        public string ClauseReference { get; }
        public string DocumentationText { get; }

        public RuleReference(
            string designation,
            string title,
            string edition,
            RuleReferenceStatus status,
            string jurisdiction,
            string clauseReference,
            string documentationText)
        {
            if (string.IsNullOrWhiteSpace(designation))
                throw new ArgumentException("A megjelölés nem lehet üres.", nameof(designation));

            Designation = designation.Trim();
            Title = title?.Trim() ?? string.Empty;
            Edition = edition?.Trim() ?? string.Empty;
            Status = status;
            Jurisdiction = jurisdiction?.Trim() ?? string.Empty;
            ClauseReference = clauseReference?.Trim() ?? string.Empty;
            DocumentationText = documentationText?.Trim() ?? string.Empty;
        }

        public string ToDisplayText()
        {
            string text = Designation;
            if (!string.IsNullOrWhiteSpace(Edition))
                text += " (" + Edition + ")";
            if (!string.IsNullOrWhiteSpace(ClauseReference))
                text += ", " + ClauseReference;
            return text;
        }
    }
}
