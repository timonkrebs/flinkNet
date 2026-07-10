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

namespace FlinkNet.Annotations.Docs;

/// <summary>Collection of attributes to modify the behavior of the JSON schema documentation generators.</summary>
[Internal]
public static class FlinkJsonSchema
{
    /// <summary>
    /// Attribute used on classes to denote that this type supports setting dynamic fields of a
    /// given type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    [Internal]
    public sealed class AdditionalFieldsAttribute : Attribute
    {
        /// <summary>The type of the supported additional fields.</summary>
        public required Type Type { get; init; }
    }
}
