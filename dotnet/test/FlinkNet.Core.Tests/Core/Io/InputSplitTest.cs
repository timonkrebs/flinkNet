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

using FlinkNet.Core.Io;
using Xunit;

namespace FlinkNet.Tests.Core.Io;

/// <summary>
/// Tests for <see cref="GenericInputSplit"/> and <see cref="LocatableInputSplit"/>.
///
/// <para>PORT NOTE: the Java tests for these classes live in the split-assigner tests
/// (not yet ported, they belong to the runtime); this pins the basic value semantics.</para>
/// </summary>
public class InputSplitTest
{
    [Fact]
    public void TestGenericSplitProperties()
    {
        var split = new GenericInputSplit(2, 5);
        Assert.Equal(2, split.SplitNumber);
        Assert.Equal(5, split.TotalNumberOfSplits);
        Assert.Equal("GenericSplit (2/5)", split.ToString());
    }

    [Fact]
    public void TestGenericSplitEquality()
    {
        Assert.Equal(new GenericInputSplit(3, 7), new GenericInputSplit(3, 7));
        Assert.Equal(
            new GenericInputSplit(3, 7).GetHashCode(),
            new GenericInputSplit(3, 7).GetHashCode());
        Assert.NotEqual(new GenericInputSplit(3, 7), new GenericInputSplit(4, 7));
        Assert.NotEqual(new GenericInputSplit(3, 7), new GenericInputSplit(3, 8));
    }

    [Fact]
    public void TestLocatableSplitProperties()
    {
        var multi = new LocatableInputSplit(1, new[] { "host1", "host2" });
        Assert.Equal(1, multi.SplitNumber);
        Assert.Equal(new[] { "host1", "host2" }, multi.GetHostnames());
        Assert.Equal("Locatable Split (1) at [host1, host2]", multi.ToString());

        var single = new LocatableInputSplit(2, "localhost");
        Assert.Equal(new[] { "localhost" }, single.GetHostnames());

        var noHosts = new LocatableInputSplit(3, (string?)null);
        Assert.Empty(noHosts.GetHostnames());
    }

    [Fact]
    public void TestLocatableSplitEquality()
    {
        var a = new LocatableInputSplit(1, new[] { "h1", "h2" });
        var b = new LocatableInputSplit(1, new[] { "h1", "h2" });
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());

        Assert.NotEqual(a, new LocatableInputSplit(2, new[] { "h1", "h2" }));
        Assert.NotEqual(a, new LocatableInputSplit(1, new[] { "h1" }));
    }
}
