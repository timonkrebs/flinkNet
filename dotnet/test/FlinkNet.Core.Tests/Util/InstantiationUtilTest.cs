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
using FlinkNet.Util;
using Xunit;

namespace FlinkNet.Tests.Util;

/// <summary>Tests for the portable type names of <see cref="InstantiationUtil"/>.</summary>
public class InstantiationUtilTest
{
    public static TheoryData<Type> Types => new()
    {
        typeof(int),
        typeof(string[]),
        typeof(int[,]),
        typeof(List<string>),
        typeof(Dictionary<string, List<int>>),
        typeof(List<int>[]),
        typeof(IntSerializer.IntSerializerSnapshot),
        typeof(ListSerializerSnapshot<string>),
        typeof(MapSerializerSnapshot<string, List<long>>),
        typeof(InstantiationUtilTest),
    };

    /// <summary>Persisted names must not carry assembly versions (a newer-written name would not
    /// bind on an older build) yet must resolve back to the same type.</summary>
    [Theory]
    [MemberData(nameof(Types))]
    public void TestPortableNameIsVersionFreeAndResolves(Type type)
    {
        string name = InstantiationUtil.GetPortableTypeName(type);

        Assert.DoesNotContain("Version=", name);
        Assert.DoesNotContain("PublicKeyToken=", name);
        Assert.DoesNotContain("Culture=", name);
        Assert.Equal(type, Type.GetType(name, throwOnError: true));
    }

    [Fact]
    public void TestWriteAndResolveRoundTrip()
    {
        var output = new DataOutputSerializer(128);
        InstantiationUtil.WriteTypeName(output, typeof(Dictionary<string, int[]>));

        Type resolved = InstantiationUtil.ResolveTypeByName(
            new DataInputDeserializer(output.GetCopyOfBuffer()));
        Assert.Equal(typeof(Dictionary<string, int[]>), resolved);
    }

    /// <summary>Names carrying full assembly identities still resolve.</summary>
    [Fact]
    public void TestResolvesFullAssemblyQualifiedNames()
    {
        var output = new DataOutputSerializer(256);
        output.WriteUTF(typeof(List<string>).AssemblyQualifiedName!);

        Assert.Equal(
            typeof(List<string>),
            InstantiationUtil.ResolveTypeByName(new DataInputDeserializer(output.GetCopyOfBuffer())));
    }

    [Fact]
    public void TestUnknownTypeIsAnIOException()
    {
        var output = new DataOutputSerializer(64);
        output.WriteUTF("No.Such.Type, No.Such.Assembly");

        Assert.Throws<IOException>(
            () => InstantiationUtil.ResolveTypeByName(new DataInputDeserializer(output.GetCopyOfBuffer())));
    }

    /// <summary>The versioned snapshot envelope persists the portable name.</summary>
    [Fact]
    public void TestVersionedSnapshotPersistsPortableName()
    {
        var output = new DataOutputSerializer(256);
        TypeSerializerSnapshot.WriteVersionedSnapshot(
            output, new ListSerializer<string>(StringSerializer.Instance).SnapshotConfiguration());

        string className = new DataInputDeserializer(output.GetCopyOfBuffer()).ReadUTF();
        Assert.Equal(
            InstantiationUtil.GetPortableTypeName(typeof(ListSerializerSnapshot<string>)), className);
    }
}
