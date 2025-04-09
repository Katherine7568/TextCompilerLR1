using System;
using System.Collections.Generic;
using System.Linq;

namespace TextCompiler
{
    public class Scanner
    {
        public class ScanResult
        {
            public string Value { get; set; }
            public int Code { get; set; }
            public string Type { get; set; }
            public int Start { get; set; }
            public int End { get; set; }
            public int Line { get; set; }
            public string Hint { get; set; } = "";
            public bool IsError => Code == 14;
        }

        private static readonly Dictionary<string, int> keywords = new Dictionary<string, int>
        {
            { "struct", 2 }, { "String", 3 }, { "u64", 4 }, { "i32", 5 }, { "f64", 6 }, { "bool", 7 }
        };

        private static readonly Dictionary<char, int> symbols = new Dictionary<char, int>
        {
            { '{', 9 }, { '}', 10 }, { ';', 11 }, { ':', 12 }, { ',', 13 }
        };

        private const int SPACE_CODE = 8;
        private static readonly HashSet<char> validIdentChars =
            new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_");

        public List<ScanResult> Scan(string input)
        {
            var tokens = new List<ScanResult>();
            if (string.IsNullOrEmpty(input)) return tokens;

            int position = 0;
            int line = 1;
            bool hasError = false;

            // 1. Ожидаем ключевое слово struct
            if (!ExpectKeyword(ref position, "struct", ref line, input, tokens, ref hasError))
                return tokens;

            // 2. Ожидаем значащий пробел после struct
            if (!ExpectSignificantSpace(ref position, ref line, input, tokens, ref hasError))
                return tokens;

            // 3. Ожидаем имя структуры
            if (!ExpectStructName(ref position, ref line, input, tokens, ref hasError))
                return tokens;

            // 4. Ожидаем открывающую скобку {
            if (!ExpectSymbol(ref position, '{', ref line, input, tokens, ref hasError))
                return tokens;

            // 5. Обрабатываем поля структуры
            while (position < input.Length && !hasError)
            {
                // Пропускаем пробелы и новые строки
                SkipWhitespace(ref position, ref line, input);

                // Проверяем закрывающую скобку
                if (PeekChar(position, input) == '}')
                    break;

                // 5.1. Ожидаем имя поля
                if (!ExpectIdentifier(ref position, ref line, input, tokens, ref hasError, "имя поля"))
                    return tokens;

                // 5.2. Ожидаем двоеточие
                if (!ExpectSymbol(ref position, ':', ref line, input, tokens, ref hasError))
                    return tokens;

                // 5.3. Ожидаем тип поля
                if (!ExpectType(ref position, ref line, input, tokens, ref hasError))
                    return tokens;

                // Пропускаем пробелы после типа
                SkipWhitespace(ref position, ref line, input);

                // Проверяем разделитель (запятая или закрывающая скобка)
                char nextChar = PeekChar(position, input);
                if (nextChar == ',')
                {
                    position++;
                    tokens.Add(CreateToken(",", 13, "запятая", position - 1, position - 1, line));
                }
                else if (nextChar != '}')
                {
                    tokens.Add(CreateErrorToken("", position, position, line, "Ожидается ',' или '}' после типа поля"));
                    hasError = true;
                    return tokens;
                }
            }

            // 6. Ожидаем закрывающую скобку }
            if (!ExpectSymbol(ref position, '}', ref line, input, tokens, ref hasError))
                return tokens;

            // 7. Ожидаем точку с запятой ;
            if (!ExpectSymbol(ref position, ';', ref line, input, tokens, ref hasError))
                return tokens;

            return tokens;
        }

        private bool ExpectKeyword(ref int pos, string keyword, ref int line, string input,
            List<ScanResult> tokens, ref bool hasError)
        {
            SkipWhitespace(ref pos, ref line, input);

            int start = pos;
            while (pos < input.Length && char.IsLetter(input[pos]))
                pos++;

            string word = input.Substring(start, pos - start);
            if (word != keyword)
            {
                tokens.Add(CreateErrorToken(word, start, pos - 1, line, $"Ожидается ключевое слово '{keyword}'"));
                hasError = true;
                return false;
            }

            tokens.Add(CreateToken(word, keywords[keyword], "ключевое слово", start, pos - 1, line));
            return true;
        }

        private bool ExpectSignificantSpace(ref int pos, ref int line, string input,
            List<ScanResult> tokens, ref bool hasError)
        {
            if (pos >= input.Length || !char.IsWhiteSpace(input[pos]))
            {
                tokens.Add(CreateErrorToken("", pos, pos, line, "Ожидается пробел после 'struct'"));
                hasError = true;
                return false;
            }

            tokens.Add(CreateToken(" ", SPACE_CODE, "значащий пробел", pos, pos, line));
            pos++;
            return true;
        }

