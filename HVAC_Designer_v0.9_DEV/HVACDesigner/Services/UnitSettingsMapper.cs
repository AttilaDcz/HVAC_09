using System;

namespace HVACDesigner.Services
{
    public static class UnitSettingsMapper
    {
        public static UnitSettingsData CaptureFromUnitContext()
        {
            return new UnitSettingsData
            {
                Air = new AirUnitSettingsData
                {
                    Flow = UnitContext.Air.Flow,
                    Pressure = UnitContext.Air.Pressure,
                    Temperature = UnitContext.Air.Temperature,
                    Dimension = UnitContext.Air.Dimension,
                    Length = UnitContext.Air.Length,
                    Area = UnitContext.Air.Area,
                    Volume = UnitContext.Air.Volume,
                    Velocity = UnitContext.Air.Velocity,
                    Density = UnitContext.Air.Density,
                    Roughness = UnitContext.Air.Roughness,
                    PressureGradient = UnitContext.Air.PressureGradient,
                    AirChangeRate = UnitContext.Air.AirChangeRate,
                    RelativeHumidity = UnitContext.Air.RelativeHumidity,
                    HumidityRatio = UnitContext.Air.HumidityRatio,
                    DesignTemperatureCelsius = UnitContext.Air.DesignTemperatureCelsius
                },
                Hydraulics = new HydraulicsUnitSettingsData
                {
                    Flow = UnitContext.Hydraulics.Flow,
                    Pressure = UnitContext.Hydraulics.Pressure,
                    Temperature = UnitContext.Hydraulics.Temperature,
                    Dimension = UnitContext.Hydraulics.Dimension,
                    Length = UnitContext.Hydraulics.Length,
                    Velocity = UnitContext.Hydraulics.Velocity,
                    Power = UnitContext.Hydraulics.Power,
                    Density = UnitContext.Hydraulics.Density,
                    PressureGradient = UnitContext.Hydraulics.PressureGradient
                },
                Energetics = new EnergeticsUnitSettingsData
                {
                    Thickness = UnitContext.Energetics.Thickness,
                    Length = UnitContext.Energetics.Length,
                    Area = UnitContext.Energetics.Area,
                    Volume = UnitContext.Energetics.Volume,
                    Temperature = UnitContext.Energetics.Temperature,
                    HeatFlow = UnitContext.Energetics.HeatFlow,
                    ThermalConductivity = UnitContext.Energetics.ThermalConductivity,
                    UValue = UnitContext.Energetics.UValue,
                    Alpha = UnitContext.Energetics.Alpha,
                    ThermalResistance = UnitContext.Energetics.ThermalResistance,
                    SpecificHeat = UnitContext.Energetics.SpecificHeat,
                    Density = UnitContext.Energetics.Density
                },
                Sanitary = new SanitaryUnitSettingsData
                {
                    Flow = UnitContext.Sanitary.Flow,
                    Pressure = UnitContext.Sanitary.Pressure,
                    Dimension = UnitContext.Sanitary.Dimension,
                    Length = UnitContext.Sanitary.Length,
                    PressureGradient = UnitContext.Sanitary.PressureGradient
                },
                General = new GeneralUnitSettingsData
                {
                    Temperature = UnitContext.General.Temperature,
                    Length = UnitContext.General.Length,
                    Area = UnitContext.General.Area,
                    Volume = UnitContext.General.Volume,
                    Power = UnitContext.General.Power
                }
            };
        }

        public static void ApplyToUnitContext(UnitSettingsData? settings)
        {
            if (settings == null)
                return;

            settings.Normalize();

            UnitContext.Air.Flow = settings.Air.Flow;
            UnitContext.Air.Pressure = settings.Air.Pressure;
            UnitContext.Air.Temperature = settings.Air.Temperature;
            UnitContext.Air.Dimension = settings.Air.Dimension;
            UnitContext.Air.Length = settings.Air.Length;
            UnitContext.Air.Area = settings.Air.Area;
            UnitContext.Air.Volume = settings.Air.Volume;
            UnitContext.Air.Velocity = settings.Air.Velocity;
            UnitContext.Air.Density = settings.Air.Density;
            UnitContext.Air.Roughness = settings.Air.Roughness;
            UnitContext.Air.PressureGradient = settings.Air.PressureGradient;
            UnitContext.Air.AirChangeRate = settings.Air.AirChangeRate;
            UnitContext.Air.RelativeHumidity = settings.Air.RelativeHumidity;
            UnitContext.Air.HumidityRatio = settings.Air.HumidityRatio;
            UnitContext.Air.DesignTemperatureCelsius = Math.Max(
                -273.15,
                settings.Air.DesignTemperatureCelsius);

            UnitContext.Hydraulics.Flow = settings.Hydraulics.Flow;
            UnitContext.Hydraulics.Pressure = settings.Hydraulics.Pressure;
            UnitContext.Hydraulics.Temperature = settings.Hydraulics.Temperature;
            UnitContext.Hydraulics.Dimension = settings.Hydraulics.Dimension;
            UnitContext.Hydraulics.Length = settings.Hydraulics.Length;
            UnitContext.Hydraulics.Velocity = settings.Hydraulics.Velocity;
            UnitContext.Hydraulics.Power = settings.Hydraulics.Power;
            UnitContext.Hydraulics.Density = settings.Hydraulics.Density;
            UnitContext.Hydraulics.PressureGradient = settings.Hydraulics.PressureGradient;

            UnitContext.Energetics.Thickness = settings.Energetics.Thickness;
            UnitContext.Energetics.Length = settings.Energetics.Length;
            UnitContext.Energetics.Area = settings.Energetics.Area;
            UnitContext.Energetics.Volume = settings.Energetics.Volume;
            UnitContext.Energetics.Temperature = settings.Energetics.Temperature;
            UnitContext.Energetics.HeatFlow = settings.Energetics.HeatFlow;
            UnitContext.Energetics.ThermalConductivity = settings.Energetics.ThermalConductivity;
            UnitContext.Energetics.UValue = settings.Energetics.UValue;
            UnitContext.Energetics.Alpha = settings.Energetics.Alpha;
            UnitContext.Energetics.ThermalResistance = settings.Energetics.ThermalResistance;
            UnitContext.Energetics.SpecificHeat = settings.Energetics.SpecificHeat;
            UnitContext.Energetics.Density = settings.Energetics.Density;

            UnitContext.Sanitary.Flow = settings.Sanitary.Flow;
            UnitContext.Sanitary.Pressure = settings.Sanitary.Pressure;
            UnitContext.Sanitary.Dimension = settings.Sanitary.Dimension;
            UnitContext.Sanitary.Length = settings.Sanitary.Length;
            UnitContext.Sanitary.PressureGradient = settings.Sanitary.PressureGradient;

            UnitContext.General.Temperature = settings.General.Temperature;
            UnitContext.General.Length = settings.General.Length;
            UnitContext.General.Area = settings.General.Area;
            UnitContext.General.Volume = settings.General.Volume;
            UnitContext.General.Power = settings.General.Power;

            UnitContext.NotifyUnitChanged();
        }
    }
}
