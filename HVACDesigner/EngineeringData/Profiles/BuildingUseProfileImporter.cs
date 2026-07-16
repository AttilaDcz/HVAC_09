using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace HVACDesigner.EngineeringData.Profiles
{
    public sealed class BuildingUseProfileImportResult
    {
        public BuildingUseProfileCatalog Catalog { get; }
        public IReadOnlyList<string> Diagnostics { get; }

        public bool Succeeded =>
            Diagnostics.All(item =>
                !item.StartsWith(
                    "ERROR:",
                    StringComparison.OrdinalIgnoreCase));

        public BuildingUseProfileImportResult(
            BuildingUseProfileCatalog catalog,
            IReadOnlyList<string> diagnostics)
        {
            Catalog = catalog;
            Diagnostics = diagnostics;
        }
    }

    public sealed class BuildingUseProfileImporter
    {
        public BuildingUseProfileImportResult Import(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(
                    "Az XML útvonal nem lehet üres.",
                    nameof(filePath));

            var catalog = new BuildingUseProfileCatalog();
            var diagnostics = new List<string>();

            try
            {
                XDocument document = XDocument.Load(filePath);
                XElement root = document.Root;

                if (root == null ||
                    !string.Equals(
                        root.Name.LocalName,
                        "BuildingUseProfiles",
                        StringComparison.OrdinalIgnoreCase))
                {
                    diagnostics.Add(
                        "ERROR: A gyökérelem BuildingUseProfiles legyen.");

                    return new BuildingUseProfileImportResult(
                        catalog,
                        diagnostics);
                }

                foreach (XElement element in
                    root.Elements("BuildingProfile"))
                {
                    try
                    {
                        catalog.Register(ParseBuildingProfile(element));
                    }
                    catch (Exception exception)
                    {
                        diagnostics.Add(
                            "ERROR: BuildingProfile importhiba: " +
                            exception.Message);
                    }
                }

                foreach (XElement element in
                    root.Elements("SpaceProfile"))
                {
                    try
                    {
                        catalog.Register(ParseSpaceProfile(element));
                    }
                    catch (Exception exception)
                    {
                        diagnostics.Add(
                            "ERROR: SpaceProfile importhiba: " +
                            exception.Message);
                    }
                }
            }
            catch (Exception exception)
            {
                diagnostics.Add(
                    "ERROR: Profil XML betöltési hiba: " +
                    exception.Message);
            }

            return new BuildingUseProfileImportResult(
                catalog,
                diagnostics);
        }

        private static BuildingUseProfile ParseBuildingProfile(
            XElement element)
        {
            return new BuildingUseProfile(
                Required(element, "Id"),
                Required(element, "Version"),
                Required(element, "Name"),
                Optional(element, "ParentProfileId"),
                Optional(element, "FallbackProfileId"),
                ParseSections(element),
                element.Elements("SpaceProfileRef")
                    .Select(item => Required(item, "Id")));
        }

        private static SpaceUseProfile ParseSpaceProfile(
            XElement element)
        {
            return new SpaceUseProfile(
                Required(element, "Id"),
                Required(element, "Version"),
                Required(element, "Name"),
                Optional(element, "BuildingProfileId"),
                Optional(element, "ParentSpaceProfileId"),
                Optional(element, "FallbackProfileId"),
                ParseSections(element));
        }

        private static Dictionary<string, ProfileSection>
            ParseSections(XElement element)
        {
            var result = new Dictionary<string, ProfileSection>(
                StringComparer.OrdinalIgnoreCase);

            foreach (XElement section in
                element.Elements("Section"))
            {
                string sectionName = Required(section, "Name");

                var values = new Dictionary<string, ProfileValue>(
                    StringComparer.OrdinalIgnoreCase);

                foreach (XElement value in
                    section.Elements("Value"))
                {
                    if (!Enum.TryParse(
                        Optional(
                            value,
                            "Source",
                            "EngineeringRecommendation"),
                        true,
                        out ProfileValueSource source))
                    {
                        source =
                            ProfileValueSource.EngineeringRecommendation;
                    }

                    if (!Enum.TryParse(
                        Optional(
                            value,
                            "RequirementLevel",
                            "Default"),
                        true,
                        out ProfileRequirementLevel requirementLevel))
                    {
                        requirementLevel =
                            ProfileRequirementLevel.Default;
                    }

                    values[Required(value, "Name")] =
                        new ProfileValue(
                            Optional(value, "Value"),
                            Optional(value, "Unit"),
                            source,
                            requirementLevel,
                            Optional(value, "Reference"));
                }

                result[sectionName] = new ProfileSection(values);
            }

            return result;
        }

        private static string Required(
            XElement element,
            string name)
        {
            string value = (string)element.Attribute(name);

            if (string.IsNullOrWhiteSpace(value))
                throw new FormatException(
                    "Hiányzó kötelező attribútum: " +
                    element.Name.LocalName + "/@" + name + ".");

            return value.Trim();
        }

        private static string Optional(
            XElement element,
            string name,
            string defaultValue = "")
        {
            string value = (string)element.Attribute(name);

            return string.IsNullOrWhiteSpace(value)
                ? defaultValue
                : value.Trim();
        }
    }
}
