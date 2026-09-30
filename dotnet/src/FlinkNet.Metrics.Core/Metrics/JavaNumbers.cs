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

using System.Numerics;

namespace FlinkNet.Metrics;

/// <summary>
/// The conversions of Java's <c>Number.intValue()</c> and <c>Number.longValue()</c> for CLR
/// numbers. They differ from <see cref="IConvertible"/>, which rounds floating values and throws
/// on overflow.
/// </summary>
internal static class JavaNumbers
{
    /// <summary>Whether the value is a number (Java: <c>instanceof Number</c>).</summary>
    public static bool IsNumber(object value) =>
        value is long or int or short or byte or sbyte or ushort or uint or ulong
            or float or double or decimal or BigInteger;

    /// <summary>
    /// Java's <c>longValue()</c>: floating values truncate toward zero and saturate (NaN to 0);
    /// decimals truncate exactly and keep the low-order 64 bits, like <c>BigDecimal</c>; integral
    /// values widen, or keep their low-order 64 bits, like <c>BigInteger</c>.
    /// </summary>
    /// <exception cref="ArgumentException">If the value is not a number.</exception>
    public static long ToLong(object value) => value switch
    {
        long l => l,
        int i => i,
        short s => s,
        byte b => b,
        sbyte sb => sb,
        ushort us => us,
        uint ui => ui,
        ulong ul => unchecked((long)ul),
        float f => SaturatingToLong(f),
        double d => SaturatingToLong(d),
        decimal m => LowOrder64Bits(new BigInteger(decimal.Truncate(m))),
        BigInteger bi => LowOrder64Bits(bi),
        _ => throw new ArgumentException("Not a number: " + value),
    };

    /// <summary>
    /// Java's <c>intValue()</c>: floating values truncate toward zero and saturate to the int
    /// range (NaN to 0); all other numbers keep the low-order 32 bits of their
    /// <see cref="ToLong"/> value.
    /// </summary>
    /// <exception cref="ArgumentException">If the value is not a number.</exception>
    public static int ToInt(object value) => value switch
    {
        float f => SaturatingToInt(f),
        double d => SaturatingToInt(d),
        _ => unchecked((int)ToLong(value)),
    };

    private static long SaturatingToLong(double d) =>
        double.IsNaN(d) ? 0
            : d >= long.MaxValue ? long.MaxValue
            : d <= long.MinValue ? long.MinValue
            : (long)d;

    private static int SaturatingToInt(double d) =>
        double.IsNaN(d) ? 0
            : d >= int.MaxValue ? int.MaxValue
            : d <= int.MinValue ? int.MinValue
            : (int)d;

    private static long LowOrder64Bits(BigInteger value) =>
        unchecked((long)(ulong)(value & ulong.MaxValue));
}
