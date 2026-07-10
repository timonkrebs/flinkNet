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

namespace FlinkNet.DataStream.Api.Extension.Window.Context;

/// <summary>
/// This interface enables retrieving and updating information in a window, such as the window
/// start time, end time, and window state.
///
/// <para>PORT NOTE: Java's <c>Optional&lt;State&gt; getWindowState(...)</c> maps to nullable
/// returns, per the porting conventions.</para>
/// </summary>
[Experimental]
public interface IWindowContext
{
    /// <summary>Returns the start timestamp of the current window, or -1 if the window is not a
    /// time window.</summary>
    long GetStartTime();

    /// <summary>Returns the end timestamp of the current window, or -1 if the window is not a
    /// time window.</summary>
    long GetEndTime();

    /// <summary>Get the list state that is scoped to the current window, null if the state is
    /// not available.</summary>
    IListState<T>? GetWindowState<T>(IListStateDeclaration<T> stateDeclaration);

    /// <summary>Get the map state that is scoped to the current window, null if the state is not
    /// available.</summary>
    IMapState<TKey, TValue>? GetWindowState<TKey, TValue>(
        IMapStateDeclaration<TKey, TValue> stateDeclaration);

    /// <summary>Get the value state that is scoped to the current window, null if the state is
    /// not available.</summary>
    IValueState<T>? GetWindowState<T>(IValueStateDeclaration<T> stateDeclaration);

    /// <summary>Get the reducing state that is scoped to the current window, null if the state
    /// is not available.</summary>
    IReducingState<T>? GetWindowState<T>(IReducingStateDeclaration<T> stateDeclaration);

    /// <summary>Get the aggregating state that is scoped to the current window, null if the
    /// state is not available.</summary>
    IAggregatingState<T, TOut>? GetWindowState<T, TAcc, TOut>(
        IAggregatingStateDeclaration<T, TAcc, TOut> stateDeclaration);
}
