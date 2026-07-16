using System;
using System.Collections.Generic;
using System.Linq;
using HVACDesigner.Calculations.Water;
using HVACDesigner.EngineeringData.SimpleCatalogs;

namespace HVACDesigner.Features.Water
{
    /// <summary>
    /// Az EngineeringData fixture katalógust a meglévő Water kalkulátor
    /// típusos katalógusmodelljére alakítja.
    /// </summary>
    public sealed class FixtureCatalogAdapter
    {
        public IReadOnlyDictionary<string, FixtureCatalogItem> Adapt(
            SimpleCatalog<FixtureDefinition> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Items.Values.ToDictionary(
                item => item.Id,
                item => new FixtureCatalogItem(
                    item.Id,
                    item.DisplayName,
                    item.PotableLoadingUnit,
                    item.WastewaterDu,
                    item.MinimumWasteDn,
                    item.HotWaterRelevant,
                    item.GreywaterSource,
                    item.GreywaterDemand),
                StringComparer.OrdinalIgnoreCase);
        }
    }
}


