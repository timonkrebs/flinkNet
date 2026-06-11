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

using System.Text.RegularExpressions;
using FlinkNet.Util;
using Xunit;

namespace FlinkNet.Tests.Util;

/// <summary>Tests for the <see cref="StringUtils"/>.</summary>
public class StringUtilsTest
{
    [Fact]
    public void TestControlCharacters()
    {
        const string testString = "\b \t \n \f \r default";
        string controlString = StringUtils.ShowControlCharacters(testString);
        Assert.Equal("\\b \\t \\n \\f \\r default", controlString);
    }

    [Fact]
    public void TestArrayAwareToString()
    {
        Assert.Equal("null", StringUtils.ArrayAwareToString(null));

        // PORT NOTE: .NET enum members render with their C# names ("Monday" vs Java's "MONDAY")
        Assert.Equal("Monday", StringUtils.ArrayAwareToString(DayOfWeek.Monday));

        Assert.Equal("[1, 2, 3]", StringUtils.ArrayAwareToString(new[] { 1, 2, 3 }));

        Assert.Equal(
            "[[4, 5, 6], null, []]",
            StringUtils.ArrayAwareToString(new byte[]?[] { [4, 5, 6], null, [] }));

        Assert.Equal(
            "[[4, 5, 6], null, Monday]",
            StringUtils.ArrayAwareToString(
                new object?[] { new int[] { 4, 5, 6 }, null, DayOfWeek.Monday }));
    }

    [Fact]
    public void TestStringToHexArray()
    {
        const string hex = "019f314a";
        byte[] hexArray = StringUtils.HexStringToByte(hex);
        byte[] expectedArray = [0x01, 0x9f, 0x31, 0x4a];
        Assert.Equal(expectedArray, hexArray);
    }

    [Fact]
    public void TestHexArrayToString()
    {
        byte[] byteArray = [0x01, 0x9f, 0x31, 0x4a];
        string hex = StringUtils.ByteToHexString(byteArray);
        Assert.Equal("019f314a", hex);
    }

    [Fact]
    public void TestGenerateAlphanumeric()
    {
        string str = StringUtils.GenerateRandomAlphanumericString(new Random(), 256);

        Assert.Matches(new Regex("^[a-zA-Z0-9]{256}$"), str);
    }
}
