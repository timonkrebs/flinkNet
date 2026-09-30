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
/// Extension of <see cref="IAppendingState{TIn, TOut, TSyncOut}"/> that allows merging of state.
/// That is, two instances of <see cref="IMergingState{TIn, TOut, TSyncOut}"/> can be combined into
/// a single instance that contains all the information of the two merged states.
/// </summary>
/// <typeparam name="TIn">Type of the value that can be added to the state.</typeparam>
/// <typeparam name="TOut">Type of the value that can be retrieved from the state.</typeparam>
/// <typeparam name="TSyncOut">Type of the value that can be retrieved from the state by
/// synchronous interface.</typeparam>
[Experimental]
public interface IMergingState<TIn, TOut, TSyncOut> : IAppendingState<TIn, TOut, TSyncOut>
{
}
