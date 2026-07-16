using System;
using HVACDesigner.Calculations.Common.FluidMechanics;

namespace HVACDesigner.Calculations.Air.DuctElementLoss
{
    public enum DuctElementLossModel
    {
        Zeta,
        FixedPressure,
        Transition,
        Branch,
        ZetaWithShankFriction
    }

    public enum ReferenceVelocityRule
    {
        Inlet,
        Outlet,
        HigherVelocity,
        MainPath,
        BranchPath,
        FreeArea
    }

    /// <summary>
    /// Egy légtechnikai elem nyomásveszteség-számításának
    /// SI-alapú bemeneti modellje.
    /// </summary>
    public sealed class DuctElementLossInput
    {
        public DuctElementLossModel LossModel { get; set; }
        public ReferenceVelocityRule ReferenceVelocityRule { get; set; } =
            ReferenceVelocityRule.Inlet;

        public double MainVolumeFlowRate { get; set; }      // [m3/s]
        public double BranchVolumeFlowRate { get; set; }    // [m3/s]

        public double InletArea { get; set; }               // [m2]
        public double OutletArea { get; set; }              // [m2]
        public double BranchArea { get; set; }              // [m2]
        public double FreeArea { get; set; }                // [m2]

        public double MainHydraulicDiameter { get; set; }   // [m]
        public double BranchHydraulicDiameter { get; set; } // [m]

        public double Zeta { get; set; }
        public double MainZeta { get; set; }
        public double BranchZeta { get; set; }

        public double FixedPressureDrop { get; set; }       // [Pa]

        public double MainShankLength { get; set; }         // [m]
        public double BranchShankLength { get; set; }       // [m]

        public double AbsoluteRoughness { get; set; }       // [m]

        public FluidProperties Fluid { get; set; } =
            FluidProperties.AirAt20C;

        public FrictionFactorModel FrictionModel { get; set; } =
            FrictionFactorModel.AutomaticEngineering;
    }

    public sealed class DuctElementLossResult
    {
        public double InletVelocity { get; }
        public double OutletVelocity { get; }
        public double BranchVelocity { get; }
        public double ReferenceVelocity { get; }
        public double DynamicPressure { get; }

        public double LocalLoss { get; }
        public double MainShankFrictionLoss { get; }
        public double BranchShankFrictionLoss { get; }

        public double MainPathLoss { get; }
        public double BranchPathLoss { get; }
        public double TotalLoss { get; }

        public FlowRegime MainFlowRegime { get; }
        public FlowRegime BranchFlowRegime { get; }

        public bool HasBranchResult { get; }
        public bool IsTransitionEstimate { get; }

        public DuctElementLossResult(
            double inletVelocity,
            double outletVelocity,
            double branchVelocity,
            double referenceVelocity,
            double dynamicPressure,
            double localLoss,
            double mainShankFrictionLoss,
            double branchShankFrictionLoss,
            double mainPathLoss,
            double branchPathLoss,
            double totalLoss,
            FlowRegime mainFlowRegime,
            FlowRegime branchFlowRegime,
            bool hasBranchResult,
            bool isTransitionEstimate)
        {
            InletVelocity = inletVelocity;
            OutletVelocity = outletVelocity;
            BranchVelocity = branchVelocity;
            ReferenceVelocity = referenceVelocity;
            DynamicPressure = dynamicPressure;
            LocalLoss = localLoss;
            MainShankFrictionLoss = mainShankFrictionLoss;
            BranchShankFrictionLoss = branchShankFrictionLoss;
            MainPathLoss = mainPathLoss;
            BranchPathLoss = branchPathLoss;
            TotalLoss = totalLoss;
            MainFlowRegime = mainFlowRegime;
            BranchFlowRegime = branchFlowRegime;
            HasBranchResult = hasBranchResult;
            IsTransitionEstimate = isTransitionEstimate;
        }
    }
}
