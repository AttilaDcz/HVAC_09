using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HVACDesigner.Calculations.Common;
using HVACDesigner.Calculations.Thermal.Common;
using HVACDesigner.Calculations.Thermal.UValue;
using HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates;
using HVACDesigner.EngineeringData.BuildingThermal.Materials;
using HVACDesigner.EngineeringData.BuildingThermal.AirLayers;
using HVACDesigner.EngineeringData.BuildingThermal.Openings;
using HVACDesigner.EngineeringData;
using HVACDesigner.EngineeringData.Registry;
using HVACDesigner.EngineeringData.Rules.Common;
using HVACDesigner.EngineeringData.SimpleCatalogs;
using HVACDesigner.Services;

namespace HVACDesigner.Features.BuildingThermal
{
    public sealed class UValueCalculationService
    {
        private UValueCalculator _uCalculator;
        private readonly BuildingOpeningCalculator _openingCalculator = new BuildingOpeningCalculator();

        private SimpleCatalog<MaterialDefinition> _materialCatalog;
        private SimpleCatalog<AirLayerSimpleDefinition> _airLayerCatalog;
        private SimpleCatalog<OpeningDefinition> _openingsCatalog;
        private SimpleCatalog<GlazingSimpleDefinition> _glazingCatalog;
        private SimpleCatalog<FrameSimpleDefinition> _frameCatalog;
        private SimpleCatalog<SpacerSimpleDefinition> _spacerCatalog;

        private readonly List<GlazingDefinition> _glazings = new List<GlazingDefinition>();
        private readonly List<FrameDefinition> _frames = new List<FrameDefinition>();
        private readonly List<SpacerDefinition> _spacers = new List<SpacerDefinition>();

        public IReadOnlyList<GlazingDefinition> Glazings => _glazings;
        public IReadOnlyList<FrameDefinition> Frames => _frames;
        public IReadOnlyList<SpacerDefinition> Spacers => _spacers;
        public SimpleCatalog<OpeningDefinition> OpeningsCatalog => _openingsCatalog;

