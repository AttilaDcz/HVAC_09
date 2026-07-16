using System;
using HVACDesigner.CoreUI.Notifications;

namespace HVACDesigner.CoreUI.Status
{
    public static class EngineeringStatusMessages
    {
        private static EngineeringStatusState current = new EngineeringStatusState();

        public static event EventHandler<EngineeringStatusState>? StatusChanged;

        public static EngineeringStatusState Current => current;

        public static void SetProject(string projectName)
        {
            Update(current.With(projectName: projectName));
        }

        public static void SetModule(string moduleName)
        {
            Update(current.With(moduleName: moduleName, message: "Készen áll", severity: EngineeringStatusSeverity.Neutral));
        }

        public static void SetReady(string message = "Készen áll")
        {
            Update(current.With(message: message, severity: EngineeringStatusSeverity.Neutral, recalculationSuggested: false));
        }

        public static void SetModified(string message = "Adatok módosultak | Újraszámítás javasolt")
        {
            Update(current.With(message: message, severity: EngineeringStatusSeverity.Info, recalculationSuggested: true));
        }

        public static void SetCalculationReady(int warningCount = 0)
        {
            if (warningCount > 0)
            {
                Update(current.With(
                    message: $"Ellenőrzés kész, {warningCount} figyelmeztetés",
                    severity: EngineeringStatusSeverity.Warning,
                    recalculationSuggested: false));
                return;
            }

            Update(current.With(
                message: "Számítás kész",
                severity: EngineeringStatusSeverity.Success,
                recalculationSuggested: false));
        }

        public static void SetWarning(string message)
        {
            Update(current.With(message: message, severity: EngineeringStatusSeverity.Warning));
        }

        public static void SetDanger(string message)
        {
            Update(current.With(message: message, severity: EngineeringStatusSeverity.Danger));
        }

        public static void SetDataState(string dataState, EngineeringStatusSeverity severity = EngineeringStatusSeverity.Info)
        {
            string message = string.IsNullOrWhiteSpace(dataState)
                ? current.Message
                : dataState.Trim();

            Update(current.With(dataState: dataState, message: message, severity: severity));
        }

        public static void SetUnitSummary(string unitSummary)
        {
            Update(current.With(unitSummary: unitSummary));
        }

        public static void FromNotification(EngineeringNotification notification)
        {
            if (notification == null)
                return;

            EngineeringStatusSeverity severity = notification.Kind switch
            {
                EngineeringNotificationKind.Success => EngineeringStatusSeverity.Success,
                EngineeringNotificationKind.Warning => EngineeringStatusSeverity.Warning,
                EngineeringNotificationKind.Danger => EngineeringStatusSeverity.Danger,
                EngineeringNotificationKind.Info => EngineeringStatusSeverity.Info,
                _ => EngineeringStatusSeverity.Neutral
            };

            Update(current.With(message: notification.Title, severity: severity));
        }

        private static void Update(EngineeringStatusState state)
        {
            current = state ?? new EngineeringStatusState();
            StatusChanged?.Invoke(null, current);
        }
    }
}
