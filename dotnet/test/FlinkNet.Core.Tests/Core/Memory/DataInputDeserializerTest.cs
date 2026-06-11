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

/// <summary>Test suite for the <see cref="DataInputDeserializer"/> class.</summary>
public class DataInputDeserializerTest
{
    [Fact]
    public void TestAvailable()
    {
        byte[] bytes = [];
        var dis = new DataInputDeserializer(bytes, 0, bytes.Length);
        Assert.Equal(bytes.Length, dis.Available);

        bytes = [1, 2, 3];
        dis = new DataInputDeserializer(bytes, 0, bytes.Length);
        Assert.Equal(bytes.Length, dis.Available);

        dis.ReadByte();
        Assert.Equal(2, dis.Available);
        dis.ReadByte();
        Assert.Equal(1, dis.Available);
        dis.ReadByte();
        Assert.Equal(0, dis.Available);

        Assert.Throws<EndOfStreamException>(() => dis.ReadByte());
        Assert.Equal(0, dis.Available);
    }

    [Fact]
    public void TestReadWithLenZero()
    {
        byte[] bytes = [];
        var dis = new DataInputDeserializer(bytes, 0, bytes.Length);
        Assert.Equal(0, dis.Available);

        byte[] bytesForRead = [];
        Assert.Equal(0, dis.Read(bytesForRead, 0, 0)); // do not throw when read with len 0
    }
}
