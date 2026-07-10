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

using FlinkNet.Annotations;
using Xunit;

namespace FlinkNet.Tests;

/// <summary>Tests for <see cref="FlinkVersions"/>.</summary>
public class FlinkVersionTest
{
    /// <summary>Java's LinkedHashSet-backed rangeOf iterates in chronological version order;
    /// the port must preserve that.</summary>
    [Fact]
    public void TestRangeOfIteratesInChronologicalOrder()
    {
        FlinkVersion[] all = Enum.GetValues<FlinkVersion>();
        FlinkVersion start = all[0];
        FlinkVersion end = all[^1];

        IReadOnlySet<FlinkVersion> range = FlinkVersions.RangeOf(start, end);

        Assert.Equal(all, range);
    }

    [Fact]
    public void TestRangeOfIsInclusive()
    {
        FlinkVersion[] all = Enum.GetValues<FlinkVersion>();
        FlinkVersion start = all[1];
        FlinkVersion end = all[3];

        IReadOnlySet<FlinkVersion> range = FlinkVersions.RangeOf(start, end);

        Assert.Equal(new[] { all[1], all[2], all[3] }, range);
        Assert.Contains(start, range);
        Assert.Contains(end, range);
    }
}
