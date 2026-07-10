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
using FlinkNet.Api.Common.TypeUtils.Base;
using FlinkNet.Core.Memory;
using Xunit;

namespace FlinkNet.Tests.Api.Common.TypeUtils;

/// <summary>Tests for the <see cref="GenericArraySerializer{C}"/> and its snapshot.</summary>
public class GenericArraySerializerTest
{
    [Fact]
    public void TestSerializationRoundTripWithNulls()
    {
        var serializer = new GenericArraySerializer<string>(StringSerializer.Instance);
        string?[] data = ["beep", null, "boop"];

        var output = new DataOutputSerializer(64);
        serializer.Serialize(data, output);
        string?[] deserialized =
            serializer.Deserialize(new DataInputDeserializer(output.GetCopyOfBuffer()));

        Assert.Equal(data, deserialized);
    }

    [Fact]
    public void TestBinaryCopy()
    {
        var serializer = new GenericArraySerializer<string>(StringSerializer.Instance);
        string?[] data = [null, "x", null];

        var output = new DataOutputSerializer(64);
        serializer.Serialize(data, output);

        var copyTarget = new DataOutputSerializer(64);
        serializer.Copy(new DataInputDeserializer(output.GetCopyOfBuffer()), copyTarget);

        string?[] copied =
            serializer.Deserialize(new DataInputDeserializer(copyTarget.GetCopyOfBuffer()));
        Assert.Equal(data, copied);
    }

    [Fact]
    public void TestObjectCopyAndCreateInstance()
    {
        var serializer = new GenericArraySerializer<string>(StringSerializer.Instance);

        string?[] copy = serializer.Copy(["a", null]);
        Assert.Equal(new string?[] { "a", null }, copy);

        Assert.Empty(serializer.CreateInstance());
        Assert.Same(serializer.CreateInstance(), serializer.CreateInstance());
    }

    [Fact]
    public void TestSnapshotRoundTripAndCompatibility()
    {
        var serializer = new GenericArraySerializer<string>(StringSerializer.Instance);
        TypeSerializerSnapshot<string?[]> snapshot = serializer.SnapshotConfiguration();

        var output = new DataOutputSerializer(256);
        TypeSerializerSnapshot.WriteVersionedSnapshot(output, snapshot);
        TypeSerializerSnapshot restored = TypeSerializerSnapshot.ReadVersionedSnapshot(
            new DataInputDeserializer(output.GetCopyOfBuffer()));

        var typed = Assert.IsType<GenericArraySerializerSnapshot<string>>(restored);
        Assert.Equal(serializer, typed.RestoreSerializer());

        // same component class and serializer: compatible as is (the outer snapshot check)
        Assert.True(
            serializer
                .SnapshotConfiguration()
                .ResolveSchemaCompatibility(typed)
                .IsCompatibleAsIs());
    }
}
