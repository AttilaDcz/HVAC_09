using System;
using System.Globalization;
using System.Linq;
using HVACDesigner.Calculations.Common;
using HVACDesigner.Calculations.Water;
using HVACDesigner.CoreUI.Components.Results;

namespace HVACDesigner.Features.Water
{
    public sealed class WaterResultPresentationAdapter
    {
        public EngineeringResultCardModel CreateDailyDemand(
            CalculationResult<DailyWaterDemandResult> result,
            WaterRuleInfo rule)
        {
            string value = result.Result == null
                ? "-"
                : result.Result.DailyDemandCubicMetres.ToString(
                    "0.00",
                    CultureInfo.CurrentCulture);

            string subtitle = result.Result == null
                ? "További adat szükséges."
                : result.Result.OccupancyWasEstimated
                    ? $"{result.Result.Occupancy:0.##} {result.Result.DemandUnitLabel}, profilból becsülve"
                    : $"{result.Result.Occupancy:0.##} {result.Result.DemandUnitLabel}";

            double? perPerson = (result.Result != null && result.Result.Occupancy > 0)
                ? (double?)(result.Result.DailyDemandLitres / result.Result.Occupancy)
                : null;

            return Create(
                "Napi ivóvízigény",
                value,
                "m³/nap",
                subtitle,
                result,
                rule,
                advisorVariableName: "DailyWaterPerPerson",
                advisorValue: perPerson,
                advisorUnitLabel: "L/fő/nap");
        }

        public EngineeringResultCardModel CreateDhwDemand(
            CalculationResult<DhwDemandResult> result,
            WaterRuleInfo rule)
        {
            string value = result.Result == null
                ? "-"
                : result.Result.DailyDhwVolumeCubicMetres.ToString(
                    "0.00",
                    CultureInfo.CurrentCulture);

            string energyText = result.Result == null
                ? "-"
                : result.Result.DailyDhwEnergyKwh.ToString(
                    "0.0",
                    CultureInfo.CurrentCulture);

            string subtitle = result.Result == null
                ? "További adat szükséges."
                : $"Felmelegítés napi energiaigénye: {energyText} kWh/nap";

            double? perPerson = (result.Result != null && result.Result.Occupancy > 0)
                ? (double?)(result.Result.DailyDhwVolumeLitres / result.Result.Occupancy)
                : null;

            return Create(
                "Használati melegvíz",
                value,
                "m³/nap",
                subtitle,
                result,
                rule,
                advisorVariableName: "DailyHotWaterPerPerson",
                advisorValue: perPerson,
                advisorUnitLabel: "L/fő/nap");
        }

        public EngineeringResultCardModel CreatePeakDemand(
            CalculationResult<PeakWaterDemandResult> result,
            WaterRuleInfo rule)
        {
            string value = result.Result == null
                ? "-"
                : result.Result.DesignFlowLitresPerSecond.ToString(
                    "0.000",
                    CultureInfo.CurrentCulture);

            string subtitle = result.Result == null
                ? "Szerelvényadat szükséges."
                : $"Összes terhelés: {result.Result.TotalLoadingUnits:0.###} LU";

            return Create(
                "Mértékadó ivóvízhozam",
                value,
                "l/s",
                subtitle,
                result,
                rule);
        }

        public EngineeringResultCardModel CreateWastewater(
            CalculationResult<WastewaterFlowResult> result,
            WaterRuleInfo rule)
        {
            string value = result.Result == null
                ? "-"
                : result.Result.DesignFlowLitresPerSecond.ToString(
                    "0.000",
                    CultureInfo.CurrentCulture);

            string subtitle = result.Result == null
                ? "Szerelvény- vagy szabályadat szükséges."
                : $"Összes terhelés: {result.Result.TotalDischargeUnits:0.###} DU";

            double? peakValue = result.Result != null
                ? (double?)result.Result.DesignFlowLitresPerSecond
                : null;

            return Create(
                "Mértékadó szennyvízhozam",
                value,
                "l/s",
                subtitle,
                result,
                rule,
                advisorVariableName: "WastewaterPeak",
                advisorValue: peakValue,
                advisorUnitLabel: "l/s");
        }

