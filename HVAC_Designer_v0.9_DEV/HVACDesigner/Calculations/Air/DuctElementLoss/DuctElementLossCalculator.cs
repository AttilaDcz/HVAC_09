using System;
using HVACDesigner.Calculations.Common.FluidMechanics;

namespace HVACDesigner.Calculations.Air.DuctElementLoss
{
    /// <summary>
    /// Légtechnikai idomok és egyedi elemek SI-alapú
    /// nyomásveszteség-számítója.
    ///
    /// Nem ismeri az XML-t, a UI-t, a hálózatot vagy a katalógust.
    /// </summary>
    public sealed class DuctElementLossCalculator
    {
        private readonly FluidMechanicsCalculator _fluidMechanics;

        public DuctElementLossCalculator()
            : this(new FluidMechanicsCalculator())
        {
        }

        public DuctElementLossCalculator(
            FluidMechanicsCalculator fluidMechanics)
        {
            _fluidMechanics =
                fluidMechanics ??
                throw new ArgumentNullException(
                    nameof(fluidMechanics));
        }

        public DuctElementLossResult Calculate(
            DuctElementLossInput input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            ValidateCommonInput(input);

            double inletVelocity =
                CalculateVelocityOrZero(
                    input.MainVolumeFlowRate,
                    input.InletArea);

            double outletVelocity =
                CalculateVelocityOrZero(
                    input.MainVolumeFlowRate,
                    input.OutletArea);

            double branchVelocity =
                CalculateVelocityOrZero(
                    input.BranchVolumeFlowRate,
                    input.BranchArea);

            switch (input.LossModel)
            {
                case DuctElementLossModel.FixedPressure:
                    return CalculateFixedPressure(
                        input,
                        inletVelocity,
                        outletVelocity,
                        branchVelocity);

                case DuctElementLossModel.Zeta:
                    return CalculateSinglePathZeta(
                        input,
                        inletVelocity,
                        outletVelocity,
                        branchVelocity,
                        includeShankFriction: false);

                case DuctElementLossModel.ZetaWithShankFriction:
                    return CalculateSinglePathZeta(
                        input,
                        inletVelocity,
                        outletVelocity,
                        branchVelocity,
                        includeShankFriction: true);

                case DuctElementLossModel.Transition:
                    return CalculateTransition(
                        input,
                        inletVelocity,
                        outletVelocity,
                        branchVelocity);

                case DuctElementLossModel.Branch:
                    return CalculateBranch(
                        input,
                        inletVelocity,
                        outletVelocity,
                        branchVelocity);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(input.LossModel),
                        input.LossModel,
                        "Ismeretlen veszteségmodell.");
            }
        }

        private DuctElementLossResult CalculateFixedPressure(
            DuctElementLossInput input,
            double inletVelocity,
            double outletVelocity,
            double branchVelocity)
        {
            EnsureNonNegativeFinite(
                input.FixedPressureDrop,
                nameof(input.FixedPressureDrop));

            double referenceVelocity =
                SelectReferenceVelocity(
                    input,
                    inletVelocity,
                    outletVelocity,
                    branchVelocity);

            double dynamicPressure =
                _fluidMechanics.CalculateDynamicPressure(
                    referenceVelocity,
                    input.Fluid);

            return new DuctElementLossResult(
                inletVelocity,
                outletVelocity,
                branchVelocity,
                referenceVelocity,
                dynamicPressure,
                input.FixedPressureDrop,
                0.0,
                0.0,
                input.FixedPressureDrop,
                0.0,
                input.FixedPressureDrop,
                DetermineFlowRegime(
                    input.MainVolumeFlowRate,
                    input.MainHydraulicDiameter,
                    input.Fluid),
                FlowRegime.NoFlow,
                false,
                false);
        }

        private DuctElementLossResult CalculateSinglePathZeta(
            DuctElementLossInput input,
            double inletVelocity,
            double outletVelocity,
            double branchVelocity,
            bool includeShankFriction)
        {
            EnsureNonNegativeFinite(
                input.Zeta,
                nameof(input.Zeta));

            double referenceVelocity =
                SelectReferenceVelocity(
                    input,
                    inletVelocity,
                    outletVelocity,
                    branchVelocity);

            double dynamicPressure =
                _fluidMechanics.CalculateDynamicPressure(
                    referenceVelocity,
                    input.Fluid);

            double localLoss =
                input.Zeta *
                dynamicPressure;

            StraightSectionResult mainShank =
                CalculateOptionalShank(
                    input.MainVolumeFlowRate,
                    ResolveMainArea(input),
                    input.MainHydraulicDiameter,
                    input.MainShankLength,
                    input.AbsoluteRoughness,
                    input.Fluid,
                    input.FrictionModel,
                    includeShankFriction);

            double mainPathLoss =
                localLoss +
                mainShank.TotalPressureLoss;

            return new DuctElementLossResult(
                inletVelocity,
                outletVelocity,
                branchVelocity,
                referenceVelocity,
                dynamicPressure,
                localLoss,
                mainShank.TotalPressureLoss,
                0.0,
                mainPathLoss,
                0.0,
                mainPathLoss,
                mainShank.FlowRegime,
                FlowRegime.NoFlow,
                false,
                mainShank.IsTransitionEstimate);
        }

