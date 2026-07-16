using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HVACDesigner.EngineeringData.SimpleCatalogs;
using HVACDesigner.EngineeringData.BuildingThermal;
using HVACDesigner.EngineeringData.BuildingThermal.Materials;
using HVACDesigner.EngineeringData.BuildingThermal.AirLayers;
using HVACDesigner.EngineeringData.BuildingThermal.Openings;
using HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates;
using HVACDesigner.EngineeringData.Rules.Common;
using HVACDesigner.EngineeringData.Rules.Infrastructure.Xml;
using HVACDesigner.Features.BuildingThermal;
using HVACDesigner.EngineeringData.Registry;
using HVACDesigner.EngineeringData.Abstractions;
using HVACDesigner.Calculations.Thermal.Common;
using HVACDesigner.Calculations.Thermal.UValue;
using HVACDesigner.Calculations.Thermal.HeatingLoad;
using HVACDesigner.Calculations.Thermal.CoolingLoad;

namespace HVACDesigner.Tests.Integration.Thermal
{
    public static class BuildingThermalDataTests
    {
        public static void Run()
        {
            string baseDirectory = AppContext.BaseDirectory;
            string xmlRootPath = Path.Combine(baseDirectory, "Data", "Xml");

            string materialsPath = Path.Combine(xmlRootPath, "catalog-materials.xml");
            string openingsPath = Path.Combine(xmlRootPath, "catalog-openings.xml");
            string airLayersPath = Path.Combine(xmlRootPath, "catalog-air-layers.xml");
            string templatesPath = Path.Combine(xmlRootPath, "catalog-construction-templates.xml");
            string physicsRulesPath = Path.Combine(xmlRootPath, "rules-building-physics.xml");
            string heatingRulesPath = Path.Combine(xmlRootPath, "rules-heating-load.xml");
            string coolingRulesPath = Path.Combine(xmlRootPath, "rules-cooling-load.xml");
            string climatePath = Path.Combine(xmlRootPath, "design-climate.xml");
            string ekmPath = Path.Combine(xmlRootPath, "rules-ekm.xml");
            string tnmPath = Path.Combine(xmlRootPath, "rules-tnm-legacy.xml");

            var loader = new SimpleEngineeringDataLoader();

            // 1. Alapanyagok beolvasása és ellenőrzése
            SimpleCatalog<MaterialDefinition> materialsCatalog = loader.LoadMaterials(materialsPath);
            if (materialsCatalog.Items.Count == 0)
                throw new InvalidOperationException("Az anyagkatalógus üres.");

            if (!materialsCatalog.Items.TryGetValue("SolidBrick", out MaterialDefinition solidBrick))
                throw new InvalidOperationException("A 'SolidBrick' anyag nem található a katalógusban.");

            if (solidBrick.Lambda != 0.81)
                throw new InvalidOperationException($"A 'SolidBrick' Lambda értéke hibás: {solidBrick.Lambda} (várt: 0.81)");

            // 2. Nyílászárók beolvasása
            SimpleCatalog<OpeningDefinition> openingsCatalog = loader.LoadOpenings(openingsPath);
            if (openingsCatalog.Items.Count == 0)
                throw new InvalidOperationException("A nyílászáró-katalógus üres.");

            SimpleCatalog<GlazingSimpleDefinition> glazingCatalog = loader.LoadGlazings(openingsPath);
            SimpleCatalog<FrameSimpleDefinition> frameCatalog = loader.LoadFrames(openingsPath);
            SimpleCatalog<SpacerSimpleDefinition> spacerCatalog = loader.LoadSpacers(openingsPath);

            // 3. Légrétegek beolvasása (catalog-air-layers.xml)
            SimpleCatalog<AirLayerSimpleDefinition> airLayersCatalog = loader.LoadAirLayers(airLayersPath);
            if (airLayersCatalog.Items.Count == 0)
                throw new InvalidOperationException("A légréteg-katalógus üres.");

            if (!airLayersCatalog.Items.TryGetValue("AirLayer.Vertical.Unventilated.High", out var airLayerHigh))
                throw new InvalidOperationException("Az 'AirLayer.Vertical.Unventilated.High' nem található.");

            if (airLayerHigh.ThermalResistance != 0.18)
                throw new InvalidOperationException($"Légréteg ellenállás hibás: {airLayerHigh.ThermalResistance}");

            // 4. Szerkezeti rétegrendsablonok beolvasása (catalog-construction-templates.xml)
            SimpleCatalog<HVACDesigner.EngineeringData.SimpleCatalogs.ConstructionTemplateDefinition> templatesCatalog = loader.LoadConstructionTemplates(templatesPath);
            if (templatesCatalog.Items.Count == 0)
                throw new InvalidOperationException("A rétegrendsablon-katalógus üres.");

            if (!templatesCatalog.Items.TryGetValue("Wall.External.CeramicBlock.200.EPS.120", out var wallTemplate))
                throw new InvalidOperationException("Nem található a minta ETICS rétegrend sablon.");

            if (wallTemplate.Layers.Count != 4)
                throw new InvalidOperationException($"Hibás rétegszám: {wallTemplate.Layers.Count} (várt: 4)");

            // 5. Épületfizikai szabályok beolvasása (rules-building-physics.xml)
            SimpleCatalog<BuildingPhysicsRulesDefinition> physicsRulesCatalog = loader.LoadBuildingPhysicsRules(physicsRulesPath);
            var physicsDef = physicsRulesCatalog.Items.Values.First();
            if (physicsDef.RsiHorizontal != 0.13 || physicsDef.RseHorizontal != 0.04)
                throw new InvalidOperationException("Hibás felületi hőellenállás értékek a physics szabályokban.");

            // 6. Fűtési hőterhelés szabályok beolvasása (rules-heating-load.xml)
            SimpleCatalog<HeatingLoadRulesDefinition> heatingRulesCatalog = loader.LoadHeatingLoadRules(heatingRulesPath);
            var heatingDef = heatingRulesCatalog.Items.Values.First();
            if (heatingDef.AirVolumetricHeatCapacity != 0.34)
                throw new InvalidOperationException("Hibás levegő hőkapacitás a fűtési szabályokban.");

            if (heatingDef.ThermalBridgeAllowances.Count == 0)
                throw new InvalidOperationException("Nincsenek hőhíd-pótlék szabályok a fűtési szabályokban.");

            // 7. Hűtési hőterhelés szabályok beolvasása (rules-cooling-load.xml)
            SimpleCatalog<CoolingLoadRulesDefinition> coolingRulesCatalog = loader.LoadCoolingLoadRules(coolingRulesPath);
            var coolingDef = coolingRulesCatalog.Items.Values.First();
            if (coolingDef.ShadingFactorExternalLightBlind != 0.25)
                throw new InvalidOperationException("Hibás külső árnyékoló tényező a hűtési szabályokban.");

            // 8. Klímaadatok beolvasása (design-climate.xml) - nyári adatokkal kiterjesztve
            SimpleCatalog<ClimateRegionDefinition> climateCatalog = loader.LoadClimate(climatePath);
            if (!climateCatalog.Items.TryGetValue("HU.Budapest", out var bpc))
                throw new InvalidOperationException("Budapesti klímaadatok nem találhatók.");

            if (bpc.CoolingOutdoorDryBulbC != 34.0 || bpc.SolarSouthWm2 != 420.0)
                throw new InvalidOperationException($"Hibás nyári klímaadatok: DryBulb={bpc.CoolingOutdoorDryBulbC}, South={bpc.SolarSouthWm2}");

            // -------------------------------------------------------------
            // KONVERZIÓK A VALÓDI TARTALMI MODELLEKRE & CONTEXT LÉTREHOZÁSA
            // -------------------------------------------------------------
            var materialsList = materialsCatalog.Items.Values.Select(m =>
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
                materialsCatalog.Header.Id,
                materialsCatalog.Header.Version,
                materialsList);

            var airLayersList = airLayersCatalog.Items.Values.Select(a =>
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
                airLayersCatalog.Header.Id,
                airLayersCatalog.Header.Version,
                airLayersList);

            var openingsList = openingsCatalog.Items.Values.Select(op =>
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
                openingsCatalog.Header.Id,
                openingsCatalog.Header.Version,
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
                coolingDef.LatentHeatPerPersonSedentary,
                coolingDef.LightingPowerDensityOffice,
                coolingDef.LightingPowerDensityResidential,
                coolingDef.EquipmentPowerDensityOffice,
                coolingDef.EquipmentPowerDensityResidential
            );

            var climateDict = new Dictionary<string, DesignClimateRegion>();
            foreach (var c in climateCatalog.Items.Values)
            {
                climateDict.Add(c.Id, new DesignClimateRegion(
                    c.Id,
                    c.DisplayName,
                    c.HeatingOutdoorTemperatureCelsius,
                    c.CoolingOutdoorDryBulbC ?? 34.0,
                    c.CoolingOutdoorWetBulbC ?? 22.0,
                    c.DailyTemperatureRange ?? 12.0,
                    c.SolarSouthWm2 ?? 400.0,
                    c.SolarEastWm2 ?? 300.0,
                    c.SolarWestWm2 ?? 300.0,
                    c.SolarNorthWm2 ?? 100.0,
                    c.SolarHorizontalWm2 ?? 500.0,
                    c.HourlyClimateSeriesId
                ));
            }

            var context = new ThermalCalculationContext(
                realMaterialCatalog,
                realAirLayerCatalog,
                realOpeningCatalog,
                realPhysicsRules,
                realHeatingRules,
                realCoolingRules,
                climateDict
            );

            // -------------------------------------------------------------
            // KALKULÁTOR TESZTELÉSEK
            // -------------------------------------------------------------

            // A. U-Value Calculator Teszt
            var uCalc = new UValueCalculator(context);

            // Készítsünk egy külső fal rétegrend bemenetet (pl. Wall.External.CeramicBlock.200.EPS.120 sablon alapján)
            var wallInput = new UValueInput(
                "TestWall",
                "Teszt Külső Fal",
                ConstructionType.ExternalWall,
                AdjacentBoundaryKind.Outdoor,
                new[] {
                    ConstructionLayerInput.Material("GypsumRender", 0.015),
                    ConstructionLayerInput.Material("CeramicBlock", 0.200),
                    ConstructionLayerInput.Material("CementRender", 0.010),
                    ConstructionLayerInput.Material("EPS", 0.120),
                    ConstructionLayerInput.Material("CementRender", 0.010)
                }
            );

            UValueResult wallUResult = uCalc.Calculate(wallInput);

            // Manuális ellenőrzés:
            // Gipszvakolat: 0.015 / 0.57 = 0.0263
            // Vázkerámia (design lambda = 0.16 * 1.05 = 0.168): 0.200 / 0.168 = 1.1905
            // Ragasztó (Cementvakolat): 0.010 / 1.00 = 0.01
            // EPS (120mm): 0.120 / 0.038 = 3.1579
            // Külső vakolat (Cementvakolat): 0.010 / 1.00 = 0.01
            // Összes réteg ellenállás: kb 4.3947 m²K/W
            // Rsi + Rse = 0.13 + 0.04 = 0.17
            // Total R = 4.5647 m²K/W
            // U = 1 / 4.5647 = ~0.219 W/(m²K)
            if (wallUResult.UValue > 0.25 || wallUResult.UValue < 0.20)
                throw new InvalidOperationException($"UValue számítás hibás: {wallUResult.UValue:F3} (várt: ~0.219)");

            // Mivel 0.219 U-érték kisebb mint 0.25 (homlokzati fal WarningThresholdU), nem szabadna figyelmeztetést kapnia
            if (wallUResult.HasThermalBridgeWarning)
                throw new InvalidOperationException("Hőhíd-figyelmeztetési hiba: Jól szigetelt szerkezetre tévesen riasztott a program.");

            // B. Heating Load Calculator Teszt
            var heatingCalc = new HeatingLoadCalculator(context);

            var elements = new List<BuildingElementHeatInput>
            {
                new BuildingElementHeatInput(ConstructionType.ExternalWall, 25.0, wallUResult.UValue, new ConstructionBoundaryCondition(AdjacentBoundaryKind.Outdoor)),
                new BuildingElementHeatInput(ConstructionType.Ceiling, 20.0, 0.15, new ConstructionBoundaryCondition(AdjacentBoundaryKind.AdjacentUnconditionedSpace, AdjacentSpaceTemperatureMode.TemperatureReductionFactor, "", null, 0.50))
            };

            var roomHeatingInput = new RoomHeatingInput(
                "Room1",
                "Teszt Szoba",
                20.0, // Ti
                -11.0, // Te (Budapest téli tervezési hőmérséklet)
                20.0, // FloorArea
                2.7, // Height
                elements,
                0.5, // Ventilation ACH
                0.05, // Infiltration ACH
                1.0, // Reheat Factor
                0.10 // Thermal bridge allowance 10%
            );

            RoomHeatingResult roomHeatingResult = heatingCalc.CalculateRoom(roomHeatingInput);

            // Transzmissziós hőveszteség ellenőrzése:
            // Wall: 0.237 * 25.0 * 31.0 = 183.7 W
            // Ceiling: 0.15 * 20.0 * 31.0 * 0.50 (b_u) = 46.5 W
            // Sum Transmission = 230.2 W
            // Hőhíd pótlék (10%): 23.0 W
            // Szellőzési veszteség: 0.34 * 0.5 * 54.0 * 31.0 = 284.6 W
            // Total Design Load = 230.2 + 23.0 + 284.6 = 537.8 W
            if (roomHeatingResult.TotalDesignHeatLoadW < 500.0 || roomHeatingResult.TotalDesignHeatLoadW > 560.0)
                throw new InvalidOperationException($"Fűtési hőterhelés számítás hibás: {roomHeatingResult.TotalDesignHeatLoadW:F1} W");

            // C. Simple Peak Cooling Load Calculator Teszt
            var coolingCalc = new SimplePeakCoolingLoadCalculator(context);

            var opaqueCooling = new[]
            {
                new OpaqueCoolingElementInput("WallS", 25.0, wallUResult.UValue, CardinalDirection.South)
            };
            var glazedCooling = new[]
            {
                new GlazingCoolingInput("WindowS", 4.0, 1.2, 0.60, CardinalDirection.South, ShadingType.InternalLightBlind)
            };

            var roomCoolingInput = new RoomCoolingInput(
                "Room1",
                "Teszt Szoba",
                "HU.Budapest",
                24.0, // Ti nyáron
                20.0, // Area
                2.7, // Height
                opaqueCooling,
                glazedCooling,
                2, // Persons
                100.0, // Lighting W
                150.0, // Equipment W
                1.0, // Ventilation ACH
                0.85 // Concurrency Factor
            );

            RoomCoolingResult roomCoolingResult = coolingCalc.Calculate(roomCoolingInput);

            // Napbesugárzás: South = 420 W/m²
            // Window: 4.0 m² * 420 * 0.60 (SHGC) * 0.75 (internal blind shading) = 756 W
            // Személyek hőleadása: 2 * 0.85 * 75 = 127.5 W (Sensible), 2 * 0.85 * 55 = 93.5 W (Latent)
            // Transmission (Text = 34.0, Ti = 24.0, deltaT = 10.0):
            // Wall: 0.237 * 25.0 * 10.0 = 59.25 W
            // Window: 1.2 * 4.0 * 10.0 = 48.0 W
            // Ventilation: 0.34 * 1.0 * 54.0 * 10.0 = 183.6 W
            // Total Sensible = 756 (solar) + 107.25 (trans) + 127.5 (people) + 100 (lighting) + 150 (equip) + 183.6 (vent) = 1424.35 W
            // Total Cooling Load = 1424.35 (Sensible) + 93.5 (Latent) = 1517.85 W
            if (roomCoolingResult.TotalCoolingLoadW < 1450.0 || roomCoolingResult.TotalCoolingLoadW > 1580.0)
                throw new InvalidOperationException($"Hűtési hőterhelés számítás hibás: {roomCoolingResult.TotalCoolingLoadW:F1} W");

            // -------------------------------------------------------------
            // MEGLÉVŐ SZABÁLYOK ELLENŐRZÉSE (Eredeti teszt)
            // -------------------------------------------------------------
            var legacyRuleRegistry = new EngineeringRuleRegistry();
            var ruleLoader = new XmlRulePackageLoader();

            RulePackageLoadResult ekmRules = ruleLoader.Load(ekmPath);
            foreach (var ruleSet in ekmRules.RuleSets)
                legacyRuleRegistry.RegisterRuleSet(ruleSet);
            foreach (var method in ekmRules.DesignMethods)
                legacyRuleRegistry.RegisterDesignMethod(method);

            RulePackageLoadResult tnmRules = ruleLoader.Load(tnmPath);
            foreach (var ruleSet in tnmRules.RuleSets)
                legacyRuleRegistry.RegisterRuleSet(ruleSet);
            foreach (var method in tnmRules.DesignMethods)
                legacyRuleRegistry.RegisterDesignMethod(method);

            var resolver = new BuildingThermalRuleResolver();

            // ÉKM szabályfeloldás ellenőrzése
            BuildingThermalResolvedRules resolvedEkm = resolver.Resolve(legacyRuleRegistry, "Hungary.EKM");
            if (resolvedEkm.ExternalWallMaxU != 0.20)
                throw new InvalidOperationException($"ExternalWallMaxU under EKM is {resolvedEkm.ExternalWallMaxU} (expected: 0.20)");
            if (resolvedEkm.FlatRoofMaxU != 0.17)
                throw new InvalidOperationException($"FlatRoofMaxU under EKM is {resolvedEkm.FlatRoofMaxU} (expected: 0.17)");
            if (resolvedEkm.WindowMaxU != 1.10)
                throw new InvalidOperationException($"WindowMaxU under EKM is {resolvedEkm.WindowMaxU} (expected: 1.10)");

            // TNM szabályfeloldás ellenőrzése
            BuildingThermalResolvedRules resolvedTnm = resolver.Resolve(legacyRuleRegistry, "Hungary.TNM.Legacy");
            if (resolvedTnm.ExternalWallMaxU != 0.24)
                throw new InvalidOperationException($"ExternalWallMaxU under TNM is {resolvedTnm.ExternalWallMaxU} (expected: 0.24)");
            if (resolvedTnm.WindowMaxU != 1.15)
                throw new InvalidOperationException($"WindowMaxU under TNM is {resolvedTnm.WindowMaxU} (expected: 1.15)");

            // 9. UValueCalculationService tesztelése
            var testDataRegistry = new EngineeringDataRegistry();
            testDataRegistry.Register("Package", "Catalog.Materials", "1.0", EngineeringContentKind.ReferenceCatalog, materialsCatalog);
            testDataRegistry.Register("Package", "Catalog.AirLayers", "1.0", EngineeringContentKind.ReferenceCatalog, airLayersCatalog);
            testDataRegistry.Register("Package", "Catalog.Openings", "1.0", EngineeringContentKind.ComponentCatalog, openingsCatalog);
            testDataRegistry.Register("Package", "Catalog.Openings", "1.0", EngineeringContentKind.ComponentCatalog, glazingCatalog);
            testDataRegistry.Register("Package", "Catalog.Openings", "1.0", EngineeringContentKind.ComponentCatalog, frameCatalog);
            testDataRegistry.Register("Package", "Catalog.Openings", "1.0", EngineeringContentKind.ComponentCatalog, spacerCatalog);
            testDataRegistry.Register("Package", "HU.BuildingPhysics", "1.0", EngineeringContentKind.RulePackage, physicsRulesCatalog);
            testDataRegistry.Register("Package", "HU.HeatingLoad", "1.0", EngineeringContentKind.RulePackage, heatingRulesCatalog);
            testDataRegistry.Register("Package", "HU.CoolingLoad", "1.0", EngineeringContentKind.RulePackage, coolingRulesCatalog);

            var calculationService = new UValueCalculationService();
            calculationService.Initialize(testDataRegistry);

            // Teszteljük a réteges szerkezetet
            var testStructure = new UserStructure
            {
                Id = "TestWall",
                Name = "Teszt homlokzat",
                Type = ConstructionType.ExternalWall,
                IsOpening = false,
                ThermalBridgeCorrectionFactor = 0.15 // 15%
            };
            testStructure.Layers.Add(new UserLayer { ReferenceId = "SolidBrick", ThicknessM = 0.38, DisplayName = "Tömör tégla", IsAirLayer = false, DesignLambda = 0.81 });

            var calcResult = calculationService.Calculate(testStructure, legacyRuleRegistry, "Hungary.EKM");
            if (!calcResult.Succeeded)
                throw new InvalidOperationException("A réteges számítás nem sikerült.");

            var opaqueResult = calcResult.Result as OpaqueConstructionUValueResult;
            if (opaqueResult == null)
                throw new InvalidOperationException("A réteges számítás eredménye nem OpaqueConstructionUValueResult.");

            // Alap U-érték: R_si + R_se + 0.38 / 0.81 = 0.13 + 0.04 + 0.469 = 0.639 => U = 1 / 0.639 = 1.564 W/m²K
            // Multiplikatív korrekcióval (1.564 * 1.15) = 1.799 W/m²K
            if (Math.Abs(opaqueResult.BaseUValue - 1.564) > 0.05)
                throw new InvalidOperationException($"Hibás alap U-érték: {opaqueResult.BaseUValue:F3} (várt: ~1.564)");
            if (Math.Abs(opaqueResult.UValue - 1.799) > 0.05)
                throw new InvalidOperationException($"Hibás korrigált U-érték: {opaqueResult.UValue:F3} (várt: ~1.799)");

            // ÉKM határérték ellenőrzés (Külső fal limit: 0.20)
            if (opaqueResult.EkmLimit != 0.20)
                throw new InvalidOperationException($"Hibás ÉKM limit külső falra: {opaqueResult.EkmLimit} (várt: 0.20)");
            if (!opaqueResult.ExceedsEkmLimit)
                throw new InvalidOperationException("A szerkezetnek meg kellene haladnia az ÉKM határértéket.");

            // Nyílászáró katalógusos teszt
            var openingCatalogStruct = new UserStructure
            {
                Id = "TestOpeningCatalog",
                Name = "Katalógus Ablak",
                IsOpening = true,
                IsOpeningCalculated = false,
                OpeningCatalogId = "Window.Modern.Wood.Triple",
                OpeningType = "Window" // Típus
            };
            var opCatalogResult = calculationService.Calculate(openingCatalogStruct, legacyRuleRegistry, "Hungary.EKM");
            if (!opCatalogResult.Succeeded)
                throw new InvalidOperationException("A nyílászáró katalógusos számítás nem sikerült.");

            var openingResult = opCatalogResult.Result as OpeningUValueResult;
            if (openingResult == null)
                throw new InvalidOperationException("A nyílászáró számítás eredménye nem OpeningUValueResult.");

            // Ellenőrizzük, hogy a beolvasott ablaknak megfelelő határértéke és U-értéke van
            // Az EKM szerinti ablak limit: 1.10 W/m²K
            if (openingResult.EkmLimit != 1.10)
                throw new InvalidOperationException($"Hibás ÉKM limit ablakra: {openingResult.EkmLimit} (várt: 1.10)");

            // Részletes számítású ablak tesztelése
            var detailedOpeningStruct = new UserStructure
            {
                Id = "TestOpeningDetailed",
                Name = "Részletes Ablak",
                IsOpening = true,
                IsOpeningCalculated = true,
                OpeningType = "Window",
                GlazingId = "Glazing.Double",
                FrameId = "Frame.Pvc.Modern",
                SpacerId = "Spacer.Aluminium",
                FrameWidthMm = 80,
                OpeningWidthM = 1.23,
                OpeningHeightM = 1.48
            };
            var detailedOpResult = calculationService.Calculate(detailedOpeningStruct, legacyRuleRegistry, "Hungary.EKM");
            if (!detailedOpResult.Succeeded)
                throw new InvalidOperationException("A részletes nyílászáró számítás nem sikerült.");

            var detailedOpeningResult = detailedOpResult.Result as OpeningUValueResult;
            if (detailedOpeningResult == null)
                throw new InvalidOperationException("A részletes nyílászáró számítás eredménye nem OpeningUValueResult.");

            if (detailedOpeningResult.UValue <= 0 || detailedOpeningResult.UValue >= 3.0)
                throw new InvalidOperationException($"Reális határokon kívüli számított ablak U-érték: {detailedOpeningResult.UValue}");

            Console.WriteLine("Minden épületfizikai és szabályozási adatbázis-, resolver- és kalkulátorteszt sikeresen lefutott.");
        }
    }
}