        public EngineeringResultCardModel CreateMinimumConnection(
            CalculationResult<WastewaterFlowResult> result,
            WaterRuleInfo rule)
        {
            string value = result.Result == null
                ? "-"
                : result.Result.MinimumRequiredDiameter.ToString(
                    CultureInfo.CurrentCulture);

            return Create(
                "Bekötési minimum",
                value,
                result.Result == null ? string.Empty : "DN",
                "Legnagyobb szerelvénybekötési minimum; nem fővezeték-méret.",
                result,
                rule);
        }

        public EngineeringResultCardModel CreateGreywater(
            CalculationResult<GreywaterBalanceResult> result,
            WaterRuleInfo rule)
        {
            string value = result.Result == null
                ? "-"
                : result.Result.ReusableGreywaterLitresPerDay.ToString(
                    "0",
                    CultureInfo.CurrentCulture);

            string subtitle = result.Result == null
                ? "Szürkevíz számítás nincs bekapcsolva vagy adat hiányzik."
                : $"Forrás: {result.Result.EligibleSupplyLitresPerDay:0} l/nap · igény: {result.Result.EligibleDemandLitresPerDay:0} l/nap";

            return Create(
                "Szürkevíz mérleg",
                value,
                result.Result == null ? string.Empty : "l/nap",
                subtitle,
                result,
                rule);
        }

