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
/// Read access to a configuration object. Allows reading values described with meta information
/// included in <see cref="ConfigOption{T}"/>.
/// </summary>
[PublicEvolving]
public interface IReadableConfig
{
    /// <summary>
    /// Reads a value using the metadata included in <see cref="ConfigOption{T}"/>. Returns the
    /// <see cref="ConfigOption{T}.DefaultValue"/> if value key not present in the configuration.
    /// </summary>
    /// <typeparam name="T">type of the value to read</typeparam>
    /// <param name="option">metadata of the option to read</param>
    /// <returns>read value or <see cref="ConfigOption{T}.DefaultValue"/> if not found</returns>
    /// <seealso cref="TryGet{T}(ConfigOption{T}, out T)"/>
    T? Get<T>(ConfigOption<T> option);

    /// <summary>
    /// Reads a value using the metadata included in <see cref="ConfigOption{T}"/>. In contrast to
    /// <see cref="Get{T}(ConfigOption{T})"/> returns <c>false</c> if the value is not present.
    ///
    /// <para>PORT NOTE: this is the port of Java's <c>Optional&lt;T&gt; getOptional(ConfigOption&lt;T&gt;)</c>,
    /// mapped to the .NET try-pattern because <c>T?</c> cannot represent "absent" for value types.</para>
    /// </summary>
    /// <typeparam name="T">type of the value to read</typeparam>
    /// <param name="option">metadata of the option to read</param>
    /// <param name="value">the read value, when present</param>
    /// <returns>true if the value was present, false otherwise</returns>
    /// <seealso cref="Get{T}(ConfigOption{T})"/>
    bool TryGet<T>(ConfigOption<T> option, [MaybeNullWhen(false)] out T value);

    /// <summary>
    /// Converts the configuration items into a map of string key-value pairs.
    /// </summary>
    /// <returns>a map containing the configuration items, where the value is the string
    /// representation of the corresponding configuration item</returns>
    IDictionary<string, string> ToMap();
}
