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

using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using FlinkNet.Annotations;

namespace FlinkNet.Util;

/// <summary>
/// The semantics of Java's boxed primitive types where .NET differs: <c>hashCode</c>,
/// <c>compare</c>, the canonical bit conversions and the floating-point <c>toString</c>.
///
/// <para>PORT NOTE: Flink hashes keys to assign them to partitions, so every process must agree
/// on a key's hash; .NET's <see cref="string.GetHashCode()"/> is randomized per process, and
/// several primitive hashes differ from Java's. The floating-point comparisons follow Java's
/// total order (-0.0 before 0.0, NaN above positive infinity), unlike
/// <see cref="double.CompareTo(double)"/>. Java prints <c>1.0</c> and <c>1.0E10</c> where
/// .NET prints <c>1</c> and <c>1E+10</c>.</para>
/// </summary>
[Internal]
public static partial class JavaPrimitives
{
    /// <summary>Java's <c>Boolean.hashCode</c>.</summary>
    public static int HashCode(bool value) => value ? 1231 : 1237;

    /// <summary>Java's <c>Long.hashCode</c>.</summary>
    public static int HashCode(long value) => (int)(value ^ (value >>> 32));

    /// <summary>Java's <c>Float.hashCode</c>.</summary>
    public static int HashCode(float value) => FloatToIntBits(value);

    /// <summary>Java's <c>Double.hashCode</c>.</summary>
    public static int HashCode(double value) => HashCode(DoubleToLongBits(value));

    /// <summary>Java's <c>String.hashCode</c>: s[0]*31^(n-1) + ... + s[n-1] over UTF-16 code units.</summary>
    public static int HashCode(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        int hash = 0;
        foreach (char c in value)
        {
            hash = 31 * hash + c;
        }
        return hash;
    }

    /// <summary>
    /// Java's <c>hashCode</c> of a (boxed) value: the Java hash for the types whose .NET hash
    /// differs (string, bool, char, sbyte, short, long, float, double), else
    /// <see cref="object.GetHashCode"/>; 0 for null.
    /// </summary>
    public static int ObjectHashCode(object? value) =>
        value switch
        {
            null => 0,
            string s => HashCode(s),
            bool b => HashCode(b),
            char c => c,
            sbyte v => v,
            short v => v,
            int v => v,
            long v => HashCode(v),
            float v => HashCode(v),
            double v => HashCode(v),
            _ => value.GetHashCode(),
        };

    /// <summary>
    /// Java's <c>equals</c> of two (boxed) values, null-safe: doubles and floats compare by
    /// their canonical bits like Java's <c>Double/Float.equals</c> (-0.0 differs from 0.0, NaN
    /// equals NaN); everything else by <see cref="object.Equals(object)"/>.
    /// </summary>
    public static bool ObjectEquals(object? first, object? second) =>
        first switch
        {
            null => second is null,
            double d => second is double e && DoubleToLongBits(d) == DoubleToLongBits(e),
            float f => second is float g && FloatToIntBits(f) == FloatToIntBits(g),
            _ => first.Equals(second),
        };

    /// <summary>Java's <c>Float.floatToIntBits</c>: the bits, with every NaN collapsed to the
    /// canonical 0x7fc00000.</summary>
    public static int FloatToIntBits(float value) =>
        float.IsNaN(value) ? 0x7fc00000 : BitConverter.SingleToInt32Bits(value);

    /// <summary>Java's <c>Double.doubleToLongBits</c>: the bits, with every NaN collapsed to the
    /// canonical 0x7ff8000000000000.</summary>
    public static long DoubleToLongBits(double value) =>
        double.IsNaN(value) ? 0x7ff8000000000000L : BitConverter.DoubleToInt64Bits(value);

    /// <summary>Java's <c>Float.compare</c>: a total order with -0.0 before 0.0 and NaN above
    /// positive infinity (and equal to itself).</summary>
    public static int Compare(float first, float second)
    {
        if (first < second)
        {
            return -1;
        }
        if (first > second)
        {
            return 1;
        }
        int firstBits = FloatToIntBits(first);
        int secondBits = FloatToIntBits(second);
        return firstBits == secondBits ? 0 : firstBits < secondBits ? -1 : 1;
    }

