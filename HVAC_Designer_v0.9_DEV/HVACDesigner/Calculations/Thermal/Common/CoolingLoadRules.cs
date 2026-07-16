using System;

namespace HVACDesigner.Calculations.Thermal.Common
{
    /// <summary>
    /// Nyári hőterhelés számítási szabályok.
    /// Forrás: rules-cooling-load.xml
    ///
    /// Árnyékolási tényezők, egyidejűségi tényezők, belső hőterhelés alapértékek.
    /// </summary>
    public sealed class CoolingLoadRules
    {
        // ─── Légfizikai állandó ─────────────────────────────────────────────

        /// <summary>Levegő volumetrikus hőkapacitása [Wh/(m³K)]</summary>
        public double AirVolumetricHeatCapacity { get; }

        // ─── Árnyékolási tényezők [-] ──────────────────────────────────────

        public double ShadingFactorNone { get; }                // 1.00
        public double ShadingFactorInternalLightBlind { get; }  // 0.75
        public double ShadingFactorInternalDarkBlind { get; }   // 0.65
        public double ShadingFactorExternalLightBlind { get; }  // 0.25
        public double ShadingFactorExternalDarkBlind { get; }   // 0.20
        public double ShadingFactorExternalOverhang { get; }    // 0.40

        // ─── Egyidejűségi tényezők [-] ────────────────────────────────────

        public double ConcurrencyFactorResidential { get; }     // 1.00
        public double ConcurrencyFactorOffice { get; }          // 0.85
        public double ConcurrencyFactorSchool { get; }          // 0.90
        public double ConcurrencyFactorRetail { get; }          // 0.70

        // ─── Belső hőterhelés alapértékek [W/person] ──────────────────────

        public double SensibleHeatPerPersonSedentary { get; }   // 75
        public double LatentHeatPerPersonSedentary { get; }     // 55
        public double SensibleHeatPerPersonLightWork { get; }   // 85
        public double LatentHeatPerPersonLightWork { get; }     // 75

        // ─── Megvilágítási teljesítmény [W/m²] ────────────────────────────

        public double LightingPowerDensityOffice { get; }       // 8
        public double LightingPowerDensityResidential { get; }  // 4
        public double LightingPowerDensitySchool { get; }       // 10
        public double LightingPowerDensityRetail { get; }       // 15

        // ─── Berendezési teljesítmény [W/m²] ──────────────────────────────

        public double EquipmentPowerDensityOffice { get; }      // 15
        public double EquipmentPowerDensityResidential { get; } // 4
        public double EquipmentPowerDensitySchool { get; }      // 6

        public CoolingLoadRules(
            double airVolumetricHeatCapacity = 0.34,
            double shadingFactorNone = 1.00,
            double shadingFactorInternalLightBlind = 0.75,
            double shadingFactorInternalDarkBlind = 0.65,
            double shadingFactorExternalLightBlind = 0.25,
            double shadingFactorExternalDarkBlind = 0.20,
            double shadingFactorExternalOverhang = 0.40,
            double concurrencyFactorResidential = 1.00,
            double concurrencyFactorOffice = 0.85,
            double concurrencyFactorSchool = 0.90,
            double concurrencyFactorRetail = 0.70,
            double sensibleHeatPerPersonSedentary = 75.0,
            double latentHeatPerPersonSedentary = 55.0,
            double sensibleHeatPerPersonLightWork = 85.0,
            double latentHeatPerPersonLightWork = 75.0,
            double lightingPowerDensityOffice = 8.0,
            double lightingPowerDensityResidential = 4.0,
            double lightingPowerDensitySchool = 10.0,
            double lightingPowerDensityRetail = 15.0,
            double equipmentPowerDensityOffice = 15.0,
            double equipmentPowerDensityResidential = 4.0,
            double equipmentPowerDensitySchool = 6.0)
        {
            AirVolumetricHeatCapacity = airVolumetricHeatCapacity;
            ShadingFactorNone = shadingFactorNone;
            ShadingFactorInternalLightBlind = shadingFactorInternalLightBlind;
            ShadingFactorInternalDarkBlind = shadingFactorInternalDarkBlind;
            ShadingFactorExternalLightBlind = shadingFactorExternalLightBlind;
            ShadingFactorExternalDarkBlind = shadingFactorExternalDarkBlind;
            ShadingFactorExternalOverhang = shadingFactorExternalOverhang;
            ConcurrencyFactorResidential = concurrencyFactorResidential;
            ConcurrencyFactorOffice = concurrencyFactorOffice;
            ConcurrencyFactorSchool = concurrencyFactorSchool;
            ConcurrencyFactorRetail = concurrencyFactorRetail;
            SensibleHeatPerPersonSedentary = sensibleHeatPerPersonSedentary;
            LatentHeatPerPersonSedentary = latentHeatPerPersonSedentary;
            SensibleHeatPerPersonLightWork = sensibleHeatPerPersonLightWork;
            LatentHeatPerPersonLightWork = latentHeatPerPersonLightWork;
            LightingPowerDensityOffice = lightingPowerDensityOffice;
            LightingPowerDensityResidential = lightingPowerDensityResidential;
            LightingPowerDensitySchool = lightingPowerDensitySchool;
            LightingPowerDensityRetail = lightingPowerDensityRetail;
            EquipmentPowerDensityOffice = equipmentPowerDensityOffice;
            EquipmentPowerDensityResidential = equipmentPowerDensityResidential;
            EquipmentPowerDensitySchool = equipmentPowerDensitySchool;
        }

        /// <summary>
        /// Visszaadja az árnyékolási tényezőt a megadott típushoz.
        /// </summary>
        public double GetShadingFactor(ShadingType shadingType)
        {
            switch (shadingType)
            {
                case ShadingType.None:
                    return ShadingFactorNone;
                case ShadingType.InternalLightBlind:
                    return ShadingFactorInternalLightBlind;
                case ShadingType.InternalDarkBlind:
                    return ShadingFactorInternalDarkBlind;
                case ShadingType.ExternalLightBlind:
                    return ShadingFactorExternalLightBlind;
                case ShadingType.ExternalDarkBlind:
                    return ShadingFactorExternalDarkBlind;
                case ShadingType.ExternalOverhang:
                    return ShadingFactorExternalOverhang;
                default:
                    return ShadingFactorNone;
            }
        }
    }

    /// <summary>
    /// Árnyékolás típusa nyílászáróknál.
    /// </summary>
    public enum ShadingType
    {
        None,
        InternalLightBlind,
        InternalDarkBlind,
        ExternalLightBlind,
        ExternalDarkBlind,
        ExternalOverhang
    }
}
