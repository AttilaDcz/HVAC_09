using System;

namespace HVACDesigner.Services
{
    // --- LÉGTECHNIKAI ENUMOK ---
    public enum AirFlowUnit { CubicMeterPerHour, LitersPerSecond, CubicMeterPerSecond }
    public enum AirPressureUnit { Pascal, Kilopascal, Millibar, MillimetersOfWater }

    // --- HIDRAULIKAI (FŰTÉS/HŰTÉS/HMV) ENUMOK ---
    public enum FluidFlowUnit { LitersPerSecond, LitersPerHour, CubicMeterPerHour, KilogramPerHour }
    public enum FluidPressureUnit { Pascal, Kilopascal, Bar, MeterOfWater }

    // --- ÁLTALÁNOS ÉPÜLETGÉPÉSZETI ENUMOK ---
    public enum TemperatureUnit { Celsius, Kelvin, Fahrenheit }
    public enum LengthUnit { Millimeter, Centimeter, Meter, Inch }
    public enum AreaUnit { SquareMillimeter, SquareCentimeter, SquareMeter }
    public enum VolumeUnit { Liter, CubicMeter }
    public enum VelocityUnit { MeterPerSecond, MeterPerMinute }
    public enum PowerUnit { Watt, Kilowatt }
    public enum DensityUnit { KilogramPerCubicMeter }
    public enum RoughnessUnit { Millimeter, Meter }
    public enum ThermalConductivityUnit { WattPerMeterKelvin }
    public enum HeatTransferCoefficientUnit { WattPerSquareMeterKelvin }
    public enum ThermalResistanceUnit { SquareMeterKelvinPerWatt }
    public enum SpecificHeatUnit { JoulePerKilogramKelvin, KilojoulePerKilogramKelvin }
    public enum AirChangeRateUnit { PerHour }
    public enum RelativeHumidityUnit { Percent, Fraction }
    public enum HumidityRatioUnit { GramPerKilogram, KilogramPerKilogram }
    public enum PressureGradientUnit { PascalPerMeter }

    public static class UnitContext
    {
        // Globális esemény, amire az egyedi mérnöki editorok feliratkoznak.
        public static event EventHandler? UnitChanged;

        // --- SZAKÁGI ALRENDSZEREK (HIERARCHIA) ---
        public static AirUnitContext Air { get; } = new AirUnitContext();
        public static HydraulicsUnitContext Hydraulics { get; } = new HydraulicsUnitContext();
        public static EnergeticsUnitContext Energetics { get; } = new EnergeticsUnitContext();
        public static SanitaryUnitContext Sanitary { get; } = new SanitaryUnitContext();
        public static GeneralUnitContext General { get; } = new GeneralUnitContext();

