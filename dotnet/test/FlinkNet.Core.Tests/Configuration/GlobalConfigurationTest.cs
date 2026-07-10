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

/// <summary>
/// This class contains tests for the global configuration (parsing the standard YAML
/// configuration file).
/// </summary>
public class GlobalConfigurationTest : IDisposable
{
    private readonly string _tmpDir =
        Directory.CreateTempSubdirectory("flinknet-globalconf-test").FullName;

    public void Dispose() => Directory.Delete(_tmpDir, recursive: true);

    [Fact]
    public void TestConfigurationWithStandardYaml()
    {
        string confFile = Path.Combine(_tmpDir, GlobalConfiguration.FlinkConfFilename);
        File.WriteAllLines(
            confFile,
            [
                "Key1: ",
                "    Key2: v1",
                "    Key3: 'v2'",
                "Key4: 1",
                "Key5: '1'",
                "Key6: '*'",
                "Key7: true",
                "Key8: 'true'",
                "Key9: [a, b, '*', 1, '2',  true, 'true']",
                "Key10: {k1: v1, k2: '2', k3: 3}",
                "Key11: [{k1: v1, k2: '2', k3: 3}, {k4: true}]",
            ]);

        var conf = GlobalConfiguration.LoadConfiguration(_tmpDir);

        Assert.Equal(12, conf.KeySet().Count);

        Assert.Equal("v1", conf.Get(ConfigOptions.Key("Key1.Key2").StringType().NoDefaultValue()));
        Assert.Equal("v2", conf.Get(ConfigOptions.Key("Key1.Key3").StringType().NoDefaultValue()));
        Assert.Equal(1, conf.Get(ConfigOptions.Key("Key4").IntType().NoDefaultValue()));
        Assert.Equal("1", conf.Get(ConfigOptions.Key("Key5").StringType().NoDefaultValue()));
        Assert.Equal("*", conf.Get(ConfigOptions.Key("Key6").StringType().NoDefaultValue()));
        Assert.True(conf.Get(ConfigOptions.Key("Key7").BooleanType().NoDefaultValue()));
        Assert.Equal("true", conf.Get(ConfigOptions.Key("Key8").StringType().NoDefaultValue()));
        Assert.Equal(
            new List<string> { "a", "b", "*", "1", "2", "true", "true" },
            conf.Get(ConfigOptions.Key("Key9").StringType().AsList().NoDefaultValue()));

        var map = new Dictionary<string, string> { { "k1", "v1" }, { "k2", "2" }, { "k3", "3" } };
        Assert.Equal(map, conf.Get(ConfigOptions.Key("Key10").MapType().NoDefaultValue()));

        var map2 = new Dictionary<string, string> { { "k4", "true" } };
        Assert.Equal(
            new List<IDictionary<string, string>> { map, map2 },
            conf.Get(ConfigOptions.Key("Key11").MapType().AsList().NoDefaultValue()));
    }

    [Fact]
    public void TestFailIfNull()
    {
        Assert.Throws<ArgumentException>(() => GlobalConfiguration.LoadConfiguration((string)null!));
    }

    [Fact]
    public void TestFailIfNotLoaded()
    {
        Assert.Throws<IllegalConfigurationException>(
            () => GlobalConfiguration.LoadConfiguration("/some/path/" + Guid.NewGuid()));
    }

    [Fact]
    public void TestInvalidConfiguration()
    {
        Assert.Throws<IllegalConfigurationException>(
            () => GlobalConfiguration.LoadConfiguration(_tmpDir));
    }

    [Fact]
    public void TestInvalidStandardYamlFile()
    {
        // We do not allow malformed YAML files if loaded standard yaml
        string confFile = Path.Combine(_tmpDir, GlobalConfiguration.FlinkConfFilename);
        File.WriteAllText(confFile, "invalid");

        var e = Assert.Throws<InvalidOperationException>(
            () => GlobalConfiguration.LoadConfiguration(_tmpDir));
        Assert.Equal("Error parsing YAML configuration.", e.Message);
        Assert.NotNull(e.InnerException);
        Assert.IsType<InvalidCastException>(e.InnerException);
        // the cast error names the offending types, mirroring Java's ClassCastException message
        Assert.Contains("String", e.InnerException.Message);
        Assert.Contains("IDictionary", e.InnerException.Message);
    }

    [Fact]
    public void TestHiddenKey()
    {
        Assert.True(GlobalConfiguration.IsSensitive("password123", []));
        Assert.True(GlobalConfiguration.IsSensitive("123pasSword", []));
        Assert.True(GlobalConfiguration.IsSensitive("PasSword", []));
        Assert.True(GlobalConfiguration.IsSensitive("Secret", []));
        Assert.True(GlobalConfiguration.IsSensitive("polaris.client-secret", []));
        Assert.True(GlobalConfiguration.IsSensitive("client-secret", []));
        Assert.True(GlobalConfiguration.IsSensitive("service-key-json", []));
        Assert.True(GlobalConfiguration.IsSensitive("auth.basic.password", []));
        Assert.True(GlobalConfiguration.IsSensitive("auth.basic.token", []));
        Assert.True(GlobalConfiguration.IsSensitive("avro-confluent.basic-auth.user-info", []));
        Assert.True(GlobalConfiguration.IsSensitive("key.avro-confluent.basic-auth.user-info", []));
        Assert.True(
            GlobalConfiguration.IsSensitive("value.avro-confluent.basic-auth.user-info", []));
        Assert.True(GlobalConfiguration.IsSensitive("kafka.jaas.config", []));
        Assert.True(GlobalConfiguration.IsSensitive("properties.ssl.truststore.password", []));
        Assert.True(GlobalConfiguration.IsSensitive("properties.ssl.keystore.password", []));
        Assert.True(
            GlobalConfiguration.IsSensitive(
                "fs.azure.account.key.storageaccount123456.core.windows.net", []));
        Assert.False(GlobalConfiguration.IsSensitive("Hello", []));
        Assert.True(GlobalConfiguration.IsSensitive("metrics.reporter.dghttp.apikey", []));

        // access-key / access.key / accesskey patterns
        Assert.True(GlobalConfiguration.IsSensitive("s3.access-key", []));
        Assert.True(GlobalConfiguration.IsSensitive("fs.s3a.access.key", []));
        Assert.True(GlobalConfiguration.IsSensitive("s3.access.key", []));
        Assert.True(GlobalConfiguration.IsSensitive("fs.oss.accessKeyId", []));
        Assert.True(GlobalConfiguration.IsSensitive("fs.oss.accesskey", []));
    }

    [Fact]
    public void TestAdditionalSensitiveKeys()
    {
        Assert.True(
            GlobalConfiguration.IsSensitive(
                "my.custom.credential", ["my.custom.credential", "VENDOR_TOKEN_ID"]));
        Assert.True(
            GlobalConfiguration.IsSensitive(
                "prefix.my.custom.credential.suffix", ["my.custom.credential"]));
        // case-insensitive matching
        Assert.True(GlobalConfiguration.IsSensitive("vendor_token_id", ["VENDOR_TOKEN_ID"]));
        // built-in keys are unaffected when additional list is empty
        Assert.True(GlobalConfiguration.IsSensitive("password", []));
        // unrelated key not matched
        Assert.False(GlobalConfiguration.IsSensitive("unrelated.key", ["my.custom.credential"]));
    }
}
