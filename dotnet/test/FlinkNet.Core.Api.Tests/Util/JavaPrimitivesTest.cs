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
using FlinkNet.Util;
using Xunit;

namespace FlinkNet.Tests.Util;

/// <summary>Tests for <see cref="JavaPrimitives"/>; every expected value was produced on a
/// JDK.</summary>
public class JavaPrimitivesTest
{
    [Fact]
    public void TestBooleanHashCode()
    {
        Assert.Equal(1231, JavaPrimitives.HashCode(true));
        Assert.Equal(1237, JavaPrimitives.HashCode(false));
    }

    [Fact]
    public void TestLongHashCode()
    {
        Assert.Equal(0, JavaPrimitives.HashCode(-1L));
        Assert.Equal(-2147483648, JavaPrimitives.HashCode(long.MaxValue));
        Assert.Equal(256, JavaPrimitives.HashCode(1L << 40));
    }

    [Fact]
    public void TestFloatingPointHashCode()
    {
        Assert.Equal(1069547520, JavaPrimitives.HashCode(1.5f));
        Assert.Equal(-2147483648, JavaPrimitives.HashCode(-0.0f));
        Assert.Equal(2143289344, JavaPrimitives.HashCode(float.NaN));

        Assert.Equal(1073217536, JavaPrimitives.HashCode(1.5));
        Assert.Equal(-2147483648, JavaPrimitives.HashCode(-0.0));
        Assert.Equal(0, JavaPrimitives.HashCode(0.0));
        Assert.Equal(2146959360, JavaPrimitives.HashCode(double.NaN));
    }

    [Fact]
    public void TestStringHashCode()
    {
        Assert.Equal(0, JavaPrimitives.HashCode(string.Empty));
        Assert.Equal(96354, JavaPrimitives.HashCode("abc"));
        Assert.Equal(27236, JavaPrimitives.HashCode("é中"));
        Assert.Equal(768645004, JavaPrimitives.HashCode("the quick brown fox jumps over"));
        Assert.Throws<ArgumentNullException>(() => JavaPrimitives.HashCode((string)null!));
    }

    /// <summary>Java's boxed equals and hashCode (values from a JDK).</summary>
    [Fact]
    public void TestObjectEqualsAndHashCode()
    {
        Assert.False(JavaPrimitives.ObjectEquals(-0.0, 0.0));
        Assert.True(JavaPrimitives.ObjectEquals(double.NaN, double.NaN));
        Assert.False(JavaPrimitives.ObjectEquals(-0.0f, 0.0f));
        Assert.False(JavaPrimitives.ObjectEquals(1.0, 1.0f));
        Assert.True(JavaPrimitives.ObjectEquals(null, null));
        Assert.False(JavaPrimitives.ObjectEquals("a", null));
        Assert.True(JavaPrimitives.ObjectEquals("a", "a"));

        Assert.Equal(0, JavaPrimitives.ObjectHashCode(null));
        Assert.Equal(96354, JavaPrimitives.ObjectHashCode("abc"));
        Assert.Equal(1231, JavaPrimitives.ObjectHashCode(true));
        Assert.Equal(65535, JavaPrimitives.ObjectHashCode('￿'));
        Assert.Equal(-1, JavaPrimitives.ObjectHashCode((sbyte)-1));
        Assert.Equal(1368, JavaPrimitives.ObjectHashCode((short)1368));
        Assert.Equal(256, JavaPrimitives.ObjectHashCode(1L << 40));
        Assert.Equal(1069547520, JavaPrimitives.ObjectHashCode(1.5f));
        Assert.Equal(-2147483648, JavaPrimitives.ObjectHashCode(-0.0));
    }

