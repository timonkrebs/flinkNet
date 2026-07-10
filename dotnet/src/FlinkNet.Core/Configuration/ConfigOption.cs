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
// The child namespace FlinkNet.Configuration.Description shadows the type name, so the
// Description type needs an alias inside this namespace.
using DescriptionDoc = FlinkNet.Configuration.Description.Description;

namespace FlinkNet.Configuration;

/// <summary>
/// A <c>ConfigOption</c> describes a configuration parameter. It encapsulates the configuration
/// key, deprecated older versions of the key, and an optional default value for the configuration
/// parameter.
///
/// <para><c>ConfigOptions</c> are built via the <see cref="ConfigOptions"/> class. Once created, a
/// config option is immutable.</para>
/// </summary>
/// <typeparam name="T">The type of value associated with the configuration option.</typeparam>
[PublicEvolving]
public class ConfigOption<T>
{
    /// <summary>The current key for that config option.</summary>
    private readonly string _key;

    /// <summary>The list of deprecated keys, in the order to be checked.</summary>
    private readonly FallbackKey[] _fallbackKeys;

    /// <summary>
    /// The default value for this option, stored boxed.
    ///
    /// <para>PORT NOTE: Java relies on boxed <c>null</c> to express "no default". For C# value
    /// types <c>default(T)</c> cannot express absence, so the default is kept boxed internally
    /// and <c>null</c> means "no default value".</para>
    /// </summary>
    private readonly object? _defaultValue;

    /// <summary>The description for this option.</summary>
    private readonly DescriptionDoc _description;

    /// <summary>
    /// Type of the value that this ConfigOption describes.
    /// <list type="bullet">
    ///   <item><description>typeClass == atomic type (e.g. <c>typeof(int)</c>) for <c>ConfigOption&lt;int&gt;</c></description></item>
    ///   <item><description>typeClass == <c>typeof(IDictionary&lt;string, string&gt;)</c> for <c>ConfigOption&lt;IDictionary&lt;string, string&gt;&gt;</c></description></item>
    ///   <item><description>typeClass == atomic type and isList == true for <c>ConfigOption&lt;IList&lt;int&gt;&gt;</c></description></item>
    /// </list>
    /// </summary>
    private readonly Type _clazz;

    private readonly bool _isList;

    // ------------------------------------------------------------------------

    internal Type Clazz => _clazz;

    internal bool IsList => _isList;

    internal object? DefaultValueBoxed => _defaultValue;

    /// <summary>
    /// Creates a new config option with fallback keys.
    /// </summary>
    /// <param name="key">The current key for that config option</param>
    /// <param name="clazz">describes type of the ConfigOption, see description of the clazz field</param>
    /// <param name="description">Description for that option</param>
    /// <param name="defaultValue">The default value for this option (boxed; null means no default)</param>
    /// <param name="isList">tells if the ConfigOption describes a list option, see description of
    /// the clazz field</param>
    /// <param name="fallbackKeys">The list of fallback keys, in the order to be checked</param>
    internal ConfigOption(
        string key,
        Type clazz,
        DescriptionDoc description,
        object? defaultValue,
        bool isList,
        params FallbackKey[] fallbackKeys)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(clazz);
        _key = key;
        _description = description;
        _defaultValue = defaultValue;
        _fallbackKeys =
            fallbackKeys == null || fallbackKeys.Length == 0
                ? ConfigOptionShared.EmptyFallbackKeys
                : fallbackKeys;
        _clazz = clazz;
        _isList = isList;
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Creates a new config option, using this option's key and default value, and adding the
    /// given fallback keys.
    ///
    /// <para>When obtaining a value from the configuration via
    /// <see cref="Configuration.GetValue{T}(ConfigOption{T})"/>, the fallback keys will be checked
    /// in the order provided to this method. The first key for which a value is found will be
    /// used - that value will be returned.</para>
    /// </summary>
    /// <param name="fallbackKeys">The fallback keys, in the order in which they should be checked.</param>
    /// <returns>A new config options, with the given fallback keys.</returns>
    public ConfigOption<T> WithFallbackKeys(params string[] fallbackKeys)
    {
        IEnumerable<FallbackKey> newFallbackKeys = fallbackKeys.Select(FallbackKey.CreateFallbackKey);

        // put fallback keys first so that they are prioritized
        FallbackKey[] mergedAlternativeKeys = newFallbackKeys.Concat(_fallbackKeys).ToArray();
        return new ConfigOption<T>(
            _key, _clazz, _description, _defaultValue, _isList, mergedAlternativeKeys);
    }

    /// <summary>
    /// Creates a new config option, using this option's key and default value, and adding the
    /// given deprecated keys.
    ///
    /// <para>When obtaining a value from the configuration via
    /// <see cref="Configuration.GetValue{T}(ConfigOption{T})"/>, the deprecated keys will be
    /// checked in the order provided to this method. The first key for which a value is found will
    /// be used - that value will be returned.</para>
    /// </summary>
    /// <param name="deprecatedKeys">The deprecated keys, in the order in which they should be checked.</param>
    /// <returns>A new config options, with the given deprecated keys.</returns>
    public ConfigOption<T> WithDeprecatedKeys(params string[] deprecatedKeys)
    {
        IEnumerable<FallbackKey> newDeprecatedKeys =
            deprecatedKeys.Select(FallbackKey.CreateDeprecatedKey);

        // put deprecated keys last so that they are de-prioritized
        FallbackKey[] mergedAlternativeKeys = _fallbackKeys.Concat(newDeprecatedKeys).ToArray();
        return new ConfigOption<T>(
            _key, _clazz, _description, _defaultValue, _isList, mergedAlternativeKeys);
    }

