using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HVACDesigner.EngineeringData.Common;

namespace HVACDesigner.EngineeringData.BuildingThermal.Openings
{
    public enum BuildingOpeningType
    {
        Window,
        RoofWindow,
        GlazedDoor,
        OpaqueDoor,
        MixedDoor,
        BalconyDoor,
        SlidingDoor,
        GarageDoor,
        IndustrialDoor,
        CurtainWallPanel,
        CustomOpening
    }

    public enum BuildingOpeningCalculationMode
    {
        DirectValue,
        ComponentCalculated,
        ManufacturerValue
    }

    public enum FrameMaterialKind
    {
        Unspecified,
        Wood,
        Pvc,
        Aluminium,
        Steel,
        Composite,
        Other
    }

    public sealed class GlazingDefinition
    {
        public EngineeringDataHeader Header { get; }

        public string Id => Header.Id;
        public string Name => Header.Name;
        public double Ug { get; }                    // [W/(m2*K)]
        public double SolarEnergyTransmittance { get; } // g [-]
        public double? VisibleTransmittance { get; }
        public int? PaneCount { get; }
        public string GasFill { get; }
        public string CoatingType { get; }

        public GlazingDefinition(
            string id,
            string name,
            double ug,
            double solarEnergyTransmittance,
            double? visibleTransmittance = null,
            int? paneCount = null,
            string gasFill = "",
            string coatingType = "")
            : this(
                new EngineeringDataHeader(
                    id,
                    name,
                    category: "Glazing"),
                ug,
                solarEnergyTransmittance,
                visibleTransmittance,
                paneCount,
                gasFill,
                coatingType)
        {
        }

        public GlazingDefinition(
            EngineeringDataHeader header,
            double ug,
            double solarEnergyTransmittance,
            double? visibleTransmittance = null,
            int? paneCount = null,
            string gasFill = "",
            string coatingType = "")
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            EnsurePositiveFinite(ug, nameof(ug));
            EnsureFraction(
                solarEnergyTransmittance,
                nameof(solarEnergyTransmittance));

            if (visibleTransmittance.HasValue)
                EnsureFraction(
                    visibleTransmittance.Value,
                    nameof(visibleTransmittance));

            if (paneCount.HasValue && paneCount.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(paneCount));

            Ug = ug;
            SolarEnergyTransmittance = solarEnergyTransmittance;
            VisibleTransmittance = visibleTransmittance;
            PaneCount = paneCount;
            GasFill = gasFill?.Trim() ?? string.Empty;
            CoatingType = coatingType?.Trim() ?? string.Empty;
        }

        internal static void EnsurePositiveFinite(
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

        internal static void EnsureFraction(
            double value,
            string parameterName)
        {
            if (value < 0.0 ||
                value > 1.0 ||
                double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        internal static string RequireText(
            string value,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    "Az érték nem lehet üres.",
                    parameterName);

            return value.Trim();
        }
    }

    public sealed class FrameDefinition
    {
        public EngineeringDataHeader Header { get; }

        public string Id => Header.Id;
        public string Name => Header.Name;
        public double Uf { get; } // [W/(m2*K)]
        public FrameMaterialKind MaterialKind { get; }
        public double? ProfileDepth { get; } // [m]
        public int? ChamberCount { get; }
        public double? DefaultVisibleWidth { get; } // [m]

        public FrameDefinition(
            string id,
            string name,
            double uf,
            FrameMaterialKind materialKind,
            double? profileDepth = null,
            int? chamberCount = null,
            double? defaultVisibleWidth = null)
            : this(
                new EngineeringDataHeader(
                    id,
                    name,
                    category: "Frame"),
                uf,
                materialKind,
                profileDepth,
                chamberCount,
                defaultVisibleWidth)
        {
        }

        public FrameDefinition(
            EngineeringDataHeader header,
            double uf,
            FrameMaterialKind materialKind,
            double? profileDepth = null,
            int? chamberCount = null,
            double? defaultVisibleWidth = null)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            GlazingDefinition.EnsurePositiveFinite(uf, nameof(uf));

            if (profileDepth.HasValue &&
                profileDepth.Value <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(profileDepth));

            if (chamberCount.HasValue &&
                chamberCount.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(chamberCount));

            if (defaultVisibleWidth.HasValue &&
                defaultVisibleWidth.Value <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(defaultVisibleWidth));

            Uf = uf;
            MaterialKind = materialKind;
            ProfileDepth = profileDepth;
            ChamberCount = chamberCount;
            DefaultVisibleWidth = defaultVisibleWidth;
        }
    }

    public sealed class SpacerDefinition
    {
        public EngineeringDataHeader Header { get; }

        public string Id => Header.Id;
        public string Name => Header.Name;
        public double LinearThermalTransmittance { get; } // [W/(m*K)]
        public string SpacerType { get; }

        public SpacerDefinition(
            string id,
            string name,
            double linearThermalTransmittance,
            string spacerType = "")
            : this(
                new EngineeringDataHeader(
                    id,
                    name,
                    category: "Spacer"),
                linearThermalTransmittance,
                spacerType)
        {
        }

