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
using FlinkNet.Types;
using Xunit;

namespace FlinkNet.Tests.Api.Common.TypeUtils.Base;

/// <summary>Tests for the <see cref="StringComparator"/>.</summary>
public class StringComparatorTest : ComparatorTestBase<string>
{
    protected override TypeComparator<string> CreateComparator(bool ascending) => new StringComparator(ascending);

    protected override TypeSerializer<string> CreateSerializer() => StringSerializer.Instance;

    protected override string[] GetSortedTestData() =>
    [
        "",
        "Lorem Ipsum Dolor Omit Longer",
        "aaaa",
        "abcd",
        "abce",
        "abdd",
        "accd",
        "bbcd",
    ];

    /// <summary>The hash decides the partition of a key, so it must be Java's
    /// <c>String.hashCode</c>, not .NET's per-process randomized one (values from a JDK).</summary>
    [Fact]
    public void TestHashIsJavaHashCode()
    {
        var comparator = new StringComparator(true);
        Assert.Equal(0, comparator.Hash(""));
        Assert.Equal(96354, comparator.Hash("abc"));
        Assert.Equal(27236, comparator.Hash("\u00e9\u4e2d"));
        Assert.Equal(-1603267905, comparator.Hash("Lorem Ipsum Dolor Omit Longer"));
    }

    /// <summary>Java's <c>String.compareTo</c> orders by UTF-16 code unit, independent of the
    /// culture (outcomes verified on a JDK).</summary>
    [Fact]
    public void TestCompareIsOrdinal()
    {
        var comparator = new StringComparator(true);
        Assert.True(comparator.Compare("a", "B") > 0);
        Assert.True(comparator.Compare("ab", "abc") < 0);
        Assert.True(comparator.Compare("\u00e9", "z") > 0);
        Assert.True(comparator.CompareSerialized(Serialize("a"), Serialize("B")) > 0);

        comparator.SetReference("a");
        Assert.False(comparator.EqualToReference("A"));
    }

    /// <summary>The normalized key bytes, as written by the Java comparator (from a JDK): one
    /// byte below 0x80, two below 0x2000, three above, cut at the key length and zero-padded.</summary>
    [Fact]
    public void TestNormalizedKeyEncoding()
    {
        Assert.Equal("61", NormalizedKey("a\u00e9\u4e2d", 1));
        Assert.Equal("6181", NormalizedKey("a\u00e9\u4e2d", 2));
        Assert.Equal("6181e9d3", NormalizedKey("a\u00e9\u4e2d", 4));
        Assert.Equal("6181e9d38b2d", NormalizedKey("a\u00e9\u4e2d", 6));
        Assert.Equal("6181e9d38b2d000000", NormalizedKey("a\u00e9\u4e2d", 9));
        Assert.Equal("bfffc800", NormalizedKey("\u1fff\u2000", 4));
    }

    /// <summary>Comparing null fails as in Java (where it is a NullPointerException); a missing
    /// reference never equals a candidate.</summary>
    [Fact]
    public void TestNullHandling()
    {
        var comparator = new StringComparator(true);
        Assert.Throws<ArgumentNullException>(() => comparator.Compare(null!, "a"));
        Assert.Throws<ArgumentNullException>(() => comparator.Compare("a", null!));
        Assert.Throws<ArgumentNullException>(() => comparator.Hash(null!));
        Assert.Throws<ArgumentNullException>(() => comparator.EqualToReference(null!));
        Assert.False(comparator.EqualToReference("a"));
        Assert.Throws<InvalidOperationException>(
            () => comparator.CompareToReference(new StringComparator(true)));
        Assert.Throws<InvalidOperationException>(
            () => comparator.CompareSerialized(Serialize(null), Serialize("a")));
    }

    private static string NormalizedKey(string value, int numBytes)
    {
        // surround the key with sentinels to check that exactly numBytes are written
        byte[] buffer = new byte[numBytes + 2];
        Array.Fill(buffer, (byte)0xee);
        new StringComparator(true).PutNormalizedKey(value, MemorySegmentFactory.Wrap(buffer), 1, numBytes);
        Assert.Equal(0xee, buffer[0]);
        Assert.Equal(0xee, buffer[^1]);
        return Convert.ToHexString(buffer, 1, numBytes).ToLowerInvariant();
    }

    private static DataInputDeserializer Serialize(string? value)
    {
        var output = new DataOutputSerializer(16);
        StringValue.WriteString(value, output);
        return new DataInputDeserializer(output.GetCopyOfBuffer());
    }
}
