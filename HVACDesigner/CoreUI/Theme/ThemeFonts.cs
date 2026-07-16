using System.Drawing;
using System.Windows.Forms;

namespace HVACDesigner.CoreUI.Theme
{
    public static class ThemeFonts
    {
        private static AppSizeMode currentSizeMode = AppSizeMode.Normal;

        // --- Globálisan elérhető Tűéles Betűtípusok ---
        public static Font Title { get; private set; } = null!;
        public static Font Subtitle { get; private set; } = null!;
        public static Font Section { get; private set; } = null!;
        public static Font Body { get; private set; } = null!;
        public static Font BodyBold { get; private set; } = null!;
        public static Font Caption { get; private set; } = null!;
        public static Font Tiny { get; private set; } = null!;

        // --- Mérnöki számítások fix szélességű betűtípusa (Monospaced) ---
        public static Font Code { get; private set; } = null!;

        static ThemeFonts()
        {
            UpdateFonts(AppSizeMode.Normal);
            // Amikor a felhasználó méretmódot vált, teljesen új Font objektumokat generálunk le natívan
            ThemeManager.ThemeChanged += (s, e) =>
            {
                if (e.SizeMode != currentSizeMode)
                    UpdateFonts(e.SizeMode);
            };
        }

        private static void UpdateFonts(AppSizeMode sizeMode)
        {
            currentSizeMode = sizeMode;

            // Bázis méretek pontban (pt) megadva
            float baseTitle = 16f;
            float baseSection = 11f;
            float baseBody = 9.5f;
            float baseCaption = 8.5f;
            float baseTiny = 7.5f;
            float baseCode = 9.0f;

            // Skálázási szorzó alkalmazása a diszkrét fokozatok alapján
            float multiplier = 1.0f;
            if (sizeMode == AppSizeMode.Large) multiplier = 1.15f;
            else if (sizeMode == AppSizeMode.ExtraLarge) multiplier = 1.25f;

            // Új fontok példányosítása. A Windows ClearType technológiája ezt a monitor
            // aktuális fizikai pixelsűrűségéhez igazítva, tökéletesen élesen rajzolja ki.
            string fontSecondary = "Segoe UI";

            Title   = new Font("Segoe UI Semibold", baseTitle   * multiplier, FontStyle.Regular);
            Subtitle = new Font(fontSecondary,       13f         * multiplier, FontStyle.Regular);
            Section  = new Font("Segoe UI Semibold", baseSection * multiplier, FontStyle.Regular);

            Body     = new Font(fontSecondary,       baseBody    * multiplier, FontStyle.Regular);
            BodyBold = new Font("Segoe UI Semibold", baseBody    * multiplier, FontStyle.Regular);

            Caption  = new Font(fontSecondary,       baseCaption * multiplier, FontStyle.Regular);
            Tiny     = new Font(fontSecondary,       baseTiny    * multiplier, FontStyle.Regular);

            // Monospaced mérnöki betűtípus: Consolas, különben Courier New fallback
            Code = new Font("Consolas", baseCode * multiplier, FontStyle.Regular);
        }
    }
}
