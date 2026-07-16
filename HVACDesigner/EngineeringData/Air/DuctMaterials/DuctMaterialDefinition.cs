using System;
using HVACDesigner.EngineeringData.Common;

namespace HVACDesigner.EngineeringData.Air.DuctMaterials
{
    /// <summary>
    /// Technológiafüggetlen légcsatorna-anyag definíció.
    /// Az abszolút érdességet belső SI-egységben, méterben tárolja.
    /// </summary>
    public sealed class DuctMaterialDefinition
    {
        public EngineeringDataHeader Header { get; }

        public string Id => Header.Id;
        public string Name => Header.Name;
        public double AbsoluteRoughness { get; } // [m]
        public bool IsFlexible { get; }
        public string SourcePackageId => Header.SourcePackageId;
        public string SourceContentSetId => Header.SourceContentSetId;
        public string SourceVersion => Header.SourceVersion;

        public double AbsoluteRoughnessMillimeters =>
            AbsoluteRoughness * 1000.0;

        public DuctMaterialDefinition(
            string id,
            string name,
            double absoluteRoughness,
            bool isFlexible,
            string sourcePackageId,
            string sourceContentSetId,
            string sourceVersion)
            : this(
                new EngineeringDataHeader(
                    id,
                    name,
                    category: "DuctMaterial",
                    sourcePackageId: RequireText(
                        sourcePackageId,
                        nameof(sourcePackageId)),
                    sourceContentSetId: RequireText(
                        sourceContentSetId,
                        nameof(sourceContentSetId)),
                    sourceVersion: RequireText(
                        sourceVersion,
                        nameof(sourceVersion))),
                absoluteRoughness,
                isFlexible)
        {
        }

        public DuctMaterialDefinition(
            EngineeringDataHeader header,
            double absoluteRoughness,
            bool isFlexible)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));

            if (absoluteRoughness < 0.0 ||
                double.IsNaN(absoluteRoughness) ||
                double.IsInfinity(absoluteRoughness))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(absoluteRoughness),
                    "Az abszolút érdesség nem lehet negatív vagy nem véges.");
            }

            AbsoluteRoughness = absoluteRoughness;
            IsFlexible = isFlexible;
        }

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
