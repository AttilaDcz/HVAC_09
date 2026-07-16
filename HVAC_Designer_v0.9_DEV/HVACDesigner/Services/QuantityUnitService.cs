using System;
using System.Globalization;

namespace HVACDesigner.Services
{
    public static class QuantityUnitService
    {
        public static QuantityUnitInfo GetInfo(QuantityKind kind)
        {
            return kind switch
            {
                QuantityKind.None => Create(kind, string.Empty, 2, Identity, Identity),

                QuantityKind.Length => Create(
                    kind,
                    UnitContext.General.GetLengthUnitLabel(),
                    GetLengthDecimals(UnitContext.General.Length),
                    value => UnitContext.ConvertLengthToDisplay(value, UnitContext.General.Length),
                    value => UnitContext.ConvertLengthFromDisplay(value, UnitContext.General.Length)),

                QuantityKind.Area => Create(
                    kind,
                    UnitContext.General.GetAreaUnitLabel(),
                    GetAreaDecimals(UnitContext.General.Area),
                    value => UnitContext.ConvertAreaToDisplay(value, UnitContext.General.Area),
                    value => UnitContext.ConvertAreaFromDisplay(value, UnitContext.General.Area)),

                QuantityKind.Volume => Create(
                    kind,
                    UnitContext.General.GetVolumeUnitLabel(),
                    GetVolumeDecimals(UnitContext.General.Volume),
                    value => UnitContext.ConvertVolumeToDisplay(value, UnitContext.General.Volume),
                    value => UnitContext.ConvertVolumeFromDisplay(value, UnitContext.General.Volume)),

                QuantityKind.Temperature => Create(
                    kind,
                    UnitContext.General.GetTemperatureUnitLabel(),
                    1,
                    UnitContext.General.ConvertTemperatureToDisplay,
                    UnitContext.General.ConvertTemperatureFromDisplay),

                QuantityKind.Power => Create(
                    kind,
                    UnitContext.General.GetPowerUnitLabel(),
                    GetPowerDecimals(UnitContext.General.Power),
                    value => UnitContext.ConvertPowerToDisplay(value, UnitContext.General.Power),
                    value => UnitContext.ConvertPowerFromDisplay(value, UnitContext.General.Power)),

                QuantityKind.AirFlow => Create(
                    kind,
                    UnitContext.Air.GetFlowUnitLabel(),
                    GetAirFlowDecimals(UnitContext.Air.Flow),
                    UnitContext.Air.ConvertFlowToDisplay,
                    UnitContext.Air.ConvertFlowFromDisplay),

                QuantityKind.AirPressure => Create(
                    kind,
                    UnitContext.Air.GetPressureUnitLabel(),
                    GetAirPressureDecimals(UnitContext.Air.Pressure),
                    UnitContext.Air.ConvertPressureToDisplay,
                    UnitContext.Air.ConvertPressureFromDisplay),

                QuantityKind.AirVelocity => Create(
                    kind,
                    UnitContext.Air.GetVelocityUnitLabel(),
                    GetVelocityDecimals(UnitContext.Air.Velocity),
                    UnitContext.Air.ConvertVelocityToDisplay,
                    UnitContext.Air.ConvertVelocityFromDisplay),

                QuantityKind.AirDimension => Create(
                    kind,
                    UnitContext.Air.GetDimensionUnitLabel(),
                    GetLengthDecimals(UnitContext.Air.Dimension),
                    UnitContext.Air.ConvertDimensionToDisplay,
                    UnitContext.Air.ConvertDimensionFromDisplay),

                QuantityKind.AirLength => Create(
                    kind,
                    UnitContext.Air.GetLengthUnitLabel(),
                    GetLengthDecimals(UnitContext.Air.Length),
                    UnitContext.Air.ConvertLengthToDisplay,
                    UnitContext.Air.ConvertLengthFromDisplay),

                QuantityKind.AirArea => Create(
                    kind,
                    UnitContext.Air.GetAreaUnitLabel(),
                    GetAreaDecimals(UnitContext.Air.Area),
                    UnitContext.Air.ConvertAreaToDisplay,
                    UnitContext.Air.ConvertAreaFromDisplay),

                QuantityKind.AirVolume => Create(
                    kind,
                    UnitContext.Air.GetVolumeUnitLabel(),
                    GetVolumeDecimals(UnitContext.Air.Volume),
                    UnitContext.Air.ConvertVolumeToDisplay,
                    UnitContext.Air.ConvertVolumeFromDisplay),

                QuantityKind.AirDensity => Create(
                    kind,
                    UnitContext.Air.GetDensityUnitLabel(),
                    3,
                    Identity,
                    Identity),

                QuantityKind.AirRoughness => Create(
                    kind,
                    UnitContext.Air.GetRoughnessUnitLabel(),
                    UnitContext.Air.Roughness == RoughnessUnit.Millimeter ? 3 : 6,
                    value => ConvertRoughnessToDisplay(value, UnitContext.Air.Roughness),
                    value => ConvertRoughnessFromDisplay(value, UnitContext.Air.Roughness)),

                QuantityKind.AirPressureGradient => FixedUnit(
                    kind,
                    UnitContext.Air.GetPressureGradientUnitLabel(),
                    1),

                QuantityKind.AirChangeRate => FixedUnit(
                    kind,
                    UnitContext.Air.GetAirChangeRateUnitLabel(),
                    2),

                QuantityKind.RelativeHumidity => Create(
                    kind,
                    UnitContext.Air.GetRelativeHumidityUnitLabel(),
                    UnitContext.Air.RelativeHumidity == RelativeHumidityUnit.Percent ? 0 : 3,
                    value => ConvertRelativeHumidityToDisplay(value, UnitContext.Air.RelativeHumidity),
                    value => ConvertRelativeHumidityFromDisplay(value, UnitContext.Air.RelativeHumidity)),

                QuantityKind.HumidityRatio => Create(
                    kind,
                    UnitContext.Air.GetHumidityRatioUnitLabel(),
                    UnitContext.Air.HumidityRatio == HumidityRatioUnit.GramPerKilogram ? 1 : 5,
                    value => ConvertHumidityRatioToDisplay(value, UnitContext.Air.HumidityRatio),
                    value => ConvertHumidityRatioFromDisplay(value, UnitContext.Air.HumidityRatio)),

                QuantityKind.HydraulicFlow => Create(
                    kind,
                    UnitContext.Hydraulics.GetFlowUnitLabel(),
                    GetFluidFlowDecimals(UnitContext.Hydraulics.Flow),
                    UnitContext.Hydraulics.ConvertFlowToDisplay,
                    UnitContext.Hydraulics.ConvertFlowFromDisplay),

                QuantityKind.HydraulicPressure => Create(
                    kind,
                    UnitContext.Hydraulics.GetPressureUnitLabel(),
                    GetFluidPressureDecimals(UnitContext.Hydraulics.Pressure),
                    UnitContext.Hydraulics.ConvertPressureToDisplay,
                    UnitContext.Hydraulics.ConvertPressureFromDisplay),

                QuantityKind.HydraulicVelocity => Create(
                    kind,
                    UnitContext.Hydraulics.GetVelocityUnitLabel(),
                    GetVelocityDecimals(UnitContext.Hydraulics.Velocity),
                    UnitContext.Hydraulics.ConvertVelocityToDisplay,
                    UnitContext.Hydraulics.ConvertVelocityFromDisplay),

                QuantityKind.HydraulicDimension => Create(
                    kind,
                    UnitContext.Hydraulics.GetDimensionUnitLabel(),
                    GetLengthDecimals(UnitContext.Hydraulics.Dimension),
                    UnitContext.Hydraulics.ConvertDimensionToDisplay,
                    UnitContext.Hydraulics.ConvertDimensionFromDisplay),

                QuantityKind.HydraulicLength => Create(
                    kind,
                    UnitContext.Hydraulics.GetLengthUnitLabel(),
                    GetLengthDecimals(UnitContext.Hydraulics.Length),
                    UnitContext.Hydraulics.ConvertLengthToDisplay,
                    UnitContext.Hydraulics.ConvertLengthFromDisplay),

                QuantityKind.HydraulicPower => Create(
                    kind,
                    UnitContext.Hydraulics.GetPowerUnitLabel(),
                    GetPowerDecimals(UnitContext.Hydraulics.Power),
                    UnitContext.Hydraulics.ConvertPowerToDisplay,
                    UnitContext.Hydraulics.ConvertPowerFromDisplay),

                QuantityKind.HydraulicDensity => Create(
                    kind,
                    UnitContext.Hydraulics.GetDensityUnitLabel(),
                    1,
                    Identity,
                    Identity),

                QuantityKind.HydraulicPressureGradient => FixedUnit(
                    kind,
                    UnitContext.Hydraulics.GetPressureGradientUnitLabel(),
                    1),

                QuantityKind.Thickness => Create(
                    kind,
                    UnitContext.Energetics.GetThicknessUnitLabel(),
                    GetLengthDecimals(UnitContext.Energetics.Thickness),
                    UnitContext.Energetics.ConvertThicknessToDisplay,
                    UnitContext.Energetics.ConvertThicknessFromDisplay),

                QuantityKind.ThermalConductivity => FixedUnit(
                    kind,
                    UnitContext.Energetics.GetThermalConductivityUnitLabel(),
                    3),

                QuantityKind.HeatTransferCoefficient => FixedUnit(
                    kind,
                    UnitContext.GetHeatTransferCoefficientUnitLabel(HeatTransferCoefficientUnit.WattPerSquareMeterKelvin),
                    3),

                QuantityKind.UValue => FixedUnit(
                    kind,
                    UnitContext.Energetics.GetUValueUnitLabel(),
                    3),

                QuantityKind.Alpha => FixedUnit(
                    kind,
                    UnitContext.Energetics.GetAlphaUnitLabel(),
                    2),

                QuantityKind.ThermalResistance => FixedUnit(
                    kind,
                    UnitContext.Energetics.GetThermalResistanceUnitLabel(),
                    3),

                QuantityKind.SpecificHeat => Create(
                    kind,
                    UnitContext.Energetics.GetSpecificHeatUnitLabel(),
                    UnitContext.Energetics.SpecificHeat == SpecificHeatUnit.KilojoulePerKilogramKelvin ? 3 : 0,
                    value => ConvertSpecificHeatToDisplay(value, UnitContext.Energetics.SpecificHeat),
                    value => ConvertSpecificHeatFromDisplay(value, UnitContext.Energetics.SpecificHeat)),

                QuantityKind.Density => Create(
                    kind,
                    UnitContext.Energetics.GetDensityUnitLabel(),
                    1,
                    Identity,
                    Identity),

                QuantityKind.HeatFlow => Create(
                    kind,
                    UnitContext.Energetics.GetHeatFlowUnitLabel(),
                    GetPowerDecimals(UnitContext.Energetics.HeatFlow),
                    UnitContext.Energetics.ConvertHeatFlowToDisplay,
                    UnitContext.Energetics.ConvertHeatFlowFromDisplay),

                QuantityKind.EnergeticLength => Create(
                    kind,
                    UnitContext.Energetics.GetLengthUnitLabel(),
                    GetLengthDecimals(UnitContext.Energetics.Length),
                    UnitContext.Energetics.ConvertLengthToDisplay,
                    UnitContext.Energetics.ConvertLengthFromDisplay),

                QuantityKind.EnergeticArea => Create(
                    kind,
                    UnitContext.Energetics.GetAreaUnitLabel(),
                    GetAreaDecimals(UnitContext.Energetics.Area),
                    UnitContext.Energetics.ConvertAreaToDisplay,
                    UnitContext.Energetics.ConvertAreaFromDisplay),

                QuantityKind.EnergeticVolume => Create(
                    kind,
                    UnitContext.Energetics.GetVolumeUnitLabel(),
                    GetVolumeDecimals(UnitContext.Energetics.Volume),
                    UnitContext.Energetics.ConvertVolumeToDisplay,
                    UnitContext.Energetics.ConvertVolumeFromDisplay),

                QuantityKind.SanitaryFlow => Create(
                    kind,
                    UnitContext.Sanitary.GetFlowUnitLabel(),
                    GetFluidFlowDecimals(UnitContext.Sanitary.Flow),
                    UnitContext.Sanitary.ConvertFlowToDisplay,
                    UnitContext.Sanitary.ConvertFlowFromDisplay),

                QuantityKind.SanitaryPressure => Create(
                    kind,
                    UnitContext.Sanitary.GetPressureUnitLabel(),
                    GetFluidPressureDecimals(UnitContext.Sanitary.Pressure),
                    UnitContext.Sanitary.ConvertPressureToDisplay,
                    UnitContext.Sanitary.ConvertPressureFromDisplay),

                QuantityKind.SanitaryDimension => Create(
                    kind,
                    UnitContext.Sanitary.GetDimensionUnitLabel(),
                    GetLengthDecimals(UnitContext.Sanitary.Dimension),
                    UnitContext.Sanitary.ConvertDimensionToDisplay,
                    UnitContext.Sanitary.ConvertDimensionFromDisplay),

                QuantityKind.SanitaryLength => Create(
                    kind,
                    UnitContext.Sanitary.GetLengthUnitLabel(),
                    GetLengthDecimals(UnitContext.Sanitary.Length),
                    UnitContext.Sanitary.ConvertLengthToDisplay,
                    UnitContext.Sanitary.ConvertLengthFromDisplay),

                QuantityKind.SanitaryPressureGradient => FixedUnit(
                    kind,
                    UnitContext.Sanitary.GetPressureGradientUnitLabel(),
                    1),

                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported quantity kind.")
            };
        }

