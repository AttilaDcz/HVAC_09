using System;
using System.Collections.Generic;

namespace HVACDesigner.Services
{
    [Serializable]
    public class UserSettings
    {
        public const string CurrentSchemaVersion = "1.0";

        public string SettingsSchemaVersion { get; set; } = CurrentSchemaVersion;

        // Kompatibilitási propertyk: a meglévő SettingsForm ezeket használja.
        public string CurrentTheme { get; set; } = "Dark";
        public string CurrentSizeMode { get; set; } = "Normal";
        public int AutoSaveIntervalMinutes { get; set; } = 5;

        public string LastProjectFilePath { get; set; } = "";
        public List<string> RecentProjects { get; set; } = new List<string>();

        public WindowSettingsData Window { get; set; } = new WindowSettingsData();
        public PathSettingsData Paths { get; set; } = new PathSettingsData();
        public UnitSettingsData Units { get; set; } = UnitSettingsData.CreateDefault();
        public DeveloperSettingsData Developer { get; set; } = new DeveloperSettingsData();
        public List<PdfModuleExportSettingsData> PdfExportSettings { get; set; } =
            new List<PdfModuleExportSettingsData>();

        public string DefaultDesignerName { get; set; } = "";
        public string DefaultEligibilityNumber { get; set; } = "";
        public string DefaultCompany { get; set; } = "";
        public string DefaultDesignerPhone { get; set; } = "";
        public string DefaultDesignerEmail { get; set; } = "";

        public string DefaultDesZip { get; set; } = "";
        public string DefaultDesCity { get; set; } = "";
        public string DefaultDesStreet { get; set; } = "";
        public string DefaultDesStreetType { get; set; } = "";
        public string DefaultDesHouse { get; set; } = "";

        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(SettingsSchemaVersion))
                SettingsSchemaVersion = CurrentSchemaVersion;

            if (string.IsNullOrWhiteSpace(CurrentTheme))
                CurrentTheme = "Dark";

            if (string.IsNullOrWhiteSpace(CurrentSizeMode))
                CurrentSizeMode = "Normal";

            if (AutoSaveIntervalMinutes < 1)
                AutoSaveIntervalMinutes = 5;

            RecentProjects ??= new List<string>();
            if (RecentProjects.Count > 10)
                RecentProjects = RecentProjects.GetRange(0, 10);

