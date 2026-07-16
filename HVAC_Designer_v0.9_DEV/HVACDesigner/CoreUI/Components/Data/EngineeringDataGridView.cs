using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Structural;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Data
{
    public enum EngineeringTableRowState
    {
        None,
        Info,
        Success,
        Warning,
        Danger
    }

    public sealed class EngineeringTableRowEditRequestedEventArgs : EventArgs
    {
        public EngineeringTableRowEditRequestedEventArgs(
            int rowIndex,
            DataGridViewRow row,
            object? rowModel)
        {
            RowIndex = rowIndex;
            Row = row ?? throw new ArgumentNullException(nameof(row));
            RowModel = rowModel;
        }

        public int RowIndex { get; }
        public DataGridViewRow Row { get; }
        public object? RowModel { get; }
    }

    public class EngineeringDataGridView : DataGridView, IThemeable
    {
        private readonly Dictionary<string, EngineeringColumnMetadata> columnMetadata =
            new Dictionary<string, EngineeringColumnMetadata>(StringComparer.OrdinalIgnoreCase);

        private const int WmMouseWheel = 0x020A;
        private ThemePalette palette = ThemeManager.CurrentPalette;
        private bool allowMouseWheelBubbleAtEdges = true;
        private string emptyText = "Nincs megjeleníthető adat.";
        private bool showEmptyState = true;

        public EngineeringDataGridView()
        {
            InitializeBehavior();
            ApplyTheme(palette);
        }

        public event EventHandler<EngineeringTableRowEditRequestedEventArgs>? RowEditRequested;

        [Category("Engineering")]
        [DefaultValue(true)]
        public bool AllowMouseWheelBubbleAtEdges
        {
            get => allowMouseWheelBubbleAtEdges;
            set => allowMouseWheelBubbleAtEdges = value;
        }

        [Category("Engineering")]
        [DefaultValue("Nincs megjeleníthető adat.")]
        public string EmptyText
        {
            get => emptyText;
            set
            {
                emptyText = value ?? string.Empty;
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DefaultValue(true)]
        public bool ShowEmptyState
        {
            get => showEmptyState;
            set
            {
                showEmptyState = value;
                Invalidate();
            }
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));

            SuspendLayout();

            try
            {
                BackgroundColor = palette.Surface;
                BackColor = palette.Surface;
                ForeColor = palette.TextPrimary;
                GridColor = palette.BorderLight;
                Font = ThemeFonts.Body;

                DefaultCellStyle.BackColor = palette.Surface;
                DefaultCellStyle.ForeColor = palette.TextPrimary;
                DefaultCellStyle.Font = ThemeFonts.Body;
                DefaultCellStyle.SelectionBackColor = Blend(palette.SurfaceSelected, palette.Surface, 0.26);
                DefaultCellStyle.SelectionForeColor = palette.TextPrimary;
                DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

                AlternatingRowsDefaultCellStyle.BackColor = Blend(palette.SurfaceAlt, palette.Surface, 0.45);
                AlternatingRowsDefaultCellStyle.ForeColor = palette.TextPrimary;
                AlternatingRowsDefaultCellStyle.SelectionBackColor = Blend(palette.SurfaceSelected, palette.SurfaceAlt, 0.30);
                AlternatingRowsDefaultCellStyle.SelectionForeColor = palette.TextPrimary;
                AlternatingRowsDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

                ColumnHeadersDefaultCellStyle.BackColor = palette.SurfaceAlt;
                ColumnHeadersDefaultCellStyle.ForeColor = palette.TextSecondary;
                ColumnHeadersDefaultCellStyle.Font = ThemeFonts.Caption;
                ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                ColumnHeadersDefaultCellStyle.SelectionBackColor = palette.SurfaceAlt;
                ColumnHeadersDefaultCellStyle.SelectionForeColor = palette.TextPrimary;
                ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

                RowHeadersDefaultCellStyle.BackColor = palette.Surface;
                RowHeadersDefaultCellStyle.ForeColor = palette.TextSecondary;
                RowHeadersDefaultCellStyle.SelectionBackColor = palette.Surface;
                RowHeadersDefaultCellStyle.SelectionForeColor = palette.TextPrimary;

                foreach (DataGridViewColumn column in Columns)
                {
                    ApplyColumnDefaults(column);
                    ApplyColumnTheme(column);
                }
            }
            finally
            {
                ResumeLayout();
                Invalidate();
            }
        }

        public DataGridViewTextBoxColumn AddTextColumn(
            string name,
            string headerText,
            float fillWeight,
            DataGridViewContentAlignment alignment = DataGridViewContentAlignment.MiddleLeft)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = headerText,
                FillWeight = fillWeight,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };

            columnMetadata[name] = new EngineeringColumnMetadata();
            column.DefaultCellStyle.Alignment = alignment;
            Columns.Add(column);
            ApplyColumnDefaults(column);
            ApplyColumnTheme(column);
            return column;
        }

        public DataGridViewTextBoxColumn AddNumericColumn(
            string name,
            string headerText,
            float fillWeight)
        {
            return AddTextColumn(
                name,
                headerText,
                fillWeight,
                DataGridViewContentAlignment.MiddleRight);
        }

        public void SetColumnEditable(string columnName, bool editable)
        {
            DataGridViewColumn column = GetColumn(columnName);
            EngineeringColumnMetadata metadata = GetColumnMetadata(column);

            metadata.IsEditable = editable;
            column.ReadOnly = !editable;

            ApplyColumnTheme(column);
            Invalidate();
        }

        public bool IsColumnEditable(string columnName)
        {
            DataGridViewColumn column = GetColumn(columnName);
            return GetColumnMetadata(column).IsEditable;
        }

        public void SetNumericFormat(string columnName, int decimals)
        {
            if (decimals < 0)
                throw new ArgumentOutOfRangeException(nameof(decimals));

            DataGridViewColumn column = GetColumn(columnName);
            EngineeringColumnMetadata metadata = GetColumnMetadata(column);
            metadata.NumericDecimals = decimals;

            column.DefaultCellStyle.Format = $"N{decimals}";
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            Invalidate();
        }

        public void SetRowState(DataGridViewRow row, EngineeringTableRowState state)
        {
            if (row == null)
                return;

            GetRowMetadata(row).State = state;
            InvalidateRow(row.Index);
        }

        public EngineeringTableRowState GetRowState(DataGridViewRow row)
        {
            if (row == null)
                return EngineeringTableRowState.None;

            return GetRowMetadata(row).State;
        }

        public void SetRowModel(DataGridViewRow row, object? model)
        {
            if (row == null)
                return;

            GetRowMetadata(row).Model = model;
        }

        public object? GetRowModel(DataGridViewRow row)
        {
            if (row == null)
                return null;

            return GetRowMetadata(row).Model;
        }

        public T? GetRowModel<T>(DataGridViewRow row) where T : class
        {
            return GetRowModel(row) as T;
        }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            base.OnColumnAdded(e);
            ApplyColumnDefaults(e.Column);
            ApplyColumnTheme(e.Column);
        }

        protected override void OnCellDoubleClick(DataGridViewCellEventArgs e)
        {
            base.OnCellDoubleClick(e);

            if (e.RowIndex < 0 || e.RowIndex >= Rows.Count)
                return;

            DataGridViewRow row = Rows[e.RowIndex];
            RowEditRequested?.Invoke(
                this,
                new EngineeringTableRowEditRequestedEventArgs(
                    e.RowIndex,
                    row,
                    GetRowModel(row)));
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (!Focused && !ContainsFocus && CanFocus)
                Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (allowMouseWheelBubbleAtEdges && ShouldBubbleMouseWheel(e.Delta))
            {
                BubbleMouseWheelToScrollableParent(e.Delta);
                return;
            }

            ScrollRowsByWheelDelta(e.Delta);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmMouseWheel)
            {
                if (!Focused && !ContainsFocus)
                    Focus();

                int delta = unchecked((short)((long)m.WParam >> 16));
                if (allowMouseWheelBubbleAtEdges && ShouldBubbleMouseWheel(delta))
                {
                    BubbleMouseWheelToScrollableParent(delta);
                    return;
                }

                ScrollRowsByWheelDelta(delta);
                return;
            }

            base.WndProc(ref m);
        }

        protected override void OnRowPostPaint(DataGridViewRowPostPaintEventArgs e)
        {
            base.OnRowPostPaint(e);

            if (e.RowIndex < 0 || e.RowIndex >= Rows.Count)
                return;

            DataGridViewRow row = Rows[e.RowIndex];
            EngineeringTableRowState state = GetRowMetadata(row).State;

            bool selected = row.Selected;
            Color accentColor = ResolveStateColor(state, selected);

            if (state == EngineeringTableRowState.None && !selected)
                return;

            Rectangle rowBounds = new Rectangle(
                e.RowBounds.Left,
                e.RowBounds.Top + 1,
                3,
                Math.Max(0, e.RowBounds.Height - 2));

            using SolidBrush brush = new SolidBrush(accentColor);
            e.Graphics.FillRectangle(brush, rowBounds);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!showEmptyState || Rows.Count > 0 || string.IsNullOrWhiteSpace(emptyText))
                return;

            Rectangle textBounds = new Rectangle(
                12,
                ColumnHeadersVisible ? ColumnHeadersHeight + 10 : 12,
                Math.Max(0, Width - 24),
                Math.Max(24, Height - (ColumnHeadersVisible ? ColumnHeadersHeight : 0) - 20));

            TextRenderer.DrawText(
                e.Graphics,
                emptyText,
                ThemeFonts.Caption,
                textBounds,
                palette.TextSecondary,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
        }

        private void InitializeBehavior()
        {
            SuspendLayout();

            try
            {
                BorderStyle = BorderStyle.None;
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

                EnableHeadersVisualStyles = false;
                RowHeadersVisible = false;

                AllowUserToAddRows = false;
                AllowUserToDeleteRows = false;
                AllowUserToResizeRows = false;
                AllowUserToResizeColumns = true;
                ReadOnly = false;
                EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;

                MultiSelect = false;
                SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                ColumnHeadersHeight = 34;
                RowTemplate.Height = 31;

                ShowCellToolTips = true;
                EnableDoubleBuffering();
            }
            finally
            {
                ResumeLayout();
            }
        }

        private void ApplyColumnDefaults(DataGridViewColumn column)
        {
            if (column == null)
                return;

            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            column.DefaultCellStyle.Font = ThemeFonts.Body;

            if (column.DefaultCellStyle.Alignment == DataGridViewContentAlignment.NotSet)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
        }

        private void ApplyColumnTheme(DataGridViewColumn column)
        {
            if (column == null)
                return;

            EngineeringColumnMetadata metadata = GetColumnMetadata(column);

            if (metadata.IsEditable)
            {
                column.DefaultCellStyle.BackColor = Blend(palette.Accent, palette.Surface, 0.08);
                column.DefaultCellStyle.SelectionBackColor = Blend(palette.SurfaceSelected, palette.Surface, 0.34);
                column.DefaultCellStyle.ForeColor = palette.TextPrimary;
            }
            else
            {
                column.DefaultCellStyle.BackColor = Color.Empty;
                column.DefaultCellStyle.SelectionBackColor = Color.Empty;
                column.DefaultCellStyle.ForeColor = palette.TextPrimary;
            }

            if (metadata.NumericDecimals.HasValue)
            {
                column.DefaultCellStyle.Format = $"N{metadata.NumericDecimals.Value}";
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void ScrollRowsByWheelDelta(int delta)
        {
            if (Rows.Count == 0)
                return;

            int displayedRows = DisplayedRowCount(false);
            int visibleRows = displayedRows > 0 ? displayedRows : 3;
            int step = Math.Max(1, Math.Min(visibleRows - 1, Math.Abs(delta) / 120));

            if (step <= 0)
                step = 1;

            int direction = delta > 0 ? -1 : 1;
            int nextIndex = FindNextVisibleRowIndex(FirstDisplayedScrollingRowIndexSafe(), direction, step);
            if (nextIndex >= 0)
                FirstDisplayedScrollingRowIndex = nextIndex;
        }

        private bool ShouldBubbleMouseWheel(int delta)
        {
            if (Rows.Count == 0)
                return true;

            int displayedRows = DisplayedRowCount(false);
            int visibleRows = CountVisibleRows();
            if (visibleRows <= Math.Max(1, displayedRows))
                return true;

            int firstDisplayed = FirstDisplayedScrollingRowIndexSafe();
            if (firstDisplayed < 0)
                return true;

            int direction = delta > 0 ? -1 : 1;
            if (direction < 0)
                return firstDisplayed <= FirstVisibleRowIndex();

            return LastDisplayedVisibleRowIndex() >= LastVisibleRowIndex();
        }

        private void BubbleMouseWheelToScrollableParent(int delta)
        {
            HVACScrollableContainer? parent = FindScrollableParent();
            if (parent == null)
                return;

            parent.ScrollByWheelDeltaFromChild(delta);
        }

        private HVACScrollableContainer? FindScrollableParent()
        {
            Control? parent = Parent;
            while (parent != null)
            {
                if (parent is HVACScrollableContainer scrollable)
                    return scrollable;

                parent = parent.Parent;
            }

            return null;
        }

        private int CountVisibleRows()
        {
            int count = 0;
            foreach (DataGridViewRow row in Rows)
            {
                if (row.Visible)
                    count++;
            }

            return count;
        }

        private int FirstVisibleRowIndex()
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                if (Rows[i].Visible)
                    return i;
            }

            return -1;
        }

        private int LastVisibleRowIndex()
        {
            for (int i = Rows.Count - 1; i >= 0; i--)
            {
                if (Rows[i].Visible)
                    return i;
            }

            return -1;
        }

        private int LastDisplayedVisibleRowIndex()
        {
            int displayed = DisplayedRowCount(false);
            int first = FirstDisplayedScrollingRowIndexSafe();
            if (first < 0 || displayed <= 0)
                return first;

            int index = first;
            int remaining = displayed - 1;
            while (remaining > 0)
            {
                int next = FindNextVisibleRowIndex(index, 1, 1);
                if (next <= index)
                    break;

                index = next;
                remaining--;
            }

            return index;
        }

        private int FirstDisplayedScrollingRowIndexSafe()
        {
            try
            {
                return FirstDisplayedScrollingRowIndex;
            }
            catch (InvalidOperationException)
            {
                return FindNextVisibleRowIndex(-1, 1, 1);
            }
        }

        private int FindNextVisibleRowIndex(int currentIndex, int direction, int steps)
        {
            if (Rows.Count == 0)
                return -1;

            int index = currentIndex < 0
                ? direction > 0 ? -1 : Rows.Count
                : currentIndex;

            int remaining = Math.Max(1, steps);
            while (remaining > 0)
            {
                int candidate = index + direction;
                if (candidate < 0 || candidate >= Rows.Count)
                    return index >= 0 && index < Rows.Count ? index : -1;

                index = candidate;
                if (Rows[index].Visible)
                    remaining--;
            }

            return index;
        }

        private DataGridViewColumn GetColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Az oszlop neve nem lehet üres.", nameof(columnName));

            DataGridViewColumn? column = Columns[columnName];
            if (column == null)
                throw new ArgumentException($"Nem található ilyen oszlop: {columnName}", nameof(columnName));

            return column;
        }

        private EngineeringColumnMetadata GetColumnMetadata(DataGridViewColumn column)
        {
            string key = string.IsNullOrWhiteSpace(column.Name)
                ? column.Index.ToString()
                : column.Name;

            if (!columnMetadata.TryGetValue(key, out EngineeringColumnMetadata? metadata))
            {
                metadata = new EngineeringColumnMetadata();
                columnMetadata[key] = metadata;
            }

            return metadata;
        }

        private static EngineeringRowMetadata GetRowMetadata(DataGridViewRow row)
        {
            if (row.Tag is EngineeringRowMetadata metadata)
                return metadata;

            metadata = new EngineeringRowMetadata();

            if (row.Tag is EngineeringTableRowState legacyState)
                metadata.State = legacyState;
            else
                metadata.Model = row.Tag;

            row.Tag = metadata;
            return metadata;
        }

        private Color ResolveStateColor(EngineeringTableRowState state, bool selected)
        {
            return state switch
            {
                EngineeringTableRowState.Info => palette.Info,
                EngineeringTableRowState.Success => palette.Success,
                EngineeringTableRowState.Warning => palette.Warning,
                EngineeringTableRowState.Danger => palette.Danger,
                _ => selected ? palette.Accent : palette.Border
            };
        }

        private static Color Blend(Color foreground, Color background, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            int r = (int)(background.R + (foreground.R - background.R) * amount);
            int g = (int)(background.G + (foreground.G - background.G) * amount);
            int b = (int)(background.B + (foreground.B - background.B) * amount);
            return Color.FromArgb(r, g, b);
        }

        private void EnableDoubleBuffering()
        {
            typeof(DataGridView)
                .GetProperty(
                    "DoubleBuffered",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic)
                ?.SetValue(this, true);
        }

        private sealed class EngineeringColumnMetadata
        {
            public bool IsEditable { get; set; }
            public int? NumericDecimals { get; set; }
        }

        private sealed class EngineeringRowMetadata
        {
            public EngineeringTableRowState State { get; set; }
            public object? Model { get; set; }
        }
    }
}
