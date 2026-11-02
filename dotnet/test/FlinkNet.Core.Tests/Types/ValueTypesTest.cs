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

/// <summary>Tests for the primitive <see cref="IValue"/> types.</summary>
public class ValueTypesTest
{
    private static T WriteAndRead<T>(T value)
        where T : IValue, new()
    {
        var output = new DataOutputSerializer(32);
        value.Write(output);
        var read = new T();
        read.Read(new DataInputDeserializer(output.GetCopyOfBuffer()));
        return read;
    }

    [Fact]
    public void TestIntValue()
    {
        var value = new IntValue(42);
        Assert.Equal(value, WriteAndRead(value));
        Assert.Equal(42, WriteAndRead(value).GetValue());
        Assert.Equal("42", value.ToString());
        Assert.Equal(4, value.BinaryLength);

        Assert.True(new IntValue(1).CompareTo(new IntValue(2)) < 0);
        Assert.True(new IntValue(2).CompareTo(new IntValue(1)) > 0);
        Assert.Equal(0, new IntValue(7).CompareTo(new IntValue(7)));

        var target = new IntValue();
        value.CopyTo(target);
        Assert.Equal(value, target);
        Assert.Equal(value, value.Copy());

        var reset = new IntValue();
        reset.SetValue(new IntValue(9));
        Assert.Equal(9, reset.GetValue());
    }

    [Fact]
    public void TestLongValue()
    {
        var value = new LongValue(478236947162389746L);
        Assert.Equal(value, WriteAndRead(value));
        Assert.Equal(8, value.BinaryLength);
        Assert.True(new LongValue(long.MinValue).CompareTo(new LongValue(long.MaxValue)) < 0);

        // Java: 43 + (int) (value ^ value >>> 32)
        Assert.Equal(43, new LongValue(0).GetHashCode());
        const long v = 478236947162389746L;
        Assert.Equal(unchecked(43 + (int)(v ^ (v >>> 32))), value.GetHashCode());
    }

    [Fact]
    public void TestBooleanValue()
    {
        var value = new BooleanValue(true);
        Assert.Equal(value, WriteAndRead(value));
        Assert.Equal("true", value.ToString());
        Assert.True(BooleanValue.False.CompareTo(BooleanValue.True) < 0);
    }

    [Fact]
    public void TestNullValue()
    {
        NullValue value = NullValue.GetInstance();
        Assert.Equal(value, WriteAndRead(value));
        Assert.Equal(0, value.CompareTo(new NullValue()));
        Assert.Equal("(null)", value.ToString());
        Assert.Same(NullValue.GetInstance(), value.Copy());

        // the wire format is a single (false) boolean
        var output = new DataOutputSerializer(4);
        value.Write(output);
        Assert.Equal<byte[]>([0], output.GetCopyOfBuffer());
    }

    /// <summary>Java's byte is signed: ByteValue holds an sbyte, writes one byte and reads
    /// 0xff back as -1.</summary>
    [Fact]
    public void TestByteValue()
    {
        var value = new ByteValue(-5);
        Assert.Equal(value, WriteAndRead(value));
        Assert.Equal("-5", value.ToString());
        Assert.Equal(-5, value.GetHashCode());
        Assert.Equal(1, value.BinaryLength);
        Assert.True(new ByteValue(-1).CompareTo(new ByteValue(1)) < 0);

        var input = new DataInputDeserializer([0xff]);
        var read = new ByteValue();
        read.Read(input);
        Assert.Equal(-1, read.GetValue());
    }

    [Fact]
    public void TestShortAndCharValues()
    {
        var shortValue = new ShortValue(-1368);
        Assert.Equal(shortValue, WriteAndRead(shortValue));
        Assert.Equal("-1368", shortValue.ToString());
        Assert.Equal(-1368, shortValue.GetHashCode());
        Assert.Equal(2, shortValue.BinaryLength);

        var charValue = new CharValue('\u9a5f');
        Assert.Equal(charValue, WriteAndRead(charValue));
        Assert.Equal("\u9a5f", charValue.ToString());
        Assert.Equal(0x9a5f, charValue.GetHashCode());
        Assert.True(new CharValue('a').CompareTo(new CharValue('\uffff')) < 0);
    }

