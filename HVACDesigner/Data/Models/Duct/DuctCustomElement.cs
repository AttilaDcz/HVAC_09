using System;
using System.Data;

namespace HVACDesigner.Data.Models.Duct
{
    /// <summary>
    /// A felhasználó által a hálózatba helyezett egyedi elem (pl. hangcsillapító, szűrő, VAV szabályzó) konkrét példánya.
    /// </summary>
    public class DuctCustomElement : DuctElement
    {
        /// <summary>
        /// Ha igaz, a számítási motor nem Zetát, hanem a kézzel megadott fix Pa értéket veszi alapul.
        /// </summary>
        public bool UseFixPressureDrop { get; set; }
        public double FixPressureDrop { get; set; }  // Kézzel megadott fix ellenállás (Pa)

        // Szűrők / Zsírfogók specifikus adatai (Pa)
        public double InitialPressureDrop { get; set; }
        public double FinalPressureDrop { get; set; }

        /// <summary>
        /// Ha igaz, a hálózat méretezési (legrosszabb, eltömődött) állapotában számol a szűrővel.
        /// </summary>
        public bool CalculateWithFinalDrop { get; set; }

        // VAV / CAV szabályzódoboz specifikus adat
        public double MinOperatingPressure { get; set; }

        public DuctCustomElement()
        {
            Category = "Egyedi elem";
            GeometryType = GeometryType.SingleSize;
        }

        /// <summary>
        /// Az egyedi elem saját, belső áramlástani nyomásesés számítása.
        /// </summary>
        public override double CalculatePressureDrop(double airDensity = 1.2)
        {
            double baseDrop = 0;

            // 1. Ha fix nyomásesés van beállítva
            if (UseFixPressureDrop)
            {
                baseDrop = FixPressureDrop;
            }
            // 2. Ha szűrőről vagy zsírfogóról van szó (név vagy altípus alapján ellenőrizve)
            else if (Subtype != null && (Subtype.Contains("Szűrő") || Subtype.Contains("Zsírfogó") || Subtype.Contains("Filter")))
            {
                baseDrop = CalculateWithFinalDrop ? FinalPressureDrop : InitialPressureDrop;
            }
            // 3. Alapértelmezett áramlási Zeta alapú számítás
            else
            {
                double v = GetVelocity();
                if (v <= 0) return 0;
                baseDrop = Zeta * (airDensity * Math.Pow(v, 2) / 2.0);
            }

            // Mérnöki plusz: Ha VAV/CAV doboz, a fojtási veszteségen felül kötelező hozzáadni a szabályzási minimum nyomást is!
            if (Subtype != null && (Subtype.Contains("VAV") || Subtype.Contains("CAV")))
            {
                baseDrop += MinOperatingPressure > 0 ? MinOperatingPressure : 50.0;
            }

            return baseDrop;
        }
    }
}
