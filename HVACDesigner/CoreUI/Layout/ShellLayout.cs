using System;
using System.Drawing;

namespace HVACDesigner.CoreUI.Layout
{
    /// <summary>
    /// Pure mathematical layout engine for the Application Shell.
    /// Kiszámítja a zónák abszolút koordinátáit, teljesen kiiktatva a WinForms Docking hibáit.
    /// </summary>
    public class ShellLayout
    {
        // Kiszámolt fix zónák téglalapjai (X, Y, Width, Height)
        public Rectangle HeaderBounds { get; private set; }
        public Rectangle NavigationBounds { get; private set; }
        public Rectangle ContentBounds { get; private set; }
        public Rectangle StatusBarBounds { get; private set; }

        /// <summary>
        /// Végrehajtja a teljes képernyőterület pixelpontos felosztását a LayoutMetrics alapján.
        /// </summary>
        /// <param name="clientSize">A Form aktuális belső ablakmérete (this.ClientSize)</param>
        /// <param name="isMenuExpanded">Nyitva van-e a bal oldali menü</param>
        public void Recalculate(Size clientSize, bool isMenuExpanded)
        {
            if (clientSize.Width < 50 || clientSize.Height < 50)
            {
                // Minimális méret, hogy a hostok ne nullázódjanak le
                HeaderBounds = new Rectangle(0, 0, 50, LayoutMetrics.HeaderHeight);
                NavigationBounds = new Rectangle(0, LayoutMetrics.HeaderHeight, LayoutMetrics.NavigationExpandedWidth, 100);
                ContentBounds = new Rectangle(LayoutMetrics.NavigationExpandedWidth, LayoutMetrics.HeaderHeight, 50, 100);
                StatusBarBounds = new Rectangle(0, 150, 50, LayoutMetrics.StatusBarHeight);
                return;
            }
            int totalWidth = clientSize.Width;
            int totalHeight = clientSize.Height;

            // 1. STÁTUSZSÁV (Mindig az ablak legalja, teljes szélességben)
            int statusY = totalHeight - LayoutMetrics.StatusBarHeight;
            StatusBarBounds = new Rectangle(0, statusY, totalWidth, LayoutMetrics.StatusBarHeight);

            // 2. FEJLÉC (Mindig az ablak legteteje, teljes szélességben)
            HeaderBounds = new Rectangle(0, 0, totalWidth, LayoutMetrics.HeaderHeight);

            // Maradék hasznos magasság a Header és a StatusBar között
            int remainingHeight = totalHeight - LayoutMetrics.HeaderHeight - LayoutMetrics.StatusBarHeight;
            int mainY = LayoutMetrics.HeaderHeight;

            // 3. NAVIGÁCIÓS MENÜ (Bal oldalon, a megmaradt függőleges sávban)
            int navWidth = isMenuExpanded ? LayoutMetrics.NavigationExpandedWidth : LayoutMetrics.NavigationCollapsedWidth;
            NavigationBounds = new Rectangle(0, mainY, navWidth, remainingHeight);

            // 4. MUNKATÉR / TARTALOM (A maradék tiszta terület a jobb oldalon)
            int contentX = navWidth;
            int contentWidth = totalWidth - navWidth;
            ContentBounds = new Rectangle(contentX, mainY, contentWidth, remainingHeight);
        }
    }
}