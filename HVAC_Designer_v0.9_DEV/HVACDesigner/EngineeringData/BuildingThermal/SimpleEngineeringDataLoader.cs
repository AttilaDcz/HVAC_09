using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using HVACDesigner.EngineeringData.SimpleCatalogs;

namespace HVACDesigner.EngineeringData.BuildingThermal
{
    /// <summary>
    /// Vékony, típusos loader az egyszerű EngineeringData XML-ekhez.
    /// Nem használ DTO/Mapper réteget, és nem végez kalkulációt.
    /// </summary>
    public sealed class SimpleEngineeringDataLoader
    {
        public SimpleCatalog<FixtureDefinition> LoadFixtures(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = RequiredElement(document.Root, "Fixtures");

            return new SimpleCatalog<FixtureDefinition>(
                header,
                root.Elements("Fixture").Select(item =>
                    new FixtureDefinition(
                        Required(item, "Id"),
                        Required(item, "DisplayName"),
                        Optional(item, "Category"),
                        OptionalDouble(item, "PotableLoadingUnit"),
                        OptionalDouble(item, "WastewaterDU"),
                        OptionalInt(item, "MinimumWasteDN"),
                        OptionalBool(item, "HotWaterRelevant"),
                        Optional(item, "GreywaterSource"),
                        OptionalBool(item, "GreywaterDemand"))),
                item => item.Id);
        }

        public SimpleCatalog<MaterialDefinition> LoadMaterials(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = RequiredElement(document.Root, "Materials");

            var materials = new List<MaterialDefinition>();

            // 1. Lapos Material elemek beolvasása közvetlenül a Materials alatt
            foreach (XElement item in root.Elements("Material"))
            {
                materials.Add(new MaterialDefinition(
                    Required(item, "Id"),
                    Required(item, "DisplayName"),
                    RequiredDouble(item, "Lambda"),
                    OptionalDouble(item, "Density"),
                    Optional(item, "Category"),
                    OptionalDouble(item, "Mu"),
                    OptionalDouble(item, "SpecificHeat"),
                    OptionalDouble(item, "LambdaCorrection")));
            }

            // 2. Category tag-be ágyazott Material elemek beolvasása
            foreach (XElement categoryEl in root.Elements("Category"))
            {
                string categoryName = categoryEl.Attribute("Name")?.Value ?? string.Empty;
                foreach (XElement item in categoryEl.Elements("Material"))
                {
                    materials.Add(new MaterialDefinition(
                        Required(item, "Id"),
                        Required(item, "DisplayName"),
                        RequiredDouble(item, "Lambda"),
                        OptionalDouble(item, "Density"),
                        categoryName,
                        OptionalDouble(item, "Mu"),
                        OptionalDouble(item, "SpecificHeat"),
                        OptionalDouble(item, "LambdaCorrection")));
                }
            }

            return new SimpleCatalog<MaterialDefinition>(
                header,
                materials,
                item => item.Id);
        }

        public SimpleCatalog<OpeningDefinition> LoadOpenings(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = RequiredElement(document.Root, "Openings");

            return new SimpleCatalog<OpeningDefinition>(
                header,
                root.Elements("Opening").Select(item =>
                    new OpeningDefinition(
                        Required(item, "Id"),
                        Required(item, "DisplayName"),
                        Required(item, "Type"),
                        RequiredDouble(item, "UValue"),
                        OptionalDouble(item, "GValue"))),
                item => item.Id);
        }

        public SimpleCatalog<BuildingFunctionDefinition> LoadBuildingFunctions(
            string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = RequiredElement(document.Root, "BuildingFunctions");

            return new SimpleCatalog<BuildingFunctionDefinition>(
                header,
                root.Elements("Function").Select(item =>
                    new BuildingFunctionDefinition(
                        Required(item, "Id"),
                        Required(item, "DisplayName"),
                        Optional(item, "Category"))),
                item => item.Id);
        }

        public SimpleCatalog<BuildingProfileDefinition> LoadBuildingProfiles(
            string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = RequiredElement(document.Root, "Profiles");

            return new SimpleCatalog<BuildingProfileDefinition>(
                header,
                root.Elements("Profile").Select(profile =>
                    new BuildingProfileDefinition(
                        Required(profile, "Id"),
                        Required(profile, "DisplayName"),
                        profile.Elements("Section").Select(section =>
                            new BuildingProfileSection(
                                Required(section, "Name"),
                                section.Elements("Value").Select(value =>
                                    new BuildingProfileValue(
                                        Required(value, "Id"),
                                        Required(value, "DisplayName"),
                                        Required(value, "Value"),
                                        Optional(value, "Unit"),
                                        Optional(value, "Source"),
                                        Optional(value, "RequirementLevel"))))))),
                item => item.Id);
        }

