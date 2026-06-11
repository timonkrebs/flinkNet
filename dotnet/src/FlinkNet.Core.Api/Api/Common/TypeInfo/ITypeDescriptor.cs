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

namespace FlinkNet.Api.Common.TypeInfo;

/// <summary>
/// Descriptor interface to create TypeInformation instances.
/// </summary>
/// <typeparam name="T">The type represented by this type descriptor.</typeparam>
[Experimental]
public interface ITypeDescriptor<T>
{
    /// <summary>
    /// Gets the class of the type represented by this type descriptor.
    /// </summary>
    /// <value>The class of the type represented by this type descriptor.</value>
    Type TypeClass { get; }
}
