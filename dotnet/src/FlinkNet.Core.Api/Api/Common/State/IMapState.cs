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

namespace FlinkNet.Api.Common.State;

/// <summary>
/// <see cref="IState"/> interface for partitioned key-value state. The key-value pair can be
/// added, updated and retrieved.
/// <para>The state is accessed and modified by user functions, and checkpointed consistently by
/// the system as part of the distributed snapshots.</para>
/// <para>The state is only accessible by functions applied on a <c>KeyedStream</c>. The key is
/// automatically supplied by the system, so the function always sees the value mapped to the key
/// of the current element. That way, the system can handle stream and state partitioning
/// consistently together.</para>
/// <para>The user value could be null, but change log state backend is not compatible with the
/// user value is null, see FLINK-38144 for more details.</para>
/// </summary>
/// <typeparam name="TKey">Type of the keys in the state.</typeparam>
/// <typeparam name="TValue">Type of the values in the state.</typeparam>
[PublicEvolving]
public interface IMapState<TKey, TValue> : IState
{
    /// <summary>
    /// Returns the current value associated with the given key.
    /// </summary>
    /// <param name="key">The key of the mapping.</param>
    /// <returns>The value of the mapping with the given key, or <c>null</c> if no mapping for the
    /// given key exists.</returns>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    TValue? Get(TKey key);

    /// <summary>
    /// Associates a new value with the given key.
    /// </summary>
    /// <param name="key">The key of the mapping.</param>
    /// <param name="value">The new value of the mapping.</param>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    void Put(TKey key, TValue value);

    /// <summary>
    /// Copies all of the mappings from the given map into the state.
    /// </summary>
    /// <param name="map">The mappings to be stored in this state.</param>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    void PutAll(IDictionary<TKey, TValue> map);

    /// <summary>
    /// Deletes the mapping of the given key.
    /// </summary>
    /// <param name="key">The key of the mapping.</param>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    void Remove(TKey key);

    /// <summary>
    /// Returns whether there exists the given mapping.
    /// </summary>
    /// <param name="key">The key of the mapping.</param>
    /// <returns>True if there exists a mapping whose key equals to the given key.</returns>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    bool Contains(TKey key);

    /// <summary>
    /// Returns all the mappings in the state.
    /// </summary>
    /// <returns>An iterable view of all the key-value pairs in the state.</returns>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    IEnumerable<KeyValuePair<TKey, TValue>> Entries();

    /// <summary>
    /// Returns all the keys in the state.
    /// </summary>
    /// <returns>An iterable view of all the keys in the state.</returns>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    IEnumerable<TKey> Keys();

    /// <summary>
    /// Returns all the values in the state.
    /// </summary>
    /// <returns>An iterable view of all the values in the state.</returns>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    IEnumerable<TValue> Values();

    /// <summary>
    /// Returns true if this state contains no key-value mappings, otherwise false.
    /// </summary>
    /// <returns>True if this state contains no key-value mappings, otherwise false.</returns>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    bool IsEmpty();
}