    /// <summary>Java's <c>Double.compare</c>: a total order with -0.0 before 0.0 and NaN above
    /// positive infinity (and equal to itself).</summary>
    public static int Compare(double first, double second)
    {
        if (first < second)
        {
            return -1;
        }
        if (first > second)
        {
            return 1;
        }
        long firstBits = DoubleToLongBits(first);
        long secondBits = DoubleToLongBits(second);
        return firstBits == secondBits ? 0 : firstBits < secondBits ? -1 : 1;
    }

    /// <summary>
    /// Java's <c>Double.toString</c> (JDK 19 and later): the shortest decimal that rounds to
    /// the value, plain for magnitudes in [10^-3, 10^7) and in computerized scientific notation
    /// otherwise, always with a fractional digit (<c>1.0</c>, <c>1.0E10</c>, <c>4.9E-324</c>,
    /// <c>NaN</c>, <c>-Infinity</c>).
    /// </summary>
    public static string ToString(double value)
    {
        if (!double.IsFinite(value) || value == 0)
        {
            return SpecialToString(double.IsNaN(value), double.IsNegative(value), double.IsInfinity(value));
        }
        (bool negative, string digits, int exponent) =
            ShortestDecimal(value.ToString("R", CultureInfo.InvariantCulture));
        if (digits.Length == 1)
        {
            long bits = BitConverter.DoubleToInt64Bits(Math.Abs(value));
            int biasedExponent = (int)(bits >>> 52);
            long fraction = bits & 0xfffffffffffffL;
            (digits, exponent) = PreferTwoDigits(
                digits,
                exponent,
                biasedExponent == 0 ? fraction : fraction | (1L << 52),
                Math.Max(biasedExponent, 1) - 1075,
                candidate => double.Parse(candidate, CultureInfo.InvariantCulture) == Math.Abs(value));
        }
        return Format(negative, digits, exponent);
    }

    /// <summary>Java's <c>Float.toString</c> (JDK 19 and later), with the rules of
    /// <see cref="ToString(double)"/> applied to the float's own shortest decimal.</summary>
    public static string ToString(float value)
    {
        if (!float.IsFinite(value) || value == 0)
        {
            return SpecialToString(float.IsNaN(value), float.IsNegative(value), float.IsInfinity(value));
        }
        (bool negative, string digits, int exponent) =
            ShortestDecimal(value.ToString("R", CultureInfo.InvariantCulture));
        if (digits.Length == 1)
        {
            int bits = BitConverter.SingleToInt32Bits(Math.Abs(value));
            int biasedExponent = bits >>> 23;
            int fraction = bits & 0x7fffff;
            (digits, exponent) = PreferTwoDigits(
                digits,
                exponent,
                biasedExponent == 0 ? fraction : fraction | (1 << 23),
                Math.Max(biasedExponent, 1) - 150,
                candidate => float.Parse(candidate, CultureInfo.InvariantCulture) == Math.Abs(value));
        }
        return Format(negative, digits, exponent);
    }

    /// <summary>
    /// Java's <c>Double.parseDouble</c>: the value is trimmed of characters up to U+0020, and
    /// accepts an optional sign with <c>NaN</c>, <c>Infinity</c> (case-sensitive), a decimal
    /// number with an optional exponent, or a hexadecimal float (<c>0x1.8p1</c>), each with an
    /// optional <c>f</c>/<c>d</c> type suffix. Unlike .NET's default parsing, grouping separators
    /// ("1,5") are rejected.
    /// </summary>
    /// <exception cref="FormatException">If the value is not a Java floating-point literal (Java:
    /// <c>NumberFormatException</c>).</exception>
    public static double ParseDouble(string value)
    {
        string s = JavaTrim(value);
        if (TryParseSpecial(s, out bool negative, out bool isNaN))
        {
            // the canonical NaN: .NET's double.NaN has the sign bit set
            return isNaN
                ? BitConverter.Int64BitsToDouble(0x7ff8000000000000L)
                : negative ? double.NegativeInfinity : double.PositiveInfinity;
        }
        Match hex = HexFloatRegex().Match(s);
        if (hex.Success)
        {
            return BitConverter.Int64BitsToDouble(HexFloatToBits(hex, fractionBits: 52, exponentBias: 1023));
        }
        return double.Parse(CheckDecimal(s, value), DecimalStyle, CultureInfo.InvariantCulture);
    }

