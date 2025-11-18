using System.Text.RegularExpressions;

namespace WindowsService.Functions
{
    public class TextFunctions
    {
        public static Regex WildcardMatch(string text)
        {
            string patternString = Regex.Replace(text, ".",
            x =>
            {
                string y = x.Value;
                if (y.Equals("?")) { return "."; }
                else if (y.Equals("*")) { return ".*"; }
                else { return Regex.Escape(y); }
            });
            if (!patternString.StartsWith("*")) { patternString = "^" + patternString; }
            if (!patternString.EndsWith("*")) { patternString = patternString + "$"; }
            return new Regex(patternString, RegexOptions.IgnoreCase);
        }

        public static string FormatFileSize(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        #region Enum parser

        /// <summary>
        /// String -> Enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T StringToFlags<T>(string text, Dictionary<string[], T> map) where T : Enum
        {
            var flags = default(T);
            foreach (var part in text.Split(',').Select(x => x.Trim()))
            {
                bool found = false;
                foreach (var kvp in map)
                {
                    if (kvp.Key.Any(x => string.Equals(x, part, StringComparison.OrdinalIgnoreCase)))
                    {
                        flags = (T)(object)(((int)(object)flags) | ((int)(object)kvp.Value));
                        found = true;
                        break;
                    }
                }
                if (!found) throw new ArgumentException($"The text '{text}' does not correspond to any value of the enum '{typeof(T).Name}'.");
            }
            return flags;
        }

        /// <summary>
        /// String -> Number
        /// if multiple parts, sum them up.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int StringToFlags(string text, Dictionary<string[], int> map)
        {
            int number = 0;
            foreach (var part in text.Split(',').Select(x => x.Trim()))
            {
                bool found = false;
                foreach (var kvp in map)
                {
                    if (kvp.Key.Any(x => string.Equals(x, part, StringComparison.OrdinalIgnoreCase)))
                    {
                        number += kvp.Value;
                        found = true;
                        break;
                    }
                }
                if (!found) throw new ArgumentException($"The text '{text}' does not correspond to any known value.");
            }
            return number;
        }

        /// <summary>
        /// Enum -> String
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="flags"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static string FlagsToString<T>(T flags, Dictionary<string[], T> map) where T : Enum
        {
            var parts = new List<string>();
            foreach (var kvp in map)
            {
                if (flags.HasFlag(kvp.Value))
                {
                    parts.Add(kvp.Key[0]);
                }
            }
            return parts.Count > 0 ? string.Join(", ", parts) : "Unknown";
        }

        /// <summary>
        /// Number -> String
        /// </summary>
        /// <param name="number"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static string FlagsToString(int number, Dictionary<string[], int> map)
        {
            foreach (var kvp in map)
            {
                if (number == kvp.Value)
                {
                    return kvp.Key[0];
                }
            }
            return number.ToString();
        }

        /// <summary>
        /// String -> Corrected String
        /// (string[], T) map to get the correct casing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetCorrect<T>(string text, Dictionary<string[], T> map) where T : Enum
        {
            var parts = new List<string>();
            foreach (var part in text.Split(',').Select(x => x.Trim()))
            {
                bool found = false;
                foreach (var key in map.Keys)
                {
                    if (key.Any(x => string.Equals(x, part, StringComparison.OrdinalIgnoreCase)))
                    {
                        parts.Add(key[0]);
                        found = true;
                        break;
                    }
                }
                if (!found) throw new ArgumentException($"The text '{text}' does not correspond to any value of the enum '{typeof(T).Name}'.");
            }
            return parts.Count > 0 ? string.Join(", ", parts) : "Unknown";
        }

        /// <summary>
        /// String -> Corrected String
        /// (string[], int) map to get the correct casing
        /// </summary>
        /// <param name="text"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetCorrect(string text, Dictionary<string[], int> map)
        {
            var parts = new List<string>();
            foreach (var part in text.Split(',').Select(x => x.Trim()))
            {
                bool found = false;
                foreach (var key in map.Keys)
                {
                    if (key.Any(x => string.Equals(x, part, StringComparison.OrdinalIgnoreCase)))
                    {
                        parts.Add(key[0]);
                        found = true;
                        break;
                    }
                }
                if (!found) throw new ArgumentException($"The text '{text}' does not correspond to any known value.");
            }
            return parts.Count > 0 ? string.Join(", ", parts) : "Unknown";
        }

        /// <summary>
        /// text Merge to Enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="flags"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T MergeFlags<T>(string text, T flags, Dictionary<string[], T> map) where T : Enum
        {
            var result = flags;
            foreach (var part in text.Split(',').Select(x => x.Trim()))
            {
                bool found = false;
                string tempPart = "";
                if (part.StartsWith("-"))
                {
                    tempPart = part.TrimStart('-');
                    foreach (var kvp in map)
                    {
                        if (kvp.Key.Any(x => string.Equals(x, tempPart, StringComparison.OrdinalIgnoreCase)))
                        {
                            result = (T)(object)(((int)(object)result) & ~((int)(object)kvp.Value));
                            found = true;
                            break;
                        }
                    }
                    continue;
                }
                tempPart = part.StartsWith("+") ? part.TrimStart('+') : part;
                foreach (var kvp in map)
                {
                    if (kvp.Key.Any(x => string.Equals(x, tempPart, StringComparison.OrdinalIgnoreCase)))
                    {
                        result = (T)(object)(((int)(object)result) | ((int)(object)kvp.Value));
                        found = true;
                        break;
                    }
                }
                if (!found) throw new ArgumentException($"The text '{text}' does not correspond to any value of the enum '{typeof(T).Name}'.");
            }
            return result;
        }

        #endregion
    }
}