        private bool ExpectStructName(ref int pos, ref int line, string input, List<ScanResult> tokens, ref bool hasError)
        {
            SkipWhitespace(ref pos, ref line, input);

            int start = pos;
            while (pos < input.Length && validIdentChars.Contains(input[pos]))
                pos++;

            if (start == pos)
            {
                tokens.Add(CreateErrorToken("", pos, pos, line, "Ожидается имя структуры"));
                hasError = true;
                return false;
            }

            // Проверяем, есть ли недопустимые символы после имени структуры
            if (pos < input.Length && !char.IsWhiteSpace(input[pos]) && input[pos] != '{')
            {
                tokens.Add(CreateErrorToken(input[pos].ToString(), pos, pos, line, "Недопустимый символ в имени структуры"));
                hasError = true;
                return false;
            }

            string ident = input.Substring(start, pos - start);
            tokens.Add(CreateToken(ident, 1, "идентификатор", start, pos - 1, line));
            return true;
        }

        private bool ExpectIdentifier(ref int pos, ref int line, string input, List<ScanResult> tokens, ref bool hasError, string expected)
        {
            SkipWhitespace(ref pos, ref line, input);

            int start = pos;
            while (pos < input.Length && validIdentChars.Contains(input[pos]))
                pos++;

            if (start == pos)
            {
                tokens.Add(CreateErrorToken("", pos, pos, line, $"Ожидается {expected}"));
                hasError = true;
                return false;
            }

            // Проверяем, есть ли недопустимые символы после идентификатора
            if (pos < input.Length && !char.IsWhiteSpace(input[pos]) &&
                input[pos] != ':' && input[pos] != '{' && input[pos] != '}')
            {
                tokens.Add(CreateErrorToken(input[pos].ToString(), pos, pos, line, "Недопустимый символ в идентификаторе"));
                hasError = true;
                return false;
            }

            string ident = input.Substring(start, pos - start);
            tokens.Add(CreateToken(ident, 1, "идентификатор", start, pos - 1, line));
            return true;
        }

        private bool ExpectType(ref int pos, ref int line, string input,
            List<ScanResult> tokens, ref bool hasError)
        {
            SkipWhitespace(ref pos, ref line, input);

            int start = pos;
            while (pos < input.Length && char.IsLetterOrDigit(input[pos]))
                pos++;

            string type = input.Substring(start, pos - start);
            if (!keywords.ContainsKey(type))
            {
                tokens.Add(CreateErrorToken(type, start, pos - 1, line, "Ожидается тип поля (String, u64, i32, f64, bool)"));
                hasError = true;
                return false;
            }

            tokens.Add(CreateToken(type, keywords[type], "ключевое слово", start, pos - 1, line));
            return true;
        }

        private bool ExpectSymbol(ref int pos, char symbol, ref int line, string input,
            List<ScanResult> tokens, ref bool hasError)
        {
            SkipWhitespace(ref pos, ref line, input);

            if (pos >= input.Length || input[pos] != symbol)
            {
                tokens.Add(CreateErrorToken("", pos, pos, line, $"Ожидается символ '{symbol}'"));
                hasError = true;
                return false;
            }

            tokens.Add(CreateToken(symbol.ToString(), symbols[symbol], GetSymbolType(symbol), pos, pos, line));
            pos++;
            return true;
        }

        private void SkipWhitespace(ref int pos, ref int line, string input)
        {
            while (pos < input.Length)
            {
                if (input[pos] == '\n')
                {
                    line++;
                    pos++;
                }
                else if (char.IsWhiteSpace(input[pos]))
                {
                    pos++;
                }
                else
                {
                    break;
                }
            }
        }

        private char PeekChar(int pos, string input)
        {
            return pos < input.Length ? input[pos] : '\0';
        }

        private ScanResult CreateToken(string value, int code, string type, int start, int end, int line)
        {
            return new ScanResult
            {
                Value = value,
                Code = code,
                Type = type,
                Start = start,
                End = end,
                Line = line
            };
        }

        private ScanResult CreateErrorToken(string value, int start, int end, int line, string hint)
        {
            return new ScanResult
            {
                Value = value,
                Code = 14,
                Type = "ERROR",
                Start = start,
                End = end,
                Line = line,
                Hint = hint
            };
        }

        private string GetSymbolType(char c)
        {
            switch (c)
            {
                case ';': return "конец оператора";
                case '{': return "открывающая скобка";
                case '}': return "закрывающая скобка";
                case ':': return "двоеточие";
                case ',': return "запятая";
                default: return "символ";
            }
        }
    }
}