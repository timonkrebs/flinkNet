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

using System.Globalization;
using FlinkNet.Util;

namespace FlinkNet.Configuration;

/// <summary>
/// Utility class for <see cref="Configuration"/> related helper functions.
///
/// <para>PORT NOTE: structured values render and parse as standard YAML (via
/// <see cref="YamlParserUtils"/>) with the legacy Flink 1.x format as parsing fallback, matching
/// Java. Helpers that depend on yet-unported option catalogs (<c>parseTempDirectories</c>,
/// <c>getStandaloneClusterStartupPeriodTime</c>, JVM argument parsing, ...) are deferred to the
/// increments that port those catalogs.</para>
/// </summary>
public static class ConfigurationUtils
{
    private static readonly string[] Empty = [];

    /// <summary>
    /// Converts the provided configuration data into a format suitable for writing to a file,
    /// based on the <paramref name="flattenYaml"/> flag.
    ///
    /// <para>Only when <paramref name="flattenYaml"/> is <c>false</c> is a nested YAML format
    /// used; otherwise a flat key-value pair format is output. Each entry in the returned list
    /// represents a single line that can be written directly to a file.</para>
    /// </summary>
    /// <param name="configuration">The configuration to be converted.</param>
    /// <param name="flattenYaml">A boolean flag indicating if the configuration data should be
    /// output in a flattened format.</param>
    /// <returns>A list of strings, where each string represents a line of the file-writable data
    /// in the chosen format.</returns>
    public static IList<string> ConvertConfigToWritableLines(
        Configuration configuration, bool flattenYaml)
    {
        if (!flattenYaml)
        {
            return YamlParserUtils.ConvertAndDumpYamlFromFlatMap(configuration.ConfData);
        }

        IDictionary<string, string> fileWritableMap = configuration.ToFileWritableMap();
        return fileWritableMap.Select(entry => entry.Key + ": " + entry.Value).ToList();
    }

    /// <summary>
    /// Parses a string as a map of strings. The expected format of the map is:
    /// <code>key1:value1,key2:value2</code>
    ///
    /// <para>Parts of the string can be escaped by wrapping with single or double quotes.</para>
    /// </summary>
    /// <param name="stringSerializedMap">a string to parse</param>
    /// <returns>parsed map</returns>
    public static IDictionary<string, string> ParseStringToMap(string stringSerializedMap) =>
        ConvertToProperties(stringSerializedMap);

    public static string ParseMapToString(IDictionary<string, string> map) => ConvertToString(map);

