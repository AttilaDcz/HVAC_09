using System.Drawing;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Icons
{
    public static class HeaderIconRenderer
    {
        public static Bitmap Render(
            HeaderIconKind icon,
            AppThemeMode themeMode,
            int size)
        {
            return HvacIconRenderer.Render(
                Map(icon),
                themeMode,
                size);
        }

        private static HvacIconKind Map(HeaderIconKind icon)
        {
            return icon switch
            {
                HeaderIconKind.AppLogo => HvacIconKind.AppLogo,
                HeaderIconKind.NewProject => HvacIconKind.NewProject,
                HeaderIconKind.OpenProject => HvacIconKind.OpenProject,
                HeaderIconKind.SaveProject => HvacIconKind.SaveProject,
                HeaderIconKind.ProjectProperties => HvacIconKind.ProjectProperties,
                HeaderIconKind.Settings => HvacIconKind.Settings,
                HeaderIconKind.Export => HvacIconKind.Export,
                HeaderIconKind.PrintReport => HvacIconKind.PrintReport,
                HeaderIconKind.Info => HvacIconKind.Info,
                HeaderIconKind.NavigateBack => HvacIconKind.NavigateBack,
                HeaderIconKind.NavigateForward => HvacIconKind.NavigateForward,
                HeaderIconKind.Undo => HvacIconKind.Undo,
                HeaderIconKind.Redo => HvacIconKind.Redo,
                HeaderIconKind.Help => HvacIconKind.Help,
                _ => HvacIconKind.Info
            };
        }
    }
}