        public static string GetUnitLabel(QuantityKind kind)
        {
            return GetInfo(kind).UnitLabel;
        }

        public static int GetDecimals(QuantityKind kind)
        {
            return GetInfo(kind).Decimals;
        }

        public static double ToDisplay(QuantityKind kind, double siValue)
        {
            return GetInfo(kind).ToDisplay(siValue);
        }

        public static double FromDisplay(QuantityKind kind, double displayValue)
        {
            return GetInfo(kind).FromDisplay(displayValue);
        }

        public static string FormatDisplay(QuantityKind kind, double siValue)
        {
            QuantityUnitInfo info = GetInfo(kind);
            double displayValue = info.ToDisplay(siValue);
            return displayValue.ToString($"F{info.Decimals}", CultureInfo.CurrentCulture);
        }

        public static string FormatDisplayWithUnit(QuantityKind kind, double siValue)
        {
            QuantityUnitInfo info = GetInfo(kind);
            string formattedValue = info.ToDisplay(siValue).ToString($"F{info.Decimals}", CultureInfo.CurrentCulture);
            return string.IsNullOrEmpty(info.UnitLabel)
                ? formattedValue
                : $"{formattedValue} {info.UnitLabel}";
        }

        private static QuantityUnitInfo Create(
            QuantityKind kind,
            string unitLabel,
            int decimals,
            Func<double, double> toDisplay,
            Func<double, double> fromDisplay)
        {
            return new QuantityUnitInfo(kind, unitLabel, decimals, toDisplay, fromDisplay);
        }

