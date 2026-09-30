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

namespace FlinkNet.Api.Common.State.V2;

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
[Experimental]
public interface IMapState<TKey, TValue> : IState
{
    /// <summary>
    /// Returns the current value associated with the given key asynchronously. When the state is
    /// not partitioned the returned value is the same for all inputs in a given operator instance.
    /// If state partitioning is applied, the value returned depends on the current operator input,
    /// as the operator maintains an independent state for each partition.
    /// </summary>
    /// <returns>The <see cref="IStateFuture{T}"/> that will return value corresponding to the
    /// current input. When no corresponding value for this key, the future will return
    /// <c>null</c>.</returns>
    IStateFuture<TValue?> AsyncGet(TKey key);

    /// <summary>
    /// Update the current value associated with the given key asynchronously. When the state is
    /// not partitioned the value is updated for all inputs in a given operator instance. If state
    /// partitioning is applied, the updated value depends on the current operator input, as the
    /// operator maintains an independent state for each partition. When a <c>null</c> value is
    /// provided, the state for the given key will be removed.
    /// </summary>
    /// <param name="key">The key that will be updated.</param>
    /// <param name="value">The new value for the key.</param>
    /// <returns>The <see cref="IStateFuture{T}"/> that will trigger the callback when update
    /// finishes.</returns>
    IStateFuture<object?> AsyncPut(TKey key, TValue? value);

    /// <summary>
    /// Update all of the mappings from the given map into the state asynchronously. When the state
    /// is not partitioned the value is updated for all inputs in a given operator instance. If
    /// state partitioning is applied, the updated mapping depends on the current operator input,
    /// as the operator maintains an independent state for each partition. When a <c>null</c> value
    /// is provided within the map, the state for the corresponding key will be removed.
    /// <para>If an empty map is passed in, the state value remains unchanged.</para>
    /// <para>Null map pointer is not allowed.</para>
    /// </summary>
    /// <param name="map">The mappings to be stored in this state.</param>
    /// <returns>The <see cref="IStateFuture{T}"/> that will trigger the callback when update
    /// finishes.</returns>
    IStateFuture<object?> AsyncPutAll(IDictionary<TKey, TValue> map);

    /// <summary>
    /// Delete the mapping of the given key from the state asynchronously. When the state is not
    /// partitioned the deleted value is the same for all inputs in a given operator instance. If
    /// state partitioning is applied, the value deleted depends on the current operator input, as
    /// the operator maintains an independent state for each partition.
    /// </summary>
    /// <param name="key">The key of the mapping.</param>
    /// <returns>The <see cref="IStateFuture{T}"/> that will trigger the callback when update
    /// finishes.</returns>
    IStateFuture<object?> AsyncRemove(TKey key);

    /// <summary>
    /// Returns whether there exists the given mapping asynchronously. When the state is not
    /// partitioned the returned value is the same for all inputs in a given operator instance. If
    /// state partitioning is applied, the value returned depends on the current operator input, as
    /// the operator maintains an independent state for each partition.
    /// </summary>
    /// <param name="key">The key of the mapping.</param>
    /// <returns>The <see cref="IStateFuture{T}"/> that will return true if there exists a mapping
    /// whose key equals to the given key.</returns>
    IStateFuture<bool> AsyncContains(TKey key);

    /// <summary>
    /// Returns the current iterator for all the mappings of this state asynchronously. When the
    /// state is not partitioned the returned iterator is the same for all inputs in a given
    /// operator instance. If state partitioning is applied, the iterator returned depends on the
    /// current operator input, as the operator maintains an independent state for each partition.
    /// </summary>
    /// <returns>The <see cref="IStateFuture{T}"/> that will return mapping iterator corresponding
    /// to the current input.</returns>
    IStateFuture<IStateIterator<KeyValuePair<TKey, TValue>>> AsyncEntries();

    /// <summary>
    /// Returns the current iterator for all the keys of this state asynchronously. When the state
    /// is not partitioned the returned iterator is the same for all inputs in a given operator
    /// instance. If state partitioning is applied, the iterator returned depends on the current
    /// operator input, as the operator maintains an independent state for each partition.
    /// </summary>
    /// <returns>The <see cref="IStateFuture{T}"/> that will return key iterator corresponding to
    /// the current input.</returns>
    IStateFuture<IStateIterator<TKey>> AsyncKeys();

    /// <summary>
    /// Returns the current iterator for all the values of this state asynchronously. When the
    /// state is not partitioned the returned iterator is the same for all inputs in a given
    /// operator instance. If state partitioning is applied, the iterator returned depends on the
    /// current operator input, as the operator maintains an independent state for each partition.
    /// </summary>
    /// <returns>The <see cref="IStateFuture{T}"/> that will return value iterator corresponding to
    /// the current input.</returns>
    IStateFuture<IStateIterator<TValue>> AsyncValues();

    /// <summary>
    /// Returns whether this state contains no key-value mappings asynchronously. When the state is
    /// not partitioned the returned value is the same for all inputs in a given operator instance.
    /// If state partitioning is applied, the value returned depends on the current operator input,
    /// as the operator maintains an independent state for each partition.
    /// </summary>
    /// <returns>The <see cref="IStateFuture{T}"/> that will return true if there is no key-value
    /// mapping, otherwise false.</returns>
    IStateFuture<bool> AsyncIsEmpty();

    /// <summary>
    /// Returns the current value associated with the given key.
    /// </summary>
    /// <param name="key">The key of the mapping.</param>
    /// <returns>The value of the mapping with the given key, or <c>null</c> if no mapping for the
    /// given key exists.</returns>
    TValue? Get(TKey key);

    /// <summary>
    /// Associates a new value with the given key.
    /// </summary>
    /// <param name="key">The key of the mapping.</param>
    /// <param name="value">The new value of the mapping.</param>
    void Put(TKey key, TValue value);

    /// <summary>
    /// Copies all of the mappings from the given map into the state.
    /// </summary>
    /// <param name="map">The mappings to be stored in this state.</param>
    void PutAll(IDictionary<TKey, TValue> map);

    /// <summary>
    /// Deletes the mapping of the given key.
    /// </summary>
    /// <param name="key">The key of the mapping.</param>
    void Remove(TKey key);

    /// <summary>
    /// Returns whether there exists the given mapping.
    /// </summary>
    /// <param name="key">The key of the mapping.</param>
    /// <returns>True if there exists a mapping whose key equals to the given key.</returns>
    bool Contains(TKey key);

    /// <summary>
    /// Returns all the mappings in the state.
    /// </summary>
    /// <returns>An iterable view of all the key-value pairs in the state.</returns>
    IEnumerable<KeyValuePair<TKey, TValue>> Entries();

    /// <summary>
    /// Returns all the keys in the state.
    /// </summary>
    /// <returns>An iterable view of all the keys in the state.</returns>
    IEnumerable<TKey> Keys();

    /// <summary>
    /// Returns all the values in the state.
    /// </summary>
    /// <returns>An iterable view of all the values in the state.</returns>
    IEnumerable<TValue> Values();

    /// <summary>
    /// Returns true if this state contains no key-value mappings, otherwise false.
    /// </summary>
    /// <returns>True if this state contains no key-value mappings, otherwise false.</returns>
    bool IsEmpty();
}
