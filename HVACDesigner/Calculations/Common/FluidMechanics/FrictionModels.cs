using System;

namespace HVACDesigner.Calculations.Common.FluidMechanics
{
    /// <summary>
    /// Darcy-féle súrlódási tényező belső modelljei.
    /// </summary>
    internal static class FrictionModels
    {
        public const double LaminarUpperReynolds = 2300.0;
        public const double TurbulentLowerReynolds = 4000.0;

        private const int ColebrookMaximumIterations = 50;
        private const double ColebrookTolerance = 1e-10;

        public static FrictionCalculationResult Calculate(
            double reynoldsNumber,
            double relativeRoughness,
            FrictionFactorModel requestedModel)
        {
            ValidateInputs(
                reynoldsNumber,
                relativeRoughness);

            if (reynoldsNumber == 0.0)
            {
                return new FrictionCalculationResult(
                    0.0,
                    FlowRegime.NoFlow,
                    requestedModel,
                    0,
                    true);
            }

            FlowRegime regime =
                DetermineFlowRegime(reynoldsNumber);

            if (requestedModel ==
                FrictionFactorModel.AutomaticEngineering)
            {
                return CalculateAutomatic(
                    reynoldsNumber,
                    relativeRoughness,
                    regime);
            }

            if (regime == FlowRegime.Laminar)
            {
                return new FrictionCalculationResult(
                    64.0 / reynoldsNumber,
                    FlowRegime.Laminar,
                    requestedModel,
                    0,
                    true);
            }

            switch (requestedModel)
            {
                case FrictionFactorModel.ColebrookWhite:
                    return CalculateColebrookWhiteResult(
                        reynoldsNumber,
                        relativeRoughness,
                        regime);

                case FrictionFactorModel.SwameeJain:
                    return new FrictionCalculationResult(
                        CalculateSwameeJain(
                            reynoldsNumber,
                            relativeRoughness),
                        regime,
                        FrictionFactorModel.SwameeJain,
                        0,
                        true);

                case FrictionFactorModel.Haaland:
                    return new FrictionCalculationResult(
                        CalculateHaaland(
                            reynoldsNumber,
                            relativeRoughness),
                        regime,
                        FrictionFactorModel.Haaland,
                        0,
                        true);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(requestedModel),
                        requestedModel,
                        "Ismeretlen súrlódási modell.");
            }
        }

        private static FrictionCalculationResult CalculateAutomatic(
            double reynoldsNumber,
            double relativeRoughness,
            FlowRegime regime)
        {
            if (regime == FlowRegime.Laminar)
            {
                return new FrictionCalculationResult(
                    64.0 / reynoldsNumber,
                    FlowRegime.Laminar,
                    FrictionFactorModel.AutomaticEngineering,
                    0,
                    true);
            }

            if (regime == FlowRegime.Turbulent)
            {
                FrictionCalculationResult turbulent =
                    CalculateColebrookWhiteResult(
                        reynoldsNumber,
                        relativeRoughness,
                        FlowRegime.Turbulent);

                return new FrictionCalculationResult(
                    turbulent.FrictionFactor,
                    FlowRegime.Turbulent,
                    FrictionFactorModel.ColebrookWhite,
                    turbulent.IterationCount,
                    turbulent.Converged);
            }

            // Az átmeneti tartomány eredménye kísérletileg bizonytalan.
            // A program a 2300-as lamináris és a 4000-es Colebrook-White
            // végpont között simított mérnöki átmenetet ad, és ezt
            // diagnosztikailag Transitional állapotként jelöli.
            double laminarEndpoint =
                64.0 / LaminarUpperReynolds;

            FrictionCalculationResult turbulentEndpoint =
                CalculateColebrookWhiteResult(
                    TurbulentLowerReynolds,
                    relativeRoughness,
                    FlowRegime.Turbulent);

            double fraction =
                (reynoldsNumber -
                 LaminarUpperReynolds) /
                (TurbulentLowerReynolds -
                 LaminarUpperReynolds);

            // Smoothstep: folytonos, sima átmenet a két végpont között.
            double smoothFraction =
                fraction * fraction *
                (3.0 - 2.0 * fraction);

            double frictionFactor =
                laminarEndpoint +
                (turbulentEndpoint.FrictionFactor -
                 laminarEndpoint) *
                smoothFraction;

            return new FrictionCalculationResult(
                frictionFactor,
                FlowRegime.Transitional,
                FrictionFactorModel.AutomaticEngineering,
                turbulentEndpoint.IterationCount,
                turbulentEndpoint.Converged);
        }

        private static FrictionCalculationResult
            CalculateColebrookWhiteResult(
                double reynoldsNumber,
                double relativeRoughness,
                FlowRegime regime)
        {
            // Haaland jó és stabil kezdőérték.
            double frictionFactor =
                CalculateHaaland(
                    reynoldsNumber,
                    relativeRoughness);

            for (int iteration = 1;
                 iteration <= ColebrookMaximumIterations;
                 iteration++)
            {
                double denominator =
                    relativeRoughness / 3.7 +
                    2.51 /
                    (reynoldsNumber *
                     Math.Sqrt(frictionFactor));

                double inverseSquareRoot =
                    -2.0 *
                    Math.Log10(denominator);

                double nextFrictionFactor =
                    1.0 /
                    (inverseSquareRoot *
                     inverseSquareRoot);

                if (Math.Abs(
                        nextFrictionFactor -
                        frictionFactor) <=
                    ColebrookTolerance)
                {
                    return new FrictionCalculationResult(
                        nextFrictionFactor,
                        regime,
                        FrictionFactorModel.ColebrookWhite,
                        iteration,
                        true);
                }

                frictionFactor =
                    nextFrictionFactor;
            }

            throw new InvalidOperationException(
                "A Colebrook-White iteráció nem konvergált " +
                $"{ColebrookMaximumIterations} iteráción belül.");
        }

        private static double CalculateSwameeJain(
            double reynoldsNumber,
            double relativeRoughness)
        {
            double argument =
                relativeRoughness / 3.7 +
                5.74 /
                Math.Pow(reynoldsNumber, 0.9);

            return 0.25 /
                   Math.Pow(
                       Math.Log10(argument),
                       2.0);
        }

        private static double CalculateHaaland(
            double reynoldsNumber,
            double relativeRoughness)
        {
            double inverseSquareRoot =
                -1.8 *
                Math.Log10(
                    Math.Pow(
                        relativeRoughness / 3.7,
                        1.11) +
                    6.9 / reynoldsNumber);

            return 1.0 /
                   (inverseSquareRoot *
                    inverseSquareRoot);
        }

        private static FlowRegime DetermineFlowRegime(
            double reynoldsNumber)
        {
            if (reynoldsNumber <
                LaminarUpperReynolds)
            {
                return FlowRegime.Laminar;
            }

            if (reynoldsNumber <
                TurbulentLowerReynolds)
            {
                return FlowRegime.Transitional;
            }

            return FlowRegime.Turbulent;
        }

        private static void ValidateInputs(
            double reynoldsNumber,
            double relativeRoughness)
        {
            if (reynoldsNumber < 0.0 ||
                double.IsNaN(reynoldsNumber) ||
                double.IsInfinity(reynoldsNumber))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reynoldsNumber),
                    "A Reynolds-szám nem lehet negatív vagy nem véges.");
            }

            if (relativeRoughness < 0.0 ||
                double.IsNaN(relativeRoughness) ||
                double.IsInfinity(relativeRoughness))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(relativeRoughness),
                    "A relatív érdesség nem lehet negatív vagy nem véges.");
            }
        }
    }
}
