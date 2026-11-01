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
using Xunit;

namespace FlinkNet.Tests.Api.Common.TypeUtils.Base;

/// <summary>Tests for the <see cref="ByteComparator"/>.</summary>
public class ByteComparatorTest : ComparatorTestBase<sbyte>
{
    protected override TypeComparator<sbyte> CreateComparator(bool ascending) => new ByteComparator(ascending);

    protected override TypeSerializer<sbyte> CreateSerializer() => ByteSerializer.Instance;

    protected override sbyte[] GetSortedTestData() =>
    [
        sbyte.MinValue,
        -RndByte,
        -1,
        0,
        1,
        2,
        RndByte,
        sbyte.MaxValue,
    ];

    // the value java.util.Random(874597969123412338L) yields in the Java test
    private const sbyte RndByte = 55;

    /// <summary>Java's <c>Byte.hashCode</c> is the (signed) value itself, unlike .NET's.</summary>
    [Fact]
    public void TestHashIsJavaHashCode()
    {
        var comparator = new ByteComparator(true);
        Assert.Equal(55, comparator.Hash(55));
        Assert.Equal(-1, comparator.Hash(-1));
    }
}
