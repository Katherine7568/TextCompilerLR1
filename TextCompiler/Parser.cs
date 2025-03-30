using System;
using System.Collections.Generic;
using System.Linq;

namespace TextCompiler
{
    internal class Parser
    {
        private List<Token> tokens;
        private int currentIndex;
        public List<(string Fragment, string Location)> Errors { get; private set; }
        private bool hasError;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens.Where(t => t.Code != 8).ToList(); // игнорируем пробелы
            currentIndex = 0;
            Errors = new List<(string, string)>();
            hasError = false;
        }

        public bool Parse()
        {
            try
            {
                Expect("struct");
                ExpectIdentifier("Student");
                ExpectSymbol("{");
                ExpectIdentifier("name");
                ExpectSymbol(":");
                ExpectKeyword("String");
                ExpectSymbol(",");
                ExpectIdentifier("roll");
                ExpectSymbol(":");
                ExpectKeyword("u64");
                ExpectSymbol(",");
                ExpectIdentifier("dept");
                ExpectSymbol(":");
                ExpectKeyword("String");
                ExpectSymbol(",");
                ExpectSymbol("}");
                ExpectSymbol(";");
                return Errors.Count == 0;
            }
            catch
            {
                return false;
            }
        }

        private void Expect(string value)
        {
            if (hasError) return;

            if (currentIndex >= tokens.Count || tokens[currentIndex].Value != value)
            {
                AddError(value);
            }
            else
            {
                currentIndex++;
            }
        }

        private void ExpectSymbol(string symbol)
        {
            if (hasError) return;

            if (currentIndex >= tokens.Count || tokens[currentIndex].Value != symbol)
            {
                AddError(symbol);
            }
            else
            {
                currentIndex++;
            }
        }

        private void ExpectKeyword(string keyword)
        {
            if (hasError) return;

            if (currentIndex >= tokens.Count || tokens[currentIndex].Value != keyword)
            {
                AddError(keyword);
            }
            else
            {
                currentIndex++;
            }
        }

        private void ExpectIdentifier(string expected = null)
        {
            if (hasError) return;

            if (currentIndex >= tokens.Count || tokens[currentIndex].Code != 7)
            {
                AddError(expected ?? "идентификатор");
            }
            else
            {
                currentIndex++;
            }
        }

        private void AddError(string expected)
        {
            if (hasError) return;

            if (currentIndex < tokens.Count)
            {
                var token = tokens[currentIndex];
                string location = $"с {token.Start + 1} по {token.End + 1} символ";
                Errors.Add(($"Ожидалось: '{expected}', получено: '{token.Value}'", location));
            }
            else
            {
                Errors.Add(($"Ожидалось: '{expected}', но достигнут конец ввода", "в конце строки"));
            }

            hasError = true; // Останавливаем после первой ошибки
        }
    }
}
