using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using HVACDesigner.EngineeringData.Rules.Common;

namespace HVACDesigner.EngineeringData.Rules.Infrastructure.Xml
{
    public sealed class RulePackageLoadResult
    {
        public IReadOnlyList<RuleSetDescriptor> RuleSets { get; }
        public IReadOnlyList<DesignMethodProfile> DesignMethods { get; }
        public IReadOnlyList<string> Diagnostics { get; }

        public bool Succeeded =>
            Diagnostics.All(x => !x.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase));

        public RulePackageLoadResult(
            IReadOnlyList<RuleSetDescriptor> ruleSets,
            IReadOnlyList<DesignMethodProfile> designMethods,
            IReadOnlyList<string> diagnostics)
        {
            RuleSets = ruleSets;
            DesignMethods = designMethods;
            Diagnostics = diagnostics;
        }
    }

    public sealed class XmlRulePackageLoader
    {
        public RulePackageLoadResult Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Az XML útvonal nem lehet üres.", nameof(filePath));

            var ruleSets = new List<RuleSetDescriptor>();
            var methods = new List<DesignMethodProfile>();
            var diagnostics = new List<string>();

            try
            {
                XDocument document = XDocument.Load(filePath, LoadOptions.SetLineInfo);
                XElement root = document.Root;

                if (root == null ||
                    !string.Equals(root.Name.LocalName, "EngineeringRules", StringComparison.OrdinalIgnoreCase))
                {
                    diagnostics.Add("ERROR: A gyökérelem EngineeringRules legyen.");
                    return new RulePackageLoadResult(ruleSets, methods, diagnostics);
                }

                foreach (XElement element in root.Elements("RuleSet"))
                {
                    try { ruleSets.Add(ParseRuleSet(element)); }
                    catch (Exception ex) { diagnostics.Add("ERROR: RuleSet importhiba: " + ex.Message); }
                }

                foreach (XElement element in root.Elements("DesignMethod"))
                {
                    try { methods.Add(ParseDesignMethod(element)); }
                    catch (Exception ex) { diagnostics.Add("ERROR: DesignMethod importhiba: " + ex.Message); }
                }

                if (ruleSets.Count == 0)
                    diagnostics.Add("WARNING: Az XML nem tartalmaz RuleSet elemet.");
                if (methods.Count == 0)
                    diagnostics.Add("WARNING: Az XML nem tartalmaz DesignMethod elemet.");
            }
            catch (Exception ex)
            {
                diagnostics.Add("ERROR: Az XML betöltése meghiúsult: " + ex.Message);
            }

            return new RulePackageLoadResult(ruleSets, methods, diagnostics);
        }

        public void RegisterAll(RulePackageLoadResult result, EngineeringRuleRegistry registry)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (!result.Succeeded)
                throw new InvalidOperationException("Hibás szabálycsomag nem regisztrálható.");

            foreach (RuleSetDescriptor ruleSet in result.RuleSets)
                registry.RegisterRuleSet(ruleSet);
            foreach (DesignMethodProfile method in result.DesignMethods)
                registry.RegisterDesignMethod(method);
        }

        private static RuleSetDescriptor ParseRuleSet(XElement element)
        {
            if (!Enum.TryParse(Optional(element, "Discipline", "Shared"), true,
                out EngineeringRuleDiscipline discipline))
                throw new FormatException("Ismeretlen Discipline.");

            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            XElement parameterRoot = element.Element("Parameters");
            if (parameterRoot != null)
            {
                foreach (XElement parameter in parameterRoot.Elements("Parameter"))
                    parameters[Required(parameter, "Name")] = Required(parameter, "Value");
            }

            return new RuleSetDescriptor(
                Required(element, "Id"),
                Required(element, "Version"),
                Required(element, "Name"),
                discipline,
                Required(element, "MethodId"),
                ParseApplicability(element.Element("Applicability")),
                new RuleParameterSet(parameters),
                ParseReferences(element.Element("References")));
        }

        private static DesignMethodProfile ParseDesignMethod(XElement element)
        {
            bool isLegacy = bool.TryParse(Optional(element, "IsLegacy", "false"), out bool parsed) && parsed;
            return new DesignMethodProfile(
                Required(element, "Id"),
                Required(element, "Version"),
                Required(element, "Name"),
                Optional(element, "Jurisdiction", string.Empty),
                ParseDate(Optional(element, "ValidFrom", string.Empty)),
                ParseDate(Optional(element, "ValidTo", string.Empty)),
                isLegacy,
                element.Elements("UseRuleSet").Select(x => Required(x, "Key")).ToList(),
                ParseReferences(element.Element("References")));
        }

        private static RuleApplicability ParseApplicability(XElement element)
        {
            if (element == null)
                return new RuleApplicability(string.Empty, null, null);

            return new RuleApplicability(
                Optional(element, "Jurisdiction", string.Empty),
                ParseDate(Optional(element, "ValidFrom", string.Empty)),
                ParseDate(Optional(element, "ValidTo", string.Empty)),
                element.Elements("BuildingUse").Select(x => Optional(x, "Id", string.Empty)),
                element.Elements("RequiredTag").Select(x => Optional(x, "Value", string.Empty)));
        }

        private static List<RuleReference> ParseReferences(XElement element)
        {
            var result = new List<RuleReference>();
            if (element == null) return result;

            foreach (XElement reference in element.Elements("Reference"))
            {
                RuleReferenceStatus status = RuleReferenceStatus.Active;
                Enum.TryParse(Optional(reference, "Status", "Active"), true, out status);
                result.Add(new RuleReference(
                    Required(reference, "Designation"),
                    Optional(reference, "Title", string.Empty),
                    Optional(reference, "Edition", string.Empty),
                    status,
                    Optional(reference, "Jurisdiction", string.Empty),
                    Optional(reference, "Clause", string.Empty),
                    Optional(reference, "DocumentationText", string.Empty)));
            }
            return result;
        }

        private static DateTime? ParseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime parsed))
                throw new FormatException("A dátum formátuma yyyy-MM-dd legyen: " + value + ".");
            return parsed.Date;
        }

        private static string Required(XElement element, string name)
        {
            string value = (string)element.Attribute(name);
            if (string.IsNullOrWhiteSpace(value))
                throw new FormatException("Hiányzó kötelező attribútum: " + element.Name.LocalName + "/@" + name + ".");
            return value.Trim();
        }

        private static string Optional(XElement element, string name, string defaultValue)
        {
            string value = (string)element.Attribute(name);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        }
    }
}
