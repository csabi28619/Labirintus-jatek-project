using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using LabyrinthEditor.Models;

namespace LabyrinthEditor.Services
{

    public enum HighlightKind { None, InvalidChar, Isolated, Exit }

    public class HighlightLayer
    {

        private readonly Dictionary<string, HighlightKind> _cells =
            new Dictionary<string, HighlightKind>();

        public void Set(int row, int col, HighlightKind kind) =>
            _cells[$"{row}:{col}"] = kind;

        public HighlightKind Get(int row, int col) =>
            _cells.TryGetValue($"{row}:{col}", out var k) ? k : HighlightKind.None;

        public void Clear() => _cells.Clear();
        public int Count    => _cells.Count;
    }

    public static class MapRenderer
    {
        public const double DefaultCellSize = 32.0;

        private static readonly Brush BrushVoid       = Brushes.Black;
        private static readonly Brush BrushFloor      = new SolidColorBrush(Color.FromRgb(30,  30,  40));
        private static readonly Brush BrushRoom       = new SolidColorBrush(Color.FromRgb(180, 140, 20));
        private static readonly Brush BrushRoomGlow   = new SolidColorBrush(Color.FromArgb(80,  255, 220, 60));
        private static readonly Brush BrushPlayer     = new SolidColorBrush(Color.FromRgb(40,  180, 80));
        private static readonly Brush BrushPlayerGlow = new SolidColorBrush(Color.FromArgb(80,  60,  255, 120));
        private static readonly Brush BrushGrid       = new SolidColorBrush(Color.FromArgb(30,  255, 255, 255));
        private static readonly Brush BrushHover      = new SolidColorBrush(Color.FromArgb(80,  100, 200, 255));

        private static readonly Brush BrushHlInvalid  = new SolidColorBrush(Color.FromArgb(160, 255, 30,  30));
        private static readonly Brush BrushHlIsolated = new SolidColorBrush(Color.FromArgb(160, 255, 140, 0));
        private static readonly Brush BrushHlExit     = new SolidColorBrush(Color.FromArgb(120, 0,   220, 255));

        private static readonly Pen PenHlInvalid      = new Pen(new SolidColorBrush(Color.FromRgb(255, 30,  30)),  2.0);
        private static readonly Pen PenHlIsolated     = new Pen(new SolidColorBrush(Color.FromRgb(255, 140, 0)),   2.0);
        private static readonly Pen PenHlExit         = new Pen(new SolidColorBrush(Color.FromRgb(0,   220, 255)), 2.0);

        private static readonly Brush BrushEnemyBody  = new SolidColorBrush(Color.FromRgb(220, 40,  40));
        private static readonly Brush BrushEnemyBadge = new SolidColorBrush(Color.FromRgb(255, 100, 100));
        private static readonly Brush BrushEnemyText  = Brushes.White;

        private static readonly Pen WallPen       = new Pen(new SolidColorBrush(Color.FromRgb(200, 50,  50)),  2.5);
        private static readonly Pen GridPen       = new Pen(BrushGrid, 0.5);
        private static readonly Pen RoomOutline   = new Pen(new SolidColorBrush(Color.FromRgb(255, 220, 60)),  2.0);
        private static readonly Pen PlayerOutline = new Pen(new SolidColorBrush(Color.FromRgb(60,  255, 120)), 2.0);
        private static readonly Pen EnemyPen      = new Pen(new SolidColorBrush(Color.FromRgb(255, 80,  80)),  1.5);

        private static readonly Typeface LabelTypeface = new Typeface("Consolas");

        static MapRenderer()
        {
            BrushVoid.Freeze();       BrushFloor.Freeze();      BrushRoom.Freeze();
            BrushRoomGlow.Freeze();   BrushPlayer.Freeze();     BrushPlayerGlow.Freeze();
            BrushGrid.Freeze();       BrushHover.Freeze();
            BrushHlInvalid.Freeze();  BrushHlIsolated.Freeze(); BrushHlExit.Freeze();
            BrushEnemyBody.Freeze();  BrushEnemyBadge.Freeze(); BrushEnemyText.Freeze();
            WallPen.Freeze();         GridPen.Freeze();          RoomOutline.Freeze();
            PlayerOutline.Freeze();   EnemyPen.Freeze();
            PenHlInvalid.Freeze();    PenHlIsolated.Freeze();   PenHlExit.Freeze();
        }

        public static void Draw(
            DrawingContext dc,
            MapData        map,
            double         cellSize,
            bool           showGrid,
            HighlightLayer highlights = null,
            int            hoverRow   = -1,
            int            hoverCol   = -1)
        {
            if (map == null) return;

            for (int r = 0; r < map.Rows; r++)
            {
                for (int c = 0; c < map.Cols; c++)
                {
                    double x    = c * cellSize;
                    double y    = r * cellSize;
                    var    rect = new Rect(x, y, cellSize, cellSize);

                    DrawTile(dc, map.GetTile(r, c), rect);

                    int eCount = map.GetEnemyCount(r, c);
                    if (eCount > 0)
                        DrawEnemies(dc, rect, eCount, cellSize);

                    var kind = highlights?.Get(r, c) ?? HighlightKind.None;
                    if (kind != HighlightKind.None)
                        DrawHighlight(dc, rect, kind, cellSize);

                    if (showGrid)
                        dc.DrawRectangle(null, GridPen, rect);

                    if (r == hoverRow && c == hoverCol)
                        dc.DrawRectangle(BrushHover, null, rect);
                }
            }
        }

