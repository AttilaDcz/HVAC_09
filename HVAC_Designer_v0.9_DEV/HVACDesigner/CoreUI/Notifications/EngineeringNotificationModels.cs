using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HVACDesigner.CoreUI.Icons;

namespace HVACDesigner.CoreUI.Notifications
{
    public enum EngineeringNotificationKind
    {
        Neutral,
        Info,
        Success,
        Warning,
        Danger
    }

    public enum EngineeringNotificationScope
    {
        General,
        Project,
        Module,
        EngineeringData,
        RulePackage,
        Calculation,
        Import,
        AiRule
    }

    public enum EngineeringMessageVisibility
    {
        InternalOnly,
        DeveloperDiagnostics,
        UserDetails,
        UserToast,
        UserBlocking
    }

    public enum EngineeringMessageDisplayMode
    {
        None,
        StatusBar,
        Toast,
        ResultCard,
        FieldValidation,
        Dialog,
        DetailsPanel
    }

    public sealed class EngineeringNotification
    {
        private readonly ReadOnlyCollection<string> details;

        public EngineeringNotification(
            string title,
            string message,
            EngineeringNotificationKind kind,
            EngineeringNotificationScope scope = EngineeringNotificationScope.General,
            string code = "",
            string sourceId = "",
            string entityKey = "",
            IEnumerable<string>? details = null,
            string actionText = "",
            Action? action = null,
            TimeSpan? duration = null,
            bool isPersistent = false,
            HvacIconKind? iconKind = null)
        {
            Title = title?.Trim() ?? string.Empty;
            Message = message?.Trim() ?? string.Empty;
            Kind = kind;
            Scope = scope;
            Code = code?.Trim() ?? string.Empty;
            SourceId = sourceId?.Trim() ?? string.Empty;
            EntityKey = entityKey?.Trim() ?? string.Empty;
            ActionText = actionText?.Trim() ?? string.Empty;
            Action = action;
            Duration = duration ?? ResolveDefaultDuration(kind);
            IsPersistent = isPersistent;
            IconKind = iconKind;
            this.details = new ReadOnlyCollection<string>(
                (details ?? Enumerable.Empty<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .ToList());
        }

        public string Title { get; }
        public string Message { get; }
        public EngineeringNotificationKind Kind { get; }
        public EngineeringNotificationScope Scope { get; }
        public string Code { get; }
        public string SourceId { get; }
        public string EntityKey { get; }
        public IReadOnlyList<string> Details => details;
        public string ActionText { get; }
        public Action? Action { get; }
        public TimeSpan Duration { get; }
        public bool IsPersistent { get; }
        public HvacIconKind? IconKind { get; }

        private static TimeSpan ResolveDefaultDuration(EngineeringNotificationKind kind)
        {
            return kind == EngineeringNotificationKind.Danger
                ? TimeSpan.FromSeconds(8)
                : TimeSpan.FromSeconds(5);
        }
    }

    public sealed class EngineeringDiagnosticEvent
    {
        private readonly ReadOnlyCollection<string> details;

        public EngineeringDiagnosticEvent(
            string code,
            EngineeringNotificationKind kind,
            EngineeringNotificationScope scope,
            string technicalMessage,
            string sourceId = "",
            string entityKey = "",
            string recordId = "",
            string propertyName = "",
            IEnumerable<string>? details = null)
        {
            Code = code?.Trim() ?? string.Empty;
            Kind = kind;
            Scope = scope;
            TechnicalMessage = technicalMessage?.Trim() ?? string.Empty;
            SourceId = sourceId?.Trim() ?? string.Empty;
            EntityKey = entityKey?.Trim() ?? string.Empty;
            RecordId = recordId?.Trim() ?? string.Empty;
            PropertyName = propertyName?.Trim() ?? string.Empty;
            this.details = new ReadOnlyCollection<string>(
                (details ?? Enumerable.Empty<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .ToList());
        }

        public string Code { get; }
        public EngineeringNotificationKind Kind { get; }
        public EngineeringNotificationScope Scope { get; }
        public string TechnicalMessage { get; }
        public string SourceId { get; }
        public string EntityKey { get; }
        public string RecordId { get; }
        public string PropertyName { get; }
        public IReadOnlyList<string> Details => details;
    }

    public sealed class EngineeringUserMessage
    {
        public EngineeringUserMessage(
            string titleHu,
            string messageHu,
            EngineeringNotificationKind kind,
            EngineeringNotificationScope scope,
            EngineeringMessageVisibility visibility,
            EngineeringMessageDisplayMode displayMode,
            string code = "",
            string actionTextHu = "",
            IEnumerable<string>? detailsHu = null,
            bool isPersistent = false)
        {
            TitleHu = titleHu?.Trim() ?? string.Empty;
            MessageHu = messageHu?.Trim() ?? string.Empty;
            Kind = kind;
            Scope = scope;
            Visibility = visibility;
            DisplayMode = displayMode;
            Code = code?.Trim() ?? string.Empty;
            ActionTextHu = actionTextHu?.Trim() ?? string.Empty;
            DetailsHu = new ReadOnlyCollection<string>(
                (detailsHu ?? Enumerable.Empty<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .ToList());
            IsPersistent = isPersistent;
        }

        public string TitleHu { get; }
        public string MessageHu { get; }
        public EngineeringNotificationKind Kind { get; }
        public EngineeringNotificationScope Scope { get; }
        public EngineeringMessageVisibility Visibility { get; }
        public EngineeringMessageDisplayMode DisplayMode { get; }
        public string Code { get; }
        public string ActionTextHu { get; }
        public IReadOnlyList<string> DetailsHu { get; }
        public bool IsPersistent { get; }

        public EngineeringNotification ToNotification(Action? action = null)
        {
            return new EngineeringNotification(
                TitleHu,
                MessageHu,
                Kind,
                Scope,
                Code,
                details: DetailsHu,
                actionText: ActionTextHu,
                action: action,
                isPersistent: IsPersistent);
        }
    }
}
