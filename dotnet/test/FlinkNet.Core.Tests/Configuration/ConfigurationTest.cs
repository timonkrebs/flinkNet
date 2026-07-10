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
using static FlinkNet.Configuration.ConfigurationUtils;

namespace FlinkNet.Tests.Configuration;

/// <summary>
/// This class contains tests for the configuration package.
///
/// <para>PORT NOTES: the Java serialization round-trip goes through
/// <c>IIOReadableWritable.Write</c>/<c>Read</c> directly instead of
/// <c>InstantiationUtil.createCopyWritable</c>; <c>toMap</c> expectations use the legacy Flink
/// list/map format instead of standard YAML (see ConfigurationUtils PORT NOTE);
/// <c>testToFileWritableMap</c> is deferred together with <c>YamlParserUtils</c>.</para>
/// </summary>
public class ConfigurationTest
{
    private static readonly ConfigOption<string> StringOption =
        ConfigOptions.Key("test-string-key").StringType().NoDefaultValue();

    private static readonly ConfigOption<IList<string>> ListStringOption =
        ConfigOptions.Key("test-list-key").StringType().AsList().NoDefaultValue();

    private static readonly ConfigOption<IDictionary<string, string>> MapOption =
        ConfigOptions.Key("test-map-key").MapType().NoDefaultValue();

    private static readonly ConfigOption<TimeSpan> DurationOption =
        ConfigOptions.Key("test-duration-key").DurationType().NoDefaultValue();

    private static readonly Dictionary<string, string> PropertiesMap =
        new() { { "prop1", "value1" }, { "prop2", "12" } };

    private static readonly string MapProperty1 = MapOption.Key + ".prop1";

    private static readonly string MapProperty2 = MapOption.Key + ".prop2";

    /// <summary>This test checks the copying of configuration objects and the getters.</summary>
    [Fact]
    public void TestConfigurationCopyAndGetters()
    {
        var orig = new FlinkNet.Configuration.Configuration();
        orig.SetString("mykey", "myvalue");
        orig.Set(GetIntConfigOption("mynumber"), 100);
        orig.Set(GetLongConfigOption("longvalue"), 478236947162389746L);
        orig.Set(GetFloatConfigOption("PI"), 3.1415926f);
        orig.Set(GetDoubleConfigOption("E"), Math.E);
        orig.Set(GetBooleanConfigOption("shouldbetrue"), true);
        orig.SetBytes("bytes sequence", [1, 2, 3, 4, 5]);

        var copy = new FlinkNet.Configuration.Configuration(orig);
        Assert.Equal("myvalue", copy.GetString("mykey", "null"));
        Assert.Equal(100, copy.Get(GetIntConfigOption("mynumber"), 0));
        Assert.Equal(478236947162389746L, copy.Get(GetLongConfigOption("longvalue"), 0L));
        Assert.Equal(3.1415926f, copy.Get(GetFloatConfigOption("PI"), 3.1415926f), 0.0f);
        Assert.Equal(Math.E, copy.Get(GetDoubleConfigOption("E"), 0.0), 0.0);
        Assert.True(copy.Get(GetBooleanConfigOption("shouldbetrue"), false));
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, copy.GetBytes("bytes sequence", null));

