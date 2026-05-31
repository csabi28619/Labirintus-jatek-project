using System.Collections.Generic;

namespace LabyrinthEditor.Models
{
    public class TileInfo
    {
        public char Character  { get; }
        public bool OpenNorth  { get; }
        public bool OpenEast   { get; }
        public bool OpenSouth  { get; }
        public bool OpenWest   { get; }
        public bool IsVoid     { get; }
        public bool IsRoom     { get; }
        public bool IsPlayer   { get; }
        public bool IsCorridor => !IsVoid && !IsRoom && !IsPlayer;

        private TileInfo(char c, bool n, bool e, bool s, bool w,
                         bool isVoid = false, bool isRoom = false, bool isPlayer = false)
        {
            Character = c;
            OpenNorth = n; OpenEast = e; OpenSouth = s; OpenWest = w;
            IsVoid    = isVoid;
            IsRoom    = isRoom;
            IsPlayer  = isPlayer;
        }

        private static readonly Dictionary<char, TileInfo> _lookup =
            new Dictionary<char, TileInfo>
        {
            { '╬', new TileInfo('╬', true,  true,  true,  true)  },
            { '═', new TileInfo('═', false, true,  false, true)  },
            { '║', new TileInfo('║', true,  false, true,  false) },
            { '╔', new TileInfo('╔', false, true,  true,  false) },
            { '╗', new TileInfo('╗', false, false, true,  true)  },
            { '╚', new TileInfo('╚', true,  true,  false, false) },
            { '╝', new TileInfo('╝', true,  false, false, true)  },
            { '╦', new TileInfo('╦', false, true,  true,  true)  },
            { '╩', new TileInfo('╩', true,  true,  false, true)  },
            { '╠', new TileInfo('╠', true,  true,  true,  false) },
            { '╣', new TileInfo('╣', true,  false, true,  true)  },
            { '█', new TileInfo('█', true,  true,  true,  true,  isRoom:   true) },
            { 'P', new TileInfo('P', true,  true,  true,  true,  isPlayer: true) },
            { '.', new TileInfo('.', false, false, false, false,  isVoid:   true) },
        };

        public static TileInfo Get(char c) =>
            _lookup.TryGetValue(c, out var info)
                ? info
                : new TileInfo(c, false, false, false, false, isVoid: true);

        public static bool IsValidChar(char c) => _lookup.ContainsKey(c);
        public static IEnumerable<TileInfo> AllTiles => _lookup.Values;

        public static char FromDirections(bool n, bool e, bool s, bool w)
        {
            foreach (var kv in _lookup)
            {
                var t = kv.Value;
                if (t.IsCorridor &&
                    t.OpenNorth == n && t.OpenEast == e &&
                    t.OpenSouth == s && t.OpenWest == w)
                    return kv.Key;
            }
            return '.';
        }

        public override string ToString() => Character.ToString();
    }
}
