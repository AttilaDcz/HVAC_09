using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Icons
{
    public static class HvacIconRenderer
    {
        private static readonly Dictionary<IconCacheKey, Bitmap> Cache =
            new Dictionary<IconCacheKey, Bitmap>();
        private static readonly object CacheLock = new object();

        static HvacIconRenderer()
        {
            ThemeManager.ThemeChanged += (s, e) => ClearCache();
        }

        public static Bitmap Render(
            HvacIconKind icon,
            AppThemeMode themeMode,
            int size,
            Color? accentColor = null)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            Color accent = accentColor ?? DefaultAccent(icon, themeMode);
            IconCacheKey key = new IconCacheKey(
                icon,
                themeMode,
                size,
                accent.ToArgb());

            lock (CacheLock)
            {
                if (!Cache.TryGetValue(key, out Bitmap? cached))
                {
                    cached = RenderCore(icon, themeMode, size, accent);
                    Cache[key] = cached;
                }

                return (Bitmap)cached.Clone();
            }
        }

        public static Bitmap RenderOutline(
            HvacIconKind icon,
            AppThemeMode themeMode,
            int size,
            Color accentColor)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            return RenderCore(icon, themeMode, size, accentColor, outlineOnly: true);
        }

        public static void ClearCache()
        {
            lock (CacheLock)
            {
                foreach (Bitmap bitmap in Cache.Values)
                {
                    bitmap.Dispose();
                }

                Cache.Clear();
            }
        }

        private static Bitmap RenderCore(
            HvacIconKind icon,
            AppThemeMode themeMode,
            int size,
            Color accentColor,
            bool outlineOnly = false)
        {
            Bitmap bitmap = new Bitmap(size, size, PixelFormat.Format32bppPArgb);

            using Graphics g = Graphics.FromImage(bitmap);
            g.CompositingMode = CompositingMode.SourceCopy;
            g.Clear(Color.Transparent);
            g.CompositingMode = CompositingMode.SourceOver;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float scale = size / 24f;
            g.ScaleTransform(scale, scale);

            IconColors colors = outlineOnly
                ? IconColors.ForOutline(accentColor)
                : IconColors.ForTheme(themeMode, accentColor);

            using Pen stroke = CreatePen(colors.Stroke);
            using Pen softStroke = CreatePen(colors.SoftStroke);
            using Brush fill = new SolidBrush(colors.Fill);
            using Brush strongFill = new SolidBrush(colors.StrongFill);

            switch (icon)
            {
                case HvacIconKind.AppLogo:
                    DrawAppLogo(g, fill, strongFill, stroke, softStroke);
                    break;
                case HvacIconKind.NewProject:
                    DrawDocument(g, fill, stroke, addPlus: true);
                    break;
                case HvacIconKind.OpenProject:
                    DrawFolder(g, fill, stroke);
                    break;
                case HvacIconKind.SaveProject:
                    DrawSave(g, fill, stroke);
                    break;
                case HvacIconKind.ProjectProperties:
                    DrawProjectProperties(g, fill, stroke);
                    break;
                case HvacIconKind.Settings:
                    DrawSettings(g, fill, stroke);
                    break;
                case HvacIconKind.Export:
                    DrawArrowDocument(g, fill, stroke, arrowUp: true);
                    break;
                case HvacIconKind.Import:
                    DrawArrowDocument(g, fill, stroke, arrowUp: false);
                    break;
                case HvacIconKind.PrintReport:
                    DrawPrinter(g, fill, stroke);
                    break;
                case HvacIconKind.Info:
                    DrawInfo(g, fill, stroke);
                    break;
                case HvacIconKind.Search:
                    DrawSearch(g, stroke);
                    break;
                case HvacIconKind.ThemeToggle:
                    DrawThemeToggle(g, stroke);
                    break;
                case HvacIconKind.NavigateBack:
                    DrawNavigationArrow(g, stroke, left: true);
                    break;
                case HvacIconKind.NavigateForward:
                    DrawNavigationArrow(g, stroke, left: false);
                    break;
                case HvacIconKind.Undo:
                    DrawUndoRedo(g, stroke, undo: true);
                    break;
                case HvacIconKind.Redo:
                    DrawUndoRedo(g, stroke, undo: false);
                    break;
                case HvacIconKind.Help:
                    DrawHelp(g, fill, stroke);
                    break;
                case HvacIconKind.Dashboard:
                    DrawDashboard(g, fill, stroke);
                    break;
                case HvacIconKind.BuildingEnergy:
                    DrawBuildingEnergy(g, fill, strongFill, stroke, softStroke);
                    break;
                case HvacIconKind.Building:
                    DrawBuilding(g, fill, stroke);
                    break;
                case HvacIconKind.UValue:
                    DrawUValue(g, fill, stroke);
                    break;
                case HvacIconKind.HeatLoad:
                    DrawHeatLoad(g, fill, stroke);
                    break;
                case HvacIconKind.Certification:
                    DrawCertification(g, fill, stroke);
                    break;
                case HvacIconKind.Heating:
                    DrawHeating(g, fill, stroke);
                    break;
                case HvacIconKind.Cooling:
                    DrawCooling(g, fill, stroke);
                    break;
                case HvacIconKind.HeatingCooling:
                    DrawHeatingCooling(g, fill, stroke);
                    break;
                case HvacIconKind.PipeNetwork:
                    DrawPipeNetwork(g, fill, stroke);
                    break;
                case HvacIconKind.Pump:
                    DrawPump(g, fill, stroke);
                    break;
                case HvacIconKind.SanitaryWater:
                    DrawSanitaryWater(g, fill, stroke);
                    break;
                case HvacIconKind.WaterDemand:
                    DrawWaterDemand(g, fill, stroke);
                    break;
                case HvacIconKind.Sewer:
                    DrawSewer(g, fill, stroke);
                    break;
                case HvacIconKind.DhwCirculation:
                    DrawDhwCirculation(g, fill, stroke);
                    break;
                case HvacIconKind.LongProfile:
                    DrawLongProfile(g, fill, stroke);
                    break;
                case HvacIconKind.Air:
                    DrawAir(g, stroke);
                    break;
                case HvacIconKind.AirVelocity:
                    DrawAirVelocity(g, stroke);
                    break;
                case HvacIconKind.DuctSizing:
                    DrawDuctSizing(g, fill, stroke);
                    break;
                case HvacIconKind.DuctNetwork:
                    DrawDuctNetwork(g, fill, stroke);
                    break;
                case HvacIconKind.FlueGas:
                    DrawFlueGas(g, fill, stroke);
                    break;
                case HvacIconKind.Safety:
                    DrawSafety(g, fill, stroke);
                    break;
            }

            return bitmap;
        }

        private static Pen CreatePen(Color color)
        {
            return new Pen(color, 1.9f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
        }

        private static Color DefaultAccent(
            HvacIconKind icon,
            AppThemeMode themeMode)
        {
            return icon switch
            {
                HvacIconKind.AppLogo
                    => Color.FromArgb(0, 145, 210),

                HvacIconKind.BuildingEnergy or
                HvacIconKind.Building or
                HvacIconKind.UValue or
                HvacIconKind.HeatLoad or
                HvacIconKind.Certification
                    => Color.FromArgb(230, 90, 75),

                HvacIconKind.Heating or
                HvacIconKind.Cooling or
                HvacIconKind.HeatingCooling or
                HvacIconKind.PipeNetwork or
                HvacIconKind.Pump
                    => Color.FromArgb(52, 152, 219),

                HvacIconKind.SanitaryWater or
                HvacIconKind.WaterDemand or
                HvacIconKind.Sewer or
                HvacIconKind.DhwCirculation or
                HvacIconKind.LongProfile
                    => Color.FromArgb(26, 188, 156),

                HvacIconKind.Air or
                HvacIconKind.AirVelocity or
                HvacIconKind.DuctSizing or
                HvacIconKind.DuctNetwork
                    => Color.FromArgb(46, 204, 113),

                HvacIconKind.FlueGas or
                HvacIconKind.Safety
                    => Color.FromArgb(230, 126, 34),

                _ => themeMode == AppThemeMode.Dark
                    ? Color.FromArgb(22, 137, 216)
                    : Color.FromArgb(0, 111, 188)
            };
        }

        private static void DrawDocument(
            Graphics g,
            Brush fill,
            Pen stroke,
            bool addPlus)
        {
            PointF[] doc =
            {
                new PointF(6, 3), new PointF(14, 3), new PointF(18, 7),
                new PointF(18, 21), new PointF(6, 21)
            };

            g.FillPolygon(fill, doc);
            g.DrawPolygon(stroke, doc);
            g.DrawLines(stroke, new[] { new PointF(14, 3), new PointF(14, 8), new PointF(18, 8) });

            if (addPlus)
            {
                g.DrawLine(stroke, 9, 14, 15, 14);
                g.DrawLine(stroke, 12, 11, 12, 17);
            }
        }

        private static void DrawAppLogo(
            Graphics g,
            Brush fill,
            Brush strongFill,
            Pen stroke,
            Pen softStroke)
        {
            g.FillRectangle(fill, 5, 7, 8, 12);
            g.DrawRectangle(stroke, 5, 7, 8, 12);
            g.DrawLine(stroke, 7.5f, 10, 10.5f, 10);
            g.DrawLine(stroke, 7.5f, 13, 10.5f, 13);
            g.DrawLine(stroke, 7.5f, 16, 10.5f, 16);

            g.DrawCurve(
                softStroke,
                new[] { new PointF(13.5f, 7), new PointF(17, 4.8f), new PointF(20.5f, 7) });
            g.DrawCurve(
                softStroke,
                new[] { new PointF(13.5f, 12), new PointF(17, 9.8f), new PointF(20.5f, 12) });
            g.DrawCurve(
                softStroke,
                new[] { new PointF(13.5f, 17), new PointF(17, 14.8f), new PointF(20.5f, 17) });

            g.FillEllipse(strongFill, 3.8f, 3.8f, 5.2f, 5.2f);
            g.DrawEllipse(softStroke, 3.8f, 3.8f, 5.2f, 5.2f);
        }

        private static void DrawFolder(Graphics g, Brush fill, Pen stroke)
        {
            PointF[] folder =
            {
                new PointF(3, 8), new PointF(10, 8), new PointF(12, 10),
                new PointF(21, 10), new PointF(21, 19), new PointF(3, 19)
            };

            g.FillPolygon(fill, folder);
            g.DrawPolygon(stroke, folder);
            g.DrawLines(stroke, new[] { new PointF(3, 8), new PointF(3, 6), new PointF(10, 6), new PointF(12, 8) });
            g.DrawLine(stroke, 4, 12, 20, 12);
        }

        private static void DrawSave(Graphics g, Brush fill, Pen stroke)
        {
            PointF[] body =
            {
                new PointF(5, 3), new PointF(16, 3), new PointF(19, 6),
                new PointF(19, 21), new PointF(5, 21)
            };

            g.FillPolygon(fill, body);
            g.DrawPolygon(stroke, body);
            g.DrawLines(stroke, new[] { new PointF(8, 3), new PointF(8, 9), new PointF(16, 9), new PointF(16, 4) });
            g.DrawRectangle(stroke, 8, 14, 8, 7);
        }

        private static void DrawProjectProperties(Graphics g, Brush fill, Pen stroke)
        {
            g.FillRectangle(fill, 5, 4, 14, 16);
            g.DrawRectangle(stroke, 5, 4, 14, 16);
            g.DrawLine(stroke, 8, 8, 16, 8);
            g.DrawLine(stroke, 8, 12, 16, 12);
            g.DrawLine(stroke, 8, 16, 12, 16);
        }

        private static void DrawSettings(Graphics g, Brush fill, Pen stroke)
        {
            g.FillEllipse(fill, 8.8f, 8.8f, 6.4f, 6.4f);
            g.DrawEllipse(stroke, 8.8f, 8.8f, 6.4f, 6.4f);

            g.DrawLine(stroke, 12, 3, 12, 6);
            g.DrawLine(stroke, 12, 18, 12, 21);
            g.DrawLine(stroke, 3, 12, 6, 12);
            g.DrawLine(stroke, 18, 12, 21, 12);
            g.DrawLine(stroke, 4.2f, 7.5f, 6.8f, 9);
            g.DrawLine(stroke, 17.2f, 15, 19.8f, 16.5f);
            g.DrawLine(stroke, 19.8f, 7.5f, 17.2f, 9);
            g.DrawLine(stroke, 6.8f, 15, 4.2f, 16.5f);
        }

        private static void DrawArrowDocument(
            Graphics g,
            Brush fill,
            Pen stroke,
            bool arrowUp)
        {
            DrawDocument(g, fill, stroke, addPlus: false);

            if (arrowUp)
            {
                g.DrawLine(stroke, 12, 16, 12, 9);
                g.DrawLines(stroke, new[] { new PointF(9, 12), new PointF(12, 9), new PointF(15, 12) });
            }
            else
            {
                g.DrawLine(stroke, 12, 9, 12, 16);
                g.DrawLines(stroke, new[] { new PointF(9, 13), new PointF(12, 16), new PointF(15, 13) });
            }

            g.DrawLine(stroke, 8, 19, 16, 19);
        }

        private static void DrawPrinter(Graphics g, Brush fill, Pen stroke)
        {
            g.FillRectangle(fill, 7, 14, 10, 7);
            g.DrawRectangle(stroke, 7, 14, 10, 7);
            g.DrawLines(stroke, new[] { new PointF(7, 8), new PointF(7, 3), new PointF(17, 3), new PointF(17, 8) });
            g.DrawLines(stroke, new[] { new PointF(6, 17), new PointF(4, 17), new PointF(4, 10), new PointF(20, 10), new PointF(20, 17), new PointF(18, 17) });
            g.DrawLine(stroke, 8, 15, 16, 15);
            g.DrawLine(stroke, 8, 18, 16, 18);
        }

        private static void DrawInfo(Graphics g, Brush fill, Pen stroke)
        {
            using Brush dotBrush = new SolidBrush(stroke.Color);

            g.FillEllipse(fill, 3, 3, 18, 18);
            g.DrawEllipse(stroke, 3, 3, 18, 18);
            g.DrawLine(stroke, 12, 11, 12, 17);
            g.FillEllipse(dotBrush, 11.1f, 6.4f, 1.8f, 1.8f);
        }

        private static void DrawSearch(Graphics g, Pen stroke)
        {
            g.DrawEllipse(stroke, 4, 4, 11, 11);
            g.DrawLine(stroke, 13, 13, 20, 20);
        }

        private static void DrawThemeToggle(Graphics g, Pen stroke)
        {
            g.DrawArc(stroke, 4, 4, 16, 9, 200, 250);
            g.DrawLines(stroke, new[] { new PointF(16.5f, 3.8f), new PointF(20, 5.2f), new PointF(17.8f, 8.2f) });

            g.DrawArc(stroke, 4, 11, 16, 9, 20, 250);
            g.DrawLines(stroke, new[] { new PointF(7.5f, 20.2f), new PointF(4, 18.8f), new PointF(6.2f, 15.8f) });
        }

        private static void DrawNavigationArrow(Graphics g, Pen stroke, bool left)
        {
            if (left)
            {
                g.DrawLine(stroke, 18, 12, 6, 12);
                g.DrawLines(stroke, new[] { new PointF(10, 7), new PointF(5, 12), new PointF(10, 17) });
            }
            else
            {
                g.DrawLine(stroke, 6, 12, 18, 12);
                g.DrawLines(stroke, new[] { new PointF(14, 7), new PointF(19, 12), new PointF(14, 17) });
            }
        }

        private static void DrawUndoRedo(Graphics g, Pen stroke, bool undo)
        {
            if (undo)
            {
                g.DrawArc(stroke, 5, 6, 14, 12, 190, 250);
                g.DrawLines(stroke, new[] { new PointF(7, 7), new PointF(4, 12), new PointF(9, 13) });
            }
            else
            {
                g.DrawArc(stroke, 5, 6, 14, 12, 100, 250);
                g.DrawLines(stroke, new[] { new PointF(17, 7), new PointF(20, 12), new PointF(15, 13) });
            }
        }

        private static void DrawHelp(Graphics g, Brush fill, Pen stroke)
        {
            using Brush dotBrush = new SolidBrush(stroke.Color);

            g.FillEllipse(fill, 3, 3, 18, 18);
            g.DrawEllipse(stroke, 3, 3, 18, 18);
            g.DrawCurve(stroke, new[] { new PointF(8.2f, 9), new PointF(9, 6.5f), new PointF(12, 6.5f), new PointF(15, 8.5f), new PointF(12, 12) });
            g.DrawLine(stroke, 12, 12, 12, 14);
            g.FillEllipse(dotBrush, 11.1f, 17f, 1.8f, 1.8f);
        }

        private static void DrawDashboard(Graphics g, Brush fill, Pen stroke)
        {
            g.FillRectangle(fill, 4, 4, 7, 7);
            g.FillRectangle(fill, 13, 4, 7, 7);
            g.FillRectangle(fill, 4, 13, 7, 7);
            g.FillRectangle(fill, 13, 13, 7, 7);
            g.DrawRectangle(stroke, 4, 4, 7, 7);
            g.DrawRectangle(stroke, 13, 4, 7, 7);
            g.DrawRectangle(stroke, 4, 13, 7, 7);
            g.DrawRectangle(stroke, 13, 13, 7, 7);
        }

        private static void DrawBuildingEnergy(
            Graphics g,
            Brush fill,
            Brush strongFill,
            Pen stroke,
            Pen softStroke)
        {
            DrawBuilding(g, fill, stroke);
            g.FillEllipse(strongFill, 13.5f, 3.5f, 6.5f, 6.5f);
            g.DrawEllipse(softStroke, 13.5f, 3.5f, 6.5f, 6.5f);
            g.DrawLine(softStroke, 16.8f, 2.2f, 16.8f, 5);
            g.DrawLine(softStroke, 16.8f, 9, 16.8f, 11.8f);
            g.DrawLine(softStroke, 12.2f, 6.8f, 15, 6.8f);
            g.DrawLine(softStroke, 18.6f, 6.8f, 21.4f, 6.8f);
        }

        private static void DrawBuilding(Graphics g, Brush fill, Pen stroke)
        {
            g.FillRectangle(fill, 5, 6, 14, 15);
            g.DrawRectangle(stroke, 5, 6, 14, 15);
            g.DrawLine(stroke, 8, 9, 8, 10);
            g.DrawLine(stroke, 12, 9, 12, 10);
            g.DrawLine(stroke, 16, 9, 16, 10);
            g.DrawLine(stroke, 8, 13, 8, 14);
            g.DrawLine(stroke, 12, 13, 12, 14);
            g.DrawLine(stroke, 16, 13, 16, 14);
            g.DrawRectangle(stroke, 10, 17, 4, 4);
        }

        private static void DrawUValue(Graphics g, Brush fill, Pen stroke)
        {
            g.FillRectangle(fill, 5, 5, 14, 14);
            g.DrawRectangle(stroke, 5, 5, 14, 14);
            g.DrawLine(stroke, 9, 9, 9, 15);
            g.DrawLine(stroke, 15, 9, 15, 15);
            g.DrawLine(stroke, 9, 15, 15, 15);
        }

        private static void DrawHeatLoad(Graphics g, Brush fill, Pen stroke)
        {
            DrawHeating(g, fill, stroke);
            g.DrawLine(stroke, 15, 5, 19, 5);
            g.DrawLine(stroke, 17, 3, 17, 7);
        }

        private static void DrawCertification(Graphics g, Brush fill, Pen stroke)
        {
            g.FillRectangle(fill, 6, 4, 12, 16);
            g.DrawRectangle(stroke, 6, 4, 12, 16);
            g.DrawLine(stroke, 9, 9, 11, 12);
            g.DrawLine(stroke, 11, 12, 16, 7);
            g.DrawLine(stroke, 9, 16, 15, 16);
        }

        private static void DrawHeating(Graphics g, Brush fill, Pen stroke)
        {
            PointF[] flame =
            {
                new PointF(12, 3), new PointF(16, 9), new PointF(15, 16),
                new PointF(12, 21), new PointF(8, 18), new PointF(7, 12),
                new PointF(10, 8)
            };

            g.FillClosedCurve(fill, flame);
            g.DrawClosedCurve(stroke, flame);
            g.DrawCurve(stroke, new[] { new PointF(12, 8), new PointF(10, 12), new PointF(12, 17) });
        }

        private static void DrawCooling(Graphics g, Brush fill, Pen stroke)
        {
            g.FillEllipse(fill, 9, 9, 6, 6);
            g.DrawLine(stroke, 12, 3, 12, 21);
            g.DrawLine(stroke, 4.2f, 7.5f, 19.8f, 16.5f);
            g.DrawLine(stroke, 19.8f, 7.5f, 4.2f, 16.5f);
            g.DrawLine(stroke, 9.6f, 5.4f, 12, 7.8f);
            g.DrawLine(stroke, 14.4f, 5.4f, 12, 7.8f);
            g.DrawLine(stroke, 9.6f, 18.6f, 12, 16.2f);
            g.DrawLine(stroke, 14.4f, 18.6f, 12, 16.2f);
        }

        private static void DrawHeatingCooling(Graphics g, Brush fill, Pen stroke)
        {
            DrawHeating(g, fill, stroke);
            g.DrawLine(stroke, 15.5f, 5, 20.5f, 5);
            g.DrawLine(stroke, 18, 2.5f, 18, 7.5f);
            g.DrawLine(stroke, 16.2f, 3.2f, 19.8f, 6.8f);
            g.DrawLine(stroke, 19.8f, 3.2f, 16.2f, 6.8f);
        }

        private static void DrawPipeNetwork(Graphics g, Brush fill, Pen stroke)
        {
            g.FillEllipse(fill, 4, 9, 6, 6);
            g.FillEllipse(fill, 14, 4, 6, 6);
            g.FillEllipse(fill, 14, 14, 6, 6);
            g.DrawLine(stroke, 9, 12, 15, 7);
            g.DrawLine(stroke, 9, 12, 15, 17);
            g.DrawEllipse(stroke, 4, 9, 6, 6);
            g.DrawEllipse(stroke, 14, 4, 6, 6);
            g.DrawEllipse(stroke, 14, 14, 6, 6);
        }

        private static void DrawPump(Graphics g, Brush fill, Pen stroke)
        {
            g.FillEllipse(fill, 6, 6, 12, 12);
            g.DrawEllipse(stroke, 6, 6, 12, 12);
            g.DrawLine(stroke, 3, 12, 6, 12);
            g.DrawLine(stroke, 18, 12, 21, 12);
            g.DrawLines(stroke, new[] { new PointF(11, 8), new PointF(15, 12), new PointF(11, 16) });
        }

        private static void DrawSanitaryWater(Graphics g, Brush fill, Pen stroke)
        {
            g.DrawLine(stroke, 6, 7, 16, 7);
            g.DrawLine(stroke, 16, 7, 16, 11);
            g.DrawLine(stroke, 13, 11, 20, 11);
            g.DrawLine(stroke, 8, 5, 8, 9);
            g.FillEllipse(fill, 10, 15, 4, 5);
            g.DrawCurve(stroke, new[] { new PointF(12, 13), new PointF(9, 17), new PointF(12, 21), new PointF(15, 17), new PointF(12, 13) });
        }

        private static void DrawWaterDemand(Graphics g, Brush fill, Pen stroke)
        {
            DrawSanitaryWater(g, fill, stroke);
            g.DrawLine(stroke, 4, 20, 20, 20);
        }

        private static void DrawSewer(Graphics g, Brush fill, Pen stroke)
        {
            g.FillRectangle(fill, 5, 8, 14, 8);
            g.DrawRectangle(stroke, 5, 8, 14, 8);
            g.DrawLine(stroke, 7, 12, 17, 12);
            g.DrawLine(stroke, 9, 16, 9, 19);
            g.DrawLine(stroke, 15, 16, 15, 19);
        }

        private static void DrawDhwCirculation(Graphics g, Brush fill, Pen stroke)
        {
            g.FillEllipse(fill, 5, 5, 14, 14);
            g.DrawArc(stroke, 5, 5, 14, 14, 35, 250);
            g.DrawLines(stroke, new[] { new PointF(14, 5), new PointF(18, 5), new PointF(18, 9) });
            g.DrawArc(stroke, 5, 5, 14, 14, 215, 250);
            g.DrawLines(stroke, new[] { new PointF(10, 19), new PointF(6, 19), new PointF(6, 15) });
        }

        private static void DrawLongProfile(Graphics g, Brush fill, Pen stroke)
        {
            g.FillRectangle(fill, 4, 15, 16, 3);
            g.DrawLine(stroke, 4, 18, 20, 6);
            g.DrawLine(stroke, 4, 18, 20, 18);
            g.DrawLine(stroke, 7, 16, 7, 19);
            g.DrawLine(stroke, 14, 11, 14, 19);
        }

        private static void DrawAir(Graphics g, Pen stroke)
        {
            g.DrawCurve(stroke, new[] { new PointF(4, 8), new PointF(9, 5), new PointF(14, 7), new PointF(20, 6) });
            g.DrawCurve(stroke, new[] { new PointF(4, 13), new PointF(10, 10), new PointF(15, 13), new PointF(20, 11) });
            g.DrawCurve(stroke, new[] { new PointF(4, 18), new PointF(9, 16), new PointF(14, 18), new PointF(19, 16) });
        }

        private static void DrawAirVelocity(Graphics g, Pen stroke)
        {
            DrawAir(g, stroke);
            g.DrawLine(stroke, 13, 19, 20, 19);
            g.DrawLines(stroke, new[] { new PointF(17, 16), new PointF(20, 19), new PointF(17, 22) });
        }

        private static void DrawDuctSizing(Graphics g, Brush fill, Pen stroke)
        {
            g.FillRectangle(fill, 5, 7, 14, 10);
            g.DrawRectangle(stroke, 5, 7, 14, 10);
            g.DrawLine(stroke, 5, 20, 19, 20);
            g.DrawLine(stroke, 5, 18, 5, 22);
            g.DrawLine(stroke, 19, 18, 19, 22);
        }

        private static void DrawDuctNetwork(Graphics g, Brush fill, Pen stroke)
        {
            g.FillRectangle(fill, 4, 9, 7, 6);
            g.FillRectangle(fill, 14, 5, 6, 5);
            g.FillRectangle(fill, 14, 15, 6, 5);
            g.DrawRectangle(stroke, 4, 9, 7, 6);
            g.DrawRectangle(stroke, 14, 5, 6, 5);
            g.DrawRectangle(stroke, 14, 15, 6, 5);
            g.DrawLine(stroke, 11, 12, 14, 7.5f);
            g.DrawLine(stroke, 11, 12, 14, 17.5f);
        }

        private static void DrawFlueGas(Graphics g, Brush fill, Pen stroke)
        {
            g.FillRectangle(fill, 8, 9, 6, 11);
            g.DrawRectangle(stroke, 8, 9, 6, 11);
            g.DrawLine(stroke, 14, 11, 19, 8);
            g.DrawLine(stroke, 14, 15, 19, 18);
            g.DrawCurve(stroke, new[] { new PointF(6, 5), new PointF(9, 2), new PointF(12, 5), new PointF(15, 3), new PointF(18, 6) });
        }

        private static void DrawSafety(Graphics g, Brush fill, Pen stroke)
        {
            PointF[] shield =
            {
                new PointF(12, 3), new PointF(19, 6), new PointF(18, 14),
                new PointF(12, 21), new PointF(6, 14), new PointF(5, 6)
            };

            g.FillPolygon(fill, shield);
            g.DrawPolygon(stroke, shield);
            g.DrawLine(stroke, 12, 8, 12, 13);
            g.DrawLine(stroke, 12, 17, 12, 17.2f);
        }

        private readonly struct IconCacheKey : IEquatable<IconCacheKey>
        {
            public IconCacheKey(
                HvacIconKind icon,
                AppThemeMode themeMode,
                int size,
                int accentArgb)
            {
                Icon = icon;
                ThemeMode = themeMode;
                Size = size;
                AccentArgb = accentArgb;
            }

            private HvacIconKind Icon { get; }
            private AppThemeMode ThemeMode { get; }
            private int Size { get; }
            private int AccentArgb { get; }

            public bool Equals(IconCacheKey other)
            {
                return Icon == other.Icon &&
                    ThemeMode == other.ThemeMode &&
                    Size == other.Size &&
                    AccentArgb == other.AccentArgb;
            }

            public override bool Equals(object? obj)
            {
                return obj is IconCacheKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Icon, ThemeMode, Size, AccentArgb);
            }
        }

        private readonly struct IconColors
        {
            private IconColors(
                Color stroke,
                Color fill,
                Color strongFill,
                Color softStroke)
            {
                Stroke = stroke;
                Fill = fill;
                StrongFill = strongFill;
                SoftStroke = softStroke;
            }

            public Color Stroke { get; }
            public Color Fill { get; }
            public Color StrongFill { get; }
            public Color SoftStroke { get; }

            public static IconColors ForTheme(
                AppThemeMode themeMode,
                Color accent)
            {
                int fillAlpha = themeMode == AppThemeMode.Dark ? 42 : 54;
                int strongFillAlpha = themeMode == AppThemeMode.Dark ? 82 : 94;
                int softStrokeAlpha = themeMode == AppThemeMode.Dark ? 210 : 230;

                return new IconColors(
                    accent,
                    Color.FromArgb(fillAlpha, accent),
                    Color.FromArgb(strongFillAlpha, accent),
                    Color.FromArgb(softStrokeAlpha, accent));
            }

            public static IconColors ForOutline(Color accent)
            {
                return new IconColors(
                    accent,
                    Color.Transparent,
                    Color.Transparent,
                    accent);
            }
        }
    }
}
