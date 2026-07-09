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
using FlinkNet.Api.Common.State;

namespace FlinkNet.DataStream.Api.Context;

/// <summary>
/// This is responsible for managing runtime information related to the state of the process
/// function.
///
/// <para>PORT NOTE: Java'"'"'s <c>Optional&lt;State&gt; getStateOptional(...)</c> maps to nullable
/// returns, per the porting conventions.</para>
/// </summary>
[Experimental]
public interface IStateManager
{
    /// <summary>Get the key of the current record.</summary>
    /// <returns>The key of the current processed record.</returns>
    /// <exception cref="NotSupportedException">if the key can not be extracted for this function,
    /// for instance, get the key from a non-keyed partition stream.</exception>
    TKey GetCurrentKey<TKey>();

    /// <summary>Get the optional of the specific list state.</summary>
    /// <param name="stateDeclaration">of this state.</param>
    /// <returns>the state matched the declaration, null if the state is not available.</returns>
    IListState<T>? GetStateOptional<T>(IListStateDeclaration<T> stateDeclaration);

    /// <summary>Get the specific list state.</summary>
    /// <param name="stateDeclaration">of this state.</param>
    /// <returns>the state matched the declaration.</returns>
    /// <exception cref="InvalidOperationException">if the state is not available.</exception>
    IListState<T> GetState<T>(IListStateDeclaration<T> stateDeclaration);

    /// <summary>Get the optional of the specific value state.</summary>
    IValueState<T>? GetStateOptional<T>(IValueStateDeclaration<T> stateDeclaration);

    /// <summary>Get the specific value state.</summary>
    IValueState<T> GetState<T>(IValueStateDeclaration<T> stateDeclaration);

    /// <summary>Get the optional of the specific map state.</summary>
    IMapState<TKey, TValue>? GetStateOptional<TKey, TValue>(
        IMapStateDeclaration<TKey, TValue> stateDeclaration);

    /// <summary>Get the specific map state.</summary>
    IMapState<TKey, TValue> GetState<TKey, TValue>(
        IMapStateDeclaration<TKey, TValue> stateDeclaration);

    /// <summary>Get the optional of the specific reducing state.</summary>
    IReducingState<T>? GetStateOptional<T>(IReducingStateDeclaration<T> stateDeclaration);

    /// <summary>Get the specific reducing state.</summary>
    IReducingState<T> GetState<T>(IReducingStateDeclaration<T> stateDeclaration);

    /// <summary>Get the optional of the specific aggregating state.</summary>
    IAggregatingState<TIn, TOut>? GetStateOptional<TIn, TAcc, TOut>(
        IAggregatingStateDeclaration<TIn, TAcc, TOut> stateDeclaration);

    /// <summary>Get the specific aggregating state.</summary>
    IAggregatingState<TIn, TOut> GetState<TIn, TAcc, TOut>(
        IAggregatingStateDeclaration<TIn, TAcc, TOut> stateDeclaration);

    /// <summary>Get the optional of the specific broadcast state.</summary>
    IBroadcastState<TKey, TValue>? GetStateOptional<TKey, TValue>(
        IBroadcastStateDeclaration<TKey, TValue> stateDeclaration);

    /// <summary>Get the specific broadcast state.</summary>
    IBroadcastState<TKey, TValue> GetState<TKey, TValue>(
        IBroadcastStateDeclaration<TKey, TValue> stateDeclaration);
}
