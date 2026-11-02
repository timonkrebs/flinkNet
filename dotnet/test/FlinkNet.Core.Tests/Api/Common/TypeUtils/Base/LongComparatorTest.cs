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

/// <summary>Tests for the <see cref="LongComparator"/>.</summary>
public class LongComparatorTest : ComparatorTestBase<long>
{
    protected override TypeComparator<long> CreateComparator(bool ascending) => new LongComparator(ascending);

    protected override TypeSerializer<long> CreateSerializer() => LongSerializer.Instance;

    protected override long[] GetSortedTestData() =>
    [
        long.MinValue,
        -RndLong,
        -1L,
        0L,
        1L,
        2L,
        RndLong,
        long.MaxValue,
    ];

    // the value java.util.Random(874597969123412338L) yields in the Java test
    private const long RndLong = 1430743183740085468L;

    /// <summary>The hash decides the partition of a key, so it must be Java's
    /// <c>Long.hashCode</c> (values from a JDK).</summary>
    [Fact]
    public void TestHashIsJavaHashCode()
    {
        var comparator = new LongComparator(true);
        Assert.Equal(0, comparator.Hash(-1L));
        Assert.Equal(-2147483648, comparator.Hash(long.MaxValue));
        Assert.Equal(256, comparator.Hash(1L << 40));
    }
}
