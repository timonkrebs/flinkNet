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

namespace FlinkNet.Configuration;

/// <summary>
/// A configuration that manages a subset of keys with a common prefix from a given configuration.
///
/// <para>PORT NOTE: <c>addAllToProperties</c> is deferred together with its carrier
/// (see Configuration).</para>
/// </summary>
public sealed class DelegatingConfiguration : Configuration
{
    /// <summary>The configuration actually storing the data.</summary>
    private readonly Configuration _backingConfig;

    /// <summary>The prefix key by which keys for this config are marked.
    /// Not readonly because <see cref="Read"/> replaces it, as in Java.</summary>
    private string _prefix;

    // --------------------------------------------------------------------------------------------

    /// <summary>Default constructor for serialization. Creates an empty delegating configuration.</summary>
    public DelegatingConfiguration()
        : this(new Configuration(), "")
    {
    }

    /// <summary>
    /// Creates a new delegating configuration which stores its key/value pairs in the given
    /// configuration using the specified key prefix.
    /// </summary>
    /// <param name="backingConfig">The configuration holding the actual config data.</param>
    /// <param name="prefix">The prefix prepended to all config keys.</param>
    public DelegatingConfiguration(Configuration backingConfig, string prefix)
    {
        ArgumentNullException.ThrowIfNull(backingConfig);
        ArgumentNullException.ThrowIfNull(prefix, "The 'prefix' attribute mustn't be null.");
        _backingConfig = backingConfig;
        _prefix = prefix;
    }

    // --------------------------------------------------------------------------------------------

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public override string? GetString(string key, string? defaultValue) =>
        _backingConfig.GetString(_prefix + key, defaultValue);

    public override void SetString(string key, string value) =>
        _backingConfig.SetString(_prefix + key, value);

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public override byte[]? GetBytes(string key, byte[]? defaultValue) =>
        _backingConfig.GetBytes(_prefix + key, defaultValue);

    public override void SetBytes(string key, byte[] bytes) =>
        _backingConfig.SetBytes(_prefix + key, bytes);

    public override string? GetValue<T>(ConfigOption<T> configOption) =>
        _backingConfig.GetValue(PrefixOption(configOption, _prefix));

    public override TEnum GetEnum<TEnum>(ConfigOption<string> configOption) =>
        _backingConfig.GetEnum<TEnum>(PrefixOption(configOption, _prefix));

    public override void AddAll(Configuration other) => AddAll(other, "");

    public override void AddAll(Configuration other, string prefix) =>
        _backingConfig.AddAll(other, _prefix + prefix);

    public override string ToString() => _backingConfig.ToString();

    public override ISet<string> KeySet()
    {
        if (_prefix.Length == 0)
        {
            return _backingConfig.KeySet();
        }

        var set = new HashSet<string>();
        int prefixLen = _prefix.Length;

        foreach (string key in _backingConfig.KeySet())
        {
            if (key.StartsWith(_prefix, StringComparison.Ordinal))
            {
                set.Add(key[prefixLen..]);
            }
        }

        return set;
    }

    public override Configuration Clone() =>
        new DelegatingConfiguration(_backingConfig.Clone(), _prefix);

    public override IDictionary<string, string> ToMap()
    {
        IDictionary<string, string> map = _backingConfig.ToMap();
        var prefixed = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> entry in map)
        {
            if (entry.Key.StartsWith(_prefix, StringComparison.Ordinal))
            {
                prefixed[entry.Key[_prefix.Length..]] = entry.Value;
            }
        }
        return prefixed;
    }

    public override IDictionary<string, string> ToFileWritableMap()
    {
        IDictionary<string, string> map = _backingConfig.ToFileWritableMap();
        var prefixed = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> entry in map)
        {
            if (entry.Key.StartsWith(_prefix, StringComparison.Ordinal))
            {
                // PORT NOTE: Java escapes the backing values a second time here (upstream
                // bug: a delegated "*" round-trips through a file as "'*'"); the backing
                // ToFileWritableMap already escaped them, so pass them through like ToMap.
                prefixed[entry.Key[_prefix.Length..]] = entry.Value;
            }
        }
        return prefixed;
    }

    public override bool RemoveConfig<T>(ConfigOption<T> configOption) =>
        _backingConfig.RemoveConfig(PrefixOption(configOption, _prefix));

    public override bool RemoveKey(string key) => _backingConfig.RemoveKey(_prefix + key);

    public override bool ContainsKey(string key) => _backingConfig.ContainsKey(_prefix + key);

    public override bool Contains<T>(ConfigOption<T> configOption) =>
        _backingConfig.Contains(PrefixOption(configOption, _prefix));

    public override T? Get<T>(ConfigOption<T> option) where T : default =>
        _backingConfig.Get(PrefixOption(option, _prefix));

    public override T Get<T>(ConfigOption<T> configOption, T overrideDefault) =>
        _backingConfig.Get(PrefixOption(configOption, _prefix), overrideDefault);

    public override bool TryGet<T>(ConfigOption<T> option, [MaybeNullWhen(false)] out T value) =>
        _backingConfig.TryGet(PrefixOption(option, _prefix), out value);

    public override Configuration Set<T>(ConfigOption<T> option, T value)
    {
        _backingConfig.Set(PrefixOption(option, _prefix), value);
        return this;
    }

    protected internal override void SetValueInternal<T>(string key, T value, bool canBePrefixMap) =>
        _backingConfig.SetValueInternal(_prefix + key, value, canBePrefixMap);

    // --------------------------------------------------------------------------------------------

    public override void Read(Core.Memory.IDataInputView input)
    {
        _prefix = input.ReadUTF();
        _backingConfig.Read(input);
    }

    public override void Write(Core.Memory.IDataOutputView output)
    {
        output.WriteUTF(_prefix);
        _backingConfig.Write(output);
    }

    // --------------------------------------------------------------------------------------------

    public override int GetHashCode() => _prefix.GetHashCode() ^ _backingConfig.GetHashCode();

    public override bool Equals(object? obj) =>
        obj is DelegatingConfiguration other
            && _prefix == other._prefix
            && _backingConfig.Equals(other._backingConfig);

    // --------------------------------------------------------------------------------------------

    private static ConfigOption<T> PrefixOption<T>(ConfigOption<T> option, string prefix)
    {
        string key = prefix + option.Key;

        var deprecatedKeys = new List<FallbackKey>();
        if (option.HasFallbackKeys)
        {
            foreach (FallbackKey dk in option.FallbackKeys)
            {
                deprecatedKeys.Add(FallbackKey.CreateDeprecatedKey(prefix + dk.Key));
            }
        }

        return new ConfigOption<T>(
            key,
            option.Clazz,
            option.Description,
            option.DefaultValueBoxed,
            option.IsList,
            deprecatedKeys.ToArray());
    }
}
