using System;
using System.Drawing;

namespace HVACDesigner.CoreUI.Layout
{
    // Egy teljes menükategóriát (pl. Légtechnika) leíró logikai osztály
    public class NavigationSection : LayoutNode
    {
        // A kategória egyedi díszítőszíne (pl. kék az energetikának, piros a fűtésnek)
        public Color AccentColor { get; set; }

        // A kategória fejléc ikonja
        public string Icon { get; set; }

        public NavigationSection(string name, string icon, Color accentColor) : base(name)
        {
            Icon = icon;
            AccentColor = accentColor;
        }
    }
}