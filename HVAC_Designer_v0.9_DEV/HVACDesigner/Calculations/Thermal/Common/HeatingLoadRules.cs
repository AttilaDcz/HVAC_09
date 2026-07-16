using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates;

namespace HVACDesigner.Calculations.Thermal.Common
{
    /// <summary>
    /// EN 12831-1:2017 módszerparaméterek és hőhíd %-os ajánlások.
    /// Forrás: rules-heating-load.xml
    /// </summary>
    public sealed class HeatingLoadRules
    {
        // ─── Légfizikai állandó ─────────────────────────────────────────────

        /// <summary>Levegő volumetrikus hőkapacitása [Wh/(m³K)], ρ·cp ≈ 0.34</summary>
        public double AirVolumetricHeatCapacity { get; }

        // ─── Infiltráció alapértékek [h⁻¹] ────────────────────────────────

        public double DefaultInfiltrationAchNew { get; }       // 0.05
        public double DefaultInfiltrationAchRecent { get; }    // 0.10
        public double DefaultInfiltrationAchLegacy { get; }    // 0.15

        // ─── Felfűtési pótlék tényezők [-] ────────────────────────────────

        public double ReheatFactorResidential8h { get; }       // 1.10
        public double ReheatFactorOffice12h { get; }           // 1.15
        public double ReheatFactorSchoolWeekend { get; }       // 1.20

        // ─── Hőhíd ajánlások szerkezettípusonként ─────────────────────────

        private readonly ReadOnlyCollection<ThermalBridgeAllowanceRule> _allowances;

        public IReadOnlyList<ThermalBridgeAllowanceRule> ThermalBridgeAllowances
            => _allowances;

        public HeatingLoadRules(
            double airVolumetricHeatCapacity = 0.34,
            double defaultInfiltrationAchNew = 0.05,
            double defaultInfiltrationAchRecent = 0.10,
            double defaultInfiltrationAchLegacy = 0.15,
            double reheatFactorResidential8h = 1.10,
            double reheatFactorOffice12h = 1.15,
            double reheatFactorSchoolWeekend = 1.20,
            IEnumerable<ThermalBridgeAllowanceRule> allowances = null)
        {
            AirVolumetricHeatCapacity =
                EnsurePositive(airVolumetricHeatCapacity,
                               nameof(airVolumetricHeatCapacity));

            DefaultInfiltrationAchNew =
                EnsurePositive(defaultInfiltrationAchNew,
                               nameof(defaultInfiltrationAchNew));
            DefaultInfiltrationAchRecent =
                EnsurePositive(defaultInfiltrationAchRecent,
                               nameof(defaultInfiltrationAchRecent));
            DefaultInfiltrationAchLegacy =
                EnsurePositive(defaultInfiltrationAchLegacy,
                               nameof(defaultInfiltrationAchLegacy));

            ReheatFactorResidential8h =
                EnsurePositive(reheatFactorResidential8h,
                               nameof(reheatFactorResidential8h));
            ReheatFactorOffice12h =
                EnsurePositive(reheatFactorOffice12h,
                               nameof(reheatFactorOffice12h));
            ReheatFactorSchoolWeekend =
                EnsurePositive(reheatFactorSchoolWeekend,
                               nameof(reheatFactorSchoolWeekend));

            _allowances = new ReadOnlyCollection<ThermalBridgeAllowanceRule>(
                new List<ThermalBridgeAllowanceRule>(
                    allowances ?? GetDefaultAllowances()));
        }

        /// <summary>
        /// Megkeresi az adott szerkezettípushoz az ajánlott hőhíd-pótlék szabályt.
        /// Ha nem találja, visszaadja az ÉKM alapértéket (10%).
        /// </summary>
        public ThermalBridgeAllowanceRule FindAllowance(
            ConstructionType constructionType,
            double constructionUValue)
        {
            ThermalBridgeAllowanceRule bestMatch = null;
            bool isPoorly = false;

            foreach (var rule in _allowances)
            {
                if (rule.ConstructionType != constructionType)
                    continue;

                bool currentPoorly =
                    constructionUValue > rule.WarningThresholdU;

                if (bestMatch == null)
                {
                    bestMatch = rule;
                    isPoorly = currentPoorly;
                }
                else if (currentPoorly && !isPoorly)
                {
                    // Gyengén szigetelt esetre váltunk, ha illik
                    bestMatch = rule;
                    isPoorly = true;
                }
            }

            return bestMatch ?? ThermalBridgeAllowanceRule.CreateDefault(
                constructionType);
        }

