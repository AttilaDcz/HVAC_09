using System;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Stylers;

namespace HVACDesigner.CoreUI.Theme
{
    public static class ThemeApplicator
    {
        public static void MonitorControl(Form targetForm)
        {
            if (targetForm == null)
                return;

            ApplyTheme(
                targetForm,
                ThemeManager.CurrentPalette);

            EventHandler<ThemeChangedEventArgs> themeHandler = (s, e) =>
            {
                if (!targetForm.IsDisposed)
                {
                    ApplyTheme(targetForm, e.Palette);
                }
            };

            ThemeManager.ThemeChanged += themeHandler;

            targetForm.FormClosed += (s, e) =>
            {
                ThemeManager.ThemeChanged -= themeHandler;
            };
        }

        public static void ApplyTheme(
            Control root,
            ThemePalette palette)
        {
            if (root == null)
                return;

            if (palette == null)
                throw new ArgumentNullException(nameof(palette));

            ApplyToControlTree(root, palette);
        }

        private static void ApplyToControlTree(
            Control control,
            ThemePalette palette)
        {
            if (control == null)
                return;

            bool noTheme = string.Equals(
                control.Tag?.ToString(),
                "NoTheme",
                StringComparison.OrdinalIgnoreCase);

            if (noTheme)
                return;

            bool themeBoundary = string.Equals(
                control.Tag?.ToString(),
                "ThemeBoundary",
                StringComparison.OrdinalIgnoreCase);

            if (control is Form form)
            {
                form.BackColor = palette.Window;
                form.ForeColor = palette.TextPrimary;
                form.Font = ThemeFonts.Body;
            }
            else if (control is IThemeable themeable)
            {
                themeable.ApplyTheme(palette);
            }
            else
            {
                ControlStyler.Style(control, palette);
            }

            if (themeBoundary)
                return;

            foreach (Control child in control.Controls)
            {
                ApplyToControlTree(child, palette);
            }
        }
    }
}
