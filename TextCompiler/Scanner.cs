using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TextCompiler
{
    public class Scanner
    {
        private static readonly Dictionary<string, int> keywords = new Dictionary<string, int>
        {
            { "struct", 1 },
            { "String", 2 },
            { "u64", 3 },
            { "i32", 4 },  // Дополнительные типы данных Rust
            { "f64", 5 },
            { "bool", 6 }
        };

        private static readonly Dictionary<char, int> symbols = new Dictionary<char, int>
        {
            { '{', 9 },
            { '}', 10 },
            { ';', 11 },
            { ':', 12 },
            { ',', 13 }
        };

        public List<Token> Scan(string input)
        {
            List<Token> tokens = new List<Token>();
            string[] words = Regex.Split(input, "(\\s+|[{;}:,{}])");
            int position = 0;

            foreach (string word in words)
            {
                if (string.IsNullOrEmpty(word))
                    continue;

                int startPos = position;
                position += word.Length;

                if (keywords.ContainsKey(word))
                {
                    tokens.Add(new Token(word, keywords[word], "ключевое слово", startPos, position - 1));
                }
                else if (symbols.ContainsKey(word[0]) && word.Length == 1)
                {
                    string type = word[0] == ';' ? "конец оператора" : "символ";
                    tokens.Add(new Token(word, symbols[word[0]], type, startPos, position - 1));
                }
                else if (Regex.IsMatch(word, "^[a-zA-Z_][a-zA-Z0-9_]*$"))
                {
                    tokens.Add(new Token(word, 7, "идентификатор", startPos, position - 1));
                }
                else if (Regex.IsMatch(word, "^\\s+$"))
                {
                    tokens.Add(new Token(" ", 8, "разделитель", startPos, position - 1));
                }
                else
                {
                    tokens.Add(new Token(word, 14, "ERROR", startPos, position - 1));
                }
            }
            return tokens;
        }
    }

    public class Token
    {
        public string Value { get; }
        public int Code { get; }
        public string Type { get; }
        public int Start { get; }
        public int End { get; }

        public Token(string value, int code, string type, int start, int end)
        {
            Value = value;
            Code = code;
            Type = type;
            Start = start;
            End = end;
        }
    }
}