        private DuctElementLossResult CalculateTransition(
            DuctElementLossInput input,
            double inletVelocity,
            double outletVelocity,
            double branchVelocity)
        {
            EnsureNonNegativeFinite(
                input.Zeta,
                nameof(input.Zeta));

            EnsurePositiveFinite(
                input.InletArea,
                nameof(input.InletArea));

            EnsurePositiveFinite(
                input.OutletArea,
                nameof(input.OutletArea));

            double referenceVelocity =
                Math.Max(
                    inletVelocity,
                    outletVelocity);

            double dynamicPressure =
                _fluidMechanics.CalculateDynamicPressure(
                    referenceVelocity,
                    input.Fluid);

            double localLoss =
                input.Zeta *
                dynamicPressure;

            StraightSectionResult mainShank =
                CalculateOptionalShank(
                    input.MainVolumeFlowRate,
                    ResolveMainArea(input),
                    input.MainHydraulicDiameter,
                    input.MainShankLength,
                    input.AbsoluteRoughness,
                    input.Fluid,
                    input.FrictionModel,
                    includeShankFriction:
                        input.MainShankLength > 0.0);

            double mainPathLoss =
                localLoss +
                mainShank.TotalPressureLoss;

            return new DuctElementLossResult(
                inletVelocity,
                outletVelocity,
                branchVelocity,
                referenceVelocity,
                dynamicPressure,
                localLoss,
                mainShank.TotalPressureLoss,
                0.0,
                mainPathLoss,
                0.0,
                mainPathLoss,
                mainShank.FlowRegime,
                FlowRegime.NoFlow,
                false,
                mainShank.IsTransitionEstimate);
        }

        private DuctElementLossResult CalculateBranch(
            DuctElementLossInput input,
            double inletVelocity,
            double outletVelocity,
            double branchVelocity)
        {
            EnsureNonNegativeFinite(
                input.MainZeta,
                nameof(input.MainZeta));

            EnsureNonNegativeFinite(
                input.BranchZeta,
                nameof(input.BranchZeta));

            EnsurePositiveFinite(
                input.InletArea,
                nameof(input.InletArea));

            EnsurePositiveFinite(
                input.OutletArea,
                nameof(input.OutletArea));

            EnsurePositiveFinite(
                input.BranchArea,
                nameof(input.BranchArea));

            double mainDynamicPressure =
                _fluidMechanics.CalculateDynamicPressure(
                    outletVelocity,
                    input.Fluid);

            double branchDynamicPressure =
                _fluidMechanics.CalculateDynamicPressure(
                    branchVelocity,
                    input.Fluid);

            double mainLocalLoss =
                input.MainZeta *
                mainDynamicPressure;

            double branchLocalLoss =
                input.BranchZeta *
                branchDynamicPressure;

            StraightSectionResult mainShank =
                CalculateOptionalShank(
                    input.MainVolumeFlowRate,
                    input.OutletArea,
                    input.MainHydraulicDiameter,
                    input.MainShankLength,
                    input.AbsoluteRoughness,
                    input.Fluid,
                    input.FrictionModel,
                    includeShankFriction:
                        input.MainShankLength > 0.0);

            StraightSectionResult branchShank =
                CalculateOptionalShank(
                    input.BranchVolumeFlowRate,
                    input.BranchArea,
                    input.BranchHydraulicDiameter,
                    input.BranchShankLength,
                    input.AbsoluteRoughness,
                    input.Fluid,
                    input.FrictionModel,
                    includeShankFriction:
                        input.BranchShankLength > 0.0);

            double mainPathLoss =
                mainLocalLoss +
                mainShank.TotalPressureLoss;

            double branchPathLoss =
                branchLocalLoss +
                branchShank.TotalPressureLoss;

            double totalLoss =
                Math.Max(
                    mainPathLoss,
                    branchPathLoss);

            bool isTransitionEstimate =
                mainShank.IsTransitionEstimate ||
                branchShank.IsTransitionEstimate;

            return new DuctElementLossResult(
                inletVelocity,
                outletVelocity,
                branchVelocity,
                outletVelocity,
                mainDynamicPressure,
                mainLocalLoss,
                mainShank.TotalPressureLoss,
                branchShank.TotalPressureLoss,
                mainPathLoss,
                branchPathLoss,
                totalLoss,
                mainShank.FlowRegime,
                branchShank.FlowRegime,
                true,
                isTransitionEstimate);
        }

