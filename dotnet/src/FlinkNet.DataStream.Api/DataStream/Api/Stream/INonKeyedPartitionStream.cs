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
/// This interface represents a kind of partitioned data stream. For this stream, each parallel
/// task corresponds to a partition.
/// </summary>
/// <typeparam name="T">the type of the records in the stream.</typeparam>
[Experimental]
public interface INonKeyedPartitionStream<T> : IDataStream
{
    /// <summary>Apply an operation to this <see cref="INonKeyedPartitionStream{T}"/>.</summary>
    /// <param name="processFunction">to perform operation.</param>
    /// <returns>new stream with this operation.</returns>
    IProcessConfigurableAndNonKeyedPartitionStream<TOut> Process<TOut>(
        IOneInputStreamProcessFunction<T, TOut> processFunction);

    /// <summary>Apply a two-output operation to this stream.</summary>
    /// <param name="processFunction">to perform two output operation.</param>
    /// <returns>new stream with this operation.</returns>
    IProcessConfigurableAndTwoNonKeyedPartitionStream<TOut1, TOut2> Process<TOut1, TOut2>(
        ITwoOutputStreamProcessFunction<T, TOut1, TOut2> processFunction);

    /// <summary>Apply to a two-input operation on this and another
    /// <see cref="INonKeyedPartitionStream{T}"/>.</summary>
    IProcessConfigurableAndNonKeyedPartitionStream<TOut> ConnectAndProcess<TOther, TOut>(
        INonKeyedPartitionStream<TOther> other,
        ITwoInputNonBroadcastStreamProcessFunction<T, TOther, TOut> processFunction);

    /// <summary>Apply a two-input operation to this and a <see cref="IBroadcastStream{T}"/>.</summary>
    IProcessConfigurableAndNonKeyedPartitionStream<TOut> ConnectAndProcess<TOther, TOut>(
        IBroadcastStream<TOther> other,
        ITwoInputBroadcastStreamProcessFunction<T, TOther, TOut> processFunction);

    /// <summary>Coalesce this stream to a <see cref="IGlobalStream{T}"/>.</summary>
    IGlobalStream<T> Global();

    /// <summary>Transform this stream to a <see cref="IKeyedPartitionStream{TKey,T}"/>.</summary>
    IKeyedPartitionStream<TKey, T> KeyBy<TKey>(IKeySelector<T, TKey> keySelector);

    /// <summary>Transform this stream to a new <see cref="INonKeyedPartitionStream{T}"/>, data
    /// will be shuffled between these two streams.</summary>
    INonKeyedPartitionStream<T> Shuffle();

    /// <summary>Transform this stream to a new <see cref="IBroadcastStream{T}"/>.</summary>
    IBroadcastStream<T> Broadcast();

    /// <summary>Sink data from this stream.</summary>
    IProcessConfigurable ToSink(ISink<T> sink);
}

/// <summary>This interface represents a configurable <see cref="INonKeyedPartitionStream{T}"/>.
/// (PORT NOTE: hoisted from Java'"'"'s nested interface.)</summary>
[Experimental]
public interface IProcessConfigurableAndNonKeyedPartitionStream<T>
    : INonKeyedPartitionStream<T>,
        IProcessConfigurable<IProcessConfigurableAndNonKeyedPartitionStream<T>>
{
}

/// <summary>This interface represents a combination of two
/// <see cref="INonKeyedPartitionStream{T}"/>s. It will be used as the return value of operations
/// with two outputs. (PORT NOTE: hoisted from Java'"'"'s nested interface.)</summary>
[Experimental]
public interface IProcessConfigurableAndTwoNonKeyedPartitionStream<TOut1, TOut2>
    : IProcessConfigurable<IProcessConfigurableAndTwoNonKeyedPartitionStream<TOut1, TOut2>>
{
    /// <summary>Get the first stream.</summary>
    IProcessConfigurableAndNonKeyedPartitionStream<TOut1> GetFirst();

    /// <summary>Get the second stream.</summary>
    IProcessConfigurableAndNonKeyedPartitionStream<TOut2> GetSecond();
}
