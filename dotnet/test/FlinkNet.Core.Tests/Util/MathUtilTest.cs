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

/// <summary>Tests for <see cref="MathUtils"/>.</summary>
public class MathUtilTest
{
    [Fact]
    public void TestLog2Computation()
    {
        Assert.Equal(0, MathUtils.Log2Floor(1));
        Assert.Equal(1, MathUtils.Log2Floor(2));
        Assert.Equal(1, MathUtils.Log2Floor(3));
        Assert.Equal(2, MathUtils.Log2Floor(4));
        Assert.Equal(2, MathUtils.Log2Floor(5));
        Assert.Equal(2, MathUtils.Log2Floor(7));
        Assert.Equal(3, MathUtils.Log2Floor(8));
        Assert.Equal(3, MathUtils.Log2Floor(9));
        Assert.Equal(4, MathUtils.Log2Floor(16));
        Assert.Equal(4, MathUtils.Log2Floor(17));
        Assert.Equal(13, MathUtils.Log2Floor((0x1 << 13) + 1));
        Assert.Equal(30, MathUtils.Log2Floor(int.MaxValue));
        Assert.Equal(31, MathUtils.Log2Floor(-1));

        Assert.Throws<ArithmeticException>(() => MathUtils.Log2Floor(0));
    }

    [Fact]
    public void TestRoundDownToPowerOf2()
    {
        Assert.Equal(0, MathUtils.RoundDownToPowerOf2(0));
        Assert.Equal(1, MathUtils.RoundDownToPowerOf2(1));
        Assert.Equal(2, MathUtils.RoundDownToPowerOf2(2));
        Assert.Equal(2, MathUtils.RoundDownToPowerOf2(3));
        Assert.Equal(4, MathUtils.RoundDownToPowerOf2(4));
        Assert.Equal(4, MathUtils.RoundDownToPowerOf2(5));
        Assert.Equal(4, MathUtils.RoundDownToPowerOf2(6));
        Assert.Equal(4, MathUtils.RoundDownToPowerOf2(7));
        Assert.Equal(8, MathUtils.RoundDownToPowerOf2(8));
        Assert.Equal(8, MathUtils.RoundDownToPowerOf2(9));
        Assert.Equal(8, MathUtils.RoundDownToPowerOf2(15));
        Assert.Equal(16, MathUtils.RoundDownToPowerOf2(16));
        Assert.Equal(16, MathUtils.RoundDownToPowerOf2(17));
        Assert.Equal(16, MathUtils.RoundDownToPowerOf2(31));
        Assert.Equal(32, MathUtils.RoundDownToPowerOf2(32));
        Assert.Equal(32, MathUtils.RoundDownToPowerOf2(33));
        Assert.Equal(32, MathUtils.RoundDownToPowerOf2(42));
        Assert.Equal(32, MathUtils.RoundDownToPowerOf2(63));
        Assert.Equal(64, MathUtils.RoundDownToPowerOf2(64));
        Assert.Equal(64, MathUtils.RoundDownToPowerOf2(125));
        Assert.Equal(16384, MathUtils.RoundDownToPowerOf2(25654));
        Assert.Equal(33554432, MathUtils.RoundDownToPowerOf2(34366363));
        Assert.Equal(33554432, MathUtils.RoundDownToPowerOf2(63463463));
        Assert.Equal(1073741824, MathUtils.RoundDownToPowerOf2(1852987883));
        Assert.Equal(1073741824, MathUtils.RoundDownToPowerOf2(int.MaxValue));
    }