    /// <summary>
    /// Creates a new config option, using this option's key and default value, and adding the
    /// given description. The given description is used when generating the configuration
    /// documentation.
    /// </summary>
    /// <param name="description">The description for this option.</param>
    /// <returns>A new config option, with given description.</returns>
    public ConfigOption<T> WithDescription(string description) =>
        WithDescription(DescriptionDoc.Builder().Text(description).Build());

    /// <summary>
    /// Creates a new config option, using this option's key and default value, and adding the
    /// given description. The given description is used when generating the configuration
    /// documentation.
    /// </summary>
    /// <param name="description">The description for this option.</param>
    /// <returns>A new config option, with given description.</returns>
    public ConfigOption<T> WithDescription(DescriptionDoc description) =>
        new(_key, _clazz, description, _defaultValue, _isList, _fallbackKeys);

    // ------------------------------------------------------------------------

    /// <summary>Gets the configuration key.</summary>
    public string Key => _key;

    /// <summary>Checks if this option has a default value.</summary>
    public bool HasDefaultValue => _defaultValue is not null;

    /// <summary>
    /// Returns the default value, or <c>default(T)</c> if there is no default value.
    ///
    /// <para>PORT NOTE: Java returns boxed <c>null</c> when there is no default. For C# value
    /// types this property returns <c>default(T)</c> instead; use <see cref="HasDefaultValue"/>
    /// to distinguish an absent default.</para>
    /// </summary>
    public T? DefaultValue => _defaultValue is null ? default : (T)_defaultValue;

    /// <summary>Checks whether this option has fallback keys.</summary>
    public bool HasFallbackKeys => !ReferenceEquals(_fallbackKeys, ConfigOptionShared.EmptyFallbackKeys);

    /// <summary>Gets the fallback keys, in the order to be checked.</summary>
    public IEnumerable<FallbackKey> FallbackKeys => _fallbackKeys;

    /// <summary>Returns the description of this option.</summary>
    public DescriptionDoc Description => _description;

    // ------------------------------------------------------------------------

    public override bool Equals(object? o)
    {
        if (ReferenceEquals(this, o))
        {
            return true;
        }
        if (o is not null && o.GetType() == GetType())
        {
            var that = (ConfigOption<T>)o;
            return _key == that._key
                && _fallbackKeys.SequenceEqual(that._fallbackKeys)
                && ConfigOptionShared.DefaultValuesEqual(_defaultValue, that._defaultValue);
        }
        return false;
    }

    public override int GetHashCode() =>
        31 * _key.GetHashCode()
            + 17 * _fallbackKeys.Aggregate(1, (hash, key) => 31 * hash + key.GetHashCode())
            + ConfigOptionShared.DefaultValueHashCode(_defaultValue);

    public override string ToString() =>
        string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            "Key: '{0}' , default: {1} (fallback keys: [{2}])",
            _key,
            ConfigOptionShared.DefaultValueToString(_defaultValue),
            string.Join(", ", (IEnumerable<FallbackKey>)_fallbackKeys));
}

/// <summary>
/// Shared (non-generic) helpers and constants for <see cref="ConfigOption{T}"/>. In Java these
/// are static members of the erased <c>ConfigOption</c> class.
/// </summary>
internal static class ConfigOptionShared
{
    internal static readonly FallbackKey[] EmptyFallbackKeys = [];

    internal static readonly DescriptionDoc EmptyDescription =
        DescriptionDoc.Builder().Text("").Build();

    /// <summary>
    /// Structural equality for boxed default values. List-typed defaults compare element-wise
    /// (Java's <c>List.equals</c> semantics, which .NET lists do not have).
    /// </summary>
    internal static bool DefaultValuesEqual(object? left, object? right)
    {
        if (left is null || right is null)
        {
            return left is null && right is null;
        }
        if (left is not string
            && left is System.Collections.IEnumerable leftSeq
            && right is System.Collections.IEnumerable rightSeq)
        {
            return leftSeq.Cast<object?>().SequenceEqual(rightSeq.Cast<object?>());
        }
        return left.Equals(right);
    }

    internal static int DefaultValueHashCode(object? defaultValue)
    {
        if (defaultValue is null)
        {
            return 0;
        }
        if (defaultValue is not string && defaultValue is System.Collections.IEnumerable seq)
        {
            return seq.Cast<object?>()
                .Aggregate(1, (hash, element) => 31 * hash + (element?.GetHashCode() ?? 0));
        }
        return defaultValue.GetHashCode();
    }

    /// <summary>Renders a boxed default for ToString, mirroring Java's implicit conversions.</summary>
    internal static string DefaultValueToString(object? defaultValue)
    {
        if (defaultValue is null)
        {
            return "null";
        }
        if (defaultValue is not string && defaultValue is System.Collections.IEnumerable seq)
        {
            return "[" + string.Join(", ", seq.Cast<object?>()) + "]";
        }
        return defaultValue.ToString() ?? "null";
    }
}