        private StraightSectionResult CalculateOptionalShank(
            double volumeFlowRate,
            double area,
            double hydraulicDiameter,
            double length,
            double absoluteRoughness,
            FluidProperties fluid,
            FrictionFactorModel frictionModel,
            bool includeShankFriction)
        {
            EnsureNonNegativeFinite(
                length,
                nameof(length));

            if (!includeShankFriction)
            {
                return CreateNoShankResult();
            }

            EnsurePositiveFinite(
                area,
                nameof(area));

            EnsurePositiveFinite(
                hydraulicDiameter,
                nameof(hydraulicDiameter));

            return _fluidMechanics.CalculateStraightSection(
                new StraightSectionInput
                {
                    VolumeFlowRate =
                        volumeFlowRate,
                    FlowArea =
                        area,
                    HydraulicDiameter =
                        hydraulicDiameter,
                    Length =
                        length,
                    AbsoluteRoughness =
                        absoluteRoughness,
                    Fluid =
                        fluid,
                    FrictionModel =
                        frictionModel
                });
        }

        private static StraightSectionResult CreateNoShankResult()
        {
            return new StraightSectionResult(
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                FlowRegime.NoFlow,
                FrictionFactorModel.AutomaticEngineering,
                0,
                true);
        }

        private double SelectReferenceVelocity(
            DuctElementLossInput input,
            double inletVelocity,
            double outletVelocity,
            double branchVelocity)
        {
            switch (input.ReferenceVelocityRule)
            {
                case ReferenceVelocityRule.Inlet:
                    EnsurePositiveFinite(
                        input.InletArea,
                        nameof(input.InletArea));
                    return inletVelocity;

                case ReferenceVelocityRule.Outlet:
                    EnsurePositiveFinite(
                        input.OutletArea,
                        nameof(input.OutletArea));
                    return outletVelocity;

                case ReferenceVelocityRule.HigherVelocity:
                    EnsurePositiveFinite(
                        input.InletArea,
                        nameof(input.InletArea));
                    EnsurePositiveFinite(
                        input.OutletArea,
                        nameof(input.OutletArea));
                    return Math.Max(
                        inletVelocity,
                        outletVelocity);

                case ReferenceVelocityRule.MainPath:
                    EnsurePositiveFinite(
                        input.OutletArea,
                        nameof(input.OutletArea));
                    return outletVelocity;

                case ReferenceVelocityRule.BranchPath:
                    EnsurePositiveFinite(
                        input.BranchArea,
                        nameof(input.BranchArea));
                    return branchVelocity;

                case ReferenceVelocityRule.FreeArea:
                    EnsurePositiveFinite(
                        input.FreeArea,
                        nameof(input.FreeArea));
                    return _fluidMechanics.CalculateVelocity(
                        input.MainVolumeFlowRate,
                        input.FreeArea);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(input.ReferenceVelocityRule),
                        input.ReferenceVelocityRule,
                        "Ismeretlen referencia-sebesség szabály.");
            }
        }

        private static double ResolveMainArea(
            DuctElementLossInput input)
        {
            if (input.OutletArea > 0.0)
                return input.OutletArea;

            if (input.InletArea > 0.0)
                return input.InletArea;

            throw new ArgumentOutOfRangeException(
                nameof(input.InletArea),
                "A főági szársúrlódáshoz legalább egy pozitív főági keresztmetszet szükséges.");
        }

        private double CalculateVelocityOrZero(
            double volumeFlowRate,
            double area)
        {
            EnsureNonNegativeFinite(
                volumeFlowRate,
                nameof(volumeFlowRate));

            if (volumeFlowRate == 0.0)
                return 0.0;

            EnsurePositiveFinite(
                area,
                nameof(area));

            return _fluidMechanics.CalculateVelocity(
                volumeFlowRate,
                area);
        }

        private FlowRegime DetermineFlowRegime(
            double volumeFlowRate,
            double hydraulicDiameter,
            FluidProperties fluid)
        {
            if (volumeFlowRate == 0.0)
                return FlowRegime.NoFlow;

            if (hydraulicDiameter <= 0.0)
                return FlowRegime.NoFlow;

            double area =
                Math.PI *
                hydraulicDiameter *
                hydraulicDiameter /
                4.0;

            double velocity =
                _fluidMechanics.CalculateVelocity(
                    volumeFlowRate,
                    area);

            double reynolds =
                _fluidMechanics.CalculateReynoldsNumber(
                    velocity,
                    hydraulicDiameter,
                    fluid);

            if (reynolds < 2300.0)
                return FlowRegime.Laminar;

            if (reynolds < 4000.0)
                return FlowRegime.Transitional;

            return FlowRegime.Turbulent;
        }

        private static void ValidateCommonInput(
            DuctElementLossInput input)
        {
            if (input.Fluid == null)
                throw new ArgumentNullException(nameof(input.Fluid));

            EnsureNonNegativeFinite(
                input.MainVolumeFlowRate,
                nameof(input.MainVolumeFlowRate));

            EnsureNonNegativeFinite(
                input.BranchVolumeFlowRate,
                nameof(input.BranchVolumeFlowRate));

            EnsureNonNegativeFinite(
                input.AbsoluteRoughness,
                nameof(input.AbsoluteRoughness));

            EnsureNonNegativeFinite(
                input.MainShankLength,
                nameof(input.MainShankLength));

            EnsureNonNegativeFinite(
                input.BranchShankLength,
                nameof(input.BranchShankLength));
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
