using System;

namespace HVACDesigner.CoreUI.Layout
{
    // A menü egyetlen funkciógombját leíró logikai osztály
    public class NavigationItem : LayoutNode
    {
        // A gomb mellett megjelenő emoji vagy ikon karakter
        public string Icon { get; set; }

        public NavigationItem(string name, string icon) : base(name)
        {
            Icon = icon;
        }
    }
}