using System.Collections.Generic;

namespace HVACDesigner.CoreUI.Services
{
    public static class ModuleDisplayNames
    {
        private static readonly Dictionary<string, string> Names =
            new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
            {
                [ModuleKeys.Dashboard] = "Kezdőlap",
                [ModuleKeys.BuildingUValue] = "U-érték számítás",
                [ModuleKeys.BuildingInventory] = "Épület felvétele",
                [ModuleKeys.BuildingHeatLoad] = "Hőszükséglet számítás",
                [ModuleKeys.BuildingCertification] = "Tanúsítás",
                [ModuleKeys.HeatingPipePressureDrop] = "Csőhálózati nyomásesés",
                [ModuleKeys.HeatingPumpSelection] = "Szivattyú választás",
                [ModuleKeys.WaterPeakDemand] = "Mértékadó vízigény",
                [ModuleKeys.WaterDailyDemand] = "Napi vízigény",
                [ModuleKeys.WaterDrainagePeak] = "Mértékadó csatorna",
                [ModuleKeys.WaterDhwCirculation] = "HMV cirkuláció",
                [ModuleKeys.WaterLongProfile] = "Hosszszelvény tervező",
                [ModuleKeys.AirVelocity] = "Légsebesség",
                [ModuleKeys.AirDuctSizing] = "Légcsatorna méretezés",
                [ModuleKeys.AirDuctNetwork] = "Légtechnikai hálózat számítás",
                [ModuleKeys.FlueGas] = "Füstgáz modul",
                [ModuleKeys.EngineeringDataSandbox] = "EngineeringData teszt"
            };

        public static string Get(string moduleKey)
        {
            if (string.IsNullOrWhiteSpace(moduleKey))
                return string.Empty;

            return Names.TryGetValue(moduleKey.Trim(), out string? displayName)
                ? displayName
                : moduleKey;
        }
    }
}
