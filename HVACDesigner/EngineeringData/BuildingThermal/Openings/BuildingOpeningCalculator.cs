using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HVACDesigner.EngineeringData.BuildingThermal.Openings
{
    public sealed class BuildingOpeningCalculationInput
    {
        public double Width { get; set; }        // [m]
        public double Height { get; set; }       // [m]
        public double FrameVisibleWidth { get; set; } // [m]

        public GlazingDefinition Glazing { get; set; }
        public FrameDefinition Frame { get; set; }
        public SpacerDefinition Spacer { get; set; }

        /// <summary>
        /// Opcionális tömör panel felülete [m2].
        /// </summary>
        public double OpaquePanelArea { get; set; }

        /// <summary>
        /// Opcionális tömör panel U-értéke [W/(m2*K)].
        /// </summary>
        public double? OpaquePanelUValue { get; set; }
    }

    public sealed class BuildingOpeningCalculationResult
    {
        private readonly ReadOnlyCollection<string> _diagnostics;

        public double TotalArea { get; }
        public double GlazingArea { get; }
        public double FrameArea { get; }
        public double OpaquePanelArea { get; }
        public double GlazingEdgeLength { get; }

        public double UValue { get; }
        public double EffectiveGValue { get; }

        public IReadOnlyList<string> Diagnostics => _diagnostics;

        public BuildingOpeningCalculationResult(
            double totalArea,
            double glazingArea,
            double frameArea,
            double opaquePanelArea,
            double glazingEdgeLength,
            double uValue,
            double effectiveGValue,
            IEnumerable<string> diagnostics)
        {
            TotalArea = totalArea;
            GlazingArea = glazingArea;
            FrameArea = frameArea;
            OpaquePanelArea = opaquePanelArea;
            GlazingEdgeLength = glazingEdgeLength;
            UValue = uValue;
            EffectiveGValue = effectiveGValue;
            _diagnostics = new ReadOnlyCollection<string>(
                new List<string>(
                    diagnostics ??
                    Array.Empty<string>()));
        }
    }

    /// <summary>
    /// Egyszerű, gyakorlati komponensalapú nyílászáró-számítás.
    /// Nem végez keretprofil-FEM számítást.
    /// </summary>
    public sealed class BuildingOpeningCalculator
    {
        public BuildingOpeningCalculationResult Calculate(
            BuildingOpeningCalculationInput input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            EnsurePositiveFinite(input.Width, nameof(input.Width));
            EnsurePositiveFinite(input.Height, nameof(input.Height));
            EnsurePositiveFinite(
                input.FrameVisibleWidth,
                nameof(input.FrameVisibleWidth));

            if (input.Glazing == null)
                throw new ArgumentNullException(nameof(input.Glazing));

            if (input.Frame == null)
                throw new ArgumentNullException(nameof(input.Frame));

            if (input.Spacer == null)
                throw new ArgumentNullException(nameof(input.Spacer));

            if (input.FrameVisibleWidth * 2.0 >= input.Width ||
                input.FrameVisibleWidth * 2.0 >= input.Height)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(input.FrameVisibleWidth),
                    "A keret látható szélessége túl nagy a nyílászáró méretéhez.");
            }

            if (input.OpaquePanelArea < 0.0 ||
                double.IsNaN(input.OpaquePanelArea) ||
                double.IsInfinity(input.OpaquePanelArea))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(input.OpaquePanelArea));
            }

            if (input.OpaquePanelArea > 0.0 &&
                !input.OpaquePanelUValue.HasValue)
            {
                throw new ArgumentException(
                    "Tömör panelhez U-értéket is meg kell adni.",
                    nameof(input.OpaquePanelUValue));
            }

            if (input.OpaquePanelUValue.HasValue)
                EnsurePositiveFinite(
                    input.OpaquePanelUValue.Value,
                    nameof(input.OpaquePanelUValue));

            double totalArea =
                input.Width * input.Height;

            double clearWidth =
                input.Width -
                2.0 * input.FrameVisibleWidth;

            double clearHeight =
                input.Height -
                2.0 * input.FrameVisibleWidth;

            double clearArea =
                clearWidth * clearHeight;

            if (input.OpaquePanelArea > clearArea)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(input.OpaquePanelArea),
                    "A tömör panel felülete nagyobb a kereten belüli szabad felületnél.");
            }

            double glazingArea =
                clearArea -
                input.OpaquePanelArea;

            double frameArea =
                totalArea -
                clearArea;

            double glazingEdgeLength =
                glazingArea > 0.0
                    ? 2.0 * (clearWidth + clearHeight)
                    : 0.0;

            double numerator =
                glazingArea * input.Glazing.Ug +
                frameArea * input.Frame.Uf +
                glazingEdgeLength *
                input.Spacer.LinearThermalTransmittance;

            if (input.OpaquePanelArea > 0.0)
            {
                numerator +=
                    input.OpaquePanelArea *
                    input.OpaquePanelUValue.Value;
            }

            double uValue =
                numerator / totalArea;

            double effectiveGValue =
                totalArea > 0.0
                    ? input.Glazing.SolarEnergyTransmittance *
                      glazingArea /
                      totalArea
                    : 0.0;

            var diagnostics = new List<string>();

            if (effectiveGValue <= 0.0)
            {
                diagnostics.Add(
                    "A nyílászáró nem rendelkezik effektív szoláris átbocsátással.");
            }

            if (input.OpaquePanelArea > 0.0)
            {
                diagnostics.Add(
                    "Az effektív g-érték csak az üvegezett felületet veszi figyelembe.");
            }

            return new BuildingOpeningCalculationResult(
                totalArea,
                glazingArea,
                frameArea,
                input.OpaquePanelArea,
                glazingEdgeLength,
                uValue,
                effectiveGValue,
                diagnostics);
        }

        private static void EnsurePositiveFinite(
            double value,
            string parameterName)
        {
            if (value <= 0.0 ||
                double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
