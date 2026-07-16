using System;

namespace HVACDesigner.CoreUI.Layout
{
    /// <summary>
    /// A teljes HVAC Designer felület statikus méretmátrixa és konstansai.
    /// Központosított hely a DPI-aware skálázás és az egységes megjelenés érdekében.
    /// </summary>
    public static class LayoutMetrics
    {
        // --- SZERKEZETI MÉRETEK (PIXELBEN) ---
        public static int DefaultFormWidth => 1150;
        public static int DefaultFormHeight => 780;

        public static int HeaderHeight => 95;
        public static int TopBarHeight => 55;
        public static int BottomBarHeight => 40;

        public static int NavigationExpandedWidth => 230;
        public static int NavigationCollapsedWidth => 50;

        public static int StatusBarHeight => 25;
        public static int GlobalButtonSize => 38;

        // --- FINOMHANGOLÁSI ÉRTÉKEK ---
        public static int MenuButtonHeight => 44;
        public static int MenuSectionHeaderHeight => 36;
        public static int ScrollBarWidth => 6;

        // --- MARGIN ÉS PADDING MATRICÁK ---
        public static int ContentPadding => 10;
        public static int MenuPaddingTopBottom => 15;
        public static int MenuPaddingLeftRight => 10;
    }
}