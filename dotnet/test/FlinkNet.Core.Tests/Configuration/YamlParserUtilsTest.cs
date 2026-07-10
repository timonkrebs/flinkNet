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
using YamlDotNet.Core;

namespace FlinkNet.Tests.Configuration;

/// <summary>Tests for <see cref="YamlParserUtils"/>.</summary>
public class YamlParserUtilsTest : IDisposable
{
    private readonly string _tmpDir = Directory.CreateTempSubdirectory("flinknet-yaml-test").FullName;

    public void Dispose() => Directory.Delete(_tmpDir, recursive: true);

    [Fact]
    public void TestLoadYamlFile()
    {
        string confFile = Path.Combine(_tmpDir, "test.yaml");
        File.WriteAllLines(
            confFile,
            [
                "key1: value1",
                "key2: ",
                "  subKey1: value2",
                "key3: [a, b, c]",
                "key4: {k1: v1, k2: v2, k3: v3}",
                "key5: '*'",
                "key6: true",
                "key7: 'true'",
            ]);

        IDictionary<string, object?> yamlData = YamlParserUtils.LoadYamlFile(confFile);
        Assert.NotNull(yamlData);
        Assert.Equal("value1", yamlData["key1"]);
        Assert.Equal("value2", ((Dictionary<object, object?>)yamlData["key2"]!)["subKey1"]);
        Assert.Equal(new List<object?> { "a", "b", "c" }, yamlData["key3"]);

        var map = new Dictionary<object, object?> { { "k1", "v1" }, { "k2", "v2" }, { "k3", "v3" } };
        Assert.Equal(map, yamlData["key4"]);
        Assert.Equal("*", yamlData["key5"]);
        Assert.Equal(true, yamlData["key6"]);
        Assert.Equal("true", yamlData["key7"]);
    }

    /// <summary>
    /// Tests to avoid potential unexpected behavior changes for FLINK configuration due to
    /// differences between YAML 1.2 and its predecessor YAML 1.1, based on the YAML Changes page
    /// https://yaml.org/spec/1.2.2/ext/changes.
    /// </summary>
    [Fact]
    public void TestYaml12Features()
    {
        // In YAML 1.2, only true and false strings are parsed as booleans (including True and
        // TRUE); y, yes, on, and their negative counterparts are parsed as strings.
        const string booleanRepresentation = "key1: Yes\nkey2: y\nkey3: on";
        var parsedBooleans =
            YamlParserUtils.ConvertToObject<Dictionary<object, object?>>(booleanRepresentation)!;
        Assert.Equal("Yes", parsedBooleans["key1"]); // Boolean#True in YAML 1.1
        Assert.Equal("y", parsedBooleans["key2"]); // Boolean#True in YAML 1.1
        Assert.Equal("on", parsedBooleans["key3"]); // Boolean#True in YAML 1.1

        // In YAML 1.2, underlines '_' cannot be used within numerical values.
        var underlineInNumber =
            YamlParserUtils.ConvertToObject<Dictionary<object, object?>>("key1: 1_000")!;
        Assert.Equal("1_000", underlineInNumber["key1"]); // number 1000 in YAML 1.1

        // In YAML 1.2, octal values need a 0o prefix; e.g. 010 is now parsed with the value 10
        // rather than 8.
        var octalNumber1 = YamlParserUtils.ConvertToObject<Dictionary<object, object?>>("octal: 010")!;
        Assert.Equal(10, octalNumber1["octal"]); // number 8 in YAML 1.1
        var octalNumber2 = YamlParserUtils.ConvertToObject<Dictionary<object, object?>>("octal: 0o10")!;
        Assert.Equal(8, octalNumber2["octal"]);

        // In YAML 1.2, the binary and sexagesimal integer formats have been dropped.
        var binaryNumber =
            YamlParserUtils.ConvertToObject<Dictionary<object, object?>>("binary: 0b101")!;
        Assert.Equal("0b101", binaryNumber["binary"]); // number 5 in YAML 1.1
        var sexagesimalNumber =
            YamlParserUtils.ConvertToObject<Dictionary<object, object?>>("sexagesimal: 1:00")!;
        Assert.Equal("1:00", sexagesimalNumber["sexagesimal"]); // number 60 in YAML 1.1

        // In YAML 1.2, the !!pairs, !!omap, !!set, !!timestamp and !!binary types are dropped.
        const string timestamp = "!!timestamp 2001-12-15T02:59:43.1Z";
        Assert.Throws<YamlException>(() => YamlParserUtils.ConvertToObject<object>(timestamp));
    }

    [Fact]
    public void TestLoadEmptyYamlFile()
    {
        string confFile = Path.Combine(_tmpDir, "test.yaml");
        File.WriteAllText(confFile, "");

        Assert.Empty(YamlParserUtils.LoadYamlFile(confFile));
    }

    [Fact]
    public void TestLoadYamlFileInvalidYamlSyntaxException()
    {
        string confFile = Path.Combine(_tmpDir, "invalid.yaml");
        File.WriteAllLines(confFile, ["key: value: secret"]);

        Exception e = Assert.ThrowsAny<Exception>(() => YamlParserUtils.LoadYamlFile(confFile));
        Assert.DoesNotContain("secret", e.ToString());
    }

    [Fact]
    public void TestLoadYamlFileDuplicateKeyException()
    {
        string confFile = Path.Combine(_tmpDir, "invalid.yaml");
        File.WriteAllLines(confFile, ["key: secret1", "key: secret2"]);

        Exception e = Assert.ThrowsAny<Exception>(() => YamlParserUtils.LoadYamlFile(confFile));
        Assert.DoesNotContain("secret1", e.ToString());
        Assert.DoesNotContain("secret2", e.ToString());
    }

