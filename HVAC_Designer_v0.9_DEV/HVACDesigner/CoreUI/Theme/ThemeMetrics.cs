using System.Windows.Forms;

namespace HVACDesigner.CoreUI.Theme
{
    public static class ThemeMetrics
    {
        // --- Dinamikus Méretek (Heights & Sizes) ---
        public static int TextBoxHeight { get; private set; }
        public static int ComboHeight { get; private set; }
        public static int ButtonHeight { get; private set; }

        // --- Térközök és Margók (Margins & Paddings) ---
        public static int MarginSmall { get; private set; }
        public static int MarginNormal { get; private set; }
        public static int MarginLarge { get; private set; }
        public static Padding Padding { get; private set; }
        public static Padding CardPadding { get; private set; }

        // --- Elrendezési Tényezők (Layout Spacing) ---
        public static int ControlSpacing { get; private set; }
        public static int GroupSpacing { get; private set; }
        public static int CornerRadius { get; private set; }
        public static int ShadowSize { get; private set; }

        // --- Ikon Méretek (Ikonográfia) ---
        public static int IconSizeSmall { get; private set; }
        public static int IconSizeNormal { get; private set; }
        public static int IconSizeLarge { get; private set; }

        // Statikus inicializáló az alapértelmezett méretekhez
        static ThemeMetrics()
        {
            UpdateMetrics(AppSizeMode.Normal);
            // Feliratkozunk a ThemeManager eseményére, hogy a méretek dinamikusan kövessék a váltást
            ThemeManager.ThemeChanged += (s, e) => UpdateMetrics(e.SizeMode);
        }

        private static void UpdateMetrics(AppSizeMode sizeMode)
        {
            // Skálázási szorzók meghatározása diszkrét fokozatok szerint
            double scaleFactor = 1.0;
            if (sizeMode == AppSizeMode.Large) scaleFactor = 1.15;
            else if (sizeMode == AppSizeMode.ExtraLarge) scaleFactor = 1.25;

            // 1. Vezérlők magasságainak skálázása (Mérnöki beviteli mezők, gombok)
            TextBoxHeight = (int)(24 * scaleFactor);
            ComboHeight = (int)(24 * scaleFactor);
            ButtonHeight = (int)(28 * scaleFactor);

            // 2. Térközök és margók (Biztosítják, hogy a felület ne tapadjon össze)
            MarginSmall = (int)(4 * scaleFactor);
            MarginNormal = (int)(8 * scaleFactor);
            MarginLarge = (int)(16 * scaleFactor);

            int pNormal = (int)(6 * scaleFactor);
            Padding = new Padding(pNormal);

            int pCard = (int)(12 * scaleFactor);
            CardPadding = new Padding(pCard);

            // 3. Elrendezési térközök (A PropertyRow és CardPanel közötti dinamikus lélegzőtér)
            ControlSpacing = (int)(6 * scaleFactor);
            GroupSpacing = (int)(14 * scaleFactor);
            CornerRadius = (int)(6 * scaleFactor); // CardPanel finom lekerekítése
            ShadowSize = (int)(4 * scaleFactor);

            // 4. Ikonok méretezése (Fejlécekhez, súgókhoz és gépészeti státuszjelzésekhez)
            IconSizeSmall = (int)(14 * scaleFactor);
            IconSizeNormal = (int)(18 * scaleFactor);
            IconSizeLarge = (int)(24 * scaleFactor);
        }
    }
}