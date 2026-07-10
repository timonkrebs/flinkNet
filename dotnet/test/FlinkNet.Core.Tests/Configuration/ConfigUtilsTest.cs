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

/// <summary>Tests the <see cref="ConfigUtils"/> methods.</summary>
public class ConfigUtilsTest
{
    private static readonly ConfigOption<IList<string>> TestOption =
        ConfigOptions.Key("test.option.key").StringType().AsList().NoDefaultValue();

    private static readonly int?[] IntArray = [1, 3, 2, 4];
    private static readonly List<int?> IntList = [.. IntArray];

    [Fact]
    public void CollectionIsCorrectlyPutAndFetched()
    {
        var configurationUnderTest = new FlinkNet.Configuration.Configuration();
        ConfigUtils.EncodeCollectionToConfig(
            configurationUnderTest, TestOption, IntList, v => v!.ToString());

        IList<int?> recovered =
            ConfigUtils.DecodeListFromConfig(
                configurationUnderTest, TestOption, v => (int?)int.Parse(v));
        Assert.Equal(IntList, recovered);
    }

    [Fact]
    public void ArrayIsCorrectlyPutAndFetched()
    {
        var configurationUnderTest = new FlinkNet.Configuration.Configuration();
        ConfigUtils.EncodeArrayToConfig(
            configurationUnderTest, TestOption, IntArray, v => v!.ToString());

        IList<int?> recovered =
            ConfigUtils.DecodeListFromConfig(
                configurationUnderTest, TestOption, v => (int?)int.Parse(v));
        Assert.Equal(IntList, recovered);
    }

    [Fact]
    public void NullCollectionPutsNothingInConfig()
    {
        var configurationUnderTest = new FlinkNet.Configuration.Configuration();
        ConfigUtils.EncodeCollectionToConfig(
            configurationUnderTest, TestOption, (ICollection<int?>?)null, v => v!.ToString());

        Assert.Empty(configurationUnderTest.KeySet());

        object? recovered = configurationUnderTest.Get(TestOption);
        Assert.Null(recovered);

        IList<int> recoveredList =
            ConfigUtils.DecodeListFromConfig(configurationUnderTest, TestOption, int.Parse);
        Assert.Empty(recoveredList);
    }

    [Fact]
    public void NullArrayPutsNothingInConfig()
    {
        var configurationUnderTest = new FlinkNet.Configuration.Configuration();
        ConfigUtils.EncodeArrayToConfig(
            configurationUnderTest, TestOption, (int?[]?)null, v => v!.ToString());

        Assert.Empty(configurationUnderTest.KeySet());

        object? recovered = configurationUnderTest.Get(TestOption);
        Assert.Null(recovered);

        IList<int> recoveredList =
            ConfigUtils.DecodeListFromConfig(configurationUnderTest, TestOption, int.Parse);
        Assert.Empty(recoveredList);
    }

    [Fact]
    public void EmptyCollectionPutsNothingInConfig()
    {
        var configurationUnderTest = new FlinkNet.Configuration.Configuration();
        ConfigUtils.EncodeCollectionToConfig(
            configurationUnderTest, TestOption, new List<int?>(), v => v!.ToString());

        IList<string>? recovered = configurationUnderTest.Get(TestOption);
        Assert.Null(recovered);

        IList<int> recoveredList =
            ConfigUtils.DecodeListFromConfig(configurationUnderTest, TestOption, int.Parse);
        Assert.Empty(recoveredList);
    }

    [Fact]
    public void EmptyArrayPutsNothingInConfig()
    {
        var configurationUnderTest = new FlinkNet.Configuration.Configuration();
        ConfigUtils.EncodeArrayToConfig(
            configurationUnderTest, TestOption, new int?[5], v => v!.ToString());

        IList<string>? recovered = configurationUnderTest.Get(TestOption);
        Assert.Null(recovered);

        IList<int> recoveredList =
            ConfigUtils.DecodeListFromConfig(configurationUnderTest, TestOption, int.Parse);
        Assert.Empty(recoveredList);
    }
}
