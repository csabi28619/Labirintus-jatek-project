using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LabyrinthEditor.Models;
using LabyrinthEditor.Services;

namespace LabyrinthEditor.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        public event EventHandler CanExecuteChanged;
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        { _execute = execute; _canExecute = canExecute; }
        public bool CanExecute(object p) => _canExecute == null || _canExecute(p);
        public void Execute(object p)    => _execute(p);
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public enum EditorTool { Corridor, Room, Void, Enemy, PlayerStart }

    internal class MapSnapshot
    {
        public char[,]                 Grid    { get; }
        public Dictionary<string, int> Enemies { get; }
        public MapSnapshot(MapData map)
        { Grid = map.GetGridCopy(); Enemies = map.GetEnemiesCopy(); }
    }

    public class MainViewModel : INotifyPropertyChanged
    {

        private MapData    _map;
        private double     _cellSize   = MapRenderer.DefaultCellSize;
        private bool       _showGrid   = true;
        private bool       _showHighlights = true;
        private EditorTool _activeTool = EditorTool.Corridor;
        private char       _selectedCorridorChar = '═';
        private int        _hoverRow   = -1;
        private int        _hoverCol   = -1;
        private string     _statusText = "";

        private readonly Stack<MapSnapshot> _undoStack = new Stack<MapSnapshot>();
        private readonly Stack<MapSnapshot> _redoStack = new Stack<MapSnapshot>();

        public HighlightLayer Highlights { get; } = new HighlightLayer();

        public MapData Map
        {
            get => _map;
            private set { _map = value; OnPropertyChanged(); OnPropertyChanged(nameof(TitleText)); }
        }

        public double CellSize
        {
            get => _cellSize;
            set { _cellSize = Math.Clamp(value, 8, 128); OnPropertyChanged(); RequestRedraw?.Invoke(); }
        }

        public bool ShowGrid
        {
            get => _showGrid;
            set { _showGrid = value; OnPropertyChanged(); RequestRedraw?.Invoke(); }
        }

        public bool ShowHighlights
        {
            get => _showHighlights;
            set { _showHighlights = value; OnPropertyChanged(); RequestRedraw?.Invoke(); }
        }

        public EditorTool ActiveTool
        {
            get => _activeTool;
            set { _activeTool = value; OnPropertyChanged(); }
        }

        public char SelectedCorridorChar
        {
            get => _selectedCorridorChar;
            set { _selectedCorridorChar = value; OnPropertyChanged(); }
        }

        public int HoverRow
        {
            get => _hoverRow;
            set { _hoverRow = value; OnPropertyChanged(); UpdateStatusText(); RequestRedraw?.Invoke(); }
        }

        public int HoverCol
        {
            get => _hoverCol;
            set { _hoverCol = value; OnPropertyChanged(); UpdateStatusText(); RequestRedraw?.Invoke(); }
        }

        public string StatusText
        {
            get => _statusText;
            private set { _statusText = value; OnPropertyChanged(); }
        }

        public string TitleText
        {
            get
            {
                if (_map == null) return "Labyrinth Editor";
                string dirty = _map.IsDirty ? " *" : "";
                return $"Labyrinth Editor — {_map.MapName}{dirty}";
            }
        }

        public event Action RequestRedraw;
        public event Action MapChanged;

        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        public MainViewModel()
        {
            UndoCommand = new RelayCommand(_ => Undo(), _ => _undoStack.Count > 0);
            RedoCommand = new RelayCommand(_ => Redo(), _ => _redoStack.Count > 0);
            NewMap(15, 20);
        }

        public void NewMap(int rows, int cols)
        {
            _undoStack.Clear(); _redoStack.Clear();
            Map = new MapData(rows, cols);
            RefreshHighlights();
            NotifyMapChanged();
        }

        public void LoadMap(string filePath)
        {
            _undoStack.Clear(); _redoStack.Clear();
            Map = MapFileService.Load(filePath);
            RefreshHighlights();
            NotifyMapChanged();
        }

        public void SaveMap(string filePath)
        {
            MapFileService.Save(filePath, Map);
            OnPropertyChanged(nameof(TitleText));
        }

        public void PaintCell(int row, int col)
        {
            if (_map == null || !_map.InBounds(row, col)) return;

            if (ActiveTool == EditorTool.Enemy)
            {
                var info = TileInfo.Get(_map.GetTile(row, col));
                if (info.IsVoid) return;
                PushUndo();
                if (!_map.AddEnemy(row, col)) { _undoStack.Pop(); return; }
            }
            else
            {
                char newChar = GetPaintChar();
                if (_map.GetTile(row, col) == newChar) return;
                PushUndo();

                if (newChar == 'P')
                    ClearExistingPlayerStart();
                _map.SetTile(row, col, newChar);
            }

            OnPropertyChanged(nameof(TitleText));
            RefreshHighlights();
            NotifyMapChanged();
        }

        public void EraseCell(int row, int col)
        {
            if (_map == null || !_map.InBounds(row, col)) return;

            if (ActiveTool == EditorTool.Enemy)
            {
                if (_map.GetEnemyCount(row, col) == 0) return;
                PushUndo();
                _map.RemoveEnemy(row, col);
            }
            else
            {
                if (_map.GetTile(row, col) == '.') return;
                PushUndo();
                _map.SetTile(row, col, '.');
            }

            OnPropertyChanged(nameof(TitleText));
            RefreshHighlights();
            NotifyMapChanged();
        }

        private void ClearExistingPlayerStart()
        {
            for (int r = 0; r < _map.Rows; r++)
                for (int c = 0; c < _map.Cols; c++)
                    if (_map.GetTile(r, c) == 'P')
                        _map.SetTile(r, c, '.');
        }

        private char GetPaintChar() => ActiveTool switch
        {
            EditorTool.Void        => '.',
            EditorTool.PlayerStart => 'P',
            EditorTool.Room        => '█',
            EditorTool.Corridor    => _selectedCorridorChar,
            _                      => '.'
        };

        public void RefreshHighlights()
        {
            Highlights.Clear();
            if (_map == null || !_showHighlights) return;

            var grid = _map.GetGridCopy();
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (!TileInfo.IsValidChar(grid[r, c]))
                        Highlights.Set(r, c, HighlightKind.InvalidChar);

            foreach (var pos in ValidationService.GetUnavailableElements(grid))
            {
                var parts = pos.Split(':');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int r) &&
                    int.TryParse(parts[1], out int c))
                {

                    if (Highlights.Get(r, c) == HighlightKind.None)
                        Highlights.Set(r, c, HighlightKind.Isolated);
                }
            }

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    bool onBorder = (r == 0 || r == rows-1 || c == 0 || c == cols-1);
                    if (!onBorder) continue;
                    var info = TileInfo.Get(grid[r, c]);
                    if (!info.IsCorridor && !info.IsPlayer) continue;
                    bool isExit =
                        (r == 0      && info.OpenNorth) ||
                        (r == rows-1 && info.OpenSouth) ||
                        (c == 0      && info.OpenWest)  ||
                        (c == cols-1 && info.OpenEast);

                    if (isExit && Highlights.Get(r, c) == HighlightKind.None)
                        Highlights.Set(r, c, HighlightKind.Exit);
                }
            }
        }

        public ValidationResult RunValidation()
        {
            RefreshHighlights();
            RequestRedraw?.Invoke();
            return _map != null ? ValidationService.Validate(_map) : new ValidationResult();
        }

        private void PushUndo()
        {
            _undoStack.Push(new MapSnapshot(_map));
            _redoStack.Clear();
            ((RelayCommand)UndoCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RedoCommand).RaiseCanExecuteChanged();
        }

        private void Undo()
        {
            if (_undoStack.Count == 0) return;
            _redoStack.Push(new MapSnapshot(_map));
            RestoreSnapshot(_undoStack.Pop());
        }

        private void Redo()
        {
            if (_redoStack.Count == 0) return;
            _undoStack.Push(new MapSnapshot(_map));
            RestoreSnapshot(_redoStack.Pop());
        }

        private void RestoreSnapshot(MapSnapshot snap)
        {
            var restored = MapData.FromGrid(snap.Grid);
            restored.FilePath = _map.FilePath;
            restored.IsDirty  = true;
            restored.SetEnemies(snap.Enemies);
            Map = restored;
            ((RelayCommand)UndoCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RedoCommand).RaiseCanExecuteChanged();
            RefreshHighlights();
            NotifyMapChanged();
        }

        private void UpdateStatusText()
        {
            if (_map == null) { StatusText = ""; return; }

            string tileChar  = _map.InBounds(_hoverRow, _hoverCol)
                ? _map.GetTile(_hoverRow, _hoverCol).ToString() : "-";
            int enemyCount   = _map.InBounds(_hoverRow, _hoverCol)
                ? _map.GetEnemyCount(_hoverRow, _hoverCol) : 0;
            string enemyInfo = enemyCount > 0 ? $"  Enemies: {enemyCount}/3" : "";

            var hlKind = _map.InBounds(_hoverRow, _hoverCol)
                ? Highlights.Get(_hoverRow, _hoverCol)
                : HighlightKind.None;
            string hlInfo = hlKind switch
            {
                HighlightKind.InvalidChar => "  ⚠ INVALID CHAR",
                HighlightKind.Isolated    => "  ⚠ ISOLATED",
                HighlightKind.Exit        => "  ✓ EXIT",
                _                         => ""
            };

            StatusText =
                $"Row: {_hoverRow}  Col: {_hoverCol}  Tile: {tileChar}{enemyInfo}{hlInfo}" +
                $"  |  Zoom: {(int)(_cellSize / MapRenderer.DefaultCellSize * 100)}%" +
                $"  |  {(_map.IsDirty ? "● Unsaved" : "Saved")}";
        }

        private void NotifyMapChanged()
        {
            MapChanged?.Invoke();
            RequestRedraw?.Invoke();
            UpdateStatusText();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
