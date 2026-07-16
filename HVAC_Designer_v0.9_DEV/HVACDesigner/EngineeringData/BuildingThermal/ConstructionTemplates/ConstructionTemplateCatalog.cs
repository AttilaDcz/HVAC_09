using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HVACDesigner.EngineeringData.Common;

namespace HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates
{
    public enum ConstructionType
    {
        ExternalWall,
        InternalWall,
        Roof,
        Ceiling,
        Floor,
        GroundFloor,
        BasementWall,
        Slab,
        HistoricalWall,
        Custom
    }

    public enum ConstructionLayerKind
    {
        Material,
        AirLayer,
        FixedResistance
    }

    public enum AdjacentBoundaryKind
    {
        Outdoor,
        Ground,
        AdjacentConditionedZone,
        AdjacentUnconditionedSpace,
        Adiabatic,
        UserDefinedTemperature
    }

    public enum AdjacentSpaceTemperatureMode
    {
        None,
        FixedTemperature,
        TemperatureReductionFactor,
        CalculatedUnconditionedSpace
    }

    /// <summary>
    /// Projektbeli szerkezetalkalmazás határfeltétele.
    /// Nem része a rétegrendi sablonnak, de ugyanebben a csomagban
    /// kap stabil szerződést a későbbi kalkulátorok számára.
    /// </summary>
    public sealed class ConstructionBoundaryCondition
    {
        public AdjacentBoundaryKind BoundaryKind { get; }
        public AdjacentSpaceTemperatureMode TemperatureMode { get; }

        public string AdjacentSpaceId { get; }
        public double? FixedAdjacentTemperature { get; }      // [°C]
        public double? TemperatureReductionFactor { get; }    // b_u [-]
        public string RuleReference { get; }

        public ConstructionBoundaryCondition(
            AdjacentBoundaryKind boundaryKind,
            AdjacentSpaceTemperatureMode temperatureMode =
                AdjacentSpaceTemperatureMode.None,
            string adjacentSpaceId = "",
            double? fixedAdjacentTemperature = null,
            double? temperatureReductionFactor = null,
            string ruleReference = "")
        {
            if (fixedAdjacentTemperature.HasValue &&
                (double.IsNaN(fixedAdjacentTemperature.Value) ||
                 double.IsInfinity(fixedAdjacentTemperature.Value)))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(fixedAdjacentTemperature));
            }

