using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SweetBazaar.Core
{
    // Reads and writes a LevelPack as JSON:
    // {
    //   "formatVersion": 1,
    //   "levels": [
    //     {"number":1,"seed":1,"boxCapacity":4,"minMoves":4,"botWinRate":100,"boxes":[[0,1,0,1],[1,0,1,0],[],[]]},
    //     ...
    //   ]
    // }
    // Boxes list candies bottom -> top. "botWinRate" (percent of casual-bot play-throughs won) is optional.
    // One level per line keeps git diffs readable.
    public static class LevelPackJson
    {
        public const int FormatVersion = 1;

        public static string Serialize(LevelPack pack)
        {
            if (pack == null)
                throw new ArgumentNullException(nameof(pack));

            var json = new StringBuilder();
            json.Append("{\n");
            json.Append("  \"formatVersion\": ").Append(FormatVersion).Append(",\n");
            json.Append("  \"levels\": [");

            for (int i = 0; i < pack.Count; i++)
            {
                json.Append(i == 0 ? "\n    " : ",\n    ");
                AppendLevel(json, pack.Levels[i]);
            }

            json.Append(pack.Count == 0 ? "]\n" : "\n  ]\n");
            json.Append("}\n");
            return json.ToString();
        }

        // Throws FormatException if the text is not valid JSON or does not describe a level pack.
        // Unknown properties are ignored so newer files stay readable.
        public static LevelPack Parse(string json)
        {
            var root = MiniJson.Parse(json) as Dictionary<string, object>
                ?? throw new FormatException("The JSON root must be an object.");

            int version = ReadInt(root, "formatVersion", "the file");
            if (version != FormatVersion)
                throw new FormatException($"Unsupported level pack format version {version} (expected {FormatVersion}).");

            var levels = Require<List<object>>(root, "levels", "the file");

            var pack = new LevelPack();
            for (int i = 0; i < levels.Count; i++)
            {
                string where = $"level entry {i + 1}";
                var entry = levels[i] as Dictionary<string, object>
                    ?? throw new FormatException($"The {where} must be an object.");

                var definition = new LevelDefinition
                {
                    BoxCapacity = ReadInt(entry, "boxCapacity", where),
                    Boxes = ReadBoxes(entry, where),
                };
                var record = new LevelRecord(
                    ReadInt(entry, "number", where),
                    ReadInt(entry, "seed", where),
                    ReadInt(entry, "minMoves", where),
                    definition,
                    entry.ContainsKey("botWinRate") ? ReadInt(entry, "botWinRate", where) : -1);

                try
                {
                    pack.Add(record);
                }
                catch (ArgumentException e)
                {
                    throw new FormatException($"The {where}: {e.Message}", e);
                }
            }
            return pack;
        }

        private static void AppendLevel(StringBuilder json, LevelRecord record)
        {
            json.Append("{\"number\":").Append(Format(record.Number));
            json.Append(",\"seed\":").Append(Format(record.Seed));
            json.Append(",\"boxCapacity\":").Append(Format(record.Definition.BoxCapacity));
            json.Append(",\"minMoves\":").Append(Format(record.MinMoves));
            if (record.WinRate >= 0)
                json.Append(",\"botWinRate\":").Append(Format(record.WinRate));
            json.Append(",\"boxes\":[");

            var boxes = record.Definition.Boxes;
            for (int b = 0; b < boxes.Count; b++)
            {
                if (b > 0)
                    json.Append(',');
                json.Append('[');
                for (int c = 0; c < boxes[b].Length; c++)
                {
                    if (c > 0)
                        json.Append(',');
                    json.Append(Format(boxes[b][c]));
                }
                json.Append(']');
            }
            json.Append("]}");
        }

        private static string Format(int value) => value.ToString(CultureInfo.InvariantCulture);

        private static T Require<T>(Dictionary<string, object> obj, string name, string where) where T : class
        {
            if (!obj.TryGetValue(name, out object value))
                throw new FormatException($"Missing \"{name}\" in {where}.");
            return value as T
                ?? throw new FormatException($"\"{name}\" in {where} has the wrong type.");
        }

        private static int ReadInt(Dictionary<string, object> obj, string name, string where)
        {
            if (!obj.TryGetValue(name, out object value))
                throw new FormatException($"Missing \"{name}\" in {where}.");
            return ToInt(value, name, where);
        }

        private static int ToInt(object value, string name, string where)
        {
            if (value is double number
                && number == Math.Floor(number)
                && number >= int.MinValue
                && number <= int.MaxValue)
            {
                return (int)number;
            }
            throw new FormatException($"\"{name}\" in {where} must be a whole number.");
        }

        private static List<int[]> ReadBoxes(Dictionary<string, object> entry, string where)
        {
            var rawBoxes = Require<List<object>>(entry, "boxes", where);

            var boxes = new List<int[]>(rawBoxes.Count);
            foreach (var rawBox in rawBoxes)
            {
                var candies = rawBox as List<object>
                    ?? throw new FormatException($"Each box in the {where} must be an array.");

                var box = new int[candies.Count];
                for (int i = 0; i < box.Length; i++)
                    box[i] = ToInt(candies[i], "boxes", where);
                boxes.Add(box);
            }
            return boxes;
        }
    }
}