    /// <summary>
    /// Replaces values whose keys are sensitive according to
    /// <see cref="GlobalConfiguration.IsSensitive(string, IList{string})"/> with
    /// <see cref="GlobalConfiguration.HiddenContent"/>.
    ///
    /// <para>This can be useful when displaying configuration values.</para>
    /// </summary>
    /// <param name="keyValuePairs">for which to hide sensitive values</param>
    /// <param name="additionalSensitiveKeys">user-defined additional sensitive key substrings; use
    /// <see cref="SecurityOptions.AdditionalSensitiveKeys"/> to obtain these from a loaded
    /// configuration</param>
    /// <returns>A map where all sensitive values are hidden</returns>
    public static IDictionary<string, string> HideSensitiveValues(
        IDictionary<string, string> keyValuePairs, IList<string> additionalSensitiveKeys)
    {
        var result = new Dictionary<string, string>();

        foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
        {
            if (GlobalConfiguration.IsSensitive(keyValuePair.Key, additionalSensitiveKeys))
            {
                result[keyValuePair.Key] = GlobalConfiguration.HiddenContent;
            }
            else
            {
                result[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// Splits a comma or path-separator separated list of directories.
    /// </summary>
    public static string[] SplitPaths(string separatedPaths)
    {
        ArgumentNullException.ThrowIfNull(separatedPaths);
        return separatedPaths.Length > 0
            ? separatedPaths.Split(',', Path.PathSeparator)
            : Empty;
    }

    /// <summary>
    /// Creates a dynamic parameter list <c>string</c> of the passed configuration map.
    /// </summary>
    /// <param name="config">A map containing parameter/value entries that shall be used in the
    /// dynamic parameter list.</param>
    /// <returns>The dynamic parameter list <c>string</c>.</returns>
    public static string AssembleDynamicConfigsStr(IDictionary<string, string> config) =>
        string.Join(" ", config.Select(e => $"-D {e.Key}={e.Value}"));

    /// <summary>
    /// Extract and parse Flink configuration properties with a given name prefix and return the
    /// result as a Map.
    /// </summary>
    public static IDictionary<string, string> GetPrefixedKeyValuePairs(
        string prefix, Configuration configuration)
    {
        var result = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> entry in configuration.ToMap())
        {
            if (entry.Key.StartsWith(prefix, StringComparison.Ordinal)
                && entry.Key.Length > prefix.Length)
            {
                string key = entry.Key[prefix.Length..];
                result[key] = entry.Value;
            }
        }
        return result;
    }

    // --------------------------------------------------------------------------------------------
    //  Type conversion
    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Tries to convert the raw value into the provided type.
    /// </summary>
    /// <param name="rawValue">rawValue to convert into the provided type clazz</param>
    /// <param name="clazz">clazz specifying the target type</param>
    /// <returns>the converted value (boxed) if rawValue is of type clazz</returns>
    /// <exception cref="ArgumentException">if the rawValue cannot be converted in the specified
    /// target type clazz</exception>
    public static object ConvertValue(object rawValue, Type clazz)
    {
        if (clazz == typeof(int))
        {
            return ConvertToInt(rawValue);
        }
        else if (clazz == typeof(long))
        {
            return ConvertToLong(rawValue);
        }
        else if (clazz == typeof(bool))
        {
            return ConvertToBoolean(rawValue);
        }
        else if (clazz == typeof(float))
        {
            return ConvertToFloat(rawValue);
        }
        else if (clazz == typeof(double))
        {
            return ConvertToDouble(rawValue);
        }
        else if (clazz == typeof(string))
        {
            return ConvertToString(rawValue);
        }
        else if (clazz.IsEnum)
        {
            return ConvertToEnum(rawValue, clazz);
        }
        else if (clazz == typeof(TimeSpan))
        {
            return ConvertToDuration(rawValue);
        }
        else if (clazz == typeof(MemorySize))
        {
            return ConvertToMemorySize(rawValue);
        }
        else if (clazz == typeof(IDictionary<string, string>))
        {
            return ConvertToProperties(rawValue);
        }

        throw new ArgumentException("Unsupported type: " + clazz);
    }

    /// <summary>
    /// Converts the raw value to a list whose elements are of the provided atomic type. The
    /// returned list is a <c>List&lt;atomicClass&gt;</c> so it can be cast to the option's
    /// <c>IList&lt;T&gt;</c> type. Standard-YAML lists are tried first, falling back to the
    /// legacy semicolon-separated format.
    /// </summary>
    public static object ConvertToList(object rawValue, Type atomicClass)
    {
        if (rawValue is System.Collections.IList && rawValue is not Array)
        {
            return rawValue;
        }

        try
        {
            List<object?> data =
                YamlParserUtils.ConvertToObject<List<object?>>(ConvertToString(rawValue))
                    ?? throw new ArgumentException("not a YAML list");
            return BuildTypedList(data, atomicClass);
        }
        catch (Exception)
        {
            // Fallback to legacy pattern
            return ConvertToListWithLegacyProperties(rawValue, atomicClass);
        }
    }

    private static object ConvertToListWithLegacyProperties(object rawValue, Type atomicClass)
    {
        List<string> splits = StructuredOptionsSplitter.SplitEscaped(ConvertToString(rawValue), ';');
        return BuildTypedList(splits, atomicClass);
    }

    private static object BuildTypedList(
        System.Collections.IEnumerable elements, Type atomicClass)
    {
        Type listType = typeof(List<>).MakeGenericType(atomicClass);
        var result = (System.Collections.IList)Activator.CreateInstance(listType)!;
        foreach (object? element in elements)
        {
            result.Add(ConvertValue(element!, atomicClass));
        }
        return result;
    }

    internal static IDictionary<string, string> ConvertToProperties(object o)
    {
        if (o is IDictionary<string, string> alreadyMap)
        {
            return alreadyMap;
        }
        if (o is System.Collections.IDictionary rawMap)
        {
            return ConvertToStringMap(rawMap);
        }

        try
        {
            Dictionary<object, object?> map =
                YamlParserUtils.ConvertToObject<Dictionary<object, object?>>(ConvertToString(o))
                    ?? throw new ArgumentException("not a YAML map");
            return ConvertToStringMap(map);
        }
        catch (Exception)
        {
            // Fallback to legacy pattern
            return ConvertToPropertiesWithLegacyPattern(o);
        }
    }

    private static IDictionary<string, string> ConvertToPropertiesWithLegacyPattern(object o)
    {
        List<string> listOfRawProperties =
            StructuredOptionsSplitter.SplitEscaped(ConvertToString(o), ',');
        var result = new Dictionary<string, string>();
        foreach (string rawProperty in listOfRawProperties)
        {
            List<string> pair = StructuredOptionsSplitter.SplitEscaped(rawProperty, ':');
            if (pair.Count != 2)
            {
                throw new ArgumentException("Map item is not a key-value pair (missing ':'?)");
            }
            result[pair[0]] = pair[1];
        }
        return result;
    }

    private static Dictionary<string, string> ConvertToStringMap(System.Collections.IDictionary map)
    {
        var result = new Dictionary<string, string>();
        foreach (System.Collections.DictionaryEntry entry in map)
        {
            result[ConvertToString(entry.Key)] = ConvertToString(entry.Value!);
        }
        return result;
    }

    /// <summary>
    /// Converts the raw value to the given enum type, matching enum member names
    /// case-insensitively. Returns the boxed enum value.
    /// </summary>
    public static object ConvertToEnum(object o, Type clazz)
    {
        if (o.GetType() == clazz)
        {
            return o;
        }

        string upper = o.ToString()!.ToUpperInvariant();
        foreach (object constant in Enum.GetValues(clazz))
        {
            if (constant.ToString()!.ToUpperInvariant() == upper)
            {
                return constant;
            }
        }

        throw new ArgumentException(
            string.Format(
                CultureInfo.InvariantCulture,
                "Could not parse value for enum {0}. Expected one of: [{1}]",
                clazz,
                string.Join(", ", Enum.GetValues(clazz).Cast<object>())));
    }

    internal static TimeSpan ConvertToDuration(object o)
    {
        if (o is TimeSpan duration)
        {
            return duration;
        }

        return TimeUtils.ParseDuration(ConvertToString(o));
    }

    internal static MemorySize ConvertToMemorySize(object o)
    {
        if (o is MemorySize memorySize)
        {
            return memorySize;
        }

        return MemorySize.Parse(ConvertToString(o));
    }

    /// <summary>
    /// Converts a stored raw value to its string representation. Non-string values are rendered
    /// in standard YAML flow syntax (lists as <c>[a, b]</c>, maps as <c>{k: v}</c>, durations via
    /// <see cref="TimeUtils.FormatWithHighestUnit"/>).
    /// </summary>
    internal static string ConvertToString(object o)
    {
        if (o is string s)
        {
            return s;
        }
        return YamlParserUtils.ToYamlString(o);
    }

    internal static int ConvertToInt(object o)
    {
        if (o is int i)
        {
            return i;
        }
        else if (o is long l)
        {
            if (l is <= int.MaxValue and >= int.MinValue)
            {
                return (int)l;
            }
            throw new ArgumentException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Configuration value {0} overflows/underflows the integer type.",
                    l));
        }

        return int.Parse(o.ToString()!, CultureInfo.InvariantCulture);
    }

    internal static long ConvertToLong(object o)
    {
        if (o is long l)
        {
            return l;
        }
        else if (o is int i)
        {
            return i;
        }

        return long.Parse(o.ToString()!, CultureInfo.InvariantCulture);
    }

    internal static bool ConvertToBoolean(object o)
    {
        if (o is bool b)
        {
            return b;
        }

        return o.ToString()!.ToUpperInvariant() switch
        {
            "TRUE" => true,
            "FALSE" => false,
            _ =>
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Unrecognized option for boolean: {0}. Expected either true or false(case insensitive)",
                        o)),
        };
    }

    internal static float ConvertToFloat(object o)
    {
        if (o is float f)
        {
            return f;
        }
        else if (o is double value)
        {
            if (value == 0.0
                || (value >= float.Epsilon && value <= float.MaxValue)
                || (value >= -float.MaxValue && value <= -float.Epsilon))
            {
                return (float)value;
            }
            throw new ArgumentException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Configuration value {0} overflows/underflows the float type.",
                    value));
        }

        return float.Parse(o.ToString()!, CultureInfo.InvariantCulture);
    }

    internal static double ConvertToDouble(object o)
    {
        if (o is double d)
        {
            return d;
        }
        else if (o is float f)
        {
            return f;
        }

        return double.Parse(o.ToString()!, CultureInfo.InvariantCulture);
    }

    // --------------------------------------------------------------------------------------------
    //  Prefix map handling
    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Maps can be represented in two ways.
    ///
    /// <para>With constant key space:</para>
    /// <code>avro-confluent.properties = schema:1,other-prop:2</code>
    ///
    /// <para>Or with variable key space (i.e. prefix notation):</para>
    /// <code>
    /// avro-confluent.properties.schema = 1
    /// avro-confluent.properties.other-prop = 2
    /// </code>
    /// </summary>
    public static bool CanBePrefixMap<T>(ConfigOption<T> configOption) =>
        configOption.Clazz == typeof(IDictionary<string, string>) && !configOption.IsList;

    /// <summary>Filter condition for prefix map keys.</summary>
    public static bool FilterPrefixMapKey(string key, string candidate)
    {
        string prefixKey = key + ".";
        return candidate.StartsWith(prefixKey, StringComparison.Ordinal);
    }

    internal static IDictionary<string, string> ConvertToPropertiesPrefixed(
        Dictionary<string, object> confData, string key)
    {
        string prefixKey = key + ".";
        return confData.Keys
            .Where(k => k.StartsWith(prefixKey, StringComparison.Ordinal))
            .ToDictionary(k => k[prefixKey.Length..], k => ConvertToString(confData[k]));
    }

    internal static bool ContainsPrefixMap(Dictionary<string, object> confData, string key) =>
        confData.Keys.Any(candidate => FilterPrefixMapKey(key, candidate));

    internal static bool RemovePrefixMap(Dictionary<string, object> confData, string key)
    {
        List<string> prefixKeys =
            confData.Keys.Where(candidate => FilterPrefixMapKey(key, candidate)).ToList();
        foreach (string prefixKey in prefixKeys)
        {
            confData.Remove(prefixKey);
        }
        return prefixKeys.Count != 0;
    }

    // --------------------------------------------------------------------------------------------

    public static ConfigOption<bool> GetBooleanConfigOption(string key) =>
        ConfigOptions.Key(key).BooleanType().NoDefaultValue();

    public static ConfigOption<double> GetDoubleConfigOption(string key) =>
        ConfigOptions.Key(key).DoubleType().NoDefaultValue();

    public static ConfigOption<float> GetFloatConfigOption(string key) =>
        ConfigOptions.Key(key).FloatType().NoDefaultValue();

    public static ConfigOption<int> GetIntConfigOption(string key) =>
        ConfigOptions.Key(key).IntType().NoDefaultValue();

    public static ConfigOption<long> GetLongConfigOption(string key) =>
        ConfigOptions.Key(key).LongType().NoDefaultValue();
}