            Window ??= new WindowSettingsData();
            Paths ??= new PathSettingsData();
            Units ??= UnitSettingsData.CreateDefault();
            Developer ??= new DeveloperSettingsData();
            PdfExportSettings ??= new List<PdfModuleExportSettingsData>();
            Units.Normalize();
        }
    }

    [Serializable]
    public class PdfModuleExportSettingsData
    {
        public string ModuleId { get; set; } = "";
        public bool IncludeProjectData { get; set; } = true;
        public bool IncludeDesignerData { get; set; } = true;
        public bool IncludeFixtures { get; set; } = true;
        public bool IncludeCalculationInputs { get; set; } = true;
        public bool IncludeResults { get; set; } = true;
        public bool IncludeStandards { get; set; } = true;
        public bool IncludeNotes { get; set; } = true;
        public bool IncludeSignature { get; set; } = true;
        public bool IncludeFooter { get; set; } = true;
        public bool IncludePageNumbers { get; set; } = true;
        public bool IncludeDate { get; set; } = true;
        public bool IncludeProgramVersion { get; set; } = true;
        public string PaperSize { get; set; } = "A4";
        public string Orientation { get; set; } = "Portrait";
    }

    [Serializable]
    public class WindowSettingsData
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; } = 1150;
        public int Height { get; set; } = 780;
        public bool IsMaximized { get; set; }
        public bool IsNavigationExpanded { get; set; } = true;
        public string LastActiveModuleId { get; set; } = "Dashboard";
    }

    [Serializable]
    public class PathSettingsData
    {
        public string AutoSavePath { get; set; } = "";
        public string LogFolder { get; set; } = "";
        public string CacheFolder { get; set; } = "";
        public string UserXmlFolder { get; set; } = "";
        public string ExportFolder { get; set; } = "";
    }

    [Serializable]
    public class DeveloperSettingsData
    {
        public bool EnableDebugViews { get; set; }
        public bool ShowBootstrapDiagnostics { get; set; }
        public bool VerboseNotifications { get; set; }
    }

    [Serializable]
    public class UnitSettingsData
    {
        public AirUnitSettingsData Air { get; set; } = new AirUnitSettingsData();
        public HydraulicsUnitSettingsData Hydraulics { get; set; } = new HydraulicsUnitSettingsData();
        public EnergeticsUnitSettingsData Energetics { get; set; } = new EnergeticsUnitSettingsData();
        public SanitaryUnitSettingsData Sanitary { get; set; } = new SanitaryUnitSettingsData();
        public GeneralUnitSettingsData General { get; set; } = new GeneralUnitSettingsData();

        public static UnitSettingsData CreateDefault()
        {
            return new UnitSettingsData();
        }

        public void Normalize()
        {
            Air ??= new AirUnitSettingsData();
            Hydraulics ??= new HydraulicsUnitSettingsData();
            Energetics ??= new EnergeticsUnitSettingsData();
            Sanitary ??= new SanitaryUnitSettingsData();
            General ??= new GeneralUnitSettingsData();
        }
    }

    [Serializable]
    public class AirUnitSettingsData
    {
        public AirFlowUnit Flow { get; set; } = AirFlowUnit.CubicMeterPerHour;
        public AirPressureUnit Pressure { get; set; } = AirPressureUnit.Pascal;
        public TemperatureUnit Temperature { get; set; } = TemperatureUnit.Celsius;
        public LengthUnit Dimension { get; set; } = LengthUnit.Millimeter;
        public LengthUnit Length { get; set; } = LengthUnit.Meter;
        public AreaUnit Area { get; set; } = AreaUnit.SquareMeter;
        public VolumeUnit Volume { get; set; } = VolumeUnit.CubicMeter;
        public VelocityUnit Velocity { get; set; } = VelocityUnit.MeterPerSecond;
        public DensityUnit Density { get; set; } = DensityUnit.KilogramPerCubicMeter;
        public RoughnessUnit Roughness { get; set; } = RoughnessUnit.Millimeter;
        public PressureGradientUnit PressureGradient { get; set; } = PressureGradientUnit.PascalPerMeter;
        public AirChangeRateUnit AirChangeRate { get; set; } = AirChangeRateUnit.PerHour;
        public RelativeHumidityUnit RelativeHumidity { get; set; } = RelativeHumidityUnit.Percent;
        public HumidityRatioUnit HumidityRatio { get; set; } = HumidityRatioUnit.GramPerKilogram;
        public double DesignTemperatureCelsius { get; set; } = 20.0;
    }

    [Serializable]
    public class HydraulicsUnitSettingsData
    {
        public FluidFlowUnit Flow { get; set; } = FluidFlowUnit.CubicMeterPerHour;
        public FluidPressureUnit Pressure { get; set; } = FluidPressureUnit.Pascal;
        public TemperatureUnit Temperature { get; set; } = TemperatureUnit.Celsius;
        public LengthUnit Dimension { get; set; } = LengthUnit.Millimeter;
        public LengthUnit Length { get; set; } = LengthUnit.Meter;
        public VelocityUnit Velocity { get; set; } = VelocityUnit.MeterPerSecond;
        public PowerUnit Power { get; set; } = PowerUnit.Kilowatt;
        public DensityUnit Density { get; set; } = DensityUnit.KilogramPerCubicMeter;
        public PressureGradientUnit PressureGradient { get; set; } = PressureGradientUnit.PascalPerMeter;
    }

    [Serializable]
    public class EnergeticsUnitSettingsData
    {
        public LengthUnit Thickness { get; set; } = LengthUnit.Centimeter;
        public LengthUnit Length { get; set; } = LengthUnit.Meter;
        public AreaUnit Area { get; set; } = AreaUnit.SquareMeter;
        public VolumeUnit Volume { get; set; } = VolumeUnit.CubicMeter;
        public TemperatureUnit Temperature { get; set; } = TemperatureUnit.Celsius;
        public PowerUnit HeatFlow { get; set; } = PowerUnit.Watt;
        public ThermalConductivityUnit ThermalConductivity { get; set; } =
            ThermalConductivityUnit.WattPerMeterKelvin;
        public HeatTransferCoefficientUnit UValue { get; set; } =
            HeatTransferCoefficientUnit.WattPerSquareMeterKelvin;
        public HeatTransferCoefficientUnit Alpha { get; set; } =
            HeatTransferCoefficientUnit.WattPerSquareMeterKelvin;
        public ThermalResistanceUnit ThermalResistance { get; set; } =
            ThermalResistanceUnit.SquareMeterKelvinPerWatt;
        public SpecificHeatUnit SpecificHeat { get; set; } =
            SpecificHeatUnit.JoulePerKilogramKelvin;
        public DensityUnit Density { get; set; } = DensityUnit.KilogramPerCubicMeter;
    }

    [Serializable]
    public class SanitaryUnitSettingsData
    {
        public FluidFlowUnit Flow { get; set; } = FluidFlowUnit.LitersPerSecond;
        public FluidPressureUnit Pressure { get; set; } = FluidPressureUnit.Bar;
        public LengthUnit Dimension { get; set; } = LengthUnit.Millimeter;
        public LengthUnit Length { get; set; } = LengthUnit.Meter;
        public PressureGradientUnit PressureGradient { get; set; } = PressureGradientUnit.PascalPerMeter;
    }

    [Serializable]
    public class GeneralUnitSettingsData
    {
        public TemperatureUnit Temperature { get; set; } = TemperatureUnit.Celsius;
        public LengthUnit Length { get; set; } = LengthUnit.Meter;
        public AreaUnit Area { get; set; } = AreaUnit.SquareMeter;
        public VolumeUnit Volume { get; set; } = VolumeUnit.CubicMeter;
        public PowerUnit Power { get; set; } = PowerUnit.Kilowatt;
    }
}
