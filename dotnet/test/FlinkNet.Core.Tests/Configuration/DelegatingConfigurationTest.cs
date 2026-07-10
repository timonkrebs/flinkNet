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

using System.Reflection;
using FlinkNet.Configuration;
using Xunit;

namespace FlinkNet.Tests.Configuration;

/// <summary>Tests for the <see cref="DelegatingConfiguration"/>.</summary>
public class DelegatingConfigurationTest
{
    [Fact]
    public void TestIfDelegatesImplementAllMethods()
    {
        // For each public instance method declared in the Configuration class...
        MethodInfo[] confMethods =
            typeof(FlinkNet.Configuration.Configuration).GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        MethodInfo[] delegateMethods =
            typeof(DelegatingConfiguration).GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (MethodInfo configurationMethod in confMethods)
        {
            bool hasMethod = false;

            // Find matching method in wrapper class
            foreach (MethodInfo wrapperMethod in delegateMethods)
            {
                if (configurationMethod.Name != wrapperMethod.Name)
                {
                    continue;
                }

                ParameterInfo[] wrapperMethodParams = wrapperMethod.GetParameters();
                ParameterInfo[] configMethodParams = configurationMethod.GetParameters();
                if (wrapperMethodParams.Length != configMethodParams.Length)
                {
                    continue;
                }

                bool parametersMatch = true;
                for (int i = 0; i < wrapperMethodParams.Length; i++)
                {
                    // compare by display string so generic parameters (T) compare structurally
                    if (wrapperMethodParams[i].ParameterType.ToString()
                        != configMethodParams[i].ParameterType.ToString())
                    {
                        parametersMatch = false;
                        break;
                    }
                }
                if (parametersMatch)
                {
                    hasMethod = true;
                    break;
                }
            }

            Assert.True(
                hasMethod,
                $"Configuration method '{configurationMethod.Name}' has not been wrapped "
                    + "correctly in DelegatingConfiguration wrapper");
        }
    }

    [Fact]
    public void TestDelegationConfigurationWithNullOrEmptyPrefix()
    {
        var backingConf = new FlinkNet.Configuration.Configuration();
        backingConf.SetValueInternal("test-key", "value", false);

        Assert.Throws<ArgumentNullException>(
            () => new DelegatingConfiguration(backingConf, null!));

        var configuration = new DelegatingConfiguration(backingConf, "");
        Assert.Equal(backingConf.KeySet(), configuration.KeySet());
    }

    [Fact]
    public void TestDelegationConfigurationWithPrefix()
    {
        const string prefix = "pref-";
        const string expectedKey = "key";

        // Key matches the prefix
        var backingConf = new FlinkNet.Configuration.Configuration();
        backingConf.SetValueInternal(prefix + expectedKey, "value", false);

        var configuration = new DelegatingConfiguration(backingConf, prefix);
        ISet<string> keySet = configuration.KeySet();
        Assert.Equal(new HashSet<string> { expectedKey }, keySet);

        // Key does not match the prefix
        backingConf = new FlinkNet.Configuration.Configuration();
        backingConf.SetValueInternal("test-key", "value", false);

        configuration = new DelegatingConfiguration(backingConf, prefix);
        Assert.Empty(configuration.KeySet());
    }

    [Fact]
    public void TestDelegationConfigurationToMap()
    {
        // PORT NOTE: Java compares toMap against addAllToProperties; Properties is not ported,
        // so the prefix filtering of ToMap is asserted directly.
        var conf = new FlinkNet.Configuration.Configuration();
        conf.SetString("k0", "v0");
        conf.SetString("prefix.k1", "v1");
        conf.SetString("prefix.prefix.k2", "v2");
        conf.SetString("k3.prefix.prefix.k3", "v3");
        var dc = new DelegatingConfiguration(conf, "prefix.");

        var expected = new Dictionary<string, string> { { "k1", "v1" }, { "prefix.k2", "v2" } };
        Assert.Equal(expected, dc.ToMap());
    }

