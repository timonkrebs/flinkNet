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
using FlinkNet.Api.Common.TypeInfo;

namespace FlinkNet.Api.Common.State;

/// <summary>This represents a declaration of the list state.</summary>
[Experimental]
public interface IListStateDeclaration<T> : IStateDeclaration
{
    /// <summary>
    /// Get the <see cref="State.RedistributionStrategy"/> of this list state.
    /// </summary>
    /// <value>The redistribution strategy of this list state.</value>
    RedistributionStrategy RedistributionStrategy { get; }

    /// <summary>Get type descriptor of this list state's element.</summary>
    ITypeDescriptor<T> TypeDescriptor { get; }
}

/// <summary>
/// <see cref="RedistributionStrategy"/> is used to guide the assignment of states during
/// rescaling.
/// </summary>
[Experimental]
public enum RedistributionStrategy
{
    /// <summary>
    /// The whole state is logically a concatenation of all lists. On restore/redistribution, the
    /// list is evenly divided into as many sub-lists as there are parallel operators. Each
    /// operator gets a sub-list, which can be empty, or contain one or more elements.
    /// </summary>
    Split,

    /// <summary>
    /// The whole state is logically a concatenation of all lists. On restore/redistribution, each
    /// operator gets the complete list of state elements.
    /// </summary>
    Union
}
