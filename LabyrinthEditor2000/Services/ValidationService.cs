using System.Collections.Generic;
using LabyrinthEditor.Models;

namespace LabyrinthEditor.Services
{

    public enum ValidationSeverity { Info, Warning, Error }

    public class ValidationIssue
    {
        public ValidationSeverity Severity { get; }
        public string             Message  { get; }

        public ValidationIssue(ValidationSeverity severity, string message)
        {
            Severity = severity;
            Message  = message;
        }

        public override string ToString() => $"[{Severity}] {Message}";
    }

    public class ValidationResult
    {
        public List<ValidationIssue> Issues { get; } = new List<ValidationIssue>();
        public bool IsValid => !Issues.Exists(i => i.Severity == ValidationSeverity.Error);

        public void Add(ValidationSeverity s, string msg) =>
            Issues.Add(new ValidationIssue(s, msg));
    }

    public static class ValidationService
    {

        public static int GetRoomNumber(char[,] map)
        {
            int count = 0;
            int rows  = map.GetLength(0);
            int cols  = map.GetLength(1);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (map[r, c] == '█') count++;
            return count;
        }

        public static int GetSuitableEntrance(char[,] map)
        {
            int count = 0;
            int rows  = map.GetLength(0);
            int cols  = map.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {

                    bool onBorder = (r == 0 || r == rows - 1 || c == 0 || c == cols - 1);
                    if (!onBorder) continue;

                    var info = TileInfo.Get(map[r, c]);
                    if (!info.IsCorridor) continue;

                    if (r == 0       && info.OpenNorth) count++;
                    if (r == rows-1  && info.OpenSouth) count++;
                    if (c == 0       && info.OpenWest)  count++;
                    if (c == cols-1  && info.OpenEast)  count++;
                }
            }
            return count;
        }

        public static bool IsInvalidElement(char[,] map)
        {
            int rows = map.GetLength(0);
            int cols = map.GetLength(1);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (!TileInfo.IsValidChar(map[r, c])) return true;
            return false;
        }

        public static List<string> GetUnavailableElements(char[,] map)
        {
            var unavailable = new List<string>();
            int rows = map.GetLength(0);
            int cols = map.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var info = TileInfo.Get(map[r, c]);
                    if (info.IsVoid) continue;

                    bool reachable = false;

                    if (r > 0 && TileInfo.Get(map[r-1, c]).OpenSouth) reachable = true;

                    if (!reachable && r < rows-1 && TileInfo.Get(map[r+1, c]).OpenNorth) reachable = true;

                    if (!reachable && c > 0 && TileInfo.Get(map[r, c-1]).OpenEast) reachable = true;

                    if (!reachable && c < cols-1 && TileInfo.Get(map[r, c+1]).OpenWest) reachable = true;

                    if (!reachable)
                    {
                        if (r == 0       && info.OpenNorth) reachable = true;
                        if (r == rows-1  && info.OpenSouth) reachable = true;
                        if (c == 0       && info.OpenWest)  reachable = true;
                        if (c == cols-1  && info.OpenEast)  reachable = true;
                    }

                    if (!reachable)
                        unavailable.Add($"{r}:{c}");
                }
            }
            return unavailable;
        }

        public static char[,] GenerateLabyrinth(List<string> positionsList)
        {
            if (positionsList == null || positionsList.Count == 0)
                return new char[0, 0];

            var positions = new HashSet<(int r, int c)>();
            int maxR = 0, maxC = 0;
            foreach (var pos in positionsList)
            {
                var parts = pos.Split(':');
                if (parts.Length != 2) continue;
                if (!int.TryParse(parts[0], out int r)) continue;
                if (!int.TryParse(parts[1], out int c)) continue;
                positions.Add((r, c));
                if (r > maxR) maxR = r;
                if (c > maxC) maxC = c;
            }

            var grid = new char[maxR + 1, maxC + 1];

            for (int r = 0; r <= maxR; r++)
                for (int c = 0; c <= maxC; c++)
                    grid[r, c] = '.';

            foreach (var (r, c) in positions)
            {
                bool n = positions.Contains((r - 1, c));
                bool e = positions.Contains((r, c + 1));
                bool s = positions.Contains((r + 1, c));
                bool w = positions.Contains((r, c - 1));
                grid[r, c] = TileInfo.FromDirections(n, e, s, w);
            }

            return grid;
        }

        public static ValidationResult Validate(MapData map)
        {
            var result = new ValidationResult();
            var grid   = map.GetGridCopy();

            if (IsInvalidElement(grid))
                result.Add(ValidationSeverity.Error,
                    "Map contains invalid characters that are not in the allowed set.");

            int rooms = GetRoomNumber(grid);
            if (rooms == 0)
                result.Add(ValidationSeverity.Error,
                    "Map has no treasure rooms (█). Add at least one.");
            else
                result.Add(ValidationSeverity.Info,
                    $"Treasure rooms: {rooms}");

            int exits = GetSuitableEntrance(grid);
            if (exits == 0)
                result.Add(ValidationSeverity.Error,
                    "Map has no valid exits on the border. A corridor tile must open outward.");
            else
                result.Add(ValidationSeverity.Info,
                    $"Exits/entrances: {exits}");

            var isolated = GetUnavailableElements(grid);
            if (isolated.Count > 0)
                result.Add(ValidationSeverity.Warning,
                    $"Isolated tiles (unreachable by neighbors) at: {string.Join(", ", isolated)}");

            return result;
        }
    }
}
