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
public static class JavaPrimitives
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