        public static void NotifyUnitChanged()
        {
            UnitChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void TriggerUnitChanged()
        {
            NotifyUnitChanged();
        }

        // --- LÉGTECHNIKAI KONTEXTUS OSZTÁLY ---
        public class AirUnitContext
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

            // Globális levegő fizikai paraméterek.
            public double DesignTemperatureCelsius { get; set; } = 20.0;

            public double GetDensity()
            {
                // Alapértelmezett száraz levegő sűrűség p=101325 Pa mellett.
                return 1.2041 * (293.15 / (DesignTemperatureCelsius + 273.15));
            }

            public double GetViscosity()
            {
                double tKelvin = DesignTemperatureCelsius + 273.15;
                return 1.716e-5 *
                    Math.Pow(tKelvin / 273.15, 1.5) *
                    ((273.15 + 110.4) / (tKelvin + 110.4)) /
                    GetDensity();
            }

            public double ConvertFlowToDisplay(double m3PerSecondValue)
            {
                return Flow switch
                {
                    AirFlowUnit.CubicMeterPerHour => m3PerSecondValue * 3600.0,
                    AirFlowUnit.LitersPerSecond => m3PerSecondValue * 1000.0,
                    _ => m3PerSecondValue
                };
            }

            public double ConvertFlowFromDisplay(double displayValue)
            {
                return Flow switch
                {
                    AirFlowUnit.CubicMeterPerHour => displayValue / 3600.0,
                    AirFlowUnit.LitersPerSecond => displayValue / 1000.0,
                    _ => displayValue
                };
            }

            public double ConvertPressureToDisplay(double pascalValue)
            {
                return Pressure switch
                {
                    AirPressureUnit.Kilopascal => pascalValue / 1000.0,
                    AirPressureUnit.Millibar => pascalValue / 100.0,
                    AirPressureUnit.MillimetersOfWater => pascalValue / 9.80665,
                    _ => pascalValue
                };
            }

            public double ConvertPressureFromDisplay(double displayValue)
            {
                return Pressure switch
                {
                    AirPressureUnit.Kilopascal => displayValue * 1000.0,
                    AirPressureUnit.Millibar => displayValue * 100.0,
                    AirPressureUnit.MillimetersOfWater => displayValue * 9.80665,
                    _ => displayValue
                };
            }

            public double ConvertVelocityToDisplay(double meterPerSecondValue)
            {
                return Velocity == VelocityUnit.MeterPerMinute
                    ? meterPerSecondValue * 60.0
                    : meterPerSecondValue;
            }

            public double ConvertVelocityFromDisplay(double displayValue)
            {
                return Velocity == VelocityUnit.MeterPerMinute
                    ? displayValue / 60.0
                    : displayValue;
            }

            public double ConvertDimensionToDisplay(double meterValue)
            {
                return UnitContext.ConvertLengthToDisplay(meterValue, Dimension);
            }

            public double ConvertDimensionFromDisplay(double displayValue)
            {
                return UnitContext.ConvertLengthFromDisplay(displayValue, Dimension);
            }

            public double ConvertLengthToDisplay(double meterValue)
            {
                return UnitContext.ConvertLengthToDisplay(meterValue, Length);
            }

            public double ConvertLengthFromDisplay(double displayValue)
            {
                return UnitContext.ConvertLengthFromDisplay(displayValue, Length);
            }

            public double ConvertAreaToDisplay(double squareMeterValue)
            {
                return UnitContext.ConvertAreaToDisplay(squareMeterValue, Area);
            }

            public double ConvertAreaFromDisplay(double displayValue)
            {
                return UnitContext.ConvertAreaFromDisplay(displayValue, Area);
            }

            public double ConvertVolumeToDisplay(double cubicMeterValue)
            {
                return UnitContext.ConvertVolumeToDisplay(cubicMeterValue, Volume);
            }

            public double ConvertVolumeFromDisplay(double displayValue)
            {
                return UnitContext.ConvertVolumeFromDisplay(displayValue, Volume);
            }

            public string GetFlowUnitLabel() => GetAirFlowUnitLabel(Flow);
            public string GetPressureUnitLabel() => UnitContext.GetAirPressureUnitLabel(Pressure);
            public string GetVelocityUnitLabel() => UnitContext.GetVelocityUnitLabel(Velocity);
            public string GetDimensionUnitLabel() => UnitContext.GetLengthUnitLabel(Dimension);
            public string GetLengthUnitLabel() => UnitContext.GetLengthUnitLabel(Length);
            public string GetAreaUnitLabel() => UnitContext.GetAreaUnitLabel(Area);
            public string GetVolumeUnitLabel() => UnitContext.GetVolumeUnitLabel(Volume);
            public string GetDensityUnitLabel() => UnitContext.GetDensityUnitLabel(Density);
            public string GetRoughnessUnitLabel() => UnitContext.GetRoughnessUnitLabel(Roughness);
            public string GetPressureGradientUnitLabel() => UnitContext.GetPressureGradientUnitLabel(PressureGradient);
            public string GetAirChangeRateUnitLabel() => UnitContext.GetAirChangeRateUnitLabel(AirChangeRate);
            public string GetRelativeHumidityUnitLabel() => UnitContext.GetRelativeHumidityUnitLabel(RelativeHumidity);
            public string GetHumidityRatioUnitLabel() => UnitContext.GetHumidityRatioUnitLabel(HumidityRatio);
        }

