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

/// <summary>Tests for the <see cref="BooleanComparator"/>.</summary>
public class BooleanComparatorTest : ComparatorTestBase<bool>
{
    protected override TypeComparator<bool> CreateComparator(bool ascending) => new BooleanComparator(ascending);

    protected override TypeSerializer<bool> CreateSerializer() => BooleanSerializer.Instance;

    protected override bool[] GetSortedTestData() =>
    [
        false,
        true,
    ];

    /// <summary>Java's <c>Boolean.hashCode</c> (1231/1237), unlike .NET's (1/0).</summary>
    [Fact]
    public void TestHashIsJavaHashCode()
    {
        var comparator = new BooleanComparator(true);
        Assert.Equal(1231, comparator.Hash(true));
        Assert.Equal(1237, comparator.Hash(false));
    }
}
