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
/// A type of state that can be created to store the state of a <c>BroadcastStream</c>. This state
/// assumes that <b>the same elements are sent to all instances of an operator.</b>
/// <para><b>CAUTION:</b> the user has to guarantee that all task instances store the same elements
/// in this type of state.</para>
/// <para>Each operator instance individually maintains and stores elements in the broadcast state.
/// The fact that the incoming stream is a broadcast one guarantees that all instances see all the
/// elements. Upon recovery or re-scaling, the same state is given to each of the instances. To
/// avoid hotspots, each task reads its previous partition, and if there are more tasks (scale up),
/// then the new instances read from the old instances in a round robin fashion. This is why each
/// instance has to guarantee that it stores the same elements as the rest. If not, upon recovery
/// or rescaling you may have unpredictable redistribution of the partitions, thus unpredictable
/// results.</para>
/// </summary>
/// <typeparam name="TKey">The key type of the elements in the
/// <see cref="IBroadcastState{TKey, TValue}"/>.</typeparam>
/// <typeparam name="TValue">The value type of the elements in the
/// <see cref="IBroadcastState{TKey, TValue}"/>.</typeparam>
[PublicEvolving]
public interface IBroadcastState<TKey, TValue> : IReadOnlyBroadcastState<TKey, TValue>
{
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
    /// Returns all the mappings in the state.
    /// </summary>
    /// <returns>An iterable view of all the key-value pairs in the state.</returns>
    /// <exception cref="Exception">Thrown if the system cannot access the state.</exception>
    IEnumerable<KeyValuePair<TKey, TValue>> Entries();
}