    /// <summary>Java's <c>Float.parseFloat</c>, with the grammar of <see cref="ParseDouble"/>;
    /// the value is rounded once, directly to float.</summary>
    /// <exception cref="FormatException">If the value is not a Java floating-point
    /// literal.</exception>
    public static float ParseFloat(string value)
    {
        string s = JavaTrim(value);
        if (TryParseSpecial(s, out bool negative, out bool isNaN))
        {
            return isNaN
                ? BitConverter.Int32BitsToSingle(0x7fc00000)
                : negative ? float.NegativeInfinity : float.PositiveInfinity;
        }
        Match hex = HexFloatRegex().Match(s);
        if (hex.Success)
        {
            return BitConverter.Int32BitsToSingle((int)HexFloatToBits(hex, fractionBits: 23, exponentBias: 127));
        }
        return float.Parse(CheckDecimal(s, value), DecimalStyle, CultureInfo.InvariantCulture);
    }

    private const NumberStyles DecimalStyle =
        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;

    // Java's String.trim: removes the characters up to U+0020 (incl. control characters, but
    // not other Unicode white space such as U+00A0)
    private static string JavaTrim(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        int start = 0;
        int end = value.Length;
        while (start < end && value[start] <= ' ')
        {
            start++;
        }
        while (end > start && value[end - 1] <= ' ')
        {
            end--;
        }
        return value[start..end];
    }

    private static bool TryParseSpecial(string s, out bool negative, out bool isNaN)
    {
        negative = s.StartsWith('-');
        string unsigned = s.Length > 0 && (s[0] == '-' || s[0] == '+') ? s[1..] : s;
        isNaN = unsigned == "NaN";
        return isNaN || unsigned == "Infinity";
    }

    // validates Java's decimal grammar and drops the type suffix, which .NET does not accept
    private static string CheckDecimal(string s, string original)
    {
        if (!DecimalFloatRegex().IsMatch(s))
        {
            throw new FormatException("For input string: \"" + original + "\"");
        }
        return s[^1] is 'f' or 'F' or 'd' or 'D' ? s[..^1] : s;
    }

