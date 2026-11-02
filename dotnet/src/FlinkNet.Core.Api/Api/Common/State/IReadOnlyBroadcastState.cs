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
/// A read-only view of the <see cref="IBroadcastState{TKey, TValue}"/>.
/// <para>Although read-only, the user code should not modify the value returned by the
/// <see cref="Get"/> or the entries of the immutable iterator returned by the
/// <see cref="ImmutableEntries"/>, as this can lead to inconsistent states. The reason for this is
/// that we do not create extra copies of the elements for performance reasons.</para>
/// </summary>
/// <typeparam name="TKey">The key type of the elements in the
/// <see cref="IReadOnlyBroadcastState{TKey, TValue}"/>.</typeparam>
/// <typeparam name="TValue">The value type of the elements in the
/// <see cref="IReadOnlyBroadcastState{TKey, TValue}"/>.</typeparam>
[PublicEvolving]
public interface IReadOnlyBroadcastState<TKey, TValue> : IState
{
    /// <summary>
    /// Returns the current value associated with the given key.
    /// <para>The user code must not modify the value returned, as this can lead to inconsistent
    /// states.</para>
    /// </summary>
    /// <param name="key">The key of the mapping.</param>
    /// <returns>The value of the mapping with the given key, or <c>null</c> if no mapping for the
    /// given key exists.</returns>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    TValue? Get(TKey key);

    /// <summary>
    /// Returns whether there exists the given mapping.
    /// </summary>
    /// <param name="key">The key of the mapping.</param>
    /// <returns>True if there exists a mapping whose key equals to the given key.</returns>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    bool Contains(TKey key);

    /// <summary>
    /// Returns an immutable <see cref="IEnumerable{T}"/> over the entries in the state.
    /// <para>The user code must not modify the entries of the returned immutable iterator, as this
    /// can lead to inconsistent states.</para>
    /// </summary>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    IEnumerable<KeyValuePair<TKey, TValue>> ImmutableEntries();
}
