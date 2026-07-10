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
/// Tests for <see cref="TimeUtils"/>.
///
/// <para>PORT NOTE: TimeSpan ticks are 100ns, so nanosecond expectations are adapted to the
/// documented truncation (e.g. 424562ns parses to 4245 ticks), and values beyond the TimeSpan
/// range are overflow errors where Java's Duration could still represent them.</para>
/// </summary>
public class TimeUtilsTest
{
    [Fact]
    public void TestParseDurationNanos()
    {
        // 424562ns == 4245.62 ticks, truncated to 4245 ticks
        Assert.Equal(4245, TimeUtils.ParseDuration("424562ns").Ticks);
        Assert.Equal(4245, TimeUtils.ParseDuration("424562nano").Ticks);
        Assert.Equal(4245, TimeUtils.ParseDuration("424562nanos").Ticks);
        Assert.Equal(4245, TimeUtils.ParseDuration("424562nanosecond").Ticks);
        Assert.Equal(4245, TimeUtils.ParseDuration("424562nanoseconds").Ticks);
        Assert.Equal(4245, TimeUtils.ParseDuration("424562 ns").Ticks);
        // Java can represent Duration.ofMillis(long.MaxValue).plusNanos(1); the TimeSpan range
        // is smaller, so this is a numeric overflow in the port.
        Assert.Throws<ArgumentException>(
            () => TimeUtils.ParseDuration("9223372036854775807000001 ns"));
    }

    [Fact]
    public void TestParseDurationMicros()
    {
        Assert.Equal(TimeSpan.FromMicroseconds(565731), TimeUtils.ParseDuration("565731µs"));
        Assert.Equal(TimeSpan.FromMicroseconds(565731), TimeUtils.ParseDuration("565731micro"));
        Assert.Equal(TimeSpan.FromMicroseconds(565731), TimeUtils.ParseDuration("565731micros"));
        Assert.Equal(TimeSpan.FromMicroseconds(565731), TimeUtils.ParseDuration("565731microsecond"));
        Assert.Equal(TimeSpan.FromMicroseconds(565731), TimeUtils.ParseDuration("565731microseconds"));
        Assert.Equal(TimeSpan.FromMicroseconds(565731), TimeUtils.ParseDuration("565731 µs"));
    }

