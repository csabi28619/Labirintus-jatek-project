using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LabyrinthEditor.Services;
using LabyrinthEditor.ViewModels;

namespace LabyrinthEditor.Views
{
    public class MapCanvas : FrameworkElement
    {
        private MainViewModel _vm;

        private bool   _isPanning;
        private Point  _panStart;
        private double _panOffsetX;
        private double _panOffsetY;

        private bool _isPainting;
        private bool _isErasing;

        private readonly TranslateTransform _translate = new TranslateTransform();
        private readonly ScaleTransform     _scale     = new ScaleTransform(1, 1);
        private readonly TransformGroup     _transform;

        public MapCanvas()
        {
            _transform = new TransformGroup();
            _transform.Children.Add(_scale);
            _transform.Children.Add(_translate);

            ClipToBounds     = true;
            Focusable        = true;
            IsHitTestVisible = true;
        }

        public void SetViewModel(MainViewModel vm)
        {
            _vm = vm;
            _vm.RequestRedraw += () => InvalidateVisual();
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize) => availableSize;

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawRectangle(Brushes.Black, null, new Rect(RenderSize));

            if (_vm?.Map == null) return;

            dc.PushTransform(_transform);
            MapRenderer.Draw(
                dc,
                _vm.Map,
                _vm.CellSize,
                _vm.ShowGrid,
                _vm.ShowHighlights ? _vm.Highlights : null,
                _vm.HoverRow,
                _vm.HoverCol);
            dc.Pop();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            Focus();
            _isPainting = true;
            CaptureMouse();
            PaintAt(e.GetPosition(this));
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            _isPainting = false;
            ReleaseMouseCapture();
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            _isErasing = true;
            CaptureMouse();
            EraseAt(e.GetPosition(this));
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            _isErasing = false;
            ReleaseMouseCapture();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Middle)
            {
                _isPanning = true;
                _panStart  = e.GetPosition(this);
                CaptureMouse();
                Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.ChangedButton == MouseButton.Middle)
            {
                _isPanning = false;
                ReleaseMouseCapture();
                Cursor = Cursors.Arrow;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var pos = e.GetPosition(this);

            if (_isPanning)
            {
                var delta     = pos - _panStart;
                _panStart     = pos;
                _panOffsetX  += delta.X;
                _panOffsetY  += delta.Y;
                _translate.X  = _panOffsetX;
                _translate.Y  = _panOffsetY;
                InvalidateVisual();
                return;
            }

            var cell = CanvasToCell(pos);
            _vm.HoverRow = cell.row;
            _vm.HoverCol = cell.col;

            if (_isPainting) PaintAt(pos);
            if (_isErasing)  EraseAt(pos);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (_vm != null) { _vm.HoverRow = -1; _vm.HoverCol = -1; }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            double factor = e.Delta > 0 ? 1.15 : 1.0 / 1.15;
            _vm.CellSize *= factor;
            _scale.ScaleX *= factor;
            _scale.ScaleY *= factor;
            InvalidateVisual();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
                _vm.UndoCommand.Execute(null);
            if (e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control)
                _vm.RedoCommand.Execute(null);
        }

        private void PaintAt(Point screenPos)
        {
            var cell = CanvasToCell(screenPos);
            _vm.PaintCell(cell.row, cell.col);
        }

        private void EraseAt(Point screenPos)
        {
            var cell = CanvasToCell(screenPos);
            _vm.EraseCell(cell.row, cell.col);
        }

        private (int row, int col) CanvasToCell(Point screenPos)
        {
            var inv   = _transform.Inverse;
            var mapPt = inv != null ? inv.Transform(screenPos) : screenPos;
            return MapRenderer.PixelToCell(mapPt.X, mapPt.Y, _vm.CellSize);
        }

        public void ResetView()
        {
            _panOffsetX   = 0; _panOffsetY   = 0;
            _translate.X  = 0; _translate.Y  = 0;
            _scale.ScaleX = 1; _scale.ScaleY = 1;
            InvalidateVisual();
        }
    }
}