    /// <summary>Every NaN collapses to the canonical bit pattern.</summary>
    [Fact]
    public void TestCanonicalNaNBits()
    {
        float otherFloatNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xffc00001));
        Assert.True(float.IsNaN(otherFloatNaN));
        Assert.Equal(0x7fc00000, JavaPrimitives.FloatToIntBits(otherFloatNaN));
        Assert.Equal(BitConverter.SingleToInt32Bits(-1.5f), JavaPrimitives.FloatToIntBits(-1.5f));

        double otherDoubleNaN = BitConverter.Int64BitsToDouble(unchecked((long)0xfff8000000000001UL));
        Assert.True(double.IsNaN(otherDoubleNaN));
        Assert.Equal(0x7ff8000000000000L, JavaPrimitives.DoubleToLongBits(otherDoubleNaN));
        Assert.Equal(BitConverter.DoubleToInt64Bits(-1.5), JavaPrimitives.DoubleToLongBits(-1.5));
    }

    /// <summary>Java's <c>Double.toString</c> (JDK 21); the implementation was also checked
    /// against 300,000 JDK-formatted doubles, random bit patterns and decimal values.</summary>
    [Theory]
    [InlineData(1.0, "1.0")]
    [InlineData(100.0, "100.0")]
    [InlineData(0.5, "0.5")]
    [InlineData(-1.25, "-1.25")]
    [InlineData(0.001, "0.001")]
    [InlineData(0.002, "0.002")]
    [InlineData(1e-4, "1.0E-4")]
    [InlineData(1.23e-4, "1.23E-4")]
    [InlineData(9999999.0, "9999999.0")]
    [InlineData(1e7, "1.0E7")]
    [InlineData(12345678.9, "1.23456789E7")]
    [InlineData(1e16, "1.0E16")]
    [InlineData(1e22, "1.0E22")]
    [InlineData(1e23, "1.0E23")]
    [InlineData(2e23, "2.0E23")]
    [InlineData(1.5e300, "1.5E300")]
    [InlineData(0.1 + 0.2, "0.30000000000000004")]
    [InlineData(3.077560743296009, "3.077560743296009")]
    [InlineData(double.MaxValue, "1.7976931348623157E308")]
    [InlineData(2.2250738585072014E-308, "2.2250738585072014E-308")]
    [InlineData(0.0, "0.0")]
    [InlineData(-0.0, "-0.0")]
    [InlineData(double.NaN, "NaN")]
    [InlineData(double.PositiveInfinity, "Infinity")]
    [InlineData(double.NegativeInfinity, "-Infinity")]
    public void TestDoubleToString(double value, string expected)
    {
        Assert.Equal(expected, JavaPrimitives.ToString(value));
    }

    /// <summary>When the shortest decimal has one digit, Java lets the two-digit decimals that
    /// round to the value compete and prints the closest (.NET: 5E-324 and 1E-323).</summary>
    [Fact]
    public void TestDoubleToStringPrefersCloserTwoDigitDecimal()
    {
        Assert.Equal("4.9E-324", JavaPrimitives.ToString(double.Epsilon));
        Assert.Equal("9.9E-324", JavaPrimitives.ToString(BitConverter.Int64BitsToDouble(2)));
        Assert.Equal("1.4E-45", JavaPrimitives.ToString(float.Epsilon));
    }

    [Theory]
    [InlineData(1.0f, "1.0")]
    [InlineData(0.001f, "0.001")]
    [InlineData(1e-4f, "1.0E-4")]
    [InlineData(0.1f, "0.1")]
    [InlineData(0.3f, "0.3")]
    [InlineData(9999999.0f, "9999999.0")]
    [InlineData(1e7f, "1.0E7")]
    [InlineData(16777216f, "1.6777216E7")]
    [InlineData(1e10f, "1.0E10")]
    [InlineData(3.0775607f, "3.0775607")]
    [InlineData(float.MaxValue, "3.4028235E38")]
    [InlineData(1.1754944E-38f, "1.1754944E-38")]
    [InlineData(-0.0f, "-0.0")]
    [InlineData(float.NaN, "NaN")]
    [InlineData(float.NegativeInfinity, "-Infinity")]
    public void TestFloatToString(float value, string expected)
    {
        Assert.Equal(expected, JavaPrimitives.ToString(value));
    }

    /// <summary>Java's total order: -0.0 before 0.0 and NaN above positive infinity, unlike
    /// .NET's CompareTo, which puts NaN first and ties the zeros.</summary>
    [Fact]
    public void TestFloatingPointCompare()
    {
        Assert.Equal(-1, JavaPrimitives.Compare(-0.0, 0.0));
        Assert.Equal(1, JavaPrimitives.Compare(double.NaN, double.PositiveInfinity));
        Assert.Equal(0, JavaPrimitives.Compare(double.NaN, double.NaN));
        Assert.Equal(-1, JavaPrimitives.Compare(1.0, 2.0));

        Assert.Equal(-1, JavaPrimitives.Compare(-0.0f, 0.0f));
        Assert.Equal(1, JavaPrimitives.Compare(float.NaN, float.PositiveInfinity));
        Assert.Equal(0, JavaPrimitives.Compare(float.NaN, float.NaN));
        Assert.Equal(1, JavaPrimitives.Compare(2.0f, 1.0f));
    }
}
