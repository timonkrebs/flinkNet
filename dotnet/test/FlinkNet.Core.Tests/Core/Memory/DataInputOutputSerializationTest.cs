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
using Xunit;

namespace FlinkNet.Tests.Core.Memory;

/// <summary>
/// Round-trip tests for <see cref="DataOutputSerializer"/>, <see cref="DataInputDeserializer"/>
/// and the stream wrappers, pinning the Java DataOutput big-endian wire format.
///
/// <para>PORT NOTE: replaces Java's <c>DataInputOutputSerializerTest</c>, which builds on the
/// not-yet-ported testutils serialization type zoo.</para>
/// </summary>
public class DataInputOutputSerializationTest
{
    [Fact]
    public void TestWireFormatIsBigEndian()
    {
        var output = new DataOutputSerializer(16);
        output.WriteInt(0x01020304);
        output.WriteShort(0x0506);
        output.WriteLong(0x0708090A0B0C0D0EL);
        output.WriteChar('ሴ');

        Assert.Equal(
            new byte[]
            {
                0x01, 0x02, 0x03, 0x04,
                0x05, 0x06,
                0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E,
                0x12, 0x34,
            },
            output.GetCopyOfBuffer());
    }

    [Fact]
    public void TestPrimitiveRoundTrip()
    {
        var rnd = new Random(42);
        var output = new DataOutputSerializer(1);

        var bools = new bool[100];
        var bytes = new byte[100];
        var shorts = new short[100];
        var chars = new char[100];
        var ints = new int[100];
        var longs = new long[100];
        var floats = new float[100];
        var doubles = new double[100];

        for (int i = 0; i < 100; i++)
        {
            bools[i] = rnd.Next(2) == 0;
            bytes[i] = (byte)rnd.Next(256);
            shorts[i] = (short)rnd.Next(short.MinValue, short.MaxValue + 1);
            chars[i] = (char)rnd.Next(char.MaxValue + 1);
            ints[i] = rnd.Next(int.MinValue, int.MaxValue);
            longs[i] = rnd.NextInt64(long.MinValue, long.MaxValue);
            floats[i] = (float)rnd.NextDouble();
            doubles[i] = rnd.NextDouble();

            output.WriteBoolean(bools[i]);
            output.WriteByte(bytes[i]);
            output.WriteShort(shorts[i]);
            output.WriteChar(chars[i]);
            output.WriteInt(ints[i]);
            output.WriteLong(longs[i]);
            output.WriteFloat(floats[i]);
            output.WriteDouble(doubles[i]);
        }

        var input = new DataInputDeserializer(output.GetSharedBuffer(), 0, output.Length);
        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(bools[i], input.ReadBoolean());
            Assert.Equal(bytes[i], input.ReadByte());
            Assert.Equal(shorts[i], input.ReadShort());
            Assert.Equal(chars[i], input.ReadChar());
            Assert.Equal(ints[i], input.ReadInt());
            Assert.Equal(longs[i], input.ReadLong());
            Assert.Equal(floats[i], input.ReadFloat());
            Assert.Equal(doubles[i], input.ReadDouble());
        }
        Assert.Equal(0, input.Available);
    }

    [Fact]
    public void TestUtfRoundTrip()
    {
        string[] strings =
        [
            "",
            "abc",
            "Flink",
            "äöü ßẞ",                 // 2-byte sequences
            "߿ࠀ￿", // boundary code points incl. 3-byte
            "中文字符串",               // 3-byte sequences
            "mixed ascii und 中文 plus äöü",
        ];

        var output = new DataOutputSerializer(4);
        foreach (string s in strings)
        {
            output.WriteUTF(s);
            output.WriteLongUTF(s);
        }

        var input = new DataInputDeserializer(output.GetCopyOfBuffer());
        foreach (string s in strings)
        {
            Assert.Equal(s, input.ReadUTF());
            Assert.Equal(s, input.ReadLongUTF());
        }
        Assert.Equal(0, input.Available);
    }

    [Fact]
    public void TestUtfUsesJavaModifiedUtf8Lengths()
    {
        // U+0000 encodes as the two-byte sequence 0xC0 0x80 in modified UTF-8
        var output = new DataOutputSerializer(8);
        output.WriteUTF("\0");
        Assert.Equal(new byte[] { 0x00, 0x02, 0xC0, 0x80 }, output.GetCopyOfBuffer());

        var input = new DataInputDeserializer(output.GetCopyOfBuffer());
        Assert.Equal("\0", input.ReadUTF());
    }

    [Fact]
    public void TestBufferGrowsAndSetPosition()
    {
        var output = new DataOutputSerializer(1);
        for (int i = 0; i < 1000; i++)
        {
            output.WriteLong(i);
        }
        Assert.Equal(8000, output.Length);

        output.WriteIntUnsafe(0x7F7F7F7F, 0);
        var input = new DataInputDeserializer(output.GetSharedBuffer(), 0, output.Length);
        Assert.Equal(0x7F7F7F7F, input.ReadInt());

        output.SetPosition(8);
        Assert.Equal(8, output.Length);
        Assert.Throws<ArgumentException>(() => output.SetPosition(9));

        output.Clear();
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void TestCopyBetweenViews()
    {
        var source = new DataOutputSerializer(16);
        source.WriteLong(0x1122334455667788L);
        source.WriteInt(0x4CD55DE6);

        var target = new DataOutputSerializer(16);
        target.SkipBytesToWrite(4);
        target.Write(new DataInputDeserializer(source.GetCopyOfBuffer()), 12);

        var input = new DataInputDeserializer(target.GetCopyOfBuffer());
        input.SkipBytesToRead(4);
        Assert.Equal(0x1122334455667788L, input.ReadLong());
        Assert.Equal(0x4CD55DE6, input.ReadInt());
    }

    [Fact]
    public void TestStreamWrappersRoundTrip()
    {
        var stream = new MemoryStream();
        var output = new DataOutputViewStreamWrapper(stream);
        output.WriteBoolean(true);
        output.WriteByte(0xFE);
        output.WriteShort(-12345);
        output.WriteChar('λ');
        output.WriteInt(int.MinValue);
        output.WriteLong(long.MaxValue);
        output.WriteFloat(3.14f);
        output.WriteDouble(Math.E);
        output.WriteUTF("Flink auf .NET — 流处理");

        // the stream wrapper and the byte-array serializer must produce identical bytes
        var serializer = new DataOutputSerializer(64);
        serializer.WriteBoolean(true);
        serializer.WriteByte(0xFE);
        serializer.WriteShort(-12345);
        serializer.WriteChar('λ');
        serializer.WriteInt(int.MinValue);
        serializer.WriteLong(long.MaxValue);
        serializer.WriteFloat(3.14f);
        serializer.WriteDouble(Math.E);
        serializer.WriteUTF("Flink auf .NET — 流处理");
        Assert.Equal(serializer.GetCopyOfBuffer(), stream.ToArray());

        stream.Position = 0;
        var input = new DataInputViewStreamWrapper(stream);
        Assert.True(input.ReadBoolean());
        Assert.Equal(0xFE, input.ReadUnsignedByte());
        Assert.Equal(-12345, input.ReadShort());
        Assert.Equal('λ', input.ReadChar());
        Assert.Equal(int.MinValue, input.ReadInt());
        Assert.Equal(long.MaxValue, input.ReadLong());
        Assert.Equal(3.14f, input.ReadFloat());
        Assert.Equal(Math.E, input.ReadDouble());
        Assert.Equal("Flink auf .NET — 流处理", input.ReadUTF());
    }
}
