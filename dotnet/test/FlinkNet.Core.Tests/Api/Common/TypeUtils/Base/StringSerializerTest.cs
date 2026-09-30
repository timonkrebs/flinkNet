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

namespace FlinkNet.Tests.Api.Common.TypeUtils.Base;

/// <summary>A test for the <see cref="StringSerializer"/>.</summary>
public class StringSerializerTest : SerializerTestBase<string>
{
    protected override TypeSerializer<string> CreateSerializer() => StringSerializer.Instance;

    protected override int ExpectedLength => -1;

    protected override string[] GetTestData()
    {
        var rnd = new Random(874597969123412341L.GetHashCode());

        return
        [
            "a",
            "",
            "bcd",
            "jbmbmner8 jhk hj \n \t üäßß@µ",
            "",
            "non-empty",
            // exercises the multi-byte length varint (length > 127)
            StringUtils.GetRandomString(rnd, 200, 300),
            // exercises the 2-byte and 3-byte char varint paths
            "中文测试字符串 with mixed ascii ° and ★",
            StringUtils.GetRandomString(rnd, 10, 50, (char)0x80, (char)0x3FFF),
            StringUtils.GetRandomString(rnd, 10, 50, (char)0x4000, char.MaxValue),
        ];
    }

    [Fact]
    public void TestNullStringRoundTrip()
    {
        var output = new DataOutputSerializer(8);
        StringSerializer.Instance.Serialize(null!, output);
        Assert.Equal(new byte[] { 0 }, output.GetCopyOfBuffer());

        var input = new DataInputDeserializer(output.GetCopyOfBuffer());
        Assert.Null(FlinkNet.Types.StringValue.ReadString(input));
    }
}
