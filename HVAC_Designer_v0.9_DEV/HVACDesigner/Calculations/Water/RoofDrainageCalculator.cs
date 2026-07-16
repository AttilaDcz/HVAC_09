using System.Linq;
using HVACDesigner.Calculations.Common;

namespace HVACDesigner.Calculations.Water
{
    public sealed class RoofDrainageCalculator
    {
        public CalculationResult<RoofDrainageResult> Calculate(
            RoofDrainageInput input,
            string ruleSetKey)
        {
            var diagnostics =
                WaterValidation.ValidateRoofDrainage(input);

            var builder =
                new CalculationResultBuilder<RoofDrainageResult>()
                .UseRuleSet(ruleSetKey);

            foreach (CalculationDiagnostic diagnostic in diagnostics)
                builder.AddDiagnostic(diagnostic);

            if (diagnostics.Any(item =>
                item.Severity ==
                CalculationDiagnosticSeverity.Error))
            {
                return builder.BuildWaitingForInput(null);
            }

            double weightedArea =
                input.Catchments.Sum(
                    item =>
                        item.EffectiveAreaSquareMetres *
                        item.RunoffCoefficient);

            double totalPhysicalArea =
                input.Catchments.Sum(
                    item =>
                        item.EffectiveAreaSquareMetres);

            double designFlow =
                weightedArea *
                input.RainfallIntensityLitresPerSecondSquareMetre;

            builder.AddInput(
                "RainfallIntensity",
                input.RainfallIntensityLitresPerSecondSquareMetre
                    .ToString("0.#####"));

            builder.AddInput(
                "WeightedCatchmentArea",
                weightedArea.ToString("0.###"));

            return builder.Build(
                new RoofDrainageResult(
                    totalPhysicalArea,
                    designFlow));
        }
    }
}
