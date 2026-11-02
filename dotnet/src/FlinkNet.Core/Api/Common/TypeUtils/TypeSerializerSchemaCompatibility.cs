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

namespace FlinkNet.Api.Common.TypeUtils;

/// <summary>
/// Non-generic base of <see cref="TypeSerializerSchemaCompatibility{T}"/>.
///
/// <para>PORT NOTE: stands in for Java's wildcard <c>TypeSerializerSchemaCompatibility&lt;?&gt;</c>,
/// used when nested snapshots of heterogeneous types are resolved pairwise.</para>
/// </summary>
[PublicEvolving]
public abstract class TypeSerializerSchemaCompatibility
{
    /// <summary>The type of the compatibility (Java's package-private <c>Type</c> enum).</summary>
    internal enum CompatibilityType
    {
        /// <summary>This indicates that the new serializer continued to be used as is.</summary>
        CompatibleAsIs,

        /// <summary>
        /// This indicates that it is possible to use the new serializer after performing a
        /// full-scan migration over all state, by reading bytes with the previous serializer and
        /// then writing it again with the new serializer, effectively converting the
        /// serialization schema to correspond to the new serializer.
        /// </summary>
        CompatibleAfterMigration,

        /// <summary>
        /// This indicates that a reconfigured version of the new serializer is compatible, and
        /// should be used instead of the original new serializer.
        /// </summary>
        CompatibleWithReconfiguredSerializer,

        /// <summary>
        /// This indicates that the new serializer is incompatible, even with migration. This
        /// normally implies that the deserialized class can not be commonly recognized by the
        /// previous and new serializer.
        /// </summary>
        Incompatible,
    }

    private readonly CompatibilityType _resultType;

    private readonly TypeSerializer? _reconfiguredNewSerializer;

    private protected TypeSerializerSchemaCompatibility(
        CompatibilityType resultType, TypeSerializer? reconfiguredNewSerializer)
    {
        _resultType = resultType;
        _reconfiguredNewSerializer = reconfiguredNewSerializer;
    }

    internal CompatibilityType ResultType => _resultType;

    /// <summary>Untyped access to the reconfigured serializer (see the class PORT NOTE).</summary>
    internal TypeSerializer? ReconfiguredSerializerUntyped => _reconfiguredNewSerializer;

    /// <summary>Whether the type of the compatibility is "compatible as is".</summary>
    public bool IsCompatibleAsIs() => _resultType == CompatibilityType.CompatibleAsIs;

    /// <summary>Whether the type of the compatibility is "compatible after migration".</summary>
    public bool IsCompatibleAfterMigration() =>
        _resultType == CompatibilityType.CompatibleAfterMigration;

    /// <summary>Whether the type of the compatibility is "compatible with a reconfigured
    /// serializer".</summary>
    public bool IsCompatibleWithReconfiguredSerializer() =>
        _resultType == CompatibilityType.CompatibleWithReconfiguredSerializer;

    /// <summary>Whether the type of the compatibility is "incompatible".</summary>
    public bool IsIncompatible() => _resultType == CompatibilityType.Incompatible;

    public override string ToString() =>
        "TypeSerializerSchemaCompatibility{resultType=" + _resultType
            + ", reconfiguredNewSerializer=" + _reconfiguredNewSerializer + "}";
}

/// <summary>
/// A <c>TypeSerializerSchemaCompatibility</c> represents information about whether or not a
/// <see cref="TypeSerializer{T}"/> can be safely used to read data written by a previous type
/// serializer.
///
/// <para>Typically, the compatibility of the new serializer is resolved by checking the
/// serializer snapshot against the <see cref="TypeSerializerSnapshot{T}"/> of the previous
/// serializer. Depending on the type of the resolved compatibility result, migration (i.e.,
/// reading bytes with the previous serializer and then writing it again with the new serializer)
/// may be required before the new serializer can be used.</para>
/// </summary>
/// <typeparam name="T">the type of data serialized by the serializer that was being checked.</typeparam>
[PublicEvolving]
public sealed class TypeSerializerSchemaCompatibility<T> : TypeSerializerSchemaCompatibility
{
    private TypeSerializerSchemaCompatibility(
        CompatibilityType resultType, TypeSerializer<T>? reconfiguredNewSerializer)
        : base(resultType, reconfiguredNewSerializer)
    {
    }

    /// <summary>
    /// Returns a result that indicates that the new serializer is compatible and no migration is
    /// required. The new serializer can continued to be used as is.
    /// </summary>
    public static TypeSerializerSchemaCompatibility<T> CompatibleAsIs() =>
        new(CompatibilityType.CompatibleAsIs, null);

    /// <summary>
    /// Returns a result that indicates that the new serializer can be used after migrating the
    /// written bytes, i.e. reading it with the old serializer and then writing it again with the
    /// new serializer.
    /// </summary>
    public static TypeSerializerSchemaCompatibility<T> CompatibleAfterMigration() =>
        new(CompatibilityType.CompatibleAfterMigration, null);

    /// <summary>
    /// Returns a result that indicates a reconfigured version of the new serializer is
    /// compatible, and should be used instead of the original new serializer.
    /// </summary>
    /// <param name="reconfiguredSerializer">the reconfigured version of the new serializer.</param>
    public static TypeSerializerSchemaCompatibility<T> CompatibleWithReconfiguredSerializer(
        TypeSerializer<T> reconfiguredSerializer)
    {
        ArgumentNullException.ThrowIfNull(reconfiguredSerializer);
        return new TypeSerializerSchemaCompatibility<T>(
            CompatibilityType.CompatibleWithReconfiguredSerializer, reconfiguredSerializer);
    }

    /// <summary>
    /// Returns a result that indicates there is no possible way for the new serializer to be
    /// use-able. This normally indicates that there is no common class between what the previous
    /// bytes can be deserialized into and what can be written by the new serializer.
    ///
    /// <para>In this case, there is no possible way for the new serializer to continue to be
    /// used, even with migration. Recovery of the Flink job will fail.</para>
    /// </summary>
    public static TypeSerializerSchemaCompatibility<T> Incompatible() =>
        new(CompatibilityType.Incompatible, null);

    /// <summary>
    /// Gets the reconfigured serializer. This throws an exception if
    /// <see cref="TypeSerializerSchemaCompatibility.IsCompatibleWithReconfiguredSerializer"/>
    /// is <c>false</c>.
    /// </summary>
    public TypeSerializer<T> GetReconfiguredSerializer()
    {
        if (!IsCompatibleWithReconfiguredSerializer())
        {
            throw new InvalidOperationException(
                "It is only possible to get a reconfigured serializer if the compatibility type is "
                    + CompatibilityType.CompatibleWithReconfiguredSerializer
                    + ", but the type is " + ResultType);
        }
        return (TypeSerializer<T>)ReconfiguredSerializerUntyped!;
    }
}
