using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HVACDesigner.Calculations.Thermal.Common;

namespace HVACDesigner.Calculations.Thermal.CoolingLoad
{
    /// <summary>
    /// Egyszerűsített nyári hűtési csúcsterhelés kalkulátor.
    /// Kvázi-stacionárius módszer (CIBSE Guide A / WinWatt-szerű).
    ///
    /// Algoritmus:
    ///   1. Napsugárzáson alapuló hőnyereség üveges felületenként:
    ///      Q_solar,i = A_i × G_solar(tájolás) × g × shadingFactor
    ///
    ///   2. Transzmissziós hőnyereség tömör szerkezeteken:
    ///      Q_trans = U × A × (T_ext - T_int)
    ///      (hőnyereség, ha T_ext > T_int, azaz pozitív érték)
    ///
    ///   3. Személyek érzékelhető hőleadása:
    ///      Q_people,s = n × SensibleHeatPerPerson × concurrencyFactor
    ///
    ///   4. Személyek látens hőleadása:
    ///      Q_people,l = n × LatentHeatPerPerson × concurrencyFactor
    ///
    ///   5. Megvilágítás hőleadása:
    ///      Q_lighting = LightingW
    ///
    ///   6. Berendezések hőleadása:
    ///      Q_equipment = EquipmentW
    ///
    ///   7. Szellőzési hőnyereség:
    ///      Q_vent = ρcp × n_vent × V × max(0, T_ext - T_int)
    ///
    ///   8. Összes érzékelhető terhelés:
    ///      Q_sens = Q_solar,Σ + Q_trans + Q_people,s + Q_lighting + Q_equipment + Q_vent
    ///
    ///   9. Teljes csúcsterhelés:
    ///      Q_total = Q_sens + Q_people,l
    ///
    /// Megjegyzés: Ez a módszer nem veszi figyelembe a szerkezetek
    /// hőtároló hatását (hőtehetetlenség). Az ISO 52016-1 RC-modell
    /// pontosabb eredményt ad – az IHourlyCoolingLoadCalculator hook-ként
    /// előkészített.
    /// </summary>
    public sealed class SimplePeakCoolingLoadCalculator
    {
        private readonly ThermalCalculationContext _context;

        public SimplePeakCoolingLoadCalculator(ThermalCalculationContext context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Egy helyiség nyári hűtési csúcsterhelésének számítása.
        /// </summary>
        public RoomCoolingResult Calculate(RoomCoolingInput input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var climate = _context.RequireClimateRegion(input.ClimateRegionId);
            var coolingRules = _context.CoolingRules;

            double tExt = climate.CoolingOutdoorDryBulbC;
            double tInt = input.IndoorTemperatureC;
            double deltaT = tExt - tInt;   // pozitív = hőnyereség (nyár)

            // 1+2. Üveges és tömör felületek hőnyeresége
            double solarGainW = 0.0;
            double transmissionGainFromGlazingW = 0.0;

            foreach (var glazing in input.GlazedElements)
            {
                double solarIrradiance =
                    climate.GetSolarIrradiance(glazing.Orientation);

                double shadingFactor =
                    coolingRules.GetShadingFactor(glazing.Shading);

                // Napsugárzáson alapuló hőnyereség
                solarGainW +=
                    glazing.AreaM2
                    * solarIrradiance
                    * glazing.SolarHeatGainCoefficient
                    * shadingFactor;

                // Transzmissziós hőnyereség az üvegen
                if (deltaT > 0.0)
                {
                    transmissionGainFromGlazingW +=
                        glazing.UValue * glazing.AreaM2 * deltaT;
                }
            }

            // Tömör szerkezetek transzmissziós hőnyeresége
            double transmissionGainFromOpaqueW = 0.0;
            if (deltaT > 0.0)
            {
                foreach (var opaque in input.OpaqueElements)
                {
                    transmissionGainFromOpaqueW +=
                        opaque.UValue * opaque.AreaM2 * deltaT;
                }
            }

            double totalTransmissionW =
                transmissionGainFromGlazingW + transmissionGainFromOpaqueW;

            // 3+4. Személyek hőleadása
            double effectiveOccupants =
                input.OccupancyPersons * input.OccupancyConcurrencyFactor;

            double peopleSensibleW =
                effectiveOccupants
                * coolingRules.SensibleHeatPerPersonSedentary;

            double peopleLatentW =
                effectiveOccupants
                * coolingRules.LatentHeatPerPersonSedentary;

            // 5. Megvilágítás
            double lightingGainW = input.LightingW;

            // 6. Berendezések
            double equipmentGainW = input.EquipmentW;

            // 7. Szellőzési hőnyereség (csak ha T_ext > T_int)
            double ventilationGainW = 0.0;
            if (deltaT > 0.0)
            {
                ventilationGainW =
                    coolingRules.AirVolumetricHeatCapacity
                    * input.VentilationAch
                    * input.AirVolumeM3
                    * deltaT;
            }

            return new RoomCoolingResult(
                input.RoomId,
                input.DisplayName,
                solarGainW,
                totalTransmissionW,
                peopleSensibleW,
                peopleLatentW,
                lightingGainW,
                equipmentGainW,
                ventilationGainW);
        }

        /// <summary>
        /// Épület összes helyiségének hűtési csúcsterhelése.
        /// </summary>
        public BuildingCoolingResult CalculateBuilding(
            IEnumerable<RoomCoolingInput> rooms,
            string climateRegionId)
        {
            if (rooms == null)
                throw new ArgumentNullException(nameof(rooms));

            var results = new List<RoomCoolingResult>();
            foreach (var room in rooms)
                results.Add(Calculate(room));

            return new BuildingCoolingResult(
                new ReadOnlyCollection<RoomCoolingResult>(results),
                climateRegionId);
        }
    }
}
