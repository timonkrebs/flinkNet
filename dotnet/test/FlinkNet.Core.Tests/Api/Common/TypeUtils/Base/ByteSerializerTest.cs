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

namespace FlinkNet.Tests.Api.Common.TypeUtils.Base;

/// <summary>A test for the <see cref="ByteSerializer"/>.</summary>
public class ByteSerializerTest : SerializerTestBase<sbyte>
{
    protected override TypeSerializer<sbyte> CreateSerializer() => ByteSerializer.Instance;

    protected override int ExpectedLength => 1;

    protected override sbyte[] GetTestData() =>
    [
        0,
        1,
        sbyte.MaxValue,
        sbyte.MinValue,
        -1,
        42,
    ];

    /// <summary>Java's byte is signed: the wire byte 0xff is -1, not 255.</summary>
    [Fact]
    public void TestSignedJavaByteDomain()
    {
        var input = new DataInputDeserializer([0xff, 0x80]);
        Assert.Equal(-1, ByteSerializer.Instance.Deserialize(input));
        Assert.Equal(sbyte.MinValue, ByteSerializer.Instance.Deserialize(input));

        var output = new DataOutputSerializer(2);
        ByteSerializer.Instance.Serialize(-1, output);
        Assert.Equal<byte[]>([0xff], output.GetCopyOfBuffer());
    }
}
