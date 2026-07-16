using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using HVACDesigner.EngineeringData.Rules.Common;
using HVACDesigner.EngineeringData.Rules.Infrastructure.Xml;

namespace HVACDesigner.EngineeringData.Rules
{
    public sealed class RulePackageBootstrapResult
    {
        public int DiscoveredFileCount { get; }
        public int LoadedFileCount { get; }
        public int SkippedFileCount { get; }
        public int FailedFileCount { get; }
        public int RegisteredRuleSetCount { get; }
        public int RegisteredDesignMethodCount { get; }
        public IReadOnlyList<RulePackageDiagnostic> Diagnostics { get; }

        public bool Succeeded =>
            FailedFileCount == 0 &&
            Diagnostics.All(x => x.Severity != RulePackageDiagnosticSeverity.Error);

        public RulePackageBootstrapResult(
            int discoveredFileCount,
            int loadedFileCount,
            int skippedFileCount,
            int failedFileCount,
            int registeredRuleSetCount,
            int registeredDesignMethodCount,
            IEnumerable<RulePackageDiagnostic> diagnostics)
        {
            DiscoveredFileCount = discoveredFileCount;
            LoadedFileCount = loadedFileCount;
            SkippedFileCount = skippedFileCount;
            FailedFileCount = failedFileCount;
            RegisteredRuleSetCount = registeredRuleSetCount;
            RegisteredDesignMethodCount = registeredDesignMethodCount;
            Diagnostics = (diagnostics ?? Array.Empty<RulePackageDiagnostic>()).ToList();
        }
    }

    public sealed class RulePackageBootstrapper
    {
        private readonly XmlRulePackageLoader _loader;
        private readonly RulePackageValidator _validator;

        public RulePackageBootstrapper()
            : this(new XmlRulePackageLoader(), new RulePackageValidator())
        {
        }

        public RulePackageBootstrapper(
            XmlRulePackageLoader loader,
            RulePackageValidator validator)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public RulePackageBootstrapResult Bootstrap(
            EngineeringRuleRegistry registry,
            string xmlRootPath = null,
            bool searchSubdirectories = true)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            string rootPath = RulePackagePaths.ResolveXmlRoot(xmlRootPath);
            var diagnostics = new List<RulePackageDiagnostic>();

            if (!Directory.Exists(rootPath))
            {
                diagnostics.Add(new RulePackageDiagnostic(
                    RulePackageDiagnosticSeverity.Error,
                    "XML_ROOT_MISSING",
                    "A Data/Xml mappa nem található.",
                    rootPath));

                return new RulePackageBootstrapResult(
                    0, 0, 0, 1, 0, 0, diagnostics);
            }

            SearchOption searchOption = searchSubdirectories
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            string[] files = Directory.GetFiles(rootPath, "*.xml", searchOption)
                .OrderBy(x => x)
                .ToArray();

            var candidates = new List<RulePackageFileCandidate>();
            int skipped = 0;
            int failed = 0;

            foreach (string filePath in files)
            {
                if (!IsEngineeringRulesFile(filePath, out string detectionError))
                {
                    if (string.IsNullOrEmpty(detectionError))
                        skipped++;
                    else
                    {
                        failed++;
                        diagnostics.Add(new RulePackageDiagnostic(
                            RulePackageDiagnosticSeverity.Error,
                            "XML_ROOT_READ_FAILED",
                            detectionError,
                            filePath));
                    }

                    continue;
                }

                RulePackageLoadResult loadResult = _loader.Load(filePath);
                RulePackageValidationResult validation =
                    _validator.ValidateFileResult(filePath, loadResult);

                diagnostics.AddRange(validation.Diagnostics);

                if (!validation.IsValid)
                {
                    failed++;
                    continue;
                }

                candidates.Add(new RulePackageFileCandidate(
                    filePath,
                    loadResult.RuleSets,
                    loadResult.DesignMethods));
            }

            RulePackageValidationResult batchValidation =
                _validator.ValidateBatch(candidates);

            diagnostics.AddRange(batchValidation.Diagnostics);

