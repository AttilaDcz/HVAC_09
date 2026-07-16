using System;
using System.Collections.Generic;
using System.Globalization;
using HVACDesigner.EngineeringData.Rules.Common;

namespace HVACDesigner.Calculations.Common
{
    public enum AdvisorAiLevel
    {
        None,
        RuleCheck,
        Recommendation,
        Assistant
    }

    public sealed class AdvisorEvaluation
    {
        public AdvisorAiLevel AiLevel { get; }
        public string LimitText { get; }
        public string RecommendationText { get; }
        public List<CalculationDiagnostic> Diagnostics { get; }

        public AdvisorEvaluation(
            AdvisorAiLevel aiLevel,
            string limitText,
            string recommendationText,
            List<CalculationDiagnostic> diagnostics)
        {
            AiLevel = aiLevel;
            LimitText = limitText ?? string.Empty;
            RecommendationText = recommendationText ?? string.Empty;
            Diagnostics = diagnostics ?? new List<CalculationDiagnostic>();
        }
    }

    /// <summary>
    /// Modul-független mérnöki szabály- és ajánlásellenőrző asszisztens (Level 1 és Level 2).
    /// </summary>
    public static class EngineeringAdvisor
    {
        public static AdvisorEvaluation Evaluate(
            string variableName,
            double? value,
            RuleParameterSet parameters,
            string unitLabel = "")
        {
            var diagnostics = new List<CalculationDiagnostic>();

            if (parameters == null)
            {
                return new AdvisorEvaluation(
                    AdvisorAiLevel.None,
                    string.Empty,
                    string.Empty,
                    diagnostics);
            }

            // Paraméter kulcsok dinamikus összeállítása
            string limitMaxKey = $"Limit.{variableName}.Max";
            string limitMinKey = $"Limit.{variableName}.Min";
            string recommendationMaxKey = $"Recommendation.{variableName}.Max";
            string recommendationMinKey = $"Recommendation.{variableName}.Min";
            string severityKey = $"Limit.{variableName}.Severity";
            string descriptionKey = $"Description.{variableName}";

            double? maxLimit = GetDouble(parameters, limitMaxKey);
            double? minLimit = GetDouble(parameters, limitMinKey);
            string description = parameters.GetStringOrDefault(descriptionKey, variableName);

            // Határérték szöveg meghatározása (L1 szint)
            string limitText = string.Empty;
            if (minLimit.HasValue && maxLimit.HasValue)
                limitText = $"Ajánlott: {minLimit.Value:0.###}-{maxLimit.Value:0.###} {unitLabel}".Trim();
            else if (maxLimit.HasValue)
                limitText = $"max. {maxLimit.Value:0.###} {unitLabel}".Trim();
            else if (minLimit.HasValue)
                limitText = $"min. {minLimit.Value:0.###} {unitLabel}".Trim();

            if (!value.HasValue)
            {
                return new AdvisorEvaluation(
                    AdvisorAiLevel.RuleCheck,
                    limitText,
                    string.Empty,
                    diagnostics);
            }

            double val = value.Value;
            AdvisorAiLevel level = AdvisorAiLevel.RuleCheck;
            string recommendationText = string.Empty;

            // Diagnosztikai súlyosság (Warning vagy Error)
            string severityStr = parameters.GetStringOrDefault(severityKey, "Warning");
            CalculationDiagnosticSeverity severity = string.Equals(severityStr, "Error", StringComparison.OrdinalIgnoreCase)
                ? CalculationDiagnosticSeverity.Error
                : CalculationDiagnosticSeverity.Warning;

            // Felső határérték túllépés ellenőrzése
            if (maxLimit.HasValue && val > maxLimit.Value)
            {
                level = AdvisorAiLevel.Recommendation;
                recommendationText = parameters.GetStringOrDefault(
                    recommendationMaxKey,
                    $"A(z) {description} értéke ({val:0.###}) magasabb a megengedett felső határértéknél ({maxLimit.Value:0.###}).");

                diagnostics.Add(new CalculationDiagnostic(
                    $"{variableName.ToUpperInvariant()}_TOO_HIGH",
                    $"A(z) {description} ({val:0.###}) magasabb az ajánlott felső határértéknél ({maxLimit.Value:0.###}).",
                    severity));
            }
            // Alsó határérték alatti érték ellenőrzése
            else if (minLimit.HasValue && val < minLimit.Value)
            {
                level = AdvisorAiLevel.Recommendation;
                recommendationText = parameters.GetStringOrDefault(
                    recommendationMinKey,
                    $"A(z) {description} értéke ({val:0.###}) alacsonyabb a megengedett alsó határértéknél ({minLimit.Value:0.###}).");

                diagnostics.Add(new CalculationDiagnostic(
                    $"{variableName.ToUpperInvariant()}_TOO_LOW",
                    $"A(z) {description} ({val:0.###}) elmarad az ajánlott alsó határértéktől ({minLimit.Value:0.###}).",
                    severity));
            }
            else
            {
                // Ha tartományon belül van
                if (maxLimit.HasValue || minLimit.HasValue)
                {
                    recommendationText = "Megfelel a határértékeknek.";
                }
            }

            return new AdvisorEvaluation(
                level,
                limitText,
                recommendationText,
                diagnostics);
        }

        private static double? GetDouble(RuleParameterSet parameters, string key)
        {
            string val = parameters.GetStringOrDefault(key, string.Empty);
            if (string.IsNullOrWhiteSpace(val))
                return null;

            if (double.TryParse(
                val,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}
