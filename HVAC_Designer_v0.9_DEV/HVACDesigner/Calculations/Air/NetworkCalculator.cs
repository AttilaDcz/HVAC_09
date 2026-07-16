using System;
using System.Collections.Generic;
using HVACDesigner.Data.Models.Duct;

namespace HVACDesigner.Calculations.Air
{
    /// <summary>
    /// Teljes légtechnikai hálózat számítása.
    /// Összegzi a csőszakaszok, idomok, átmenetek és egyedi elemek nyomásveszteségét.
    /// </summary>
    public class NetworkCalculator
    {
        private readonly DuctCalculator _calc;

        public NetworkCalculator(DuctCalculator calculator)
        {
            _calc = calculator;
        }

        /// <summary>
        /// Egy hálózati ág (sorba kötött elemek) teljes nyomásvesztesége a tiszta OO elv alapján.
        /// </summary>
        public double ComputeBranchLoss(IEnumerable<DuctElement> elements, double flowM3h)
        {
            double total = 0;

            foreach (var elem in elements)
            {
                // Biztosítjuk, hogy az elem a megfelelő áramlási értéket ismerje
                elem.Airflow = flowM3h;

                // Az elem polimorf módon, saját magán belül dönt a geometria, 
                // a veszteségtípus és az egyedi anyagérdesség alapján
                total += elem.CalculatePressureDrop(_calc.AirDensity);
            }

            return total;
        }

        /// <summary>
        /// Több ág (pl. mellékágak) összesített nyomásvesztesége.
        /// A legnagyobb veszteségű ág határozza meg a ventilátor szükséges nyomását.
        /// </summary>
        public double ComputeNetworkLoss(IEnumerable<IEnumerable<DuctElement>> branches, double flowM3h)
        {
            double maxLoss = 0;

            foreach (var branch in branches)
            {
                double loss = ComputeBranchLoss(branch, flowM3h);
                if (loss > maxLoss)
                {
                    maxLoss = loss;
                }
            }

            return maxLoss;
        }
    }
}
