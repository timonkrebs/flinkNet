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
/// Attribute for marking types as public, stable interfaces.
///
/// <para>Types annotated with this attribute are stable across minor releases (2.0, 2.1, 2.2).
/// In other words, applications using <c>[Public]</c> annotated types will compile against newer
/// versions of the same major release.</para>
///
/// <para>Only major releases (1.0, 2.0) can break interfaces with this attribute.</para>
/// </summary>
[AttributeUsage(
    AttributeTargets.Class
        | AttributeTargets.Interface
        | AttributeTargets.Enum
        | AttributeTargets.Struct
        | AttributeTargets.Delegate)]
[Public]
public sealed class PublicAttribute : Attribute;
