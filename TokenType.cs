// Lexer.cs
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PumaCompiler
{
    public enum TokenType
    {
        Keyword,
        Integer,
        Float,
        Boolean,
        String,
        Char,
        Identifier,
        EndOfLine,
        Unknown
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Position { get; set; }
    }

    public class Lexer
    {
        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "using", "as", "type", "trait", "module", "is", "has",
            "value", "object", "base", "number", "optional",
            "enums", "records", "properties", "functions", "start", "initialize", "finalize",
            "return", "yield", "public", "private", "internal", "override", "delegate",
            "constant", "readonly", "readwrite", "int128", "int64", "int32", "int16", "int8",
            "uint128", "uint64", "uint32", "uint16", "uint8", "flt128", "flt64", "flt32",
            "fix128", "fix64", "fix32", "char", "str", "fstr", "vstr", "bool", "true", "false",
            "hex", "oct", "bin", "implicit", "explicit", "operator", "get", "set", "with", "self",
            "if", "else", "and", "or", "not", "for", "in", "while", "repeat", "forall",
            "begin", "end", "break", "continue", "match", "when", "error", "catch",
            "multithread", "multiprocess"
        };

        private readonly string _source;

        public Lexer(string source)
        {
            _source = source;
        }

        public IEnumerable<Token> Tokenize()
        {
            var patterns = new Dictionary<TokenType, string>
            {
                { TokenType.Float, @"^\d+\.\d+" },
                { TokenType.Integer, @"^\d+" },
                { TokenType.Boolean, @"^(true|false)" },
                { TokenType.String, "^\"(\\\\.|[^\"])*\"" },
                { TokenType.Char, @"^'(\\.|[^'])'" },
                { TokenType.Identifier, @"^[a-zA-Z_][a-zA-Z0-9_]*" }
            };

            int position = 0;
            string input = _source;

            while (position < input.Length)
            {
                // Handle end-of-line markers
                if (IsEndOfLine(input, position, out int eolLength))
                {
                    // Check for backslash before EOL
                    bool isEscaped = position > 0 && input[position - 1] == '\\';
                    if (!isEscaped)
                    {
                        yield return new Token
                        {
                            Type = TokenType.EndOfLine,
                            Value = "\n",
                            Position = position
                        };
                    }
                    position += eolLength;
                    continue;
                }

                // Skip whitespace
                if (char.IsWhiteSpace(input[position]))
                {
                    position++;
                    continue;
                }

                bool matched = false;

                foreach (var pattern in patterns)
                {
                    var regex = new Regex(pattern.Value);
                    var match = regex.Match(input.Substring(position));
                    if (match.Success)
                    {
                        var value = match.Value;
                        TokenType type = pattern.Key;

                        // Check if identifier is a keyword
                        if (type == TokenType.Identifier && Keywords.Contains(value))
                        {
                            type = TokenType.Keyword;
                        }

                        yield return new Token
                        {
                            Type = type,
                            Value = value,
                            Position = position
                        };
                        position += match.Length;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    // Unknown character/token
                    yield return new Token
                    {
                        Type = TokenType.Unknown,
                        Value = input[position].ToString(),
                        Position = position
                    };
                    position++;
                }
            }
        }

        private bool IsEndOfLine(string input, int position, out int length)
        {
            // Check for \r\n, \n, \r
            if (input[position] == '\r')
            {
                if (position + 1 < input.Length && input[position + 1] == '\n')
                {
                    length = 2;
                    return true;
                }
                length = 1;
                return true;
            }
            if (input[position] == '\n')
            {
                length = 1;
                return true;
            }
            length = 0;
            return false;
        }
    }
}
