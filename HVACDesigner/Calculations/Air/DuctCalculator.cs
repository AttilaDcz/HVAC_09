using System;
using HVACDesigner.Data.Models.Duct;

namespace HVACDesigner.Calculations.Air
{
    /// <summary>
    /// Légtechnikai számítások megújult, BME módszertanú objektumorientált motorja.
    /// </summary>
    public class DuctCalculator
    {
        // Globális, felhasználó által is módosítható fizikai paraméterek
        public double AirDensity { get; set; } = 1.204;     // kg/m3 (normál levegő)
        public double AirViscosity { get; set; } = 1.81e-5; // Pa*s (dinamikai viszkozitás)

        /// <summary>
        /// Reynolds-szám számítása a központi fizikai állandókkal.
        /// </summary>
        public double ComputeRe(double velocity, double dh)
        {
            if (dh <= 0 || velocity <= 0) return 0;
            return AirDensity * velocity * dh / AirViscosity;
        }

        /// <summary>
        /// Súrlódási tényező (Swamee–Jain közelítés) az elem abszolút érdességével.
        /// </summary>
        public double ComputeLambda(double velocity, double dh, double elementRoughnessMm)
        {
            if (dh <= 0 || velocity <= 0) return 0;

            double re = ComputeRe(velocity, dh);
            if (re < 2300) return 64.0 / (re > 0 ? re : 1); // Lamináris szakasz fallback

            double roughnessMeter = elementRoughnessMm / 1000.0; // mm -> m átváltás

            return 0.25 / Math.Pow(
                Math.Log10((roughnessMeter / (3.7 * dh)) + (5.74 / Math.Pow(re, 0.9))),
                2
            );
        }

        /// <summary>
        /// Csőszakasz nyomásvesztesége polimorf módon, delegálva a fizikai motornak.
        /// </summary>
        public double ComputeSegmentLoss(DuctSegment seg, double flowM3h)
        {
            seg.Airflow = flowM3h;
            double velocity = seg.GetVelocity();
            double dh = seg.GetHydraulicDiameter();
            double roughness = seg.MaterialOverride?.Roughness ?? 0.15; // XML-ből vagy alapértelmezett Spiro

            if (seg.Length <= 0 || velocity <= 0 || dh <= 0) return 0;

            double lambda = ComputeLambda(velocity, dh, roughness);
            double dynamicPressure = AirDensity * velocity * velocity / 2.0;

            seg.Velocity = velocity;
            seg.PressureDrop = lambda * (seg.Length / dh) * dynamicPressure;
            return seg.PressureDrop;
        }

        /// Könyökök és idomok nyomásvesztesége a gépészetileg helyes vonatkoztatott sebességgel.
        
        public double ComputeFittingLoss(DuctFitting fit, double flowM3h)
        {
            if (fit.PressureLossType == PressureLossType.FixedPressure)
            {
                fit.PressureDrop = fit.FixedPressureDrop;
                return fit.PressureDrop;
            }

            fit.Airflow = flowM3h;
            // Gépészeti korrekció: Idomoknál a belépő keresztmetszet sebessége a mérvadó, nem az átlag!
            double velocity = fit.GetVelocity();
            if (velocity <= 0) return 0;

            double dynamicPressure = AirDensity * velocity * velocity / 2.0;
            double loss = fit.Zeta * dynamicPressure;

            // Szárhosszak súrlódásának hozzáadása
            if (fit.ShankLength1 > 0 || fit.ShankLength2 > 0)
            {
                double length = (fit.ShankLength1 + fit.ShankLength2) / 1000.0;
                double dh = fit.GetHydraulicDiameter();
                double roughness = fit.MaterialOverride?.Roughness ?? 0.15;
                double lambda = ComputeLambda(velocity, dh, roughness);
                loss += lambda * (length / dh) * dynamicPressure;
            }

            fit.Velocity = velocity;
            fit.PressureDrop = loss;
            return fit.PressureDrop;
        }

        /// Átmeneti idomok (szűkítők/bővítők) nyomásvesztesége a nagyobb sebesség (BME jegyzet) alapján.
        
        public double ComputeTransitionLoss(DuctTransition tr, double flowM3h)
        {
            if (tr.PressureLossType == PressureLossType.FixedPressure)
            {
                tr.PressureDrop = tr.FixedPressureDrop;
                return tr.PressureDrop;
            }

            tr.Airflow = flowM3h;
            double vIn = tr.GetVelocity();
            double vOut = tr.Geometry != null && tr.Geometry.GetEffectiveOutletArea() > 0
                ? (flowM3h / 3600.0) / tr.Geometry.GetEffectiveOutletArea() : vIn;

            // BME áramlástani szabály: Szűkítésnél/bővülésnél a Zeta a nagyobb sebességre (kisebb keresztmetszetre) vonatkozik!
            double calcVelocity = Math.Max(vIn, vOut);
            if (calcVelocity <= 0) return 0;

            double dynamicPressure = AirDensity * calcVelocity * calcVelocity / 2.0;

            tr.Velocity = calcVelocity;
            tr.PressureDrop = tr.Zeta * dynamicPressure;
            return tr.PressureDrop;
        }
                
        /// Elágazások (T-idomok, Nadrágidomok) nyomásveszteségének különválasztott számítása mindkét áramlási irányra.
        
        public (double mainToBranch, double mainToStraight) ComputeBranchLoss(BranchFitting br, double flowMainM3h)
        {
            if (br.PressureLossType == PressureLossType.FixedPressure)
            {
                br.PressureDrop = br.FixedPressureDrop;
                br.BranchPressureDrop = br.FixedPressureDrop;
                return (br.BranchPressureDrop, br.PressureDrop);
            }

            br.Airflow = flowMainM3h;

            // 1. FŐÁG -> EGYENES TOVÁBB MENŐ IRÁNY
            double vMain = br.GetVelocity();
            double dpStraight = 0;
            if (vMain > 0)
            {
                double dynMain = AirDensity * vMain * vMain / 2.0;
                dpStraight = br.ZetaMain * dynMain; // Szigorúan a főági egyenes Zetával
            }

            // 2. FŐÁG -> LEÁGAZÓ MELLÉKÁG IRÁNY
            double areaBranch = br.Geometry?.GetEffectiveBranchArea() ?? 0;
            double dpBranch = 0;
            if (areaBranch > 0 && br.BranchAirflow > 0)
            {
                double vBranch = (br.BranchAirflow / 3600.0) / areaBranch;
                double dynBranch = AirDensity * vBranch * vBranch / 2.0;
                dpBranch = br.BranchZeta * dynBranch; // Szigorúan a leágazás egyedi Zetájával
            }

            br.Velocity = vMain;
            br.PressureDrop = dpStraight;
            br.BranchPressureDrop = dpBranch;

            return (dpBranch, dpStraight);
        }
    }
}