        public EngineeringResultCardModel CreateRoofDrainage(
            CalculationResult<RoofDrainageResult> result,
            WaterRuleInfo rule)
        {
            string value = result.Result == null
                ? "-"
                : result.Result.DesignFlowLitresPerSecond.ToString(
                    "0.000",
                    CultureInfo.CurrentCulture);

            string subtitle = result.Result == null
                ? "Tetővíz számítás nincs bekapcsolva vagy adat hiányzik."
                : $"Tetőfelület: {result.Result.TotalEffectiveAreaSquareMetres:0.##} m²";

            double? rainfallIntensity = null;
            if (result.Inputs.TryGetValue("RainfallIntensity", out string valStr) &&
                double.TryParse(valStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
            {
                rainfallIntensity = val;
            }

            return Create(
                "Tetővíz hozam",
                value,
                result.Result == null ? string.Empty : "l/s",
                subtitle,
                result,
                rule,
                advisorVariableName: "RainfallIntensity",
                advisorValue: rainfallIntensity,
                advisorUnitLabel: "l/(s·m²)");
        }

        public EngineeringResultCardModel CreatePending(
            string title)
        {
            return new EngineeringResultCardModel(
                title,
                "-",
                string.Empty,
                EngineeringResultStatus.Neutral,
                "Az adatok módosultak. Futtasd újra a számítást.");
        }

        public EngineeringResultCardModel CreateFailure(
            string title,
            string message)
        {
            return new EngineeringResultCardModel(
                title,
                "-",
                string.Empty,
                EngineeringResultStatus.Danger,
                "A számítás nem indítható.",
                diagnostics:
                    new[]
                    {
                        new EngineeringResultDiagnostic(
                            "WATER_MODULE_ERROR",
                            message,
                            EngineeringResultDiagnosticSeverity.Error)
                    });
        }

        private static EngineeringResultCardModel Create<T>(
            string title,
            string value,
            string unit,
            string subtitle,
            CalculationResult<T> result,
            WaterRuleInfo rule,
            string advisorVariableName = null,
            double? advisorValue = null,
            string advisorUnitLabel = "")
        {
            var diagnostics = result.Diagnostics
                .Select(MapDiagnostic)
                .ToList();

            if (rule.UsesMvpData)
            {
                diagnostics.Add(
                    new EngineeringResultDiagnostic(
                        "WATER_RULE_MVP_DATA",
                        "MVP minta-paraméterek; szakmai tervezéshez még nem hitelesített.",
                        EngineeringResultDiagnosticSeverity.Warning));
            }

            // --- L1/L2 mérnöki szabály- és ajánlásellenőrző futtatása ---
            AdvisorEvaluation advisor = null;
            if (!string.IsNullOrEmpty(advisorVariableName))
            {
                advisor = EngineeringAdvisor.Evaluate(
                    advisorVariableName,
                    advisorValue,
                    rule.Parameters,
                    advisorUnitLabel);

                // Hozzáadjuk az advisor által generált figyelmeztetéseket/hibákat a kártyához
                foreach (var diag in advisor.Diagnostics)
                {
                    diagnostics.Add(MapDiagnostic(diag));
                }
            }

            // Státusz felülbírálása, ha az advisor figyelmeztetést vagy hibát talált
            EngineeringResultStatus status = MapStatus(result.Status, rule.UsesMvpData);
            if (advisor != null)
            {
                if (advisor.AiLevel == AdvisorAiLevel.Recommendation)
                {
                    if (status == EngineeringResultStatus.Success || status == EngineeringResultStatus.Neutral)
                    {
                        // Ha van benne error súlyosságú diagnosztika, akkor Danger, egyébként Warning
                        bool hasError = advisor.Diagnostics.Any(d => d.Severity == CalculationDiagnosticSeverity.Error);
                        status = hasError ? EngineeringResultStatus.Danger : EngineeringResultStatus.Warning;
                    }
                }
            }

            return new EngineeringResultCardModel(
                title,
                value,
                unit,
                status,
                subtitle,
                sourceText:
                    rule.DisplayName +
                    " · " +
                    rule.RuleSetId +
                    "@" +
                    rule.Version,
                recommendationText:
                    advisor != null && !string.IsNullOrEmpty(advisor.RecommendationText)
                        ? advisor.RecommendationText
                        : rule.UsesMvpData
                            ? "Integrációs teszthez használható."
                            : string.Empty,
                aiLevel:
                    advisor != null
                        ? MapAiLevel(advisor.AiLevel)
                        : EngineeringAiSupportLevel.RuleCheck,
                limitText:
                    advisor != null ? advisor.LimitText : string.Empty,
                diagnostics:
                    diagnostics,
                references:
                    rule.References.Select(reference =>
                        new EngineeringResultReference(reference)));
        }

        private static EngineeringAiSupportLevel MapAiLevel(AdvisorAiLevel level)
        {
            switch (level)
            {
                case AdvisorAiLevel.RuleCheck:
                    return EngineeringAiSupportLevel.RuleCheck;
                case AdvisorAiLevel.Recommendation:
                    return EngineeringAiSupportLevel.Recommendation;
                case AdvisorAiLevel.Assistant:
                    return EngineeringAiSupportLevel.Assistant;
                default:
                    return EngineeringAiSupportLevel.None;
            }
        }

        private static EngineeringResultDiagnostic MapDiagnostic(
            CalculationDiagnostic diagnostic)
        {
            EngineeringResultDiagnosticSeverity severity =
                diagnostic.Severity ==
                    CalculationDiagnosticSeverity.Error
                    ? EngineeringResultDiagnosticSeverity.Error
                    : diagnostic.Severity ==
                        CalculationDiagnosticSeverity.Warning
                        ? EngineeringResultDiagnosticSeverity.Warning
                        : EngineeringResultDiagnosticSeverity.Info;

            return new EngineeringResultDiagnostic(
                diagnostic.Code,
                diagnostic.Message,
                severity);
        }

        private static EngineeringResultStatus MapStatus(
            CalculationStatus status,
            bool usesMvpData)
        {
            switch (status)
            {
                case CalculationStatus.Success:
                    return usesMvpData
                        ? EngineeringResultStatus.Warning
                        : EngineeringResultStatus.Success;

                case CalculationStatus.SuccessWithWarnings:
                    return EngineeringResultStatus.Warning;

                case CalculationStatus.WaitingForInput:
                    return EngineeringResultStatus.Info;

                case CalculationStatus.Failed:
                    return EngineeringResultStatus.Danger;

                default:
                    return EngineeringResultStatus.Neutral;
            }
        }
    }
}