        // --- HIDRAULIKAI (FŰTÉS/HŰTÉS) KONTEXTUS OSZTÁLY ---
        public class HydraulicsUnitContext
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

            public double FluidTemperatureCelsius { get; set; } = 75.0;
            public double GlycolPercentage { get; set; } = 0.0;

            public double GetDensity()
            {
                double t = FluidTemperatureCelsius;
                double waterDensity = 1000.0 - 0.0178 * Math.Pow(t - 4.0, 1.7);
                double glycolFactor = GlycolPercentage * 1.5;
                return waterDensity + glycolFactor;
            }

            public double ConvertFlowToDisplay(double m3PerSecondValue)
            {
                double m3PerHour = m3PerSecondValue * 3600.0;
                return Flow switch
                {
                    FluidFlowUnit.LitersPerSecond => m3PerSecondValue * 1000.0,
                    FluidFlowUnit.LitersPerHour => m3PerSecondValue * 3600000.0,
                    FluidFlowUnit.KilogramPerHour => m3PerHour * GetDensity(),
                    _ => m3PerHour
                };
            }

            public double ConvertFlowFromDisplay(double displayValue)
            {
                return Flow switch
                {
                    FluidFlowUnit.LitersPerSecond => displayValue / 1000.0,
                    FluidFlowUnit.LitersPerHour => displayValue / 3600000.0,
                    FluidFlowUnit.KilogramPerHour => (displayValue / GetDensity()) / 3600.0,
                    _ => displayValue / 3600.0
                };
            }

            public double ConvertPressureToDisplay(double pascalValue)
            {
                return Pressure switch
                {
                    FluidPressureUnit.Kilopascal => pascalValue / 1000.0,
                    FluidPressureUnit.Bar => pascalValue / 100000.0,
                    FluidPressureUnit.MeterOfWater => pascalValue / 9806.65,
                    _ => pascalValue
                };
            }

            public double ConvertPressureFromDisplay(double displayValue)
            {
                return Pressure switch
                {
                    FluidPressureUnit.Kilopascal => displayValue * 1000.0,
                    FluidPressureUnit.Bar => displayValue * 100000.0,
                    FluidPressureUnit.MeterOfWater => displayValue * 9806.65,
                    _ => displayValue
                };
            }

            public double ConvertDimensionToDisplay(double meterValue)
            {
                return UnitContext.ConvertLengthToDisplay(meterValue, Dimension);
            }

            public double ConvertDimensionFromDisplay(double displayValue)
            {
                return UnitContext.ConvertLengthFromDisplay(displayValue, Dimension);
            }

            public double ConvertLengthToDisplay(double meterValue)
            {
                return UnitContext.ConvertLengthToDisplay(meterValue, Length);
            }

            public double ConvertLengthFromDisplay(double displayValue)
            {
                return UnitContext.ConvertLengthFromDisplay(displayValue, Length);
            }

            public double ConvertVelocityToDisplay(double meterPerSecondValue)
            {
                return Velocity == VelocityUnit.MeterPerMinute
                    ? meterPerSecondValue * 60.0
                    : meterPerSecondValue;
            }

            public double ConvertVelocityFromDisplay(double displayValue)
            {
                return Velocity == VelocityUnit.MeterPerMinute
                    ? displayValue / 60.0
                    : displayValue;
            }

            public double ConvertPowerToDisplay(double wattValue)
            {
                return UnitContext.ConvertPowerToDisplay(wattValue, Power);
            }

            public double ConvertPowerFromDisplay(double displayValue)
            {
                return UnitContext.ConvertPowerFromDisplay(displayValue, Power);
            }

