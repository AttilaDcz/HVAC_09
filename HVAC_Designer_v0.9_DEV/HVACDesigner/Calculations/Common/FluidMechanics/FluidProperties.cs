using System;

namespace HVACDesigner.Calculations.Common.FluidMechanics
{
    public enum GlycolType
    {
        Ethylene,
        Propylene
    }

    /// <summary>
    /// Egy homogén, egyfázisú közeg áramlástani tulajdonságai SI-egységekben.
    /// Az objektum változtathatatlan.
    /// </summary>
    public sealed class FluidProperties
    {
        public string Name { get; }
        public double TemperatureCelsius { get; }
        public double Density { get; }                 // [kg/m3]
        public double DynamicViscosity { get; }        // [Pa*s]
        public double KinematicViscosity =>
            DynamicViscosity / Density;                // [m2/s]

        public FluidProperties(
            string name,
            double temperatureCelsius,
            double density,
            double dynamicViscosity)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "A közeg neve nem lehet üres.",
                    nameof(name));

            EnsureFinite(
                temperatureCelsius,
                nameof(temperatureCelsius));

            EnsurePositiveFinite(
                density,
                nameof(density));

            EnsurePositiveFinite(
                dynamicViscosity,
                nameof(dynamicViscosity));

            Name = name;
            TemperatureCelsius = temperatureCelsius;
            Density = density;
            DynamicViscosity = dynamicViscosity;
        }

        /// <summary>
        /// Száraz levegő közelítő tulajdonságai.
        /// A sűrűség ideális gázból, a viszkozitás Sutherland-közelítéssel készül.
        /// Alkalmazási tartomány: -30...80 °C.
        /// </summary>
        public static FluidProperties FromAir(
            double temperatureCelsius,
            double absolutePressurePa = 101325.0)
        {
            EnsureRange(
                temperatureCelsius,
                -30.0,
                80.0,
                nameof(temperatureCelsius));

            EnsurePositiveFinite(
                absolutePressurePa,
                nameof(absolutePressurePa));

            const double specificGasConstant = 287.058;
            const double referenceTemperatureK = 273.15;
            const double referenceViscosity = 1.716e-5;
            const double sutherlandConstantK = 111.0;

            double temperatureK =
                temperatureCelsius + 273.15;

            double density =
                absolutePressurePa /
                (specificGasConstant * temperatureK);

            double dynamicViscosity =
                referenceViscosity *
                Math.Pow(
                    temperatureK /
                    referenceTemperatureK,
                    1.5) *
                (referenceTemperatureK +
                 sutherlandConstantK) /
                (temperatureK +
                 sutherlandConstantK);

            return new FluidProperties(
                $"Száraz levegő {temperatureCelsius:0.#} °C",
                temperatureCelsius,
                Math.Round(density, 4),
                dynamicViscosity);
        }

        /// <summary>
        /// Víz közelítő tulajdonságai 0...100 °C között.
        /// </summary>
        public static FluidProperties FromWater(
            double temperatureCelsius)
        {
            EnsureRange(
                temperatureCelsius,
                0.0,
                100.0,
                nameof(temperatureCelsius));

            double t = temperatureCelsius;

            // Mérnöki közelítés a folyékony víz sűrűségére.
            double density =
                1000.0 *
                (1.0 -
                 ((t + 288.9414) /
                  (508929.2 * (t + 68.12963))) *
                 Math.Pow(t - 3.9863, 2.0));

            // Mérnöki közelítés a víz dinamikai viszkozitására.
            double dynamicViscosity =
                2.414e-5 *
                Math.Pow(
                    10.0,
                    247.8 / (t + 133.15));

            return new FluidProperties(
                $"Víz {temperatureCelsius:0.#} °C",
                temperatureCelsius,
                Math.Round(density, 1),
                dynamicViscosity);
        }

        /// <summary>
        /// 10...30 tömeg%-os etilén- vagy propilénglikolos víz
        /// közelítő tulajdonságai -10...80 °C között.
        ///
        /// A táblák épületgépészeti előméretezésre alkalmas közelítések.
        /// Gyártói végleges méretezésnél a konkrét hőhordozó adatlapja az irányadó.
        /// </summary>
        public static FluidProperties FromGlycolWater(
            GlycolType glycolType,
            double concentrationPercent,
            double temperatureCelsius)
        {
            EnsureRange(
                concentrationPercent,
                10.0,
                30.0,
                nameof(concentrationPercent));

            EnsureRange(
                temperatureCelsius,
                -10.0,
                80.0,
                nameof(temperatureCelsius));

            double density = InterpolateGlycolProperty(
                glycolType,
                concentrationPercent,
                temperatureCelsius,
                isDensity: true);

            double dynamicViscosityMilliPascalSecond =
                InterpolateGlycolProperty(
                    glycolType,
                    concentrationPercent,
                    temperatureCelsius,
                    isDensity: false);

            string glycolName =
                glycolType == GlycolType.Ethylene
                    ? "Etilénglikol"
                    : "Propilénglikol";

            return new FluidProperties(
                $"{glycolName} {concentrationPercent:0.#}% / {temperatureCelsius:0.#} °C",
                temperatureCelsius,
                Math.Round(density, 1),
                dynamicViscosityMilliPascalSecond / 1000.0);
        }

        public static FluidProperties AirAt20C =>
            FromAir(20.0);

        public static FluidProperties WaterAt20C =>
            FromWater(20.0);

        private static double InterpolateGlycolProperty(
            GlycolType glycolType,
            double concentrationPercent,
            double temperatureCelsius,
            bool isDensity)
        {
            double[] concentrations = { 10.0, 20.0, 30.0 };
            double[] temperatures =
            {
                -10.0, 0.0, 10.0, 20.0,
                40.0, 60.0, 80.0
            };

            // Sűrűség [kg/m3].
            double[,] ethyleneDensity =
            {
                { 1028.0, 1045.0, 1061.0 },
                { 1022.0, 1039.0, 1055.0 },
                { 1016.0, 1033.0, 1049.0 },
                { 1010.0, 1027.0, 1043.0 },
                {  998.0, 1015.0, 1031.0 },
                {  986.0, 1003.0, 1019.0 },
                {  974.0,  991.0, 1007.0 }
            };

            double[,] propyleneDensity =
            {
                { 1022.0, 1034.0, 1046.0 },
                { 1017.0, 1029.0, 1041.0 },
                { 1012.0, 1024.0, 1036.0 },
                { 1007.0, 1019.0, 1031.0 },
                {  995.0, 1007.0, 1019.0 },
                {  983.0,  995.0, 1007.0 },
                {  971.0,  983.0,  995.0 }
            };

            // Dinamikai viszkozitás [mPa*s].
            double[,] ethyleneViscosity =
            {
                { 3.20, 5.20, 8.20 },
                { 2.30, 3.60, 5.40 },
                { 1.70, 2.50, 3.60 },
                { 1.30, 1.80, 2.50 },
                { 0.82, 1.05, 1.35 },
                { 0.56, 0.69, 0.85 },
                { 0.41, 0.49, 0.59 }
            };

            double[,] propyleneViscosity =
            {
                { 4.00, 7.00, 12.0 },
                { 2.80, 4.70, 7.70 },
                { 2.00, 3.20, 5.00 },
                { 1.50, 2.30, 3.50 },
                { 0.92, 1.30, 1.85 },
                { 0.62, 0.82, 1.10 },
                { 0.45, 0.57, 0.73 }
            };

            double[,] table;

            if (glycolType == GlycolType.Ethylene)
            {
                table = isDensity
                    ? ethyleneDensity
                    : ethyleneViscosity;
            }
            else
            {
                table = isDensity
                    ? propyleneDensity
                    : propyleneViscosity;
            }

            int lowerTemperatureIndex =
                FindLowerIndex(
                    temperatures,
                    temperatureCelsius);

            int upperTemperatureIndex =
                Math.Min(
                    lowerTemperatureIndex + 1,
                    temperatures.Length - 1);

            int lowerConcentrationIndex =
                FindLowerIndex(
                    concentrations,
                    concentrationPercent);

            int upperConcentrationIndex =
                Math.Min(
                    lowerConcentrationIndex + 1,
                    concentrations.Length - 1);

            double t0 =
                temperatures[lowerTemperatureIndex];

            double t1 =
                temperatures[upperTemperatureIndex];

            double c0 =
                concentrations[lowerConcentrationIndex];

            double c1 =
                concentrations[upperConcentrationIndex];

            double q00 =
                table[
                    lowerTemperatureIndex,
                    lowerConcentrationIndex];

            double q01 =
                table[
                    lowerTemperatureIndex,
                    upperConcentrationIndex];

            double q10 =
                table[
                    upperTemperatureIndex,
                    lowerConcentrationIndex];

            double q11 =
                table[
                    upperTemperatureIndex,
                    upperConcentrationIndex];

            double concentrationFraction =
                c1 == c0
                    ? 0.0
                    : (concentrationPercent - c0) /
                      (c1 - c0);

            double lowerTemperatureValue =
                LinearInterpolate(
                    q00,
                    q01,
                    concentrationFraction);

            double upperTemperatureValue =
                LinearInterpolate(
                    q10,
                    q11,
                    concentrationFraction);

            double temperatureFraction =
                t1 == t0
                    ? 0.0
                    : (temperatureCelsius - t0) /
                      (t1 - t0);

            return LinearInterpolate(
                lowerTemperatureValue,
                upperTemperatureValue,
                temperatureFraction);
        }

        private static int FindLowerIndex(
            double[] values,
            double value)
        {
            for (int index = 0;
                 index < values.Length - 1;
                 index++)
            {
                if (value >= values[index] &&
                    value <= values[index + 1])
                {
                    return index;
                }
            }

            return values.Length - 1;
        }

        private static double LinearInterpolate(
            double start,
            double end,
            double fraction)
        {
            return start +
                   (end - start) *
                   fraction;
        }

        private static void EnsureRange(
            double value,
            double minimum,
            double maximum,
            string parameterName)
        {
            EnsureFinite(value, parameterName);

            if (value < minimum || value > maximum)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    $"Az érték megengedett tartománya: {minimum}...{maximum}.");
            }
        }

        private static void EnsurePositiveFinite(
            double value,
            string parameterName)
        {
            if (value <= 0.0 ||
                double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Az értéknek pozitív és véges számnak kell lennie.");
            }
        }

        private static void EnsureFinite(
            double value,
            string parameterName)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Az értéknek véges számnak kell lennie.");
            }
        }
    }
}
