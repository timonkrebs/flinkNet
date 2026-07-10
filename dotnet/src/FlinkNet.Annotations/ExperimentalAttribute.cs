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

namespace FlinkNet.Annotations;

/// <summary>
/// Attribute to mark types for experimental use.
///
/// <para>Types with this attribute are neither battle-tested nor stable, and may be changed or
/// removed in future versions.</para>
///
/// <para>This attribute also excludes types with evolving interfaces / signatures annotated with
/// <see cref="PublicAttribute"/>.</para>
/// </summary>
[AttributeUsage(
    AttributeTargets.Class
        | AttributeTargets.Interface
        | AttributeTargets.Enum
        | AttributeTargets.Struct
        | AttributeTargets.Delegate
        | AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Field
        | AttributeTargets.Constructor)]
[Public]
public sealed class ExperimentalAttribute : Attribute;
