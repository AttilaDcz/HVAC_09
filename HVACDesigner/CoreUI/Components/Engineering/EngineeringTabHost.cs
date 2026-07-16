using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Dialogs;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Engineering
{
    public enum EngineeringTabStyle
    {
        Standard,
        Segmented,
        Compact
    }

    public enum EngineeringTabOverflowMode
    {
        Clip,
        ScrollButtons,
        Shrink
    }

    public enum EngineeringTabSeverity
    {
        None,
        Info,
        Success,
        Warning,
        Danger
    }

    public sealed class EngineeringTabPage
    {
        public string Key { get; set; } = Guid.NewGuid().ToString("N");
        public string Text { get; set; } = "Fül";
        public string Subtitle { get; set; } = string.Empty;
        public string BadgeText { get; set; } = string.Empty;
        public HvacIconKind? IconKind { get; set; }
        public EngineeringTabSeverity Severity { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool CanClose { get; set; }
        public bool ConfirmCloseWhenContentExists { get; set; } = true;
        public Control? Content { get; set; }
        public object? Model { get; set; }
        public string ToolTipText { get; set; } = string.Empty;
    }

    public sealed class EngineeringTabChangedEventArgs : EventArgs
    {
        public EngineeringTabChangedEventArgs(
            EngineeringTabPage? oldPage,
            EngineeringTabPage? newPage)
        {
            OldPage = oldPage;
            NewPage = newPage;
        }

        public EngineeringTabPage? OldPage { get; }
        public EngineeringTabPage? NewPage { get; }
    }

    public sealed class EngineeringTabClosingEventArgs : CancelEventArgs
    {
        public EngineeringTabClosingEventArgs(EngineeringTabPage page)
        {
            Page = page ?? throw new ArgumentNullException(nameof(page));
        }

        public EngineeringTabPage Page { get; }
    }

    public sealed class EngineeringTabPageCollection : Collection<EngineeringTabPage>
    {
        private readonly EngineeringTabHost owner;

        internal EngineeringTabPageCollection(EngineeringTabHost owner)
        {
            this.owner = owner;
        }

        public EngineeringTabPage Add(
            string key,
            string text,
            Control? content = null,
            object? model = null)
        {
            var page = new EngineeringTabPage
            {
                Key = key,
                Text = text,
                Content = content,
                Model = model
            };
            Add(page);
            return page;
        }

        protected override void InsertItem(int index, EngineeringTabPage item)
        {
            base.InsertItem(index, item);
            owner.OnPagesChanged(item);
        }

        protected override void SetItem(int index, EngineeringTabPage item)
        {
            base.SetItem(index, item);
            owner.OnPagesChanged(item);
        }

        protected override void RemoveItem(int index)
        {
            EngineeringTabPage removed = this[index];
            base.RemoveItem(index);
            owner.OnPageRemoved(removed);
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            owner.OnPagesCleared();
        }
    }

    public sealed class EngineeringTabHost : UserControl, IThemeable
    {
        private readonly TabStripControl tabStrip;
        private readonly Panel contentHost;
        private readonly EngineeringTabPageCollection pages;
        private readonly ToolTip toolTip;
        private ThemePalette palette = ThemeManager.CurrentPalette;
        private string selectedKey = string.Empty;
        private bool showBorder = true;

        public EngineeringTabHost()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            pages = new EngineeringTabPageCollection(this);
            toolTip = new ToolTip
            {
                InitialDelay = 350,
                ReshowDelay = 120,
                AutoPopDelay = 6000,
                ShowAlways = true
            };

            tabStrip = new TabStripControl(this)
            {
                Dock = DockStyle.Top,
                Height = 38
            };

            contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = palette.Surface,
                Padding = new Padding(1)
            };
            contentHost.Paint += (_, args) => DrawContentBorder(args.Graphics);

            Controls.Add(contentHost);
            Controls.Add(tabStrip);
            Size = new Size(420, 260);
            ApplyTheme(palette);
        }

        public event EventHandler<EngineeringTabChangedEventArgs>? SelectedTabChanged;
        public event EventHandler<EngineeringTabClosingEventArgs>? TabClosing;
        public event EventHandler? AddTabRequested;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EngineeringTabPageCollection Pages => pages;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel ContentHost => contentHost;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EngineeringTabPage? SelectedPage => FindPage(selectedKey);

        [Category("Engineering")]
        [DefaultValue("")]
        public string SelectedKey
        {
            get => selectedKey;
            set => SelectPage(value, true);
        }

        [Category("Engineering")]
        [DefaultValue(EngineeringTabStyle.Standard)]
        public EngineeringTabStyle TabStyle
        {
            get => tabStrip.TabStyle;
            set
            {
                tabStrip.TabStyle = value;
                tabStrip.Height = value == EngineeringTabStyle.Compact ? 32 : 38;
                tabStrip.Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(EngineeringTabOverflowMode.ScrollButtons)]
        public EngineeringTabOverflowMode OverflowMode
        {
            get => tabStrip.OverflowMode;
            set
            {
                tabStrip.OverflowMode = value;
                tabStrip.Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(false)]
        public bool ShowAddButton
        {
            get => tabStrip.ShowAddButton;
            set
            {
                tabStrip.ShowAddButton = value;
                tabStrip.Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(true)]
        public bool ShowBorder
        {
            get => showBorder;
            set
            {
                if (showBorder == value)
                    return;

                showBorder = value;
                contentHost.Padding = value ? new Padding(1) : Padding.Empty;
                contentHost.Invalidate();
            }
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.Surface;
            ForeColor = palette.TextPrimary;
            contentHost.BackColor = palette.Surface;
            tabStrip.ApplyTheme(palette);
            contentHost.Invalidate();
            Invalidate(true);
        }

        public EngineeringTabPage AddPage(EngineeringTabPage page)
        {
            Pages.Add(page);
            return page;
        }

        public bool RemovePage(string key)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                if (string.Equals(pages[i].Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    pages.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        internal void RequestAddTab()
        {
            AddTabRequested?.Invoke(this, EventArgs.Empty);
        }

        internal void RequestCloseTab(EngineeringTabPage page)
        {
            if (page == null || !page.CanClose)
                return;

            if (page.ConfirmCloseWhenContentExists && (page.Content != null || page.Model != null))
            {
                DialogResult result = ShowCloseConfirmation(page);
                if (result != DialogResult.OK)
                    return;
            }

            var args = new EngineeringTabClosingEventArgs(page);
            TabClosing?.Invoke(this, args);
            if (args.Cancel)
                return;

            RemovePage(page.Key);
        }

        private DialogResult ShowCloseConfirmation(EngineeringTabPage page)
        {
            using EngineeringDialog dialog = new EngineeringDialog
            {
                DialogTitle = "Fül törlése",
                DialogSubtitle = $"A(z) \"{page.Text}\" fül törlésével a hozzá tartozó adatok elveszhetnek.",
                Severity = EngineeringDialogSeverity.Warning,
                ButtonSet = EngineeringDialogButtonSet.DeleteCancel,
                DialogSize = EngineeringDialogSize.Small,
                IconKind = HvacIconKind.Safety
            };

            Form? ownerForm = FindForm();
            return ownerForm == null ? dialog.ShowDialog() : dialog.ShowDialog(ownerForm);
        }

        internal void SelectPageFromStrip(EngineeringTabPage page)
        {
            if (page == null || !page.IsEnabled)
                return;

            SelectPage(page.Key, true);
        }

        internal Color ResolveSeverityColor(EngineeringTabSeverity severity)
        {
            return severity switch
            {
                EngineeringTabSeverity.Info => palette.Info,
                EngineeringTabSeverity.Success => palette.Success,
                EngineeringTabSeverity.Warning => palette.Warning,
                EngineeringTabSeverity.Danger => palette.Danger,
                _ => palette.Accent
            };
        }

        internal void SetToolTip(Control control, string text)
        {
            toolTip.SetToolTip(control, text);
        }

        private void SelectPage(string key, bool raiseEvent)
        {
            EngineeringTabPage? oldPage = SelectedPage;
            EngineeringTabPage? newPage = FindPage(key);

            if (newPage == null || !newPage.IsEnabled)
                return;

            if (string.Equals(selectedKey, newPage.Key, StringComparison.OrdinalIgnoreCase))
                return;

            selectedKey = newPage.Key;
            ShowPageContent(newPage);
            tabStrip.EnsureSelectedVisible();
            tabStrip.Invalidate();

            if (raiseEvent)
                SelectedTabChanged?.Invoke(this, new EngineeringTabChangedEventArgs(oldPage, newPage));
        }

        private void ShowPageContent(EngineeringTabPage page)
        {
            if (page.Content == null)
                return;

            contentHost.SuspendLayout();
            contentHost.Controls.Clear();
            page.Content.Dock = DockStyle.Fill;
            contentHost.Controls.Add(page.Content);

            contentHost.ResumeLayout(true);
        }

        private void DrawContentBorder(Graphics graphics)
        {
            if (!showBorder || contentHost.Width <= 1 || contentHost.Height <= 1)
                return;

            using Pen borderPen = new Pen(palette.Border, 1f);
            graphics.DrawRectangle(
                borderPen,
                0,
                0,
                contentHost.Width - 1,
                contentHost.Height - 1);
        }

        private EngineeringTabPage? FindPage(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            foreach (EngineeringTabPage page in pages)
            {
                if (string.Equals(page.Key, key, StringComparison.OrdinalIgnoreCase))
                    return page;
            }

            return null;
        }

        internal void OnPagesChanged(EngineeringTabPage changedPage)
        {
            NormalizePage(changedPage);

            if (string.IsNullOrWhiteSpace(selectedKey) && changedPage.IsEnabled)
                SelectPage(changedPage.Key, false);
            else
                tabStrip.Invalidate();
        }

        internal void OnPageRemoved(EngineeringTabPage removed)
        {
            if (!string.Equals(selectedKey, removed.Key, StringComparison.OrdinalIgnoreCase))
            {
                tabStrip.Invalidate();
                return;
            }

            selectedKey = string.Empty;
            contentHost.Controls.Clear();

            foreach (EngineeringTabPage page in pages)
            {
                if (page.IsEnabled)
                {
                    SelectPage(page.Key, true);
                    return;
                }
            }

            SelectedTabChanged?.Invoke(this, new EngineeringTabChangedEventArgs(removed, null));
            tabStrip.Invalidate();
        }

        internal void OnPagesCleared()
        {
            EngineeringTabPage? oldPage = SelectedPage;
            selectedKey = string.Empty;
            contentHost.Controls.Clear();
            SelectedTabChanged?.Invoke(this, new EngineeringTabChangedEventArgs(oldPage, null));
            tabStrip.Invalidate();
        }

        private static void NormalizePage(EngineeringTabPage page)
        {
            if (string.IsNullOrWhiteSpace(page.Key))
                page.Key = Guid.NewGuid().ToString("N");

            if (string.IsNullOrWhiteSpace(page.Text))
                page.Text = "Fül";

            page.Key = page.Key.Trim();
            page.Text = page.Text.Trim();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                toolTip.Dispose();

            base.Dispose(disposing);
        }

        private sealed class TabStripControl : Control
        {
            private readonly EngineeringTabHost owner;
            private readonly Collection<TabHitTarget> hitTargets = new Collection<TabHitTarget>();
            private ThemePalette palette = ThemeManager.CurrentPalette;
            private int scrollOffset;
            private int hoverIndex = -1;
            private Rectangle leftScrollBounds;
            private Rectangle rightScrollBounds;
            private Rectangle addBounds;

            public TabStripControl(EngineeringTabHost owner)
            {
                this.owner = owner;
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.Selectable |
                    ControlStyles.UserPaint,
                    true);

                Cursor = Cursors.Hand;
                TabStop = true;
                Font = ThemeFonts.Body;
                TabStyle = EngineeringTabStyle.Standard;
                OverflowMode = EngineeringTabOverflowMode.ScrollButtons;
            }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public EngineeringTabStyle TabStyle { get; set; }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public EngineeringTabOverflowMode OverflowMode { get; set; }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool ShowAddButton { get; set; }

            public void ApplyTheme(ThemePalette palette)
            {
                this.palette = palette;
                BackColor = palette.Surface;
                ForeColor = palette.TextPrimary;
                Invalidate();
            }

            public void EnsureSelectedVisible()
            {
                if (OverflowMode != EngineeringTabOverflowMode.ScrollButtons)
                    return;

                int selectedIndex = GetSelectedIndex();
                if (selectedIndex < 0)
                    return;

                int reservedRight = ShowAddButton ? 34 : 0;
                if (CalculateTotalWidth() <= Width - reservedRight)
                    return;

                int availableLeft = 28;
                int availableRight = Width - reservedRight - 28;
                Rectangle bounds = CalculateTabBounds(selectedIndex, availableLeft);

                if (bounds.Left < availableLeft)
                    scrollOffset = Math.Max(0, scrollOffset - (availableLeft - bounds.Left));
                else if (bounds.Right > availableRight)
                    scrollOffset += bounds.Right - availableRight;

                int maxScroll = Math.Max(0, CalculateTotalWidth() - (availableRight - availableLeft));
                scrollOffset = Math.Max(0, Math.Min(maxScroll, scrollOffset));
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using SolidBrush backgroundBrush = new SolidBrush(palette.Surface);
                e.Graphics.FillRectangle(backgroundBrush, ClientRectangle);

                DrawTabs(e.Graphics);
                DrawScrollButtons(e.Graphics);
                DrawAddButton(e.Graphics);

                using Pen bottomPen = new Pen(palette.Border, 1f);
                e.Graphics.DrawLine(bottomPen, 0, Height - 1, Width, Height - 1);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                int nextHover = HitTestIndex(e.Location);
                if (hoverIndex != nextHover)
                {
                    hoverIndex = nextHover;
                    UpdateHoverToolTip();
                    Invalidate();
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                hoverIndex = -1;
                owner.SetToolTip(this, string.Empty);
                Invalidate();
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                Focus();

                if (e.Button != MouseButtons.Left)
                    return;

                if (leftScrollBounds.Contains(e.Location))
                {
                    ScrollBy(-120);
                    return;
                }

                if (rightScrollBounds.Contains(e.Location))
                {
                    ScrollBy(120);
                    return;
                }

                if (addBounds.Contains(e.Location))
                {
                    owner.RequestAddTab();
                    return;
                }

                foreach (TabHitTarget target in hitTargets)
                {
                    if (!target.Bounds.Contains(e.Location))
                        continue;

                    if (target.CloseBounds.Contains(e.Location))
                        owner.RequestCloseTab(target.Page);
                    else
                        owner.SelectPageFromStrip(target.Page);

                    return;
                }
            }

            protected override void OnMouseWheel(MouseEventArgs e)
            {
                base.OnMouseWheel(e);
                if (OverflowMode == EngineeringTabOverflowMode.ScrollButtons)
                    ScrollBy(e.Delta > 0 ? -80 : 80);
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);

                if (e.KeyCode == Keys.Left)
                {
                    SelectRelative(-1);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Right)
                {
                    SelectRelative(1);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Home)
                {
                    SelectFirstOrLast(first: true);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.End)
                {
                    SelectFirstOrLast(first: false);
                    e.Handled = true;
                }
            }

            private void DrawTabs(Graphics graphics)
            {
                hitTargets.Clear();

                int reservedRight = 0;
                if (ShowAddButton)
                    reservedRight += 34;
                bool needsScroll = CalculateTotalWidth() > Width - reservedRight;
                bool showScroll = OverflowMode == EngineeringTabOverflowMode.ScrollButtons && needsScroll;

                int left = showScroll ? 28 : 0;
                int rightLimit = Width - reservedRight - (showScroll ? 28 : 0);
                int x = left - (showScroll ? scrollOffset : 0);
                int pageCount = Math.Max(1, owner.Pages.Count);
                int shrinkWidth = Math.Max(72, (rightLimit - left) / pageCount);

                for (int i = 0; i < owner.Pages.Count; i++)
                {
                    EngineeringTabPage page = owner.Pages[i];
                    int width = OverflowMode == EngineeringTabOverflowMode.Shrink
                        ? shrinkWidth
                        : MeasureTabWidth(page);
                    Rectangle bounds = new Rectangle(x, 3, width, Height - 7);
                    x += width + ResolveGap();

                    if (bounds.Right < left || bounds.Left > rightLimit)
                        continue;

                    Rectangle visibleBounds = Rectangle.Intersect(
                        bounds,
                        new Rectangle(left, 3, rightLimit - left, Height - 7));
                    if (visibleBounds.IsEmpty)
                        continue;

                    Region previousClip = graphics.Clip;
                    graphics.SetClip(visibleBounds);
                    DrawTab(graphics, page, bounds, i == hoverIndex);
                    graphics.Clip = previousClip;
                    previousClip.Dispose();
                }

                if (showScroll)
                {
                    int maxScroll = Math.Max(0, CalculateTotalWidth() - (rightLimit - left));
                    scrollOffset = Math.Max(0, Math.Min(maxScroll, scrollOffset));
                    leftScrollBounds = new Rectangle(0, 3, 26, Height - 8);
                    rightScrollBounds = new Rectangle(Width - reservedRight - 26, 3, 26, Height - 8);
                }
                else
                {
                    scrollOffset = 0;
                    leftScrollBounds = Rectangle.Empty;
                    rightScrollBounds = Rectangle.Empty;
                }

                MaskCommandZones(graphics, showScroll, reservedRight);
            }

            private void MaskCommandZones(Graphics graphics, bool showScroll, int reservedRight)
            {
                using SolidBrush surfaceBrush = new SolidBrush(palette.Surface);

                if (showScroll)
                {
                    graphics.FillRectangle(surfaceBrush, 0, 0, 28, Height);
                    int rightZoneLeft = Width - reservedRight - 28;
                    graphics.FillRectangle(
                        surfaceBrush,
                        rightZoneLeft,
                        0,
                        Math.Max(0, Width - rightZoneLeft),
                        Height);
                }
                else if (reservedRight > 0)
                {
                    graphics.FillRectangle(
                        surfaceBrush,
                        Width - reservedRight,
                        0,
                        reservedRight,
                        Height);
                }
            }

            private void DrawTab(
                Graphics graphics,
                EngineeringTabPage page,
                Rectangle bounds,
                bool hovered)
            {
                bool selected = string.Equals(
                    page.Key,
                    owner.SelectedKey,
                    StringComparison.OrdinalIgnoreCase);
                bool enabled = page.IsEnabled;
                Color severityColor = owner.ResolveSeverityColor(page.Severity);

                Color background = selected
                    ? palette.SurfaceAlt
                    : hovered && enabled ? palette.SurfaceHover : palette.Surface;
                Color border = selected ? severityColor : palette.Border;
                Color text = enabled
                    ? selected ? palette.TextPrimary : palette.TextSecondary
                    : palette.TextDisabled;

                if (TabStyle == EngineeringTabStyle.Segmented)
                {
                    background = selected
                        ? Blend(severityColor, palette.Surface, 0.18)
                        : hovered && enabled ? palette.SurfaceHover : palette.SurfaceAlt;
                }

                using SolidBrush backgroundBrush = new SolidBrush(background);
                graphics.FillRectangle(backgroundBrush, bounds);

                if (selected || hovered || TabStyle == EngineeringTabStyle.Segmented)
                {
                    using Pen borderPen = new Pen(border, 1f);
                    graphics.DrawRectangle(borderPen, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1);
                }

                int accentHeight = selected ? 3 : 2;
                using SolidBrush accentBrush = new SolidBrush(severityColor);
                graphics.FillRectangle(
                    accentBrush,
                    bounds.Left,
                    bounds.Bottom - accentHeight,
                    bounds.Width,
                    accentHeight);

                int contentLeft = bounds.Left + 10;
                if (page.IconKind.HasValue)
                {
                    int iconSize = TabStyle == EngineeringTabStyle.Compact ? 14 : 16;
                    Rectangle iconBounds = new Rectangle(
                        contentLeft,
                        bounds.Top + (bounds.Height - iconSize) / 2,
                        iconSize,
                        iconSize);
                    using Bitmap icon = HvacIconRenderer.Render(
                        page.IconKind.Value,
                        ThemeManager.CurrentThemeMode,
                        iconSize,
                        severityColor);
                    graphics.DrawImage(icon, iconBounds);
                    contentLeft += iconSize + 6;
                }

                Rectangle closeBounds = Rectangle.Empty;
                int contentRight = bounds.Right - 8;
                if (page.CanClose)
                {
                    closeBounds = new Rectangle(bounds.Right - 22, bounds.Top + 8, 14, 14);
                    contentRight = closeBounds.Left - 4;
                    TextRenderer.DrawText(
                        graphics,
                        "x",
                        ThemeFonts.Caption,
                        closeBounds,
                        text,
                        TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.NoPadding);
                }

                if (!string.IsNullOrWhiteSpace(page.BadgeText))
                {
                    Size badgeTextSize = TextRenderer.MeasureText(
                        page.BadgeText,
                        ThemeFonts.Tiny,
                        Size.Empty,
                        TextFormatFlags.NoPadding);
                    int badgeWidth = Math.Min(58, badgeTextSize.Width + 12);
                    Rectangle badgeBounds = new Rectangle(
                        contentRight - badgeWidth,
                        bounds.Top + (bounds.Height - 18) / 2,
                        badgeWidth,
                        18);
                    using SolidBrush badgeBrush = new SolidBrush(Blend(severityColor, palette.Surface, 0.28));
                    using Pen badgePen = new Pen(severityColor, 1f);
                    graphics.FillRectangle(badgeBrush, badgeBounds);
                    graphics.DrawRectangle(badgePen, badgeBounds);
                    TextRenderer.DrawText(
                        graphics,
                        page.BadgeText,
                        ThemeFonts.Tiny,
                        badgeBounds,
                        severityColor,
                        TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.EndEllipsis |
                        TextFormatFlags.NoPadding);
                    contentRight = badgeBounds.Left - 6;
                }

                Rectangle textBounds = new Rectangle(
                    contentLeft,
                    bounds.Top,
                    Math.Max(0, contentRight - contentLeft),
                    bounds.Height);

                TextRenderer.DrawText(
                    graphics,
                    page.Text,
                    selected ? ThemeFonts.BodyBold : ThemeFonts.Body,
                    textBounds,
                    text,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding);

                hitTargets.Add(new TabHitTarget(page, bounds, closeBounds));
            }

            private void DrawScrollButtons(Graphics graphics)
            {
                DrawArrowButton(graphics, leftScrollBounds, left: true);
                DrawArrowButton(graphics, rightScrollBounds, left: false);
            }

            private void DrawAddButton(Graphics graphics)
            {
                addBounds = ShowAddButton
                    ? new Rectangle(Width - 32, 3, 28, Height - 8)
                    : Rectangle.Empty;

                if (addBounds.IsEmpty)
                    return;

                DrawTabStripButton(graphics, addBounds);
                DrawPlusIcon(graphics, addBounds);
            }

            private void DrawArrowButton(Graphics graphics, Rectangle bounds, bool left)
            {
                if (bounds.IsEmpty)
                    return;

                DrawTabStripButton(graphics, bounds);
                DrawChevronIcon(graphics, bounds, left);
            }

            private void DrawTabStripButton(Graphics graphics, Rectangle bounds)
            {
                using SolidBrush brush = new SolidBrush(palette.SurfaceAlt);
                using Pen borderPen = new Pen(palette.Border, 1f);
                graphics.FillRectangle(brush, bounds);
                graphics.DrawRectangle(borderPen, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1);
            }

            private void DrawChevronIcon(Graphics graphics, Rectangle bounds, bool left)
            {
                int centerX = bounds.Left + (bounds.Width / 2);
                int centerY = bounds.Top + (bounds.Height / 2);
                Point[] chevron = left
                    ? new[]
                    {
                        new Point(centerX + 3, centerY - 5),
                        new Point(centerX - 3, centerY),
                        new Point(centerX + 3, centerY + 5)
                    }
                    : new[]
                    {
                        new Point(centerX - 3, centerY - 5),
                        new Point(centerX + 3, centerY),
                        new Point(centerX - 3, centerY + 5)
                    };

                using Pen arrowPen = new Pen(palette.TextSecondary, 1.6f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                };

                SmoothingMode previousMode = graphics.SmoothingMode;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.DrawLines(arrowPen, chevron);
                graphics.SmoothingMode = previousMode;
            }

            private void DrawPlusIcon(Graphics graphics, Rectangle bounds)
            {
                int centerX = bounds.Left + (bounds.Width / 2);
                int centerY = bounds.Top + (bounds.Height / 2);

                using Pen plusPen = new Pen(palette.Accent, 1.7f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };

                SmoothingMode previousMode = graphics.SmoothingMode;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.DrawLine(plusPen, centerX - 4, centerY, centerX + 4, centerY);
                graphics.DrawLine(plusPen, centerX, centerY - 4, centerX, centerY + 4);
                graphics.SmoothingMode = previousMode;
            }

            private int MeasureTabWidth(EngineeringTabPage page)
            {
                int width = TabStyle == EngineeringTabStyle.Compact ? 74 : 96;
                Size text = TextRenderer.MeasureText(
                    page.Text,
                    ThemeFonts.Body,
                    Size.Empty,
                    TextFormatFlags.NoPadding);
                width = Math.Max(width, text.Width + 28);

                if (page.IconKind.HasValue)
                    width += 22;
                if (!string.IsNullOrWhiteSpace(page.BadgeText))
                    width += Math.Min(58, page.BadgeText.Length * 7 + 12) + 8;
                if (page.CanClose)
                    width += 20;

                return Math.Min(TabStyle == EngineeringTabStyle.Compact ? 150 : 220, width);
            }

            private int CalculateTotalWidth()
            {
                int width = 0;
                for (int i = 0; i < owner.Pages.Count; i++)
                    width += MeasureTabWidth(owner.Pages[i]) + ResolveGap();
                return Math.Max(0, width - ResolveGap());
            }

            private Rectangle CalculateTabBounds(int pageIndex, int left)
            {
                int x = left - scrollOffset;
                for (int i = 0; i < owner.Pages.Count; i++)
                {
                    int width = OverflowMode == EngineeringTabOverflowMode.Shrink
                        ? Math.Max(72, (Width - left) / Math.Max(1, owner.Pages.Count))
                        : MeasureTabWidth(owner.Pages[i]);

                    if (i == pageIndex)
                        return new Rectangle(x, 3, width, Height - 7);

                    x += width + ResolveGap();
                }

                return Rectangle.Empty;
            }

            private int ResolveGap()
            {
                return TabStyle == EngineeringTabStyle.Segmented ? 6 : 2;
            }

            private int HitTestIndex(Point location)
            {
                for (int i = 0; i < hitTargets.Count; i++)
                {
                    if (hitTargets[i].Bounds.Contains(location))
                        return i;
                }

                return -1;
            }

            private int GetSelectedIndex()
            {
                for (int i = 0; i < owner.Pages.Count; i++)
                {
                    if (string.Equals(owner.Pages[i].Key, owner.SelectedKey, StringComparison.OrdinalIgnoreCase))
                        return i;
                }

                return -1;
            }

            private void ScrollBy(int delta)
            {
                scrollOffset = Math.Max(0, scrollOffset + delta);
                Invalidate();
            }

            private void SelectRelative(int delta)
            {
                if (owner.Pages.Count == 0)
                    return;

                int index = GetSelectedIndex();
                for (int i = 1; i <= owner.Pages.Count; i++)
                {
                    int next = (index + (delta * i) + owner.Pages.Count) % owner.Pages.Count;
                    if (owner.Pages[next].IsEnabled)
                    {
                        owner.SelectPageFromStrip(owner.Pages[next]);
                        return;
                    }
                }
            }

            private void SelectFirstOrLast(bool first)
            {
                if (first)
                {
                    foreach (EngineeringTabPage page in owner.Pages)
                    {
                        if (page.IsEnabled)
                        {
                            owner.SelectPageFromStrip(page);
                            return;
                        }
                    }
                }
                else
                {
                    for (int i = owner.Pages.Count - 1; i >= 0; i--)
                    {
                        if (owner.Pages[i].IsEnabled)
                        {
                            owner.SelectPageFromStrip(owner.Pages[i]);
                            return;
                        }
                    }
                }
            }

            private void UpdateHoverToolTip()
            {
                if (hoverIndex < 0 || hoverIndex >= hitTargets.Count)
                {
                    owner.SetToolTip(this, string.Empty);
                    return;
                }

                EngineeringTabPage page = hitTargets[hoverIndex].Page;
                string text = string.IsNullOrWhiteSpace(page.ToolTipText)
                    ? page.Text
                    : page.ToolTipText;
                owner.SetToolTip(this, text);
            }

            private static Color Blend(Color foreground, Color background, double amount)
            {
                amount = Math.Max(0, Math.Min(1, amount));
                int r = (int)(background.R + ((foreground.R - background.R) * amount));
                int g = (int)(background.G + ((foreground.G - background.G) * amount));
                int b = (int)(background.B + ((foreground.B - background.B) * amount));
                return Color.FromArgb(r, g, b);
            }

            private readonly struct TabHitTarget
            {
                public TabHitTarget(
                    EngineeringTabPage page,
                    Rectangle bounds,
                    Rectangle closeBounds)
                {
                    Page = page;
                    Bounds = bounds;
                    CloseBounds = closeBounds;
                }

                public EngineeringTabPage Page { get; }
                public Rectangle Bounds { get; }
                public Rectangle CloseBounds { get; }
            }
        }
    }
}
