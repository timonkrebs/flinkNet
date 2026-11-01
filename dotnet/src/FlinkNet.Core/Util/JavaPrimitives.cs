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

using FlinkNet.Annotations;

namespace FlinkNet.Util;

/// <summary>
/// The semantics of Java's boxed primitive types where .NET differs: <c>hashCode</c>,
/// <c>compare</c> and the canonical bit conversions.
///
/// <para>PORT NOTE: Flink hashes keys to assign them to partitions, so every process must agree
/// on a key's hash; .NET's <see cref="string.GetHashCode()"/> is randomized per process, and
/// several primitive hashes differ from Java's. The floating-point comparisons follow Java's
/// total order (-0.0 before 0.0, NaN above positive infinity), unlike
/// <see cref="double.CompareTo(double)"/>.</para>
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
}
