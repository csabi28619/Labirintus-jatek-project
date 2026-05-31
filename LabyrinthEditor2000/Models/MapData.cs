using System;
using System.Collections.Generic;

namespace LabyrinthEditor.Models
{
    public class MapData
    {
        public int    Rows     { get; private set; }
        public int    Cols     { get; private set; }
        public string FilePath { get; set; } = string.Empty;
        public bool   IsDirty  { get; set; }

        private char[,] _grid;

        private readonly Dictionary<string, int> _enemies = new Dictionary<string, int>();

        public const int MaxEnemiesPerTile = 3;

        public MapData(int rows, int cols)
        {
            Rows  = rows;
            Cols  = cols;
            _grid = new char[rows, cols];
            Fill('.');
        }

        public static MapData FromGrid(char[,] grid)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            var md   = new MapData(rows, cols);
            Array.Copy(grid, md._grid, grid.Length);
            return md;
        }

        public char GetTile(int row, int col)
        {
            if (!InBounds(row, col)) return '.';
            return _grid[row, col];
        }

        public void SetTile(int row, int col, char c)
        {
            if (!InBounds(row, col)) return;
            if (_grid[row, col] == c) return;
            _grid[row, col] = c;

            if (c == '.')
                _enemies.Remove(Key(row, col));
            IsDirty = true;
        }

        public bool InBounds(int row, int col) =>
            row >= 0 && row < Rows && col >= 0 && col < Cols;

        public char[,] GetGridCopy()
        {
            var copy = new char[Rows, Cols];
            Array.Copy(_grid, copy, _grid.Length);
            return copy;
        }

        public int GetEnemyCount(int row, int col)
        {
            _enemies.TryGetValue(Key(row, col), out int count);
            return count;
        }

        public bool AddEnemy(int row, int col)
        {
            if (!InBounds(row, col)) return false;
            var info = TileInfo.Get(GetTile(row, col));
            if (info.IsVoid) return false;

            string key = Key(row, col);
            _enemies.TryGetValue(key, out int current);
            if (current >= MaxEnemiesPerTile) return false;

            _enemies[key] = current + 1;
            IsDirty = true;
            return true;
        }

        public bool RemoveEnemy(int row, int col)
        {
            if (!InBounds(row, col)) return false;
            string key = Key(row, col);
            if (!_enemies.TryGetValue(key, out int current) || current == 0) return false;

            if (current == 1)
                _enemies.Remove(key);
            else
                _enemies[key] = current - 1;

            IsDirty = true;
            return true;
        }

        public Dictionary<string, int> GetEnemiesCopy() =>
            new Dictionary<string, int>(_enemies);

        public void SetEnemies(Dictionary<string, int> enemies)
        {
            _enemies.Clear();
            foreach (var kv in enemies)
                _enemies[kv.Key] = kv.Value;
        }

        public int TotalEnemies()
        {
            int total = 0;
            foreach (var v in _enemies.Values) total += v;
            return total;
        }

        public void Fill(char c)
        {
            for (int r = 0; r < Rows; r++)
                for (int col = 0; col < Cols; col++)
                    _grid[r, col] = c;
        }

        public void Resize(int newRows, int newCols)
        {
            var newGrid = new char[newRows, newCols];
            for (int r = 0; r < newRows; r++)
                for (int c = 0; c < newCols; c++)
                    newGrid[r, c] = '.';
            int copyRows = Math.Min(Rows, newRows);
            int copyCols = Math.Min(Cols, newCols);
            for (int r = 0; r < copyRows; r++)
                for (int c = 0; c < copyCols; c++)
                    newGrid[r, c] = _grid[r, c];

            _grid = newGrid;
            Rows  = newRows;
            Cols  = newCols;

            var toRemove = new List<string>();
            foreach (var key in _enemies.Keys)
            {
                var (er, ec) = ParseKey(key);
                if (er >= newRows || ec >= newCols) toRemove.Add(key);
            }
            foreach (var k in toRemove) _enemies.Remove(k);

            IsDirty = true;
        }

        public string MapName =>
            string.IsNullOrEmpty(FilePath)
                ? "Untitled"
                : System.IO.Path.GetFileNameWithoutExtension(FilePath);

        private static string Key(int row, int col) => $"{row}:{col}";

        private static (int r, int c) ParseKey(string key)
        {
            var parts = key.Split(':');
            return (int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }
}
