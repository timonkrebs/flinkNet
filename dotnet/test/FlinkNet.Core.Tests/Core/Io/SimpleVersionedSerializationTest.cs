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

using System.Text;
using FlinkNet.Core.Io;
using FlinkNet.Core.Memory;
using Xunit;

namespace FlinkNet.Tests.Core.Io;

/// <summary>Tests for the <see cref="SimpleVersionedSerialization"/> class.</summary>
public class SimpleVersionedSerializationTest
{
    [Fact]
    public void TestSerializationRoundTrip()
    {
        ISimpleVersionedSerializer<string> utfEncoder = new TestStringSerializer();

        const string testString = "dugfakgs";
        var output = new DataOutputSerializer(32);
        SimpleVersionedSerialization.WriteVersionAndSerialize(utfEncoder, testString, output);
        byte[] outBytes = output.GetCopyOfBuffer();

        byte[] bytes = SimpleVersionedSerialization.WriteVersionAndSerialize(utfEncoder, testString);
        Assert.Equal<byte[]>(bytes, outBytes);

        var input = new DataInputDeserializer(bytes);
        string deserialized = SimpleVersionedSerialization.ReadVersionAndDeSerialize(utfEncoder, input);
        string deserializedFromBytes =
            SimpleVersionedSerialization.ReadVersionAndDeSerialize(utfEncoder, outBytes);
        Assert.Equal(testString, deserialized);
        Assert.Equal(testString, deserializedFromBytes);
    }

    [Fact]
    public void TestSerializeEmpty()
    {
        const string testString = "beeeep!";

        ISimpleVersionedSerializer<string> emptySerializer = new EmptySerializer(testString);

        var output = new DataOutputSerializer(32);
        SimpleVersionedSerialization.WriteVersionAndSerialize(emptySerializer, "abc", output);
        byte[] outBytes = output.GetCopyOfBuffer();

        byte[] bytes = SimpleVersionedSerialization.WriteVersionAndSerialize(emptySerializer, "abc");
        Assert.Equal<byte[]>(bytes, outBytes);

        var input = new DataInputDeserializer(bytes);
        string deserialized =
            SimpleVersionedSerialization.ReadVersionAndDeSerialize(emptySerializer, input);
        string deserializedFromBytes =
            SimpleVersionedSerialization.ReadVersionAndDeSerialize(emptySerializer, outBytes);
        Assert.Equal(testString, deserialized);
        Assert.Equal(testString, deserializedFromBytes);
    }

    [Fact]
    public void TestListSerializationRoundTrip()
    {
        ISimpleVersionedSerializer<string> utfEncoder = new TestStringSerializer();
        var datums = new List<string> { "beeep!", "beep!!!" };

        var output = new DataOutputSerializer(32);
        SimpleVersionedSerialization.WriteVersionAndSerializeList(utfEncoder, datums, output);
        byte[] outBytes = output.GetCopyOfBuffer();

        var input = new DataInputDeserializer(outBytes);
        List<string> deserialized =
            SimpleVersionedSerialization.ReadVersionAndDeserializeList(utfEncoder, input);
        Assert.Equal(datums, deserialized);
    }

    [Fact]
    public void TestUnderflow()
    {
        Assert.Throws<ArgumentException>(
            () =>
                SimpleVersionedSerialization.ReadVersionAndDeSerialize(
                    new TestStringSerializer(), new byte[7]));
    }

    /// <summary>PORT NOTE: extra pin (not in the Java test) for the header validation that
    /// rejects byte arrays whose length does not match the encoded payload length.</summary>
    [Fact]
    public void TestLengthMismatchRejected()
    {
        ISimpleVersionedSerializer<string> utfEncoder = new TestStringSerializer();
        byte[] bytes = SimpleVersionedSerialization.WriteVersionAndSerialize(utfEncoder, "hello");

        byte[] truncated = bytes.AsSpan(0, bytes.Length - 1).ToArray();
        Assert.Throws<ArgumentException>(
            () => SimpleVersionedSerialization.ReadVersionAndDeSerialize(utfEncoder, truncated));
    }

    /// <summary>PORT NOTE: extra pin (not in the Java test) for the exact byte layout of the
    /// version-and-length header: 4 bytes version (big endian), 4 bytes length (big endian).</summary>
    [Fact]
    public void TestHeaderLayout()
    {
        ISimpleVersionedSerializer<string> utfEncoder = new TestStringSerializer();
        byte[] bytes = SimpleVersionedSerialization.WriteVersionAndSerialize(utfEncoder, "AB");

        // version = int.MaxValue / 2 = 0x3FFFFFFF, length = 2, payload = "AB"
        Assert.Equal<byte[]>(
            [0x3F, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x02, (byte)'A', (byte)'B'], bytes);
    }

    // ------------------------------------------------------------------------

    private sealed class TestStringSerializer : ISimpleVersionedSerializer<string>
    {
        // version should occupy many bytes
        private const int SerializerVersion = int.MaxValue / 2;

        public int Version => SerializerVersion;

        public byte[] Serialize(string str) => Encoding.UTF8.GetBytes(str);

        public string Deserialize(int version, byte[] serialized)
        {
            Assert.Equal(SerializerVersion, version);
            return Encoding.UTF8.GetString(serialized);
        }
    }

    private sealed class EmptySerializer(string result) : ISimpleVersionedSerializer<string>
    {
        public int Version => 42;

        public byte[] Serialize(string obj) => [];

        public string Deserialize(int version, byte[] serialized)
        {
            Assert.Equal(42, version);
            Assert.Empty(serialized);
            return result;
        }
    }
}
