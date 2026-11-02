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
/// Asynchronous iterators allow to iterate over data that comes asynchronously, on-demand.
/// </summary>
/// <typeparam name="T">The element type of this iterator.</typeparam>
[Experimental]
public interface IStateIterator<T>
{
    /// <summary>
    /// Async iterate the data and call the callback when data is ready.
    /// </summary>
    /// <typeparam name="TResult">The type of the inner returned StateFuture's result.</typeparam>
    /// <param name="iterating">The data action when it is ready. The return is the state future
    /// for chaining.</param>
    /// <returns>The Future that will trigger when this iterator and all returned state future get
    /// its results.</returns>
    IStateFuture<ICollection<TResult>> OnNext<TResult>(Func<T, IStateFuture<TResult>> iterating);

    /// <summary>
    /// Async iterate the data and call the callback when data is ready.
    /// </summary>
    /// <param name="iterating">The data action when it is ready.</param>
    /// <returns>The Future that will trigger when this iterator ends.</returns>
    IStateFuture<object?> OnNext(Action<T> iterating);

    /// <summary>Return if this iterator is empty synchronously.</summary>
    bool IsEmpty();
}
