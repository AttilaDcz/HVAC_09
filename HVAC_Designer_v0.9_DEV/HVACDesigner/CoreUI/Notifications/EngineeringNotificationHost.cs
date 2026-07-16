using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Notifications
{
    public sealed class EngineeringNotificationHost : Control, IThemeable
    {
        private const int CardWidth = 360;
        private const int CardHeight = 78;
        private const int CardGap = 10;
        private const int MaxVisibleCards = 3;

        private readonly List<NotificationEntry> entries = new List<NotificationEntry>();
        private readonly System.Windows.Forms.Timer timer;
        private ThemePalette palette = ThemeManager.CurrentPalette;
        private int hoveredIndex = -1;
        private bool isUpdatingHostLayout;

        public EngineeringNotificationHost()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            Size = new Size(CardWidth + 20, (CardHeight * MaxVisibleCards) + (CardGap * 2) + 20);
            BackColor = palette.Window;
            Visible = false;

            timer = new System.Windows.Forms.Timer { Interval = 250 };
            timer.Tick += Timer_Tick;
            timer.Start();

            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        }

        public void ShowNotification(EngineeringNotification notification)
        {
            if (notification == null)
                return;

            entries.Insert(0, new NotificationEntry(notification));

            while (entries.Count > 8)
                entries.RemoveAt(entries.Count - 1);

            Visible = true;
            UpdateHostLayout();
            BringToFront();
            Invalidate();
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.Window;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
                timer.Dispose();
                ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            }

            base.Dispose(disposing);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int index = HitTest(e.Location);
            if (hoveredIndex == index)
                return;

            hoveredIndex = index;
            Cursor = hoveredIndex >= 0 ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoveredIndex = -1;
            Cursor = Cursors.Default;
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (!isUpdatingHostLayout && entries.Count > 0)
                UpdateHostLayout();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int index = HitTest(e.Location);
            if (index < 0 || index >= VisibleEntries.Count)
                return;

            NotificationEntry entry = VisibleEntries[index];
            if (entry.Notification.Action != null)
                entry.Notification.Action.Invoke();

            entries.Remove(entry);
            Visible = entries.Count > 0;
            UpdateHostLayout();
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // The host is only a lightweight overlay. The Region is clipped to
            // visible toast cards, so no background panel should be painted.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            List<NotificationEntry> visible = VisibleEntries;
            for (int i = visible.Count - 1; i >= 0; i--)
            {
                Rectangle bounds = GetCardBounds(i);
                DrawNotification(g, visible[i], bounds, i == hoveredIndex);
            }
        }

        private void DrawNotification(
            Graphics g,
            NotificationEntry entry,
            Rectangle bounds,
            bool hovered)
        {
            EngineeringNotification notification = entry.Notification;
            Color accent = ResolveKindColor(notification.Kind);
            Color background = hovered ? palette.SurfaceHover : palette.SurfaceAlt;

            using SolidBrush backgroundBrush = new SolidBrush(background);
            using Pen borderPen = new Pen(hovered ? accent : palette.Border);
            using SolidBrush accentBrush = new SolidBrush(accent);

            g.FillRectangle(backgroundBrush, bounds);
            g.FillRectangle(accentBrush, bounds.Left, bounds.Top, 4, bounds.Height);
            g.DrawRectangle(borderPen, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1);

            HvacIconKind iconKind = notification.IconKind ?? ResolveIcon(notification.Kind);
            using Bitmap icon = HvacIconRenderer.RenderOutline(
                iconKind,
                ThemeManager.CurrentThemeMode,
                20,
                accent);
            g.DrawImage(icon, bounds.Left + 14, bounds.Top + 14, 20, 20);

            int textLeft = bounds.Left + 44;
            int textWidth = bounds.Width - 58;

            TextRenderer.DrawText(
                g,
                notification.Title,
                ThemeFonts.BodyBold,
                new Rectangle(textLeft, bounds.Top + 10, textWidth, 20),
                palette.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            TextRenderer.DrawText(
                g,
                notification.Message,
                ThemeFonts.Caption,
                new Rectangle(textLeft, bounds.Top + 34, textWidth, 34),
                palette.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.UtcNow;
            int removed = entries.RemoveAll(entry =>
                !entry.Notification.IsPersistent &&
                now - entry.CreatedUtc >= entry.Notification.Duration);

            if (removed > 0)
            {
                Visible = entries.Count > 0;
                UpdateHostLayout();
                Invalidate();
            }
        }

        private void ThemeManager_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            ApplyTheme(e.Palette);
        }

        private int HitTest(Point location)
        {
            List<NotificationEntry> visible = VisibleEntries;
            for (int i = 0; i < visible.Count; i++)
            {
                if (GetCardBounds(i).Contains(location))
                    return i;
            }

            return -1;
        }

        private Rectangle GetCardBounds(int index)
        {
            int x = Width - CardWidth - 10;
            int y = Height - 10 - ((index + 1) * CardHeight) - (index * CardGap);
            return new Rectangle(x, y, CardWidth, CardHeight);
        }

        private void UpdateHostLayout()
        {
            int visibleCount = Math.Min(MaxVisibleCards, entries.Count);
            if (visibleCount <= 0)
            {
                Region?.Dispose();
                Region = null;
                return;
            }

            isUpdatingHostLayout = true;
            int right = Right;
            int bottom = Bottom;
            int desiredHeight = (visibleCount * CardHeight) +
                ((visibleCount - 1) * CardGap) + 20;

            try
            {
                Size = new Size(CardWidth + 20, desiredHeight);

                if (right > 0)
                    Left = right - Width;
                if (bottom > 0)
                    Top = bottom - Height;
            }
            finally
            {
                isUpdatingHostLayout = false;
            }

            using GraphicsPath path = new GraphicsPath();
            for (int i = 0; i < visibleCount; i++)
                path.AddRectangle(GetCardBounds(i));

            Region? oldRegion = Region;
            Region = new Region(path);
            oldRegion?.Dispose();
        }

        private List<NotificationEntry> VisibleEntries =>
            entries.Take(MaxVisibleCards).ToList();

        private Color ResolveKindColor(EngineeringNotificationKind kind)
        {
            return kind switch
            {
                EngineeringNotificationKind.Info => palette.Info,
                EngineeringNotificationKind.Success => palette.Success,
                EngineeringNotificationKind.Warning => palette.Warning,
                EngineeringNotificationKind.Danger => palette.Danger,
                _ => palette.Accent
            };
        }

        private static HvacIconKind ResolveIcon(EngineeringNotificationKind kind)
        {
            return kind switch
            {
                EngineeringNotificationKind.Success => HvacIconKind.Certification,
                EngineeringNotificationKind.Warning => HvacIconKind.Safety,
                EngineeringNotificationKind.Danger => HvacIconKind.Safety,
                _ => HvacIconKind.Info
            };
        }

        private sealed class NotificationEntry
        {
            public NotificationEntry(EngineeringNotification notification)
            {
                Notification = notification;
                CreatedUtc = DateTime.UtcNow;
            }

            public EngineeringNotification Notification { get; }
            public DateTime CreatedUtc { get; }
        }
    }
}