            if (temperatureReductionFactor.HasValue &&
                (temperatureReductionFactor.Value < 0.0 ||
                 temperatureReductionFactor.Value > 1.0 ||
                 double.IsNaN(temperatureReductionFactor.Value) ||
                 double.IsInfinity(temperatureReductionFactor.Value)))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(temperatureReductionFactor),
                    "A b_u értéknek 0 és 1 közé kell esnie.");
            }

            if (temperatureMode ==
                    AdjacentSpaceTemperatureMode.FixedTemperature &&
                !fixedAdjacentTemperature.HasValue)
            {
                throw new ArgumentException(
                    "FixedTemperature módhoz túloldali hőmérséklet szükséges.",
                    nameof(fixedAdjacentTemperature));
            }

            if (temperatureMode ==
                    AdjacentSpaceTemperatureMode.TemperatureReductionFactor &&
                !temperatureReductionFactor.HasValue)
            {
                throw new ArgumentException(
                    "TemperatureReductionFactor módhoz b_u érték szükséges.",
                    nameof(temperatureReductionFactor));
            }

            BoundaryKind = boundaryKind;
            TemperatureMode = temperatureMode;
            AdjacentSpaceId = adjacentSpaceId?.Trim() ?? string.Empty;
            FixedAdjacentTemperature = fixedAdjacentTemperature;
            TemperatureReductionFactor = temperatureReductionFactor;
            RuleReference = ruleReference?.Trim() ?? string.Empty;
        }

        public double ResolveTemperatureDifference(
            double insideTemperature,
            double outsideTemperature)
        {
            if (double.IsNaN(insideTemperature) ||
                double.IsInfinity(insideTemperature))
                throw new ArgumentOutOfRangeException(
                    nameof(insideTemperature));

            if (double.IsNaN(outsideTemperature) ||
                double.IsInfinity(outsideTemperature))
                throw new ArgumentOutOfRangeException(
                    nameof(outsideTemperature));

            switch (TemperatureMode)
            {
                case AdjacentSpaceTemperatureMode.FixedTemperature:
                    return insideTemperature -
                           FixedAdjacentTemperature.Value;

                case AdjacentSpaceTemperatureMode.TemperatureReductionFactor:
                    return TemperatureReductionFactor.Value *
                           (insideTemperature - outsideTemperature);

                default:
                    return insideTemperature - outsideTemperature;
            }
        }
    }

    public sealed class ConstructionLayerDefinition
    {
        public int Order { get; }
        public ConstructionLayerKind LayerKind { get; }
        public string ReferenceId { get; }
        public double? Thickness { get; }          // [m]
        public double? FixedThermalResistance { get; } // [m2*K/W]
        public double AreaFraction { get; }        // [0..1]
        public string Description { get; }

        public ConstructionLayerDefinition(
            int order,
            ConstructionLayerKind layerKind,
            string referenceId,
            double? thickness,
            double? fixedThermalResistance,
            double areaFraction = 1.0,
            string description = "")
        {
            if (order < 0)
                throw new ArgumentOutOfRangeException(nameof(order));

            if (areaFraction <= 0.0 ||
                areaFraction > 1.0 ||
                double.IsNaN(areaFraction) ||
                double.IsInfinity(areaFraction))
            {
                throw new ArgumentOutOfRangeException(nameof(areaFraction));
            }

            if (layerKind == ConstructionLayerKind.Material)
            {
                if (string.IsNullOrWhiteSpace(referenceId))
                    throw new ArgumentException(
                        "Anyagréteghez hivatkozás szükséges.",
                        nameof(referenceId));

                if (!thickness.HasValue ||
                    thickness.Value <= 0.0 ||
                    double.IsNaN(thickness.Value) ||
                    double.IsInfinity(thickness.Value))
                {
                    throw new ArgumentOutOfRangeException(nameof(thickness));
                }
            }

            if (layerKind == ConstructionLayerKind.AirLayer &&
                string.IsNullOrWhiteSpace(referenceId))
            {
                throw new ArgumentException(
                    "Légréteghez hivatkozás szükséges.",
                    nameof(referenceId));
            }

            if (layerKind == ConstructionLayerKind.FixedResistance)
            {
                if (!fixedThermalResistance.HasValue ||
                    fixedThermalResistance.Value <= 0.0 ||
                    double.IsNaN(fixedThermalResistance.Value) ||
                    double.IsInfinity(fixedThermalResistance.Value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(fixedThermalResistance));
                }
            }

            Order = order;
            LayerKind = layerKind;
            ReferenceId = referenceId?.Trim() ?? string.Empty;
            Thickness = thickness;
            FixedThermalResistance = fixedThermalResistance;
            AreaFraction = areaFraction;
            Description = description?.Trim() ?? string.Empty;
        }
    }

    public sealed class ConstructionPathDefinition
    {
        private readonly ReadOnlyCollection<ConstructionLayerDefinition> _layers;

        public string Id { get; }
        public string Name { get; }
        public double AreaFraction { get; }
        public IReadOnlyList<ConstructionLayerDefinition> Layers => _layers;

        public ConstructionPathDefinition(
            string id,
            string name,
            double areaFraction,
            IEnumerable<ConstructionLayerDefinition> layers)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(
                    "Az útvonalazonosító nem lehet üres.",
                    nameof(id));

            if (areaFraction <= 0.0 ||
                areaFraction > 1.0 ||
                double.IsNaN(areaFraction) ||
                double.IsInfinity(areaFraction))
            {
                throw new ArgumentOutOfRangeException(nameof(areaFraction));
            }

            var list = (layers ??
                throw new ArgumentNullException(nameof(layers)))
                .OrderBy(item => item.Order)
                .ToList();

            if (list.Count == 0)
                throw new ArgumentException(
                    "Az útvonalnak legalább egy réteget tartalmaznia kell.",
                    nameof(layers));

            Id = id.Trim();
            Name = string.IsNullOrWhiteSpace(name)
                ? Id
                : name.Trim();
            AreaFraction = areaFraction;
            _layers =
                new ReadOnlyCollection<ConstructionLayerDefinition>(list);
        }
    }

    public sealed class ConstructionTemplateDefinition
    {
        private readonly ReadOnlyCollection<ConstructionLayerDefinition>
            _layers;

        private readonly ReadOnlyCollection<ConstructionPathDefinition>
            _parallelPaths;

        public EngineeringDataHeader Header { get; }

        public string Id => Header.Id;
        public string Name => Header.Name;
        public ConstructionType ConstructionType { get; }

        /// <summary>
        /// Rétegek belülről kifelé.
        /// </summary>
        public IReadOnlyList<ConstructionLayerDefinition> Layers => _layers;

        /// <summary>
        /// Opcionális párhuzamos hőáramú utak könnyűszerkezetekhez.
        /// </summary>
        public IReadOnlyList<ConstructionPathDefinition> ParallelPaths =>
            _parallelPaths;

        public string ValueSource { get; }
        public string StandardReference => Header.StandardReference;
        public string Notes { get; }

        public string SourcePackageId => Header.SourcePackageId;
        public string SourceContentSetId => Header.SourceContentSetId;
        public string SourceVersion => Header.SourceVersion;

        public IReadOnlyDictionary<string, string> Metadata =>
            Header.Metadata;

        public bool UsesParallelPaths => _parallelPaths.Count > 0;

        public ConstructionTemplateDefinition(
            string id,
            string name,
            ConstructionType constructionType,
            IEnumerable<ConstructionLayerDefinition> layers,
            IEnumerable<ConstructionPathDefinition> parallelPaths,
            string valueSource,
            string standardReference,
            string notes,
            string sourcePackageId,
            string sourceContentSetId,
            string sourceVersion,
            IDictionary<string, string>? metadata = null)
            : this(
                new EngineeringDataHeader(
                    id,
                    name,
                    category: "ConstructionTemplate",
                    sourcePackageId: RequireText(
                        sourcePackageId,
                        nameof(sourcePackageId)),
                    sourceContentSetId: RequireText(
                        sourceContentSetId,
                        nameof(sourceContentSetId)),
                    sourceVersion: RequireText(
                        sourceVersion,
                        nameof(sourceVersion)),
                    standardReference: standardReference,
                    metadata: metadata),
                constructionType,
                layers,
                parallelPaths,
                valueSource,
                notes)
        {
        }

        public ConstructionTemplateDefinition(
            EngineeringDataHeader header,
            ConstructionType constructionType,
            IEnumerable<ConstructionLayerDefinition> layers,
            IEnumerable<ConstructionPathDefinition> parallelPaths,
            string valueSource,
            string notes)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            ConstructionType = constructionType;

            var layerList =
                (layers ?? Enumerable.Empty<ConstructionLayerDefinition>())
                .OrderBy(item => item.Order)
                .ToList();

            var pathList =
                (parallelPaths ??
                 Enumerable.Empty<ConstructionPathDefinition>())
                .ToList();

            if (layerList.Count == 0 && pathList.Count == 0)
                throw new ArgumentException(
                    "A szerkezetsablonnak réteget vagy párhuzamos útvonalat kell tartalmaznia.");

            _layers =
                new ReadOnlyCollection<ConstructionLayerDefinition>(
                    layerList);

            _parallelPaths =
                new ReadOnlyCollection<ConstructionPathDefinition>(
                    pathList);

            ValueSource = valueSource?.Trim() ?? string.Empty;
            Notes = notes?.Trim() ?? string.Empty;
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

    public sealed class ConstructionTemplateCatalog
    {
        private readonly ReadOnlyCollection<ConstructionTemplateDefinition>
            _items;

        private readonly Dictionary<string, ConstructionTemplateDefinition>
            _byId;

        public string CatalogId { get; }
        public string Version { get; }
        public IReadOnlyList<ConstructionTemplateDefinition> Items => _items;

        public ConstructionTemplateCatalog(
            string catalogId,
            string version,
            IEnumerable<ConstructionTemplateDefinition> items)
        {
            if (string.IsNullOrWhiteSpace(catalogId))
                throw new ArgumentException(
                    "A katalógusazonosító nem lehet üres.",
                    nameof(catalogId));

            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException(
                    "A verzió nem lehet üres.",
                    nameof(version));

            var list = (items ??
                throw new ArgumentNullException(nameof(items)))
                .OrderBy(item => item.ConstructionType)
                .ThenBy(item => item.Name)
                .ToList();

            string duplicateId = list
                .GroupBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(duplicateId))
                throw new ArgumentException(
                    $"Duplikált szerkezetsablon-azonosító: {duplicateId}.",
                    nameof(items));

            CatalogId = catalogId.Trim();
            Version = version.Trim();
            _items =
                new ReadOnlyCollection<ConstructionTemplateDefinition>(list);
            _byId = list.ToDictionary(
                item => item.Id,
                StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGet(
            string id,
            out ConstructionTemplateDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                definition = null;
                return false;
            }

            return _byId.TryGetValue(id.Trim(), out definition);
        }

        public ConstructionTemplateDefinition GetRequired(string id)
        {
            if (TryGet(id, out ConstructionTemplateDefinition value))
                return value;

            throw new KeyNotFoundException(
                $"A szerkezetsablon nem található: {id}.");
        }
    }
}