        private static QuantityUnitInfo FixedUnit(QuantityKind kind, string unitLabel, int decimals)
        {
            return Create(kind, unitLabel, decimals, Identity, Identity);
        }

        private static double Identity(double value)
        {
            return value;
        }

        private static double ConvertRoughnessToDisplay(double meterValue, RoughnessUnit unit)
        {
            return unit == RoughnessUnit.Millimeter ? meterValue * 1000.0 : meterValue;
        }

        private static double ConvertRoughnessFromDisplay(double displayValue, RoughnessUnit unit)
        {
            return unit == RoughnessUnit.Millimeter ? displayValue / 1000.0 : displayValue;
        }

        private static double ConvertRelativeHumidityToDisplay(double fractionValue, RelativeHumidityUnit unit)
        {
            return unit == RelativeHumidityUnit.Percent ? fractionValue * 100.0 : fractionValue;
        }

        private static double ConvertRelativeHumidityFromDisplay(double displayValue, RelativeHumidityUnit unit)
        {
            return unit == RelativeHumidityUnit.Percent ? displayValue / 100.0 : displayValue;
        }

        private static double ConvertHumidityRatioToDisplay(double kgPerKgValue, HumidityRatioUnit unit)
        {
            return unit == HumidityRatioUnit.GramPerKilogram ? kgPerKgValue * 1000.0 : kgPerKgValue;
        }

