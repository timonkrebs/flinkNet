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
using FlinkNet.Api.Tuples;
using FlinkNet.Api.TypeUtils.Runtime;
using FlinkNet.Tests.Api.Common.TypeUtils;
using Xunit;

namespace FlinkNet.Tests.Api.TypeUtils.Runtime;

/// <summary>A test for the <see cref="TupleSerializer{T}"/> with two fields.</summary>
public class Tuple2SerializerTest : SerializerTestBase<Tuple2<int, string>>
{
    protected override TypeSerializer<Tuple2<int, string>> CreateSerializer() =>
        TupleSerializer<Tuple2<int, string>>.ForFields(
            IntSerializer.Instance, StringSerializer.Instance);

    protected override int ExpectedLength => -1;

    protected override Tuple2<int, string>[] GetTestData() =>
    [
        new Tuple2<int, string>(42, "hello"),
        new Tuple2<int, string>(0, ""),
        new Tuple2<int, string>(int.MinValue, "中文测试"),
        new Tuple2<int, string>(int.MaxValue, new string('x', 300)),
    ];
}

/// <summary>A test for the <see cref="TupleSerializer{T}"/> with three fixed-length fields.</summary>
public class Tuple3SerializerTest : SerializerTestBase<Tuple3<bool, long, double>>
{
    protected override TypeSerializer<Tuple3<bool, long, double>> CreateSerializer() =>
        TupleSerializer<Tuple3<bool, long, double>>.ForFields(
            BooleanSerializer.Instance, LongSerializer.Instance, DoubleSerializer.Instance);

    // all fields are fixed-length, so the tuple is fixed-length too (1 + 8 + 8)
    protected override int ExpectedLength => 17;

    protected override Tuple3<bool, long, double>[] GetTestData() =>
    [
        new Tuple3<bool, long, double>(true, 1L, 2.0),
        new Tuple3<bool, long, double>(false, long.MinValue, double.NaN),
        new Tuple3<bool, long, double>(true, long.MaxValue, double.NegativeInfinity),
    ];
}

/// <summary>Additional shape checks for the tuple serializer.</summary>
public class TupleSerializerShapeTest
{
    [Fact]
    public void TestArityMismatchIsRejected()
    {
        Assert.Throws<ArgumentException>(
            () => TupleSerializer<Tuple2<int, string>>.ForFields(IntSerializer.Instance));
    }

    [Fact]
    public void TestEqualityComposesFromFieldSerializers()
    {
        var a = TupleSerializer<Tuple2<int, string>>.ForFields(
            IntSerializer.Instance, StringSerializer.Instance);
        var b = TupleSerializer<Tuple2<int, string>>.ForFields(
            IntSerializer.Instance, StringSerializer.Instance);
        var c = TupleSerializer<Tuple2<int, string>>.ForFields(
            IntSerializer.Instance, IntSerializer.Instance);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void TestStatelessDuplicateReturnsSameInstance()
    {
        var serializer = TupleSerializer<Tuple2<int, string>>.ForFields(
            IntSerializer.Instance, StringSerializer.Instance);
        Assert.Same(serializer, serializer.Duplicate());
    }
}
