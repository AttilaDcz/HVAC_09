using System;
using System.Collections.Generic;
using HVACDesigner.EngineeringData.Common;

namespace HVACDesigner.EngineeringData.Air.DuctElements
{
    public enum DuctElementCategory
    {
        StraightDuct,
        FlexibleDuct,
        Elbow,
        Reducer,
        Expander,
        Transition,
        Branch,
        Damper,
        Accessory,
        Louver,
        AirTerminal,
        Custom,
        Other
    }

    public enum DuctGeometryKind
    {
        Unspecified,
        Circular,
        Rectangular,
        SingleSize,
        SizeChange,
        ShapeChange,
        ThreeWay,
        FourWay,
        FreeArea,
        Custom
    }

    public enum DuctFlowDirection
    {
        Unspecified,
        Forward,
        Reverse,
        Both
    }

    public enum DuctPressureModel
    {
        Unspecified,
        Friction,
        Zeta,
        FixedPressure,
        ManufacturerData
    }

    /// <summary>
    /// Technológiafüggetlen, változtathatatlan légtechnikai elemdefiníció.
    /// Nem konkrét projektelem és nem végez számítást.
    /// </summary>
    public sealed class DuctElementDefinition
    {
        public EngineeringDataHeader Header { get; }

        public string Id => Header.Id;
        public string Name => Header.Name;
        public DuctElementCategory Category { get; }
        public DuctGeometryKind GeometryKind { get; }
        public DuctFlowDirection FlowDirection { get; }
        public DuctPressureModel PressureModel { get; }

        public string DefaultMaterialId { get; }
        public double DefaultZeta { get; }

        public bool AllowSizeChange { get; }
        public bool AllowLength { get; }
        public bool AllowRadius { get; }
        public bool AllowBranch { get; }

        public string SourceElementName { get; }
        public string SourceSectionPath { get; }

        public string SourcePackageId => Header.SourcePackageId;
        public string SourceContentSetId => Header.SourceContentSetId;
        public string SourceVersion => Header.SourceVersion;

        /// <summary>
        /// Ismeretlen vagy később bevezetett XML-attribútumok megőrzése.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata =>
            Header.Metadata;

        public DuctElementDefinition(
            string id,
            string name,
            DuctElementCategory category,
            DuctGeometryKind geometryKind,
            DuctFlowDirection flowDirection,
            DuctPressureModel pressureModel,
            string defaultMaterialId,
            double defaultZeta,
            bool allowSizeChange,
            bool allowLength,
            bool allowRadius,
            bool allowBranch,
            string sourceElementName,
            string sourceSectionPath,
            string sourcePackageId,
            string sourceContentSetId,
            string sourceVersion,
            IDictionary<string, string>? metadata = null)
            : this(
                new EngineeringDataHeader(
                    id,
                    name,
                    category: "DuctElement",
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
                category,
                geometryKind,
                flowDirection,
                pressureModel,
                defaultMaterialId,
                defaultZeta,
                allowSizeChange,
                allowLength,
                allowRadius,
                allowBranch,
                sourceElementName,
                sourceSectionPath)
        {
        }

        public DuctElementDefinition(
            EngineeringDataHeader header,
            DuctElementCategory category,
            DuctGeometryKind geometryKind,
            DuctFlowDirection flowDirection,
            DuctPressureModel pressureModel,
            string defaultMaterialId,
            double defaultZeta,
            bool allowSizeChange,
            bool allowLength,
            bool allowRadius,
            bool allowBranch,
            string sourceElementName,
            string sourceSectionPath)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));

            if (defaultZeta < 0.0 ||
                double.IsNaN(defaultZeta) ||
                double.IsInfinity(defaultZeta))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(defaultZeta),
                    "Az alapértelmezett ζ nem lehet negatív vagy nem véges.");
            }

            Category = category;
            GeometryKind = geometryKind;
            FlowDirection = flowDirection;
            PressureModel = pressureModel;
            DefaultMaterialId = defaultMaterialId?.Trim() ?? string.Empty;
            DefaultZeta = defaultZeta;
            AllowSizeChange = allowSizeChange;
            AllowLength = allowLength;
            AllowRadius = allowRadius;
            AllowBranch = allowBranch;
            SourceElementName = RequireText(
                sourceElementName,
                nameof(sourceElementName));
            SourceSectionPath = sourceSectionPath?.Trim() ?? string.Empty;
        }

        private static string RequireText(
            string? value,
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