        private static double ConvertHumidityRatioFromDisplay(double displayValue, HumidityRatioUnit unit)
        {
            return unit == HumidityRatioUnit.GramPerKilogram ? displayValue / 1000.0 : displayValue;
        }

        private static double ConvertSpecificHeatToDisplay(double joulePerKilogramKelvinValue, SpecificHeatUnit unit)
        {
            return unit == SpecificHeatUnit.KilojoulePerKilogramKelvin
                ? joulePerKilogramKelvinValue / 1000.0
                : joulePerKilogramKelvinValue;
        }

        private static double ConvertSpecificHeatFromDisplay(double displayValue, SpecificHeatUnit unit)
        {
            return unit == SpecificHeatUnit.KilojoulePerKilogramKelvin
                ? displayValue * 1000.0
                : displayValue;
        }

        private static int GetAirFlowDecimals(AirFlowUnit unit)
        {
            return unit switch
            {
                AirFlowUnit.CubicMeterPerHour => 0,
                AirFlowUnit.LitersPerSecond => 1,
                _ => 4
            };
        }

        private static int GetAirPressureDecimals(AirPressureUnit unit)
        {
            return unit switch
            {
                AirPressureUnit.Pascal => 0,
                AirPressureUnit.Kilopascal => 2,
                AirPressureUnit.Millibar => 1,
                _ => 1
            };
        }

