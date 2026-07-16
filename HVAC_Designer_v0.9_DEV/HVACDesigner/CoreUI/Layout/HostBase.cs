using System;
using System.Windows.Forms;

namespace HVACDesigner.CoreUI.Layout
{
    // Minden egyedi felülethost közös ősosztálya
    public class HostBase : UserControl
    {
        // A hosthoz tartozó logikai csomópont
        public LayoutNode Node { get; private set; }

        public HostBase(string nodeName)
        {
            Node = new LayoutNode(nodeName);

            // Beállítjuk az alapvető WinForms optimalizációkat a villódzás ellen
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            // Globális ClearType szövegmegjelenítés
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Simább vonalak, ikonok, keretek
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            base.OnPaint(e);
        }


        // Biztosítja a tiszta memóriakezelést a bezáráskor
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Node?.Clear();
            }
            base.Dispose(disposing);
        }
    }
}