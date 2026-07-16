using System;

namespace HVACDesigner.CoreUI.Theme
{
    public enum AppThemeMode
    {
        Light,
        Dark
    }

    public enum AppSizeMode
    {
        Normal,     // Alapértelmezett mérnöki méret
        Large,      // Nagyított mód (+15%)
        ExtraLarge  // Extra nagyított akadálymentesített mód (+25%)
    }

    public class ThemeChangedEventArgs : EventArgs
    {
        public AppThemeMode ThemeMode { get; }
        public AppSizeMode SizeMode { get; }
        public ThemePalette Palette { get; }

        public ThemeChangedEventArgs(AppThemeMode themeMode, AppSizeMode sizeMode, ThemePalette palette)
        {
            ThemeMode = themeMode;
            SizeMode = sizeMode;
            Palette = palette;
        }
    }

    public static class ThemeManager
    {
        private static AppThemeMode _currentThemeMode = AppThemeMode.Dark; // Sötét mód az alapértelmezett
        private static AppSizeMode _currentSizeMode = AppSizeMode.Normal;

        // Globális tulajdonság a Téma mód eléréséhez és módosításához
        public static AppThemeMode CurrentThemeMode
        {
            get => _currentThemeMode;
            set
            {
                if (_currentThemeMode != value)
                {
                    _currentThemeMode = value;
                    RegeneratePalette();
                    NotifySubscribers();
                }
            }
        }

        // Globális tulajdonság a Betű- és Felületméret eléréséhez
        public static AppSizeMode CurrentSizeMode
        {
            get => _currentSizeMode;
            set
            {
                if (_currentSizeMode != value)
                {
                    _currentSizeMode = value;
                    // A méretváltozás nem feltétlenül generál új színpalettát, 
                    // de az eseményt el kell sütnie a ThemeMetrics frissítéséhez.
                    NotifySubscribers();
                }
            }
        }

        // Az aktuálisan aktív, gyorsan lekérdezhető színpaletta referencia
        public static ThemePalette CurrentPalette { get; private set; } = null!;

        // Globális esemény, amire minden ablak és egyedi komponens feliratkozik
        public static event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        // Statikus konstruktor az első paletta felépítéséhez az indításkor
        static ThemeManager()
        {
            RegeneratePalette();
        }

        private static void RegeneratePalette()
        {
            CurrentPalette = new ThemePalette(_currentThemeMode);
        }

        private static void NotifySubscribers()
        {
            ThemeChanged?.Invoke(null, new ThemeChangedEventArgs(_currentThemeMode, _currentSizeMode, CurrentPalette));
        }
    }
}
