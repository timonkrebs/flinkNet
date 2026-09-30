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

using FlinkNet.Configuration;
using Xunit;

namespace FlinkNet.Tests.Configuration;

/// <summary>Tests for the <see cref="MemorySize"/> class.</summary>
public class MemorySizeTest
{
    [Fact]
    public void TestUnitConversion()
    {
        MemorySize zero = MemorySize.Zero;
        Assert.Equal(0, zero.Bytes);
        Assert.Equal(0, zero.KibiBytes);
        Assert.Equal(0, zero.MebiBytes);
        Assert.Equal(0, zero.GibiBytes);
        Assert.Equal(0, zero.TebiBytes);

        var bytes = new MemorySize(955);
        Assert.Equal(955, bytes.Bytes);
        Assert.Equal(0, bytes.KibiBytes);
        Assert.Equal(0, bytes.MebiBytes);
        Assert.Equal(0, bytes.GibiBytes);
        Assert.Equal(0, bytes.TebiBytes);

        var kilos = new MemorySize(18500);
        Assert.Equal(18500, kilos.Bytes);
        Assert.Equal(18, kilos.KibiBytes);
        Assert.Equal(0, kilos.MebiBytes);
        Assert.Equal(0, kilos.GibiBytes);
        Assert.Equal(0, kilos.TebiBytes);

        var megas = new MemorySize(15 * 1024 * 1024);
        Assert.Equal(15_728_640, megas.Bytes);
        Assert.Equal(15_360, megas.KibiBytes);
        Assert.Equal(15, megas.MebiBytes);
        Assert.Equal(0, megas.GibiBytes);
        Assert.Equal(0, megas.TebiBytes);

        var teras = new MemorySize(2L * 1024 * 1024 * 1024 * 1024 + 10);
        Assert.Equal(2199023255562L, teras.Bytes);
        Assert.Equal(2147483648L, teras.KibiBytes);
        Assert.Equal(2097152, teras.MebiBytes);
        Assert.Equal(2048, teras.GibiBytes);
        Assert.Equal(2, teras.TebiBytes);
    }

    [Fact]
    public void TestInvalid()
    {
        Assert.Throws<ArgumentException>(() => new MemorySize(-1));
    }

    [Fact]
    public void TestStandardUtils()
    {
        // PORT NOTE: the Java test round-trips through Java serialization; the port verifies the
        // same equality/hashCode/toString contract on an independently constructed instance.
        var size = new MemorySize(1234567890L);
        var copy = new MemorySize(1234567890L);

        Assert.Equal(size, copy);
        Assert.Equal(size.GetHashCode(), copy.GetHashCode());
        Assert.Equal(size.ToString(), copy.ToString());
    }

    [Fact]
    public void TestParseBytes()
    {
        Assert.Equal(1234, MemorySize.ParseBytes("1234"));
        Assert.Equal(1234, MemorySize.ParseBytes("1234b"));
        Assert.Equal(1234, MemorySize.ParseBytes("1234 b"));
        Assert.Equal(1234, MemorySize.ParseBytes("1234bytes"));
        Assert.Equal(1234, MemorySize.ParseBytes("1234 bytes"));
    }

    [Fact]
    public void TestParseKibiBytes()
    {
        Assert.Equal(667766, MemorySize.Parse("667766k").KibiBytes);
        Assert.Equal(667766, MemorySize.Parse("667766 k").KibiBytes);
        Assert.Equal(667766, MemorySize.Parse("667766kb").KibiBytes);
        Assert.Equal(667766, MemorySize.Parse("667766 kb").KibiBytes);
        Assert.Equal(667766, MemorySize.Parse("667766kibibytes").KibiBytes);
        Assert.Equal(667766, MemorySize.Parse("667766 kibibytes").KibiBytes);
    }

    [Fact]
    public void TestParseMebiBytes()
    {
        Assert.Equal(7657623, MemorySize.Parse("7657623m").MebiBytes);
        Assert.Equal(7657623, MemorySize.Parse("7657623 m").MebiBytes);
        Assert.Equal(7657623, MemorySize.Parse("7657623mb").MebiBytes);
        Assert.Equal(7657623, MemorySize.Parse("7657623 mb").MebiBytes);
        Assert.Equal(7657623, MemorySize.Parse("7657623mebibytes").MebiBytes);
        Assert.Equal(7657623, MemorySize.Parse("7657623 mebibytes").MebiBytes);
    }

    [Fact]
    public void TestParseGibiBytes()
    {
        Assert.Equal(987654, MemorySize.Parse("987654g").GibiBytes);
        Assert.Equal(987654, MemorySize.Parse("987654 g").GibiBytes);
        Assert.Equal(987654, MemorySize.Parse("987654gb").GibiBytes);
        Assert.Equal(987654, MemorySize.Parse("987654 gb").GibiBytes);
        Assert.Equal(987654, MemorySize.Parse("987654gibibytes").GibiBytes);
        Assert.Equal(987654, MemorySize.Parse("987654 gibibytes").GibiBytes);
    }