            public string GetFlowUnitLabel() => GetFluidFlowUnitLabel(Flow);
            public string GetPressureUnitLabel() => GetFluidPressureUnitLabel(Pressure);
            public string GetDimensionUnitLabel() => UnitContext.GetLengthUnitLabel(Dimension);
            public string GetLengthUnitLabel() => UnitContext.GetLengthUnitLabel(Length);
            public string GetVelocityUnitLabel() => UnitContext.GetVelocityUnitLabel(Velocity);
            public string GetPowerUnitLabel() => UnitContext.GetPowerUnitLabel(Power);
            public string GetDensityUnitLabel() => UnitContext.GetDensityUnitLabel(Density);
            public string GetPressureGradientUnitLabel() => UnitContext.GetPressureGradientUnitLabel(PressureGradient);
        }

        // --- ÉPÜLETENERGETIKAI KONTEXTUS OSZTÁLY ---
        public class EnergeticsUnitContext
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
                SpecificHeatUnit.KilojoulePerKilogramKelvin;
            public DensityUnit Density { get; set; } = DensityUnit.KilogramPerCubicMeter;

            public double ConvertThicknessToDisplay(double meterValue)
            {
                return UnitContext.ConvertLengthToDisplay(meterValue, Thickness);
            }

            public double ConvertThicknessFromDisplay(double displayValue)
            {
                return UnitContext.ConvertLengthFromDisplay(displayValue, Thickness);
            }

            public double ConvertLengthToDisplay(double meterValue)
            {
                return UnitContext.ConvertLengthToDisplay(meterValue, Length);
            }

            public double ConvertLengthFromDisplay(double displayValue)
            {
                return UnitContext.ConvertLengthFromDisplay(displayValue, Length);
            }

            public double ConvertAreaToDisplay(double squareMeterValue)
            {
                return UnitContext.ConvertAreaToDisplay(squareMeterValue, Area);
            }

            public double ConvertAreaFromDisplay(double displayValue)
            {
                return UnitContext.ConvertAreaFromDisplay(displayValue, Area);
            }

            public double ConvertVolumeToDisplay(double cubicMeterValue)
            {
                return UnitContext.ConvertVolumeToDisplay(cubicMeterValue, Volume);
            }

            public double ConvertVolumeFromDisplay(double displayValue)
            {
                return UnitContext.ConvertVolumeFromDisplay(displayValue, Volume);
            }

            public double ConvertHeatFlowToDisplay(double wattValue)
            {
                return UnitContext.ConvertPowerToDisplay(wattValue, HeatFlow);
            }

            public double ConvertHeatFlowFromDisplay(double displayValue)
            {
                return UnitContext.ConvertPowerFromDisplay(displayValue, HeatFlow);
            }

