using System;
using System.Collections.Generic;

namespace HVACDesigner.Services
{
    [Serializable]
    public class ProjectData
    {
        public const string CurrentSchemaVersion = "1.0";

        // =========================================================================
        // 0. PROJEKTFÁJL / VERZIÓ METAADATOK
        // =========================================================================
        public string ProjectSchemaVersion { get; set; } = CurrentSchemaVersion;
        public string ApplicationVersionAtSave { get; set; } = "";
        public string EngineeringDataVersion { get; set; } = "";
        public string CountryCode { get; set; } = "HU";
        public string BuildingFunctionSource { get; set; } = "Catalog";
        public string BuildingFunctionId { get; set; } = "";
        public string BuildingFunctionDisplayName { get; set; } = "";
        public string CustomBuildingFunctionName { get; set; } = "";
        public string BuildingProfileId { get; set; } = "";
        public string DesignMethodId { get; set; } = "";
        public string DesignPhase { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAtUtc { get; set; } = DateTime.UtcNow;

        // =========================================================================
        // 1. INGATLAN / PROJEKT HELYSZÍN ADATOK
        // =========================================================================
        public string Name { get; set; } = "Új Projekt";
        public string TopographicalNumber { get; set; } = "";

        public string ProjZipCode { get; set; } = "";
        public string ProjSettlementName { get; set; } = "";
        public string ProjStreetName { get; set; } = "";
        public string ProjStreetType { get; set; } = "utca";
        public string ProjHouseNumber { get; set; } = "";
        public string ProjBuilding { get; set; } = "";
        public string ProjFloor { get; set; } = "";
        public string ProjDoorNumber { get; set; } = "";

        // =========================================================================
        // 2. TERVEZŐI CSAPAT ADATAI
        // =========================================================================
        public string DesignerName { get; set; } = "";
        public string EligibilityNumber { get; set; } = "";
        public string CoDesignerName { get; set; } = "";
        public string CoEligibilityNumber { get; set; } = "";
        public string DesignerCompany { get; set; } = "";
        public string DesignerAddress { get; set; } = "";
        public string DesignerPhone { get; set; } = "";
        public string DesignerEmail { get; set; } = "";

        // =========================================================================
        // 3. MEGBÍZÓ / ÜGYFÉL ADATAI
        // =========================================================================
        public bool ClientIsCompany { get; set; }
        public string ClientName { get; set; } = "";
        public string ClientAddress { get; set; } = "";
        public string ClientTaxNumber { get; set; } = "";
        public string ClientContactPerson { get; set; } = "";
        public string ClientPhone { get; set; } = "";
        public string ClientEmail { get; set; } = "";

        // =========================================================================
        // 4. MODULÁRIS SZAKÁGI BEÁLLÍTÁSOK
        // =========================================================================
        public AirModuleSettings AirSettings { get; set; } = new AirModuleSettings();
        public Dictionary<string, ModuleProjectData> Modules { get; set; } =
            new Dictionary<string, ModuleProjectData>(StringComparer.OrdinalIgnoreCase);

        public static ProjectData CreateNew(string name)
        {
            return new ProjectData
            {
                Name = string.IsNullOrWhiteSpace(name) ? "Új Projekt" : name.Trim()
            };
        }

        public void PrepareForSave()
        {
            if (string.IsNullOrWhiteSpace(ProjectSchemaVersion))
                ProjectSchemaVersion = CurrentSchemaVersion;

            if (string.IsNullOrWhiteSpace(CountryCode))
                CountryCode = "HU";

            if (string.IsNullOrWhiteSpace(BuildingFunctionSource))
                BuildingFunctionSource = string.IsNullOrWhiteSpace(CustomBuildingFunctionName)
                    ? "Catalog"
                    : "Custom";

            ApplicationVersionAtSave =
                typeof(ProjectData).Assembly.GetName().Version?.ToString() ?? "";

            ModifiedAtUtc = DateTime.UtcNow;
            Modules ??= new Dictionary<string, ModuleProjectData>(StringComparer.OrdinalIgnoreCase);
            AirSettings ??= new AirModuleSettings();
        }

        public void NormalizeAfterLoad()
        {
            if (string.IsNullOrWhiteSpace(ProjectSchemaVersion))
                ProjectSchemaVersion = "legacy";

            if (string.IsNullOrWhiteSpace(CountryCode))
                CountryCode = "HU";

            if (string.IsNullOrWhiteSpace(BuildingFunctionSource))
                BuildingFunctionSource = string.IsNullOrWhiteSpace(CustomBuildingFunctionName)
                    ? "Catalog"
                    : "Custom";

            if (CreatedAtUtc == default)
                CreatedAtUtc = DateTime.UtcNow;

            if (ModifiedAtUtc == default)
                ModifiedAtUtc = CreatedAtUtc;

            Modules ??= new Dictionary<string, ModuleProjectData>(StringComparer.OrdinalIgnoreCase);
            AirSettings ??= new AirModuleSettings();
        }
    }

    [Serializable]
    public class ModuleProjectData
    {
        public string ModuleId { get; set; } = "";
        public string ModuleSchemaVersion { get; set; } = "1.0";
        public Dictionary<string, string> Inputs { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> Settings { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> LastResultMetadata { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    [Serializable]
    public class AirModuleSettings
    {
        public string AirSystemType { get; set; } = "Supply Air";
    }
}
