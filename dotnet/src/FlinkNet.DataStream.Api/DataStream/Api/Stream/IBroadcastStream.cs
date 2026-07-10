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
using FlinkNet.Api.Functions;
using FlinkNet.DataStream.Api.Function;

namespace FlinkNet.DataStream.Api.Stream;

/// <summary>This interface represents a stream where each parallel task processes the same
/// data.</summary>
/// <typeparam name="T">the type of the records in the stream.</typeparam>
[Experimental]
public interface IBroadcastStream<T> : IDataStream
{
    /// <summary>Apply a two-input operation to this and another
    /// <see cref="IKeyedPartitionStream{TKey,T}"/>, resulting in a non-keyed stream.</summary>
    IProcessConfigurableAndNonKeyedPartitionStream<TOut> ConnectAndProcess<TKey, TOther, TOut>(
        IKeyedPartitionStream<TKey, TOther> other,
        ITwoInputBroadcastStreamProcessFunction<TOther, T, TOut> processFunction);

    /// <summary>Apply a two-input operation to this and another
    /// <see cref="INonKeyedPartitionStream{T}"/>.</summary>
    IProcessConfigurableAndNonKeyedPartitionStream<TOut> ConnectAndProcess<TOther, TOut>(
        INonKeyedPartitionStream<TOther> other,
        ITwoInputBroadcastStreamProcessFunction<TOther, T, TOut> processFunction);

    /// <summary>Apply a two-input operation to this and a keyed stream, remaining keyed.
    ///
    /// <para>It is required that for the same record, the new
    /// <see cref="IKeySelector{TIn,TKey}"/> must extract the same key as the original one on the
    /// other stream.</para></summary>
    IProcessConfigurableAndKeyedPartitionStream<TKey, TOut> ConnectAndProcess<TKey, TOther, TOut>(
        IKeyedPartitionStream<TKey, TOther> other,
        ITwoInputBroadcastStreamProcessFunction<TOther, T, TOut> processFunction,
        IKeySelector<TOut, TKey> newKeySelector);
}
