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

/// <summary>
/// This class verifies that the UnmodifiableConfiguration class overrides all setter methods in
/// Configuration.
/// </summary>
public class UnmodifiableConfigurationTest
{
    [Fact]
    public void TestOverrideAddMethods()
    {
        Type clazz = typeof(UnmodifiableConfiguration);
        foreach (MethodInfo m in clazz.GetMethods())
        {
            if (m.Name.StartsWith("Add", StringComparison.Ordinal))
            {
                Assert.Equal(clazz, m.DeclaringType);
            }
        }
    }

    [Fact]
    public void TestExceptionOnSet()
    {
        var parameters = new Dictionary<Type, object>
        {
            { typeof(byte[]), Array.Empty<byte>() },
            { typeof(string), "" },
        };

        Type clazz = typeof(UnmodifiableConfiguration);
        var config = new UnmodifiableConfiguration(new FlinkNet.Configuration.Configuration());

        foreach (MethodInfo m in clazz.GetMethods())
        {
            // ignore IWritableConfig.Set as it is covered via SetValueInternal; only the
            // string-key setters take non-generic parameters here
            if (m.Name.StartsWith("Set", StringComparison.Ordinal)
                && m.Name != "Set"
                && !m.IsGenericMethod)
            {
                Type parameterClass = m.GetParameters()[1].ParameterType;

                Assert.True(
                    parameters.TryGetValue(parameterClass, out object? parameter),
                    $"method {m} not covered by test");

                var exception =
                    Assert.Throws<TargetInvocationException>(
                        () => m.Invoke(config, ["key", parameter]));
                Assert.IsType<NotSupportedException>(exception.InnerException);
            }
        }

        // the generic Set<T>(ConfigOption<T>, T) must fail as well
        ConfigOption<string> option = ConfigOptions.Key("testkey").StringType().DefaultValue("value");
        Assert.Throws<NotSupportedException>(() => config.Set(option, "value"));
    }
}
