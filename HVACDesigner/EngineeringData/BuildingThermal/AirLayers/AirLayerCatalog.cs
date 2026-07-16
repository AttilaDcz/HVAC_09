using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HVACDesigner.EngineeringData.Common;

namespace HVACDesigner.EngineeringData.BuildingThermal.AirLayers
{
    public enum AirLayerOrientation
    {
        Unspecified,
        Vertical,
        HorizontalHeatFlowUp,
        HorizontalHeatFlowDown
    }

    public enum AirLayerVentilationLevel
    {
        Unspecified,
        Unventilated,
        SlightlyVentilated,
        WellVentilated
    }

    public sealed class AirLayerDefinition
    {
        public EngineeringDataHeader Header { get; }

        public string Id => Header.Id;
        public string Name => Header.Name;
        public double ThermalResistance { get; } // [m2*K/W]
        public AirLayerOrientation Orientation { get; }
        public AirLayerVentilationLevel VentilationLevel { get; }

        public string SourcePackageId => Header.SourcePackageId;
        public string SourceContentSetId => Header.SourceContentSetId;
        public string SourceVersion => Header.SourceVersion;

        public IReadOnlyDictionary<string, string> Metadata =>
            Header.Metadata;

        public AirLayerDefinition(
            string id,
            string name,
            double thermalResistance,
            AirLayerOrientation orientation,
            AirLayerVentilationLevel ventilationLevel,
            string sourcePackageId,
            string sourceContentSetId,
            string sourceVersion,
            IDictionary<string, string>? metadata = null)
            : this(
                new EngineeringDataHeader(
                    id,
                    name,
                    category: "AirLayer",
                    sourcePackageId: RequireText(
                        sourcePackageId,
                        nameof(sourcePackageId)),
                    sourceContentSetId: RequireText(
                        sourceContentSetId,
                        nameof(sourceContentSetId)),
                    sourceVersion: RequireText(
                        sourceVersion,
                        nameof(sourceVersion)),
                    metadata: metadata),
                thermalResistance,
                orientation,
                ventilationLevel)
        {
        }

        public AirLayerDefinition(
            EngineeringDataHeader header,
            double thermalResistance,
            AirLayerOrientation orientation,
            AirLayerVentilationLevel ventilationLevel)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));

            if (thermalResistance <= 0.0 ||
                double.IsNaN(thermalResistance) ||
                double.IsInfinity(thermalResistance))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(thermalResistance),
                    "A hőellenállásnak pozitív és véges számnak kell lennie.");
            }

            ThermalResistance = thermalResistance;
            Orientation = orientation;
            VentilationLevel = ventilationLevel;
        }

        private static string RequireText(
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

    public sealed class AirLayerCatalog
    {
        private readonly ReadOnlyCollection<AirLayerDefinition> _items;
        private readonly Dictionary<string, AirLayerDefinition> _byId;

        public string CatalogId { get; }
        public string Version { get; }
        public IReadOnlyList<AirLayerDefinition> Items => _items;

        public AirLayerCatalog(
            string catalogId,
            string version,
            IEnumerable<AirLayerDefinition> items)
        {
            CatalogId = RequireText(catalogId, nameof(catalogId));
            Version = RequireText(version, nameof(version));

            var list = (items ?? throw new ArgumentNullException(nameof(items)))
                .OrderBy(item => item.Name)
                .ToList();

            string duplicateId = list
                .GroupBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(duplicateId))
                throw new ArgumentException(
                    $"Duplikált légréteg-azonosító: {duplicateId}.",
                    nameof(items));

            _items = new ReadOnlyCollection<AirLayerDefinition>(list);
            _byId = list.ToDictionary(
                item => item.Id,
                StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGet(string id, out AirLayerDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                definition = null;
                return false;
            }

            return _byId.TryGetValue(id.Trim(), out definition);
        }

        public AirLayerDefinition GetRequired(string id)
        {
            if (TryGet(id, out AirLayerDefinition definition))
                return definition;

            throw new KeyNotFoundException(
                $"A légréteg nem található: {id}.");
        }

        private static string RequireText(
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
}
