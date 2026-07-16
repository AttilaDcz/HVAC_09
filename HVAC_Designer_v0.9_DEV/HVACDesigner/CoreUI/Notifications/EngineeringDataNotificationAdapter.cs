using System.Collections.Generic;
using System.Linq;
using HVACDesigner.EngineeringData.Importing;
using HVACDesigner.EngineeringData.Rules;
using HVACDesigner.EngineeringData.Rules.DisciplineRules;

namespace HVACDesigner.CoreUI.Notifications
{
    public static class EngineeringDataNotificationAdapter
    {
        public static IReadOnlyList<EngineeringUserMessage> FromImportResult(
            DataPackageImportResult result)
        {
            var messages = new List<EngineeringUserMessage>();

            if (result == null)
                return messages;

            if (result.Succeeded && !result.HasWarnings && !result.HasErrors)
            {
                messages.Add(new EngineeringUserMessage(
                    "Adatkatalógus betöltve",
                    $"{result.ImportedCount} rekord betöltve, kihagyott rekord nincs.",
                    EngineeringNotificationKind.Success,
                    EngineeringNotificationScope.Import,
                    EngineeringMessageVisibility.UserToast,
                    EngineeringMessageDisplayMode.Toast,
                    "IMPORT_SUCCESS"));
                return messages;
            }

            EngineeringNotificationKind summaryKind = result.HasErrors
                ? EngineeringNotificationKind.Danger
                : EngineeringNotificationKind.Warning;

            string title = result.HasErrors
                ? "Adatkatalógus hibákkal töltődött be"
                : "Adatkatalógus figyelmeztetésekkel töltődött be";

            string message = result.IsPartialSuccess
                ? $"{result.ImportedCount} rekord betöltve, {result.SkippedCount} rekord kihagyva."
                : "Az adatcsomag betöltése ellenőrzést igényel.";

            messages.Add(new EngineeringUserMessage(
                title,
                message,
                summaryKind,
                EngineeringNotificationScope.Import,
                EngineeringMessageVisibility.UserToast,
                EngineeringMessageDisplayMode.Toast,
                result.HasErrors ? "IMPORT_HAS_ERRORS" : "IMPORT_HAS_WARNINGS",
                "Részletek",
                CollectImportSummaryDetails(result),
                result.HasErrors));

            foreach (ImportDiagnostic diagnostic in result.PackageDiagnostics)
                messages.Add(EngineeringDiagnosticPresenter.Present(FromImportDiagnostic(diagnostic)));

            foreach (ContentSetImportResult contentSet in result.ContentSetResults)
            {
                foreach (ImportDiagnostic diagnostic in contentSet.Diagnostics)
                    messages.Add(EngineeringDiagnosticPresenter.Present(FromImportDiagnostic(diagnostic)));
            }

            return messages;
        }

        public static IReadOnlyList<EngineeringUserMessage> FromRuleBootstrapResult(
            RulePackageBootstrapResult result)
        {
            var messages = new List<EngineeringUserMessage>();

            if (result == null)
                return messages;

            bool hasErrors = result.Diagnostics.Any(
                item => item.Severity == RulePackageDiagnosticSeverity.Error);
            bool hasWarnings = result.Diagnostics.Any(
                item => item.Severity == RulePackageDiagnosticSeverity.Warning);

            if (!hasErrors && !hasWarnings)
            {
                messages.Add(new EngineeringUserMessage(
                    "Szabálycsomagok betöltve",
                    $"{result.RegisteredRuleSetCount} RuleSet és {result.RegisteredDesignMethodCount} tervezési módszer elérhető.",
                    EngineeringNotificationKind.Success,
                    EngineeringNotificationScope.RulePackage,
                    EngineeringMessageVisibility.UserToast,
                    EngineeringMessageDisplayMode.Toast,
                    "RULE_BOOTSTRAP_SUCCESS"));
                return messages;
            }

            messages.Add(new EngineeringUserMessage(
                hasErrors ? "Szabálycsomag hibákkal töltődött be" : "Szabálycsomag figyelmeztetésekkel töltődött be",
                $"{result.LoadedFileCount} szabályfájl betöltve, {result.FailedFileCount} sikertelen, {result.SkippedFileCount} kihagyva.",
                hasErrors ? EngineeringNotificationKind.Danger : EngineeringNotificationKind.Warning,
                EngineeringNotificationScope.RulePackage,
                EngineeringMessageVisibility.UserToast,
                EngineeringMessageDisplayMode.Toast,
                hasErrors ? "RULE_BOOTSTRAP_HAS_ERRORS" : "RULE_BOOTSTRAP_HAS_WARNINGS",
                "Részletek",
                CollectRuleSummaryDetails(result),
                hasErrors));

            foreach (RulePackageDiagnostic diagnostic in result.Diagnostics)
                messages.Add(EngineeringDiagnosticPresenter.Present(FromRuleDiagnostic(diagnostic)));

            return messages;
        }