        public SimpleCatalog<BuildingFunctionMapping> LoadBuildingMappings(
            string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = RequiredElement(document.Root, "Mappings");

            return new SimpleCatalog<BuildingFunctionMapping>(
                header,
                root.Elements("Mapping").Select(item =>
                    new BuildingFunctionMapping(
                        Required(item, "FunctionId"),
                        Required(item, "ProfileId"))),
                item => item.FunctionId);
        }

        public SimpleCatalog<ClimateRegionDefinition> LoadClimate(
            string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = RequiredElement(
                document.Root,
                "DesignClimateCatalog");

            return new SimpleCatalog<ClimateRegionDefinition>(
                header,
                root.Elements("Region").Select(item =>
                    new ClimateRegionDefinition(
                        Required(item, "Id"),
                        Required(item, "DisplayName"),
                        RequiredDouble(item, "HeatingOutdoorTemperature"),
                        Optional(item, "Notes"),
                        OptionalDouble(item, "CoolingOutdoorDryBulb"),
                        OptionalDouble(item, "CoolingOutdoorWetBulb"),
                        OptionalDouble(item, "DailyTemperatureRange"),
                        OptionalDouble(item, "SolarRadiation_South_Wm2"),
                        OptionalDouble(item, "SolarRadiation_East_Wm2"),
                        OptionalDouble(item, "SolarRadiation_West_Wm2"),
                        OptionalDouble(item, "SolarRadiation_North_Wm2"),
                        OptionalDouble(item, "SolarRadiation_Horizontal_Wm2"),
                        Optional(item, "HourlyClimateSeriesId"))),
                item => item.Id);
        }

        public SimpleCatalog<EngineeringDictionaryEntry> LoadDictionary(
            string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = RequiredElement(document.Root, "Entries");

            return new SimpleCatalog<EngineeringDictionaryEntry>(
                header,
                root.Elements("Entry").Select(item =>
                    new EngineeringDictionaryEntry(
                        Required(item, "Id"),
                        Required(item, "DisplayName"),
                        item.Elements("Alias")
                            .Select(alias => alias.Value.Trim())
                            .Where(alias => alias.Length > 0))),
                item => item.Id);
        }

        private static XDocument LoadDocument(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(
                    "Az XML-fájl útvonala nem lehet üres.",
                    nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException(
                    "Az EngineeringData XML-fájl nem található.",
                    filePath);

            XDocument document = XDocument.Load(
                filePath,
                LoadOptions.SetLineInfo);

            if (document.Root == null ||
                (!string.Equals(
                    document.Root.Name.LocalName,
                    "EngineeringData",
                    StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(
                    document.Root.Name.LocalName,
                    "EngineeringRules",
                    StringComparison.OrdinalIgnoreCase)))
            {
                throw new FormatException(
                    "Az XML gyökéreleme EngineeringData vagy EngineeringRules legyen: " +
                    filePath);
            }

            return document;
        }

        private static EngineeringDataHeaderInfo ReadHeader(
            XDocument document)
        {
            XElement root = document.Root;

            return new EngineeringDataHeaderInfo(
                Required(root, "Id"),
                Required(root, "Version"),
                Required(root, "SchemaVersion"),
                Required(root, "DataType"),
                Optional(root, "Country"),
                Optional(root, "DisplayName"));
        }

        private static XElement RequiredElement(
            XElement parent,
            string localName)
        {
            XElement element = parent.Elements().FirstOrDefault(item =>
                string.Equals(
                    item.Name.LocalName,
                    localName,
                    StringComparison.OrdinalIgnoreCase));

            if (element == null)
            {
                throw new FormatException(
                    "Hiányzó XML-szekció: " +
                    parent.Name.LocalName + "/" + localName + ".");
            }

            return element;
        }

        private static string Required(XElement element, string name)
        {
            string value = (string)element.Attribute(name);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException(
                    "Hiányzó kötelező attribútum: " +
                    element.Name.LocalName + "/@" + name + ".");
            }

            return value.Trim();
        }

        private static string Optional(XElement element, string name)
        {
            string value = (string)element.Attribute(name);
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }

        private static double RequiredDouble(
            XElement element,
            string name)
        {
            double? value = OptionalDouble(element, name);

            if (!value.HasValue)
            {
                throw new FormatException(
                    "Hiányzó vagy hibás numerikus attribútum: " +
                    element.Name.LocalName + "/@" + name + ".");
            }

            return value.Value;
        }

        private static double? OptionalDouble(
            XElement element,
            string name)
        {
            string value = Optional(element, name);
            if (value.Length == 0)
                return null;

            if (!double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double result))
            {
                throw new FormatException(
                    "Hibás numerikus attribútum: " +
                    element.Name.LocalName + "/@" + name +
                    " = " + value + ".");
            }

            return result;
        }

