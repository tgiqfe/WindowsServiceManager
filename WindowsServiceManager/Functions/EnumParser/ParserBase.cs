namespace WindowsService.Functions.EnumParser
{
    internal class ParserBase<T> where T : Enum
    {
        protected Dictionary<string[], T> map;

        protected virtual void Initialize() { }

        public virtual T TextToFlags(string text)
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

        public virtual int TextToNumber(string text)
        {
            int number = 0;
            foreach (var part in text.Split(',').Select(x => x.Trim()))
            {
                bool found = false;
                foreach (var kvp in map)
                {
                    if (kvp.Key.Any(x => string.Equals(x, part, StringComparison.OrdinalIgnoreCase)))
                    {
                        number += (int)(object)kvp.Value;
                        found = true;
                        break;
                    }
                }
                if (!found) throw new ArgumentException($"The text '{text}' does not correspond to any known value.");
            }
            return number;
        }

        public virtual string FlagsToString(T flags)
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

        public virtual int FlagsToNumber(T flags)
        {
            var number = 0;
            foreach (var kvp in map)
            {
                if (flags.HasFlag((T)(object)kvp.Value))
                {
                    number += (int)(object)kvp.Value;
                }
            }
            return number;
        }

        public virtual string NumberToText(int number)
        {
            var parts = new List<string>();
            foreach (var kvp in map)
            {
                if (number >= (int)(object)kvp.Value)
                {
                    parts.Add(kvp.Key[0]);
                    number -= (int)(object)kvp.Value;
                }
            }
            return parts.Count > 0 ? string.Join(", ", parts) : "Unknown";
        }

        public virtual T NumberToFlags(int number)
        {
            var flags = default(T);
            foreach (var kvp in map)
            {
                if (number >= (int)(object)kvp.Value)
                {
                    flags = (T)(object)(((int)(object)flags) | ((int)(object)kvp.Value));
                    number -= (int)(object)kvp.Value;
                }
            }
            return flags;
        }

        public string GetCorrect(string text)
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

        public T MergeFlags(string text, T existingFlags)
        {
            var result = existingFlags;
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




    }
}
