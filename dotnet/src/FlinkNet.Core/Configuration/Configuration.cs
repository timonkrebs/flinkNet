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

using System.Diagnostics.CodeAnalysis;
using FlinkNet.Annotations;

namespace FlinkNet.Configuration;

/// <summary>
/// Lightweight configuration object which stores key/value pairs.
///
/// <para>PORT NOTES:</para>
/// <list type="bullet">
///   <item><description>The Java class extends <c>ExecutionConfig.GlobalJobParameters</c>, which
///   is not ported yet.</description></item>
///   <item><description>Java logs a warning when a deprecated key is used; the port is silent
///   until a logging abstraction is introduced.</description></item>
/// </list>
/// </summary>
[Public]
public class Configuration : IReadableConfig, IWritableConfig, Core.Io.IIOReadableWritable
{
    /// <summary>
    /// Stores the concrete key/value pairs of this configuration object.
    /// </summary>
    protected internal readonly Dictionary<string, object> ConfData;

    // --------------------------------------------------------------------------------------------

    /// <summary>Creates a new empty configuration.</summary>
    public Configuration()
    {
        ConfData = new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a new configuration with the copy of the given configuration.
    /// </summary>
    /// <param name="other">The configuration to copy the entries from.</param>
    public Configuration(Configuration other)
    {
        ConfData = new Dictionary<string, object>(other.ConfData);
    }

    // --------------------------------------------------------------------------------------------

    /// <summary>Creates a new configuration that is initialized with the options of the given map.</summary>
    public static Configuration FromMap(IDictionary<string, string> map)
    {
        var configuration = new Configuration();
        foreach (KeyValuePair<string, string> entry in map)
        {
            configuration.SetString(entry.Key, entry.Value);
        }
        return configuration;
    }

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Returns the value associated with the given key as a string. We encourage users and
    /// developers to always use ConfigOption for getting the configurations if possible, for its
    /// rich description, type, default-value and other supports. The string-key-based getter
    /// should only be used when ConfigOption is not applicable, e.g., the key is programmatically
    /// generated in runtime.
    /// </summary>
    /// <param name="key">the key pointing to the associated value</param>
    /// <param name="defaultValue">the default value which is returned in case there is no value
    /// associated with the given key</param>
    /// <returns>the (default) value associated with the given key</returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public virtual string? GetString(string key, string? defaultValue) =>
        TryGetRawValue(key, false, out object? rawValue)
            ? ConfigurationUtils.ConvertToString(rawValue)
            : defaultValue;

    /// <summary>
    /// Adds the given key/value pair to the configuration object. We encourage users and
    /// developers to always use ConfigOption for setting the configurations if possible, for its
    /// rich description, type, default-value and other supports. The string-key-based setter
    /// should only be used when ConfigOption is not applicable, e.g., the key is programmatically
    /// generated in runtime.
    /// </summary>
    /// <param name="key">the key of the key/value pair to be added</param>
    /// <param name="value">the value of the key/value pair to be added</param>
    public virtual void SetString(string key, string value) => SetValueInternal(key, value, false);

    /// <summary>
    /// Returns the value associated with the given key as a byte array.
    /// </summary>
    /// <param name="key">The key pointing to the associated value.</param>
    /// <param name="defaultValue">The default value which is returned in case there is no value
    /// associated with the given key.</param>
    /// <returns>the (default) value associated with the given key.</returns>
    [Internal]
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public virtual byte[]? GetBytes(string key, byte[]? defaultValue)
    {
        if (!TryGetRawValue(key, false, out object? rawValue))
        {
            return defaultValue;
        }
        if (rawValue is byte[] bytes)
        {
            return bytes;
        }
        throw new ArgumentException(
            $"Configuration cannot evaluate value {rawValue} as a byte[] value");
    }

    /// <summary>
    /// Adds the given byte array to the configuration object.
    /// </summary>
    /// <param name="key">The key under which the bytes are added.</param>
    /// <param name="bytes">The bytes to be added.</param>
    [Internal]
    public virtual void SetBytes(string key, byte[] bytes) => SetValueInternal(key, bytes, false);

    /// <summary>
    /// Returns the value associated with the given config option as a string.
    /// </summary>
    /// <param name="configOption">The configuration option</param>
    /// <returns>the (default) value associated with the given config option</returns>
    [PublicEvolving]
    public virtual string? GetValue<T>(ConfigOption<T> configOption)
    {
        object? rawValue =
            TryGetRawValueFromOption(configOption, out object? value)
                ? value
                : configOption.DefaultValueBoxed;
        // PORT NOTE: Java renders with String.valueOf, which is structural for List/Map
        // values; .NET collections print their type name from ToString(), so the port uses
        // its canonical stringification (YAML flow), which its own parsers can read back.
        return rawValue == null ? null : ConfigurationUtils.ConvertToString(rawValue);
    }

    /// <summary>
    /// Returns the value associated with the given config option as an enum.
    /// </summary>
    /// <typeparam name="TEnum">The return enum type</typeparam>
    /// <param name="configOption">The configuration option</param>
    /// <exception cref="ArgumentException">If the string associated with the given config option
    /// cannot be parsed as a value of the provided enum type.</exception>
    [PublicEvolving]
    public virtual TEnum GetEnum<TEnum>(ConfigOption<string> configOption)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(configOption);

        object? rawValue =
            TryGetRawValueFromOption(configOption, out object? value)
                ? value
                : configOption.DefaultValueBoxed;
        try
        {
            return (TEnum)ConfigurationUtils.ConvertToEnum(rawValue!, typeof(TEnum));
        }
        catch (ArgumentException)
        {
            string errorMessage = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "Value for config option {0} must be one of [{1}] (was {2})",
                configOption.Key,
                string.Join(", ", Enum.GetValues<TEnum>()),
                rawValue);
            throw new ArgumentException(errorMessage);
        }
    }

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Returns the keys of all key/value pairs stored inside this configuration object.
    /// </summary>
    public virtual ISet<string> KeySet()
    {
        lock (ConfData)
        {
            return new HashSet<string>(ConfData.Keys);
        }
    }

    public virtual void AddAll(Configuration other)
    {
        lock (ConfData)
        {
            lock (other.ConfData)
            {
                foreach (KeyValuePair<string, object> entry in other.ConfData)
                {
                    ConfData[entry.Key] = entry.Value;
                }
            }
        }
    }

    /// <summary>
    /// Adds all entries from the given configuration into this configuration. The keys are
    /// prepended with the given prefix.
    /// </summary>
    /// <param name="other">The configuration whose entries are added to this configuration.</param>
    /// <param name="prefix">The prefix to prepend.</param>
    public virtual void AddAll(Configuration other, string prefix)
    {
        lock (ConfData)
        {
            lock (other.ConfData)
            {
                foreach (KeyValuePair<string, object> entry in other.ConfData)
                {
                    ConfData[prefix + entry.Key] = entry.Value;
                }
            }
        }
    }

    /// <summary>Returns a copy of this configuration (port of Java's <c>clone()</c>).</summary>
    public virtual Configuration Clone()
    {
        var config = new Configuration();
        config.AddAll(this);
        return config;
    }

    /// <summary>
    /// Checks whether there is an entry with the specified key.
    /// </summary>
    /// <param name="key">key of entry</param>
    /// <returns>true if the key is stored, false otherwise</returns>
    public virtual bool ContainsKey(string key)
    {
        lock (ConfData)
        {
            return ConfData.ContainsKey(key);
        }
    }

    /// <summary>
    /// Checks whether there is an entry for the given config option.
    /// </summary>
    /// <param name="configOption">The configuration option</param>
    /// <returns><c>true</c> if a valid (current or deprecated) key of the config option is stored,
    /// <c>false</c> otherwise</returns>
    [PublicEvolving]
    public virtual bool Contains<T>(ConfigOption<T> configOption)
    {
        lock (ConfData)
        {
            return ApplyWithOption(
                configOption,
                (key, canBePrefixMap) =>
                    (canBePrefixMap && ConfigurationUtils.ContainsPrefixMap(ConfData, key))
                        || ConfData.ContainsKey(key));
        }
    }

    /// <summary>
    /// Returns the value of the given option, falling back to the option's default value if no
    /// keys are found in this configuration.
    ///
    /// <para>NOTE: current logic is not able to get the default value of the fallback key's
    /// ConfigOption, in case the given ConfigOption has no default value. If you want to use a
    /// fallback key, please make sure its value can be found in the configuration at runtime.</para>
    ///
    /// <para>PORT NOTE: for value-type options without a configured value and without a default,
    /// Java returns boxed <c>null</c>; the port returns <c>default(T)</c>. Use
    /// <see cref="TryGet{T}(ConfigOption{T}, out T)"/> to distinguish absence.</para>
    /// </summary>
    /// <param name="option">metadata of the option to read</param>
    /// <returns>the value of the given option</returns>
    public virtual T? Get<T>(ConfigOption<T> option) =>
        TryGet(option, out T? value) ? value : option.DefaultValue;

    /// <summary>
    /// Returns the value associated with the given config option as a T. If no value is mapped
    /// under any key of the option, it returns the specified default instead of the option's
    /// default value.
    /// </summary>
    /// <param name="configOption">The configuration option</param>
    /// <param name="overrideDefault">The value to return if no value was mapped for any key of the
    /// option</param>
    /// <returns>the configured value associated with the given config option, or the
    /// overrideDefault</returns>
    [PublicEvolving]
    public virtual T Get<T>(ConfigOption<T> configOption, T overrideDefault) =>
        TryGet(configOption, out T? value) ? value : overrideDefault;

    /// <inheritdoc cref="IReadableConfig.TryGet{T}(ConfigOption{T}, out T)"/>
    public virtual bool TryGet<T>(ConfigOption<T> option, [MaybeNullWhen(false)] out T value)
    {
        if (!TryGetRawValueFromOption(option, out object? rawValue))
        {
            value = default;
            return false;
        }

        try
        {
            object converted =
                option.IsList
                    ? ConfigurationUtils.ConvertToList(rawValue, option.Clazz)
                    : ConfigurationUtils.ConvertValue(rawValue, option.Clazz);
            value = (T)converted;
            return true;
        }
        catch (Exception e)
        {
            throw new ArgumentException(
                GlobalConfiguration.IsSensitive(
                        option.Key, Get(SecurityOptions.AdditionalSensitiveKeys) ?? [])
                    ? $"Could not parse value for key '{option.Key}'."
                    : $"Could not parse value '{rawValue}' for key '{option.Key}'.",
                e);
        }
    }

    public virtual Configuration Set<T>(ConfigOption<T> option, T value)
    {
        bool canBePrefixMap = ConfigurationUtils.CanBePrefixMap(option);
        SetValueInternal(option.Key, value, canBePrefixMap);
        return this;
    }

    IWritableConfig IWritableConfig.Set<T>(ConfigOption<T> option, T value) => Set(option, value);

    // --------------------------------------------------------------------------------------------

    public virtual IDictionary<string, string> ToMap()
    {
        lock (ConfData)
        {
            var ret = new Dictionary<string, string>(ConfData.Count);
            foreach (KeyValuePair<string, object> entry in ConfData)
            {
                ret[entry.Key] = ConfigurationUtils.ConvertToString(entry.Value);
            }
            return ret;
        }
    }

    /// <summary>
    /// Converts the configuration into a <c>IDictionary&lt;string, string&gt;</c> representation
    /// suitable for writing to a file.
    ///
    /// <para>This method ensures the value is properly escaped when writing the key-value pair to
    /// a standard YAML file.</para>
    /// </summary>
    [Internal]
    public virtual IDictionary<string, string> ToFileWritableMap()
    {
        lock (ConfData)
        {
            var ret = new Dictionary<string, string>(ConfData.Count);
            foreach (KeyValuePair<string, object> entry in ConfData)
            {
                // Because some characters in standard yaml must be escaped by quotes, such as
                // '*', the value is wrapped via the YAML dumper here
                ret[entry.Key] = YamlParserUtils.ToYamlString(entry.Value);
            }
            return ret;
        }
    }

    /// <summary>
    /// Removes given config option from the configuration.
    /// </summary>
    /// <param name="configOption">config option to remove</param>
    /// <typeparam name="T">Type of the config option</typeparam>
    /// <returns>true if config has been removed, false otherwise</returns>
    public virtual bool RemoveConfig<T>(ConfigOption<T> configOption)
    {
        lock (ConfData)
        {
            return ApplyWithOption(
                configOption,
                (key, canBePrefixMap) =>
                    (canBePrefixMap && ConfigurationUtils.RemovePrefixMap(ConfData, key))
                        || ConfData.Remove(key));
        }
    }

    /// <summary>
    /// Removes given key from the configuration.
    /// </summary>
    /// <param name="key">key of a config option to remove</param>
    /// <returns>true if config has been removed, false otherwise</returns>
    public virtual bool RemoveKey(string key)
    {
        lock (ConfData)
        {
            bool removed = ConfData.Remove(key);
            removed |= ConfigurationUtils.RemovePrefixMap(ConfData, key);
            return removed;
        }
    }

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Stores the given value under the given key, removing any prefix-map entries for the key
    /// first when the option may be represented as a prefix map.
    /// </summary>
    protected internal virtual void SetValueInternal<T>(string key, T value, bool canBePrefixMap)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key), "Key must not be null.");
        }
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Value must not be null.");
        }

        lock (ConfData)
        {
            if (canBePrefixMap)
            {
                ConfigurationUtils.RemovePrefixMap(ConfData, key);
            }
            ConfData[key] = value;
        }
    }

    private bool TryGetRawValue(string key, bool canBePrefixMap, [NotNullWhen(true)] out object? rawValue)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key), "Key must not be null.");
        }

        lock (ConfData)
        {
            if (ConfData.TryGetValue(key, out rawValue))
            {
                return true;
            }
            if (!canBePrefixMap)
            {
                return false;
            }
            IDictionary<string, string> valueFromPrefixMap =
                ConfigurationUtils.ConvertToPropertiesPrefixed(ConfData, key);
            if (valueFromPrefixMap.Count == 0)
            {
                return false;
            }
            rawValue = valueFromPrefixMap;
            return true;
        }
    }

    /// <summary>
    /// This method will do the following steps to get the value of a config option:
    ///
    /// <para>1. get the value from the configuration;<br/>
    /// 2. if the key is not found, try to get the value with fallback keys;<br/>
    /// 3. if no fallback key value is found, return false.</para>
    /// </summary>
    private bool TryGetRawValueFromOption<T>(
        ConfigOption<T> configOption, [NotNullWhen(true)] out object? rawValue)
    {
        bool canBePrefixMap = ConfigurationUtils.CanBePrefixMap(configOption);
        if (TryGetRawValue(configOption.Key, canBePrefixMap, out rawValue))
        {
            return true;
        }
        if (configOption.HasFallbackKeys)
        {
            foreach (FallbackKey fallbackKey in configOption.FallbackKeys)
            {
                if (TryGetRawValue(fallbackKey.Key, canBePrefixMap, out rawValue))
                {
                    // PORT NOTE: Java logs deprecated/fallback key usage here; the port is silent
                    // until a logging abstraction is introduced.
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Applies the given predicate to the option's key and then, in order, to its fallback keys,
    /// returning the first successful application.
    /// </summary>
    private bool ApplyWithOption<T>(ConfigOption<T> option, Func<string, bool, bool> applier)
    {
        bool canBePrefixMap = ConfigurationUtils.CanBePrefixMap(option);
        if (applier(option.Key, canBePrefixMap))
        {
            return true;
        }
        if (option.HasFallbackKeys)
        {
            foreach (FallbackKey fallbackKey in option.FallbackKeys)
            {
                if (applier(fallbackKey.Key, canBePrefixMap))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // --------------------------------------------------------------------------------------------
    //  Serialization
    // --------------------------------------------------------------------------------------------

    private const byte TypeString = 0;
    private const byte TypeInt = 1;
    private const byte TypeLong = 2;
    private const byte TypeBoolean = 3;
    private const byte TypeFloat = 4;
    private const byte TypeDouble = 5;
    private const byte TypeBytes = 6;

    /// <summary>Reads key/value pairs written with <see cref="Write"/>. This binary format only
    /// supports the primitive value types (like Java's deprecated read/write pair).</summary>
    public virtual void Read(Core.Memory.IDataInputView input)
    {
        lock (ConfData)
        {
            int numberOfProperties = input.ReadInt();

            for (int i = 0; i < numberOfProperties; i++)
            {
                string key = Types.StringValue.ReadString(input)!;
                object value;

                byte type = input.ReadByte();
                switch (type)
                {
                    case TypeString:
                        value = Types.StringValue.ReadString(input)!;
                        break;
                    case TypeInt:
                        value = input.ReadInt();
                        break;
                    case TypeLong:
                        value = input.ReadLong();
                        break;
                    case TypeFloat:
                        value = input.ReadFloat();
                        break;
                    case TypeDouble:
                        value = input.ReadDouble();
                        break;
                    case TypeBoolean:
                        value = input.ReadBoolean();
                        break;
                    case TypeBytes:
                        byte[] bytes = new byte[input.ReadInt()];
                        input.ReadFully(bytes);
                        value = bytes;
                        break;
                    default:
                        throw new IOException(
                            $"Unrecognized type: {type}. This method is deprecated and might not"
                                + " work for all supported types.");
                }

                ConfData[key] = value;
            }
        }
    }

    /// <summary>Writes the key/value pairs in a simple binary format; see <see cref="Read"/>.</summary>
    public virtual void Write(Core.Memory.IDataOutputView output)
    {
        lock (ConfData)
        {
            output.WriteInt(ConfData.Count);

            foreach (KeyValuePair<string, object> entry in ConfData)
            {
                Types.StringValue.WriteString(entry.Key, output);
                object val = entry.Value;

                switch (val)
                {
                    case string s:
                        output.Write(TypeString);
                        Types.StringValue.WriteString(s, output);
                        break;
                    case int i:
                        output.Write(TypeInt);
                        output.WriteInt(i);
                        break;
                    case long l:
                        output.Write(TypeLong);
                        output.WriteLong(l);
                        break;
                    case float f:
                        output.Write(TypeFloat);
                        output.WriteFloat(f);
                        break;
                    case double d:
                        output.Write(TypeDouble);
                        output.WriteDouble(d);
                        break;
                    case byte[] bytes:
                        output.Write(TypeBytes);
                        output.WriteInt(bytes.Length);
                        output.Write(bytes);
                        break;
                    case bool b:
                        output.Write(TypeBoolean);
                        output.WriteBoolean(b);
                        break;
                    default:
                        throw new ArgumentException(
                            "Unrecognized type. This method is deprecated and might not work for"
                                + " all supported types.");
                }
            }
        }
    }

    // --------------------------------------------------------------------------------------------

    public override int GetHashCode()
    {
        int hash = 0;
        foreach (string s in ConfData.Keys)
        {
            hash ^= s.GetHashCode();
        }
        return hash;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj is not Configuration other)
        {
            return false;
        }

        Dictionary<string, object> otherConf = other.ConfData;

        // PORT NOTE: Java iterates only this side's entries, so a configuration compares
        // equal to any superset of itself (asymmetrically); the count check restores the
        // equals contract.
        if (ConfData.Count != otherConf.Count)
        {
            return false;
        }

        foreach (KeyValuePair<string, object> e in ConfData)
        {
            otherConf.TryGetValue(e.Key, out object? otherVal);
            if (otherVal == null || !ValueEquals(e.Value, otherVal))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Compares two stored raw values the way Java's <c>Object.equals</c> does: structurally.
    /// Java's List/Map values compare element-wise, while .NET collections compare by
    /// reference, so lists and maps are matched entry by entry here.
    /// </summary>
    private static bool ValueEquals(object thisVal, object otherVal)
    {
        if (thisVal is byte[] thisBytes)
        {
            return otherVal is byte[] otherBytes && thisBytes.SequenceEqual(otherBytes);
        }
        if (thisVal is System.Collections.IDictionary thisMap)
        {
            if (otherVal is not System.Collections.IDictionary otherMap
                || thisMap.Count != otherMap.Count)
            {
                return false;
            }
            foreach (System.Collections.DictionaryEntry entry in thisMap)
            {
                object? otherEntry = otherMap.Contains(entry.Key) ? otherMap[entry.Key] : null;
                if (entry.Value == null || otherEntry == null
                    ? !Equals(entry.Value, otherEntry)
                    : !ValueEquals(entry.Value, otherEntry))
                {
                    return false;
                }
            }
            return true;
        }
        if (thisVal is System.Collections.IList thisList)
        {
            if (otherVal is not System.Collections.IList otherList
                || thisList.Count != otherList.Count)
            {
                return false;
            }
            for (int i = 0; i < thisList.Count; i++)
            {
                object? thisItem = thisList[i];
                object? otherItem = otherList[i];
                if (thisItem == null || otherItem == null
                    ? !Equals(thisItem, otherItem)
                    : !ValueEquals(thisItem, otherItem))
                {
                    return false;
                }
            }
            return true;
        }
        return thisVal.Equals(otherVal);
    }

    public override string ToString()
    {
        IDictionary<string, string> hidden =
            ConfigurationUtils.HideSensitiveValues(
                ConfData.ToDictionary(e => e.Key, e => e.Value.ToString() ?? ""),
                Get(SecurityOptions.AdditionalSensitiveKeys) ?? []);
        // Java prints the java.util.Map toString format
        return "{" + string.Join(", ", hidden.Select(e => $"{e.Key}={e.Value}")) + "}";
    }
}
