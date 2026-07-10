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
/// General utilities for parsing values to configuration options.
///
/// <para>PORT NOTE: Java's reflection-based <c>getAllConfigOptions(Class)</c> relies on the raw
/// (erased) <c>ConfigOption</c> type and is deferred until the port introduces a non-generic
/// <c>ConfigOption</c> abstraction for the documentation generators.</para>
/// </summary>
[Internal]
public static class ConfigUtils
{
    /// <summary>
    /// Puts an array of values of type <typeparamref name="TIn"/> in a <see cref="IWritableConfig"/>
    /// as a <see cref="ConfigOption{T}"/> of type list of type <typeparamref name="TOut"/>. If the
    /// values is null or empty, then nothing is put in the configuration.
    /// </summary>
    /// <param name="configuration">the configuration object to put the list in</param>
    /// <param name="key">the option under which the list will be stored</param>
    /// <param name="values">the array of values to put in the configuration</param>
    /// <param name="mapper">the transformation function from <typeparamref name="TIn"/> to
    /// <typeparamref name="TOut"/></param>
    public static void EncodeArrayToConfig<TIn, TOut>(
        IWritableConfig configuration,
        ConfigOption<IList<TOut>> key,
        TIn[]? values,
        Func<TIn, TOut?> mapper)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(mapper);

        if (values == null)
        {
            return;
        }

        EncodeCollectionToConfig(configuration, key, values, mapper);
    }

    /// <summary>
    /// Puts a collection of values of type <typeparamref name="TIn"/> in a
    /// <see cref="IWritableConfig"/> as a <see cref="ConfigOption{T}"/> of type list of type
    /// <typeparamref name="TOut"/>. If the collection is null or empty, then nothing is put in the
    /// configuration.
    /// </summary>
    /// <param name="configuration">the configuration object to put the list in</param>
    /// <param name="key">the option under which the list will be stored</param>
    /// <param name="values">the collection of values to put in the configuration</param>
    /// <param name="mapper">the transformation function from <typeparamref name="TIn"/> to
    /// <typeparamref name="TOut"/></param>
    public static void EncodeCollectionToConfig<TIn, TOut>(
        IWritableConfig configuration,
        ConfigOption<IList<TOut>> key,
        ICollection<TIn>? values,
        Func<TIn, TOut?> mapper)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(mapper);

        if (values == null)
        {
            return;
        }

        IList<TOut> encodedOption =
            values.Where(v => v is not null)
                .Select(mapper)
                .Where(v => v is not null)
                .Cast<TOut>()
                .ToList();

        if (encodedOption.Count != 0)
        {
            configuration.Set(key, encodedOption);
        }
    }

    /// <summary>
    /// Gets a <see cref="ConfigOption{T}"/> of type list of type <typeparamref name="TIn"/> from a
    /// <see cref="IReadableConfig"/> and transforms it to a list of type
    /// <typeparamref name="TOut"/> based on the provided mapper function.
    /// </summary>
    /// <param name="configuration">the configuration object to get the value out of</param>
    /// <param name="key">the option for which to retrieve the value</param>
    /// <param name="mapper">the transformation function from <typeparamref name="TIn"/> to
    /// <typeparamref name="TOut"/></param>
    /// <returns>the transformed values in a list of type <typeparamref name="TOut"/></returns>
    public static IList<TOut> DecodeListFromConfig<TIn, TOut>(
        IReadableConfig configuration,
        ConfigOption<IList<TIn>> key,
        Func<TIn, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(mapper);

        IList<TIn>? encodedString = configuration.Get(key);
        if (encodedString == null || encodedString.Count == 0)
        {
            return new List<TOut>();
        }

        var result = new List<TOut>(encodedString.Count);
        foreach (TIn input in encodedString)
        {
            result.Add(mapper(input));
        }
        return result;
    }
}
