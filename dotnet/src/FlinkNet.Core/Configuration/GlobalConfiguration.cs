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
/// <para>PORT NOTE: Java reads the config directory from
/// <c>ConfigConstants.ENV_FLINK_CONF_DIR</c>; the constant is inlined here until ConfigConstants
/// is ported. Java logs the loaded configuration (with sensitive values hidden); the port is
/// silent until a logging abstraction is introduced.</para>
/// </summary>
[Internal]
public static class GlobalConfiguration
{
    public const string FlinkConfFilename = "config.yaml";

    /// <summary>The environment variable pointing at the configuration directory.</summary>
    private const string EnvFlinkConfDir = "FLINK_CONF_DIR";

    /// <summary>Key separator character for flattening nested YAML keys.</summary>
    private const string KeySeparator = ".";

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

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Loads the global configuration from the environment. Fails if an error occurs during
    /// loading. Returns an empty configuration object if the environment variable is not set. In
    /// production this variable is set but tests and local execution/debugging don't have this
    /// environment variable set. That's why we should fail if it is not set.
    /// </summary>
    /// <returns>Returns the Configuration</returns>
    public static Configuration LoadConfiguration() => LoadConfiguration(new Configuration());

    /// <summary>
    /// Loads the global configuration and adds the given dynamic properties configuration.
    /// </summary>
    /// <param name="dynamicProperties">The given dynamic properties</param>
    /// <returns>Returns the loaded global configuration with dynamic properties</returns>
    public static Configuration LoadConfiguration(Configuration dynamicProperties)
    {
        string? configDir = Environment.GetEnvironmentVariable(EnvFlinkConfDir);
        if (configDir == null)
        {
            return new Configuration(dynamicProperties);
        }

        return LoadConfiguration(configDir, dynamicProperties);
    }

    /// <summary>
    /// Loads the configuration files from the specified directory.
    /// </summary>
    /// <param name="configDir">the directory which contains the configuration files</param>
    public static Configuration LoadConfiguration(string configDir) =>
        LoadConfiguration(configDir, null);

    /// <summary>
    /// Loads the configuration files from the specified directory. If the dynamic properties
    /// configuration is not null, then it is added to the loaded configuration.
    /// </summary>
    /// <param name="configDir">directory to load the configuration from</param>
    /// <param name="dynamicProperties">configuration file containing the dynamic properties.
    /// Null if none.</param>
    /// <returns>The configuration loaded from the given configuration directory</returns>
    public static Configuration LoadConfiguration(string configDir, Configuration? dynamicProperties)
    {
        if (configDir == null)
        {
            throw new ArgumentException(
                "Given configuration directory is null, cannot load configuration");
        }

        if (!Directory.Exists(configDir))
        {
            throw new IllegalConfigurationException(
                "The given configuration directory name '"
                    + configDir
                    + "' ("
                    + Path.GetFullPath(configDir)
                    + ") does not describe an existing directory.");
        }

        // get Flink yaml configuration file
        string yamlConfigFile = Path.Combine(configDir, FlinkConfFilename);
        if (!File.Exists(yamlConfigFile))
        {
            throw new IllegalConfigurationException(
                "The Flink config file '"
                    + yamlConfigFile
                    + "' ("
                    + Path.GetFullPath(yamlConfigFile)
                    + ") does not exist.");
        }

        Configuration configuration = LoadYamlResource(yamlConfigFile);

        if (dynamicProperties != null)
        {
            configuration.AddAll(dynamicProperties);
        }

        return configuration;
    }

    /// <summary>
    /// Loads a YAML-file of key-value pairs.
    /// </summary>
    /// <param name="file">the YAML file to read from</param>
    /// <returns>the configuration parsed from the YAML file</returns>
    private static Configuration LoadYamlResource(string file)
    {
        var config = new Configuration();

        try
        {
            IDictionary<string, object?> configDocument = Flatten(YamlParserUtils.LoadYamlFile(file));
            foreach (KeyValuePair<string, object?> entry in configDocument)
            {
                config.SetValueInternal(entry.Key, entry.Value!, false);
            }

            return config;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error parsing YAML configuration.", e);
        }
    }

    /// <summary>
    /// Flattens a nested configuration map to be only one level deep. Nested keys are
    /// concatenated using the <see cref="KeySeparator"/> character, so that:
    ///
    /// <code>
    /// keyA:
    ///   keyB:
    ///     keyC: "hello"
    ///     keyD: "world"
    /// </code>
    ///
    /// becomes:
    ///
    /// <code>
    /// keyA.keyB.keyC: "hello"
    /// keyA.keyB.keyD: "world"
    /// </code>
    /// </summary>
    private static IDictionary<string, object?> Flatten(IDictionary<string, object?> config)
    {
        // Since we start flattening from the root, keys should not be prefixed with anything.
        var flattenedMap = new Dictionary<string, object?>();
        Flatten(config, "", flattenedMap);
        return flattenedMap;
    }

    private static void Flatten(
        IDictionary<string, object?> config,
        string keyPrefix,
        IDictionary<string, object?> flattenedMap)
    {
        foreach (KeyValuePair<string, object?> entry in config)
        {
            string flattenedKey = keyPrefix + entry.Key;
            if (entry.Value is Dictionary<object, object?> nestedMap)
            {
                var stringKeyed = new Dictionary<string, object?>();
                foreach (KeyValuePair<object, object?> nested in nestedMap)
                {
                    stringKeyed[Convert.ToString(
                        nested.Key, System.Globalization.CultureInfo.InvariantCulture)!] =
                        nested.Value;
                }
                Flatten(stringKeyed, flattenedKey + KeySeparator, flattenedMap);
            }
            else if (entry.Value is System.Collections.IList)
            {
                flattenedMap[flattenedKey] = YamlParserUtils.ToYamlString(entry.Value);
            }
            else
            {
                flattenedMap[flattenedKey] = entry.Value;
            }
        }
    }

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
