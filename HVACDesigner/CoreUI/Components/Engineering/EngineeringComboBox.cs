using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Engineering
{
    public class EngineeringComboBox : UserControl, IThemeable
    {
        private readonly Label titleLabel;
        private readonly DropDownSurface inputSurface;
        private readonly EngineeringComboBoxItemCollection items;

        private ThemePalette palette = ThemeManager.CurrentPalette;
        private ToolStripDropDown? dropDown;
        private DropDownListPanel? dropDownList;
        private bool isFocused;
        private bool readOnly;
        private bool isAdjustingBounds;
        private int selectedIndex = -1;
        private EngineeringLabelPosition labelPosition = EngineeringLabelPosition.Top;
        private DateTime lastDropDownClosedUtc = DateTime.MinValue;

        public EngineeringComboBox()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            TabStop = true;
            items = new EngineeringComboBoxItemCollection(this);

            titleLabel = new Label
            {
                AutoEllipsis = true,
                Text = "Választás",
                TextAlign = ContentAlignment.MiddleLeft,
                UseCompatibleTextRendering = false
            };

            inputSurface = new DropDownSurface
            {
                Cursor = Cursors.Hand,
                Height = Math.Max(30, ThemeMetrics.TextBoxHeight + 8)
            };

            inputSurface.Click += (_, _) => ToggleDropDown();
            inputSurface.MouseDown += (_, _) => Focus();

            Controls.Add(titleLabel);
            Controls.Add(inputSurface);

            Size = new Size(180, 58);
            MinimumSize = new Size(96, 34);

            ApplyTheme(palette);
            UpdateDisplayedText();
            PerformLayout();
        }

        public event EventHandler? SelectedIndexChanged;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EngineeringComboBoxItemCollection Items => items;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object? SelectedItem
        {
            get => selectedIndex >= 0 && selectedIndex < items.Count
                ? items[selectedIndex]
                : null;
            set
            {
                int index = -1;
                for (int i = 0; i < items.Count; i++)
                {
                    if (Equals(items[i], value))
                    {
                        index = i;
                        break;
                    }
                }

                SelectedIndex = index;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                int normalized = value < 0 || value >= items.Count ? -1 : value;
                if (selectedIndex == normalized)
                    return;

                selectedIndex = normalized;
                UpdateDisplayedText();
                inputSurface.Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("Engineering")]
        [DefaultValue(true)]
        public bool LabelVisible
        {
            get => titleLabel.Visible;
            set
            {
                titleLabel.Visible = value;
                PerformLayout();
            }
        }

        [Category("Engineering")]
        [DefaultValue("Választás")]
        public string LabelText
        {
            get => titleLabel.Text;
            set
            {
                titleLabel.Text = NormalizeLineBreaks(value);
                PerformLayout();
            }
        }

        [Category("Engineering")]
        [DefaultValue(EngineeringLabelPosition.Top)]
        public EngineeringLabelPosition LabelPosition
        {
            get => labelPosition;
            set
            {
                if (labelPosition == value)
                    return;

                labelPosition = value;
                PerformLayout();
            }
        }

        [Category("Engineering")]
        [DefaultValue("")]
        public string PlaceholderText { get; set; } = string.Empty;

        [Category("Engineering")]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get => readOnly;
            set
            {
                readOnly = value;
                inputSurface.IsReadOnly = value;
                ApplyTheme(palette);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DisplayMember { get; set; } = string.Empty;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ValueMember { get; set; } = string.Empty;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object? SelectedValue
        {
            get
            {
                object? item = SelectedItem;
                return item == null || string.IsNullOrWhiteSpace(ValueMember)
                    ? item
                    : TypeDescriptor.GetProperties(item)[ValueMember]?.GetValue(item);
            }
            set
            {
                if (value is null)
                {
                    SelectedIndex = -1;
                    return;
                }

                for (int i = 0; i < items.Count; i++)
                {
                    object? itemValue = string.IsNullOrWhiteSpace(ValueMember)
                        ? items[i]
                        : TypeDescriptor.GetProperties(items[i])[ValueMember]?.GetValue(items[i]);

                    if (Equals(itemValue, value))
                    {
                        SelectedIndex = i;
                        return;
                    }
                }

                SelectedIndex = -1;
            }
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));

            BackColor = Color.Transparent;
            ForeColor = palette.TextPrimary;

            titleLabel.BackColor = Color.Transparent;
            titleLabel.ForeColor = palette.TextSecondary;
            titleLabel.Font = ThemeFonts.Caption;

            inputSurface.ApplyTheme(palette);
            inputSurface.IsFocused = isFocused;
            inputSurface.IsReadOnly = readOnly || !Enabled;
            inputSurface.Font = ThemeFonts.Body;
            inputSurface.Invalidate();

            dropDownList?.ApplyTheme(palette);

            Invalidate(true);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            inputSurface.IsReadOnly = readOnly || !Enabled;
            ApplyTheme(palette);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            SetFocusState(true);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (dropDown == null || !dropDown.Visible)
                SetFocusState(false);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                ShowDropDown();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                CloseDropDown();
                e.Handled = true;
            }
        }

        protected override void SetBoundsCore(
            int x,
            int y,
            int width,
            int height,
            BoundsSpecified specified)
        {
            if (!isAdjustingBounds &&
                ShouldUseInputAsLayoutOrigin() &&
                (specified & BoundsSpecified.Y) == BoundsSpecified.Y)
            {
                y -= GetTopLabelOffset();
            }

            isAdjustingBounds = true;
            try
            {
                base.SetBoundsCore(x, y, width, height, specified);
            }
            finally
            {
                isAdjustingBounds = false;
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            int inputHeight = Math.Max(30, ThemeMetrics.TextBoxHeight + 8);
            int gap = ThemeMetrics.MarginSmall;
            bool hasExplicitLineBreak = HasExplicitLineBreak(titleLabel.Text);
            int singleLineLabelHeight = Math.Max(17, TextRenderer.MeasureText("Ag", titleLabel.Font).Height);
            int labelHeight = hasExplicitLineBreak
                ? Math.Max(singleLineLabelHeight * 2, inputHeight)
                : singleLineLabelHeight;

            Rectangle inputBounds;

            if (titleLabel.Visible && labelPosition == EngineeringLabelPosition.Top)
            {
                titleLabel.Location = new Point(0, 0);
                titleLabel.Size = new Size(Width, labelHeight);
                inputBounds = new Rectangle(0, labelHeight + gap, Width, inputHeight);
            }
            else if (titleLabel.Visible)
            {
                int labelWidth = Math.Min(Math.Max(76, Width / 3), 140);
                int leftLabelHeight = hasExplicitLineBreak ? inputHeight : singleLineLabelHeight;
                int leftLabelTop = hasExplicitLineBreak
                    ? 0
                    : Math.Max(0, (inputHeight - leftLabelHeight) / 2);

                titleLabel.Location = new Point(0, leftLabelTop);
                titleLabel.Size = new Size(labelWidth, leftLabelHeight);
                inputBounds = new Rectangle(
                    labelWidth + gap,
                    0,
                    Math.Max(40, Width - labelWidth - gap),
                    inputHeight);
            }
            else
            {
                titleLabel.Bounds = Rectangle.Empty;
                inputBounds = new Rectangle(0, 0, Width, inputHeight);
            }

            inputSurface.Location = inputBounds.Location;
            inputSurface.Size = inputBounds.Size;
        }

        private void ToggleDropDown()
        {
            if (dropDown != null && dropDown.Visible)
            {
                CloseDropDown();
                return;
            }

            if ((DateTime.UtcNow - lastDropDownClosedUtc).TotalMilliseconds < 180)
                return;

            ShowDropDown();
        }

        private void ShowDropDown()
        {
            if (!Enabled || readOnly || items.Count == 0)
                return;

            Focus();
            SetFocusState(true);

            CloseDropDown();

            int itemHeight = Math.Max(24, ThemeFonts.Body.Height + 10);
            int visibleItems = Math.Min(items.Count, 8);
            int popupHeight = Math.Max(itemHeight, visibleItems * itemHeight);
            int popupWidth = Math.Max(inputSurface.Width, 120);

            dropDownList = new DropDownListPanel(this, palette, itemHeight)
            {
                Size = new Size(popupWidth, popupHeight)
            };

            ToolStripControlHost host = new ToolStripControlHost(dropDownList)
            {
                AutoSize = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Size = dropDownList.Size
            };

            dropDown = new ToolStripDropDown
            {
                AutoClose = true,
                AutoSize = false,
                BackColor = palette.Surface,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };
            dropDown.Items.Add(host);
            dropDown.Size = host.Size;
            ToolStripDropDown currentDropDown = dropDown;
            dropDown.Closed += (_, _) =>
            {
                if (ReferenceEquals(dropDown, currentDropDown))
                {
                    dropDown = null;
                    dropDownList = null;
                    lastDropDownClosedUtc = DateTime.UtcNow;
                }

                SetFocusState(Focused);
            };

            Point screenPoint = inputSurface.PointToScreen(new Point(0, inputSurface.Height + 1));
            dropDown.Show(screenPoint);
        }

        private void CloseDropDown()
        {
            if (dropDown == null)
                return;

            dropDown.Close();
        }

        private void SelectFromDropDown(int index)
        {
            SelectedIndex = index;
            CloseDropDown();
            Focus();
        }

        private void SetFocusState(bool focused)
        {
            isFocused = focused;
            inputSurface.IsFocused = focused;
            inputSurface.Invalidate();
        }

        private void OnItemsChanged()
        {
            if (selectedIndex >= items.Count)
                selectedIndex = -1;

            UpdateDisplayedText();
            inputSurface.Invalidate();
            dropDownList?.Invalidate();
        }

        private void UpdateDisplayedText()
        {
            inputSurface.TextValue = SelectedItem == null
                ? PlaceholderText
                : GetItemText(SelectedItem);
            inputSurface.IsPlaceholder = SelectedItem == null;
        }

        private string GetItemText(object? item)
        {
            if (item == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(DisplayMember))
            {
                object? value = TypeDescriptor.GetProperties(item)[DisplayMember]?.GetValue(item);
                return value?.ToString() ?? string.Empty;
            }

            return item.ToString() ?? string.Empty;
        }

        private bool ShouldUseInputAsLayoutOrigin()
        {
            return titleLabel != null &&
                   titleLabel.Visible &&
                   labelPosition == EngineeringLabelPosition.Top;
        }

        private int GetTopLabelOffset()
        {
            if (!ShouldUseInputAsLayoutOrigin())
                return 0;

            bool hasExplicitLineBreak = HasExplicitLineBreak(titleLabel.Text);
            int singleLineLabelHeight = Math.Max(17, TextRenderer.MeasureText("Ag", titleLabel.Font).Height);
            int inputHeight = Math.Max(30, ThemeMetrics.TextBoxHeight + 8);
            int labelHeight = hasExplicitLineBreak
                ? Math.Max(singleLineLabelHeight * 2, inputHeight)
                : singleLineLabelHeight;

            return labelHeight + ThemeMetrics.MarginSmall;
        }

        private static string NormalizeLineBreaks(string? text)
        {
            return (text ?? string.Empty)
                .Replace("\\n", Environment.NewLine, StringComparison.Ordinal)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("\r", "\n", StringComparison.Ordinal)
                .Replace("\n", Environment.NewLine, StringComparison.Ordinal);
        }

        private static bool HasExplicitLineBreak(string text)
        {
            return text.Contains('\n') || text.Contains('\r');
        }

        public sealed class EngineeringComboBoxItemCollection : Collection<object>
        {
            private readonly EngineeringComboBox owner;

            internal EngineeringComboBoxItemCollection(EngineeringComboBox owner)
            {
                this.owner = owner;
            }

            public void AddRange(object[] values)
            {
                if (values == null)
                    return;

                foreach (object value in values)
                {
                    Add(value);
                }
            }

            protected override void InsertItem(int index, object item)
            {
                base.InsertItem(index, item);
                owner.OnItemsChanged();
            }

            protected override void SetItem(int index, object item)
            {
                base.SetItem(index, item);
                owner.OnItemsChanged();
            }

            protected override void RemoveItem(int index)
            {
                base.RemoveItem(index);
                owner.OnItemsChanged();
            }

            protected override void ClearItems()
            {
                base.ClearItems();
                owner.OnItemsChanged();
            }
        }

        private sealed class DropDownSurface : Panel
        {
            private ThemePalette palette = ThemeManager.CurrentPalette;

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public string TextValue { get; set; } = string.Empty;

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool IsPlaceholder { get; set; }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool IsFocused { get; set; }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool IsReadOnly { get; set; }

            public DropDownSurface()
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint,
                    true);
            }

            public void ApplyTheme(ThemePalette palette)
            {
                this.palette = palette;
                BackColor = IsReadOnly ? palette.Surface : palette.SurfaceAlt;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
                using SolidBrush backgroundBrush = new SolidBrush(IsReadOnly ? palette.Surface : palette.SurfaceAlt);
                e.Graphics.FillRectangle(backgroundBrush, bounds);

                Color borderColor = IsFocused ? palette.BorderStrong : palette.Border;
                using Pen borderPen = new Pen(borderColor, 1f);
                e.Graphics.DrawRectangle(borderPen, bounds);

                if (IsFocused)
                {
                    Rectangle innerBounds = new Rectangle(1, 1, Width - 3, Height - 3);
                    e.Graphics.DrawRectangle(borderPen, innerBounds);
                }

                Color textColor = IsReadOnly
                    ? palette.TextDisabled
                    : IsPlaceholder
                        ? palette.TextSecondary
                        : palette.TextPrimary;

                TextRenderer.DrawText(
                    e.Graphics,
                    TextValue,
                    Font,
                    new Rectangle(10, 0, Math.Max(0, Width - 34), Height),
                    textColor,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding);

                int centerX = Width - 17;
                int centerY = Height / 2;
                Point[] arrow =
                {
                    new Point(centerX - 4, centerY - 2),
                    new Point(centerX + 4, centerY - 2),
                    new Point(centerX, centerY + 3)
                };

                using SolidBrush arrowBrush = new SolidBrush(palette.TextSecondary);
                e.Graphics.FillPolygon(arrowBrush, arrow);
            }
        }

        private sealed class DropDownListPanel : Panel
        {
            private readonly EngineeringComboBox owner;
            private readonly int itemHeight;
            private ThemePalette palette;
            private int scrollIndex;
            private int hoverIndex = -1;

            public DropDownListPanel(EngineeringComboBox owner, ThemePalette palette, int itemHeight)
            {
                this.owner = owner;
                this.palette = palette;
                this.itemHeight = itemHeight;

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint,
                    true);

                Font = ThemeFonts.Body;
                BackColor = palette.Surface;
                scrollIndex = 0;
            }

            public void ApplyTheme(ThemePalette palette)
            {
                this.palette = palette;
                BackColor = palette.Surface;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                using SolidBrush backgroundBrush = new SolidBrush(palette.Surface);
                e.Graphics.FillRectangle(backgroundBrush, ClientRectangle);

                ClampScrollIndex();

                int visibleItems = GetVisibleItemCount();
                int end = Math.Min(owner.items.Count, scrollIndex + visibleItems);

                for (int itemIndex = scrollIndex; itemIndex < end; itemIndex++)
                {
                    int y = (itemIndex - scrollIndex) * itemHeight;
                    Rectangle itemBounds = new Rectangle(0, y, Width - GetScrollBarWidth(), itemHeight);

                    bool selected = itemIndex == owner.selectedIndex;
                    bool hovered = itemIndex == hoverIndex;
                    Color backColor = selected
                        ? palette.SurfaceSelected
                        : hovered
                            ? palette.SurfaceHover
                            : palette.Surface;

                    using SolidBrush itemBrush = new SolidBrush(backColor);
                    e.Graphics.FillRectangle(itemBrush, itemBounds);

                    TextRenderer.DrawText(
                        e.Graphics,
                        owner.GetItemText(owner.items[itemIndex]),
                        Font,
                        new Rectangle(itemBounds.Left + 10, itemBounds.Top, Math.Max(0, itemBounds.Width - 18), itemBounds.Height),
                        selected ? Color.White : palette.TextPrimary,
                        TextFormatFlags.Left |
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.EndEllipsis |
                        TextFormatFlags.NoPadding);
                }

                DrawScrollBar(e.Graphics);

                using Pen borderPen = new Pen(palette.Border, 1f);
                e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                int index = HitTest(e.Location);
                if (hoverIndex != index)
                {
                    hoverIndex = index;
                    Invalidate();
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                hoverIndex = -1;
                Invalidate();
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                int index = HitTest(e.Location);
                if (index >= 0)
                    owner.SelectFromDropDown(index);
            }

            protected override void OnMouseWheel(MouseEventArgs e)
            {
                base.OnMouseWheel(e);
                ScrollBy(e.Delta > 0 ? -1 : 1);
            }

            private int HitTest(Point location)
            {
                if (location.X >= Width - GetScrollBarWidth())
                    return -1;

                int index = scrollIndex + location.Y / itemHeight;
                return index >= 0 && index < owner.items.Count ? index : -1;
            }

            private void ScrollBy(int delta)
            {
                int next = Math.Max(0, Math.Min(GetMaxScrollIndex(), scrollIndex + delta));
                if (next == scrollIndex)
                    return;

                scrollIndex = next;
                Invalidate();
            }

            private int GetVisibleItemCount()
            {
                return Math.Max(1, Height / itemHeight);
            }

            private int GetScrollBarWidth()
            {
                return owner.items.Count > GetVisibleItemCount() ? 7 : 0;
            }

            private int GetMaxScrollIndex()
            {
                return Math.Max(0, owner.items.Count - GetVisibleItemCount());
            }

            private void ClampScrollIndex()
            {
                scrollIndex = Math.Max(0, Math.Min(GetMaxScrollIndex(), scrollIndex));
            }

            private void DrawScrollBar(Graphics graphics)
            {
                int visibleItems = GetVisibleItemCount();
                if (owner.items.Count <= visibleItems)
                    return;

                int barWidth = GetScrollBarWidth();
                Rectangle track = new Rectangle(Width - barWidth, 0, barWidth, Height);
                using SolidBrush trackBrush = new SolidBrush(palette.SurfaceAlt);
                graphics.FillRectangle(trackBrush, track);

                int thumbHeight = Math.Max(24, (int)(Height * (visibleItems / (double)owner.items.Count)));
                int maxScroll = Math.Max(1, owner.items.Count - visibleItems);
                int thumbTop = (int)((Height - thumbHeight) * (scrollIndex / (double)maxScroll));

                Rectangle thumb = new Rectangle(Width - barWidth, thumbTop, barWidth, thumbHeight);
                using SolidBrush thumbBrush = new SolidBrush(palette.Border);
                graphics.FillRectangle(thumbBrush, thumb);
            }
        }
    }
}
