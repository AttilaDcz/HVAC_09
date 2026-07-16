using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Air.DuctMaterials
{
    /// <summary>
    /// Változtathatatlan, típusos légcsatorna-anyag katalógus.
    /// </summary>
    public sealed class DuctMaterialCatalog
    {
        private readonly ReadOnlyCollection<DuctMaterialDefinition>
            _items;

        private readonly Dictionary<string, DuctMaterialDefinition>
            _itemsById;

        public string CatalogId { get; }
        public string Version { get; }
        public IReadOnlyList<DuctMaterialDefinition> Items => _items;

        public DuctMaterialCatalog(
            string catalogId,
            string version,
            IEnumerable<DuctMaterialDefinition> items)
        {
            if (string.IsNullOrWhiteSpace(catalogId))
                throw new ArgumentException(
                    "A katalógusazonosító nem lehet üres.",
                    nameof(catalogId));

            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException(
                    "A katalógusverzió nem lehet üres.",
                    nameof(version));

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var itemList = items.ToList();

            string duplicateId = itemList
                .GroupBy(
                    item => item.Id,
                    StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(duplicateId))
            {
                throw new ArgumentException(
                    $"Duplikált légcsatorna-anyag azonosító: {duplicateId}.",
                    nameof(items));
            }

            CatalogId = catalogId.Trim();
            Version = version.Trim();

            _items =
                new ReadOnlyCollection<DuctMaterialDefinition>(
                    itemList);

            _itemsById =
                itemList.ToDictionary(
                    item => item.Id,
                    StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGet(
            string materialId,
            out DuctMaterialDefinition material)
        {
            if (string.IsNullOrWhiteSpace(materialId))
            {
                material = null;
                return false;
            }

            return _itemsById.TryGetValue(
                materialId.Trim(),
                out material);
        }

        public DuctMaterialDefinition GetRequired(
            string materialId)
        {
            if (TryGet(materialId, out DuctMaterialDefinition material))
                return material;

            throw new KeyNotFoundException(
                $"A légcsatorna-anyag nem található: {materialId}.");
        }
    }
}
