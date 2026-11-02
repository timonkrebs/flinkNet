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

/// <summary>Tests for the <see cref="FloatComparator"/>.</summary>
public class FloatComparatorTest : ComparatorTestBase<float>
{
    protected override TypeComparator<float> CreateComparator(bool ascending) => new FloatComparator(ascending);

    protected override TypeSerializer<float> CreateSerializer() => FloatSerializer.Instance;

    protected override float[] GetSortedTestData() =>
    [
        -RndFloat,
        -1.0f,
        0.0f,
        2.0f,
        RndFloat,
        float.MaxValue,
    ];

    // the value java.util.Random(874597969123412338L) yields in the Java test
    private static readonly float RndFloat = BitConverter.Int32BitsToSingle(1078261441); // 3.0775607

    /// <summary>Java's <c>hashCode</c> collapses every NaN to the canonical one (values from a
    /// JDK).</summary>
    [Fact]
    public void TestHashIsJavaHashCode()
    {
        var comparator = new FloatComparator(true);
        Assert.Equal(1069547520, comparator.Hash(1.5f));
        Assert.Equal(-2147483648, comparator.Hash(-0.0f));
        Assert.Equal(0, comparator.Hash(0.0f));
        Assert.Equal(2143289344, comparator.Hash(float.NaN));
        Assert.Equal(2143289344, comparator.Hash(BitConverter.Int32BitsToSingle(unchecked((int)0xffc00001))));
    }

    /// <summary>
    /// <c>Compare</c> and the reference methods follow Java's total order (as
    /// <c>Float.compare</c>), with -0.0 before 0.0 and NaN above positive infinity, and
    /// NaN equal to itself (outcomes verified on a JDK).
    /// </summary>
    [Fact]
    public void TestCompareFollowsJavaTotalOrder()
    {
        var comparator = new FloatComparator(true);
        Assert.True(comparator.Compare(-0.0f, 0.0f) < 0);
        Assert.True(comparator.Compare(float.NaN, float.PositiveInfinity) > 0);
        Assert.Equal(0, comparator.Compare(float.NaN, float.NaN));
        Assert.True(new FloatComparator(false).Compare(-0.0f, 0.0f) > 0);

        comparator.SetReference(0.0f);
        Assert.False(comparator.EqualToReference(-0.0f));
        comparator.SetReference(float.NaN);
        Assert.True(comparator.EqualToReference(float.NaN));

        var other = new FloatComparator(true);
        other.SetReference(-0.0f);
        comparator.SetReference(0.0f);
        // compares the other reference against this one
        Assert.True(comparator.CompareToReference(other) < 0);
    }

    /// <summary>The serialized comparison uses the IEEE operators as in Java, so it does not
    /// distinguish -0.0 from 0.0 and treats NaN as equal to any value.</summary>
    [Fact]
    public void TestCompareSerializedUsesIeeeOperators()
    {
        var comparator = new FloatComparator(true);
        Assert.Equal(0, comparator.CompareSerialized(Serialize(-0.0f), Serialize(0.0f)));
        Assert.Equal(0, comparator.CompareSerialized(Serialize(float.NaN), Serialize(1.0f)));
        Assert.True(comparator.CompareSerialized(Serialize(-1.0f), Serialize(1.0f)) < 0);
    }

    [Fact]
    public void TestHasNoNormalizedKey()
    {
        var comparator = new FloatComparator(true);
        Assert.False(comparator.SupportsNormalizedKey);
        Assert.Equal(0, comparator.NormalizeKeyLen);
        Assert.Throws<NotSupportedException>(
            () => comparator.PutNormalizedKey(1.0f, MemorySegmentFactory.AllocateUnpooledSegment(8), 0, 8));
    }

    private static DataInputDeserializer Serialize(float value)
    {
        var output = new DataOutputSerializer(8);
        FloatSerializer.Instance.Serialize(value, output);
        return new DataInputDeserializer(output.GetCopyOfBuffer());
    }
}