    [Fact]
    public void TestParseTebiBytes()
    {
        Assert.Equal(1234567, MemorySize.Parse("1234567t").TebiBytes);
        Assert.Equal(1234567, MemorySize.Parse("1234567 t").TebiBytes);
        Assert.Equal(1234567, MemorySize.Parse("1234567tb").TebiBytes);
        Assert.Equal(1234567, MemorySize.Parse("1234567 tb").TebiBytes);
        Assert.Equal(1234567, MemorySize.Parse("1234567tebibytes").TebiBytes);
        Assert.Equal(1234567, MemorySize.Parse("1234567 tebibytes").TebiBytes);
    }

    [Fact]
    public void TestUpperCase()
    {
        Assert.Equal(1, MemorySize.Parse("1 B").Bytes);
        Assert.Equal(1, MemorySize.Parse("1 K").KibiBytes);
        Assert.Equal(1, MemorySize.Parse("1 M").MebiBytes);
        Assert.Equal(1, MemorySize.Parse("1 G").GibiBytes);
        Assert.Equal(1, MemorySize.Parse("1 T").TebiBytes);
    }

    [Fact]
    public void TestTrimBeforeParse()
    {
        Assert.Equal(155L, MemorySize.ParseBytes("      155      "));
        Assert.Equal(155L, MemorySize.ParseBytes("      155      bytes   "));
    }

    [Fact]
    public void TestParseInvalid()
    {
        // null
        Assert.Throws<ArgumentNullException>(() => MemorySize.ParseBytes(null!));

        // empty
        Assert.Throws<ArgumentException>(() => MemorySize.ParseBytes(""));

        // blank
        Assert.Throws<ArgumentException>(() => MemorySize.ParseBytes("     "));

        // no number (Java throws NumberFormatException, which maps to FormatException)
        Assert.Throws<FormatException>(() => MemorySize.ParseBytes("foobar or fubar or foo bazz"));

        // wrong unit
        Assert.Throws<ArgumentException>(() => MemorySize.ParseBytes("16 gjah"));

        // multiple numbers
        Assert.Throws<ArgumentException>(() => MemorySize.ParseBytes("16 16 17 18 bytes"));

        // negative number (the sign is not consumed by the digit scanner, so no number is found)
        Assert.Throws<FormatException>(() => MemorySize.ParseBytes("-100 bytes"));
    }

    [Fact]
    public void TestParseNumberOverflow()
    {
        Assert.Throws<ArgumentException>(
            () => MemorySize.ParseBytes("100000000000000000000000000000000 bytes"));
    }

    [Fact]
    public void TestParseNumberTimeUnitOverflow()
    {
        Assert.Throws<ArgumentException>(() => MemorySize.ParseBytes("100000000000000 tb"));
    }

    [Fact]
    public void TestParseWithDefaultUnit()
    {
        Assert.Equal(7, MemorySize.Parse("7", MemorySize.MemoryUnit.MegaBytes).MebiBytes);
        Assert.False(MemorySize.Parse("7340032", MemorySize.MemoryUnit.MegaBytes).Equals(7));
        Assert.Equal(7, MemorySize.Parse("7m", MemorySize.MemoryUnit.MegaBytes).MebiBytes);
        Assert.Equal(7168, MemorySize.Parse("7", MemorySize.MemoryUnit.MegaBytes).KibiBytes);
        Assert.Equal(7168, MemorySize.Parse("7m", MemorySize.MemoryUnit.MegaBytes).KibiBytes);
        Assert.Equal(7, MemorySize.Parse("7 m", MemorySize.MemoryUnit.MegaBytes).MebiBytes);
        Assert.Equal(7, MemorySize.Parse("7mb", MemorySize.MemoryUnit.MegaBytes).MebiBytes);
        Assert.Equal(7, MemorySize.Parse("7 mb", MemorySize.MemoryUnit.MegaBytes).MebiBytes);
        Assert.Equal(7, MemorySize.Parse("7mebibytes", MemorySize.MemoryUnit.MegaBytes).MebiBytes);
        Assert.Equal(7, MemorySize.Parse("7 mebibytes", MemorySize.MemoryUnit.MegaBytes).MebiBytes);
    }

    [Fact]
    public void TestDivideByLong()
    {
        var memory = new MemorySize(100L);
        Assert.Equal(new MemorySize(4L), memory.Divide(23));
    }

    [Fact]
    public void TestDivideByNegativeLong()
    {
        var memory = new MemorySize(100L);
        Assert.Throws<ArgumentException>(() => memory.Divide(-23L));
    }

    [Fact]
    public void TestToHumanReadableString()
    {
        Assert.Equal("0 bytes", new MemorySize(0L).ToHumanReadableString());
        Assert.Equal("1 bytes", new MemorySize(1L).ToHumanReadableString());
        Assert.Equal("1024 bytes", new MemorySize(1024L).ToHumanReadableString());
        Assert.Equal("1.001kb (1025 bytes)", new MemorySize(1025L).ToHumanReadableString());
        Assert.Equal("1.500kb (1536 bytes)", new MemorySize(1536L).ToHumanReadableString());
        Assert.Equal("976.563kb (1000000 bytes)", new MemorySize(1_000_000L).ToHumanReadableString());
        Assert.Equal("953.674mb (1000000000 bytes)", new MemorySize(1_000_000_000L).ToHumanReadableString());
        Assert.Equal("931.323gb (1000000000000 bytes)", new MemorySize(1_000_000_000_000L).ToHumanReadableString());
        Assert.Equal("909.495tb (1000000000000000 bytes)", new MemorySize(1_000_000_000_000_000L).ToHumanReadableString());
    }
}