        private static int? OptionalInt(XElement element, string name)
        {
            string value = Optional(element, name);
            if (value.Length == 0)
                return null;

            if (!int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int result))
            {
                throw new FormatException(
                    "Hibás egész attribútum: " +
                    element.Name.LocalName + "/@" + name +
                    " = " + value + ".");
            }

            return result;
        }

        private static bool OptionalBool(XElement element, string name)
        {
            string value = Optional(element, name);
            if (value.Length == 0)
                return false;

            if (!bool.TryParse(value, out bool result))
            {
                throw new FormatException(
                    "Hibás logikai attribútum: " +
                    element.Name.LocalName + "/@" + name +
                    " = " + value + ".");
            }

            return result;
        }

        public SimpleCatalog<AirLayerSimpleDefinition> LoadAirLayers(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = RequiredElement(document.Root, "AirLayers");

            return new SimpleCatalog<AirLayerSimpleDefinition>(
                header,
                root.Elements("AirLayer").Select(item =>
                    new AirLayerSimpleDefinition(
                        Required(item, "Id"),
                        Required(item, "DisplayName"),
                        Optional(item, "Orientation"),
                        Optional(item, "HeatFlowDirection"),
                        Optional(item, "VentilationLevel"),
                        Optional(item, "EmissivityClass"),
                        RequiredDouble(item, "ThermalResistance"),
                        Optional(item, "Source"),
                        Optional(item, "Notes"))),
                item => item.Id);
        }

        public SimpleCatalog<ConstructionTemplateDefinition> LoadConstructionTemplates(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = RequiredElement(document.Root, "Constructions");

            return new SimpleCatalog<ConstructionTemplateDefinition>(
                header,
                root.Elements("Construction").Select(item =>
                    new ConstructionTemplateDefinition(
                        Required(item, "Id"),
                        Required(item, "DisplayName"),
                        Required(item, "Type"),
                        Required(item, "DataStatus"),
                        item.Elements("Layer").Select(layer =>
                            new ConstructionLayerSimpleDefinition(
                                RequiredInt(layer, "Order"),
                                Optional(layer, "MaterialId"),
                                Optional(layer, "AirLayerId"),
                                OptionalDouble(layer, "Thickness"),
                                Optional(layer, "Description"))))),
                item => item.Id);
        }

        public SimpleCatalog<BuildingPhysicsRulesDefinition> LoadBuildingPhysicsRules(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            
            XElement root = RequiredElement(document.Root, "RuleSet");
            XElement paramsEl = RequiredElement(root, "Parameters");
            
            var parameters = paramsEl.Elements("Parameter").ToDictionary(
                p => Required(p, "Name"),
                p => RequiredDouble(p, "Value"),
                StringComparer.OrdinalIgnoreCase);

            double rsiHorizontal = GetParam(parameters, "Rsi.HeatFlow.Horizontal");
            double rseHorizontal = GetParam(parameters, "Rse.HeatFlow.Horizontal");
            double rsiUpward = GetParam(parameters, "Rsi.HeatFlow.Upward");
            double rseUpward = GetParam(parameters, "Rse.HeatFlow.Upward");
            double rsiDownward = GetParam(parameters, "Rsi.HeatFlow.Downward");
            double rseDownward = GetParam(parameters, "Rse.HeatFlow.Downward");
            double defaultTB = GetParam(parameters, "ThermalBridge.DefaultAllowance");

            var ruleDef = new BuildingPhysicsRulesDefinition(
                Required(root, "Id"),
                rsiHorizontal,
                rseHorizontal,
                rsiUpward,
                rseUpward,
                rsiDownward,
                rseDownward,
                defaultTB);

            return new SimpleCatalog<BuildingPhysicsRulesDefinition>(
                header,
                new[] { ruleDef },
                item => item.Id);
        }