        private static void DrawTile(DrawingContext dc, char ch, Rect rect)
        {
            var info = TileInfo.Get(ch);

            if (info.IsVoid)   { dc.DrawRectangle(BrushVoid, null, rect); return; }
            if (info.IsRoom)   { DrawRoom(dc, rect);   return; }
            if (info.IsPlayer) { DrawPlayer(dc, rect); return; }

            dc.DrawRectangle(BrushFloor, null, rect);
            double x1 = rect.Left, x2 = rect.Right;
            double y1 = rect.Top,  y2 = rect.Bottom;
            if (!info.OpenNorth) dc.DrawLine(WallPen, new Point(x1, y1), new Point(x2, y1));
            if (!info.OpenEast)  dc.DrawLine(WallPen, new Point(x2, y1), new Point(x2, y2));
            if (!info.OpenSouth) dc.DrawLine(WallPen, new Point(x1, y2), new Point(x2, y2));
            if (!info.OpenWest)  dc.DrawLine(WallPen, new Point(x1, y1), new Point(x1, y2));
        }

        private static void DrawRoom(DrawingContext dc, Rect rect)
        {
            dc.DrawRectangle(BrushRoom,     null,        rect);
            dc.DrawRectangle(BrushRoomGlow, null,        rect);
            dc.DrawRectangle(null,          RoomOutline, rect);
        }

        private static void DrawPlayer(DrawingContext dc, Rect rect)
        {
            dc.DrawRectangle(BrushPlayer,     null,          rect);
            dc.DrawRectangle(BrushPlayerGlow, null,          rect);
            dc.DrawRectangle(null,            PlayerOutline, rect);
            DrawCentreLabel(dc, rect, "P", BrushVoid, rect.Width * 0.45);
        }

        private static void DrawHighlight(DrawingContext dc, Rect rect,
                                          HighlightKind kind, double cellSize)
        {
            Brush fill; Pen border; string label;
            switch (kind)
            {
                case HighlightKind.InvalidChar:
                    fill = BrushHlInvalid;  border = PenHlInvalid;
                    label = "!"; break;
                case HighlightKind.Isolated:
                    fill = BrushHlIsolated; border = PenHlIsolated;
                    label = "?"; break;
                case HighlightKind.Exit:
                    fill = BrushHlExit;     border = PenHlExit;
                    label = "E"; break;
                default: return;
            }

            dc.DrawRectangle(fill, border, rect);

            if (cellSize >= 20)
            {
                double fontSize = cellSize * 0.28;
                var ft = new FormattedText(
                    label, CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, LabelTypeface,
                    fontSize, Brushes.White, 96);
                dc.DrawText(ft, new Point(rect.Left + 2, rect.Top + 1));
            }
        }

        private static void DrawEnemies(DrawingContext dc, Rect rect, int count, double cellSize)
        {
            double radius = cellSize * 0.13;
            if (radius < 3) radius = 3;

            var offsets = GetEnemyOffsets(count, cellSize);
            double cx = rect.Left + rect.Width  / 2;
            double cy = rect.Top  + rect.Height / 2;

            foreach (var (ox, oy) in offsets)
                dc.DrawEllipse(BrushEnemyBody, EnemyPen, new Point(cx + ox, cy + oy), radius, radius);

            DrawEnemyBadge(dc, rect, count, cellSize);
        }

        private static (double x, double y)[] GetEnemyOffsets(int count, double cell)
        {
            double s = cell * 0.18;
            return count switch
            {
                1 => new[] { (0.0, 0.0) },
                2 => new[] { (-s,  0.0), (s, 0.0) },
                _ => new[] { (0.0, -s),  (-s, s * 0.7), (s, s * 0.7) }
            };
        }

        private static void DrawEnemyBadge(DrawingContext dc, Rect rect, int count, double cellSize)
        {
            double badgeR = cellSize * 0.14;
            if (badgeR < 5) badgeR = 5;
            double bx = rect.Right - badgeR - 2;
            double by = rect.Top   + badgeR + 2;
            dc.DrawEllipse(BrushEnemyBadge, null, new Point(bx, by), badgeR, badgeR);
            DrawCentreLabel(dc,
                new Rect(bx - badgeR, by - badgeR, badgeR * 2, badgeR * 2),
                count.ToString(), BrushEnemyText, badgeR * 1.1);
        }

        private static void DrawCentreLabel(DrawingContext dc, Rect rect,
                                             string text, Brush brush, double emSize)
        {
            var ft = new FormattedText(
                text, CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, LabelTypeface,
                emSize, brush, 96);
            dc.DrawText(ft, new Point(
                rect.Left + (rect.Width  - ft.Width)  / 2,
                rect.Top  + (rect.Height - ft.Height) / 2));
        }

        public static (int row, int col) PixelToCell(double x, double y, double cellSize) =>
            ((int)(y / cellSize), (int)(x / cellSize));

        public static Rect CellToRect(int row, int col, double cellSize) =>
            new Rect(col * cellSize, row * cellSize, cellSize, cellSize);

        public static Size MapPixelSize(MapData map, double cellSize) =>
            map == null ? Size.Empty : new Size(map.Cols * cellSize, map.Rows * cellSize);
    }
}