        private static int GetFluidFlowDecimals(FluidFlowUnit unit)
        {
            return unit switch
            {
                FluidFlowUnit.LitersPerSecond => 2,
                FluidFlowUnit.LitersPerHour => 0,
                FluidFlowUnit.KilogramPerHour => 0,
                _ => 3
            };
        }

        private static int GetFluidPressureDecimals(FluidPressureUnit unit)
        {
            return unit switch
            {
                FluidPressureUnit.Pascal => 0,
                FluidPressureUnit.Kilopascal => 2,
                FluidPressureUnit.Bar => 2,
                _ => 2
            };
        }

        private static int GetLengthDecimals(LengthUnit unit)
        {
            return unit switch
            {
                LengthUnit.Millimeter => 0,
                LengthUnit.Centimeter => 1,
                LengthUnit.Inch => 2,
                _ => 3
            };
        }

        private static int GetAreaDecimals(AreaUnit unit)
        {
            return unit switch
            {
                AreaUnit.SquareMillimeter => 0,
                AreaUnit.SquareCentimeter => 1,
                _ => 2
            };
        }

        private static int GetVolumeDecimals(VolumeUnit unit)
        {
            return unit == VolumeUnit.Liter ? 1 : 3;
        }

        private static int GetVelocityDecimals(VelocityUnit unit)
        {
            return unit == VelocityUnit.MeterPerMinute ? 0 : 2;
        }

        private static int GetPowerDecimals(PowerUnit unit)
        {
            return unit == PowerUnit.Kilowatt ? 2 : 0;
        }
    }
}
