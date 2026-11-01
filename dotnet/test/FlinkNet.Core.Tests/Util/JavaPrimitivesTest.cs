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
