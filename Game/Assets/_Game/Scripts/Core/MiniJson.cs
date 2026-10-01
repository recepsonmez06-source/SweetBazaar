using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SweetBazaar.Core
{
    // Minimal JSON reader. Core cannot use UnityEngine's JsonUtility, and no JSON library ships with Unity's
    // runtime, so this covers standard JSON syntax. Result types: Dictionary<string, object>, List<object>,
    // string, double, bool and null. Malformed input throws FormatException.
    internal static class MiniJson
    {
        private const int MaxDepth = 64;

        public static object Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var reader = new Reader(text);
            reader.SkipWhitespace();
            object value = reader.ReadValue(0);
            reader.SkipWhitespace();
            if (!reader.AtEnd)
                throw reader.Error("Unexpected data after the JSON value");
            return value;
        }

        private sealed class Reader
        {
            private readonly string _text;
            private int _pos;

            public Reader(string text)
            {
                _text = text;
            }

            public bool AtEnd => _pos >= _text.Length;

            public FormatException Error(string message) =>
                new FormatException($"{message} at position {_pos}.");

            public void SkipWhitespace()
            {
                while (_pos < _text.Length && IsWhitespace(_text[_pos]))
                    _pos++;
            }

            public object ReadValue(int depth)
            {
                if (depth > MaxDepth)
                    throw Error("JSON is nested too deeply");
                if (AtEnd)
                    throw Error("Unexpected end of JSON");

                char c = _text[_pos];
                switch (c)
                {
                    case '{': return ReadObject(depth);
                    case '[': return ReadArray(depth);
                    case '"': return ReadString();
                    case 't': ReadLiteral("true"); return true;
                    case 'f': ReadLiteral("false"); return false;
                    case 'n': ReadLiteral("null"); return null;
                    default:
                        if (c == '-' || (c >= '0' && c <= '9'))
                            return ReadNumber();
                        throw Error($"Unexpected character '{c}'");
                }
            }

            private Dictionary<string, object> ReadObject(int depth)
            {
                var result = new Dictionary<string, object>();
                _pos++; // '{'
                SkipWhitespace();
                if (TryConsume('}'))
                    return result;

                while (true)
                {
                    SkipWhitespace();
                    if (AtEnd || _text[_pos] != '"')
                        throw Error("Expected a property name");
                    string name = ReadString();

                    SkipWhitespace();
                    if (!TryConsume(':'))
                        throw Error("Expected ':'");

                    SkipWhitespace();
                    result[name] = ReadValue(depth + 1);

                    SkipWhitespace();
                    if (TryConsume(','))
                        continue;
                    if (TryConsume('}'))
                        return result;
                    throw Error("Expected ',' or '}'");
                }
            }

            private List<object> ReadArray(int depth)
            {
                var result = new List<object>();
                _pos++; // '['
                SkipWhitespace();
                if (TryConsume(']'))
                    return result;

                while (true)
                {
                    SkipWhitespace();
                    result.Add(ReadValue(depth + 1));

                    SkipWhitespace();
                    if (TryConsume(','))
                        continue;
                    if (TryConsume(']'))
                        return result;
                    throw Error("Expected ',' or ']'");
                }
            }

            private string ReadString()
            {
                var builder = new StringBuilder();
                _pos++; // opening quote
                while (true)
                {
                    if (AtEnd)
                        throw Error("Unterminated string");

                    char c = _text[_pos++];
                    if (c == '"')
                        return builder.ToString();
                    if (c < ' ')
                        throw Error("Control character in string");
                    if (c != '\\')
                    {
                        builder.Append(c);
                        continue;
                    }

                    if (AtEnd)
                        throw Error("Unterminated escape sequence");
                    char escape = _text[_pos++];
                    switch (escape)
                    {
                        case '"': builder.Append('"'); break;
                        case '\\': builder.Append('\\'); break;
                        case '/': builder.Append('/'); break;
                        case 'b': builder.Append('\b'); break;
                        case 'f': builder.Append('\f'); break;
                        case 'n': builder.Append('\n'); break;
                        case 'r': builder.Append('\r'); break;
                        case 't': builder.Append('\t'); break;
                        case 'u': builder.Append(ReadUnicodeEscape()); break;
                        default: throw Error($"Invalid escape '\\{escape}'");
                    }
                }
            }

            private char ReadUnicodeEscape()
            {
                if (_pos + 4 > _text.Length)
                    throw Error("Incomplete \\u escape");

                string hex = _text.Substring(_pos, 4);
                if (!int.TryParse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int code))
                    throw Error("Invalid \\u escape");

                _pos += 4;
                return (char)code;
            }

            private double ReadNumber()
            {
                int start = _pos;
                if (_text[_pos] == '-')
                    _pos++;
                while (_pos < _text.Length && IsNumberCharacter(_text[_pos]))
                    _pos++;

                string token = _text.Substring(start, _pos - start);
                if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                {
                    _pos = start;
                    throw Error($"Invalid number '{token}'");
                }
                return value;
            }

            private void ReadLiteral(string literal)
            {
                if (string.CompareOrdinal(_text, _pos, literal, 0, literal.Length) != 0)
                    throw Error($"Expected '{literal}'");
                _pos += literal.Length;
            }

            private bool TryConsume(char c)
            {
                if (_pos < _text.Length && _text[_pos] == c)
                {
                    _pos++;
                    return true;
                }
                return false;
            }

            private static bool IsWhitespace(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\r';

            private static bool IsNumberCharacter(char c) =>
                (c >= '0' && c <= '9') || c == '.' || c == 'e' || c == 'E' || c == '+' || c == '-';
        }
    }
}
