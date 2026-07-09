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

/// <summary>This interface represents a stream that forces single parallelism.</summary>
/// <typeparam name="T">the type of the records in the stream.</typeparam>
[Experimental]
public interface IGlobalStream<T> : IDataStream
{
    /// <summary>Apply an operation to this <see cref="IGlobalStream{T}"/>.</summary>
    IProcessConfigurableAndGlobalStream<TOut> Process<TOut>(
        IOneInputStreamProcessFunction<T, TOut> processFunction);

    /// <summary>Apply a two-output operation to this <see cref="IGlobalStream{T}"/>.</summary>
    ITwoGlobalStreams<TOut1, TOut2> Process<TOut1, TOut2>(
        ITwoOutputStreamProcessFunction<T, TOut1, TOut2> processFunction);

    /// <summary>Apply a two-input operation to this and another
    /// <see cref="IGlobalStream{T}"/>.</summary>
    IProcessConfigurableAndGlobalStream<TOut> ConnectAndProcess<TOther, TOut>(
        IGlobalStream<TOther> other,
        ITwoInputNonBroadcastStreamProcessFunction<T, TOther, TOut> processFunction);

    /// <summary>Transform this stream to a <see cref="IKeyedPartitionStream{TKey,T}"/>.</summary>
    IKeyedPartitionStream<TKey, T> KeyBy<TKey>(IKeySelector<T, TKey> keySelector);

    /// <summary>Transform this stream to a new <see cref="INonKeyedPartitionStream{T}"/>, data
    /// will be shuffled from the single partition to multiple partitions.</summary>
    INonKeyedPartitionStream<T> Shuffle();

    /// <summary>Transform this stream to a new <see cref="IBroadcastStream{T}"/>.</summary>
    IBroadcastStream<T> Broadcast();

    /// <summary>Sink data from this stream.</summary>
    IProcessConfigurable ToSink(ISink<T> sink);
}

/// <summary>This interface represents a configurable <see cref="IGlobalStream{T}"/>. (PORT NOTE:
/// hoisted from Java'"'"'s nested interface.)</summary>
[Experimental]
public interface IProcessConfigurableAndGlobalStream<T>
    : IGlobalStream<T>, IProcessConfigurable<IProcessConfigurableAndGlobalStream<T>>
{
}

/// <summary>This interface represents a combination of two <see cref="IGlobalStream{T}"/>s. It
/// will be used as the return value of operations with two outputs. (PORT NOTE: hoisted from
/// Java'"'"'s nested interface.)</summary>
[Experimental]
public interface ITwoGlobalStreams<T1, T2>
{
    /// <summary>Get the first stream.</summary>
    IProcessConfigurableAndGlobalStream<T1> GetFirst();

    /// <summary>Get the second stream.</summary>
    IProcessConfigurableAndGlobalStream<T2> GetSecond();
}