            public string GetThicknessUnitLabel() => UnitContext.GetLengthUnitLabel(Thickness);
            public string GetLengthUnitLabel() => UnitContext.GetLengthUnitLabel(Length);
            public string GetAreaUnitLabel() => UnitContext.GetAreaUnitLabel(Area);
            public string GetVolumeUnitLabel() => UnitContext.GetVolumeUnitLabel(Volume);
            public string GetHeatFlowUnitLabel() => UnitContext.GetPowerUnitLabel(HeatFlow);
            public string GetThermalConductivityUnitLabel() => UnitContext.GetThermalConductivityUnitLabel(ThermalConductivity);
            public string GetUValueUnitLabel() => GetHeatTransferCoefficientUnitLabel(UValue);
            public string GetAlphaUnitLabel() => GetHeatTransferCoefficientUnitLabel(Alpha);
            public string GetThermalResistanceUnitLabel() => UnitContext.GetThermalResistanceUnitLabel(ThermalResistance);
            public string GetSpecificHeatUnitLabel() => UnitContext.GetSpecificHeatUnitLabel(SpecificHeat);
            public string GetDensityUnitLabel() => UnitContext.GetDensityUnitLabel(Density);
        }

        // --- VÍZ-CSATORNA KONTEXTUS OSZTÁLY ---
        public class SanitaryUnitContext
        {
            public FluidFlowUnit Flow { get; set; } = FluidFlowUnit.LitersPerSecond;
            public FluidPressureUnit Pressure { get; set; } = FluidPressureUnit.Bar;
            public LengthUnit Dimension { get; set; } = LengthUnit.Millimeter;
            public LengthUnit Length { get; set; } = LengthUnit.Meter;
            public PressureGradientUnit PressureGradient { get; set; } = PressureGradientUnit.PascalPerMeter;

            public double ConvertFlowToDisplay(double m3PerSecondValue)
            {
                return Flow switch
                {
                    FluidFlowUnit.LitersPerSecond => m3PerSecondValue * 1000.0,
                    FluidFlowUnit.LitersPerHour => m3PerSecondValue * 3600000.0,
                    FluidFlowUnit.KilogramPerHour => m3PerSecondValue * 3600.0 * 1000.0,
                    _ => m3PerSecondValue * 3600.0
                };
            }

            public double ConvertFlowFromDisplay(double displayValue)
            {
                return Flow switch
                {
                    FluidFlowUnit.LitersPerSecond => displayValue / 1000.0,
                    FluidFlowUnit.LitersPerHour => displayValue / 3600000.0,
                    FluidFlowUnit.KilogramPerHour => (displayValue / 1000.0) / 3600.0,
                    _ => displayValue / 3600.0
                };
            }

            public double ConvertPressureToDisplay(double pascalValue)
            {
                return Pressure switch
                {
                    FluidPressureUnit.Kilopascal => pascalValue / 1000.0,
                    FluidPressureUnit.Bar => pascalValue / 100000.0,
                    FluidPressureUnit.MeterOfWater => pascalValue / 9806.65,
                    _ => pascalValue
                };
            }

            public double ConvertPressureFromDisplay(double displayValue)
            {
                return Pressure switch
                {
                    FluidPressureUnit.Kilopascal => displayValue * 1000.0,
                    FluidPressureUnit.Bar => displayValue * 100000.0,
                    FluidPressureUnit.MeterOfWater => displayValue * 9806.65,
                    _ => displayValue
                };
            }

            public double ConvertDimensionToDisplay(double meterValue)
            {
                return UnitContext.ConvertLengthToDisplay(meterValue, Dimension);
            }

            public double ConvertDimensionFromDisplay(double displayValue)
            {
                return UnitContext.ConvertLengthFromDisplay(displayValue, Dimension);
            }

            public double ConvertLengthToDisplay(double meterValue)
            {
                return UnitContext.ConvertLengthToDisplay(meterValue, Length);
            }

            public double ConvertLengthFromDisplay(double displayValue)
            {
                return UnitContext.ConvertLengthFromDisplay(displayValue, Length);
            }

            public string GetFlowUnitLabel() => GetFluidFlowUnitLabel(Flow);
            public string GetPressureUnitLabel() => GetFluidPressureUnitLabel(Pressure);
            public string GetDimensionUnitLabel() => UnitContext.GetLengthUnitLabel(Dimension);
            public string GetLengthUnitLabel() => UnitContext.GetLengthUnitLabel(Length);
            public string GetPressureGradientUnitLabel() => UnitContext.GetPressureGradientUnitLabel(PressureGradient);
        }

        // --- ÁLTALÁNOS KONTEXTUS ---
        public class GeneralUnitContext
        {
            public TemperatureUnit Temperature { get; set; } = TemperatureUnit.Celsius;
            public LengthUnit Length { get; set; } = LengthUnit.Meter;
            public AreaUnit Area { get; set; } = AreaUnit.SquareMeter;
            public VolumeUnit Volume { get; set; } = VolumeUnit.CubicMeter;
            public PowerUnit Power { get; set; } = PowerUnit.Kilowatt;

            public double ConvertTemperatureToDisplay(double celsiusValue)
            {
                return UnitContext.ConvertTemperatureToDisplay(celsiusValue, Temperature);
            }

            public double ConvertTemperatureFromDisplay(double displayValue)
            {
                return UnitContext.ConvertTemperatureFromDisplay(displayValue, Temperature);
            }

            public string GetTemperatureUnitLabel() => UnitContext.GetTemperatureUnitLabel(Temperature);
            public string GetLengthUnitLabel() => UnitContext.GetLengthUnitLabel(Length);
            public string GetAreaUnitLabel() => UnitContext.GetAreaUnitLabel(Area);
            public string GetVolumeUnitLabel() => UnitContext.GetVolumeUnitLabel(Volume);
            public string GetPowerUnitLabel() => UnitContext.GetPowerUnitLabel(Power);
        }

        public static double ConvertLengthToDisplay(double meterValue, LengthUnit unit)
        {
            return unit switch
            {
                LengthUnit.Millimeter => meterValue * 1000.0,
                LengthUnit.Centimeter => meterValue * 100.0,
                LengthUnit.Inch => meterValue / 0.0254,
                _ => meterValue
            };
        }

        public static double ConvertLengthFromDisplay(double displayValue, LengthUnit unit)
        {
            return unit switch
            {
                LengthUnit.Millimeter => displayValue / 1000.0,
                LengthUnit.Centimeter => displayValue / 100.0,
                LengthUnit.Inch => displayValue * 0.0254,
                _ => displayValue
            };
        }

        public static double ConvertAreaToDisplay(double squareMeterValue, AreaUnit unit)
        {
            return unit switch
            {
                AreaUnit.SquareMillimeter => squareMeterValue * 1000000.0,
                AreaUnit.SquareCentimeter => squareMeterValue * 10000.0,
                _ => squareMeterValue
            };
        }

        public static double ConvertAreaFromDisplay(double displayValue, AreaUnit unit)
        {
            return unit switch
            {
                AreaUnit.SquareMillimeter => displayValue / 1000000.0,
                AreaUnit.SquareCentimeter => displayValue / 10000.0,
                _ => displayValue
            };
        }

        public static double ConvertVolumeToDisplay(double cubicMeterValue, VolumeUnit unit)
        {
            return unit == VolumeUnit.Liter ? cubicMeterValue * 1000.0 : cubicMeterValue;
        }

        public static double ConvertVolumeFromDisplay(double displayValue, VolumeUnit unit)
        {
            return unit == VolumeUnit.Liter ? displayValue / 1000.0 : displayValue;
        }

        public static double ConvertPowerToDisplay(double wattValue, PowerUnit unit)
        {
            return unit == PowerUnit.Kilowatt ? wattValue / 1000.0 : wattValue;
        }

        public static double ConvertPowerFromDisplay(double displayValue, PowerUnit unit)
        {
            return unit == PowerUnit.Kilowatt ? displayValue * 1000.0 : displayValue;
        }

        public static double ConvertTemperatureToDisplay(double celsiusValue, TemperatureUnit unit)
        {
            return unit switch
            {
                TemperatureUnit.Kelvin => celsiusValue + 273.15,
                TemperatureUnit.Fahrenheit => celsiusValue * 9.0 / 5.0 + 32.0,
                _ => celsiusValue
            };
        }

        public static double ConvertTemperatureFromDisplay(double displayValue, TemperatureUnit unit)
        {
            return unit switch
            {
                TemperatureUnit.Kelvin => displayValue - 273.15,
                TemperatureUnit.Fahrenheit => (displayValue - 32.0) * 5.0 / 9.0,
                _ => displayValue
            };
        }

        public static string GetAirFlowUnitLabel(AirFlowUnit unit)
        {
            return unit switch
            {
                AirFlowUnit.LitersPerSecond => "l/s",
                AirFlowUnit.CubicMeterPerSecond => "m³/s",
                _ => "m³/h"
            };
        }

        public static string GetAirPressureUnitLabel(AirPressureUnit unit)
        {
            return unit switch
            {
                AirPressureUnit.Kilopascal => "kPa",
                AirPressureUnit.Millibar => "mbar",
                AirPressureUnit.MillimetersOfWater => "mmH₂O",
                _ => "Pa"
            };
        }

        public static string GetFluidFlowUnitLabel(FluidFlowUnit unit)
        {
            return unit switch
            {
                FluidFlowUnit.LitersPerSecond => "l/s",
                FluidFlowUnit.LitersPerHour => "l/h",
                FluidFlowUnit.KilogramPerHour => "kg/h",
                _ => "m³/h"
            };
        }

        public static string GetFluidPressureUnitLabel(FluidPressureUnit unit)
        {
            return unit switch
            {
                FluidPressureUnit.Kilopascal => "kPa",
                FluidPressureUnit.Bar => "bar",
                FluidPressureUnit.MeterOfWater => "mH₂O",
                _ => "Pa"
            };
        }

        public static string GetTemperatureUnitLabel(TemperatureUnit unit)
        {
            return unit switch
            {
                TemperatureUnit.Kelvin => "K",
                TemperatureUnit.Fahrenheit => "°F",
                _ => "°C"
            };
        }

        public static string GetLengthUnitLabel(LengthUnit unit)
        {
            return unit switch
            {
                LengthUnit.Millimeter => "mm",
                LengthUnit.Centimeter => "cm",
                LengthUnit.Inch => "in",
                _ => "m"
            };
        }

        public static string GetAreaUnitLabel(AreaUnit unit)
        {
            return unit switch
            {
                AreaUnit.SquareMillimeter => "mm²",
                AreaUnit.SquareCentimeter => "cm²",
                _ => "m²"
            };
        }

        public static string GetVolumeUnitLabel(VolumeUnit unit)
        {
            return unit == VolumeUnit.Liter ? "l" : "m³";
        }

        public static string GetVelocityUnitLabel(VelocityUnit unit)
        {
            return unit == VelocityUnit.MeterPerMinute ? "m/min" : "m/s";
        }

        public static string GetPowerUnitLabel(PowerUnit unit)
        {
            return unit == PowerUnit.Kilowatt ? "kW" : "W";
        }

        public static string GetDensityUnitLabel(DensityUnit unit)
        {
            return "kg/m³";
        }

        public static string GetRoughnessUnitLabel(RoughnessUnit unit)
        {
            return unit == RoughnessUnit.Meter ? "m" : "mm";
        }

        public static string GetThermalConductivityUnitLabel(ThermalConductivityUnit unit)
        {
            return "W/(m·K)";
        }

        public static string GetHeatTransferCoefficientUnitLabel(HeatTransferCoefficientUnit unit)
        {
            return "W/(m²·K)";
        }

        public static string GetThermalResistanceUnitLabel(ThermalResistanceUnit unit)
        {
            return "m²K/W";
        }

        public static string GetSpecificHeatUnitLabel(SpecificHeatUnit unit)
        {
            return unit == SpecificHeatUnit.KilojoulePerKilogramKelvin
                ? "kJ/(kg·K)"
                : "J/(kg·K)";
        }

        public static string GetAirChangeRateUnitLabel(AirChangeRateUnit unit)
        {
            return "1/h";
        }

        public static string GetRelativeHumidityUnitLabel(RelativeHumidityUnit unit)
        {
            return unit == RelativeHumidityUnit.Fraction ? "-" : "%";
        }

        public static string GetHumidityRatioUnitLabel(HumidityRatioUnit unit)
        {
            return unit == HumidityRatioUnit.KilogramPerKilogram ? "kg/kg" : "g/kg";
        }

        public static string GetPressureGradientUnitLabel(PressureGradientUnit unit)
        {
            return "Pa/m";
        }
    }
}
