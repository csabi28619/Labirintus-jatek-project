using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LabyrinthEditor.Models;

namespace LabyrinthEditor.Services
{
    public static class MapFileService
    {
        private static readonly Encoding FileEncoding =
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

        private const string MapSection    = "[MAP]";
        private const string EnemySection  = "[ENEMIES]";

        public static void Save(string filePath, MapData map)
        {
            var sb = new StringBuilder();

            sb.AppendLine(MapSection);
            for (int r = 0; r < map.Rows; r++)
            {
                for (int c = 0; c < map.Cols; c++)
                    sb.Append(map.GetTile(r, c));
                sb.AppendLine();
            }

            var enemies = map.GetEnemiesCopy();
            if (enemies.Count > 0)
            {
                sb.AppendLine(EnemySection);
                foreach (var kv in enemies)
                    sb.AppendLine($"{kv.Key}={kv.Value}");
            }

            File.WriteAllText(filePath, sb.ToString(), FileEncoding);

            string oldEnt = Path.ChangeExtension(filePath, ".ent");
            if (File.Exists(oldEnt)) File.Delete(oldEnt);

            map.FilePath = filePath;
            map.IsDirty  = false;
        }

        public static MapData Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Map file not found: {filePath}");

            string[] lines = File.ReadAllLines(filePath, FileEncoding);
            if (lines.Length == 0)
                throw new FormatException("Map file is empty.");

            bool hasSections = Array.Exists(lines, l => l.TrimEnd() == MapSection);

            MapData map = hasSections
                ? LoadSectioned(lines, filePath)
                : LoadLegacy(lines, filePath);

            string entPath = Path.ChangeExtension(filePath, ".ent");
            if (File.Exists(entPath))
            {
                LoadEnemiesFromEnt(map, entPath);

            }

            return map;
        }

        private static MapData LoadSectioned(string[] lines, string filePath)
        {
            var mapLines    = new List<string>();
            var enemyLines  = new List<string>();
            string section  = "";

            foreach (var raw in lines)
            {
                string line = raw.TrimEnd();
                if (line == MapSection)   { section = "map";    continue; }
                if (line == EnemySection) { section = "enemy";  continue; }

                if (section == "map")   mapLines.Add(line);
                if (section == "enemy") enemyLines.Add(line);
            }

            if (mapLines.Count == 0)
                throw new FormatException("No [MAP] section found in file.");

            var map = BuildGridFromLines(mapLines);
            map.FilePath = filePath;
            map.IsDirty  = false;

            ParseEnemyLines(map, enemyLines);
            return map;
        }

        private static MapData LoadLegacy(string[] lines, string filePath)
        {
            var gridLines = new List<string>();
            foreach (var line in lines)
                gridLines.Add(line.TrimEnd());

            var map = BuildGridFromLines(gridLines);
            map.FilePath = filePath;
            map.IsDirty  = false;
            return map;
        }

        private static MapData BuildGridFromLines(List<string> mapLines)
        {
            int rows = mapLines.Count;
            int cols = 0;
            foreach (var l in mapLines)
                cols = Math.Max(cols, l.Length);

            if (rows == 0 || cols == 0)
                throw new FormatException("Map grid is empty.");

            var grid = new char[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                string line = mapLines[r];
                for (int c = 0; c < cols; c++)
                    grid[r, c] = c < line.Length ? line[c] : '.';
            }
            return MapData.FromGrid(grid);
        }

        private static void ParseEnemyLines(MapData map, List<string> lines)
        {
            var enemies = new Dictionary<string, int>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                int eq = line.IndexOf('=');
                if (eq < 0) continue;
                string key   = line.Substring(0, eq).Trim();
                string value = line.Substring(eq + 1).Trim();
                if (int.TryParse(value, out int count) && count > 0)
                    enemies[key] = Math.Min(count, MapData.MaxEnemiesPerTile);
            }
            map.SetEnemies(enemies);
        }

        private static void LoadEnemiesFromEnt(MapData map, string entPath)
        {
            var lines = new List<string>(File.ReadAllLines(entPath, Encoding.UTF8));

            lines.RemoveAll(l => l.Trim() == "ENEMIES");
            ParseEnemyLines(map, lines);
        }

        public static string? ValidateFile(string filePath)
        {
            try { Load(filePath); return null; }
            catch (Exception ex) { return ex.Message; }
        }
    }
}
