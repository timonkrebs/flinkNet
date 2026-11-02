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
using FlinkNet.Core.Memory;
using FlinkNet.Types;
using Xunit;

namespace FlinkNet.Tests.Types;

/// <summary>
/// Tests for the <see cref="INormalizableKey{T}"/> implementations (port of Flink's
/// <c>NormalizableKeyTest</c>), plus the byte and short values.
///
/// <para>PORT NOTE: the mutable <c>StringValue</c> class is not ported yet, so its case is left
/// out.</para>
/// </summary>
public class NormalizableKeyTest
{
    [Fact]
    public void TestIntValue()
    {
        var int0 = new IntValue(10);
        var int1 = new IntValue(10);
        var int2 = new IntValue(-10);
        var int3 = new IntValue(255);
        var int4 = new IntValue(int.MaxValue);
        var int5 = new IntValue(int.MaxValue & unchecked((int)0xff800000));
        var int6 = new IntValue(int.MinValue);
        var int7 = new IntValue(int.MinValue & unchecked((int)0xff800000));

        for (int length = 2; length <= 4; length++)
        {
            AssertNormalizableKey(int0, int1, length);
            AssertNormalizableKey(int0, int2, length);
            AssertNormalizableKey(int0, int3, length);
            AssertNormalizableKey(int0, int4, length);
            AssertNormalizableKey(int0, int5, length);
            AssertNormalizableKey(int0, int6, length);
            AssertNormalizableKey(int0, int7, length);
            AssertNormalizableKey(int4, int5, length);
            AssertNormalizableKey(int6, int7, length);
        }
    }

    [Fact]
    public void TestLongValue()
    {
        var long0 = new LongValue(10);
        var long1 = new LongValue(10);
        var long2 = new LongValue(-10);
        var long3 = new LongValue(255);
        var long4 = new LongValue(long.MaxValue);
        var long5 = new LongValue(long.MaxValue & unchecked((long)0xff80000000000000UL));
        var long6 = new LongValue(long.MinValue);
        var long7 = new LongValue(long.MinValue & unchecked((long)0xff80000000000000UL));

        for (int length = 2; length <= 8; length++)
        {
            AssertNormalizableKey(long0, long1, length);
            AssertNormalizableKey(long0, long2, length);
            AssertNormalizableKey(long0, long3, length);
            AssertNormalizableKey(long0, long4, length);
            AssertNormalizableKey(long0, long5, length);
            AssertNormalizableKey(long0, long6, length);
            AssertNormalizableKey(long0, long7, length);
            AssertNormalizableKey(long4, long5, length);
            AssertNormalizableKey(long6, long7, length);
        }
    }

    [Fact]
    public void TestPactNull()
    {
        var pn1 = new NullValue();
        var pn2 = new NullValue();
        AssertNormalizableKey(pn1, pn2, 0);
    }

    [Fact]
    public void TestPactChar()
    {
        var c1 = new CharValue((char)0);
        var c2 = new CharValue((char)1);
        var c3 = new CharValue((char)0xff);
        var c4 = new CharValue(char.MaxValue);
        var c5 = new CharValue(unchecked((char)(char.MaxValue + (char)1)));
        var c6 = new CharValue('\udbff'); // Character.MAX_HIGH_SURROGATE
        var c7 = new CharValue('\udfff'); // Character.MAX_LOW_SURROGATE
        var c8 = new CharValue('\udfff'); // Character.MAX_SURROGATE
        CharValue[] allChars = [c1, c2, c3, c4, c5, c6, c7, c8];

        for (int i = 0; i < 5; i++)
        {
            // self checks
            foreach (CharValue allChar1 in allChars)
            {
                foreach (CharValue allChar in allChars)
                {
                    AssertNormalizableKey(allChar1, allChar, i);
                }
            }
        }
    }

    /// <summary>Java's signed byte and short keys order across the sign boundary.</summary>
    [Fact]
    public void TestByteAndShortValues()
    {
        ByteValue[] bytes = [new(sbyte.MinValue), new(-1), new(0), new(1), new(sbyte.MaxValue)];
        ShortValue[] shorts = [new(short.MinValue), new(-256), new(-1), new(0), new(255), new(short.MaxValue)];
        for (int length = 0; length <= 3; length++)
        {
            foreach (ByteValue first in bytes)
            {
                foreach (ByteValue second in bytes)
                {
                    AssertNormalizableKey(first, second, length);
                }
            }
            foreach (ShortValue first in shorts)
            {
                foreach (ShortValue second in shorts)
                {
                    AssertNormalizableKey(first, second, length);
                }
            }
        }
    }

    private static void AssertNormalizableKey<T>(INormalizableKey<T> key1, INormalizableKey<T> key2, int len)
    {
        byte[] normalizedKeys = new byte[32];
        MemorySegment wrapper = MemorySegmentFactory.Wrap(normalizedKeys);

        key1.CopyNormalizedKey(wrapper, 0, len);
        key2.CopyNormalizedKey(wrapper, len, len);

        for (int i = 0; i < len; i++)
        {
            int comp;
            int normKey1 = normalizedKeys[i] & 0xFF;
            int normKey2 = normalizedKeys[len + i] & 0xFF;

            if ((comp = normKey1 - normKey2) != 0)
            {
                Assert.True(
                    Math.Sign(key1.CompareTo((T)key2)) == Math.Sign(comp),
                    "Normalized key comparison differs from actual key comparison");
                return;
            }
        }

        Assert.True(key1.CompareTo((T)key2) == 0 || key1.MaxNormalizedKeyLen > len);
    }
}
