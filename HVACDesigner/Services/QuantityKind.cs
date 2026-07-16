namespace HVACDesigner.Services
{
    public enum QuantityKind
    {
        None,

        Length,
        Area,
        Volume,
        Temperature,
        Power,

        AirFlow,
        AirPressure,
        AirVelocity,
        AirDimension,
        AirLength,
        AirArea,
        AirVolume,
        AirDensity,
        AirRoughness,
        AirPressureGradient,
        AirChangeRate,
        RelativeHumidity,
        HumidityRatio,

        HydraulicFlow,
        HydraulicPressure,
        HydraulicVelocity,
        HydraulicDimension,
        HydraulicLength,
        HydraulicPower,
        HydraulicDensity,
        HydraulicPressureGradient,

        Thickness,
        ThermalConductivity,
        HeatTransferCoefficient,
        UValue,
        Alpha,
        ThermalResistance,
        SpecificHeat,
        Density,
        HeatFlow,
        EnergeticLength,
        EnergeticArea,
        EnergeticVolume,

        SanitaryFlow,
        SanitaryPressure,
        SanitaryDimension,
        SanitaryLength,
        SanitaryPressureGradient
    }
}
