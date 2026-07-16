namespace HVACDesigner.Data.Models.Material
{
    public class Material
    {
        // Alap tulajdonságok (homogén anyagok)
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Lambda { get; set; }        // Hővezetési tényező [W/mK]
        public double Density { get; set; }       // Sűrűség [kg/m³]
        public double SpecificHeat { get; set; }  // Fajhő [kJ/kgK]
        public double Mu { get; set; }            // Páradiffúziós ellenállási szám [-]

        // Légréteg (AirLayer) specifikus érték
        public bool IsAirLayer { get; set; }
        public double ThermalResistanceOverride { get; set; } // Fix R érték szabványos légréteghez

        // Fix ablak/ajtó default értékek
        public bool IsWindowDefault { get; set; }
        public double UwFix { get; set; }          // Kész ablak eredő hőátbocsátása

        // Ablak komponens értékek (Egyedi számításhoz)
        public bool IsComponent { get; set; }
        public double Ug { get; set; }             // Üveg U-érték
        public double Uf { get; set; }             // Tok U-érték
        public double Psi { get; set; }            // Vonalmenti hőhíd tényező
        public double GValue { get; set; }         // Napenergia áteresztés (Solar factor)

        // --- GÉPÉSZETI KALKULÁCIÓK (Vastagság 'd' [m] esetén) ---

        public double CalculateR(double thickness)
        {
            if (IsAirLayer) return ThermalResistanceOverride;
            return Lambda > 0 ? thickness / Lambda : 0;
        }

        public double CalculateMass(double thickness) => IsAirLayer ? 0 : thickness * Density;

        public double CalculateHeatCapacity(double thickness) => IsAirLayer ? 0 : thickness * Density * SpecificHeat;

        public double CalculateSd(double thickness) => IsAirLayer ? thickness * 1 : thickness * Mu;
    }
}