    [Fact]
    public void TestParseDurationMillis()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(1234), TimeUtils.ParseDuration("1234"));
        Assert.Equal(TimeSpan.FromMilliseconds(1234), TimeUtils.ParseDuration("1234ms"));
        Assert.Equal(TimeSpan.FromMilliseconds(1234), TimeUtils.ParseDuration("1234milli"));
        Assert.Equal(TimeSpan.FromMilliseconds(1234), TimeUtils.ParseDuration("1234millis"));
        Assert.Equal(TimeSpan.FromMilliseconds(1234), TimeUtils.ParseDuration("1234millisecond"));
        Assert.Equal(TimeSpan.FromMilliseconds(1234), TimeUtils.ParseDuration("1234milliseconds"));
        Assert.Equal(TimeSpan.FromMilliseconds(1234), TimeUtils.ParseDuration("1234 ms"));
    }

    [Fact]
    public void TestParseDurationSeconds()
    {
        Assert.Equal(TimeSpan.FromSeconds(667766), TimeUtils.ParseDuration("667766s"));
        Assert.Equal(TimeSpan.FromSeconds(667766), TimeUtils.ParseDuration("667766sec"));
        Assert.Equal(TimeSpan.FromSeconds(667766), TimeUtils.ParseDuration("667766secs"));
        Assert.Equal(TimeSpan.FromSeconds(667766), TimeUtils.ParseDuration("667766second"));
        Assert.Equal(TimeSpan.FromSeconds(667766), TimeUtils.ParseDuration("667766seconds"));
        Assert.Equal(TimeSpan.FromSeconds(667766), TimeUtils.ParseDuration("667766 s"));
    }

    [Fact]
    public void TestParseDurationMinutes()
    {
        Assert.Equal(TimeSpan.FromMinutes(7657623), TimeUtils.ParseDuration("7657623m"));
        Assert.Equal(TimeSpan.FromMinutes(7657623), TimeUtils.ParseDuration("7657623min"));
        Assert.Equal(TimeSpan.FromMinutes(7657623), TimeUtils.ParseDuration("7657623minute"));
        Assert.Equal(TimeSpan.FromMinutes(7657623), TimeUtils.ParseDuration("7657623minutes"));
        Assert.Equal(TimeSpan.FromMinutes(7657623), TimeUtils.ParseDuration("7657623 min"));
    }

    [Fact]
    public void TestParseDurationHours()
    {
        Assert.Equal(TimeSpan.FromHours(987654), TimeUtils.ParseDuration("987654h"));
        Assert.Equal(TimeSpan.FromHours(987654), TimeUtils.ParseDuration("987654hour"));
        Assert.Equal(TimeSpan.FromHours(987654), TimeUtils.ParseDuration("987654hours"));
        Assert.Equal(TimeSpan.FromHours(987654), TimeUtils.ParseDuration("987654 h"));
    }

    [Fact]
    public void TestParseDurationDays()
    {
        Assert.Equal(TimeSpan.FromDays(987654), TimeUtils.ParseDuration("987654d"));
        Assert.Equal(TimeSpan.FromDays(987654), TimeUtils.ParseDuration("987654day"));
        Assert.Equal(TimeSpan.FromDays(987654), TimeUtils.ParseDuration("987654days"));
        Assert.Equal(TimeSpan.FromDays(987654), TimeUtils.ParseDuration("987654 d"));
    }

    [Fact]
    public void TestParseDurationUpperCase()
    {
        // 1ns is below tick resolution and truncates to zero (PORT NOTE above)
        Assert.Equal(TimeSpan.Zero, TimeUtils.ParseDuration("1 NS"));
        Assert.Equal(10, TimeUtils.ParseDuration("1 MICRO").Ticks);
        Assert.Equal(TimeSpan.FromMilliseconds(1), TimeUtils.ParseDuration("1 MS"));
        Assert.Equal(TimeSpan.FromSeconds(1), TimeUtils.ParseDuration("1 S"));
        Assert.Equal(TimeSpan.FromMinutes(1), TimeUtils.ParseDuration("1 MIN"));
        Assert.Equal(TimeSpan.FromHours(1), TimeUtils.ParseDuration("1 H"));
        Assert.Equal(TimeSpan.FromDays(1), TimeUtils.ParseDuration("1 D"));
    }

    [Fact]
    public void TestParseDurationTrim()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(155), TimeUtils.ParseDuration("      155      "));
        Assert.Equal(TimeSpan.FromMilliseconds(155), TimeUtils.ParseDuration("      155      ms   "));
    }

    [Fact]
    public void TestParseDurationIso8601()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(20345), TimeUtils.ParseDuration("PT20.345S"));
        Assert.Equal(TimeSpan.FromMinutes(15), TimeUtils.ParseDuration("PT15M"));
        Assert.Equal(TimeSpan.FromHours(10), TimeUtils.ParseDuration("PT10H"));
        Assert.Equal(TimeSpan.FromMinutes(3064), TimeUtils.ParseDuration("P2DT3H4M"));
    }

    [Fact]
    public void TestParseDurationInvalid()
    {
        // null
        Assert.Throws<ArgumentNullException>(() => TimeUtils.ParseDuration(null!));

        // empty
        Assert.Throws<ArgumentException>(() => TimeUtils.ParseDuration(""));

        // blank
        Assert.Throws<ArgumentException>(() => TimeUtils.ParseDuration("     "));

        // no number (Java throws NumberFormatException, which maps to FormatException)
        Assert.Throws<FormatException>(() => TimeUtils.ParseDuration("foobar or fubar or foo bazz"));

        // wrong unit
        Assert.Throws<ArgumentException>(() => TimeUtils.ParseDuration("16 gjah"));

        // multiple numbers
        Assert.Throws<ArgumentException>(() => TimeUtils.ParseDuration("16 16 17 18 ms"));

        // negative number
        Assert.Throws<FormatException>(() => TimeUtils.ParseDuration("-100 ms"));

        // negative ISO-8601
        Assert.Throws<FormatException>(() => TimeUtils.ParseDuration("-PT6H3M"));
    }

    [Fact]
    public void TestParseDurationNumberOverflow()
    {
        Assert.Throws<ArgumentException>(
            () => TimeUtils.ParseDuration("100000000000000000000000000000000 ms"));
    }

    [Fact]
    public void TestGetStringInMillis()
    {
        Assert.Equal("4567ms", TimeUtils.GetStringInMillis(TimeSpan.FromMilliseconds(4567L)));
        Assert.Equal("4567000ms", TimeUtils.GetStringInMillis(TimeSpan.FromSeconds(4567L)));
        Assert.Equal("4ms", TimeUtils.GetStringInMillis(TimeSpan.FromMicroseconds(4567L)));
    }
}
