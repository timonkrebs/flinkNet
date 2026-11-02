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

/// <summary>Tests for <see cref="JavaEqualityComparer{T}"/>: dictionaries keyed by floating-point
/// values keep -0.0 and 0.0 apart, as a Java HashMap does (verified on a JDK).</summary>
public class JavaEqualityComparerTest
{
    [Fact]
    public void TestSignedZeroKeysAreDistinct()
    {
        Assert.Equal(2, Fill(new Dictionary<double, int>(JavaEqualityComparer<double>.Default), -0.0, 0.0));
        Assert.Equal(2, Fill(new Dictionary<float, int>(JavaEqualityComparer<float>.Default), -0.0f, 0.0f));
        Assert.False(JavaEqualityComparer<double?>.Default.Equals(-0.0, 0.0));
        Assert.True(JavaEqualityComparer<double?>.Default.Equals(null, null));
        Assert.Equal(2, Fill(new Dictionary<object, int>(JavaEqualityComparer<object>.Default), -0.0, 0.0));

        // NaN keys are equal to each other, in Java as in .NET
        Assert.Equal(1, Fill(
            new Dictionary<double, int>(JavaEqualityComparer<double>.Default),
            double.NaN,
            BitConverter.Int64BitsToDouble(unchecked((long)0xfff8000000000001UL))));
    }

    [Fact]
    public void TestOtherTypesUseTheDefaultComparer()
    {
        Assert.Same(EqualityComparer<int>.Default, JavaEqualityComparer<int>.Default);
        Assert.Same(EqualityComparer<string>.Default, JavaEqualityComparer<string>.Default);
    }

    private static int Fill<TKey>(Dictionary<TKey, int> map, TKey first, TKey second)
        where TKey : notnull
    {
        map[first] = 1;
        map[second] = 2;
        return map.Count;
    }
}
