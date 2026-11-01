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
using FlinkNet.Api.Common.TypeUtils.Base;
using FlinkNet.Core.Memory;
using Xunit;

namespace FlinkNet.Tests.Api.Common.TypeUtils.Base;

/// <summary>
/// Tests for <see cref="NormalizedKeyUtil"/>. The expected bytes were produced by the Java
/// <c>NormalizedKeyUtil</c> on a JDK, for keys shorter than, equal to and longer than the
/// type's width.
/// </summary>
public class NormalizedKeyUtilTest
{
    private delegate void PutKey(MemorySegment target, int offset, int numBytes);

    [Fact]
    public void TestByteKeys()
    {
        Assert.Equal("7f", Key((t, o, n) => NormalizedKeyUtil.PutByteNormalizedKey(-1, t, o, n), 1));
        Assert.Equal("7f0000", Key((t, o, n) => NormalizedKeyUtil.PutByteNormalizedKey(-1, t, o, n), 3));
        Assert.Equal("00", Key((t, o, n) => NormalizedKeyUtil.PutByteNormalizedKey(sbyte.MinValue, t, o, n), 1));
        Assert.Equal("ff", Key((t, o, n) => NormalizedKeyUtil.PutByteNormalizedKey(sbyte.MaxValue, t, o, n), 1));
    }

    [Fact]
    public void TestCharKeys()
    {
        Assert.Equal("9a", Key((t, o, n) => NormalizedKeyUtil.PutCharNormalizedKey('驟', t, o, n), 1));
        Assert.Equal("9a5f", Key((t, o, n) => NormalizedKeyUtil.PutCharNormalizedKey('驟', t, o, n), 2));
        Assert.Equal("9a5f00", Key((t, o, n) => NormalizedKeyUtil.PutCharNormalizedKey('驟', t, o, n), 3));
    }

    [Fact]
    public void TestBooleanKeys()
    {
        Assert.Equal("01", Key((t, o, n) => NormalizedKeyUtil.PutBooleanNormalizedKey(true, t, o, n), 1));
        Assert.Equal("010000", Key((t, o, n) => NormalizedKeyUtil.PutBooleanNormalizedKey(true, t, o, n), 3));
    }

    [Fact]
    public void TestShortKeys()
    {
        Assert.Equal("7f", Key((t, o, n) => NormalizedKeyUtil.PutShortNormalizedKey(-2, t, o, n), 1));
        Assert.Equal("7ffe", Key((t, o, n) => NormalizedKeyUtil.PutShortNormalizedKey(-2, t, o, n), 2));
        Assert.Equal("7ffe00", Key((t, o, n) => NormalizedKeyUtil.PutShortNormalizedKey(-2, t, o, n), 3));
    }

    [Fact]
    public void TestIntKeys()
    {
        Assert.Equal("7fff", Key((t, o, n) => NormalizedKeyUtil.PutIntNormalizedKey(-2, t, o, n), 2));
        Assert.Equal("7ffffffe", Key((t, o, n) => NormalizedKeyUtil.PutIntNormalizedKey(-2, t, o, n), 4));
        Assert.Equal("7ffffffe0000", Key((t, o, n) => NormalizedKeyUtil.PutIntNormalizedKey(-2, t, o, n), 6));
        Assert.Equal("00000000", Key((t, o, n) => NormalizedKeyUtil.PutIntNormalizedKey(int.MinValue, t, o, n), 4));
    }

    [Fact]
    public void TestLongKeys()
    {
        Assert.Equal("7fffff", Key((t, o, n) => NormalizedKeyUtil.PutLongNormalizedKey(-2L, t, o, n), 3));
        Assert.Equal("7ffffffffffffffe", Key((t, o, n) => NormalizedKeyUtil.PutLongNormalizedKey(-2L, t, o, n), 8));
        Assert.Equal("7ffffffffffffffe0000", Key((t, o, n) => NormalizedKeyUtil.PutLongNormalizedKey(-2L, t, o, n), 10));
    }

    [Fact]
    public void TestNonPositiveLengthWritesNothing()
    {
        Assert.Equal(string.Empty, Key((t, o, n) => NormalizedKeyUtil.PutIntNormalizedKey(7, t, o, n), 0));
        Assert.Equal(string.Empty, Key((t, o, n) => NormalizedKeyUtil.PutShortNormalizedKey(7, t, o, n), 0));
        Assert.Equal(string.Empty, Key((t, o, n) => NormalizedKeyUtil.PutBooleanNormalizedKey(true, t, o, n), 0));
    }

    // Writes the key at offset 1 between sentinel bytes, checks that exactly numBytes were
    // written, and returns them as lower-case hex
    private static string Key(PutKey put, int numBytes)
    {
        byte[] buffer = new byte[numBytes + 2];
        Array.Fill(buffer, (byte)0xee);
        put(MemorySegmentFactory.Wrap(buffer), 1, numBytes);
        Assert.Equal(0xee, buffer[0]);
        Assert.Equal(0xee, buffer[^1]);
        return Convert.ToHexString(buffer, 1, numBytes).ToLowerInvariant();
    }
}
