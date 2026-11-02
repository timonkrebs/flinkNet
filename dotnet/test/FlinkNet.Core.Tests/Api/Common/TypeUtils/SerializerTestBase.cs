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

using FlinkNet.Api.Common.TypeUtils;
using FlinkNet.Core.Memory;
using Xunit;

namespace FlinkNet.Tests.Api.Common.TypeUtils;

/// <summary>
/// Abstract test base for serializers (port of Flink's <c>SerializerTestBase</c>, without the
/// deferred snapshot/compatibility tests).
/// </summary>
public abstract class SerializerTestBase<T>
{
    protected abstract TypeSerializer<T> CreateSerializer();

    protected abstract int ExpectedLength { get; }

    protected abstract T[] GetTestData();

    protected virtual bool DeepEquals(T expected, T actual) =>
        EqualityComparer<T>.Default.Equals(expected, actual);

    // --------------------------------------------------------------------------------------------

    [Fact]
    public void TestInstantiate()
    {
        TypeSerializer<T> serializer = CreateSerializer();
        T instance = serializer.CreateInstance();
        if (default(T) is null)
        {
            Assert.NotNull(instance);
        }
    }

    [Fact]
    public void TestGetLength()
    {
        Assert.Equal(ExpectedLength, CreateSerializer().Length);
    }

    [Fact]
    public void TestCopy()
    {
        TypeSerializer<T> serializer = CreateSerializer();
        foreach (T datum in GetTestData())
        {
            T copy = serializer.Copy(datum);
            Assert.True(DeepEquals(datum, copy), $"Copied element does not equal the original: {datum}");
        }
    }

    [Fact]
    public void TestCopyWithReuse()
    {
        TypeSerializer<T> serializer = CreateSerializer();
        T reuse = serializer.CreateInstance();
        foreach (T datum in GetTestData())
        {
            T copy = serializer.Copy(datum, reuse);
            Assert.True(DeepEquals(datum, copy), $"Copied element does not equal the original: {datum}");
            reuse = copy;
        }
    }

    [Fact]
    public void TestSerializeIndividually()
    {
        TypeSerializer<T> serializer = CreateSerializer();
        foreach (T datum in GetTestData())
        {
            var output = new DataOutputSerializer(32);
            serializer.Serialize(datum, output);
            var input = new DataInputDeserializer(output.GetCopyOfBuffer());
            T deserialized = serializer.Deserialize(input);
            Assert.True(DeepEquals(datum, deserialized), $"Round trip failed for: {datum}");
            Assert.Equal(0, input.Available);
        }
    }

    [Fact]
    public void TestSerializeIndividuallyWithReuse()
    {
        TypeSerializer<T> serializer = CreateSerializer();
        T reuse = serializer.CreateInstance();
        foreach (T datum in GetTestData())
        {
            var output = new DataOutputSerializer(32);
            serializer.Serialize(datum, output);
            var input = new DataInputDeserializer(output.GetCopyOfBuffer());
            T deserialized = serializer.Deserialize(reuse, input);
            Assert.True(DeepEquals(datum, deserialized), $"Round trip failed for: {datum}");
            reuse = deserialized;
        }
    }

    [Fact]
    public void TestSerializeAsSequence()
    {
        TypeSerializer<T> serializer = CreateSerializer();
        T[] data = GetTestData();

        var output = new DataOutputSerializer(64);
        foreach (T datum in data)
        {
            serializer.Serialize(datum, output);
        }

        var input = new DataInputDeserializer(output.GetCopyOfBuffer());
        foreach (T datum in data)
        {
            T deserialized = serializer.Deserialize(input);
            Assert.True(DeepEquals(datum, deserialized), $"Round trip failed for: {datum}");
        }
        Assert.Equal(0, input.Available);
    }

    [Fact]
    public void TestSerializedCopy()
    {
        TypeSerializer<T> serializer = CreateSerializer();
        T[] data = GetTestData();

        var source = new DataOutputSerializer(64);
        foreach (T datum in data)
        {
            serializer.Serialize(datum, source);
        }

        var copyTarget = new DataOutputSerializer(64);
        var copySource = new DataInputDeserializer(source.GetCopyOfBuffer());
        for (int i = 0; i < data.Length; i++)
        {
            serializer.Copy(copySource, copyTarget);
        }
        Assert.Equal(0, copySource.Available);
        Assert.Equal(source.GetCopyOfBuffer(), copyTarget.GetCopyOfBuffer());
    }

    [Fact]
    public void TestFixedLengthMatchesSerializedSize()
    {
        TypeSerializer<T> serializer = CreateSerializer();
        if (serializer.Length < 0)
        {
            return;
        }
        foreach (T datum in GetTestData())
        {
            var output = new DataOutputSerializer(16);
            serializer.Serialize(datum, output);
            Assert.Equal(serializer.Length, output.Length);
        }
    }

    [Fact]
    public void TestDuplicate()
    {
        TypeSerializer<T> serializer = CreateSerializer();
        TypeSerializer<T> duplicate = serializer.Duplicate();
        Assert.Equal(serializer, duplicate);

        foreach (T datum in GetTestData())
        {
            var output = new DataOutputSerializer(32);
            duplicate.Serialize(datum, output);
            T deserialized = duplicate.Deserialize(new DataInputDeserializer(output.GetCopyOfBuffer()));
            Assert.True(DeepEquals(datum, deserialized));
        }
    }
}
