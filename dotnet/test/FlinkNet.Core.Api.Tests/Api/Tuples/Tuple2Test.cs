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

    /// <summary>The same-instantiation fast path keeps Java's field semantics, incl. nulls.</summary>
    [Fact]
    public void TestEqualitySameInstantiation()
    {
        var withNull = new Tuple2<string, int?>(null, null);
        Assert.True(withNull.Equals(new Tuple2<string, int?>(null, null)));
        Assert.False(withNull.Equals(new Tuple2<string, int?>("a", null)));
        Assert.False(withNull.Equals(new Tuple2<string, int?>(null, 0)));

        Assert.True(Tuple2.Of(1.5, "x").Equals(Tuple2.Of(1.5, "x")));
        Assert.False(Tuple2.Of(1.5, "x").Equals(null));
    }

    /// <summary>Like Java's toString, rendering is culture-independent (a comma decimal separator
    /// would make "(1,5,2,5)" ambiguous) and booleans print as in Java.</summary>
    [Fact]
    public void TestToStringIsCultureIndependent()
    {
        System.Globalization.CultureInfo previous = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            Assert.Equal("(1.5,2.5)", Tuple2.Of(1.5, 2.5).ToString());
            Assert.Equal("(true,false)", Tuple2.Of(true, false).ToString());
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// Tuple equality and hashing follow Java's boxed <c>equals</c>/<c>hashCode</c> of the
    /// fields (outcomes verified on a JDK): doubles and floats compare by their canonical bits
    /// (-0.0 differs from 0.0, NaN equals NaN), and the hash, which decides the key group of a
    /// tuple key, is Java's, including the string hash, which .NET randomizes per process.
    /// </summary>
    [Fact]
    public void TestEqualsAndHashCodeFollowJava()
    {
        Assert.False(Tuple2.Of(-0.0, 1).Equals(Tuple2.Of(0.0, 1)));
        Assert.True(Tuple2.Of(double.NaN, 1).Equals(Tuple2.Of(double.NaN, 1)));
        Assert.False(Tuple2.Of(-0.0f, 1).Equals(Tuple2.Of(0.0f, 1)));
        Assert.False(Tuple2.Of<double?, int>(-0.0, 1).Equals(Tuple2.Of<double?, int>(0.0, 1)));
        // the structural path across instantiations
        Assert.False(Tuple2.Of<object?, int>(-0.0, 1).Equals(Tuple2.Of(0.0, 1)));
        Assert.True(Tuple2.Of<object?, int>(double.NaN, 1).Equals(Tuple2.Of(double.NaN, 1)));

        Assert.Equal(2986975, Tuple2.Of("abc", 1).GetHashCode());
        Assert.Equal(38258, Tuple2.Of(true, 'a').GetHashCode());
        Assert.Equal(-2147483648, Tuple2.Of<double, string?>(-0.0, null).GetHashCode());
        Assert.Equal(-1094189056, Tuple2.Of(double.NaN, 1.5f).GetHashCode());
        Assert.Equal(-33, Tuple2.Of((sbyte)-1, (short)-2).GetHashCode());
        Assert.Equal(73471, Tuple2.Of(1L << 40, '￿').GetHashCode());
        // boxed and nullable fields hash alike
        Assert.Equal(2986975, Tuple2.Of<object?, object?>("abc", 1).GetHashCode());
        Assert.Equal(-1094189056, Tuple2.Of<double?, float?>(double.NaN, 1.5f).GetHashCode());
    }

    /// <summary>Java prints floating-point fields with Double/Float.toString, also inside
    /// arrays (outcomes verified on a JDK).</summary>
    [Fact]
    public void TestToStringPrintsFloatingPointLikeJava()
    {
        Assert.Equal("(1.0,2.0)", Tuple2.Of(1.0, 2.0f).ToString());
        Assert.Equal("(1.0E10,NaN)", Tuple2.Of(1e10, double.NaN).ToString());
        Assert.Equal("([1.0, 2.5],-0.0)", Tuple2.Of(new[] { 1.0, 2.5 }, -0.0f).ToString());
    }
}
