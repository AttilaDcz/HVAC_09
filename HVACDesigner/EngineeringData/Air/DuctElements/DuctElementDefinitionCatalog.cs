using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Air.DuctElements
{
    public sealed class DuctElementDefinitionCatalog
    {
        private readonly ReadOnlyCollection<DuctElementDefinition> _items;
        private readonly Dictionary<string, DuctElementDefinition> _byId;

        public string CatalogId { get; }
        public string Version { get; }
        public IReadOnlyList<DuctElementDefinition> Items => _items;

        public DuctElementDefinitionCatalog(
            string catalogId,
            string version,
            IEnumerable<DuctElementDefinition> items)
        {
            CatalogId = RequireText(catalogId, nameof(catalogId));
            Version = RequireText(version, nameof(version));

            var list =
                (items ?? throw new ArgumentNullException(nameof(items)))
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
                throw new ArgumentException(
                    $"Duplikált elemdefiníció-azonosító: {duplicateId}.",
                    nameof(items));

            _items =
                new ReadOnlyCollection<DuctElementDefinition>(list);

            _byId = list.ToDictionary(
                item => item.Id,
                StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGet(
            string id,
            out DuctElementDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                definition = null;
                return false;
            }

            return _byId.TryGetValue(id.Trim(), out definition);
        }

        public DuctElementDefinition GetRequired(string id)
        {
            if (TryGet(id, out DuctElementDefinition definition))
                return definition;

            throw new KeyNotFoundException(
                $"A légtechnikai elemdefiníció nem található: {id}.");
        }

        public IReadOnlyList<DuctElementDefinition> GetByCategory(
            DuctElementCategory category) =>
            new ReadOnlyCollection<DuctElementDefinition>(
                _items.Where(item => item.Category == category).ToList());

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