    [Fact]
    public void TestToYamlString()
    {
        Assert.Equal(TestEnum.ENUM.ToString(), YamlParserUtils.ToYamlString(TestEnum.ENUM));

        object o1 = 123;
        Assert.Equal("123", YamlParserUtils.ToYamlString(o1));

        object o2 = true;
        Assert.Equal("true", YamlParserUtils.ToYamlString(o2));

        // the following values should be escaped
        object o3 = new List<string> { "*", "123", "true" };
        Assert.Equal("['*', '123', 'true']", YamlParserUtils.ToYamlString(o3));
    }

    /// <summary>Empty collections have no block representation — a bare "key:" reloads as
    /// null — so the block dumper must emit the flow forms <c>[]</c> and <c>{}</c>.</summary>
    [Fact]
    public void TestDumpYamlEmitsEmptyCollectionsExplicitly()
    {
        var flat = new Dictionary<string, object>
        {
            { "a.list", new List<string>() },
            { "a.map", new Dictionary<string, string>() },
            { "a.scalar", "v" },
        };

        IList<string> lines = YamlParserUtils.ConvertAndDumpYamlFromFlatMap(flat);
        Assert.Contains("  list: []", lines);
        Assert.Contains("  map: {}", lines);

        var reloaded = YamlParserUtils.ConvertToObject<Dictionary<object, object?>>(
            string.Join("\n", lines));
        Assert.NotNull(reloaded);
        var nested = Assert.IsAssignableFrom<System.Collections.IDictionary>(reloaded["a"]);
        Assert.Empty(Assert.IsAssignableFrom<System.Collections.IEnumerable>(nested["list"]));
        Assert.Empty(Assert.IsAssignableFrom<System.Collections.IDictionary>(nested["map"]));
    }

    /// <summary>Flow indicators terminate plain scalars inside flow collections, so list and
    /// map elements containing them must be quoted to survive a round-trip.</summary>
    [Fact]
    public void TestToYamlStringQuotesFlowIndicatorsInsideCollections()
    {
        object list = new List<string> { "a,b", "c" };
        string dumped = YamlParserUtils.ToYamlString(list);
        Assert.Equal("['a,b', c]", dumped);

        List<object?>? parsed = YamlParserUtils.ConvertToObject<List<object?>>(dumped);
        Assert.NotNull(parsed);
        Assert.Equal(new List<object?> { "a,b", "c" }, parsed);

        object map = new Dictionary<string, string> { { "k", "v1,v2" } };
        Assert.Equal("{k: 'v1,v2'}", YamlParserUtils.ToYamlString(map));

        // a comma in a plain top-level scalar needs no quotes (block context)
        Assert.Equal("a,b", YamlParserUtils.ToYamlString("a,b"));
    }

    [Fact]
    public void TestConvertToObject()
    {
        const string s1 = "test";
        Assert.Equal(s1, YamlParserUtils.ConvertToObject<string>(s1));

        const string s2 = "true";
        Assert.True(YamlParserUtils.ConvertToObject<bool>(s2));

        const string s3 = "[a, b, c]";
        Assert.Equal(
            new List<object?> { "a", "b", "c" },
            YamlParserUtils.ConvertToObject<List<object?>>(s3));

        const string s4 = "{k1: v1, k2: v2}";
        var map = new Dictionary<object, object?> { { "k1", "v1" }, { "k2", "v2" } };
        Assert.Equal(map, YamlParserUtils.ConvertToObject<Dictionary<object, object?>>(s4));
    }

    [Fact]
    public void TestDumpNestedYamlFromFlatMap()
    {
        var flattenMap = new Dictionary<string, object>
        {
            { "string", "stringValue" },
            { "integer", 42 },
            { "double", 3.14 },
            { "boolean", true },
            { "enum", TestEnum.ENUM },
            { "list1", new List<string> { "item1", "item2", "item3" } },
            { "list2", "{item1, item2, item3}" },
            { "map1", new Dictionary<string, string> { { "k1", "v1" } } },
            { "map2", "{k2: v2}" },
            {
                "listMap1",
                new List<Dictionary<string, string>>
                {
                    new() { { "k3", "v3" } },
                    new() { { "k4", "v4" } },
                }
            },
            { "listMap2", "[{k5: v5}, {k6: v6}]" },
            { "nested.key1.subKey1", "value1" },
            { "nested.key2.subKey1", "value2" },
            { "nested.key3", "value3" },
            { "escaped1", "*" },
            { "escaped2", "1" },
            { "escaped3", "true" },
        };

        IList<string> values = YamlParserUtils.ConvertAndDumpYamlFromFlatMap(flattenMap);

        var expected = new[]
        {
            "string: stringValue",
            "integer: 42",
            "double: 3.14",
            "boolean: true",
            "enum: ENUM",
            "list1:",
            "- item1",
            "- item2",
            "- item3",
            "list2: '{item1, item2, item3}'",
            "map1:",
            "  k1: v1",
            "map2: '{k2: v2}'",
            "listMap1:",
            "- k3: v3",
            "- k4: v4",
            "listMap2: '[{k5: v5}, {k6: v6}]'",
            "nested:",
            "  key1:",
            "    subKey1: value1",
            "  key2:",
            "    subKey1: value2",
            "  key3: value3",
            "escaped1: '*'",
            "escaped2: '1'",
            "escaped3: 'true'",
        };
        Assert.Equal(expected.Order(), values.Order());
    }

    private enum TestEnum
    {
        ENUM,
    }
}
