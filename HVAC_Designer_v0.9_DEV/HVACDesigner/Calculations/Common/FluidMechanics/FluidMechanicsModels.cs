using System;

namespace HVACDesigner.Calculations.Common.FluidMechanics
{
    public enum FlowRegime
    {
        NoFlow,
        Laminar,
        Transitional,
        Turbulent
    }

    public enum FrictionFactorModel
    {
        /// <summary>
        /// Lamináris tartományban 64/Re; átmeneti tartományban
        /// dokumentált mérnöki átmenet; turbulens tartományban
        /// iteratív Colebrook-White.
        /// </summary>
        AutomaticEngineering,

        ColebrookWhite,
        SwameeJain,
        Haaland
    }

    public sealed class FrictionCalculationResult
    {
        public double FrictionFactor { get; }
        public FlowRegime FlowRegime { get; }
        public FrictionFactorModel AppliedModel { get; }
        public int IterationCount { get; }
        public bool Converged { get; }
        public bool IsTransitionEstimate =>
            FlowRegime == FlowRegime.Transitional;

        public FrictionCalculationResult(
            double frictionFactor,
            FlowRegime flowRegime,
            FrictionFactorModel appliedModel,
            int iterationCount,
            bool converged)
        {
            FrictionFactor = frictionFactor;
            FlowRegime = flowRegime;
            AppliedModel = appliedModel;
            IterationCount = iterationCount;
            Converged = converged;
        }
    }

    /// <summary>
    /// Egy egyenes szakasz bemenetei SI-egységekben.
    /// </summary>
    public sealed class StraightSectionInput
    {
        public double VolumeFlowRate { get; set; }      // [m3/s]
        public double FlowArea { get; set; }            // [m2]
        public double HydraulicDiameter { get; set; }   // [m]
        public double Length { get; set; }              // [m]
        public double AbsoluteRoughness { get; set; }   // [m]

        public FluidProperties Fluid { get; set; } =
            FluidProperties.AirAt20C;

        public FrictionFactorModel FrictionModel { get; set; } =
            FrictionFactorModel.AutomaticEngineering;
    }

    public sealed class StraightSectionResult
    {
        public double Velocity { get; }
        public double ReynoldsNumber { get; }
        public double RelativeRoughness { get; }
        public double FrictionFactor { get; }
        public double DynamicPressure { get; }
        public double PressureLossPerMeter { get; }
        public double TotalPressureLoss { get; }
        public FlowRegime FlowRegime { get; }
        public FrictionFactorModel AppliedFrictionModel { get; }
        public int FrictionIterationCount { get; }
        public bool FrictionCalculationConverged { get; }
        public bool IsTransitionEstimate =>
            FlowRegime == FlowRegime.Transitional;

        public StraightSectionResult(
            double velocity,
            double reynoldsNumber,
            double relativeRoughness,
            double frictionFactor,
            double dynamicPressure,
            double pressureLossPerMeter,
            double totalPressureLoss,
            FlowRegime flowRegime,
            FrictionFactorModel appliedFrictionModel,
            int frictionIterationCount,
            bool frictionCalculationConverged)
        {
            Velocity = velocity;
            ReynoldsNumber = reynoldsNumber;
            RelativeRoughness = relativeRoughness;
            FrictionFactor = frictionFactor;
            DynamicPressure = dynamicPressure;
            PressureLossPerMeter = pressureLossPerMeter;
            TotalPressureLoss = totalPressureLoss;
            FlowRegime = flowRegime;
            AppliedFrictionModel = appliedFrictionModel;
            FrictionIterationCount = frictionIterationCount;
            FrictionCalculationConverged =
                frictionCalculationConverged;
        }
    }
}
