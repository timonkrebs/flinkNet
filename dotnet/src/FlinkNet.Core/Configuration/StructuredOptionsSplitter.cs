/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text;
using FlinkNet.Annotations;

namespace FlinkNet.Configuration;

/// <summary>Helper class for splitting a string on a given delimiter with quoting logic.</summary>
[Internal]
internal static class StructuredOptionsSplitter
{
    /// <summary>
    /// Splits the given string on the given delimiter. It supports quoting parts of the string
    /// with either single (') or double quotes ("). Quotes can be escaped by doubling the quotes.
    ///
    /// <para>Examples:</para>
    /// <list type="bullet">
    ///   <item><description><c>'A;B';C</c> =&gt; <c>[A;B], [C]</c></description></item>
    ///   <item><description><c>"AB'D";B;C</c> =&gt; <c>[AB'D], [B], [C]</c></description></item>
    ///   <item><description><c>"AB'""D;B";C</c> =&gt; <c>[AB'"D;B], [C]</c></description></item>
    /// </list>
    ///
    /// <para>For more examples check the tests.</para>
    /// </summary>
    /// <param name="str">a string to split</param>
    /// <param name="delimiter">delimiter to split on</param>
    /// <returns>a list of splits</returns>
    internal static List<string> SplitEscaped(string str, char delimiter)
    {
        ArgumentNullException.ThrowIfNull(str);
        List<Token> tokens = Tokenize(str, delimiter);
        return ProcessTokens(tokens);
    }

    /// <summary>
    /// Escapes the given string with single quotes, if the input string contains a double quote
    /// or any of the given <paramref name="charsToEscape"/>. Any single quotes in the input string
    /// will be escaped by doubling.
    ///
    /// <para>Given that the escapeChar is (;)</para>
    ///
    /// <para>Examples:</para>
    /// <list type="bullet">
    ///   <item><description>A,B,C,D =&gt; A,B,C,D</description></item>
    ///   <item><description>A'B'C'D =&gt; 'A''B''C''D'</description></item>
    ///   <item><description>A;BCD =&gt; 'A;BCD'</description></item>
    ///   <item><description>AB"C"D =&gt; 'AB"C"D'</description></item>
    ///   <item><description>AB'"D:B =&gt; 'AB''"D:B'</description></item>
    /// </list>
    /// </summary>
    /// <param name="str">a string which needs to be escaped</param>
    /// <param name="charsToEscape">escape chars for the escape conditions</param>
    /// <returns>escaped string by single quote</returns>
    internal static string EscapeWithSingleQuote(string str, params string[] charsToEscape)
    {
        bool escape =
            charsToEscape.Any(str.Contains) || str.Contains('"') || str.Contains('\'');

        if (escape)
        {
            return "'" + str.Replace("'", "''") + "'";
        }

        return str;
    }

    private static List<string> ProcessTokens(List<Token> tokens)
    {
        var splits = new List<string>();
        for (int i = 0; i < tokens.Count; i++)
        {
            Token token = tokens[i];
            switch (token.TokenType)
            {
                case TokenType.DoubleQuoted:
                case TokenType.SingleQuoted:
                    if (i + 1 < tokens.Count && tokens[i + 1].TokenType != TokenType.Delimiter)
                    {
                        int illegalPosition = tokens[i + 1].Position - 1;
                        throw new ArgumentException(
                            "Could not split string. Illegal quoting at position: "
                                + illegalPosition);
                    }
                    splits.Add(token.Value);
                    break;
                case TokenType.Unquoted:
                    splits.Add(token.Value);
                    break;
                case TokenType.Delimiter:
                    if (i + 1 < tokens.Count && tokens[i + 1].TokenType == TokenType.Delimiter)
                    {
                        splits.Add("");
                    }
                    break;
            }
        }

        return splits;
    }

    private static List<Token> Tokenize(string str, char delimiter)
    {
        var tokens = new List<Token>();
        var builder = new StringBuilder();
        for (int cursor = 0; cursor < str.Length; )
        {
            char c = str[cursor];

            int nextChar = cursor + 1;
            if (c == '\'')
            {
                nextChar = ConsumeInQuotes(str, '\'', cursor, builder);
                tokens.Add(new Token(TokenType.SingleQuoted, builder.ToString(), cursor));
            }
            else if (c == '"')
            {
                nextChar = ConsumeInQuotes(str, '"', cursor, builder);
                tokens.Add(new Token(TokenType.DoubleQuoted, builder.ToString(), cursor));
            }
            else if (c == delimiter)
            {
                tokens.Add(new Token(TokenType.Delimiter, c.ToString(), cursor));
            }
            else if (!char.IsWhiteSpace(c))
            {
                nextChar = ConsumeUnquoted(str, delimiter, cursor, builder);
                tokens.Add(new Token(TokenType.Unquoted, builder.ToString().Trim(), cursor));
            }
            builder.Length = 0;
            cursor = nextChar;
        }

        return tokens;
    }

    private static int ConsumeInQuotes(string str, char quote, int cursor, StringBuilder builder)
    {
        for (int i = cursor + 1; i < str.Length; i++)
        {
            char c = str[i];
            if (c == quote)
            {
                if (i + 1 < str.Length && str[i + 1] == quote)
                {
                    builder.Append(c);
                    i += 1;
                }
                else
                {
                    return i + 1;
                }
            }
            else
            {
                builder.Append(c);
            }
        }

        throw new ArgumentException("Could not split string. Quoting was not closed properly.");
    }

    private static int ConsumeUnquoted(string str, char delimiter, int cursor, StringBuilder builder)
    {
        int i;
        for (i = cursor; i < str.Length; i++)
        {
            char c = str[i];
            if (c == delimiter)
            {
                return i;
            }

            builder.Append(c);
        }

        return i;
    }

    private enum TokenType
    {
        DoubleQuoted,
        SingleQuoted,
        Unquoted,
        Delimiter,
    }

    private sealed class Token(StructuredOptionsSplitter.TokenType tokenType, string value, int position)
    {
        public TokenType TokenType { get; } = tokenType;

        public string Value { get; } = value;

        public int Position { get; } = position;
    }
}