        Assert.Equal(orig, copy);
        Assert.Equal(orig.KeySet(), copy.KeySet());
        Assert.Equal(orig.GetHashCode(), copy.GetHashCode());
    }

    /// <summary>This test checks the serialization of configuration objects and the getters,
    /// mirroring Java's <c>testConfigurationSerializationAndGetters</c> with the binary
    /// <c>Write</c>/<c>Read</c> round-trip.</summary>
    [Fact]
    public void TestConfigurationSerializationAndGetters()
    {
        var orig = new FlinkNet.Configuration.Configuration();
        orig.SetString("mykey", "myvalue");
        orig.Set(GetIntConfigOption("mynumber"), 100);
        orig.Set(GetLongConfigOption("longvalue"), 478236947162389746L);
        orig.Set(GetFloatConfigOption("PI"), 3.1415926f);
        orig.Set(GetDoubleConfigOption("E"), Math.E);
        orig.Set(GetBooleanConfigOption("shouldbetrue"), true);
        orig.SetBytes("bytes sequence", [1, 2, 3, 4, 5]);

        var output = new FlinkNet.Core.Memory.DataOutputSerializer(64);
        orig.Write(output);

        var copy = new FlinkNet.Configuration.Configuration();
        copy.Read(new FlinkNet.Core.Memory.DataInputDeserializer(output.GetCopyOfBuffer()));

        Assert.Equal("myvalue", copy.GetString("mykey", "null"));
        Assert.Equal(100, copy.Get(GetIntConfigOption("mynumber"), 0));
        Assert.Equal(478236947162389746L, copy.Get(GetLongConfigOption("longvalue"), 0L));
        Assert.Equal(3.1415926f, copy.Get(GetFloatConfigOption("PI"), 3.1415926f), 0.0f);
        Assert.Equal(Math.E, copy.Get(GetDoubleConfigOption("E"), 0.0), 0.0);
        Assert.True(copy.Get(GetBooleanConfigOption("shouldbetrue"), false));
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, copy.GetBytes("bytes sequence", null));

        Assert.Equal(orig, copy);
        Assert.Equal(orig.KeySet(), copy.KeySet());
        Assert.Equal(orig.GetHashCode(), copy.GetHashCode());
    }

    [Fact]
    public void TestCopyConstructor()
    {
        const string key = "theKey";

        var cfg1 = new FlinkNet.Configuration.Configuration();
        cfg1.SetString(key, "value");

        var cfg2 = new FlinkNet.Configuration.Configuration(cfg1);
        cfg2.SetString(key, "another value");

        Assert.Equal("value", cfg1.GetString(key, ""));
    }

    /// <summary>PORT NOTE: Java's <c>equals</c> lacks the size check, so a configuration
    /// compares equal to any superset of itself (asymmetrically); the port restores the
    /// equals contract (see Configuration.Equals).</summary>
    [Fact]
    public void TestEqualsIsSymmetric()
    {
        var small = new FlinkNet.Configuration.Configuration();
        small.SetString("key1", "value1");

        var big = new FlinkNet.Configuration.Configuration();
        big.SetString("key1", "value1");
        big.SetString("key2", "value2");

        Assert.False(small.Equals(big));
        Assert.False(big.Equals(small));
    }

    /// <summary>Java compares stored List/Map values structurally via
    /// <c>Object.equals</c>; the port must not fall back to reference equality.</summary>
    [Fact]
    public void TestEqualsComparesCollectionValuesStructurally()
    {
        var first = new FlinkNet.Configuration.Configuration();
        first.Set(ListStringOption, new List<string> { "one", "two" });
        first.Set(MapOption, new Dictionary<string, string> { { "k", "v" } });

        var second = new FlinkNet.Configuration.Configuration();
        second.Set(ListStringOption, new List<string> { "one", "two" });
        second.Set(MapOption, new Dictionary<string, string> { { "k", "v" } });

        Assert.Equal(first, second);

        second.Set(ListStringOption, new List<string> { "one", "other" });
        Assert.NotEqual(first, second);
    }

    /// <summary>A list loaded from a YAML file arrives as an untyped list; reading it
    /// through a typed option must convert the elements. Java relies on erasure here, .NET
    /// must rebuild the list (see the PORT NOTE on ConfigurationUtils.ConvertToList).</summary>
    [Fact]
    public void TestUntypedRawListIsConvertedToTypedList()
    {
        var raw = new List<object?> { 1, 2, 3 };
        object converted = ConvertToList(raw, typeof(int));
        Assert.IsType<List<int>>(converted);
        Assert.Equal(new List<int> { 1, 2, 3 }, converted);

        // an already-typed list passes through untouched
        var typed = new List<string> { "a", "b" };
        Assert.Same(typed, ConvertToList(typed, typeof(string)));
    }

    /// <summary>GetValue must render structured values structurally, not as CLR type
    /// names (see the PORT NOTE on Configuration.GetValue).</summary>
    [Fact]
    public void TestGetValueFormatsStructuredValues()
    {
        var cfg = new FlinkNet.Configuration.Configuration();
        cfg.Set(ListStringOption, new List<string> { "a", "b" });
        cfg.Set(MapOption, new Dictionary<string, string> { { "k1", "v1" }, { "k2", "v2" } });

        Assert.Equal("[a, b]", cfg.GetValue(ListStringOption));
        Assert.Equal("{k1: v1, k2: v2}", cfg.GetValue(MapOption));
    }

    [Fact]
    public void TestOptionWithDefault()
    {
        var cfg = new FlinkNet.Configuration.Configuration();
        cfg.Set(GetIntConfigOption("int-key"), 11);
        cfg.SetString("string-key", "abc");

        ConfigOption<string> presentStringOption =
            ConfigOptions.Key("string-key").StringType().DefaultValue("my-beautiful-default");
        ConfigOption<int> presentIntOption = ConfigOptions.Key("int-key").IntType().DefaultValue(87);

        Assert.Equal("abc", cfg.Get(presentStringOption));
        Assert.Equal("abc", cfg.GetValue(presentStringOption));

        Assert.Equal(11, cfg.Get(presentIntOption));
        Assert.Equal("11", cfg.GetValue(presentIntOption));

        // test getting default when no value is present

        ConfigOption<string> stringOption =
            ConfigOptions.Key("test").StringType().DefaultValue("my-beautiful-default");
        ConfigOption<int> intOption = ConfigOptions.Key("test2").IntType().DefaultValue(87);

        // getting strings with default value should work
        Assert.Equal("my-beautiful-default", cfg.GetValue(stringOption));
        Assert.Equal("my-beautiful-default", cfg.Get(stringOption));

        // overriding the default should work
        Assert.Equal("override", cfg.Get(stringOption, "override"));

        // getting a primitive with a default value should work
        Assert.Equal(87, cfg.Get(intOption));
        Assert.Equal("87", cfg.GetValue(intOption));
    }

    [Fact]
    public void TestOptionWithNoDefault()
    {
        var cfg = new FlinkNet.Configuration.Configuration();
        cfg.Get(GetIntConfigOption("int-key"), 11);
        cfg.SetString("string-key", "abc");

        ConfigOption<string> presentStringOption =
            ConfigOptions.Key("string-key").StringType().NoDefaultValue();

        Assert.Equal("abc", cfg.Get(presentStringOption));
        Assert.Equal("abc", cfg.GetValue(presentStringOption));

        // test getting default when no value is present

        ConfigOption<string> stringOption = ConfigOptions.Key("test").StringType().NoDefaultValue();

        // getting strings for null should work
        Assert.Null(cfg.GetValue(stringOption));
        Assert.Null(cfg.Get(stringOption));

        // overriding the null default should work
        Assert.Equal("override", cfg.Get(stringOption, "override"));
    }

    [Fact]
    public void TestDeprecatedKeys()
    {
        var cfg = new FlinkNet.Configuration.Configuration();
        cfg.Set(GetIntConfigOption("the-key"), 11);
        cfg.Set(GetIntConfigOption("old-key"), 12);
        cfg.Set(GetIntConfigOption("older-key"), 13);

        ConfigOption<int> matchesFirst =
            ConfigOptions.Key("the-key").IntType().DefaultValue(-1)
                .WithDeprecatedKeys("old-key", "older-key");

        ConfigOption<int> matchesSecond =
            ConfigOptions.Key("does-not-exist").IntType().DefaultValue(-1)
                .WithDeprecatedKeys("old-key", "older-key");

        ConfigOption<int> matchesThird =
            ConfigOptions.Key("does-not-exist").IntType().DefaultValue(-1)
                .WithDeprecatedKeys("foo", "older-key");

        ConfigOption<int> notContained =
            ConfigOptions.Key("does-not-exist").IntType().DefaultValue(-1)
                .WithDeprecatedKeys("not-there", "also-not-there");

        Assert.Equal(11, cfg.Get(matchesFirst));
        Assert.Equal(12, cfg.Get(matchesSecond));
        Assert.Equal(13, cfg.Get(matchesThird));
        Assert.Equal(-1, cfg.Get(notContained));
    }

    [Fact]
    public void TestFallbackKeys()
    {
        var cfg = new FlinkNet.Configuration.Configuration();
        cfg.Set(GetIntConfigOption("the-key"), 11);
        cfg.Set(GetIntConfigOption("old-key"), 12);
        cfg.Set(GetIntConfigOption("older-key"), 13);

        ConfigOption<int> matchesFirst =
            ConfigOptions.Key("the-key").IntType().DefaultValue(-1)
                .WithFallbackKeys("old-key", "older-key");

        ConfigOption<int> matchesSecond =
            ConfigOptions.Key("does-not-exist").IntType().DefaultValue(-1)
                .WithFallbackKeys("old-key", "older-key");

        ConfigOption<int> matchesThird =
            ConfigOptions.Key("does-not-exist").IntType().DefaultValue(-1)
                .WithFallbackKeys("foo", "older-key");

        ConfigOption<int> notContained =
            ConfigOptions.Key("does-not-exist").IntType().DefaultValue(-1)
                .WithFallbackKeys("not-there", "also-not-there");

        Assert.Equal(11, cfg.Get(matchesFirst));
        Assert.Equal(12, cfg.Get(matchesSecond));
        Assert.Equal(13, cfg.Get(matchesThird));
        Assert.Equal(-1, cfg.Get(notContained));
    }

    [Fact]
    public void TestFallbackAndDeprecatedKeys()
    {
        ConfigOption<int> fallback = ConfigOptions.Key("fallback").IntType().DefaultValue(-1);

        ConfigOption<int> deprecated = ConfigOptions.Key("deprecated").IntType().DefaultValue(-1);

        ConfigOption<int> mainOption =
            ConfigOptions.Key("main").IntType().DefaultValue(-1)
                .WithFallbackKeys(fallback.Key)
                .WithDeprecatedKeys(deprecated.Key);

        var fallbackCfg = new FlinkNet.Configuration.Configuration();
        fallbackCfg.Set(fallback, 1);
        Assert.Equal(1, fallbackCfg.Get(mainOption));

        var deprecatedCfg = new FlinkNet.Configuration.Configuration();
        deprecatedCfg.Set(deprecated, 2);
        Assert.Equal(2, deprecatedCfg.Get(mainOption));

        // reverse declaration of fallback and deprecated keys, fallback keys should always be used
        // first
        ConfigOption<int> reversedMainOption =
            ConfigOptions.Key("main").IntType().DefaultValue(-1)
                .WithDeprecatedKeys(deprecated.Key)
                .WithFallbackKeys(fallback.Key);

        var deprecatedAndFallBackConfig = new FlinkNet.Configuration.Configuration();
        deprecatedAndFallBackConfig.Set(fallback, 1);
        deprecatedAndFallBackConfig.Set(deprecated, 2);
        Assert.Equal(1, deprecatedAndFallBackConfig.Get(mainOption));
        Assert.Equal(1, deprecatedAndFallBackConfig.Get(reversedMainOption));
    }

    [Fact]
    public void TestRemove()
    {
        var cfg = new FlinkNet.Configuration.Configuration();
        cfg.Set(GetIntConfigOption("a"), 1);
        cfg.Set(GetIntConfigOption("b"), 2);

        ConfigOption<int> validOption = ConfigOptions.Key("a").IntType().DefaultValue(-1);

        ConfigOption<int> deprecatedOption =
            ConfigOptions.Key("c").IntType().DefaultValue(-1).WithDeprecatedKeys("d", "b");

        ConfigOption<int> unexistedOption =
            ConfigOptions.Key("e").IntType().DefaultValue(-1).WithDeprecatedKeys("f", "g", "j");

        Assert.Equal(2, cfg.KeySet().Count);
        Assert.True(cfg.RemoveConfig(validOption));
        Assert.Single(cfg.KeySet());
        Assert.True(cfg.RemoveConfig(deprecatedOption));
        Assert.Empty(cfg.KeySet());
        Assert.False(cfg.RemoveConfig(unexistedOption));
    }

    [Fact]
    public void TestRemoveKey()
    {
        var cfg = new FlinkNet.Configuration.Configuration();
        const string key1 = "a.b";
        const string key2 = "c.d";
        cfg.Set(GetIntConfigOption(key1), 42);
        cfg.Set(GetIntConfigOption(key2), 44);
        cfg.Set(GetIntConfigOption(key2 + ".f1"), 44);
        cfg.Set(GetIntConfigOption(key2 + ".f2"), 44);
        cfg.Set(GetIntConfigOption("e.f"), 1337);

        Assert.False(cfg.RemoveKey("not-existing-key"));
        Assert.True(cfg.RemoveKey(key1));
        Assert.False(cfg.ContainsKey(key1));

        Assert.True(cfg.RemoveKey(key2));
        Assert.Equal(new HashSet<string> { "e.f" }, cfg.KeySet());
    }

    [Fact]
    public void TestShouldParseValidStringToEnum()
    {
        var configuration = new FlinkNet.Configuration.Configuration();
        configuration.SetString(StringOption.Key, TestEnum.Value1.ToString());

        TestEnum parsedEnumValue = configuration.GetEnum<TestEnum>(StringOption);
        Assert.Equal(TestEnum.Value1, parsedEnumValue);
    }

    [Fact]
    public void TestShouldParseValidStringToEnumIgnoringCase()
    {
        var configuration = new FlinkNet.Configuration.Configuration();
        configuration.SetString(StringOption.Key, TestEnum.Value1.ToString().ToLowerInvariant());

        TestEnum parsedEnumValue = configuration.GetEnum<TestEnum>(StringOption);
        Assert.Equal(TestEnum.Value1, parsedEnumValue);
    }

    [Fact]
    public void TestThrowsExceptionIfTryingToParseInvalidStringForEnum()
    {
        var configuration = new FlinkNet.Configuration.Configuration();
        const string invalidValueForTestEnum = "InvalidValueForTestEnum";
        configuration.SetString(StringOption.Key, invalidValueForTestEnum);

        var exception =
            Assert.Throws<ArgumentException>(() => configuration.GetEnum<TestEnum>(StringOption));
        Assert.Contains(
            "Value for config option "
                + StringOption.Key
                + " must be one of [Value1, Value2] (was "
                + invalidValueForTestEnum
                + ")",
            exception.Message);
    }

    [Fact]
    public void TestToMap()
    {
        var configuration = new FlinkNet.Configuration.Configuration();
        const string listValues = "value1;value2;value3";
        const string yamlListValues = "[value1, value2, value3]";
        configuration.Set(ListStringOption, listValues.Split(';').ToList());

        const string mapValues = "key1:value1,key2:value2";
        const string yamlMapValues = "{key1: value1, key2: value2}";
        configuration.Set(
            MapOption,
            mapValues.Split(',').ToDictionary(e => e.Split(':')[0], e => e.Split(':')[1]));

        TimeSpan duration = TimeSpan.FromMilliseconds(3000);
        configuration.Set(DurationOption, duration);

        Assert.Equal(yamlListValues, configuration.ToMap()[ListStringOption.Key]);
        Assert.Equal(yamlMapValues, configuration.ToMap()[MapOption.Key]);
        Assert.Equal("3 s", configuration.ToMap()[DurationOption.Key]);
    }

    [Fact]
    public void TestToFileWritableMap()
    {
        var configuration = new FlinkNet.Configuration.Configuration();
        const string listValues = "value1;value2;value3";
        const string yamlListValues = "[value1, value2, value3]";
        configuration.Set(ListStringOption, listValues.Split(';').ToList());

        const string mapValues = "key1:value1,key2:value2";
        const string yamlMapValues = "{key1: value1, key2: value2}";
        configuration.Set(
            MapOption,
            mapValues.Split(',').ToDictionary(e => e.Split(':')[0], e => e.Split(':')[1]));

        TimeSpan duration = TimeSpan.FromMilliseconds(3000);
        configuration.Set(DurationOption, duration);

        const string strValues = "*";
        const string yamlStrValues = "'*'";
        configuration.Set(StringOption, strValues);

        Assert.Equal(yamlListValues, configuration.ToFileWritableMap()[ListStringOption.Key]);
        Assert.Equal(yamlMapValues, configuration.ToFileWritableMap()[MapOption.Key]);
        Assert.Equal(yamlStrValues, configuration.ToFileWritableMap()[StringOption.Key]);
        Assert.Equal("3 s", configuration.ToMap()[DurationOption.Key]);
    }

    [Fact]
    public void TestMapNotContained()
    {
        var cfg = new FlinkNet.Configuration.Configuration();

        Assert.False(cfg.TryGet(MapOption, out _));
        Assert.False(cfg.Contains(MapOption));
    }

    [Fact]
    public void TestMapWithPrefix()
    {
        var cfg = new FlinkNet.Configuration.Configuration();
        cfg.SetString(MapProperty1, "value1");
        cfg.Set(GetIntConfigOption(MapProperty2), 12);

        Assert.Equal(PropertiesMap, cfg.Get(MapOption));
        Assert.True(cfg.Contains(MapOption));
    }

    [Fact]
    public void TestMapWithoutPrefix()
    {
        var cfg = new FlinkNet.Configuration.Configuration();
        cfg.Set(MapOption, PropertiesMap);

        Assert.Equal(PropertiesMap, cfg.Get(MapOption));
        Assert.True(cfg.Contains(MapOption));
    }

    [Fact]
    public void TestMapNonPrefixHasPrecedence()
    {
        var cfg = new FlinkNet.Configuration.Configuration();
        cfg.Set(MapOption, PropertiesMap);
        cfg.SetString(MapProperty1, "value1");
        cfg.Get(GetIntConfigOption(MapProperty2), 99999);

        Assert.Equal(PropertiesMap, cfg.Get(MapOption));
        Assert.True(cfg.Contains(MapOption));
        Assert.True(cfg.ContainsKey(MapProperty1));
    }

    [Fact]
    public void TestMapThatOverwritesPrefix()
    {
        var cfg = new FlinkNet.Configuration.Configuration();
        cfg.SetString(MapProperty1, "value1");
        cfg.Get(GetIntConfigOption(MapProperty2), 99999);
        cfg.Set(MapOption, PropertiesMap);

        Assert.Equal(PropertiesMap, cfg.Get(MapOption));
        Assert.True(cfg.Contains(MapOption));
        Assert.False(cfg.ContainsKey(MapProperty1));
    }

    [Fact]
    public void TestMapRemovePrefix()
    {
        var cfg = new FlinkNet.Configuration.Configuration();
        cfg.SetString(MapProperty1, "value1");
        cfg.Get(GetIntConfigOption(MapProperty2), 99999);
        cfg.RemoveConfig(MapOption);

        Assert.False(cfg.Contains(MapOption));
        Assert.False(cfg.ContainsKey(MapProperty1));
        Assert.False(cfg.ContainsKey(MapProperty2));
    }

    [Fact]
    public void TestListParserErrorDoesNotLeakSensitiveData()
    {
        ConfigOption<IList<string>> secret =
            ConfigOptions.Key("secret").StringType().AsList().NoDefaultValue();

        Assert.True(GlobalConfiguration.IsSensitive(secret.Key, []));

        var cfg = new FlinkNet.Configuration.Configuration();
        // missing closing quote
        cfg.SetString(secret.Key, "'secret_value");

        var exception = Assert.Throws<ArgumentException>(() => cfg.Get(secret));
        Assert.DoesNotContain("secret_value", exception.ToString());
    }

    [Fact]
    public void TestMapParserErrorDoesNotLeakSensitiveData()
    {
        ConfigOption<IDictionary<string, string>> secret =
            ConfigOptions.Key("secret").MapType().NoDefaultValue();

        Assert.True(GlobalConfiguration.IsSensitive(secret.Key, []));

        var cfg = new FlinkNet.Configuration.Configuration();
        // malformed map representation
        cfg.SetString(secret.Key, "secret_value");

        var exception = Assert.Throws<ArgumentException>(() => cfg.Get(secret));
        Assert.DoesNotContain("secret_value", exception.ToString());
    }

    [Fact]
    public void TestToStringDoesNotLeakSensitiveData()
    {
        ConfigOption<IDictionary<string, string>> secret =
            ConfigOptions.Key("secret").MapType().NoDefaultValue();

        Assert.True(GlobalConfiguration.IsSensitive(secret.Key, []));

        var cfg = new FlinkNet.Configuration.Configuration();
        cfg.SetString(secret.Key, "secret_value");

        Assert.DoesNotContain("secret_value", cfg.ToString());
    }

    [Fact]
    public void TestGetWithOverrideDefault()
    {
        var conf = new FlinkNet.Configuration.Configuration();

        // Test for integer without default value.
        ConfigOption<int> integerOption0 = ConfigOptions.Key("integer.key0").IntType().NoDefaultValue();
        // integerOption0 doesn't exist in conf, and it should be overrideDefault.
        Assert.Equal(2, conf.Get(integerOption0, 2));
        // integerOption0 exists in conf, and it should be value that set before.
        conf.Set(integerOption0, 3);
        Assert.Equal(3, conf.Get(integerOption0, 2));

        // Test for integer with default value, the default value should be ignored.
        ConfigOption<int> integerOption1 = ConfigOptions.Key("integer.key1").IntType().DefaultValue(4);
        Assert.Equal(5, conf.Get(integerOption1, 5));
        // integerOption1 is changed.
        conf.Set(integerOption1, 6);
        Assert.Equal(6, conf.Get(integerOption1, 5));

        // Test for string without default value.
        ConfigOption<string> stringOption0 = ConfigOptions.Key("string.key0").StringType().NoDefaultValue();
        // stringOption0 doesn't exist in conf, and it should be overrideDefault.
        Assert.Equal("a", conf.Get(stringOption0, "a"));
        // stringOption0 exists in conf, and it should be value that set before.
        conf.Set(stringOption0, "b");
        Assert.Equal("b", conf.Get(stringOption0, "a"));

        // Test for string with default value, the default value should be ignored.
        ConfigOption<string> stringOption1 = ConfigOptions.Key("string.key1").StringType().DefaultValue("c");
        Assert.Equal("d", conf.Get(stringOption1, "d"));
        // stringOption1 is changed.
        conf.Set(stringOption1, "e");
        Assert.Equal("e", conf.Get(stringOption1, "d"));
    }

    // --------------------------------------------------------------------------------------------
    // Test classes
    // --------------------------------------------------------------------------------------------

    private enum TestEnum
    {
        Value1,
        Value2,
    }
}