    /// <summary>As in Java: CompareTo uses the IEEE operators, Equals and GetHashCode the
    /// canonical bits, and ToString prints like Double/Float.toString. (Equals is called
    /// directly: xUnit's equality assertions prefer IComparable.)</summary>
    [Fact]
    public void TestFloatingPointValues()
    {
        var doubleValue = new DoubleValue(1.5);
        Assert.Equal(doubleValue, WriteAndRead(doubleValue));
        Assert.Equal(8, doubleValue.BinaryLength);
        // Java: 31 + (int) (bits ^ bits >>> 32)
        Assert.Equal(31 + 1073217536, doubleValue.GetHashCode());
        Assert.Equal("1.0", new DoubleValue(1.0).ToString());
        Assert.Equal("1.0E10", new DoubleValue(1e10).ToString());
        Assert.Equal("NaN", new DoubleValue(double.NaN).ToString());

        Assert.Equal(0, new DoubleValue(-0.0).CompareTo(new DoubleValue(0.0)));
        Assert.False(new DoubleValue(-0.0).Equals(new DoubleValue(0.0)));
        Assert.Equal(0, new DoubleValue(double.NaN).CompareTo(new DoubleValue(1.0)));
        Assert.True(new DoubleValue(double.NaN).Equals(new DoubleValue(double.NaN)));
        Assert.Equal(
            new DoubleValue(double.NaN).GetHashCode(),
            new DoubleValue(BitConverter.Int64BitsToDouble(unchecked((long)0xfff8000000000001UL))).GetHashCode());

        var floatValue = new FloatValue(1.5f);
        Assert.Equal(floatValue, WriteAndRead(floatValue));
        Assert.Equal(4, floatValue.BinaryLength);
        Assert.Equal(1069547520, floatValue.GetHashCode());
        Assert.Equal("3.0775607", new FloatValue(3.0775607f).ToString());
        Assert.Equal("1.0E-4", new FloatValue(1e-4f).ToString());
        Assert.Equal(0, new FloatValue(-0.0f).CompareTo(new FloatValue(0.0f)));
        Assert.False(new FloatValue(-0.0f).Equals(new FloatValue(0.0f)));
        Assert.True(new FloatValue(float.NaN).Equals(new FloatValue(float.NaN)));
    }

    /// <summary>Byte-wise unsigned comparison of full normalized keys must order like
    /// CompareTo, including across the sign boundary.</summary>
    [Fact]
    public void TestNormalizedKeyOrdering()
    {
        AssertNormalizedOrder(new IntValue(int.MinValue), new IntValue(-1), 4);
        AssertNormalizedOrder(new IntValue(-1), new IntValue(0), 4);
        AssertNormalizedOrder(new IntValue(0), new IntValue(int.MaxValue), 4);

        AssertNormalizedOrder(new LongValue(long.MinValue), new LongValue(-1), 8);
        AssertNormalizedOrder(new LongValue(-1), new LongValue(0), 8);
        AssertNormalizedOrder(new LongValue(0), new LongValue(long.MaxValue), 8);

        AssertNormalizedOrder(new BooleanValue(false), new BooleanValue(true), 1);
    }

    /// <summary>A prefix-length normalized key keeps the most significant bytes.</summary>
    [Fact]
    public void TestPartialNormalizedKey()
    {
        MemorySegment full = MemorySegmentFactory.AllocateUnpooledSegment(4);
        MemorySegment partial = MemorySegmentFactory.AllocateUnpooledSegment(2);

        var value = new IntValue(0x12345678);
        value.CopyNormalizedKey(full, 0, 4);
        value.CopyNormalizedKey(partial, 0, 2);

        Assert.Equal(full.Get(0), partial.Get(0));
        Assert.Equal(full.Get(1), partial.Get(1));
    }

    private static void AssertNormalizedOrder<T>(T smaller, T larger, int keyLen)
        where T : INormalizableKey<T>
    {
        MemorySegment segment = MemorySegmentFactory.AllocateUnpooledSegment(2 * keyLen);
        smaller.CopyNormalizedKey(segment, 0, keyLen);
        larger.CopyNormalizedKey(segment, keyLen, keyLen);

        for (int i = 0; i < keyLen; i++)
        {
            int smallerByte = segment.Get(i) & 0xff;
            int largerByte = segment.Get(keyLen + i) & 0xff;
            if (smallerByte != largerByte)
            {
                Assert.True(
                    smallerByte < largerByte,
                    $"Normalized key ordering violated at byte {i}: {smallerByte} vs {largerByte}");
                return;
            }
        }

        Assert.Fail("Normalized keys are identical for different values.");
    }
}
