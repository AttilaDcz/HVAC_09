using System;
using System.Collections.Generic;
using System.Linq;
using HVACDesigner.EngineeringData.Rules.Common;
using HVACDesigner.EngineeringData.Rules.Infrastructure.Xml;

namespace HVACDesigner.EngineeringData.Rules
{
    public enum RulePackageDiagnosticSeverity
    {
        Information,
        Warning,
        Error
    }

    public sealed class RulePackageDiagnostic
    {
        public RulePackageDiagnosticSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public string FilePath { get; }
        public string EntityKey { get; }

        public RulePackageDiagnostic(
            RulePackageDiagnosticSeverity severity,
            string code,
            string message,
            string filePath = "",
            string entityKey = "")
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("A diagnosztikai kód nem lehet üres.", nameof(code));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("A diagnosztikai üzenet nem lehet üres.", nameof(message));

            Severity = severity;
            Code = code.Trim();
            Message = message.Trim();
            FilePath = filePath?.Trim() ?? string.Empty;
            EntityKey = entityKey?.Trim() ?? string.Empty;
        }
    }

    public sealed class RulePackageValidationResult
    {
        public IReadOnlyList<RulePackageDiagnostic> Diagnostics { get; }
        public bool IsValid => Diagnostics.All(x => x.Severity != RulePackageDiagnosticSeverity.Error);

        public RulePackageValidationResult(IEnumerable<RulePackageDiagnostic> diagnostics)
        {
            Diagnostics = (diagnostics ?? Array.Empty<RulePackageDiagnostic>()).ToList();
        }
    }

    public sealed class RulePackageFileCandidate
    {
        public string FilePath { get; }
        public IReadOnlyList<RuleSetDescriptor> RuleSets { get; }
        public IReadOnlyList<DesignMethodProfile> DesignMethods { get; }

        public RulePackageFileCandidate(
            string filePath,
            IEnumerable<RuleSetDescriptor> ruleSets,
            IEnumerable<DesignMethodProfile> designMethods)
        {
            FilePath = filePath?.Trim() ?? string.Empty;
            RuleSets = (ruleSets ?? Enumerable.Empty<RuleSetDescriptor>()).ToList();
            DesignMethods = (designMethods ?? Enumerable.Empty<DesignMethodProfile>()).ToList();
        }
    }

    public sealed class RulePackageValidator
    {
        public RulePackageValidationResult ValidateFileResult(
            string filePath,
            RulePackageLoadResult loadResult)
        {
            if (loadResult == null)
                throw new ArgumentNullException(nameof(loadResult));

            var diagnostics = new List<RulePackageDiagnostic>();

            foreach (string item in loadResult.Diagnostics)
            {
                bool isError = item.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase);
                diagnostics.Add(new RulePackageDiagnostic(
                    isError ? RulePackageDiagnosticSeverity.Error : RulePackageDiagnosticSeverity.Warning,
                    isError ? "RULE_FILE_LOAD_ERROR" : "RULE_FILE_LOAD_WARNING",
                    item,
                    filePath));
            }

            if (loadResult.RuleSets.Count == 0 &&
                loadResult.DesignMethods.Count == 0 &&
                diagnostics.All(x => x.Severity != RulePackageDiagnosticSeverity.Error))
            {
                diagnostics.Add(new RulePackageDiagnostic(
                    RulePackageDiagnosticSeverity.Warning,
                    "RULE_FILE_EMPTY",
                    "A szabályfájl nem tartalmaz RuleSet vagy DesignMethod elemet.",
                    filePath));
            }

            return new RulePackageValidationResult(diagnostics);
        }

        public RulePackageValidationResult ValidateBatch(
            IEnumerable<RulePackageFileCandidate> candidates)
        {
            var list = (candidates ?? Enumerable.Empty<RulePackageFileCandidate>()).ToList();
            var diagnostics = new List<RulePackageDiagnostic>();
            var ruleSetOwners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var profileOwners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var allRuleSetKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (RulePackageFileCandidate candidate in list)
            {
                foreach (RuleSetDescriptor ruleSet in candidate.RuleSets)
                {
                    string key = ruleSet.GetVersionedKey();
                    if (ruleSetOwners.TryGetValue(key, out string owner))
                    {
                        diagnostics.Add(new RulePackageDiagnostic(
                            RulePackageDiagnosticSeverity.Error,
                            "DUPLICATE_RULE_SET",
                            "Duplikált RuleSet kulcs. Első fájl: " + owner,
                            candidate.FilePath,
                            key));
                    }
                    else
                    {
                        ruleSetOwners.Add(key, candidate.FilePath);
                        allRuleSetKeys.Add(key);
                    }
                }

                foreach (DesignMethodProfile profile in candidate.DesignMethods)
                {
                    string key = profile.GetVersionedKey();
                    if (profileOwners.TryGetValue(key, out string owner))
                    {
                        diagnostics.Add(new RulePackageDiagnostic(
                            RulePackageDiagnosticSeverity.Error,
                            "DUPLICATE_DESIGN_METHOD",
                            "Duplikált DesignMethod kulcs. Első fájl: " + owner,
                            candidate.FilePath,
                            key));
                    }
                    else
                    {
                        profileOwners.Add(key, candidate.FilePath);
                    }
                }
            }

            foreach (RulePackageFileCandidate candidate in list)
            {
                foreach (DesignMethodProfile profile in candidate.DesignMethods)
                {
                    foreach (string ruleSetKey in profile.RuleSetKeys)
                    {
                        if (!allRuleSetKeys.Contains(ruleSetKey))
                        {
                            diagnostics.Add(new RulePackageDiagnostic(
                                RulePackageDiagnosticSeverity.Error,
                                "MISSING_RULE_SET_REFERENCE",
                                "A DesignMethod ismeretlen RuleSetre hivatkozik.",
                                candidate.FilePath,
                                profile.GetVersionedKey() + " -> " + ruleSetKey));
                        }
                    }
                }
            }

            return new RulePackageValidationResult(diagnostics);
        }
    }
}
