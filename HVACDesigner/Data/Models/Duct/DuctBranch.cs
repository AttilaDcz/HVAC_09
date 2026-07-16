using System.Collections.Generic;

namespace HVACDesigner.Data.Models.Duct
{
    public class DuctBranch
    {
        public string Name { get; set; } = string.Empty;
        public FlowDirection BranchDirection { get; set; }
        public bool IsCriticalPath { get; set; }

        // Az ág saját elemei
        public List<DuctElement> Elements { get; } = new List<DuctElement>();

        // --- HIBÁKAT JAVÍTÓ ÚJ TULAJDONSÁGOK ÉS METÓDUSOK ---

        /// <summary>
        /// Az ág alaptervezési légmennyisége (m³/h).
        /// </summary>
        public double Airflow { get; set; }

        /// <summary>
        /// Visszafelé kompatibilitás a régi 'FlowM3h' elnevezéshez.
        /// </summary>
        public double FlowM3h
        {
            get => Airflow;
            set => Airflow = value;
        }

        /// <summary>
        /// Visszafelé kompatibilitás a régi, fixen beírt nyomásértékhez.
        /// </summary>
        public double PressureLossPa { get; set; }

        public void AddElement(DuctElement element)
        {
            if (element != null)
            {
                Elements.Add(element);
            }
        }

        /// <summary>
        /// Kiszámítja az ágban lévő összes elem összesített aerodinamikai nyomásesését (Pa).
        /// </summary>
        public double GetTotalPressureDrop(double airDensity = 1.2)
        {
            double totalLoss = 0;
            foreach (var element in Elements)
            {
                totalLoss += element.CalculatePressureDrop(airDensity);
            }
            PressureLossPa = totalLoss; // Frissítjük a kompatibilitási mezőt is
            return totalLoss;
        }
    }
}
