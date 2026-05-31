using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LabyrinthEditor.Services
{
    public enum Language { Hungarian, English }

    public class LanguageService
    {
        private Dictionary<string, string> _strings = new Dictionary<string, string>();
        public Language Current { get; private set; } = Language.Hungarian;

        public static LanguageService Instance { get; } = new LanguageService();
        private LanguageService() { Load(Language.Hungarian); }

        public void Load(Language lang)
        {
            Current = lang;
            string fileName = lang == Language.Hungarian ? "lang_hu.txt" : "lang_en.txt";
            string path = Path.Combine("Resources", fileName);

            _strings.Clear();

            if (!File.Exists(path)) return;

            foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                int idx = line.IndexOf('=');
                if (idx < 0) continue;
                string key   = line.Substring(0, idx).Trim();
                string value = line.Substring(idx + 1).Trim();
                _strings[key] = value;
            }
        }

        public string Get(string key) =>
            _strings.TryGetValue(key, out var val) ? val : key;

        public void Toggle()
        {
            Load(Current == Language.Hungarian ? Language.English : Language.Hungarian);
        }
    }
}