    /// <summary>Values must be escaped exactly once (PORT NOTE: Java escapes them a second
    /// time on the delegating layer; see DelegatingConfiguration.ToFileWritableMap).</summary>
    [Fact]
    public void TestDelegationConfigurationToFileWritableMapEscapesOnce()
    {
        var conf = new FlinkNet.Configuration.Configuration();
        conf.SetString("prefix.star", "*");
        conf.SetString("prefix.plain", "value");
        var dc = new DelegatingConfiguration(conf, "prefix.");

        var expected = new Dictionary<string, string>
        {
            { "star", "'*'" },
            { "plain", "value" },
        };
        Assert.Equal(expected, dc.ToFileWritableMap());
    }

    [Fact]
    public void TestSetReturnsDelegatingConfiguration()
    {
        var conf = new FlinkNet.Configuration.Configuration();
        var delegatingConf = new DelegatingConfiguration(conf, "prefix.");

        // PORT NOTE: Java uses CoreOptions.DEFAULT_PARALLELISM; the catalog is not ported yet
        ConfigOption<int> option = ConfigOptions.Key("parallelism.default").IntType().DefaultValue(1);
        Assert.Same(delegatingConf, delegatingConf.Set(option, 1));
    }

    [Fact]
    public void TestGetWithOverrideDefault()
    {
        var original = new FlinkNet.Configuration.Configuration();
        var delegatingConf = new DelegatingConfiguration(original, "prefix.");

        // Test for integer
        ConfigOption<int> integerOption = ConfigOptions.Key("integer.key").IntType().NoDefaultValue();

        // integerOption doesn't exist in delegatingConf, and it should be overrideDefault.
        original.Set(integerOption, 1);
        Assert.Equal(2, delegatingConf.Get(integerOption, 2));
        Assert.Equal(2, delegatingConf.Get(integerOption, 2));

        // integerOption exists in delegatingConf, and it should be value that set before.
        delegatingConf.Set(integerOption, 3);
        Assert.Equal(3, delegatingConf.Get(integerOption, 2));
        Assert.Equal(3, delegatingConf.Get(integerOption, 2));

        // Test for float
        ConfigOption<float> floatOption = ConfigOptions.Key("float.key").FloatType().NoDefaultValue();
        original.Set(floatOption, 4f);
        Assert.Equal(5f, delegatingConf.Get(floatOption, 5f));
        delegatingConf.Set(floatOption, 6f);
        Assert.Equal(6f, delegatingConf.Get(floatOption, 5f));

        // Test for double
        ConfigOption<double> doubleOption = ConfigOptions.Key("double.key").DoubleType().NoDefaultValue();
        original.Set(doubleOption, 7d);
        Assert.Equal(8d, delegatingConf.Get(doubleOption, 8d));
        delegatingConf.Set(doubleOption, 9d);
        Assert.Equal(9d, delegatingConf.Get(doubleOption, 8d));

        // Test for long
        ConfigOption<long> longOption = ConfigOptions.Key("long.key").LongType().NoDefaultValue();
        original.Set(longOption, 10L);
        Assert.Equal(11L, delegatingConf.Get(longOption, 11L));
        delegatingConf.Set(longOption, 12L);
        Assert.Equal(12L, delegatingConf.Get(longOption, 11L));

        // Test for boolean
        ConfigOption<bool> booleanOption = ConfigOptions.Key("boolean.key").BooleanType().NoDefaultValue();
        original.Set(booleanOption, false);
        Assert.True(delegatingConf.Get(booleanOption, true));
        delegatingConf.Set(booleanOption, false);
        Assert.False(delegatingConf.Get(booleanOption, true));
    }

    [Fact]
    public void TestRemoveKeyOrConfig()
    {
        var original = new FlinkNet.Configuration.Configuration();
        var delegatingConf = new DelegatingConfiguration(original, "prefix.");
        ConfigOption<int> integerOption = ConfigOptions.Key("integer.key").IntType().NoDefaultValue();

        // Test for RemoveConfig
        delegatingConf.Set(integerOption, 0);
        Assert.Equal(0, delegatingConf.Get(integerOption));
        delegatingConf.RemoveConfig(integerOption);
        Assert.False(delegatingConf.TryGet(integerOption, out _));

        // Test for RemoveKey
        delegatingConf.Set(integerOption, 0);
        Assert.Equal(0, delegatingConf.Get(integerOption, -1));
        delegatingConf.RemoveKey(integerOption.Key);
        Assert.False(delegatingConf.TryGet(integerOption, out _));
    }
}
