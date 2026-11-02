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
using FlinkNet.Types;
using Xunit;

namespace FlinkNet.Tests.Types;

/// <summary>
/// Tests for the <see cref="ICopyableValue{T}"/> implementations (port of Flink's
/// <c>CopyableValueTest</c>).
///
/// <para>PORT NOTE: the mutable <c>StringValue</c> class is not ported yet, so its cases are
/// left out.</para>
/// </summary>
public class CopyableValueTest
{
    [Fact]
    public void TestCopy()
    {
        object[] valueTypes =
        [
            new BooleanValue(true).Copy(), new BooleanValue(true),
            new ByteValue(42).Copy(), new ByteValue(42),
            new CharValue('q').Copy(), new CharValue('q'),
            new DoubleValue(3.1415926535897932).Copy(), new DoubleValue(3.1415926535897932),
            new FloatValue(3.14159265f).Copy(), new FloatValue(3.14159265f),
            new IntValue(42).Copy(), new IntValue(42),
            new LongValue(42L).Copy(), new LongValue(42L),
            new NullValue().Copy(), new NullValue(),
            new ShortValue(42).Copy(), new ShortValue(42),
        ];

        for (int i = 0; i < valueTypes.Length; i += 2)
        {
            Assert.Equal(valueTypes[i + 1], valueTypes[i]);
        }
    }

    [Fact]
    public void TestCopyTo()
    {
        var booleanFrom = new BooleanValue(true);
        var booleanTo = new BooleanValue(false);
        booleanFrom.CopyTo(booleanTo);
        Assert.Equal(booleanFrom, booleanTo);

        var byteFrom = new ByteValue(3);
        var byteTo = new ByteValue(7);
        byteFrom.CopyTo(byteTo);
        Assert.Equal(byteFrom, byteTo);

        var charFrom = new CharValue('α');
        var charTo = new CharValue('ω');
        charFrom.CopyTo(charTo);
        Assert.Equal(charFrom, charTo);

        var doubleFrom = new DoubleValue(2.7182818284590451);
        var doubleTo = new DoubleValue(0);
        doubleFrom.CopyTo(doubleTo);
        Assert.Equal(doubleFrom, doubleTo);

        var floatFrom = new FloatValue(2.71828182f);
        var floatTo = new FloatValue(1.41421356f);
        floatFrom.CopyTo(floatTo);
        Assert.Equal(floatFrom, floatTo);

        var intFrom = new IntValue(8191);
        var intTo = new IntValue(131071);
        intFrom.CopyTo(intTo);
        Assert.Equal(intFrom, intTo);

        var longFrom = new LongValue(524287);
        var longTo = new LongValue(2147483647);
        longFrom.CopyTo(longTo);
        Assert.Equal(longFrom, longTo);

        var nullFrom = new NullValue();
        var nullTo = new NullValue();
        nullFrom.CopyTo(nullTo);
        Assert.Equal(nullFrom, nullTo);

        var shortFrom = new ShortValue(31);
        var shortTo = new ShortValue(127);
        shortFrom.CopyTo(shortTo);
        Assert.Equal(shortFrom, shortTo);
    }
}
