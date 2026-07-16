using System;
using System.Collections.Generic;
using System.Linq;

namespace HVACDesigner.CoreUI.Notifications
{
    public static class EngineeringDiagnosticPresenter
    {
        public static EngineeringUserMessage Present(EngineeringDiagnosticEvent diagnostic)
        {
            if (diagnostic == null)
                throw new ArgumentNullException(nameof(diagnostic));

            LocalizedDiagnostic localized = EngineeringDiagnosticLocalizer.Localize(diagnostic);
            EngineeringMessageVisibility visibility = ResolveVisibility(diagnostic);
            EngineeringMessageDisplayMode displayMode = ResolveDisplayMode(diagnostic, visibility);

            return new EngineeringUserMessage(
                localized.Title,
                localized.Message,
                diagnostic.Kind,
                diagnostic.Scope,
                visibility,
                displayMode,
                diagnostic.Code,
                ResolveActionText(displayMode),
                localized.Details,
                diagnostic.Kind == EngineeringNotificationKind.Danger);
        }

        public static IReadOnlyList<EngineeringUserMessage> PresentMany(
            IEnumerable<EngineeringDiagnosticEvent> diagnostics)
        {
            return (diagnostics ?? Enumerable.Empty<EngineeringDiagnosticEvent>())
                .Select(Present)
                .ToList();
        }

        private static EngineeringMessageVisibility ResolveVisibility(
            EngineeringDiagnosticEvent diagnostic)
        {
            if (diagnostic.Kind == EngineeringNotificationKind.Danger)
                return EngineeringMessageVisibility.UserToast;

            if (diagnostic.Scope == EngineeringNotificationScope.RulePackage)
                return EngineeringMessageVisibility.UserToast;

            if (diagnostic.Scope == EngineeringNotificationScope.Import)
            {
                bool isRecordOrProperty =
                    !string.IsNullOrWhiteSpace(diagnostic.RecordId) ||
                    !string.IsNullOrWhiteSpace(diagnostic.PropertyName);

                return isRecordOrProperty
                    ? EngineeringMessageVisibility.UserDetails
                    : EngineeringMessageVisibility.UserToast;
            }

            if (diagnostic.Scope == EngineeringNotificationScope.Calculation)
                return diagnostic.Kind == EngineeringNotificationKind.Warning
                    ? EngineeringMessageVisibility.UserDetails
                    : EngineeringMessageVisibility.UserToast;

            return diagnostic.Kind == EngineeringNotificationKind.Neutral
                ? EngineeringMessageVisibility.DeveloperDiagnostics
                : EngineeringMessageVisibility.UserDetails;
        }

        private static EngineeringMessageDisplayMode ResolveDisplayMode(
            EngineeringDiagnosticEvent diagnostic,
            EngineeringMessageVisibility visibility)
        {
            if (visibility == EngineeringMessageVisibility.InternalOnly ||
                visibility == EngineeringMessageVisibility.DeveloperDiagnostics)
                return EngineeringMessageDisplayMode.None;

            if (visibility == EngineeringMessageVisibility.UserBlocking)
                return EngineeringMessageDisplayMode.Dialog;

            if (visibility == EngineeringMessageVisibility.UserToast)
                return EngineeringMessageDisplayMode.Toast;

            if (diagnostic.Scope == EngineeringNotificationScope.Calculation)
                return EngineeringMessageDisplayMode.ResultCard;

            return EngineeringMessageDisplayMode.DetailsPanel;
        }

        private static string ResolveActionText(EngineeringMessageDisplayMode displayMode)
        {
            return displayMode switch
            {
                EngineeringMessageDisplayMode.Toast => "Részletek",
                EngineeringMessageDisplayMode.DetailsPanel => "Megnyitás",
                EngineeringMessageDisplayMode.Dialog => "Részletek",
                _ => string.Empty
            };
        }
    }

