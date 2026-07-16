using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Charts
{
    public enum EngineeringChartAxisPosition
    {
        Left,
        Right,
        Bottom,
        Top
    }

    public enum EngineeringChartLinePattern
    {
        Solid,
        Dash,
        Dot,
        DashDot
    }

    public enum EngineeringChartLegendPlacement
    {
        Hidden,
        InsideTopRight,
        Right,
        Manual
    }

    public readonly struct EngineeringChartPoint
    {
        public EngineeringChartPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }
    }

    public sealed class EngineeringChartAxis
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public EngineeringChartAxisPosition Position { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; } = 100;
        public int TickCount { get; set; } = 6;
        public double? MajorTickInterval { get; set; }
        public string LabelFormat { get; set; } = "0.#";
        public bool ShowGrid { get; set; } = true;
        public bool Visible { get; set; } = true;

        public string DisplayTitle
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Unit))
                    return Title;

                return string.IsNullOrWhiteSpace(Title)
                    ? Unit
                    : $"{Title} [{Unit}]";
            }
        }
    }

    public sealed class EngineeringChartSeries
    {
        public string Name { get; set; } = string.Empty;
        public string XAxisKey { get; set; } = "x";
        public string YAxisKey { get; set; } = "y";
        public List<EngineeringChartPoint> Points { get; } = new List<EngineeringChartPoint>();
        public Color? Color { get; set; }
        public EngineeringChartLinePattern LinePattern { get; set; } = EngineeringChartLinePattern.Solid;
        public float StrokeWidth { get; set; } = 2f;
        public bool ShowMarkers { get; set; } = true;
        public bool Visible { get; set; } = true;
    }

    public sealed class EngineeringChartWorkingPoint
    {
        public string Name { get; set; } = string.Empty;
        public string XAxisKey { get; set; } = "x";
        public string YAxisKey { get; set; } = "y";
        public double X { get; set; }
        public double Y { get; set; }
        public Color? Color { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool Visible { get; set; } = true;
    }

    public sealed class EngineeringChart : Control, IThemeable
    {
        private readonly List<Color> seriesPalette = new List<Color>();
        private ThemePalette palette = ThemeManager.CurrentPalette;
        private HoverInfo? hoverInfo;
        private Rectangle plotArea;

        public EngineeringChart()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = palette.Surface;
            ForeColor = palette.TextPrimary;
            Font = ThemeFonts.Body;
            MinimumSize = new Size(240, 180);
            Padding = new Padding(10);

            MouseMove += HandleMouseMove;
            MouseLeave += (_, _) =>
            {
                hoverInfo = null;
                Invalidate();
            };
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<EngineeringChartAxis> Axes { get; } = new List<EngineeringChartAxis>();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<EngineeringChartSeries> Series { get; } = new List<EngineeringChartSeries>();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<EngineeringChartWorkingPoint> WorkingPoints { get; } = new List<EngineeringChartWorkingPoint>();

        [DefaultValue("")]
        public string ChartTitle { get; set; } = string.Empty;

        [DefaultValue(true)]
        public bool ShowLegend { get; set; } = true;

        [DefaultValue(EngineeringChartLegendPlacement.InsideTopRight)]
        public EngineeringChartLegendPlacement LegendPlacement { get; set; } =
            EngineeringChartLegendPlacement.InsideTopRight;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle ManualLegendBounds { get; set; } = Rectangle.Empty;

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.Surface;
            ForeColor = palette.TextPrimary;
            Font = ThemeFonts.Body;

            seriesPalette.Clear();
            seriesPalette.Add(palette.Accent);
            seriesPalette.Add(palette.Success);
            seriesPalette.Add(palette.Warning);
            seriesPalette.Add(palette.Info);
            seriesPalette.Add(palette.Danger);
            seriesPalette.Add(Blend(palette.Accent, palette.TextPrimary, 0.35));
            seriesPalette.Add(Blend(palette.Warning, palette.TextPrimary, 0.35));

            Invalidate();
        }

        public EngineeringChartAxis AddAxis(
            string key,
            string title,
            string unit,
            EngineeringChartAxisPosition position,
            double minimum,
            double maximum)
        {
            EngineeringChartAxis axis = new EngineeringChartAxis
            {
                Key = key,
                Title = title,
                Unit = unit,
                Position = position,
                Minimum = minimum,
                Maximum = maximum
            };

            Axes.Add(axis);
            Invalidate();
            return axis;
        }

        public EngineeringChartSeries AddSeries(
            string name,
            string xAxisKey,
            string yAxisKey,
            IEnumerable<EngineeringChartPoint> points)
        {
            EngineeringChartSeries series = new EngineeringChartSeries
            {
                Name = name,
                XAxisKey = xAxisKey,
                YAxisKey = yAxisKey
            };

            series.Points.AddRange(points);
            Series.Add(series);
            Invalidate();
            return series;
        }

        public EngineeringChartWorkingPoint AddWorkingPoint(
            string name,
            string xAxisKey,
            string yAxisKey,
            double x,
            double y)
        {
            EngineeringChartWorkingPoint point = new EngineeringChartWorkingPoint
            {
                Name = name,
                XAxisKey = xAxisKey,
                YAxisKey = yAxisKey,
                X = x,
                Y = y
            };

            WorkingPoints.Add(point);
            Invalidate();
            return point;
        }

        public void ClearChart()
        {
            Axes.Clear();
            Series.Clear();
            WorkingPoints.Clear();
            hoverInfo = null;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.Clear(palette.Surface);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (Axes.Count == 0)
            {
                DrawEmptyState(g);
                return;
            }

            Dictionary<string, EngineeringChartAxis> axisMap = Axes
                .Where(axis => axis.Visible && !string.IsNullOrWhiteSpace(axis.Key))
                .GroupBy(axis => axis.Key)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            plotArea = CalculatePlotArea();
            DrawChartBackground(g);
            DrawGrid(g, axisMap);
            DrawAxes(g, axisMap);
            DrawSeries(g, axisMap);
            DrawWorkingPoints(g, axisMap);
            DrawLegend(g);
            DrawHover(g);
        }

        private Rectangle CalculatePlotArea()
        {
            int leftAxes = Math.Max(1, Axes.Count(axis => axis.Visible && axis.Position == EngineeringChartAxisPosition.Left));
            int rightAxes = Axes.Count(axis => axis.Visible && axis.Position == EngineeringChartAxisPosition.Right);
            int topAxes = Axes.Count(axis => axis.Visible && axis.Position == EngineeringChartAxisPosition.Top);
            int bottomAxes = Math.Max(1, Axes.Count(axis => axis.Visible && axis.Position == EngineeringChartAxisPosition.Bottom));
            bool reserveRightLegend = ShowLegend &&
                LegendPlacement == EngineeringChartLegendPlacement.Right &&
                Series.Any(series => series.Visible);

            int left = 70 + ((leftAxes - 1) * 48);
            int right = 34 + (rightAxes * 58) + (reserveRightLegend ? 190 : 0);
            int top = 30 + (topAxes * 20);
            int bottom = 46 + ((bottomAxes - 1) * 18);

            if (!string.IsNullOrWhiteSpace(ChartTitle))
                top += 24;

            Rectangle bounds = ClientRectangle;
            return new Rectangle(
                bounds.Left + Padding.Left + left,
                bounds.Top + Padding.Top + top,
                Math.Max(20, bounds.Width - Padding.Horizontal - left - right),
                Math.Max(20, bounds.Height - Padding.Vertical - top - bottom));
        }

        private void DrawChartBackground(Graphics g)
        {
            using SolidBrush plotBrush = new SolidBrush(Blend(palette.SurfaceAlt, palette.Surface, 0.34));
            using Pen borderPen = new Pen(palette.Border);

            g.FillRectangle(plotBrush, plotArea);
            g.DrawRectangle(borderPen, plotArea);

            if (!string.IsNullOrWhiteSpace(ChartTitle))
            {
                TextRenderer.DrawText(
                    g,
                    ChartTitle,
                    ThemeFonts.Section,
                    new Rectangle(12, 8, Width - 24, 24),
                    palette.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private void DrawGrid(Graphics g, Dictionary<string, EngineeringChartAxis> axisMap)
        {
            EngineeringChartAxis? xAxis = Axes.FirstOrDefault(axis =>
                axis.Visible &&
                axis.ShowGrid &&
                (axis.Position == EngineeringChartAxisPosition.Bottom || axis.Position == EngineeringChartAxisPosition.Top));

            EngineeringChartAxis? yAxis = Axes.FirstOrDefault(axis =>
                axis.Visible &&
                axis.ShowGrid &&
                (axis.Position == EngineeringChartAxisPosition.Left || axis.Position == EngineeringChartAxisPosition.Right));

            using Pen gridPen = new Pen(Blend(palette.BorderLight, palette.Surface, 0.62));

            if (xAxis != null)
            {
                foreach (double tick in GetTicks(xAxis))
                {
                    float x = MapX(tick, xAxis);
                    g.DrawLine(gridPen, x, plotArea.Top, x, plotArea.Bottom);
                }
            }

            if (yAxis != null)
            {
                foreach (double tick in GetTicks(yAxis))
                {
                    float y = MapY(tick, yAxis);
                    g.DrawLine(gridPen, plotArea.Left, y, plotArea.Right, y);
                }
            }
        }

        private void DrawAxes(Graphics g, Dictionary<string, EngineeringChartAxis> axisMap)
        {
            int leftOffset = 0;
            int rightOffset = 0;
            int topOffset = 0;
            int bottomOffset = 0;

            foreach (EngineeringChartAxis axis in Axes.Where(axis => axis.Visible))
            {
                switch (axis.Position)
                {
                    case EngineeringChartAxisPosition.Left:
                        DrawYAxis(g, axis, plotArea.Left - (leftOffset * 46), true);
                        leftOffset++;
                        break;
                    case EngineeringChartAxisPosition.Right:
                        DrawYAxis(g, axis, plotArea.Right + (rightOffset * 52), false);
                        rightOffset++;
                        break;
                    case EngineeringChartAxisPosition.Top:
                        DrawXAxis(g, axis, plotArea.Top - (topOffset * 18), true);
                        topOffset++;
                        break;
                    default:
                        DrawXAxis(g, axis, plotArea.Bottom + (bottomOffset * 18), false);
                        bottomOffset++;
                        break;
                }
            }
        }

        private void DrawXAxis(Graphics g, EngineeringChartAxis axis, int y, bool top)
        {
            using Pen axisPen = new Pen(palette.BorderStrong);
            g.DrawLine(axisPen, plotArea.Left, y, plotArea.Right, y);

            foreach (double tick in GetTicks(axis))
            {
                float x = MapX(tick, axis);
                g.DrawLine(axisPen, x, y - 3, x, y + 3);

                string label = tick.ToString(axis.LabelFormat);
                Size labelSize = TextRenderer.MeasureText(label, ThemeFonts.Tiny);
                int labelY = top ? y - 17 : y + 5;

                TextRenderer.DrawText(
                    g,
                    label,
                    ThemeFonts.Tiny,
                    new Point((int)x - (labelSize.Width / 2), labelY),
                    palette.TextSecondary);
            }

            string title = axis.DisplayTitle;
            if (!string.IsNullOrWhiteSpace(title))
            {
                TextRenderer.DrawText(
                    g,
                    title,
                    ThemeFonts.Caption,
                    new Rectangle(plotArea.Left, top ? y - 38 : y + 22, plotArea.Width, 18),
                    palette.TextSecondary,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private void DrawYAxis(Graphics g, EngineeringChartAxis axis, int x, bool left)
        {
            using Pen axisPen = new Pen(palette.BorderStrong);
            g.DrawLine(axisPen, x, plotArea.Top, x, plotArea.Bottom);

            foreach (double tick in GetTicks(axis))
            {
                float y = MapY(tick, axis);
                g.DrawLine(axisPen, x - 3, y, x + 3, y);

                string label = tick.ToString(axis.LabelFormat);
                Rectangle labelBounds = left
                    ? new Rectangle(x - 48, (int)y - 9, 42, 18)
                    : new Rectangle(x + 6, (int)y - 9, 44, 18);

                TextRenderer.DrawText(
                    g,
                    label,
                    ThemeFonts.Tiny,
                    labelBounds,
                    palette.TextSecondary,
                    TextFormatFlags.VerticalCenter | (left ? TextFormatFlags.Right : TextFormatFlags.Left));
            }

            string title = axis.DisplayTitle;
            if (!string.IsNullOrWhiteSpace(title))
            {
                Rectangle titleBounds = left
                    ? new Rectangle(x - 58, plotArea.Top, 20, plotArea.Height)
                    : new Rectangle(x + 34, plotArea.Top, 20, plotArea.Height);

                DrawRotatedText(
                    g,
                    title,
                    ThemeFonts.Caption,
                    titleBounds,
                    palette.TextSecondary,
                    left);
            }
        }

        private void DrawSeries(Graphics g, Dictionary<string, EngineeringChartAxis> axisMap)
        {
            int index = 0;

            foreach (EngineeringChartSeries series in Series.Where(series => series.Visible && series.Points.Count > 0))
            {
                if (!axisMap.TryGetValue(series.XAxisKey, out EngineeringChartAxis? xAxis) ||
                    !axisMap.TryGetValue(series.YAxisKey, out EngineeringChartAxis? yAxis))
                {
                    continue;
                }

                Color color = ResolveSeriesColor(series, index++);
                using Pen pen = CreateSeriesPen(color, series);

                PointF[] points = series.Points
                    .Select(point => new PointF(MapX(point.X, xAxis), MapY(point.Y, yAxis)))
                    .ToArray();

                if (points.Length > 1)
                    g.DrawLines(pen, points);

                if (series.ShowMarkers)
                {
                    using SolidBrush markerBrush = new SolidBrush(color);
                    using SolidBrush surfaceBrush = new SolidBrush(palette.Surface);

                    foreach (PointF point in points)
                    {
                        RectangleF marker = new RectangleF(point.X - 3, point.Y - 3, 6, 6);
                        g.FillEllipse(surfaceBrush, marker);
                        g.DrawEllipse(pen, marker);
                    }
                }
            }
        }

        private void DrawWorkingPoints(Graphics g, Dictionary<string, EngineeringChartAxis> axisMap)
        {
            foreach (EngineeringChartWorkingPoint point in WorkingPoints.Where(point => point.Visible))
            {
                if (!axisMap.TryGetValue(point.XAxisKey, out EngineeringChartAxis? xAxis) ||
                    !axisMap.TryGetValue(point.YAxisKey, out EngineeringChartAxis? yAxis))
                {
                    continue;
                }

                float x = MapX(point.X, xAxis);
                float y = MapY(point.Y, yAxis);
                Color color = point.Color ?? palette.Danger;

                using Pen guidePen = new Pen(Blend(color, palette.Surface, 0.55)) { DashStyle = DashStyle.Dash };
                using Pen pointPen = new Pen(color, 2f);
                using SolidBrush fillBrush = new SolidBrush(palette.Surface);

                g.DrawLine(guidePen, x, y, x, plotArea.Bottom);
                g.DrawLine(guidePen, plotArea.Left, y, x, y);
                g.FillEllipse(fillBrush, x - 6, y - 6, 12, 12);
                g.DrawEllipse(pointPen, x - 6, y - 6, 12, 12);
                using SolidBrush centerBrush = new SolidBrush(color);
                g.FillEllipse(centerBrush, x - 2, y - 2, 4, 4);

                string label = string.IsNullOrWhiteSpace(point.Label) ? point.Name : point.Label;
                if (!string.IsNullOrWhiteSpace(label))
                {
                    TextRenderer.DrawText(
                        g,
                        label,
                        ThemeFonts.Caption,
                        new Rectangle((int)x + 8, (int)y - 18, 140, 20),
                        color,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
            }
        }

        private void DrawLegend(Graphics g)
        {
            if (!ShowLegend ||
                LegendPlacement == EngineeringChartLegendPlacement.Hidden ||
                Series.Count == 0)
            {
                return;
            }

            int visibleCount = Series.Count(series => series.Visible);
            if (visibleCount == 0)
                return;

            int legendWidth = Math.Min(220, Math.Max(145, Width / 3));
            int legendHeight = 12 + (visibleCount * 20);
            Rectangle legendBounds = ResolveLegendBounds(legendWidth, legendHeight);

            using SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(224, palette.Surface));
            using Pen borderPen = new Pen(palette.Border);
            g.FillRectangle(backgroundBrush, legendBounds);
            g.DrawRectangle(borderPen, legendBounds);

            int y = legendBounds.Top + 8;
            int index = 0;
            foreach (EngineeringChartSeries series in Series.Where(series => series.Visible))
            {
                Color color = ResolveSeriesColor(series, index++);
                using Pen pen = CreateSeriesPen(color, series);
                int lineY = y + 7;
                g.DrawLine(pen, legendBounds.Left + 10, lineY, legendBounds.Left + 38, lineY);

                TextRenderer.DrawText(
                    g,
                    series.Name,
                    ThemeFonts.Caption,
                    new Rectangle(legendBounds.Left + 44, y - 1, legendBounds.Width - 50, 18),
                    palette.TextSecondary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                y += 20;
            }
        }

        private void DrawHover(Graphics g)
        {
            if (hoverInfo == null)
                return;

            using Pen pointPen = new Pen(hoverInfo.Color, 2f);
            using SolidBrush fillBrush = new SolidBrush(palette.Surface);
            g.FillEllipse(fillBrush, hoverInfo.ScreenPoint.X - 5, hoverInfo.ScreenPoint.Y - 5, 10, 10);
            g.DrawEllipse(pointPen, hoverInfo.ScreenPoint.X - 5, hoverInfo.ScreenPoint.Y - 5, 10, 10);

            string text = $"{hoverInfo.SeriesName}: {hoverInfo.X:0.##}; {hoverInfo.Y:0.##}";
            Size size = TextRenderer.MeasureText(text, ThemeFonts.Caption);
            Rectangle tooltip = new Rectangle(
                Math.Min(Width - size.Width - 18, Math.Max(8, (int)hoverInfo.ScreenPoint.X + 12)),
                Math.Min(Height - 30, Math.Max(8, (int)hoverInfo.ScreenPoint.Y - 28)),
                size.Width + 10,
                24);

            using SolidBrush tooltipBrush = new SolidBrush(palette.SurfaceAlt);
            using Pen borderPen = new Pen(palette.BorderStrong);
            g.FillRectangle(tooltipBrush, tooltip);
            g.DrawRectangle(borderPen, tooltip);

            TextRenderer.DrawText(
                g,
                text,
                ThemeFonts.Caption,
                tooltip,
                palette.TextPrimary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void DrawEmptyState(Graphics g)
        {
            TextRenderer.DrawText(
                g,
                "Nincs grafikon adat.",
                ThemeFonts.Body,
                ClientRectangle,
                palette.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private Rectangle ResolveLegendBounds(int legendWidth, int legendHeight)
        {
            if (LegendPlacement == EngineeringChartLegendPlacement.Manual && !ManualLegendBounds.IsEmpty)
                return ManualLegendBounds;

            if (LegendPlacement == EngineeringChartLegendPlacement.Right)
            {
                return new Rectangle(
                    plotArea.Right + 58,
                    plotArea.Top,
                    Math.Min(180, Math.Max(120, Width - plotArea.Right - 70)),
                    legendHeight);
            }

            return new Rectangle(
                plotArea.Right - legendWidth - 10,
                plotArea.Top + 10,
                legendWidth,
                legendHeight);
        }

        private static void DrawRotatedText(
            Graphics g,
            string text,
            Font font,
            Rectangle bounds,
            Color color,
            bool rotateCounterClockwise)
        {
            GraphicsState state = g.Save();

            try
            {
                using SolidBrush brush = new SolidBrush(color);
                using StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.None,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                float centerX = bounds.Left + (bounds.Width / 2f);
                float centerY = bounds.Top + (bounds.Height / 2f);
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(rotateCounterClockwise ? -90f : 90f);

                RectangleF rotatedBounds = new RectangleF(
                    -(bounds.Height / 2f),
                    -(bounds.Width / 2f),
                    bounds.Height,
                    bounds.Width);

                g.DrawString(text, font, brush, rotatedBounds, format);
            }
            finally
            {
                g.Restore(state);
            }
        }

        private IEnumerable<double> GetTicks(EngineeringChartAxis axis)
        {
            if (axis.MajorTickInterval.HasValue && axis.MajorTickInterval.Value > 0)
            {
                double interval = axis.MajorTickInterval.Value;
                double first = Math.Ceiling(axis.Minimum / interval) * interval;

                if (Math.Abs(first - axis.Minimum) > interval * 0.001)
                    yield return axis.Minimum;

                for (double tick = first; tick <= axis.Maximum + (interval * 0.001); tick += interval)
                    yield return Math.Round(tick, 10);

                double last = Math.Floor(axis.Maximum / interval) * interval;
                if (Math.Abs(last - axis.Maximum) > interval * 0.001)
                    yield return axis.Maximum;

                yield break;
            }

            int tickCount = Math.Max(2, axis.TickCount);
            double min = axis.Minimum;
            double max = Math.Abs(axis.Maximum - axis.Minimum) < double.Epsilon
                ? axis.Minimum + 1
                : axis.Maximum;

            for (int i = 0; i < tickCount; i++)
            {
                double ratio = tickCount == 1 ? 0 : i / (double)(tickCount - 1);
                yield return min + ((max - min) * ratio);
            }
        }

        private float MapX(double value, EngineeringChartAxis axis)
        {
            double range = Math.Abs(axis.Maximum - axis.Minimum) < double.Epsilon
                ? 1
                : axis.Maximum - axis.Minimum;

            double ratio = (value - axis.Minimum) / range;
            return (float)(plotArea.Left + (ratio * plotArea.Width));
        }

        private float MapY(double value, EngineeringChartAxis axis)
        {
            double range = Math.Abs(axis.Maximum - axis.Minimum) < double.Epsilon
                ? 1
                : axis.Maximum - axis.Minimum;

            double ratio = (value - axis.Minimum) / range;
            return (float)(plotArea.Bottom - (ratio * plotArea.Height));
        }

        private Color ResolveSeriesColor(EngineeringChartSeries series, int index)
        {
            if (series.Color.HasValue)
                return series.Color.Value;

            if (seriesPalette.Count == 0)
                ApplyTheme(palette);

            return seriesPalette[index % seriesPalette.Count];
        }

        private Pen CreateSeriesPen(Color color, EngineeringChartSeries series)
        {
            Pen pen = new Pen(color, Math.Max(1f, series.StrokeWidth))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            switch (series.LinePattern)
            {
                case EngineeringChartLinePattern.Dash:
                    pen.DashStyle = DashStyle.Dash;
                    break;
                case EngineeringChartLinePattern.Dot:
                    pen.DashStyle = DashStyle.Dot;
                    break;
                case EngineeringChartLinePattern.DashDot:
                    pen.DashStyle = DashStyle.DashDot;
                    break;
            }

            return pen;
        }

        private void HandleMouseMove(object? sender, MouseEventArgs e)
        {
            HoverInfo? nearest = FindNearestPoint(e.Location);
            bool changed = !Equals(hoverInfo, nearest);
            hoverInfo = nearest;

            if (changed)
                Invalidate();
        }

        private HoverInfo? FindNearestPoint(Point location)
        {
            if (!plotArea.Contains(location))
                return null;

            Dictionary<string, EngineeringChartAxis> axisMap = Axes
                .Where(axis => axis.Visible && !string.IsNullOrWhiteSpace(axis.Key))
                .GroupBy(axis => axis.Key)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            HoverInfo? nearest = null;
            double nearestDistance = 10;
            int index = 0;

            foreach (EngineeringChartWorkingPoint point in WorkingPoints.Where(point => point.Visible))
            {
                if (!axisMap.TryGetValue(point.XAxisKey, out EngineeringChartAxis? xAxis) ||
                    !axisMap.TryGetValue(point.YAxisKey, out EngineeringChartAxis? yAxis))
                {
                    continue;
                }

                PointF screenPoint = new PointF(MapX(point.X, xAxis), MapY(point.Y, yAxis));
                double distance = Math.Sqrt(
                    Math.Pow(screenPoint.X - location.X, 2) +
                    Math.Pow(screenPoint.Y - location.Y, 2));

                if (distance <= nearestDistance + 4)
                {
                    nearestDistance = distance;
                    string name = string.IsNullOrWhiteSpace(point.Name) ? "Munkapont" : point.Name;
                    nearest = new HoverInfo(
                        name,
                        point.X,
                        point.Y,
                        screenPoint,
                        point.Color ?? palette.Danger);
                }
            }

            foreach (EngineeringChartSeries series in Series.Where(series => series.Visible))
            {
                if (!axisMap.TryGetValue(series.XAxisKey, out EngineeringChartAxis? xAxis) ||
                    !axisMap.TryGetValue(series.YAxisKey, out EngineeringChartAxis? yAxis))
                {
                    continue;
                }

                Color color = ResolveSeriesColor(series, index++);

                foreach (EngineeringChartPoint point in series.Points)
                {
                    PointF screenPoint = new PointF(MapX(point.X, xAxis), MapY(point.Y, yAxis));
                    double distance = Math.Sqrt(
                        Math.Pow(screenPoint.X - location.X, 2) +
                        Math.Pow(screenPoint.Y - location.Y, 2));

                    if (distance <= nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = new HoverInfo(series.Name, point.X, point.Y, screenPoint, color);
                    }
                }
            }

            return nearest;
        }

        private static Color Blend(Color foreground, Color background, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            int r = (int)(background.R + ((foreground.R - background.R) * amount));
            int g = (int)(background.G + ((foreground.G - background.G) * amount));
            int b = (int)(background.B + ((foreground.B - background.B) * amount));
            return Color.FromArgb(r, g, b);
        }

        private sealed class HoverInfo
        {
            public HoverInfo(string seriesName, double x, double y, PointF screenPoint, Color color)
            {
                SeriesName = seriesName;
                X = x;
                Y = y;
                ScreenPoint = screenPoint;
                Color = color;
            }

            public string SeriesName { get; }
            public double X { get; }
            public double Y { get; }
            public PointF ScreenPoint { get; }
            public Color Color { get; }
        }
    }
}
