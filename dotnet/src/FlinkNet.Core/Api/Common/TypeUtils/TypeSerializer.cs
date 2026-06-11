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
using FlinkNet.Core.Memory;

namespace FlinkNet.Api.Common.TypeUtils;

/// <summary>
/// This interface describes the methods that are required for a data type to be handled by the
/// Flink runtime. Specifically, this interface contains the serialization and copying methods.
///
/// <para>The methods in this class are not necessarily thread safe. To avoid unpredictable side
/// effects, it is recommended to call <see cref="Duplicate"/> and use one serializer instance
/// per thread.</para>
///
/// <para><b>Upgrading TypeSerializers to the new TypeSerializerSnapshot model</b></para>
///
/// <para>PORT NOTE: Java's <c>snapshotConfiguration()</c> and the schema-evolution snapshot
/// subsystem (<c>TypeSerializerSnapshot</c> and friends) are deferred to the state/checkpointing
/// increment.</para>
/// </summary>
/// <typeparam name="T">The data type that the serializer serializes.</typeparam>
[PublicEvolving]
public abstract class TypeSerializer<T>
{
    // --------------------------------------------------------------------------------------------
    // General information about the type and the serializer
    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Gets whether the type is an immutable type.
    /// </summary>
    public abstract bool IsImmutableType { get; }

    /// <summary>
    /// Creates a deep copy of this serializer if it is necessary, i.e. if it is stateful. This
    /// can return itself if the serializer is not stateful.
    ///
    /// <para>We need this because Serializers might be used in several threads. Stateless
    /// serializers are inherently thread-safe while stateful serializers might not be
    /// thread-safe.</para>
    /// </summary>
    public abstract TypeSerializer<T> Duplicate();

    // --------------------------------------------------------------------------------------------
    // Instantiation & Cloning
    // --------------------------------------------------------------------------------------------

    /// <summary>Creates a new instance of the data type.</summary>
    /// <returns>A new instance of the data type.</returns>
    public abstract T CreateInstance();

    /// <summary>
    /// Creates a deep copy of the given element in a new element.
    /// </summary>
    /// <param name="from">The element to copy.</param>
    /// <returns>A deep copy of the element.</returns>
    public abstract T Copy(T from);

    /// <summary>
    /// Creates a copy from the given element. The method makes an attempt to store the copy in
    /// the given reuse element, if the type is mutable. This is, however, not guaranteed.
    /// </summary>
    /// <param name="from">The element to copy.</param>
    /// <param name="reuse">The element to be reused. May or may not be used.</param>
    /// <returns>A deep copy of the element.</returns>
    public abstract T Copy(T from, T reuse);

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Gets the length of the data type, if it is a fix length data type.
    /// </summary>
    /// <value>The length of the data type, or <c>-1</c> for variable length data types.</value>
    public abstract int Length { get; }

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Serializes the given record to the given target output view.
    /// </summary>
    /// <param name="record">The record to serialize.</param>
    /// <param name="target">The output view to write the serialized data to.</param>
    /// <exception cref="IOException">Thrown, if the serialization encountered an I/O related
    /// error. Typically raised by the output view, which may have an underlying I/O channel that
    /// it writes to.</exception>
    public abstract void Serialize(T record, IDataOutputView target);

    /// <summary>
    /// De-serializes a record from the given source input view.
    /// </summary>
    /// <param name="source">The input view from which to read the data.</param>
    /// <returns>The deserialized element.</returns>
    /// <exception cref="IOException">Thrown, if the de-serialization encountered an I/O related
    /// error. Typically raised by the input view, which may have an underlying I/O channel from
    /// which it reads.</exception>
    public abstract T Deserialize(IDataInputView source);

    /// <summary>
    /// De-serializes a record from the given source input view into the given reuse record
    /// instance if mutable.
    /// </summary>
    /// <param name="reuse">The record instance into which to de-serialize the data.</param>
    /// <param name="source">The input view from which to read the data.</param>
    /// <returns>The deserialized element.</returns>
    public abstract T Deserialize(T reuse, IDataInputView source);

    /// <summary>
    /// Copies exactly one record from the source input view to the target output view. Whether
    /// this operation works on binary data or partially de-serializes the record to determine its
    /// length (such as for records of variable length) is up to the implementer. Binary copies
    /// are typically faster. A copy of a record containing two integer numbers (8 bytes total)
    /// is most efficiently implemented as <c>target.WriteLong(source.ReadLong())</c>.
    /// </summary>
    /// <param name="source">The input view from which to read the record.</param>
    /// <param name="target">The target output view to which to write the record.</param>
    public abstract void Copy(IDataInputView source, IDataOutputView target);

    public abstract override bool Equals(object? obj);

    public abstract override int GetHashCode();
}
