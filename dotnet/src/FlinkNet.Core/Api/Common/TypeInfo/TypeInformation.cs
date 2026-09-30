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
using FlinkNet.Api.Common.Serialization;
using FlinkNet.Api.Common.TypeUtils;

namespace FlinkNet.Api.Common.TypeInfo;

/// <summary>
/// TypeInformation is the core class of Flink's type system. Flink requires a type information
/// for all types that are used as input or return type of a user function. This type information
/// class acts as the tool to generate serializers and comparators, and to perform semantic checks
/// such as whether the fields that are used as join/grouping keys actually exist.
///
/// <para>The type information also bridges between the programming languages object model and a
/// logical flat schema. It maps fields from the types to columns (fields) in a flat schema. Not
/// all fields from a type are mapped to a separate field in the flat schema and often, entire
/// types are mapped to one field.</para>
/// </summary>
/// <typeparam name="T">The type represented by this type information.</typeparam>
[Public]
public abstract class TypeInformation<T>
{
    /// <summary>
    /// Checks if this type information represents a basic type. Basic types are defined in
    /// <see cref="BasicTypeInfo"/> and are primitives, Strings, ...
    /// </summary>
    /// <value>True, if this type information describes a basic type, false otherwise.</value>
    public abstract bool IsBasicType { get; }

    /// <summary>Checks if this type information represents a Tuple type.</summary>
    /// <value>True, if this type information describes a tuple type, false otherwise.</value>
    public abstract bool IsTupleType { get; }

    /// <summary>Gets the arity of this type - the number of fields without nesting.</summary>
    public abstract int Arity { get; }

    /// <summary>
    /// Gets the number of logical fields in this type. This includes its nested and transitively
    /// nested fields, in the case of composite types.
    /// </summary>
    /// <value>The number of logical fields in this type</value>
    public abstract int TotalFields { get; }

    /// <summary>Gets the type represented by this type information.</summary>
    public abstract Type TypeClass { get; }

    /// <summary>
    /// Checks whether this type can be used as a key. As a bare minimum, types have to be
    /// hashable and comparable to be keys.
    /// </summary>
    public abstract bool IsKeyType { get; }

    /// <summary>
    /// Creates a serializer for the type.
    /// </summary>
    /// <param name="config">The config used to parameterize the serializer; may be null while
    /// the execution-config increment is pending (PORT NOTE).</param>
    /// <returns>A serializer for this type.</returns>
    public abstract TypeSerializer<T> CreateSerializer(ISerializerConfig? config);

    public abstract override string ToString();

    public abstract override bool Equals(object? obj);

    public abstract override int GetHashCode();

    /// <summary>
    /// Returns true if the given object can be equaled with this object. If not, it returns
    /// false.
    /// </summary>
    /// <param name="obj">Object which wants to take part in the equality relation</param>
    /// <returns>true if obj can be equaled with this, otherwise false</returns>
    public abstract bool CanEqual(object obj);
}