        public void Initialize(EngineeringDataRegistry dataRegistry)
        {
            _materialCatalog = dataRegistry.GetRequired<SimpleCatalog<MaterialDefinition>>("Catalog.Materials", "1.0");
            _airLayerCatalog = dataRegistry.GetRequired<SimpleCatalog<AirLayerSimpleDefinition>>("Catalog.AirLayers", "1.0");
            _openingsCatalog = dataRegistry.GetRequired<SimpleCatalog<OpeningDefinition>>("Catalog.Openings", "1.0");
            _glazingCatalog = dataRegistry.GetRequired<SimpleCatalog<GlazingSimpleDefinition>>("Catalog.Openings", "1.0");
            _frameCatalog = dataRegistry.GetRequired<SimpleCatalog<FrameSimpleDefinition>>("Catalog.Openings", "1.0");
            _spacerCatalog = dataRegistry.GetRequired<SimpleCatalog<SpacerSimpleDefinition>>("Catalog.Openings", "1.0");

            var physicsDef = dataRegistry.GetRequired<SimpleCatalog<BuildingPhysicsRulesDefinition>>("HU.BuildingPhysics", "1.0").Items.Values.First();
            var heatingDef = dataRegistry.GetRequired<SimpleCatalog<HeatingLoadRulesDefinition>>("HU.HeatingLoad", "1.0").Items.Values.First();
            var coolingDef = dataRegistry.GetRequired<SimpleCatalog<CoolingLoadRulesDefinition>>("HU.CoolingLoad", "1.0").Items.Values.First();

            // Nyílászáró összetevők áttöltése a kalkulátor domain modelljeibe
            _glazings.Clear();
            foreach (var g in _glazingCatalog.Items.Values)
            {
                _glazings.Add(new GlazingDefinition(g.Id, g.DisplayName, g.Ug, g.SolarTransmittance, null, g.PaneCount, g.GasFill, g.CoatingType));
            }

            _frames.Clear();
            foreach (var f in _frameCatalog.Items.Values)
            {
                Enum.TryParse<FrameMaterialKind>(f.MaterialKind, true, out var mk);
                _frames.Add(new FrameDefinition(f.Id, f.DisplayName, f.Uf, mk, f.ProfileDepth, f.ChamberCount, f.DefaultWidth));
            }

            _spacers.Clear();
            foreach (var s in _spacerCatalog.Items.Values)
            {
                _spacers.Add(new SpacerDefinition(s.Id, s.DisplayName, s.Psi, s.SpacerType));
            }

            // Kalkulátor inicializáció
            var materialsList = _materialCatalog.Items.Values.Select(m =>
                new BuildingMaterialDefinition(
                    m.Id,
                    m.DisplayName,
                    m.Category,
                    m.Lambda,
                    m.LambdaCorrection ?? 1.0,
                    m.Density ?? 0.0,
                    m.SpecificHeat ?? 0.0,
                    m.Mu ?? 0.0,
                    BuildingMaterialValueSource.Unspecified,
                    BuildingMaterialMoistureCondition.Dry,
                    "",
                    m.DisplayName,
                    "Package",
                    "Materials",
                    "1.0"
                )).ToList();
            var realMaterialCatalog = new BuildingMaterialCatalog(
                _materialCatalog.Header.Id,
                _materialCatalog.Header.Version,
                materialsList);

            var airLayersList = _airLayerCatalog.Items.Values.Select(a =>
                new AirLayerDefinition(
                    a.Id,
                    a.DisplayName,
                    a.ThermalResistance,
                    Enum.TryParse<AirLayerOrientation>(a.Orientation, true, out var o) ? o : AirLayerOrientation.Unspecified,
                    Enum.TryParse<AirLayerVentilationLevel>(a.VentilationLevel, true, out var v) ? v : AirLayerVentilationLevel.Unspecified,
                    "Package",
                    "AirLayers",
                    "1.0",
                    new Dictionary<string, string>()
                )).ToList();
            var realAirLayerCatalog = new AirLayerCatalog(
                _airLayerCatalog.Header.Id,
                _airLayerCatalog.Header.Version,
                airLayersList);

            var openingsList = _openingsCatalog.Items.Values.Select(op =>
                new BuildingOpeningDefinition(
                    op.Id,
                    op.DisplayName,
                    BuildingOpeningType.Window,
                    BuildingOpeningCalculationMode.DirectValue,
                    op.UValue,
                    op.GValue,
                    "",
                    "",
                    "",
                    null,
                    ""
                )).ToList();
            var realOpeningCatalog = new BuildingOpeningCatalog(
                _openingsCatalog.Header.Id,
                _openingsCatalog.Header.Version,
                Enumerable.Empty<GlazingDefinition>(),
                Enumerable.Empty<FrameDefinition>(),
                Enumerable.Empty<SpacerDefinition>(),
                openingsList);

            var realPhysicsRules = new BuildingPhysicsRules(
                physicsDef.RsiHorizontal,
                physicsDef.RseHorizontal,
                physicsDef.RsiUpward,
                physicsDef.RseUpward,
                physicsDef.RsiDownward,
                physicsDef.RseDownward,
                physicsDef.DefaultThermalBridgeAllowance
            );

            var allowancesList = heatingDef.ThermalBridgeAllowances.Select(a =>
                new ThermalBridgeAllowanceRule(
                    Enum.TryParse<ConstructionType>(a.ConstructionType, true, out var ct) ? ct : ConstructionType.Custom,
                    a.InsulationLevel,
                    a.RecommendedAllowance,
                    a.DefaultAllowance,
                    a.WarningThresholdU,
                    a.Note
                )).ToList();
            var realHeatingRules = new HeatingLoadRules(
                heatingDef.AirVolumetricHeatCapacity,
                heatingDef.InfiltrationAchNew,
                heatingDef.InfiltrationAchRecent,
                heatingDef.InfiltrationAchLegacy,
                heatingDef.ReheatFactorResidential8h,
                heatingDef.ReheatFactorOffice12h,
                heatingDef.ReheatFactorSchoolWeekend,
                allowancesList
            );

            var realCoolingRules = new CoolingLoadRules(
                coolingDef.AirVolumetricHeatCapacity,
                coolingDef.ShadingFactorNone,
                coolingDef.ShadingFactorInternalLightBlind,
                coolingDef.ShadingFactorInternalDarkBlind,
                coolingDef.ShadingFactorExternalLightBlind,
                coolingDef.ShadingFactorExternalDarkBlind,
                coolingDef.ShadingFactorExternalOverhang,
                coolingDef.ConcurrencyFactorResidential,
                coolingDef.ConcurrencyFactorOffice,
                coolingDef.ConcurrencyFactorSchool,
                coolingDef.ConcurrencyFactorRetail,
                coolingDef.SensibleHeatPerPersonSedentary,
                coolingDef.SensibleHeatPerPersonSedentary,
                coolingDef.LightingPowerDensityOffice,
                coolingDef.LightingPowerDensityResidential,
                coolingDef.EquipmentPowerDensityOffice,
                coolingDef.EquipmentPowerDensityResidential
            );

            var context = new ThermalCalculationContext(
                realMaterialCatalog,
                realAirLayerCatalog,
                realOpeningCatalog,
                realPhysicsRules,
                realHeatingRules,
                realCoolingRules,
                new Dictionary<string, DesignClimateRegion>()
            );
            _uCalculator = new UValueCalculator(context);
        }

