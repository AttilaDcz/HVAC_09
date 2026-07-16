using System.Drawing;

namespace HVACDesigner.CoreUI.Theme
{
    public class ThemePalette
    {
        // --- Felületek (Surfaces) ---
        public Color Window { get; private set; }
        public Color Surface { get; private set; }
        public Color SurfaceAlt { get; private set; }
        public Color SurfaceHover { get; private set; }
        public Color SurfacePressed { get; private set; }
        public Color SurfaceSelected { get; private set; }

        // --- Szegélyek (Borders) ---
        public Color Border { get; private set; }
        public Color BorderLight { get; private set; }
        public Color BorderStrong { get; private set; }

        // --- Szövegek (Typography) ---
        public Color TextPrimary { get; private set; }
        public Color TextSecondary { get; private set; }
        public Color TextDisabled { get; private set; }

        // --- Akcentus színek (Interactive Elements) ---
        public Color Accent { get; private set; }
        public Color AccentHover { get; private set; }
        public Color AccentPressed { get; private set; }

        // --- Mérnöki Szemantikus Státuszok (Semantic Colors) ---
        public Color Success { get; private set; }
        public Color Warning { get; private set; }
        public Color Danger { get; private set; }
        public Color Info { get; private set; }

        public ThemePalette(AppThemeMode mode)
        {
            // Egyelőre a kért szigorú sötét témát programozzuk le, 
            // de az architektúra már készen áll a későbbi light-mód bővítésre is.
            if (mode == AppThemeMode.Dark)
            {
                // Lágy, megnyugtató mély palakék-szürke háttér (nem vakít sötétben)
                Window = Color.FromArgb(20, 24, 28);

                // Kiemelkedő kártyafelület (térhatás / elevation)
                Surface = Color.FromArgb(28, 34, 40);

                // Váltakozó sorszín táblázatokhoz, listákhoz
                SurfaceAlt = Color.FromArgb(34, 41, 49);

                // Finom, nem tolakodó interaktív visszajelzések
                SurfaceHover = Color.FromArgb(44, 53, 64);
                SurfacePressed = Color.FromArgb(52, 63, 76);
                SurfaceSelected = Color.FromArgb(0, 122, 204); // Mérnöki kék kijelölés

                // Szegélyek
                Border = Color.FromArgb(50, 60, 72);
                BorderLight = Color.FromArgb(38, 46, 55);
                BorderStrong = Color.FromArgb(0, 122, 204); // Fókuszált kék szegély

                // Szövegek (Törtfehér és másodlagos szürkék a szem fáradása ellen)
                TextPrimary = Color.FromArgb(240, 244, 248);
                TextSecondary = Color.FromArgb(160, 174, 192);
                TextDisabled = Color.FromArgb(90, 105, 120);

                // Akcentusok (Gombokhoz, kiemelésekhez)
                Accent = Color.FromArgb(0, 122, 204);
                AccentHover = Color.FromArgb(20, 142, 224);
                AccentPressed = Color.FromArgb(0, 102, 174);

                // Pasztellesített, de telített mérnöki státuszszínek
                Success = Color.FromArgb(46, 196, 120);   // Lágy menta/fenyőzöld (Megfelel)
                Warning = Color.FromArgb(245, 158, 11);   // Meleg borostyán/mélyarany (Határérték közelében)
                Danger = Color.FromArgb(239, 68, 68);     // Korall-vörös (Kritikus hiba / Leválás)
                Info = Color.FromArgb(59, 130, 246);      // Információs kék
            }
            else
            {
                // Világos mód alapértelmezett fallback (későbbi kibővítéshez)
                Window = Color.FromArgb(245, 247, 250);
                Surface = Color.White;
                SurfaceAlt = Color.FromArgb(238, 242, 246);
                SurfaceHover = Color.FromArgb(230, 235, 241);
                SurfacePressed = Color.FromArgb(220, 225, 232);
                SurfaceSelected = Color.FromArgb(0, 122, 204);

                Border = Color.FromArgb(200, 208, 218);
                BorderLight = Color.FromArgb(230, 235, 241);
                BorderStrong = Color.FromArgb(0, 122, 204);

                TextPrimary = Color.FromArgb(26, 32, 44);
                TextSecondary = Color.FromArgb(113, 128, 150);
                TextDisabled = Color.FromArgb(160, 174, 192);

                Accent = Color.FromArgb(0, 122, 204);
                AccentHover = Color.FromArgb(0, 102, 174);
                AccentPressed = Color.FromArgb(0, 82, 144);

                Success = Color.FromArgb(34, 154, 88);
                Warning = Color.FromArgb(217, 119, 6);
                Danger = Color.FromArgb(220, 38, 38);
                Info = Color.FromArgb(37, 99, 235);
            }
        }
    }
}