        public SpacerDefinition(
            EngineeringDataHeader header,
            double linearThermalTransmittance,
            string spacerType = "")
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));

            if (linearThermalTransmittance < 0.0 ||
                double.IsNaN(linearThermalTransmittance) ||
                double.IsInfinity(linearThermalTransmittance))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(linearThermalTransmittance));
            }

            LinearThermalTransmittance =
                linearThermalTransmittance;
            SpacerType = spacerType?.Trim() ?? string.Empty;
        }
    }

    public sealed class BuildingOpeningDefinition
    {
        public EngineeringDataHeader Header { get; }

        public string Id => Header.Id;
        public string Name => Header.Name;
        public BuildingOpeningType OpeningType { get; }
        public BuildingOpeningCalculationMode CalculationMode { get; }

        public double? DirectUValue { get; }
        public double? DirectGValue { get; }

        public string GlazingId { get; }
        public string FrameId { get; }
        public string SpacerId { get; }

        public double? DefaultFrameWidth { get; } // [m]
        public string AirLeakageClass { get; }

        public IReadOnlyDictionary<string, string> Metadata =>
            Header.Metadata;

        public BuildingOpeningDefinition(
            string id,
            string name,
            BuildingOpeningType openingType,
            BuildingOpeningCalculationMode calculationMode,
            double? directUValue,
            double? directGValue,
            string glazingId,
            string frameId,
            string spacerId,
            double? defaultFrameWidth,
            string airLeakageClass,
            IDictionary<string, string>? metadata = null)
            : this(
                new EngineeringDataHeader(
                    id,
                    name,
                    category: "BuildingOpening",
                    metadata: metadata),
                openingType,
                calculationMode,
                directUValue,
                directGValue,
                glazingId,
                frameId,
                spacerId,
                defaultFrameWidth,
                airLeakageClass)
        {
        }

        public BuildingOpeningDefinition(
            EngineeringDataHeader header,
            BuildingOpeningType openingType,
            BuildingOpeningCalculationMode calculationMode,
            double? directUValue,
            double? directGValue,
            string glazingId,
            string frameId,
            string spacerId,
            double? defaultFrameWidth,
            string airLeakageClass)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));

            if (directUValue.HasValue)
                GlazingDefinition.EnsurePositiveFinite(
                    directUValue.Value,
                    nameof(directUValue));

            if (directGValue.HasValue)
                GlazingDefinition.EnsureFraction(
                    directGValue.Value,
                    nameof(directGValue));

            if (defaultFrameWidth.HasValue &&
                defaultFrameWidth.Value <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(defaultFrameWidth));

            OpeningType = openingType;
            CalculationMode = calculationMode;
            DirectUValue = directUValue;
            DirectGValue = directGValue;
            GlazingId = glazingId?.Trim() ?? string.Empty;
            FrameId = frameId?.Trim() ?? string.Empty;
            SpacerId = spacerId?.Trim() ?? string.Empty;
            DefaultFrameWidth = defaultFrameWidth;
            AirLeakageClass =
                airLeakageClass?.Trim() ?? string.Empty;
        }
    }

    public sealed class BuildingOpeningCatalog
    {
        private readonly Dictionary<string, GlazingDefinition> _glazings;
        private readonly Dictionary<string, FrameDefinition> _frames;
        private readonly Dictionary<string, SpacerDefinition> _spacers;
        private readonly Dictionary<string, BuildingOpeningDefinition> _openings;

        public string CatalogId { get; }
        public string Version { get; }

        public IReadOnlyCollection<GlazingDefinition> Glazings =>
            new ReadOnlyCollection<GlazingDefinition>(
                _glazings.Values.OrderBy(item => item.Name).ToList());

        public IReadOnlyCollection<FrameDefinition> Frames =>
            new ReadOnlyCollection<FrameDefinition>(
                _frames.Values.OrderBy(item => item.Name).ToList());

        public IReadOnlyCollection<SpacerDefinition> Spacers =>
            new ReadOnlyCollection<SpacerDefinition>(
                _spacers.Values.OrderBy(item => item.Name).ToList());

        public IReadOnlyCollection<BuildingOpeningDefinition> Openings =>
            new ReadOnlyCollection<BuildingOpeningDefinition>(
                _openings.Values.OrderBy(item => item.Name).ToList());

        public BuildingOpeningCatalog(
            string catalogId,
            string version,
            IEnumerable<GlazingDefinition> glazings,
            IEnumerable<FrameDefinition> frames,
            IEnumerable<SpacerDefinition> spacers,
            IEnumerable<BuildingOpeningDefinition> openings)
        {
            CatalogId = GlazingDefinition.RequireText(
                catalogId,
                nameof(catalogId));

            Version = GlazingDefinition.RequireText(
                version,
                nameof(version));

            _glazings = ToDictionary(
                glazings,
                item => item.Id,
                "üvegezés");

            _frames = ToDictionary(
                frames,
                item => item.Id,
                "keret");

            _spacers = ToDictionary(
                spacers,
                item => item.Id,
                "távtartó");

            _openings = ToDictionary(
                openings,
                item => item.Id,
                "nyílászáró");
        }

        public bool TryGetGlazing(
            string id,
            out GlazingDefinition value) =>
            TryGet(_glazings, id, out value);

        public bool TryGetFrame(
            string id,
            out FrameDefinition value) =>
            TryGet(_frames, id, out value);

        public bool TryGetSpacer(
            string id,
            out SpacerDefinition value) =>
            TryGet(_spacers, id, out value);

        public bool TryGetOpening(
            string id,
            out BuildingOpeningDefinition value) =>
            TryGet(_openings, id, out value);

        private static Dictionary<string, T> ToDictionary<T>(
            IEnumerable<T> items,
            Func<T, string> idSelector,
            string itemName)
        {
            var result = new Dictionary<string, T>(
                StringComparer.OrdinalIgnoreCase);

            foreach (T item in
                items ?? throw new ArgumentNullException(nameof(items)))
            {
                string id = idSelector(item);

                if (result.ContainsKey(id))
                    throw new ArgumentException(
                        $"Duplikált {itemName}-azonosító: {id}.");

                result.Add(id, item);
            }

            return result;
        }

        private static bool TryGet<T>(
            Dictionary<string, T> source,
            string id,
            out T value)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                value = default(T);
                return false;
            }

            return source.TryGetValue(id.Trim(), out value);
        }
    }
}