    internal static class EngineeringDiagnosticLocalizer
    {
        private static readonly Dictionary<string, LocalizedDiagnosticTemplate> Templates =
            new Dictionary<string, LocalizedDiagnosticTemplate>(StringComparer.OrdinalIgnoreCase)
            {
                ["XML_ROOT_MISSING"] = new LocalizedDiagnosticTemplate(
                    "Hiányzó adatkönyvtár",
                    "A szabály- vagy katalógusadatok mappája nem található."),

                ["RULE_FILE_EMPTY"] = new LocalizedDiagnosticTemplate(
                    "Üres szabályfájl",
                    "A szabályfájl nem tartalmaz használható RuleSet vagy DesignMethod elemet."),

                ["DUPLICATE_RULE_SET"] = new LocalizedDiagnosticTemplate(
                    "Duplikált szabálycsomag",
                    "Két szabályfájl azonos szabályazonosítót tartalmaz. A csomag ellenőrzést igényel."),

                ["DUPLICATE_DESIGN_METHOD"] = new LocalizedDiagnosticTemplate(
                    "Duplikált tervezési módszer",
                    "Két szabályfájl azonos tervezési módszert tartalmaz."),

                ["MISSING_RULE_SET_REFERENCE"] = new LocalizedDiagnosticTemplate(
                    "Hiányzó szabályhivatkozás",
                    "Egy tervezési módszer olyan szabálycsomagra hivatkozik, amely nincs betöltve."),

                ["RULE_SET_REGISTRATION_FAILED"] = new LocalizedDiagnosticTemplate(
                    "Szabálycsomag regisztrációs hiba",
                    "A szabálycsomag betöltése közben hiba történt."),

                ["DESIGN_METHOD_REGISTRATION_FAILED"] = new LocalizedDiagnosticTemplate(
                    "Tervezési módszer regisztrációs hiba",
                    "A tervezési módszer betöltése közben hiba történt."),

                ["IMPORT_PACKAGE_ERROR"] = new LocalizedDiagnosticTemplate(
                    "Adatcsomag betöltési hiba",
                    "Az adatcsomag nem tölthető be megbízhatóan."),

                ["IMPORT_CONTENTSET_ERROR"] = new LocalizedDiagnosticTemplate(
                    "Katalógusrész betöltési hiba",
                    "Egy katalógusrész nem tölthető be teljesen."),

                ["IMPORT_RECORD_ERROR"] = new LocalizedDiagnosticTemplate(
                    "Katalógusrekord hiba",
                    "Egy katalógusrekord hibás vagy kihagyásra került."),

                ["IMPORT_PROPERTY_WARNING"] = new LocalizedDiagnosticTemplate(
                    "Katalógusmező figyelmeztetés",
                    "Egy adatmező ellenőrzést igényel, de a teljes katalógus használható maradhat."),

                ["CALCULATION_MISSING_REQUIRED_INPUT"] = new LocalizedDiagnosticTemplate(
                    "Hiányzó kötelező adat",
                    "A számítás nem futtatható, mert kötelező bemeneti adat hiányzik."),

                ["CALCULATION_MISSING_OPTIONAL_INPUT"] = new LocalizedDiagnosticTemplate(
                    "Hiányzó opcionális adat",
                    "A számítás futtatható, de opcionális adatok megadásával pontosítható.")
            };

        public static LocalizedDiagnostic Localize(EngineeringDiagnosticEvent diagnostic)
        {
            LocalizedDiagnosticTemplate template = Templates.TryGetValue(
                diagnostic.Code,
                out LocalizedDiagnosticTemplate? found)
                ? found
                : CreateFallbackTemplate(diagnostic);

            var details = new List<string>();

            if (!string.IsNullOrWhiteSpace(diagnostic.SourceId))
                details.Add("Forrás: " + diagnostic.SourceId);
            if (!string.IsNullOrWhiteSpace(diagnostic.EntityKey))
                details.Add("Entitás: " + diagnostic.EntityKey);
            if (!string.IsNullOrWhiteSpace(diagnostic.RecordId))
                details.Add("Rekord: " + diagnostic.RecordId);
            if (!string.IsNullOrWhiteSpace(diagnostic.PropertyName))
                details.Add("Mező: " + diagnostic.PropertyName);
            if (!string.IsNullOrWhiteSpace(diagnostic.TechnicalMessage))
                details.Add("Technikai üzenet: " + diagnostic.TechnicalMessage);

            details.AddRange(diagnostic.Details);

            return new LocalizedDiagnostic(template.Title, template.Message, details);
        }

        private static LocalizedDiagnosticTemplate CreateFallbackTemplate(
            EngineeringDiagnosticEvent diagnostic)
        {
            return diagnostic.Kind switch
            {
                EngineeringNotificationKind.Success => new LocalizedDiagnosticTemplate(
                    "Művelet sikeres",
                    "A művelet sikeresen befejeződött."),
                EngineeringNotificationKind.Warning => new LocalizedDiagnosticTemplate(
                    "Figyelmeztetés",
                    "A művelet ellenőrzést igényel."),
                EngineeringNotificationKind.Danger => new LocalizedDiagnosticTemplate(
                    "Hiba történt",
                    "A művelet közben hiba történt."),
                EngineeringNotificationKind.Info => new LocalizedDiagnosticTemplate(
                    "Információ",
                    "A rendszer információs üzenetet adott."),
                _ => new LocalizedDiagnosticTemplate(
                    "Diagnosztikai üzenet",
                    "A rendszer belső diagnosztikai üzenetet adott.")
            };
        }
    }

    internal sealed class LocalizedDiagnostic
    {
        public LocalizedDiagnostic(string title, string message, IEnumerable<string> details)
        {
            Title = title;
            Message = message;
            Details = details.ToList();
        }

        public string Title { get; }
        public string Message { get; }
        public IReadOnlyList<string> Details { get; }
    }

    internal sealed class LocalizedDiagnosticTemplate
    {
        public LocalizedDiagnosticTemplate(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public string Title { get; }
        public string Message { get; }
    }
}
