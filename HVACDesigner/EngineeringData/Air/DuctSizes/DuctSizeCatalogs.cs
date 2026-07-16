using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Air.DuctSizes
{
    public sealed class CircularDuctSizeCatalog
    {
        private readonly ReadOnlyCollection<CircularDuctSizeDefinition> _items;

        public string CatalogId { get; }
        public string Version { get; }
        public IReadOnlyList<CircularDuctSizeDefinition> Items => _items;

        public CircularDuctSizeCatalog(
            string catalogId,
            string version,
            IEnumerable<CircularDuctSizeDefinition> items)
        {
            CatalogId = RequireText(catalogId, nameof(catalogId));
            Version = RequireText(version, nameof(version));

            var list = (items ?? throw new ArgumentNullException(nameof(items)))
                .OrderBy(item => item.DiameterMillimeters)
                .ToList();

            int duplicate = list
                .GroupBy(item => item.DiameterMillimeters)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

            if (duplicate > 0)
                throw new ArgumentException(
                    $"Duplikált körméret: {duplicate} mm.",
                    nameof(items));

            _items = new ReadOnlyCollection<CircularDuctSizeDefinition>(list);
        }

        public bool ContainsDiameter(int diameterMillimeters) =>
            _items.Any(item => item.DiameterMillimeters == diameterMillimeters);

        private static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Az érték nem lehet üres.", parameterName);

            return value.Trim();
        }
    }

    public sealed class RectangularDuctSizeCatalog
    {
        private readonly ReadOnlyCollection<RectangularDuctSizeDefinition> _items;

        public string CatalogId { get; }
        public string Version { get; }
        public IReadOnlyList<RectangularDuctSizeDefinition> Items => _items;

        public RectangularDuctSizeCatalog(
            string catalogId,
            string version,
            IEnumerable<RectangularDuctSizeDefinition> items)
        {
            CatalogId = RequireText(catalogId, nameof(catalogId));
            Version = RequireText(version, nameof(version));

            var list = (items ?? throw new ArgumentNullException(nameof(items)))
                .OrderBy(item => item.WidthMillimeters)
                .ThenBy(item => item.HeightMillimeters)
                .ToList();

            string duplicate = list
                .GroupBy(item => $"{item.WidthMillimeters}x{item.HeightMillimeters}")
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(duplicate))
                throw new ArgumentException(
                    $"Duplikált téglalapméret: {duplicate} mm.",
                    nameof(items));

            _items = new ReadOnlyCollection<RectangularDuctSizeDefinition>(list);
        }

        public bool ContainsSize(int widthMillimeters, int heightMillimeters) =>
            _items.Any(item =>
                item.WidthMillimeters == widthMillimeters &&
                item.HeightMillimeters == heightMillimeters);

        private static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Az érték nem lehet üres.", parameterName);

            return value.Trim();
        }
    }
}
