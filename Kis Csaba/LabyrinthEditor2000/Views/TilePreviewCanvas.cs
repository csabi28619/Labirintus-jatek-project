using System.Windows;
using System.Windows.Media;
using LabyrinthEditor.Models;

namespace LabyrinthEditor.Views
{
    public class TilePreviewCanvas : FrameworkElement
    {
        public char TileChar { get; set; } = '.';

        private static readonly Brush BrushVoid     = Brushes.Black;
        private static readonly Brush BrushFloor    = new SolidColorBrush(Color.FromRgb(30, 30, 40));
        private static readonly Brush BrushRoom     = new SolidColorBrush(Color.FromRgb(180, 140, 20));
        private static readonly Brush BrushRoomGlow = new SolidColorBrush(Color.FromArgb(80, 255, 220, 60));
        private static readonly Brush BrushPlayer   = new SolidColorBrush(Color.FromRgb(40, 180, 80));
        private static readonly Brush BrushPlayerGlow = new SolidColorBrush(Color.FromArgb(80, 60, 255, 120));

        private static readonly Pen WallPen     = new Pen(new SolidColorBrush(Color.FromRgb(200, 50,  50)),  2.0);
        private static readonly Pen RoomOutline = new Pen(new SolidColorBrush(Color.FromRgb(255, 220, 60)),  1.5);
        private static readonly Pen PlayerOutline = new Pen(new SolidColorBrush(Color.FromRgb(60, 255, 120)), 1.5);

        static TilePreviewCanvas()
        {
            BrushVoid.Freeze();     BrushFloor.Freeze();
            BrushRoom.Freeze();     BrushRoomGlow.Freeze();
            BrushPlayer.Freeze();   BrushPlayerGlow.Freeze();
            WallPen.Freeze();       RoomOutline.Freeze();   PlayerOutline.Freeze();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double w = ActualWidth;
            double h = ActualHeight;
            if (w <= 0 || h <= 0) return;

            var rect = new Rect(0, 0, w, h);
            var info = TileInfo.Get(TileChar);

            if (info.IsVoid)
            {
                dc.DrawRectangle(BrushVoid, null, rect);
                return;
            }

            if (info.IsRoom)
            {
                dc.DrawRectangle(BrushRoom,     null,        rect);
                dc.DrawRectangle(BrushRoomGlow, null,        rect);
                dc.DrawRectangle(null,          RoomOutline, rect);
                return;
            }

            if (TileChar == 'P')
            {
                dc.DrawRectangle(BrushPlayer,     null,          rect);
                dc.DrawRectangle(BrushPlayerGlow, null,          rect);
                dc.DrawRectangle(null,            PlayerOutline, rect);
                return;
            }

            dc.DrawRectangle(BrushFloor, null, rect);

            double x1 = rect.Left,  x2 = rect.Right;
            double y1 = rect.Top,   y2 = rect.Bottom;

            if (!info.OpenNorth) dc.DrawLine(WallPen, new Point(x1, y1), new Point(x2, y1));
            if (!info.OpenEast)  dc.DrawLine(WallPen, new Point(x2, y1), new Point(x2, y2));
            if (!info.OpenSouth) dc.DrawLine(WallPen, new Point(x1, y2), new Point(x2, y2));
            if (!info.OpenWest)  dc.DrawLine(WallPen, new Point(x1, y1), new Point(x1, y2));
        }
    }
}
