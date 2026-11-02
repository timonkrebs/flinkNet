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

using FlinkNet.Util;
using Xunit;

namespace FlinkNet.Tests.Util;

/// <summary>
/// Tests for <see cref="TaggedUnion{T1, T2}"/>.
///
/// <para>PORT NOTE: pins the explicit side discriminator — Java tells the sides apart by
/// null-ness, which .NET value types cannot support (default(int) is not null).</para>
/// </summary>
public class TaggedUnionTest
{
    [Fact]
    public void TestDiscriminatesReferenceTypes()
    {
        var one = TaggedUnion<string, object>.One("a");
        Assert.True(one.IsOne);
        Assert.False(one.IsTwo);
        Assert.Equal("a", one.GetOne());

        var two = TaggedUnion<string, object>.Two("b");
        Assert.False(two.IsOne);
        Assert.True(two.IsTwo);
        Assert.Equal("b", two.GetTwo());
    }

    [Fact]
    public void TestDiscriminatesValueTypes()
    {
        var two = TaggedUnion<int, string>.Two("x");
        Assert.False(two.IsOne);
        Assert.True(two.IsTwo);
        Assert.Equal("x", two.GetTwo());

        var one = TaggedUnion<int, string>.One(0);
        Assert.True(one.IsOne);
        Assert.False(one.IsTwo);
        Assert.Equal(0, one.GetOne());
    }

    [Fact]
    public void TestEqualityIncludesTheActiveSide()
    {
        Assert.Equal(TaggedUnion<int, int>.One(5), TaggedUnion<int, int>.One(5));
        Assert.NotEqual(TaggedUnion<int, int>.One(0), TaggedUnion<int, int>.Two(0));
        Assert.NotEqual(TaggedUnion<int, int>.One(1), TaggedUnion<int, int>.One(2));
    }
}
