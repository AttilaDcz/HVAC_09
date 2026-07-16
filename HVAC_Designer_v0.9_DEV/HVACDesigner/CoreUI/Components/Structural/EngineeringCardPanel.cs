using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Components.Help;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Structural
{
    public enum EngineeringCardVariant
    {
        Default,
        Flat,
        Elevated
    }

    public enum EngineeringCardStatus
    {
        None,
        Info,
        Success,
        Warning,
        Danger
    }

    public sealed class EngineeringCardPanel : Panel, IThemeable
    {
        private sealed class HeaderActionItem
        {
            public HeaderActionItem(
                HvacIconKind icon,
                string toolTip,
                EngineeringButton proxyButton,
                EventHandler? clickHandler)
            {
                Icon = icon;
                ToolTip = toolTip;
                ProxyButton = proxyButton;
                ClickHandler = clickHandler;
            }

            public HvacIconKind Icon { get; }
            public string ToolTip { get; }
            public EngineeringButton ProxyButton { get; }
            public EventHandler? ClickHandler { get; }
            public Rectangle Bounds { get; set; }
        }

        private sealed class HeaderActionStrip : FlowLayoutPanel
        {
            public HeaderActionStrip()
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw,
                    true);
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // The parent card paints the header. Avoid a child-control background
                // block over the custom title/subtitle rendering.
            }
        }

        private readonly Panel contentPanel;
        private readonly Panel footerPanel;
        private readonly FlowLayoutPanel actionPanel;
        private readonly EngineeringToolTip contentToolTip;
        private readonly EngineeringToolTip headerActionToolTip;
        private readonly List<HeaderActionItem> headerActions = new List<HeaderActionItem>();
        private ThemePalette palette = ThemeManager.CurrentPalette;
        private EngineeringCardVariant variant = EngineeringCardVariant.Default;
        private EngineeringCardStatus status = EngineeringCardStatus.None;
        private HvacIconKind? iconKind;
        private string title = "Szekció";
        private string subtitle = string.Empty;
        private string footerText = string.Empty;
        private bool showHeader = true;
        private bool showTitle = true;
        private bool showSubtitle = true;
        private bool showIcon;
        private bool showAccentStrip = true;
        private bool showSeparator = true;
        private bool showBorder = true;
        private bool showFooter;
        private bool showStatusBadge;
        private bool showHeaderActions = true;
        private bool isCollapsible;
        private bool isCollapsed;
        private int expandedHeight;
        private string contentToolTipText = string.Empty;
        private int hoveredHeaderActionIndex = -1;
        private int pressedHeaderActionIndex = -1;
        private string activeHeaderActionToolTip = string.Empty;
        private bool suppressNextClick;

        public EngineeringCardPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            contentPanel = new Panel
            {
                Tag = "NoTheme"
            };

            footerPanel = new Panel
            {
                Tag = "NoTheme",
                Visible = false
            };

            actionPanel = new HeaderActionStrip
            {
                AutoSize = false,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Tag = "NoTheme"
            };

            contentToolTip = new EngineeringToolTip();
            headerActionToolTip = new EngineeringToolTip();

            Controls.Add(contentPanel);
            Controls.Add(footerPanel);
            Controls.Add(actionPanel);
            actionPanel.Visible = false;

            Size = new Size(360, 180);
            expandedHeight = Height;
            ApplyTheme(palette);
            ArrangeChildPanels();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel ContentPanel => contentPanel;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel FooterPanel => footerPanel;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FlowLayoutPanel HeaderActions => actionPanel;

        [Category("Behavior")]
        [DefaultValue("")]
        public string ContentToolTipText
        {
            get => contentToolTipText;
            set
            {
                contentToolTipText = value ?? string.Empty;
                ApplyContentToolTip();
            }
        }

        [Category("Appearance")]
        [DefaultValue("Szekció")]
        public string Title
        {
            get => title;
            set
            {
                title = value ?? string.Empty;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        public string Subtitle
        {
            get => subtitle;
            set
            {
                subtitle = value ?? string.Empty;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        public string FooterText
        {
            get => footerText;
            set
            {
                footerText = value ?? string.Empty;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(EngineeringCardVariant.Default)]
        public EngineeringCardVariant Variant
        {
            get => variant;
            set
            {
                variant = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(EngineeringCardStatus.None)]
        public EngineeringCardStatus Status
        {
            get => status;
            set
            {
                status = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(null)]
        public HvacIconKind? IconKind
        {
            get => iconKind;
            set
            {
                iconKind = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowHeader
        {
            get => showHeader;
            set
            {
                showHeader = value;
                ArrangeChildPanels();
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowTitle
        {
            get => showTitle;
            set
            {
                showTitle = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowSubtitle
        {
            get => showSubtitle;
            set
            {
                showSubtitle = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool ShowIcon
        {
            get => showIcon;
            set
            {
                showIcon = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowAccentStrip
        {
            get => showAccentStrip;
            set
            {
                showAccentStrip = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowSeparator
        {
            get => showSeparator;
            set
            {
                showSeparator = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowBorder
        {
            get => showBorder;
            set
            {
                showBorder = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool ShowFooter
        {
            get => showFooter;
            set
            {
                showFooter = value;
                ArrangeChildPanels();
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool ShowStatusBadge
        {
            get => showStatusBadge;
            set
            {
                showStatusBadge = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowHeaderActions
        {
            get => showHeaderActions;
            set
            {
                showHeaderActions = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool IsCollapsible
        {
            get => isCollapsible;
            set
            {
                isCollapsible = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool IsCollapsed
        {
            get => isCollapsed;
            set
            {
                if (isCollapsed == value)
                    return;

                if (!isCollapsed)
                    expandedHeight = Height;

                isCollapsed = value;
                ApplyCollapsedState();
                Invalidate();
            }
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            Color background = ResolveBackground();
            BackColor = background;
            ForeColor = palette.TextPrimary;
            contentPanel.BackColor = background;
            footerPanel.BackColor = background;
            actionPanel.BackColor = background;
            contentToolTip.ApplyTheme(palette);
            headerActionToolTip.ApplyTheme(palette);
            ApplyChildTheme(contentPanel);
            ApplyChildTheme(footerPanel);
            ApplyChildTheme(actionPanel);

            ArrangeChildPanels();
            contentPanel.Invalidate(true);
            footerPanel.Invalidate(true);
            actionPanel.Invalidate(true);
            Invalidate(true);
        }

        public EngineeringButton AddHeaderAction(
            HvacIconKind icon,
            string toolTip,
            EventHandler? clickHandler = null)
        {
            EngineeringButton button = new EngineeringButton
            {
                IconKind = icon,
                IconPlacement = EngineeringButtonIconPlacement.IconOnly,
                Variant = EngineeringButtonVariant.Ghost,
                ButtonSize = EngineeringButtonSize.Compact,
                ShowBorder = false,
                Size = new Size(26, 24),
                Margin = new Padding(3, 0, 0, 0)
            };
            button.ApplyTheme(palette);

            if (clickHandler != null)
                button.Click += clickHandler;

            if (!string.IsNullOrWhiteSpace(toolTip))
                headerActionToolTip.SetHelp(button, toolTip, "Kártya művelet", EngineeringToolTipKind.Info);

            actionPanel.Controls.Add(button);
            headerActions.Add(new HeaderActionItem(icon, toolTip ?? string.Empty, button, clickHandler));
            ArrangeChildPanels();
            Invalidate();
            return button;
        }

        public void RefreshContentLayout()
        {
            ApplyChildTheme(contentPanel);
            ApplyChildTheme(footerPanel);
            ApplyContentToolTip();
            ArrangeChildPanels();
            Invalidate(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                contentToolTip.Dispose();
                headerActionToolTip.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);

            if (!isCollapsed)
                expandedHeight = Height;

            ArrangeChildPanels();
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (suppressNextClick)
            {
                suppressNextClick = false;
                return;
            }

            if (isCollapsible && showHeader)
                IsCollapsed = !IsCollapsed;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int hitIndex = HitTestHeaderAction(e.Location);
            if (hoveredHeaderActionIndex != hitIndex)
            {
                hoveredHeaderActionIndex = hitIndex;
                UpdateHeaderActionToolTip(hitIndex);
                Invalidate(GetHeaderActionsInvalidationBounds());
            }

            Cursor = hitIndex >= 0 || (isCollapsible && showHeader && e.Y <= GetHeaderHeight())
                ? Cursors.Hand
                : Cursors.Default;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoveredHeaderActionIndex = -1;
            pressedHeaderActionIndex = -1;
            activeHeaderActionToolTip = string.Empty;
            headerActionToolTip.ClearHelp(this);
            Cursor = Cursors.Default;
            Invalidate(GetHeaderActionsInvalidationBounds());
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            pressedHeaderActionIndex = HitTestHeaderAction(e.Location);
            if (pressedHeaderActionIndex >= 0)
                Invalidate(headerActions[pressedHeaderActionIndex].Bounds);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            int pressed = pressedHeaderActionIndex;
            pressedHeaderActionIndex = -1;

            if (pressed < 0)
                return;

            Invalidate(headerActions[pressed].Bounds);

            if (pressed == HitTestHeaderAction(e.Location))
            {
                suppressNextClick = true;
                headerActions[pressed].ClickHandler?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            Color background = ResolveBackground();
            Color accent = ResolveAccentColor();

            using SolidBrush backgroundBrush = new SolidBrush(background);
            g.FillRectangle(backgroundBrush, bounds);

            if (showAccentStrip)
            {
                using SolidBrush accentBrush = new SolidBrush(accent);
                g.FillRectangle(accentBrush, 0, 0, 4, Height);
            }

            if (showHeader)
                DrawHeader(g, accent);

            if (showFooter && !isCollapsed)
                DrawFooter(g);

            if (showBorder)
            {
                using Pen borderPen = new Pen(ResolveBorderColor());
                g.DrawRectangle(borderPen, bounds);
            }
        }

        private void DrawHeader(Graphics g, Color accent)
        {
            int headerHeight = GetHeaderHeight();
            bool hasSubtitle = showSubtitle && !string.IsNullOrWhiteSpace(subtitle);
            int left = showAccentStrip ? 14 : 12;

            if (showIcon && iconKind.HasValue)
            {
                int iconSize = ThemeMetrics.IconSizeNormal;
                int iconBoxSize = iconSize + 2;
                int iconLeft = left + (iconBoxSize - iconSize) / 2;
                int iconTop = hasSubtitle
                    ? Math.Max(6, 8 + ((22 - iconSize) / 2))
                    : (headerHeight - iconSize) / 2;
                using Bitmap icon = HvacIconRenderer.RenderOutline(
                    iconKind.Value,
                    ThemeManager.CurrentThemeMode,
                    iconSize,
                    accent);
                g.DrawImage(icon, iconLeft, iconTop, iconSize, iconSize);
                left += iconBoxSize + ThemeMetrics.MarginNormal + 4;
            }

            int rightReserve = 14;
            int actionWidth = GetHeaderActionWidth();
            if (actionWidth > 0)
                rightReserve += actionWidth + 8;
            if (showStatusBadge && status != EngineeringCardStatus.None)
                rightReserve += 82;
            if (isCollapsible)
                rightReserve += 24;

            Rectangle titleBounds = new Rectangle(
                left,
                hasSubtitle ? 8 : 0,
                Math.Max(0, Width - left - rightReserve),
                hasSubtitle ? 22 : headerHeight);

            if (showTitle)
            {
                TextRenderer.DrawText(
                    g,
                    title,
                    ThemeFonts.Section,
                    titleBounds,
                    palette.TextPrimary,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.PreserveGraphicsClipping);
            }

            if (hasSubtitle)
            {
                TextRenderer.DrawText(
                    g,
                    subtitle,
                    ThemeFonts.Caption,
                    new Rectangle(left, 31, Math.Max(0, Width - left - rightReserve), 18),
                    palette.TextSecondary,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.PreserveGraphicsClipping);
            }

            if (showStatusBadge && status != EngineeringCardStatus.None)
                DrawStatusBadge(g, accent, headerHeight);

            DrawHeaderActions(g, accent, headerHeight);

            if (isCollapsible)
                DrawCollapseChevron(g, GetChevronBounds(headerHeight), isCollapsed);

            if (showSeparator && !isCollapsed)
            {
                using Pen linePen = new Pen(palette.Border);
                g.DrawLine(linePen, showAccentStrip ? 14 : 12, headerHeight - 1, Width - 12, headerHeight - 1);
            }
        }

        private void DrawHeaderActions(Graphics g, Color accent, int headerHeight)
        {
            if (!showHeaderActions || isCollapsed || headerActions.Count == 0)
                return;

            Rectangle stripBounds = GetHeaderActionStripBounds(headerHeight);
            int x = stripBounds.Right - 24;

            for (int i = 0; i < headerActions.Count; i++)
            {
                Rectangle bounds = new Rectangle(x, stripBounds.Top + 1, 22, 22);
                headerActions[i].Bounds = bounds;

                bool isHovered = i == hoveredHeaderActionIndex;
                bool isPressed = i == pressedHeaderActionIndex;

                if (isHovered || isPressed)
                {
                    Color fill = isPressed
                        ? Blend(accent, ResolveBackground(), 0.18)
                        : Blend(accent, ResolveBackground(), 0.10);
                    using SolidBrush hoverBrush = new SolidBrush(fill);
                    g.FillRectangle(hoverBrush, bounds);
                }

                using Bitmap icon = HvacIconRenderer.Render(
                    headerActions[i].Icon,
                    ThemeManager.CurrentThemeMode,
                    14,
                    isHovered ? accent : palette.TextSecondary);

                int iconLeft = bounds.Left + (bounds.Width - 14) / 2;
                int iconTop = bounds.Top + (bounds.Height - 14) / 2;
                g.DrawImage(icon, iconLeft, iconTop, 14, 14);

                x -= 26;
            }
        }

        private void DrawStatusBadge(Graphics g, Color accent, int headerHeight)
        {
            Rectangle badgeBounds = GetStatusBadgeBounds(headerHeight);
            using SolidBrush badgeBrush = new SolidBrush(Blend(accent, ResolveBackground(), 0.16));
            using Pen badgePen = new Pen(Blend(accent, palette.Border, 0.55));
            g.FillRectangle(badgeBrush, badgeBounds);
            g.DrawRectangle(badgePen, badgeBounds);

            TextRenderer.DrawText(
                g,
                ResolveStatusText(),
                ThemeFonts.Tiny,
                badgeBounds,
                accent,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
        }

        private void DrawCollapseChevron(Graphics g, Rectangle bounds, bool collapsed)
        {
            Point center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
            Point[] points = collapsed
                ? new[]
                {
                    new Point(center.X - 3, center.Y - 5),
                    new Point(center.X + 3, center.Y),
                    new Point(center.X - 3, center.Y + 5)
                }
                : new[]
                {
                    new Point(center.X - 5, center.Y - 2),
                    new Point(center.X, center.Y + 4),
                    new Point(center.X + 5, center.Y - 2)
                };

            using Pen chevronPen = new Pen(palette.TextSecondary, 1.6f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawLines(chevronPen, points);
            g.SmoothingMode = SmoothingMode.None;
        }

        private void DrawFooter(Graphics g)
        {
            if (string.IsNullOrWhiteSpace(footerText))
                return;

            int footerTop = Height - GetFooterHeight();
            using Pen linePen = new Pen(palette.BorderLight);
            g.DrawLine(linePen, 12, footerTop, Width - 12, footerTop);

            TextRenderer.DrawText(
                g,
                footerText,
                ThemeFonts.Tiny,
                new Rectangle(14, footerTop + 6, Math.Max(0, Width - 28), GetFooterHeight() - 8),
                palette.TextDisabled,
                TextFormatFlags.Left |
                TextFormatFlags.Top |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding);
        }

        private void ArrangeChildPanels()
        {
            int headerHeight = showHeader ? GetHeaderHeight() : 0;
            int footerHeight = showFooter && !isCollapsed ? GetFooterHeight() : 0;
            int left = showAccentStrip ? 8 : 0;
            int contentTop = showHeader ? headerHeight + 8 : 12;
            int bottomPadding = showHeader ? 10 : 12;
            int contentHeight = Math.Max(0, Height - contentTop - footerHeight - bottomPadding);

            contentPanel.Bounds = new Rectangle(
                left + 10,
                contentTop,
                Math.Max(0, Width - left - 20),
                contentHeight);
            contentPanel.Visible = !isCollapsed;

            footerPanel.Bounds = new Rectangle(left + 10, Height - footerHeight, Math.Max(0, Width - left - 20), footerHeight);
            footerPanel.Visible = showFooter && !isCollapsed && footerPanel.Controls.Count > 0;

            int actionWidth = GetHeaderActionWidth();
            int actionRight = GetActionRightEdge(headerHeight);
            actionPanel.Bounds = new Rectangle(
                Math.Max(12, actionRight - actionWidth),
                Math.Max(8, (headerHeight - 24) / 2),
                actionWidth,
                26);
            actionPanel.Visible = false;
        }

        private void ApplyChildTheme(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is IThemeable themeable)
                {
                    themeable.ApplyTheme(palette);
                    continue;
                }

                child.BackColor = parent.BackColor;
                child.ForeColor = child is Label ? palette.TextSecondary : palette.TextPrimary;

                if (child is Label)
                    child.Font = ThemeFonts.Caption;

                if (child.HasChildren)
                    ApplyChildTheme(child);
            }
        }

        private void ApplyContentToolTip()
        {
            string text = string.IsNullOrWhiteSpace(contentToolTipText) ? string.Empty : contentToolTipText;
            EngineeringToolTipKind kind = ResolveToolTipKind();
            string tipTitle = string.IsNullOrWhiteSpace(text) ? string.Empty : title;
            contentToolTip.SetHelp(this, text, tipTitle, kind);
            contentToolTip.SetHelp(contentPanel, text, tipTitle, kind);

            foreach (Control child in contentPanel.Controls)
                ApplyContentToolTip(child, text);
        }

        private void ApplyContentToolTip(Control control, string text)
        {
            string tipTitle = string.IsNullOrWhiteSpace(text) ? string.Empty : title;
            contentToolTip.SetHelp(control, text, tipTitle, ResolveToolTipKind());

            foreach (Control child in control.Controls)
                ApplyContentToolTip(child, text);
        }

        private void ApplyCollapsedState()
        {
            if (isCollapsed)
            {
                contentPanel.Visible = false;
                footerPanel.Visible = false;
                Height = GetHeaderHeight();
            }
            else
            {
                Height = Math.Max(expandedHeight, GetHeaderHeight() + 30);
                contentPanel.Visible = true;
                footerPanel.Visible = showFooter && footerPanel.Controls.Count > 0;
            }

            ArrangeChildPanels();
        }

        private int GetHeaderHeight()
        {
            bool hasSubtitle = showSubtitle && !string.IsNullOrWhiteSpace(subtitle);
            return showHeader ? (hasSubtitle ? 58 : 44) : 0;
        }

        private static int GetFooterHeight()
        {
            return 32;
        }

        private int GetHeaderActionWidth()
        {
            if (!showHeaderActions || headerActions.Count == 0 || isCollapsed)
                return 0;

            return Math.Min(150, Math.Max(28, headerActions.Count * 26));
        }

        private Rectangle GetHeaderActionStripBounds(int headerHeight)
        {
            int width = GetHeaderActionWidth();
            if (width <= 0)
                return Rectangle.Empty;

            int right = GetActionRightEdge(headerHeight);
            return new Rectangle(
                Math.Max(12, right - width),
                Math.Max(8, (headerHeight - 24) / 2),
                width,
                24);
        }

        private Rectangle GetHeaderActionsInvalidationBounds()
        {
            if (!showHeader)
                return Rectangle.Empty;

            Rectangle bounds = GetHeaderActionStripBounds(GetHeaderHeight());
            bounds.Inflate(2, 2);
            return bounds;
        }

        private int HitTestHeaderAction(Point location)
        {
            if (!showHeader || !showHeaderActions || isCollapsed)
                return -1;

            for (int i = 0; i < headerActions.Count; i++)
            {
                if (headerActions[i].Bounds.Contains(location))
                    return i;
            }

            return -1;
        }

        private void UpdateHeaderActionToolTip(int hitIndex)
        {
            string text = hitIndex >= 0 ? headerActions[hitIndex].ToolTip : string.Empty;
            if (activeHeaderActionToolTip == text)
                return;

            activeHeaderActionToolTip = text;
            headerActionToolTip.SetHelp(
                this,
                text,
                string.IsNullOrWhiteSpace(text) ? string.Empty : "Kártya művelet",
                EngineeringToolTipKind.Info);
        }

        private EngineeringToolTipKind ResolveToolTipKind()
        {
            return status switch
            {
                EngineeringCardStatus.Info => EngineeringToolTipKind.Info,
                EngineeringCardStatus.Success => EngineeringToolTipKind.Success,
                EngineeringCardStatus.Warning => EngineeringToolTipKind.Warning,
                EngineeringCardStatus.Danger => EngineeringToolTipKind.Danger,
                _ => EngineeringToolTipKind.Neutral
            };
        }

        private int GetActionRightEdge(int headerHeight)
        {
            int right = Width - 12;

            if (isCollapsible)
                right = GetChevronBounds(headerHeight).Left - 6;

            if (showStatusBadge && status != EngineeringCardStatus.None)
                right = GetStatusBadgeBounds(headerHeight).Left - 6;

            return right;
        }

        private Rectangle GetStatusBadgeBounds(int headerHeight)
        {
            int right = Width - 12;
            if (isCollapsible)
                right = GetChevronBounds(headerHeight).Left - 8;

            return new Rectangle(right - 72, (headerHeight - 20) / 2, 72, 20);
        }

        private Rectangle GetChevronBounds(int headerHeight)
        {
            return new Rectangle(Width - 30, 0, 18, headerHeight);
        }

        private Color ResolveBackground()
        {
            return variant switch
            {
                EngineeringCardVariant.Flat => palette.Window,
                EngineeringCardVariant.Elevated => palette.SurfaceAlt,
                _ => palette.Surface
            };
        }

        private Color ResolveBorderColor()
        {
            if (status != EngineeringCardStatus.None)
                return Blend(ResolveAccentColor(), palette.Border, 0.36);

            return variant == EngineeringCardVariant.Flat
                ? palette.BorderLight
                : palette.Border;
        }

        private Color ResolveAccentColor()
        {
            return status switch
            {
                EngineeringCardStatus.Info => palette.Info,
                EngineeringCardStatus.Success => palette.Success,
                EngineeringCardStatus.Warning => palette.Warning,
                EngineeringCardStatus.Danger => palette.Danger,
                _ => palette.Accent
            };
        }

        private string ResolveStatusText()
        {
            return status switch
            {
                EngineeringCardStatus.Info => "Info",
                EngineeringCardStatus.Success => "OK",
                EngineeringCardStatus.Warning => "Figyelés",
                EngineeringCardStatus.Danger => "Kritikus",
                _ => string.Empty
            };
        }

        private static Color Blend(Color foreground, Color background, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            int r = (int)(background.R + ((foreground.R - background.R) * amount));
            int g = (int)(background.G + ((foreground.G - background.G) * amount));
            int b = (int)(background.B + ((foreground.B - background.B) * amount));
            return Color.FromArgb(r, g, b);
        }
    }
}
