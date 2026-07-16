using System;
using System.Collections.Generic;
using HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates;

namespace HVACDesigner.Calculations.Thermal.Common
{
    /// <summary>
    /// ISO 6946:2017 Table 1 szerinti felületi hőellenállások és
    /// hőhíd alapparaméterek.
    /// Forrás: rules-building-physics.xml
    /// </summary>
    public sealed class BuildingPhysicsRules
    {
        // ─── Felületi hőellenállások [m²K/W] ───────────────────────────────

        /// <summary>Belső felületi hőellenállás – vízszintes hőáram (fal, ajtó)</summary>
        public double RsiHorizontal { get; }       // 0.13

        /// <summary>Külső felületi hőellenállás – vízszintes hőáram</summary>
        public double RseHorizontal { get; }       // 0.04

        /// <summary>Belső felületi hőellenállás – felfelé irányuló hőáram (lapos tető)</summary>
        public double RsiUpward { get; }           // 0.10

        /// <summary>Külső felületi hőellenállás – felfelé irányuló hőáram</summary>
        public double RseUpward { get; }           // 0.04

        /// <summary>Belső felületi hőellenállás – lefelé irányuló hőáram (padló)</summary>
        public double RsiDownward { get; }         // 0.17

        /// <summary>Külső felületi hőellenállás – lefelé irányuló hőáram</summary>
        public double RseDownward { get; }         // 0.04

        /// <summary>Hőhíd alapértelmezett pótlék (ÉKM 9/2023 szerint)</summary>
        public double DefaultThermalBridgeAllowance { get; }  // 0.10

        public BuildingPhysicsRules(
            double rsiHorizontal = 0.13,
            double rseHorizontal = 0.04,
            double rsiUpward = 0.10,
            double rseUpward = 0.04,
            double rsiDownward = 0.17,
            double rseDownward = 0.04,
            double defaultThermalBridgeAllowance = 0.10)
        {
            RsiHorizontal = EnsurePositive(rsiHorizontal, nameof(rsiHorizontal));
            RseHorizontal = EnsurePositive(rseHorizontal, nameof(rseHorizontal));
            RsiUpward = EnsurePositive(rsiUpward, nameof(rsiUpward));
            RseUpward = EnsurePositive(rseUpward, nameof(rseUpward));
            RsiDownward = EnsurePositive(rsiDownward, nameof(rsiDownward));
            RseDownward = EnsurePositive(rseDownward, nameof(rseDownward));
            DefaultThermalBridgeAllowance =
                EnsurePositive(defaultThermalBridgeAllowance,
                               nameof(defaultThermalBridgeAllowance));
        }

        /// <summary>
        /// Visszaadja a szerkezet hőáram-irányát a ConstructionType alapján.
        /// </summary>
        public static HeatFlowDirection GetHeatFlowDirection(ConstructionType constructionType)
        {
            switch (constructionType)
            {
                case ConstructionType.Roof:
                case ConstructionType.Ceiling:
                    return HeatFlowDirection.Upward;

                case ConstructionType.Floor:
                case ConstructionType.GroundFloor:
                case ConstructionType.Slab:
                    return HeatFlowDirection.Downward;

                default:
                    return HeatFlowDirection.Horizontal;
            }
        }

        /// <summary>
        /// Visszaadja a bemeneti és külső felületi hőellenállásokat
        /// egy adott hőáram-irányhoz.
        /// </summary>
        public (double Rsi, double Rse) GetSurfaceResistances(
            HeatFlowDirection heatFlowDirection)
        {
            switch (heatFlowDirection)
            {
                case HeatFlowDirection.Upward:
                    return (RsiUpward, RseUpward);
                case HeatFlowDirection.Downward:
                    return (RsiDownward, RseDownward);
                default:
                    return (RsiHorizontal, RseHorizontal);
            }
        }

        /// <summary>
        /// Visszaadja a bemeneti és külső felületi hőellenállásokat
        /// szerkezet típusa alapján.
        /// </summary>
        public (double Rsi, double Rse) GetSurfaceResistances(
            ConstructionType constructionType)
        {
            return GetSurfaceResistances(
                GetHeatFlowDirection(constructionType));
        }

        private static double EnsurePositive(double value, string paramName)
        {
            if (value <= 0.0 || double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentOutOfRangeException(
                    paramName,
                    "Az értéknek pozitív és véges számnak kell lennie.");
            return value;
        }
    }

    /// <summary>
    /// Hőáram iránya egy szerkezetben (ISO 6946 szerinti Rsi/Rse meghatározáshoz).
    /// </summary>
    public enum HeatFlowDirection
    {
        Horizontal,
        Upward,
        Downward
    }
}