        public SimpleCatalog<HeatingLoadRulesDefinition> LoadHeatingLoadRules(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            
            XElement methodRulesEl = document.Root.Elements("RuleSet")
                .FirstOrDefault(e => string.Equals((string)e.Attribute("Id"), "HU.HeatingLoad.MethodParameters", StringComparison.OrdinalIgnoreCase));
            if (methodRulesEl == null)
                throw new FormatException("Hiányzó RuleSet: HU.HeatingLoad.MethodParameters.");

            XElement paramsEl = RequiredElement(methodRulesEl, "Parameters");
            var parameters = paramsEl.Elements("Parameter").ToDictionary(
                p => Required(p, "Name"),
                p => RequiredDouble(p, "Value"),
                StringComparer.OrdinalIgnoreCase);

            double airCap = GetParam(parameters, "Air.VolumetricHeatCapacity");
            double infNew = GetParam(parameters, "DefaultInfiltrationAch.NewConstruction");
            double infRecent = GetParam(parameters, "DefaultInfiltrationAch.RecentConstruction");
            double infLegacy = GetParam(parameters, "DefaultInfiltrationAch.LegacyConstruction");
            double reheatRes = GetParam(parameters, "ReheatFactor.Residential.8hSetback");
            double reheatOff = GetParam(parameters, "ReheatFactor.Office.12hSetback");
            double reheatSch = GetParam(parameters, "ReheatFactor.School.Weekend");

            XElement tbRulesEl = document.Root.Elements("RuleSet")
                .FirstOrDefault(e => string.Equals((string)e.Attribute("Id"), "HU.HeatingLoad.ThermalBridgeAllowances", StringComparison.OrdinalIgnoreCase));
            
            var allowances = new List<ThermalBridgeAllowanceSimpleDefinition>();
            if (tbRulesEl != null)
            {
                XElement tbAllowancesEl = RequiredElement(tbRulesEl, "ThermalBridgeAllowances");
                foreach (var item in tbAllowancesEl.Elements("Allowance"))
                {
                    allowances.Add(new ThermalBridgeAllowanceSimpleDefinition(
                        Required(item, "ConstructionType"),
                        Required(item, "InsulationLevel"),
                        RequiredDouble(item, "RecommendedAllowance"),
                        RequiredDouble(item, "DefaultAllowance"),
                        RequiredDouble(item, "WarningThresholdU"),
                        Optional(item, "Note")));
                }
            }

            var ruleDef = new HeatingLoadRulesDefinition(
                Required(document.Root, "Id"),
                airCap,
                infNew,
                infRecent,
                infLegacy,
                reheatRes,
                reheatOff,
                reheatSch,
                allowances);

            return new SimpleCatalog<HeatingLoadRulesDefinition>(
                header,
                new[] { ruleDef },
                item => item.Id);
        }

        public SimpleCatalog<CoolingLoadRulesDefinition> LoadCoolingLoadRules(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);

            XElement simplePeakEl = document.Root.Elements("RuleSet")
                .FirstOrDefault(e => string.Equals((string)e.Attribute("Id"), "HU.CoolingLoad.SimplePeak", StringComparison.OrdinalIgnoreCase));
            if (simplePeakEl == null)
                throw new FormatException("Hiányzó RuleSet: HU.CoolingLoad.SimplePeak.");

            XElement paramsEl = RequiredElement(simplePeakEl, "Parameters");
            var parameters = paramsEl.Elements("Parameter").ToDictionary(
                p => Required(p, "Name"),
                p => RequiredDouble(p, "Value"),
                StringComparer.OrdinalIgnoreCase);

            double airCap = GetParam(parameters, "Air.VolumetricHeatCapacity");
            double shdNone = GetParam(parameters, "ShadingFactor.None");
            double shdIntL = GetParam(parameters, "ShadingFactor.Internal.LightBlind");
            double shdIntD = GetParam(parameters, "ShadingFactor.Internal.DarkBlind");
            double shdExtL = GetParam(parameters, "ShadingFactor.External.LightBlind");
            double shdExtD = GetParam(parameters, "ShadingFactor.External.DarkBlind");
            double shdExtO = GetParam(parameters, "ShadingFactor.External.Overhang");
            
