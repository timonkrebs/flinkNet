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
using FlinkNet.Api.Connector.DsV2;
using FlinkNet.Api.Functions;
using FlinkNet.DataStream.Api.Function;

namespace FlinkNet.DataStream.Api.Stream;

/// <summary>
/// This interface represents a kind of partitioned data stream. For this stream, each key group
/// is a partition, and the partition to which each record belongs is determined by its key.
/// </summary>
/// <typeparam name="TKey">the type of the key.</typeparam>
/// <typeparam name="T">the type of the records in the stream.</typeparam>
[Experimental]
public interface IKeyedPartitionStream<TKey, T> : IDataStream
{
    /// <summary>
    /// Apply an operation to this <see cref="IKeyedPartitionStream{TKey,T}"/>.
    ///
    /// <para>This method is used to avoid shuffle after applying the process function. It is
    /// required that for the same record, the new <see cref="IKeySelector{TIn,TKey}"/> must
    /// extract the same key as the original one on this stream.</para>
    /// </summary>
    /// <param name="processFunction">to perform operation.</param>
    /// <param name="newKeySelector">to select the key after process.</param>
    /// <returns>new <see cref="IKeyedPartitionStream{TKey,T}"/> with this operation.</returns>
    IProcessConfigurableAndKeyedPartitionStream<TKey, TOut> Process<TOut>(
        IOneInputStreamProcessFunction<T, TOut> processFunction,
        IKeySelector<TOut, TKey> newKeySelector);

    /// <summary>Apply an operation to this stream, resulting in a non-keyed stream.</summary>
    IProcessConfigurableAndNonKeyedPartitionStream<TOut> Process<TOut>(
        IOneInputStreamProcessFunction<T, TOut> processFunction);

    /// <summary>Apply a two-output operation, remaining keyed on both outputs.</summary>
    IProcessConfigurableAndTwoKeyedPartitionStreams<TKey, TOut1, TOut2> Process<TOut1, TOut2>(
        ITwoOutputStreamProcessFunction<T, TOut1, TOut2> processFunction,
        IKeySelector<TOut1, TKey> keySelector1,
        IKeySelector<TOut2, TKey> keySelector2);

    /// <summary>Apply a two-output operation, resulting in non-keyed streams.</summary>
    IProcessConfigurableAndTwoNonKeyedPartitionStream<TOut1, TOut2> Process<TOut1, TOut2>(
        ITwoOutputStreamProcessFunction<T, TOut1, TOut2> processFunction);

    /// <summary>Apply a two-input operation to this and another
    /// <see cref="IKeyedPartitionStream{TKey,T}"/>, resulting in a non-keyed stream.</summary>
    IProcessConfigurableAndNonKeyedPartitionStream<TOut> ConnectAndProcess<TOther, TOut>(
        IKeyedPartitionStream<TKey, TOther> other,
        ITwoInputNonBroadcastStreamProcessFunction<T, TOther, TOut> processFunction);

    /// <summary>Apply a two-input operation to this and another keyed stream, remaining keyed.</summary>
    IProcessConfigurableAndKeyedPartitionStream<TKey, TOut> ConnectAndProcess<TOther, TOut>(
        IKeyedPartitionStream<TKey, TOther> other,
        ITwoInputNonBroadcastStreamProcessFunction<T, TOther, TOut> processFunction,
        IKeySelector<TOut, TKey> newKeySelector);

    /// <summary>Apply a two-input operation to this and a broadcast stream, resulting in a
    /// non-keyed stream.</summary>
    IProcessConfigurableAndNonKeyedPartitionStream<TOut> ConnectAndProcess<TOther, TOut>(
        IBroadcastStream<TOther> other,
        ITwoInputBroadcastStreamProcessFunction<T, TOther, TOut> processFunction);

    /// <summary>Apply a two-input operation to this and a broadcast stream, remaining keyed.</summary>
    IProcessConfigurableAndKeyedPartitionStream<TKey, TOut> ConnectAndProcess<TOther, TOut>(
        IBroadcastStream<TOther> other,
        ITwoInputBroadcastStreamProcessFunction<T, TOther, TOut> processFunction,
        IKeySelector<TOut, TKey> newKeySelector);

    /// <summary>Coalesce this stream to a <see cref="IGlobalStream{T}"/>.</summary>
    IGlobalStream<T> Global();

    /// <summary>Transform this stream to a new keyed stream with a different key.</summary>
    IKeyedPartitionStream<TNewKey, T> KeyBy<TNewKey>(IKeySelector<T, TNewKey> keySelector);

    /// <summary>Transform this stream to a new <see cref="INonKeyedPartitionStream{T}"/>, data
    /// will be shuffled between these two streams.</summary>
    INonKeyedPartitionStream<T> Shuffle();

    /// <summary>Transform this stream to a new <see cref="IBroadcastStream{T}"/>.</summary>
    IBroadcastStream<T> Broadcast();

    /// <summary>Sink data from this stream.</summary>
    IProcessConfigurable ToSink(ISink<T> sink);
}

/// <summary>This interface represents a configurable
/// <see cref="IKeyedPartitionStream{TKey,T}"/>. (PORT NOTE: hoisted from Java's nested
/// interface.)</summary>
[Experimental]
public interface IProcessConfigurableAndKeyedPartitionStream<TKey, T>
    : IKeyedPartitionStream<TKey, T>,
        IProcessConfigurable<IProcessConfigurableAndKeyedPartitionStream<TKey, T>>
{
}

/// <summary>This interface represents a combination of two
/// <see cref="IKeyedPartitionStream{TKey,T}"/>s. It will be used as the return value of
/// operations with two outputs. (PORT NOTE: hoisted from Java's nested interface.)</summary>
[Experimental]
public interface IProcessConfigurableAndTwoKeyedPartitionStreams<TKey, T1, T2>
    : IProcessConfigurable<IProcessConfigurableAndTwoKeyedPartitionStreams<TKey, T1, T2>>
{
    /// <summary>Get the first stream.</summary>
    IProcessConfigurableAndKeyedPartitionStream<TKey, T1> GetFirst();

    /// <summary>Get the second stream.</summary>
    IProcessConfigurableAndKeyedPartitionStream<TKey, T2> GetSecond();
}
