using System;

namespace HVACDesigner.Calculations.Air
{
    public static class AirCalculations
    {
        #region Szabványos méretek

        // Egyelőre csak néhány méret, később bővíthető
        private static readonly int[] StandardCircular =
        {
            80,
            100,
            125,
            160,
            200,
            250,
            315,
            350,
            400,
            450,
            500,
            600,
            815
        };

        // Téglalap légcsatorna méretlépcső
        private const int RectangularStep = 50;

        #endregion

        #region Légsebesség

        /// <summary>
        /// Kör keresztmetszetű légcsatorna légsebessége.
        /// </summary>
        public static double CalculateVelocityCircular(
            double diameterMm,
            double flowM3h)
        {
            double diameterM = diameterMm / 1000.0;
            double flowM3s = flowM3h / 3600.0;

            double area = Math.PI * Math.Pow(diameterM, 2) / 4.0;

            return flowM3s / area;
        }

        /// <summary>
        /// Téglalap keresztmetszetű légcsatorna légsebessége.
        /// </summary>
        public static double CalculateVelocityRectangular(
            double widthMm,
            double heightMm,
            double flowM3h)
        {
            double widthM = widthMm / 1000.0;
            double heightM = heightMm / 1000.0;
            double flowM3s = flowM3h / 3600.0;

            double area = widthM * heightM;

            return flowM3s / area;
        }

        #endregion

        #region Légcsatorna méretezés

        /// <summary>
        /// Szükséges kör légcsatorna átmérője.
        /// </summary>
        public static double CalculateCircularDiameter(
            double flowM3h,
            double velocity)
        {
            double flowM3s = flowM3h / 3600.0;

            double area = flowM3s / velocity;

            return Math.Sqrt((4.0 * area) / Math.PI) * 1000.0;
        }

        /// <summary>
        /// Szükséges szélesség adott magasság esetén.
        /// </summary>
        public static double CalculateRectangularWidth(
            double flowM3h,
            double velocity,
            double heightMm)
        {
            double flowM3s = flowM3h / 3600.0;

            double area = flowM3s / velocity;

            double heightM = heightMm / 1000.0;

            return area / heightM * 1000.0;
        }

        /// <summary>
        /// Szükséges magasság adott szélesség esetén.
        /// </summary>
        public static double CalculateRectangularHeight(
            double flowM3h,
            double velocity,
            double widthMm)
        {
            double flowM3s = flowM3h / 3600.0;

            double area = flowM3s / velocity;

            double widthM = widthMm / 1000.0;

            return area / widthM * 1000.0;
        }

        #endregion

        #region Kerekítés

        /// <summary>
        /// Kör légcsatorna szabványos méretre kerekítése.
        /// </summary>
        public static int RoundCircular(
            double value,
            bool roundUp,
            bool roundDown)
        {
            // nincs kerekítés
            if (!roundUp && !roundDown)
                return (int)Math.Round(value);

            // lefelé
            if (roundDown)
            {
                int selected = StandardCircular[0];

                foreach (int d in StandardCircular)
                {
                    if (d <= value)
                        selected = d;
                }

                return selected;
            }

            // felfelé
            foreach (int d in StandardCircular)
            {
                if (d >= value)
                    return d;
            }

            // ha nagyobb mint az utolsó szabványos méret
            return StandardCircular[^1];
        }

        /// <summary>
        /// Téglalap légcsatorna 50 mm-es méretlépcsőre kerekítése.
        /// </summary>
        public static int RoundRectangular(
            double value,
            bool roundUp,
            bool roundDown)
        {
            if (!roundUp && !roundDown)
                return (int)Math.Round(value);

            if (roundUp)
                return (int)(Math.Ceiling(value / RectangularStep) * RectangularStep);

            return (int)(Math.Floor(value / RectangularStep) * RectangularStep);
        }

        #endregion
    }
}