        public CalculationResult<IThermalTransmittanceResult> Calculate(
            UserStructure structure,
            EngineeringRuleRegistry ruleRegistry,
            string methodId)
        {
            if (structure == null)
                throw new ArgumentNullException(nameof(structure));
            if (ruleRegistry == null)
                throw new ArgumentNullException(nameof(ruleRegistry));

            var builder = new CalculationResultBuilder<IThermalTransmittanceResult>();
            builder.AddInput("StructureId", structure.Id);
            builder.AddInput("StructureName", structure.Name);
            builder.AddInput("IsOpening", structure.IsOpening.ToString());
            builder.AddInput("MethodId", methodId);

            // 1. Szabálycsomag lekérése
            string ruleSetId = string.Equals(methodId, "Hungary.TNM.Legacy", StringComparison.OrdinalIgnoreCase)
                ? "HU.TNM.Legacy.Envelope"
                : "HU.EKM.Envelope";
            string ruleSetVersion = string.Equals(methodId, "Hungary.TNM.Legacy", StringComparison.OrdinalIgnoreCase)
                ? "2023-10-31"
                : "2023.1";

            RuleSetDescriptor envelope = null;
            try
            {
                envelope = ruleRegistry.GetRequiredRuleSet(ruleSetId, ruleSetVersion);
                builder.UseRuleSet($"{ruleSetId}@{ruleSetVersion}");
            }
            catch (Exception)
            {
                builder.AddDiagnostic(new CalculationDiagnostic(
                    "MissingRequirementRule",
                    $"A követelményszabályok ({ruleSetId} verzió: {ruleSetVersion}) nem találhatóak a rendszerben.",
                    CalculationDiagnosticSeverity.Error));
                return builder.BuildWaitingForInput(null);
            }

            // 2. Határérték feloldó helper
            double? GetLimitValue(string paramName)
            {
                string val = envelope.Parameters.GetStringOrDefault(paramName, string.Empty);
                if (string.IsNullOrWhiteSpace(val))
                {
                    builder.AddDiagnostic(new CalculationDiagnostic(
                        "MissingRequirementRule",
                        $"A szabályok közül hiányzik a(z) '{paramName}' követelményérték.",
                        CalculationDiagnosticSeverity.Error));
                    return null;
                }

                if (double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out double limit))
                {
                    return limit;
                }

                builder.AddDiagnostic(new CalculationDiagnostic(
                    "MissingRequirementRule",
                    $"A(z) '{paramName}' követelményérték ({val}) formátuma érvénytelen.",
                    CalculationDiagnosticSeverity.Error));
                return null;
            }

