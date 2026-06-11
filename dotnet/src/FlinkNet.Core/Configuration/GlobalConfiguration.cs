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

using FlinkNet.Annotations;

namespace FlinkNet.Configuration;

/// <summary>
/// Global configuration object for Flink. Similar to Java properties configuration objects it
/// includes key-value pairs which represent the framework's configuration.
///
/// <para>PORT NOTE: only the sensitive-key machinery is ported so far. Loading the configuration
/// from a <c>config.yaml</c> file (<c>loadConfiguration</c> and friends) is deferred to the YAML
/// increment together with <c>YamlParserUtils</c>.</para>
/// </summary>
[Internal]
public static class GlobalConfiguration
{
    private static readonly string[] SensitiveKeys =
    [
        "password",
        "secret",
        "fs.azure.account.key",
        "apikey",
        "api-key",
        "auth-params",
        "service-key",
        "token",
        "basic-auth",
        "jaas.config",
        "http-headers",
        "access-key",
        "access.key",
        "accesskey",
    ];

    /// <summary>The hidden content to be displayed in place of sensitive values.</summary>
    public const string HiddenContent = "******";

    /// <summary>
    /// Check whether the key is a hidden key.
    /// </summary>
    /// <param name="key">the config key</param>
    /// <param name="additionalKeys">additional substrings to consider sensitive, in addition to
    /// the built-in list; use <see cref="SecurityOptions.AdditionalSensitiveKeys"/> to obtain
    /// these from a loaded <see cref="Configuration"/></param>
    public static bool IsSensitive(string key, IList<string> additionalKeys)
    {
        ArgumentNullException.ThrowIfNull(key);
        string keyInLower = key.ToLowerInvariant();
        foreach (string hideKey in SensitiveKeys)
        {
            if (keyInLower.Length >= hideKey.Length && keyInLower.Contains(hideKey))
            {
                return true;
            }
        }
        foreach (string hideKey in additionalKeys)
        {
            string hideKeyLower = hideKey.ToLowerInvariant();
            if (keyInLower.Length >= hideKeyLower.Length && keyInLower.Contains(hideKeyLower))
            {
                return true;
            }
        }
        return false;
    }
}