    // The IEEE bits of a hexadecimal float (sign, mantissa * 2^exponent) in a binary format with
    // the given fraction width and exponent bias, rounded once, half to even, like Java.
    private static long HexFloatToBits(Match hex, int fractionBits, int exponentBias)
    {
        // the sign is the top bit: bit 63 of a double, bit 31 of a float
        int signBit = fractionBits == 52 ? 63 : 31;
        long sign = hex.Groups[1].Value == "-" ? 1L << signBit : 0L;
        long infinity = ((2L * exponentBias) + 1) << fractionBits;
        string integerDigits = hex.Groups[2].Value;
        string fractionDigits = hex.Groups[3].Value;
        if (integerDigits.Length + fractionDigits.Length == 0)
        {
            throw new FormatException("For input string: \"" + hex.Value + "\"");
        }
        BigInteger mantissa = BigInteger.Parse(
            "0" + integerDigits + fractionDigits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        if (mantissa.IsZero)
        {
            return sign;
        }
        // an exponent too long for a long is certainly out of range
        if (!long.TryParse(hex.Groups[4].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long exponent))
        {
            return sign | (hex.Groups[4].Value.StartsWith('-') ? 0L : infinity);
        }

        // value = mantissa * 2^binaryExponent; the leading bit has the unbiased exponent `lead`
        long binaryExponent = exponent - 4L * fractionDigits.Length;
        long bitLength = (long)mantissa.GetBitLength();
        long lead = bitLength - 1 + binaryExponent;
        long minExponent = 1 - exponentBias;
        if (lead > exponentBias + 1)
        {
            return sign | infinity;
        }
        if (lead < minExponent - fractionBits - 2)
        {
            return sign; // below half the smallest subnormal: rounds to zero
        }

        // keep fractionBits + 1 significant bits, or fewer for a subnormal result
        int precision = fractionBits + 1;
        long shift = bitLength - precision + Math.Max(0, minExponent - lead);
        BigInteger q;
        if (shift > 0)
        {
            q = mantissa >> (int)shift;
            BigInteger remainder = mantissa - (q << (int)shift);
            int half = (remainder << 1).CompareTo(BigInteger.One << (int)shift);
            if (half > 0 || (half == 0 && !q.IsEven))
            {
                q += 1;
            }
        }
        else
        {
            q = mantissa << (int)-shift;
        }
        long resultExponent = binaryExponent + shift; // value = q * 2^resultExponent
        if (q.GetBitLength() > precision)
        {
            // rounding carried into a new leading bit
            q >>= 1;
            resultExponent++;
        }

        long bits = (long)q;
        if (q.GetBitLength() < precision)
        {
            return sign | bits; // subnormal (exponent field zero) or zero
        }
        long biased = precision - 1 + resultExponent + exponentBias;
        if (biased >= 2L * exponentBias + 1)
        {
            return sign | infinity;
        }
        return sign | (biased << fractionBits) | (bits & ((1L << fractionBits) - 1));
    }

    [GeneratedRegex("^[+-]?(?:[0-9]+\\.?[0-9]*|\\.[0-9]+)(?:[eE][+-]?[0-9]+)?[fFdD]?$")]
    private static partial Regex DecimalFloatRegex();

    [GeneratedRegex("^([+-]?)0[xX]([0-9a-fA-F]*)(?:\\.([0-9a-fA-F]*))?[pP]([+-]?[0-9]+)[fFdD]?$")]
    private static partial Regex HexFloatRegex();

    /// <summary>
    /// Java's <c>String.valueOf</c> of a value, with (possibly nested) arrays rendered like
    /// <c>Arrays.deepToString</c>: <c>null</c>, booleans in lower case, floating-point numbers
    /// like <see cref="ToString(double)"/>, other formattable values culture-independently. This
    /// is the format of Flink's <c>StringUtils.arrayAwareToString</c> and of tuple fields.
    /// </summary>
    public static string DeepToString(object? value)
    {
        var builder = new StringBuilder();
        AppendDeep(value, builder, null);
        return builder.ToString();
    }

    // onPath holds the arrays currently being rendered (by reference), so that an array
    // containing itself prints the recursive reference as "[...]" like Arrays.deepToString
    // instead of recursing until the stack overflows.
    private static void AppendDeep(object? value, StringBuilder builder, HashSet<Array>? onPath)
    {
        switch (value)
        {
            case null:
                builder.Append("null");
                return;
            case Array array:
                if (onPath is not null && onPath.Contains(array))
                {
                    builder.Append("[...]");
                    return;
                }
                onPath ??= new HashSet<Array>(ReferenceEqualityComparer.Instance);
                onPath.Add(array);
                builder.Append('[');
                bool first = true;
                foreach (object? element in array)
                {
                    if (!first)
                    {
                        builder.Append(", ");
                    }
                    first = false;
                    AppendDeep(element, builder, onPath);
                }
                builder.Append(']');
                onPath.Remove(array);
                return;
            case bool b:
                builder.Append(b ? "true" : "false");
                return;
            case double d:
                builder.Append(ToString(d));
                return;
            case float f:
                builder.Append(ToString(f));
                return;
            case IFormattable formattable:
                builder.Append(formattable.ToString(null, CultureInfo.InvariantCulture));
                return;
            default:
                builder.Append(value.ToString() ?? "null");
                return;
        }
    }

    private static string SpecialToString(bool isNaN, bool isNegative, bool isInfinity) =>
        isNaN ? "NaN"
            : isInfinity ? (isNegative ? "-Infinity" : "Infinity")
            : isNegative ? "-0.0" : "0.0";

    // Splits .NET's shortest round-trip form ("R") into its significant digits (no leading or
    // trailing zeros) and the exponent of their last digit: value = digits * 10^exponent.
    private static (bool Negative, string Digits, int Exponent) ShortestDecimal(string repr)
    {
        bool negative = repr[0] == '-';
        int exponentAt = repr.IndexOf('E');
        int exponent = exponentAt < 0 ? 0 : int.Parse(repr.AsSpan(exponentAt + 1), CultureInfo.InvariantCulture);
        ReadOnlySpan<char> mantissa = repr.AsSpan(negative ? 1 : 0, (exponentAt < 0 ? repr.Length : exponentAt) - (negative ? 1 : 0));
        int point = mantissa.IndexOf('.');
        if (point >= 0)
        {
            exponent -= mantissa.Length - point - 1;
        }
        string digits = point < 0 ? mantissa.ToString() : string.Concat(mantissa[..point], mantissa[(point + 1)..]);
        string trimmedEnd = digits.TrimEnd('0');
        exponent += digits.Length - trimmedEnd.Length;
        return (negative, trimmedEnd.TrimStart('0'), exponent);
    }

    // Java's rule for a one-digit shortest decimal: the two-digit decimals that round to the
    // value compete too, and the one closest to the exact value wins (5E-324 prints as 4.9E-324).
    // The exact value is significand * 2^binaryExponent.
    private static (string Digits, int Exponent) PreferTwoDigits(
        string digits, int exponent, long significand, int binaryExponent, Func<string, bool> roundsToValue)
    {
        // the two-digit grid one decade below the digit, or two when the shortest decimal
        // rounded up across a power of ten (9.88E-324 has the one-digit form 1E-323)
        int scale = exponent - 1;
        (BigInteger floor, BigInteger k) = Quotient(significand, binaryExponent, scale);
        if (floor < 10)
        {
            scale--;
            (_, k) = Quotient(significand, binaryExponent, scale);
        }

        string candidate = k.ToString(CultureInfo.InvariantCulture);
        if (candidate.Length != 2
            || candidate[1] == '0'
            || !roundsToValue(candidate + "E" + scale.ToString(CultureInfo.InvariantCulture)))
        {
            return (digits, exponent);
        }
        return (candidate, scale);
    }

    // (significand * 2^binaryExponent) / 10^scale, truncated and rounded to the nearest integer
    // (ties to even).
    private static (BigInteger Floor, BigInteger Nearest) Quotient(long significand, int binaryExponent, int scale)
    {
        BigInteger numerator = new BigInteger(significand) << Math.Max(binaryExponent, 0);
        BigInteger denominator = BigInteger.One << Math.Max(-binaryExponent, 0);
        if (scale >= 0)
        {
            denominator *= BigInteger.Pow(10, scale);
        }
        else
        {
            numerator *= BigInteger.Pow(10, -scale);
        }
        BigInteger k = BigInteger.DivRem(numerator, denominator, out BigInteger remainder);
        int half = (remainder * 2).CompareTo(denominator);
        return (k, half > 0 || (half == 0 && !k.IsEven) ? k + 1 : k);
    }

    // Java's layout of value = digits * 10^exponent: plain for 10^-3 <= |value| < 10^7,
    // otherwise d.dddE<n>; both with at least one fractional digit.
    private static string Format(bool negative, string digits, int exponent)
    {
        var builder = new StringBuilder(digits.Length + 8);
        if (negative)
        {
            builder.Append('-');
        }
        int scientificExponent = digits.Length - 1 + exponent;
        if (scientificExponent is >= -3 and < 7)
        {
            if (scientificExponent >= 0)
            {
                int integerDigits = scientificExponent + 1;
                if (digits.Length <= integerDigits)
                {
                    builder.Append(digits).Append('0', integerDigits - digits.Length).Append(".0");
                }
                else
                {
                    builder.Append(digits, 0, integerDigits).Append('.').Append(digits, integerDigits, digits.Length - integerDigits);
                }
            }
            else
            {
                builder.Append("0.").Append('0', -scientificExponent - 1).Append(digits);
            }
        }
        else
        {
            builder.Append(digits[0]).Append('.');
            if (digits.Length > 1)
            {
                builder.Append(digits, 1, digits.Length - 1);
            }
            else
            {
                builder.Append('0');
            }
            builder.Append('E').Append(scientificExponent.ToString(CultureInfo.InvariantCulture));
        }
        return builder.ToString();
    }
}
