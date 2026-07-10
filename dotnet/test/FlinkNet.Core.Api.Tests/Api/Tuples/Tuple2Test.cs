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

using FlinkNet.Api.Tuples;
using FlinkNet.Types;
using Xunit;
using Tuple = FlinkNet.Api.Tuples.Tuple;

namespace FlinkNet.Tests.Api.Tuples;

/// <summary>Tests for <see cref="Tuple2{T0,T1}"/>.</summary>
public class Tuple2Test
{
    [Fact]
    public void TestSwapValues()
    {
        var toSwap = new Tuple2<string, int?>("Test case", 25);
        Tuple2<int?, string> swapped = toSwap.Swap();

        Assert.Equal(swapped.F0, toSwap.F1);

        Assert.Equal(swapped.F1, toSwap.F0);
    }

    [Fact]
    public void TestGetFieldNotNull()
    {
        var tuple = new Tuple2<string, int?>("Test case", null);
        Assert.Equal("Test case", tuple.GetFieldNotNull<string>(0));
        Assert.Throws<NullFieldException>(() => tuple.GetFieldNotNull<int?>(1));
    }

    /// <summary>Like Java's raw <c>instanceof Tuple2</c> equality, tuples of the same arity
    /// class compare structurally across generic instantiations.</summary>
    [Fact]
    public void TestEqualityAcrossGenericInstantiations()
    {
        var typed = Tuple2.Of(1, "a");

        Tuple erased = Tuple.NewInstance(2);
        erased.SetField(1, 0);
        erased.SetField("a", 1);

        Assert.True(typed.Equals(erased));
        Assert.True(erased.Equals(typed));
        Assert.Equal(typed.GetHashCode(), erased.GetHashCode());

        Assert.False(typed.Equals(Tuple2.Of(2, "a")));
        Assert.False(typed.Equals(Tuple2.Of(1, "b")));
        Assert.False(typed.Equals(Tuple1.Of(1)));
    }
}
