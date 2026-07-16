using System;
using System.Collections.Generic;

namespace HVACDesigner.Calculations.Thermal.CoolingLoad
{
    /// <summary>
    /// ISO 52016-1:2017 szerinti óránkénti hűtési energiaigény-számítás interfésze.
    ///
    /// Ez az interfész Fázis 2-ben elkészül, de teljes implementációja
    /// (HourlyCoolingLoadCalculator osztály) egy következő fejlesztési fázisban kerül
    /// megírásra.
    ///
    /// Az egyszerűsített csúcsterhelés-számítás a SimplePeakCoolingLoadCalculator-ban
    /// van, amelyet Fázis 2 már tartalmaz.
    ///
    /// Az RC-modell alapelvei: ISO 52016-1:2017 §6.4–§6.6 (5R1C modell).
    /// </summary>
    public interface IHourlyCoolingLoadCalculator
    {
        /// <summary>
        /// ISO 52016-1 alapú óránkénti hűtési és fűtési energiaigény számítása.
        /// </summary>
        /// <param name="zoneInput">RC-hálózati zónamodell bemenete</param>
        /// <param name="hourlyClimate">8760 óra klímaadatsor</param>
        /// <returns>Óránkénti hűtési teljesítmény és energiaösszesítők</returns>
        HourlyCoolingResult Calculate(
            ZoneRcModelInput zoneInput,
            IReadOnlyList<HourlyClimateData> hourlyClimate);
    }

    /// <summary>
    /// Helyőrző implementáció az IHourlyCoolingLoadCalculator interfészhez.
    /// A teljes ISO 52016-1 szimulációs logika egy következő fejlesztési fázisban
    /// kerül megvalósításra.
    ///
    /// Addig ez az osztály NotImplementedException-t dob egy informatív üzenettel.
    /// </summary>
    public sealed class HourlyCoolingLoadCalculatorPlaceholder
        : IHourlyCoolingLoadCalculator
    {
        /// <inheritdoc />
        /// <exception cref="NotImplementedException">
        /// Mindig, mivel az ISO 52016-1 szimulációs logika még nem implementált.
        /// </exception>
        public HourlyCoolingResult Calculate(
            ZoneRcModelInput zoneInput,
            IReadOnlyList<HourlyClimateData> hourlyClimate)
        {
            if (zoneInput == null)
                throw new ArgumentNullException(nameof(zoneInput));
            if (hourlyClimate == null)
                throw new ArgumentNullException(nameof(hourlyClimate));

            throw new NotImplementedException(
                "Az ISO 52016-1 szerinti óránkénti hűtési energiaigény-számítás " +
                "(IHourlyCoolingLoadCalculator) egy jövőbeli fejlesztési fázisban " +
                "érhető el. Az egyszerűsített csúcsterhelés-számításhoz használja a " +
                "SimplePeakCoolingLoadCalculator.Calculate() metódust.");
        }
    }
}