            // 3. Számítás végrehajtása
            if (structure.IsOpening)
            {
                // A. NYÍLÁSZÁRÓ SZÁMÍTÁS
                double? limit = null;
                string opTypeStr = structure.OpeningType;

                // Ha katalógusos megadás, az XML elemből olvassuk a típust
                if (!structure.IsOpeningCalculated)
                {
                    var opDef = _openingsCatalog.Items.Values.FirstOrDefault(o => o.Id == structure.OpeningCatalogId);
                    if (opDef != null && !string.IsNullOrEmpty(opDef.Type))
                    {
                        opTypeStr = opDef.Type;
                    }
                }

                // Határérték kiválasztása típus alapján
                string limitParamName;
                if (string.Equals(opTypeStr, "EntranceDoor", StringComparison.OrdinalIgnoreCase))
                {
                    limitParamName = "EntranceDoor.MaximumU";
                }
                else if (string.Equals(opTypeStr, "GarageGate", StringComparison.OrdinalIgnoreCase) || 
                         string.Equals(opTypeStr, "IndustrialGate", StringComparison.OrdinalIgnoreCase))
                {
                    limitParamName = "IndustrialGate.MaximumU";
                }
                else // Window, TerraceDoor vagy default
                {
                    limitParamName = "Window.MaximumU";
                }

                limit = GetLimitValue(limitParamName);
                if (!limit.HasValue)
                {
                    return builder.BuildWaitingForInput(null);
                }

                if (structure.IsOpeningCalculated)
                {
                    // Részletes számítás
                    if (structure.OpeningWidthM <= 0 || structure.OpeningHeightM <= 0 || structure.FrameWidthMm <= 0)
                    {
                        builder.AddDiagnostic(new CalculationDiagnostic(
                            "InvalidOpeningGeometry",
                            "A nyílászáró szélességének, magasságának és keretszélességének pozitívnak kell lennie.",
                            CalculationDiagnosticSeverity.Error));
                        return builder.BuildWaitingForInput(null);
                    }

                    var glazing = _glazings.FirstOrDefault(g => g.Id == structure.GlazingId);
                    var frame = _frames.FirstOrDefault(f => f.Id == structure.FrameId);
                    var spacer = _spacers.FirstOrDefault(s => s.Id == structure.SpacerId);

                    if (glazing == null || frame == null || spacer == null)
                    {
                        builder.AddDiagnostic(new CalculationDiagnostic(
                            "MissingOpeningComponent",
                            "A részletes számításhoz szükséges üvegezés, keret profil vagy távtartó elem nem található.",
                            CalculationDiagnosticSeverity.Error));
                        return builder.BuildWaitingForInput(null);
                    }

                    try
                    {
                        var input = new BuildingOpeningCalculationInput
                        {
                            Width = structure.OpeningWidthM,
                            Height = structure.OpeningHeightM,
                            FrameVisibleWidth = structure.FrameWidthMm / 1000.0,
                            Glazing = glazing,
                            Frame = frame,
                            Spacer = spacer
                        };

                        var calcResult = _openingCalculator.Calculate(input);
                        
                        var result = new OpeningUValueResult(
                            structure.Id,
                            structure.Name,
                            calcResult.UValue,
                            limit.Value,
                            false,
                            "",
                            structure.OpeningWidthM,
                            structure.OpeningHeightM,
                            structure.FrameWidthMm / 1000.0
                        );

                        return builder.Build(result);
                    }
                    catch (Exception ex)
                    {
                        builder.AddDiagnostic(new CalculationDiagnostic(
                            "CalculationFailed",
                            $"A nyílászáró részletes U-érték számítása sikertelen: {ex.Message}",
                            CalculationDiagnosticSeverity.Error));
                        return builder.BuildWaitingForInput(null);
                    }
                }
                else
                {
                    // Katalógusból megadás
                    var opDef = _openingsCatalog.Items.Values.FirstOrDefault(o => o.Id == structure.OpeningCatalogId);
                    if (opDef == null)
                    {
                        builder.AddDiagnostic(new CalculationDiagnostic(
                            "MissingOpeningComponent",
                            $"A(z) '{structure.OpeningCatalogId}' azonosítójú katalógus-nyílászáró nem található.",
                            CalculationDiagnosticSeverity.Error));
                        return builder.BuildWaitingForInput(null);
                    }

                    var result = new OpeningUValueResult(
                        structure.Id,
                        structure.Name,
                        opDef.UValue,
                        limit.Value,
                        false,
                        "",
                        1.23, // Fix gyári referencia méretek az adatbázis értékhez
                        1.48,
                        0.08
                    );

                    return builder.Build(result);
                }
            }
            else
            {
                // B. ÁTLÁTSZATLAN SZERKEZET SZÁMÍTÁSA
                if (structure.Layers.Count == 0)
                {
                    builder.AddDiagnostic(new CalculationDiagnostic(
                        "InvalidLayerThickness",
                        "A szerkezet nem tartalmaz rétegeket.",
                        CalculationDiagnosticSeverity.Error));
                    return builder.BuildWaitingForInput(null);
                }

                // Határfeltétel feloldása szerkezettípus alapján
                AdjacentBoundaryKind boundary = AdjacentBoundaryKind.Outdoor;
                switch (structure.Type)
                {
                    case ConstructionType.ExternalWall:
                    case ConstructionType.BasementWall:
                        boundary = AdjacentBoundaryKind.Outdoor;
                        break;
                    case ConstructionType.Roof:
                        boundary = AdjacentBoundaryKind.Outdoor;
                        break;
                    case ConstructionType.Ceiling:
                        boundary = AdjacentBoundaryKind.AdjacentUnconditionedSpace;
                        break;
                    case ConstructionType.GroundFloor:
                        boundary = AdjacentBoundaryKind.Ground;
                        break;
                    case ConstructionType.InternalWall:
                        boundary = AdjacentBoundaryKind.AdjacentConditionedZone;
                        break;
                    default:
                        boundary = AdjacentBoundaryKind.Outdoor;
                        break;
                }

                // Határérték lekérése szerkezettípus alapján
                string limitParamName;
                switch (structure.Type)
                {
                    case ConstructionType.ExternalWall:
                    case ConstructionType.BasementWall:
                        limitParamName = "ExternalWall.MaximumU";
                        break;
                    case ConstructionType.Roof:
                        limitParamName = "FlatRoof.MaximumU";
                        break;
                    case ConstructionType.Ceiling:
                        limitParamName = "FlatRoof.MaximumU";
                        break;
                    case ConstructionType.GroundFloor:
                        limitParamName = "FloorOnGround.MaximumU";
                        break;
                    case ConstructionType.InternalWall:
                        limitParamName = "SeparatingWall.MaximumU";
                        break;
                    default:
                        limitParamName = "ExternalWall.MaximumU";
                        break;
                }

                double? limit = GetLimitValue(limitParamName);
                if (!limit.HasValue)
                {
                    return builder.BuildWaitingForInput(null);
                }

                var calcLayers = new List<ConstructionLayerInput>();
                for (int i = 0; i < structure.Layers.Count; i++)
                {
                    var layer = structure.Layers[i];
                    if (layer.ThicknessM <= 0 && !layer.IsAirLayer)
                    {
                        builder.AddDiagnostic(new CalculationDiagnostic(
                            "InvalidLayerThickness",
                            $"A(z) '{layer.DisplayName}' réteg vastagsága ({layer.ThicknessM} m) érvénytelen.",
                            CalculationDiagnosticSeverity.Error));
                        return builder.BuildWaitingForInput(null);
                    }
                    if (layer.DesignLambda <= 0)
                    {
                        builder.AddDiagnostic(new CalculationDiagnostic(
                            "InvalidLambda",
                            $"A(z) '{layer.DisplayName}' réteg hővezetési tényezője ({layer.DesignLambda}) érvénytelen.",
                            CalculationDiagnosticSeverity.Error));
                        return builder.BuildWaitingForInput(null);
                    }

                    if (layer.IsAirLayer)
                    {
                        calcLayers.Add(ConstructionLayerInput.AirLayer(layer.ReferenceId, layer.DisplayName));
                    }
                    else
                    {
                        calcLayers.Add(ConstructionLayerInput.Material(layer.ReferenceId, layer.ThicknessM, layer.DisplayName));
                    }
                }

                try
                {
                    var input = new UValueInput(
                        structure.Id,
                        structure.Name,
                        structure.Type,
                        boundary,
                        calcLayers
                    );

                    var calcResult = _uCalculator.Calculate(input);

                    var result = new OpaqueConstructionUValueResult(
                        structure.Id,
                        structure.Name,
                        calcResult.UValue,
                        structure.ThermalBridgeCorrectionFactor,
                        limit.Value,
                        calcResult.HasThermalBridgeWarning,
                        calcResult.ThermalBridgeWarning,
                        calcResult.Layers
                    );

                    return builder.Build(result);
                }
                catch (Exception ex)
                {
                    builder.AddDiagnostic(new CalculationDiagnostic(
                        "CalculationFailed",
                        $"Az U-érték számítása sikertelen: {ex.Message}",
                        CalculationDiagnosticSeverity.Error));
                    return builder.BuildWaitingForInput(null);
                }
            }
        }
    }
}
