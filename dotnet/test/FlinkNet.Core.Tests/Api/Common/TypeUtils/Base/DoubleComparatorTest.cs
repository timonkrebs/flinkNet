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
using FlinkNet.Core.Memory;
using Xunit;

namespace FlinkNet.Tests.Api.Common.TypeUtils.Base;

/// <summary>Tests for the <see cref="DoubleComparator"/>.</summary>
public class DoubleComparatorTest : ComparatorTestBase<double>
{
    protected override TypeComparator<double> CreateComparator(bool ascending) => new DoubleComparator(ascending);

    protected override TypeSerializer<double> CreateSerializer() => DoubleSerializer.Instance;

    protected override double[] GetSortedTestData() =>
    [
        -RndDouble,
        -1.0,
        0.0,
        2.0,
        RndDouble,
        double.MaxValue,
    ];

    // the value java.util.Random(874597969123412338L) yields in the Java test
    private static readonly double RndDouble = BitConverter.Int64BitsToDouble(4614112469508376394L); // 3.077560743296009

    /// <summary>Java's <c>hashCode</c> collapses every NaN to the canonical one (values from a
    /// JDK).</summary>
    [Fact]
    public void TestHashIsJavaHashCode()
    {
        var comparator = new DoubleComparator(true);
        Assert.Equal(1073217536, comparator.Hash(1.5));
        Assert.Equal(-2147483648, comparator.Hash(-0.0));
        Assert.Equal(0, comparator.Hash(0.0));
        Assert.Equal(2146959360, comparator.Hash(double.NaN));
        Assert.Equal(2146959360, comparator.Hash(BitConverter.Int64BitsToDouble(unchecked((long)0xfff8000000000001UL))));
    }

    /// <summary>
    /// <c>Compare</c> and the reference methods follow Java's total order (as
    /// <c>Double.compare</c>), with -0.0 before 0.0 and NaN above positive infinity, and
    /// NaN equal to itself (outcomes verified on a JDK).
    /// </summary>
    [Fact]
    public void TestCompareFollowsJavaTotalOrder()
    {
        var comparator = new DoubleComparator(true);
        Assert.True(comparator.Compare(-0.0, 0.0) < 0);
        Assert.True(comparator.Compare(double.NaN, double.PositiveInfinity) > 0);
        Assert.Equal(0, comparator.Compare(double.NaN, double.NaN));
        Assert.True(new DoubleComparator(false).Compare(-0.0, 0.0) > 0);

        comparator.SetReference(0.0);
        Assert.False(comparator.EqualToReference(-0.0));
        comparator.SetReference(double.NaN);
        Assert.True(comparator.EqualToReference(double.NaN));

        var other = new DoubleComparator(true);
        other.SetReference(-0.0);
        comparator.SetReference(0.0);
        // compares the other reference against this one
        Assert.True(comparator.CompareToReference(other) < 0);
    }

    /// <summary>The serialized comparison uses the IEEE operators as in Java, so it does not
    /// distinguish -0.0 from 0.0 and treats NaN as equal to any value.</summary>
    [Fact]
    public void TestCompareSerializedUsesIeeeOperators()
    {
        var comparator = new DoubleComparator(true);
        Assert.Equal(0, comparator.CompareSerialized(Serialize(-0.0), Serialize(0.0)));
        Assert.Equal(0, comparator.CompareSerialized(Serialize(double.NaN), Serialize(1.0)));
        Assert.True(comparator.CompareSerialized(Serialize(-1.0), Serialize(1.0)) < 0);
    }

    [Fact]
    public void TestHasNoNormalizedKey()
    {
        var comparator = new DoubleComparator(true);
        Assert.False(comparator.SupportsNormalizedKey);
        Assert.Equal(0, comparator.NormalizeKeyLen);
        Assert.Throws<NotSupportedException>(
            () => comparator.PutNormalizedKey(1.0, MemorySegmentFactory.AllocateUnpooledSegment(8), 0, 8));
    }

    private static DataInputDeserializer Serialize(double value)
    {
        var output = new DataOutputSerializer(8);
        DoubleSerializer.Instance.Serialize(value, output);
        return new DataInputDeserializer(output.GetCopyOfBuffer());
    }
}
