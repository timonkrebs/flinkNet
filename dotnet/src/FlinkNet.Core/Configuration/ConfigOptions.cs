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
/// <c>ConfigOptions</c> are used to build a <see cref="ConfigOption{T}"/>. The option is typically
/// built in one of the following patterns:
///
/// <code><![CDATA[
/// // simple string-valued option with a default value
/// ConfigOption<string> tempDirs =
///     ConfigOptions.Key("tmp.dir").StringType().DefaultValue("/tmp");
///
/// // simple integer-valued option with a default value
/// ConfigOption<int> parallelism =
///     ConfigOptions.Key("application.parallelism").IntType().DefaultValue(100);
///
/// // option of list of integers with a default value
/// ConfigOption<IList<int>> parallelisms =
///     ConfigOptions.Key("application.parallelisms").IntType().AsList().DefaultValues(1, 2, 3);
///
/// // option with no default value
/// ConfigOption<string> userName = ConfigOptions.Key("user.name").StringType().NoDefaultValue();
///
/// // option with deprecated keys to check
/// ConfigOption<double> threshold =
///     ConfigOptions.Key("cpu.utilization.threshold")
///         .DoubleType()
///         .DefaultValue(0.9)
///         .WithDeprecatedKeys("cpu.threshold");
/// ]]></code>
/// </summary>
[PublicEvolving]
public static class ConfigOptions
{
    /// <summary>
    /// Starts building a new <see cref="ConfigOption{T}"/>.
    /// </summary>
    /// <param name="key">The key for the config option.</param>
    /// <returns>The builder for the config option with the given key.</returns>
    public static OptionBuilder Key(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return new OptionBuilder(key);
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// The option builder is used to create a <see cref="ConfigOption{T}"/>. It is instantiated
    /// via <see cref="ConfigOptions.Key(string)"/>.
    /// </summary>
    public sealed class OptionBuilder
    {
        /// <summary>The key for the config option.</summary>
        private readonly string _key;

        /// <summary>Creates a new OptionBuilder.</summary>
        /// <param name="key">The key for the config option</param>
        internal OptionBuilder(string key)
        {
            _key = key;
        }

        /// <summary>Defines that the value of the option should be of <see cref="bool"/> type.</summary>
        public TypedConfigOptionBuilder<bool> BooleanType() => new(_key, typeof(bool));

        /// <summary>Defines that the value of the option should be of <see cref="int"/> type.</summary>
        public TypedConfigOptionBuilder<int> IntType() => new(_key, typeof(int));

        /// <summary>Defines that the value of the option should be of <see cref="long"/> type.</summary>
        public TypedConfigOptionBuilder<long> LongType() => new(_key, typeof(long));

        /// <summary>Defines that the value of the option should be of <see cref="float"/> type.</summary>
        public TypedConfigOptionBuilder<float> FloatType() => new(_key, typeof(float));

        /// <summary>Defines that the value of the option should be of <see cref="double"/> type.</summary>
        public TypedConfigOptionBuilder<double> DoubleType() => new(_key, typeof(double));

        /// <summary>Defines that the value of the option should be of <see cref="string"/> type.</summary>
        public TypedConfigOptionBuilder<string> StringType() => new(_key, typeof(string));

        /// <summary>
        /// Defines that the value of the option should be of <see cref="TimeSpan"/> type
        /// (port of Java's <c>java.time.Duration</c>).
        /// </summary>
        public TypedConfigOptionBuilder<TimeSpan> DurationType() => new(_key, typeof(TimeSpan));

        /// <summary>Defines that the value of the option should be of <see cref="MemorySize"/> type.</summary>
        public TypedConfigOptionBuilder<MemorySize> MemoryType() => new(_key, typeof(MemorySize));

        /// <summary>
        /// Defines that the value of the option should be of <typeparamref name="TEnum"/> type.
        /// </summary>
        /// <typeparam name="TEnum">type of the option value</typeparam>
        public TypedConfigOptionBuilder<TEnum> EnumType<TEnum>()
            where TEnum : struct, Enum =>
            new(_key, typeof(TEnum));

        /// <summary>
        /// Defines that the value of the option should be a set of properties, which can be
        /// represented as <c>IDictionary&lt;string, string&gt;</c>.
        /// </summary>
        public TypedConfigOptionBuilder<IDictionary<string, string>> MapType() =>
            new(_key, typeof(IDictionary<string, string>));
    }

    /// <summary>
    /// Builder for <see cref="ConfigOption{T}"/> with a defined atomic type.
    /// </summary>
    /// <typeparam name="T">atomic type of the option</typeparam>
    public class TypedConfigOptionBuilder<T>
    {
        private readonly string _key;
        private readonly Type _clazz;

        internal TypedConfigOptionBuilder(string key, Type clazz)
        {
            _key = key;
            _clazz = clazz;
        }

        /// <summary>Defines that the option's type should be a list of previously defined atomic type.</summary>
        public ListConfigOptionBuilder<T> AsList() => new(_key, _clazz);

        /// <summary>
        /// Creates a ConfigOption with the given default value.
        /// </summary>
        /// <param name="value">The default value for the config option</param>
        /// <returns>The config option with the default value.</returns>
        public ConfigOption<T> DefaultValue(T value) =>
            new(_key, _clazz, ConfigOptionShared.EmptyDescription, value, false);

        /// <summary>
        /// Creates a ConfigOption without a default value.
        /// </summary>
        /// <returns>The config option without a default value.</returns>
        public ConfigOption<T> NoDefaultValue() =>
            new(_key, _clazz, ConfigOptionShared.EmptyDescription, null, false);
    }

    /// <summary>
    /// Builder for <see cref="ConfigOption{T}"/> of list of type <typeparamref name="TElement"/>.
    /// </summary>
    /// <typeparam name="TElement">list element type of the option</typeparam>
    public class ListConfigOptionBuilder<TElement>
    {
        private readonly string _key;
        private readonly Type _clazz;

        internal ListConfigOptionBuilder(string key, Type clazz)
        {
            _key = key;
            _clazz = clazz;
        }

        /// <summary>
        /// Creates a ConfigOption with the given default values.
        /// </summary>
        /// <param name="values">The list of default values for the config option</param>
        /// <returns>The config option with the default values.</returns>
        public ConfigOption<IList<TElement>> DefaultValues(params TElement[] values) =>
            new(_key, _clazz, ConfigOptionShared.EmptyDescription, new List<TElement>(values), true);

        /// <summary>
        /// Creates a ConfigOption without a default value.
        /// </summary>
        /// <returns>The config option without a default value.</returns>
        public ConfigOption<IList<TElement>> NoDefaultValue() =>
            new(_key, _clazz, ConfigOptionShared.EmptyDescription, null, true);
    }
}