            var invalidRuleSetKeys = new HashSet<string>(
                batchValidation.Diagnostics
                    .Where(x => x.Code == "DUPLICATE_RULE_SET")
                    .Select(x => x.EntityKey),
                StringComparer.OrdinalIgnoreCase);

            var invalidDesignMethodKeys = new HashSet<string>(
                batchValidation.Diagnostics
                    .Where(x =>
                        x.Code == "DUPLICATE_DESIGN_METHOD" ||
                        x.Code == "MISSING_RULE_SET_REFERENCE")
                    .Select(x => ExtractDesignMethodKey(x.EntityKey)),
                StringComparer.OrdinalIgnoreCase);

            int registeredRuleSets = 0;
            int registeredProfiles = 0;

            // 1. fázis: minden érvényes RuleSet.
            foreach (RulePackageFileCandidate candidate in candidates)
            {
                foreach (RuleSetDescriptor ruleSet in candidate.RuleSets)
                {
                    string key = ruleSet.GetVersionedKey();
                    if (invalidRuleSetKeys.Contains(key))
                        continue;

                    try
                    {
                        registry.RegisterRuleSet(ruleSet);
                        registeredRuleSets++;
                    }
                    catch (Exception exception)
                    {
                        diagnostics.Add(new RulePackageDiagnostic(
                            RulePackageDiagnosticSeverity.Error,
                            "RULE_SET_REGISTRATION_FAILED",
                            exception.Message,
                            candidate.FilePath,
                            key));
                    }
                }
            }

            // 2. fázis: DesignMethod profilok.
            foreach (RulePackageFileCandidate candidate in candidates)
            {
                foreach (DesignMethodProfile profile in candidate.DesignMethods)
                {
                    string key = profile.GetVersionedKey();
                    if (invalidDesignMethodKeys.Contains(key))
                        continue;

                    bool allReferencesAvailable = profile.RuleSetKeys.All(ruleSetKey =>
                        TrySplitVersionedKey(ruleSetKey, out string id, out string version) &&
                        registry.TryGetRuleSet(id, version, out _));

                    if (!allReferencesAvailable)
                        continue;

                    try
                    {
                        registry.RegisterDesignMethod(profile);
                        registeredProfiles++;
                    }
                    catch (Exception exception)
                    {
                        diagnostics.Add(new RulePackageDiagnostic(
                            RulePackageDiagnosticSeverity.Error,
                            "DESIGN_METHOD_REGISTRATION_FAILED",
                            exception.Message,
                            candidate.FilePath,
                            key));
                    }
                }
            }

            int loadedFiles = candidates.Count(x =>
                x.RuleSets.Count > 0 || x.DesignMethods.Count > 0);

            return new RulePackageBootstrapResult(
                files.Length,
                loadedFiles,
                skipped,
                failed,
                registeredRuleSets,
                registeredProfiles,
                diagnostics);
        }

        public bool IsRequestedVersionAvailable(
            EngineeringRuleRegistry registry,
            string designMethodId,
            string version)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            return registry.TryGetDesignMethod(
                designMethodId,
                version,
                out _);
        }

        private static bool IsEngineeringRulesFile(
            string filePath,
            out string error)
        {
            error = string.Empty;

            try
            {
                using (XmlReader reader = XmlReader.Create(
                    filePath,
                    new XmlReaderSettings
                    {
                        IgnoreComments = true,
                        IgnoreWhitespace = true
                    }))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            return string.Equals(
                                reader.LocalName,
                                "EngineeringRules",
                                StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }

                return false;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        private static bool TrySplitVersionedKey(
            string key,
            out string id,
            out string version)
        {
            id = string.Empty;
            version = string.Empty;

            if (string.IsNullOrWhiteSpace(key))
                return false;

            int index = key.LastIndexOf('@');
            if (index <= 0 || index == key.Length - 1)
                return false;

            id = key.Substring(0, index);
            version = key.Substring(index + 1);
            return true;
        }

        private static string ExtractDesignMethodKey(string entityKey)
        {
            if (string.IsNullOrWhiteSpace(entityKey))
                return string.Empty;

            int arrow = entityKey.IndexOf(" -> ", StringComparison.Ordinal);
            return arrow > 0
                ? entityKey.Substring(0, arrow)
                : entityKey;
        }
    }
}