        private static IEnumerable<ThermalBridgeAllowanceRule> GetDefaultAllowances()
        {
            // ISO 6946, EN 12831, ÉKM 9/2023 alapján beégetett alapértékek
            // (ugyanolyan értékek mint az XML-ben, fallback esetére)
            return new[]
            {
                new ThermalBridgeAllowanceRule(
                    ConstructionType.ExternalWall, "WellInsulated",
                    0.10, 0.10, 0.25,
                    "Jól szigetelt ETICS homlokzaton tipikus 10%."),
                new ThermalBridgeAllowanceRule(
                    ConstructionType.ExternalWall, "PoorlyInsulated",
                    0.25, 0.10, 0.50,
                    "Szigeteletlen/gyengén szigetelt falnál 20-30% is lehet."),
                new ThermalBridgeAllowanceRule(
                    ConstructionType.Ceiling, "WellInsulated",
                    0.10, 0.10, 0.20,
                    "Jól szigetelt padlásfödémnél tipikusan 10%."),
                new ThermalBridgeAllowanceRule(
                    ConstructionType.Roof, "WellInsulated",
                    0.10, 0.10, 0.20,
                    "Lapos tető, megfelelő párkányszigetelés esetén 10%."),
                new ThermalBridgeAllowanceRule(
                    ConstructionType.GroundFloor, "WellInsulated",
                    0.10, 0.10, 0.35,
                    "Talajon fekvő padló; kerületi szigetelés nélkül b_u tényező kezeli."),
                new ThermalBridgeAllowanceRule(
                    ConstructionType.BasementWall, "WellInsulated",
                    0.15, 0.10, 0.30,
                    "Pincefalnál lábazati csatlakozás magasabb Ψ-értéket ad."),
                new ThermalBridgeAllowanceRule(
                    ConstructionType.InternalWall, "WellInsulated",
                    0.05, 0.10, 0.40,
                    "Belső falaknál kis mértékű csatlakozási hőhíd.")
            };
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
    /// Egy szerkezettípusra vonatkozó hőhíd %-os ajánlás.
    /// </summary>
    public sealed class ThermalBridgeAllowanceRule
    {
        public ConstructionType ConstructionType { get; }
        public string InsulationLevel { get; }

        /// <summary>Ajánlott pótlék (mérnökileg megalapozott becslés) [-]</summary>
        public double RecommendedAllowance { get; }

        /// <summary>ÉKM alapértelmezett pótlék [-]</summary>
        public double DefaultAllowance { get; }

        /// <summary>
        /// Ha a szerkezet U-értéke meghaladja ezt, a kalkulátor
        /// figyelmeztetést és ajánlást ad [W/(m²K)]
        /// </summary>
        public double WarningThresholdU { get; }

        /// <summary>Megjegyzés / forrás</summary>
        public string Note { get; }

        public ThermalBridgeAllowanceRule(
            ConstructionType constructionType,
            string insulationLevel,
            double recommendedAllowance,
            double defaultAllowance,
            double warningThresholdU,
            string note = "")
        {
            ConstructionType = constructionType;
            InsulationLevel = insulationLevel?.Trim() ?? string.Empty;
            RecommendedAllowance = recommendedAllowance;
            DefaultAllowance = defaultAllowance;
            WarningThresholdU = warningThresholdU;
            Note = note?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Létrehoz egy általános alapértéket egy szerkezettípushoz
        /// (ha nincs specifikus szabály definiálva).
        /// </summary>
        public static ThermalBridgeAllowanceRule CreateDefault(
            ConstructionType constructionType)
        {
            return new ThermalBridgeAllowanceRule(
                constructionType,
                "Default",
                0.10,
                0.10,
                double.MaxValue,
                "ÉKM 9/2023 alapértelmezett 10% pótlék.");
        }
    }
}
