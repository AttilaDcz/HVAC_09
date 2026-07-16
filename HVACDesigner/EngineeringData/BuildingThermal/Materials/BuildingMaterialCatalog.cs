using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.BuildingThermal.Materials
{
    /// <summary>
    /// Változtathatatlan, típusos homogén építőanyag-katalógus.
    /// </summary>
    public sealed class BuildingMaterialCatalog
    {
        private readonly ReadOnlyCollection<BuildingMaterialDefinition>
            _items;

        private readonly Dictionary<string, BuildingMaterialDefinition>
            _byId;

        public string CatalogId { get; }
        public string Version { get; }
        public IReadOnlyList<BuildingMaterialDefinition> Items => _items;

        public BuildingMaterialCatalog(
            string catalogId,
            string version,
            IEnumerable<BuildingMaterialDefinition> items)
        {
            CatalogId = RequireText(catalogId, nameof(catalogId));
            Version = RequireText(version, nameof(version));

            var list =
                (items ??
                 throw new ArgumentNullException(nameof(items)))
                .OrderBy(item => item.Category)
                .ThenBy(item => item.Name)
                .ToList();

            string duplicateId = list
                .GroupBy(
                    item => item.Id,
                    StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(duplicateId))
            {
                throw new ArgumentException(
                    $"Duplikált építőanyag-azonosító: {duplicateId}.",
                    nameof(items));
            }

            _items =
                new ReadOnlyCollection<BuildingMaterialDefinition>(
                    list);

            _byId = list.ToDictionary(
                item => item.Id,
                StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGet(
            string materialId,
            out BuildingMaterialDefinition material)
        {
            if (string.IsNullOrWhiteSpace(materialId))
            {
                material = null;
                return false;
            }

            return _byId.TryGetValue(
                materialId.Trim(),
                out material);
        }

        public BuildingMaterialDefinition GetRequired(
            string materialId)
        {
            if (TryGet(
                materialId,
                out BuildingMaterialDefinition material))
            {
                return material;
            }

            throw new KeyNotFoundException(
                $"Az építőanyag nem található: {materialId}.");
        }

        public IReadOnlyList<BuildingMaterialDefinition> GetByCategory(
            string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return new ReadOnlyCollection<
                    BuildingMaterialDefinition>(
                    new List<BuildingMaterialDefinition>());
            }

            return new ReadOnlyCollection<BuildingMaterialDefinition>(
                _items
                    .Where(item =>
                        string.Equals(
                            item.Category,
                            category.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                    .ToList());
        }

        public IReadOnlyList<string> GetCategories() =>
            new ReadOnlyCollection<string>(
                _items
                    .Select(item => item.Category)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(item => item)
                    .ToList());

        private static string RequireText(
            string value,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Az érték nem lehet üres.",
                    parameterName);
            }

            return value.Trim();
        }
    }
}
