using System;

namespace HVACDesigner.Calculations.Common.FluidMechanics
{
    /// <summary>
    /// Homogén, egyfázisú közeg épületgépészeti áramlástani
    /// alapjainak stateless, SI-alapú belépési pontja.
    ///
    /// Nem ismeri a UI-t, a projektet, a katalógusokat,
    /// a kijelzési egységeket vagy az adattárolást.
    /// </summary>
    public sealed class FluidMechanicsCalculator
    {
        public double CalculateVelocity(
            double volumeFlowRate,
            double flowArea)
        {
            EnsureNonNegativeFinite(
                volumeFlowRate,
                nameof(volumeFlowRate));

            EnsurePositiveFinite(
                flowArea,
                nameof(flowArea));

            return volumeFlowRate / flowArea;
        }

        public double CalculateReynoldsNumber(
            double velocity,
            double hydraulicDiameter,
            FluidProperties fluid)
        {
            EnsureNonNegativeFinite(
                velocity,
                nameof(velocity));

            EnsurePositiveFinite(
                hydraulicDiameter,
                nameof(hydraulicDiameter));

            EnsureFluid(fluid);

            if (velocity == 0.0)
                return 0.0;

            return fluid.Density *
                   velocity *
                   hydraulicDiameter /
                   fluid.DynamicViscosity;
        }

        public double CalculateDynamicPressure(
            double velocity,
            FluidProperties fluid)
        {
            EnsureNonNegativeFinite(
                velocity,
                nameof(velocity));

            EnsureFluid(fluid);

            return fluid.Density *
                   velocity *
                   velocity /
                   2.0;
        }

        public FrictionCalculationResult
            CalculateFrictionFactor(
                double reynoldsNumber,
                double absoluteRoughness,
                double hydraulicDiameter,
                FrictionFactorModel model =
                    FrictionFactorModel.AutomaticEngineering)
        {
            EnsureNonNegativeFinite(
                reynoldsNumber,
                nameof(reynoldsNumber));

            EnsureNonNegativeFinite(
                absoluteRoughness,
                nameof(absoluteRoughness));

            EnsurePositiveFinite(
                hydraulicDiameter,
                nameof(hydraulicDiameter));

            double relativeRoughness =
                absoluteRoughness /
                hydraulicDiameter;

            return FrictionModels.Calculate(
                reynoldsNumber,
                relativeRoughness,
                model);
        }

        public double
            CalculateDarcyWeisbachPressureLossPerMeter(
                double frictionFactor,
                double hydraulicDiameter,
                double dynamicPressure)
        {
            EnsureNonNegativeFinite(
                frictionFactor,
                nameof(frictionFactor));

            EnsurePositiveFinite(
                hydraulicDiameter,
                nameof(hydraulicDiameter));

            EnsureNonNegativeFinite(
                dynamicPressure,
                nameof(dynamicPressure));

            return frictionFactor *
                   dynamicPressure /
                   hydraulicDiameter;
        }

        public double CalculateDarcyWeisbachPressureLoss(
            double pressureLossPerMeter,
            double length)
        {
            EnsureNonNegativeFinite(
                pressureLossPerMeter,
                nameof(pressureLossPerMeter));

            EnsureNonNegativeFinite(
                length,
                nameof(length));

            return pressureLossPerMeter *
                   length;
        }

        public StraightSectionResult CalculateStraightSection(
            StraightSectionInput input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            EnsureNonNegativeFinite(
                input.VolumeFlowRate,
                nameof(input.VolumeFlowRate));

            EnsurePositiveFinite(
                input.FlowArea,
                nameof(input.FlowArea));

            EnsurePositiveFinite(
                input.HydraulicDiameter,
                nameof(input.HydraulicDiameter));

            EnsureNonNegativeFinite(
                input.Length,
                nameof(input.Length));

            EnsureNonNegativeFinite(
                input.AbsoluteRoughness,
                nameof(input.AbsoluteRoughness));

            EnsureFluid(input.Fluid);

            double velocity =
                CalculateVelocity(
                    input.VolumeFlowRate,
                    input.FlowArea);

            double reynoldsNumber =
                CalculateReynoldsNumber(
                    velocity,
                    input.HydraulicDiameter,
                    input.Fluid);

            double relativeRoughness =
                input.AbsoluteRoughness /
                input.HydraulicDiameter;

            FrictionCalculationResult friction =
                CalculateFrictionFactor(
                    reynoldsNumber,
                    input.AbsoluteRoughness,
                    input.HydraulicDiameter,
                    input.FrictionModel);

            double dynamicPressure =
                CalculateDynamicPressure(
                    velocity,
                    input.Fluid);

            double pressureLossPerMeter =
                CalculateDarcyWeisbachPressureLossPerMeter(
                    friction.FrictionFactor,
                    input.HydraulicDiameter,
                    dynamicPressure);

            double totalPressureLoss =
                CalculateDarcyWeisbachPressureLoss(
                    pressureLossPerMeter,
                    input.Length);

            return new StraightSectionResult(
                velocity,
                reynoldsNumber,
                relativeRoughness,
                friction.FrictionFactor,
                dynamicPressure,
                pressureLossPerMeter,
                totalPressureLoss,
                friction.FlowRegime,
                friction.AppliedModel,
                friction.IterationCount,
                friction.Converged);
        }

        private static void EnsureFluid(
            FluidProperties fluid)
        {
            if (fluid == null)
                throw new ArgumentNullException(nameof(fluid));
        }

        private static void EnsurePositiveFinite(
            double value,
            string parameterName)
        {
            if (value <= 0.0 ||
                double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Az értéknek pozitív és véges számnak kell lennie.");
            }
        }

        private static void EnsureNonNegativeFinite(
            double value,
            string parameterName)
        {
            if (value < 0.0 ||
                double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Az érték nem lehet negatív vagy nem véges.");
            }
        }
    }
}