        public static IReadOnlyList<EngineeringUserMessage> FromCalculationResolution(
            CalculationResolution resolution)
        {
            var messages = new List<EngineeringUserMessage>();

            if (resolution == null)
                return messages;

            if (resolution.MissingRequiredInputs.Count > 0)
            {
                messages.Add(EngineeringDiagnosticPresenter.Present(
                    new EngineeringDiagnosticEvent(
                        "CALCULATION_MISSING_REQUIRED_INPUT",
                        EngineeringNotificationKind.Danger,
                        EngineeringNotificationScope.Calculation,
                        "Missing required calculation inputs.",
                        resolution.CalculationId,
                        resolution.MethodId,
                        details: resolution.MissingRequiredInputs.Select(item => "Hiányzó kötelező adat: " + item))));
            }

            if (resolution.MissingOptionalInputs.Count > 0)
            {
                messages.Add(EngineeringDiagnosticPresenter.Present(
                    new EngineeringDiagnosticEvent(
                        "CALCULATION_MISSING_OPTIONAL_INPUT",
                        EngineeringNotificationKind.Warning,
                        EngineeringNotificationScope.Calculation,
                        "Missing optional calculation inputs.",
                        resolution.CalculationId,
                        resolution.MethodId,
                        details: resolution.MissingOptionalInputs.Select(item => "Hiányzó opcionális adat: " + item))));
            }

            return messages;
        }

        private static EngineeringDiagnosticEvent FromImportDiagnostic(
            ImportDiagnostic diagnostic)
        {
            string code = diagnostic.Code;
            if (string.IsNullOrWhiteSpace(code))
            {
                code = diagnostic.FailureScope switch
                {
                    ImportFailureScope.Package => "IMPORT_PACKAGE_ERROR",
                    ImportFailureScope.ContentSet => "IMPORT_CONTENTSET_ERROR",
                    ImportFailureScope.Record => "IMPORT_RECORD_ERROR",
                    ImportFailureScope.Property => "IMPORT_PROPERTY_WARNING",
                    _ => "IMPORT_DIAGNOSTIC"
                };
            }

            return new EngineeringDiagnosticEvent(
                code,
                MapImportSeverity(diagnostic.Severity),
                EngineeringNotificationScope.Import,
                diagnostic.Message,
                diagnostic.ContentSetId,
                string.Empty,
                diagnostic.RecordId,
                diagnostic.PropertyName,
                diagnostic.Exception == null
                    ? Enumerable.Empty<string>()
                    : new[] { diagnostic.Exception.Message });
        }

        private static EngineeringDiagnosticEvent FromRuleDiagnostic(
            RulePackageDiagnostic diagnostic)
        {
            return new EngineeringDiagnosticEvent(
                diagnostic.Code,
                MapRuleSeverity(diagnostic.Severity),
                EngineeringNotificationScope.RulePackage,
                diagnostic.Message,
                diagnostic.FilePath,
                diagnostic.EntityKey);
        }

        private static EngineeringNotificationKind MapImportSeverity(
            ImportDiagnosticSeverity severity)
        {
            return severity switch
            {
                ImportDiagnosticSeverity.Information => EngineeringNotificationKind.Info,
                ImportDiagnosticSeverity.Warning => EngineeringNotificationKind.Warning,
                ImportDiagnosticSeverity.Error => EngineeringNotificationKind.Danger,
                _ => EngineeringNotificationKind.Neutral
            };
        }

        private static EngineeringNotificationKind MapRuleSeverity(
            RulePackageDiagnosticSeverity severity)
        {
            return severity switch
            {
                RulePackageDiagnosticSeverity.Information => EngineeringNotificationKind.Info,
                RulePackageDiagnosticSeverity.Warning => EngineeringNotificationKind.Warning,
                RulePackageDiagnosticSeverity.Error => EngineeringNotificationKind.Danger,
                _ => EngineeringNotificationKind.Neutral
            };
        }

        private static IReadOnlyList<string> CollectImportSummaryDetails(
            DataPackageImportResult result)
        {
            return new[]
            {
                "Csomag: " + result.PackageId + " @ " + result.PackageVersion,
                "Betöltött rekordok: " + result.ImportedCount,
                "Kihagyott rekordok: " + result.SkippedCount,
                "Tartalomkészletek: " + result.ContentSetResults.Count
            };
        }

        private static IReadOnlyList<string> CollectRuleSummaryDetails(
            RulePackageBootstrapResult result)
        {
            return new[]
            {
                "Megtalált fájlok: " + result.DiscoveredFileCount,
                "Betöltött fájlok: " + result.LoadedFileCount,
                "Kihagyott fájlok: " + result.SkippedFileCount,
                "Sikertelen fájlok: " + result.FailedFileCount,
                "RuleSet: " + result.RegisteredRuleSetCount,
                "DesignMethod: " + result.RegisteredDesignMethodCount
            };
        }
    }
}
