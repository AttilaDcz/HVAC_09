using System;

namespace HVACDesigner.Services
{
    public sealed class QuantityUnitInfo
    {
        private readonly Func<double, double> toDisplay;
        private readonly Func<double, double> fromDisplay;

        public QuantityUnitInfo(
            QuantityKind kind,
            string unitLabel,
            int decimals,
            Func<double, double> toDisplay,
            Func<double, double> fromDisplay)
        {
            Kind = kind;
            UnitLabel = unitLabel ?? string.Empty;
            Decimals = Math.Max(0, decimals);
            this.toDisplay = toDisplay ?? throw new ArgumentNullException(nameof(toDisplay));
            this.fromDisplay = fromDisplay ?? throw new ArgumentNullException(nameof(fromDisplay));
        }

        public QuantityKind Kind { get; }
        public string UnitLabel { get; }
        public int Decimals { get; }

        public double ToDisplay(double siValue)
        {
            return toDisplay(siValue);
        }

        public double FromDisplay(double displayValue)
        {
            return fromDisplay(displayValue);
        }
    }
}
