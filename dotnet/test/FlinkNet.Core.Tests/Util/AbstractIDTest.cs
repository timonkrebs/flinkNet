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
/// This class contains tests for the <see cref="AbstractID"/> class.
///
/// <para>PORT NOTE: the Java-serialization round-trip tests are replaced by byte/copy round-trips;
/// <c>testOldAbstractIDDeserialization</c> (legacy Java serialization resources) is not
/// applicable.</para>
/// </summary>
public class AbstractIDTest
{
    /// <summary>Tests the copy round-trips of an abstract ID.</summary>
    [Fact]
    public void TestCopyRoundTrips()
    {
        var origID = new AbstractID();
        var copyID = new AbstractID(origID.GetBytes());

        Assert.Equal(origID.GetHashCode(), copyID.GetHashCode());
        Assert.Equal(origID, copyID);
    }

    [Fact]
    public void TestConvertToBytes()
    {
        var origID = new AbstractID();

        var copy1 = new AbstractID(origID);
        var copy2 = new AbstractID(origID.GetBytes());
        var copy3 = new AbstractID(origID.LowerPart, origID.UpperPart);

        Assert.Equal(origID, copy1);
        Assert.Equal(origID, copy2);
        Assert.Equal(origID, copy3);
    }

    [Fact]
    public void TestCompare()
    {
        var id1 = new AbstractID(0, 0);
        var id2 = new AbstractID(1, 0);
        var id3 = new AbstractID(0, 1);
        var id4 = new AbstractID(-1, 0);
        var id5 = new AbstractID(0, -1);
        var id6 = new AbstractID(-1, -1);

        var id7 = new AbstractID(long.MaxValue, long.MaxValue);
        var id8 = new AbstractID(long.MinValue, long.MinValue);
        var id9 = new AbstractID(long.MaxValue, long.MinValue);
        var id10 = new AbstractID(long.MinValue, long.MaxValue);

        // test self equality
        Assert.Equal(0, id1.CompareTo(new AbstractID(id1.GetBytes())));
        Assert.Equal(0, id2.CompareTo(new AbstractID(id2.GetBytes())));
        Assert.Equal(0, id3.CompareTo(new AbstractID(id3.GetBytes())));
        Assert.Equal(0, id4.CompareTo(new AbstractID(id4.GetBytes())));
        Assert.Equal(0, id5.CompareTo(new AbstractID(id5.GetBytes())));
        Assert.Equal(0, id6.CompareTo(new AbstractID(id6.GetBytes())));
        Assert.Equal(0, id7.CompareTo(new AbstractID(id7.GetBytes())));
        Assert.Equal(0, id8.CompareTo(new AbstractID(id8.GetBytes())));
        Assert.Equal(0, id9.CompareTo(new AbstractID(id9.GetBytes())));
        Assert.Equal(0, id10.CompareTo(new AbstractID(id10.GetBytes())));

        // test order
        AssertCompare(id1, id2, -1);
        AssertCompare(id1, id3, -1);
        AssertCompare(id1, id4, 1);
        AssertCompare(id1, id5, 1);
        AssertCompare(id1, id6, 1);
        AssertCompare(id2, id5, 1);
        AssertCompare(id3, id5, 1);
        AssertCompare(id2, id6, 1);
        AssertCompare(id3, id6, 1);
        AssertCompare(id1, id7, -1);
        AssertCompare(id1, id8, 1);
        AssertCompare(id7, id8, 1);
        AssertCompare(id9, id10, -1);
        AssertCompare(id7, id9, 1);
        AssertCompare(id7, id10, 1);
        AssertCompare(id8, id9, -1);
        AssertCompare(id8, id10, -1);
    }

    private static void AssertCompare(AbstractID a, AbstractID b, int signum)
    {
        int cmpAb = a.CompareTo(b);
        int cmpBa = b.CompareTo(a);

        int sgnAb = Math.Sign(cmpAb);
        int sgnBa = Math.Sign(cmpBa);

        Assert.Equal(signum, sgnAb);
        Assert.Equal(sgnAb, -sgnBa);
    }
}
