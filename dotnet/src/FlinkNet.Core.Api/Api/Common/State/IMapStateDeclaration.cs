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

/// <summary>This represents a declaration of the map state.</summary>
[Experimental]
public interface IMapStateDeclaration<TKey, TValue> : IStateDeclaration
{
    /// <summary>Get type descriptor of this map state's key.</summary>
    ITypeDescriptor<TKey> KeyTypeDescriptor { get; }

    /// <summary>Get type descriptor of this map state's value.</summary>
    ITypeDescriptor<TValue> ValueTypeDescriptor { get; }
}