            double concRes = GetParam(parameters, "OccupancyConcurrencyFactor.Residential");
            double concOff = GetParam(parameters, "OccupancyConcurrencyFactor.Office");
            double concSch = GetParam(parameters, "OccupancyConcurrencyFactor.School");
            double concRet = GetParam(parameters, "OccupancyConcurrencyFactor.Retail");

            double sensSed = GetParam(parameters, "InternalHeatGain.SensiblePerPerson.Sedentary");
            double latSed = GetParam(parameters, "InternalHeatGain.LatentPerPerson.Sedentary");

            double lpdOff = GetParam(parameters, "LightingPowerDensity.Office");
            double lpdRes = GetParam(parameters, "LightingPowerDensity.Residential");

            double epdOff = GetParam(parameters, "EquipmentPowerDensity.Office");
            double epdRes = GetParam(parameters, "EquipmentPowerDensity.Residential");

            var ruleDef = new CoolingLoadRulesDefinition(
                Required(document.Root, "Id"),
                airCap,
                shdNone,
                shdIntL,
                shdIntD,
                shdExtL,
                shdExtD,
                shdExtO,
                concRes,
                concOff,
                concSch,
                concRet,
                sensSed,
                latSed,
                lpdOff,
                lpdRes,
                epdOff,
                epdRes);

            return new SimpleCatalog<CoolingLoadRulesDefinition>(
                header,
                new[] { ruleDef },
                item => item.Id);
        }

        private static int RequiredInt(XElement element, string name)
        {
            int? value = OptionalInt(element, name);
            if (!value.HasValue)
            {
                throw new FormatException(
                    "Hiányzó vagy hibás egész attribútum: " +
                    element.Name.LocalName + "/@" + name + ".");
            }
            return value.Value;
        }

        private static double GetParam(Dictionary<string, double> dict, string name)
        {
            if (!dict.TryGetValue(name, out double value))
            {
                throw new FormatException($"Hiányzó kötelező szabály-paraméter: {name}.");
            }
            return value;
        }

        public SimpleCatalog<GlazingSimpleDefinition> LoadGlazings(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = document.Root.Element("Glazings");
            if (root == null)
            {
                return new SimpleCatalog<GlazingSimpleDefinition>(header, Enumerable.Empty<GlazingSimpleDefinition>(), item => item.Id);
            }

            return new SimpleCatalog<GlazingSimpleDefinition>(
                header,
                root.Elements("Glazing").Select(item =>
                    new GlazingSimpleDefinition(
                        Required(item, "Id"),
                        Required(item, "DisplayName"),
                        RequiredDouble(item, "Ug"),
                        RequiredDouble(item, "SolarTransmittance"),
                        OptionalInt(item, "PaneCount"),
                        Optional(item, "GasFill"),
                        Optional(item, "CoatingType"))),
                item => item.Id);
        }

        public SimpleCatalog<FrameSimpleDefinition> LoadFrames(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = document.Root.Element("Frames");
            if (root == null)
            {
                return new SimpleCatalog<FrameSimpleDefinition>(header, Enumerable.Empty<FrameSimpleDefinition>(), item => item.Id);
            }

            return new SimpleCatalog<FrameSimpleDefinition>(
                header,
                root.Elements("Frame").Select(item =>
                    new FrameSimpleDefinition(
                        Required(item, "Id"),
                        Required(item, "DisplayName"),
                        RequiredDouble(item, "Uf"),
                        Required(item, "MaterialKind"),
                        OptionalDouble(item, "ProfileDepth"),
                        OptionalInt(item, "ChamberCount"),
                        OptionalDouble(item, "DefaultWidth"))),
                item => item.Id);
        }

        public SimpleCatalog<SpacerSimpleDefinition> LoadSpacers(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            EngineeringDataHeaderInfo header = ReadHeader(document);
            XElement root = document.Root.Element("Spacers");
            if (root == null)
            {
                return new SimpleCatalog<SpacerSimpleDefinition>(header, Enumerable.Empty<SpacerSimpleDefinition>(), item => item.Id);
            }

            return new SimpleCatalog<SpacerSimpleDefinition>(
                header,
                root.Elements("Spacer").Select(item =>
                    new SpacerSimpleDefinition(
                        Required(item, "Id"),
                        Required(item, "DisplayName"),
                        RequiredDouble(item, "Psi"),
                        Optional(item, "SpacerType"))),
                item => item.Id);
        }
    }
}
