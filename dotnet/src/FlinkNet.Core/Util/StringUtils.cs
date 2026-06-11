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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using FlinkNet.Annotations;

namespace FlinkNet.Util;

/// <summary>
/// Utility class to convert objects into strings and vice-versa.
///
/// <para>PORT NOTE: the <c>writeString</c>/<c>readString</c> family working on
/// <c>DataInputView</c>/<c>DataOutputView</c> is deferred to the <c>core.memory</c>
/// increment together with <c>StringValue</c>.</para>
/// </summary>
[PublicEvolving]
public static class StringUtils
{
    public static readonly string[] EmptyStringArray = [];

    private static readonly char[] HexChars =
    [
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f',
    ];

    /// <summary>
    /// Given an array of bytes it will convert the bytes to a hex string representation of the
    /// bytes.
    /// </summary>
    /// <param name="bytes">the bytes to convert in a hex string</param>
    /// <param name="start">start index, inclusively</param>
    /// <param name="end">end index, exclusively</param>
    /// <returns>hex string representation of the byte array</returns>
    public static string ByteToHexString(byte[] bytes, int start, int end)
    {
        if (bytes == null)
        {
            throw new ArgumentException("bytes == null");
        }

        int length = end - start;
        char[] outChars = new char[length * 2];

        for (int i = start, j = 0; i < end; i++)
        {
            outChars[j++] = HexChars[(0xF0 & bytes[i]) >>> 4];
            outChars[j++] = HexChars[0x0F & bytes[i]];
        }

        return new string(outChars);
    }

    /// <summary>
    /// Given an array of bytes it will convert the bytes to a hex string representation of the
    /// bytes.
    /// </summary>
    /// <param name="bytes">the bytes to convert in a hex string</param>
    /// <returns>hex string representation of the byte array</returns>
    public static string ByteToHexString(byte[] bytes) => ByteToHexString(bytes, 0, bytes.Length);

    /// <summary>
    /// Given a hex string this will return the byte array corresponding to the string.
    /// </summary>
    /// <param name="hex">the hex String array</param>
    /// <returns>a byte array that is a hex string representation of the given string.</returns>
    public static byte[] HexStringToByte(string hex)
    {
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = byte.Parse(
                hex.AsSpan(2 * i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }
        return bytes;
    }

    /// <summary>
    /// Converts the given object into a string representation by calling
    /// <see cref="object.ToString"/> and formatting (possibly nested) arrays and <c>null</c>.
    /// Mirrors the format of Java's <c>Arrays.deepToString</c>.
    /// </summary>
    public static string ArrayAwareToString(object? o)
    {
        if (o is null)
        {
            return "null";
        }

        if (o is Array array)
        {
            var builder = new StringBuilder("[");
            bool first = true;
            foreach (object? element in array)
            {
                if (!first)
                {
                    builder.Append(", ");
                }
                first = false;
                builder.Append(ArrayAwareToString(element));
            }
            return builder.Append(']').ToString();
        }

        return Convert.ToString(o, CultureInfo.InvariantCulture) ?? "null";
    }

    /// <summary>
    /// Replaces control characters by their escape-coded version. For example, if the string
    /// contains a line break character ('\n'), this character will be replaced by the two
    /// characters backslash '\' and 'n'.
    /// </summary>
    /// <param name="str">The string in which to replace the control characters.</param>
    /// <returns>The string with the replaced characters.</returns>
    public static string ShowControlCharacters(string str)
    {
        int len = str.Length;
        var sb = new StringBuilder();

        for (int i = 0; i < len; i += 1)
        {
            char c = str[i];
            switch (c)
            {
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\f':
                    sb.Append("\\f");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Creates a random string with a length within the given interval. The string contains only
    /// characters that can be represented as a single code point.
    /// </summary>
    /// <param name="rnd">The random used to create the strings.</param>
    /// <param name="minLength">The minimum string length.</param>
    /// <param name="maxLength">The maximum string length (inclusive).</param>
    /// <returns>A random String.</returns>
    public static string GetRandomString(Random rnd, int minLength, int maxLength)
    {
        int len = rnd.Next(maxLength - minLength + 1) + minLength;

        char[] data = new char[len];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (char)(rnd.Next(0x7fff) + 1);
        }
        return new string(data);
    }

    /// <summary>
    /// Creates a random string with a length within the given interval. The string contains only
    /// characters between the given boundaries.
    /// </summary>
    /// <param name="rnd">The random used to create the strings.</param>
    /// <param name="minLength">The minimum string length.</param>
    /// <param name="maxLength">The maximum string length (inclusive).</param>
    /// <param name="minValue">The minimum character value to occur.</param>
    /// <param name="maxValue">The maximum character value to occur.</param>
    /// <returns>A random String.</returns>
    public static string GetRandomString(
        Random rnd, int minLength, int maxLength, char minValue, char maxValue)
    {
        int len = rnd.Next(maxLength - minLength + 1) + minLength;

        char[] data = new char[len];
        int diff = maxValue - minValue + 1;

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (char)(rnd.Next(diff) + minValue);
        }
        return new string(data);
    }

    /// <summary>
    /// Creates a random alphanumeric string of given length.
    /// </summary>
    /// <param name="rnd">The random number generator to use.</param>
    /// <param name="length">The number of alphanumeric characters to append.</param>
    public static string GenerateRandomAlphanumericString(Random rnd, int length)
    {
        ArgumentNullException.ThrowIfNull(rnd);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        var buffer = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            buffer.Append(NextAlphanumericChar(rnd));
        }
        return buffer.ToString();
    }

    private static char NextAlphanumericChar(Random rnd)
    {
        int which = rnd.Next(62);
        char c;
        if (which < 10)
        {
            c = (char)('0' + which);
        }
        else if (which < 36)
        {
            c = (char)('A' - 10 + which);
        }
        else
        {
            c = (char)('a' - 36 + which);
        }
        return c;
    }

    /// <summary>
    /// Checks if the string is null, empty, or contains only whitespace characters. A whitespace
    /// character is defined via <see cref="char.IsWhiteSpace(char)"/>.
    /// </summary>
    /// <param name="str">The string to check</param>
    /// <returns>True, if the string is null or blank, false otherwise.</returns>
    public static bool IsNullOrWhitespaceOnly([NotNullWhen(false)] string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return true;
        }

        foreach (char c in str)
        {
            if (!char.IsWhiteSpace(c))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// If both string arguments are non-null, this method concatenates them with ' and '. If only
    /// one of the arguments is non-null, this method returns the non-null argument. If both
    /// arguments are null, this method returns null.
    /// </summary>
    /// <param name="s1">The first string argument</param>
    /// <param name="s2">The second string argument</param>
    /// <returns>The concatenated string, or non-null argument, or null</returns>
    public static string? ConcatenateWithAnd(string? s1, string? s2)
    {
        if (s1 != null)
        {
            return s2 == null ? s1 : s1 + " and " + s2;
        }
        return s2;
    }

    /// <summary>
    /// Generates a string containing a comma-separated list of values in double-quotes. Uses
    /// lower-cased values returned from <see cref="object.ToString"/> method for each element in
    /// the given array. Null values are skipped.
    /// </summary>
    /// <param name="values">array of elements for the list</param>
    /// <returns>The string with quoted list of elements</returns>
    public static string ToQuotedListString(object?[] values) =>
        "\""
            + string.Join(
                ", ",
                values.Where(v => v is not null).Select(v => v!.ToString()!.ToLowerInvariant()))
            + "\"";
}