    [Fact]
    public void TestRoundUpToPowerOf2()
    {
        Assert.Equal(0, MathUtils.RoundUpToPowerOfTwo(0));
        Assert.Equal(1, MathUtils.RoundUpToPowerOfTwo(1));
        Assert.Equal(2, MathUtils.RoundUpToPowerOfTwo(2));
        Assert.Equal(4, MathUtils.RoundUpToPowerOfTwo(3));
        Assert.Equal(4, MathUtils.RoundUpToPowerOfTwo(4));
        Assert.Equal(8, MathUtils.RoundUpToPowerOfTwo(5));
        Assert.Equal(8, MathUtils.RoundUpToPowerOfTwo(6));
        Assert.Equal(8, MathUtils.RoundUpToPowerOfTwo(7));
        Assert.Equal(8, MathUtils.RoundUpToPowerOfTwo(8));
        Assert.Equal(16, MathUtils.RoundUpToPowerOfTwo(9));
        Assert.Equal(16, MathUtils.RoundUpToPowerOfTwo(15));
        Assert.Equal(16, MathUtils.RoundUpToPowerOfTwo(16));
        Assert.Equal(32, MathUtils.RoundUpToPowerOfTwo(17));
        Assert.Equal(32, MathUtils.RoundUpToPowerOfTwo(31));
        Assert.Equal(32, MathUtils.RoundUpToPowerOfTwo(32));
        Assert.Equal(64, MathUtils.RoundUpToPowerOfTwo(33));
        Assert.Equal(64, MathUtils.RoundUpToPowerOfTwo(42));
        Assert.Equal(64, MathUtils.RoundUpToPowerOfTwo(63));
        Assert.Equal(64, MathUtils.RoundUpToPowerOfTwo(64));
        Assert.Equal(128, MathUtils.RoundUpToPowerOfTwo(125));
        Assert.Equal(32768, MathUtils.RoundUpToPowerOfTwo(25654));
        Assert.Equal(67108864, MathUtils.RoundUpToPowerOfTwo(34366363));
        Assert.Equal(67108864, MathUtils.RoundUpToPowerOfTwo(67108863));
        Assert.Equal(67108864, MathUtils.RoundUpToPowerOfTwo(67108864));
        Assert.Equal(0x40000000, MathUtils.RoundUpToPowerOfTwo(0x3FFFFFFE));
        Assert.Equal(0x40000000, MathUtils.RoundUpToPowerOfTwo(0x3FFFFFFF));
        Assert.Equal(0x40000000, MathUtils.RoundUpToPowerOfTwo(0x40000000));
    }

    [Fact]
    public void TestPowerOfTwo()
    {
        Assert.True(MathUtils.IsPowerOf2(1));
        Assert.True(MathUtils.IsPowerOf2(2));
        Assert.True(MathUtils.IsPowerOf2(4));
        Assert.True(MathUtils.IsPowerOf2(8));
        Assert.True(MathUtils.IsPowerOf2(32768));
        Assert.True(MathUtils.IsPowerOf2(65536));
        Assert.True(MathUtils.IsPowerOf2(1 << 30));
        Assert.True(MathUtils.IsPowerOf2(1L + int.MaxValue));
        Assert.True(MathUtils.IsPowerOf2(1L << 41));
        Assert.True(MathUtils.IsPowerOf2(1L << 62));

        Assert.False(MathUtils.IsPowerOf2(3));
        Assert.False(MathUtils.IsPowerOf2(5));
        Assert.False(MathUtils.IsPowerOf2(567923));
        Assert.False(MathUtils.IsPowerOf2(int.MaxValue));
        Assert.False(MathUtils.IsPowerOf2(long.MaxValue));
    }

    [Fact]
    public void TestFlipSignBit()
    {
        Assert.Equal(0L, MathUtils.FlipSignBit(long.MinValue));
        Assert.Equal(long.MinValue, MathUtils.FlipSignBit(0L));
        Assert.Equal(-1L, MathUtils.FlipSignBit(long.MaxValue));
        Assert.Equal(long.MaxValue, MathUtils.FlipSignBit(-1L));
        Assert.Equal(42L | long.MinValue, MathUtils.FlipSignBit(42L));
        Assert.Equal(-42L & long.MaxValue, MathUtils.FlipSignBit(-42L));
    }

    [Fact]
    public void TestDivideRoundUp()
    {
        Assert.Equal(0, MathUtils.DivideRoundUp(0, 1));
        Assert.Equal(0, MathUtils.DivideRoundUp(0, 2));
        Assert.Equal(1, MathUtils.DivideRoundUp(1, 1));
        Assert.Equal(1, MathUtils.DivideRoundUp(1, 2));
        Assert.Equal(2, MathUtils.DivideRoundUp(2, 1));
        Assert.Equal(1, MathUtils.DivideRoundUp(2, 2));
        Assert.Equal(1, MathUtils.DivideRoundUp(2, 3));
    }

    [Fact]
    public void TestDivideRoundUpNegativeDividend()
    {
        Assert.Throws<ArgumentException>(() => MathUtils.DivideRoundUp(-1, 1));
    }

    [Fact]
    public void TestDivideRoundUpNegativeDivisor()
    {
        Assert.Throws<ArgumentException>(() => MathUtils.DivideRoundUp(1, -1));
    }

    [Fact]
    public void TestDivideRoundUpZeroDivisor()
    {
        Assert.Throws<ArgumentException>(() => MathUtils.DivideRoundUp(1, 0));
    }
}
