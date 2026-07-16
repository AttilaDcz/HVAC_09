using System;
using System.Collections.Generic;
using System.Linq;
using HVACDesigner.CoreUI.Status;

namespace HVACDesigner.CoreUI.Notifications
{
    public static class EngineeringNotificationService
    {
        private static EngineeringNotificationHost? host;

        public static void RegisterHost(EngineeringNotificationHost notificationHost)
        {
            host = notificationHost ?? throw new ArgumentNullException(nameof(notificationHost));
        }

        public static void Show(EngineeringNotification notification)
        {
            host?.ShowNotification(notification);
            EngineeringStatusMessages.FromNotification(notification);
        }

        public static void Show(EngineeringUserMessage message, Action? action = null)
        {
            if (message == null)
                return;

            if (message.DisplayMode == EngineeringMessageDisplayMode.Toast)
                Show(message.ToNotification(action));
        }

        public static void ShowMany(IEnumerable<EngineeringUserMessage> messages)
        {
            foreach (EngineeringUserMessage message in messages ?? Enumerable.Empty<EngineeringUserMessage>())
                Show(message);
        }

        public static void Info(string title, string message)
        {
            Show(new EngineeringNotification(
                title,
                message,
                EngineeringNotificationKind.Info));
        }

        public static void Success(string title, string message)
        {
            Show(new EngineeringNotification(
                title,
                message,
                EngineeringNotificationKind.Success));
        }

        public static void Warning(string title, string message)
        {
            Show(new EngineeringNotification(
                title,
                message,
                EngineeringNotificationKind.Warning));
        }

        public static void Danger(string title, string message)
        {
            Show(new EngineeringNotification(
                title,
                message,
                EngineeringNotificationKind.Danger,
                isPersistent: true));
        }
    }
}
