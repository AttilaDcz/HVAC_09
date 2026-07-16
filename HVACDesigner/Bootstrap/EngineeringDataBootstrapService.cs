using System;
using System.Collections.Generic;
using System.IO;
using HVACDesigner.EngineeringData;
using HVACDesigner.EngineeringData.Abstractions;
using HVACDesigner.EngineeringData.Registry;
using HVACDesigner.EngineeringData.Rules;
using HVACDesigner.EngineeringData.Rules.Common;
using HVACDesigner.EngineeringData.SimpleCatalogs;
using HVACDesigner.EngineeringData.BuildingThermal;

namespace HVACDesigner.Bootstrap
{
    /// <summary>
    /// Az alkalmazás EngineeringData XML-világának egyszeri belépési pontja.
    /// A Rule Package bootstrapot külön kezeli, és a legacy providereket
    /// nem távolítja el.
    /// </summary>
    public sealed class EngineeringDataBootstrapService
    {
        private readonly SimpleEngineeringDataLoader _loader;
        private readonly RulePackageBootstrapper _ruleBootstrapper;

        public EngineeringDataBootstrapService()
            : this(
                new SimpleEngineeringDataLoader(),
                new RulePackageBootstrapper())
        {
        }

        public EngineeringDataBootstrapService(
            SimpleEngineeringDataLoader loader,
            RulePackageBootstrapper ruleBootstrapper)
        {
            _loader = loader ??
                throw new ArgumentNullException(nameof(loader));

            _ruleBootstrapper = ruleBootstrapper ??
                throw new ArgumentNullException(nameof(ruleBootstrapper));
        }

        public EngineeringDataBootstrapResult Bootstrap(
            string xmlRootPath)
        {
            if (string.IsNullOrWhiteSpace(xmlRootPath))
                throw new ArgumentException(
                    "Az XML gyökérkönyvtár nem lehet üres.",
                    nameof(xmlRootPath));

            string root = Path.GetFullPath(xmlRootPath);
            var diagnostics = new List<string>();
            var dataRegistry = new EngineeringDataRegistry();
            var ruleRegistry = new EngineeringRuleRegistry();

            if (!Directory.Exists(root))
            {
                diagnostics.Add(
                    "ERROR: A Data/Xml könyvtár nem található: " + root);

                RulePackageBootstrapResult missingRuleResult =
                    _ruleBootstrapper.Bootstrap(
                        ruleRegistry,
                        root,
                        false);

                return new EngineeringDataBootstrapResult(
                    dataRegistry,
                    ruleRegistry,
                    missingRuleResult,
                    diagnostics);
            }

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "catalog-fixtures.xml"),
                _loader.LoadFixtures,
                EngineeringContentKind.ComponentCatalog);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "catalog-materials.xml"),
                _loader.LoadMaterials,
                EngineeringContentKind.ReferenceCatalog);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "catalog-openings.xml"),
                _loader.LoadOpenings,
                EngineeringContentKind.ComponentCatalog);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "catalog-openings.xml"),
                _loader.LoadGlazings,
                EngineeringContentKind.ComponentCatalog);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "catalog-openings.xml"),
                _loader.LoadFrames,
                EngineeringContentKind.ComponentCatalog);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "catalog-openings.xml"),
                _loader.LoadSpacers,
                EngineeringContentKind.ComponentCatalog);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "functions-building.xml"),
                _loader.LoadBuildingFunctions,
                EngineeringContentKind.ReferenceCatalog);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "profiles-building.xml"),
                _loader.LoadBuildingProfiles,
                EngineeringContentKind.ReferenceCatalog);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "mappings-building.xml"),
                _loader.LoadBuildingMappings,
                EngineeringContentKind.ReferenceCatalog);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "design-climate.xml"),
                _loader.LoadClimate,
                EngineeringContentKind.ReferenceCatalog);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "catalog-air-layers.xml"),
                _loader.LoadAirLayers,
                EngineeringContentKind.ReferenceCatalog);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "catalog-construction-templates.xml"),
                _loader.LoadConstructionTemplates,
                EngineeringContentKind.ComponentCatalog);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "rules-building-physics.xml"),
                _loader.LoadBuildingPhysicsRules,
                EngineeringContentKind.RulePackage);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "rules-heating-load.xml"),
                _loader.LoadHeatingLoadRules,
                EngineeringContentKind.RulePackage);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "rules-cooling-load.xml"),
                _loader.LoadCoolingLoadRules,
                EngineeringContentKind.RulePackage);

            LoadAndRegister(
                dataRegistry,
                diagnostics,
                Path.Combine(root, "engineering-dictionary-hu.xml"),
                _loader.LoadDictionary,
                EngineeringContentKind.ReferenceCatalog);

            RulePackageBootstrapResult ruleResult =
                _ruleBootstrapper.Bootstrap(
                    ruleRegistry,
                    root,
                    false);

            diagnostics.Add(
                "INFO: Rule Package bootstrap: " +
                ruleResult.RegisteredRuleSetCount +
                " RuleSet, " +
                ruleResult.RegisteredDesignMethodCount +
                " DesignMethod, " +
                ruleResult.FailedFileCount +
                " hibás fájl.");

            return new EngineeringDataBootstrapResult(
                dataRegistry,
                ruleRegistry,
                ruleResult,
                diagnostics);
        }

        private static void LoadAndRegister<T>(
            EngineeringDataRegistry registry,
            ICollection<string> diagnostics,
            string filePath,
            Func<string, SimpleCatalog<T>> load,
            EngineeringContentKind contentKind)
            where T : class
        {
            try
            {
                SimpleCatalog<T> catalog = load(filePath);

                registry.Register(
                    catalog.Header.Id,
                    catalog.Header.Id,
                    catalog.Header.Version,
                    contentKind,
                    catalog);

                diagnostics.Add(
                    "INFO: Betöltve: " +
                    Path.GetFileName(filePath) +
                    " (" + catalog.Items.Count + " rekord).");
            }
            catch (FileNotFoundException)
            {
                diagnostics.Add(
                    "WARNING: Az opcionális XML nem található: " +
                    Path.GetFileName(filePath) + ".");
            }
            catch (Exception exception)
            {
                diagnostics.Add(
                    "ERROR: " +
                    Path.GetFileName(filePath) +
                    " betöltési hiba: " +
                    exception.Message);
            }
        }
    }
}
