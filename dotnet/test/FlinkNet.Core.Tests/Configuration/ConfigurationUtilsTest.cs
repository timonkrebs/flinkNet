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

/// <summary>Tests for the value conversions of <see cref="ConfigurationUtils"/>.</summary>
public class ConfigurationUtilsTest
{
    /// <summary>
    /// String values convert with Java's <c>Double.parseDouble</c>/<c>Float.parseFloat</c>
    /// grammar (outcomes verified on a JDK): a grouping comma is rejected rather than read as
    /// "15", while Java's type suffixes and hexadecimal floats are accepted.
    /// </summary>
    [Fact]
    public void TestFloatingPointStringsParseLikeJava()
    {
        Assert.Throws<FormatException>(() => ConfigurationUtils.ConvertToDouble("1,5"));
        Assert.Throws<FormatException>(() => ConfigurationUtils.ConvertToFloat("1,5"));
        Assert.Throws<FormatException>(() => ConfigurationUtils.ConvertToDouble("1,000.5"));

        Assert.Equal(1.5, ConfigurationUtils.ConvertToDouble(" 1.5 "));
        Assert.Equal(1.5, ConfigurationUtils.ConvertToDouble("1.5d"));
        Assert.Equal(1.5f, ConfigurationUtils.ConvertToFloat("1.5F"));
        Assert.Equal(8.0, ConfigurationUtils.ConvertToDouble("0x1p3"));
        Assert.Equal(1000f, ConfigurationUtils.ConvertToFloat("1e3"));
    }
}
