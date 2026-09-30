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

/// <summary>Utilities for the <see cref="CompositeTypeSerializerSnapshot{T, TSerializer}"/>.</summary>
[Internal]
public static class CompositeTypeSerializerUtil
{
    /// <summary>
    /// Delegates compatibility checks to a
    /// <see cref="CompositeTypeSerializerSnapshot{T, TSerializer}"/> instance. This can be used
    /// by legacy snapshot classes, which have a newer implementation implemented as a composite
    /// snapshot.
    /// </summary>
    /// <param name="legacySerializerSnapshot">the legacy serializer snapshot to check for
    /// compatibility.</param>
    /// <param name="newCompositeSnapshot">an instance of the new snapshot class to delegate
    /// compatibility checks to. This instance should already contain the outer snapshot
    /// information.</param>
    /// <param name="legacyNestedSnapshots">the nested serializer snapshots of the legacy
    /// composite snapshot.</param>
    public static TypeSerializerSchemaCompatibility<T> DelegateCompatibilityCheckToNewSnapshot<T, TSerializer>(
        TypeSerializerSnapshot<T> legacySerializerSnapshot,
        CompositeTypeSerializerSnapshot<T, TSerializer> newCompositeSnapshot,
        params TypeSerializerSnapshot[] legacyNestedSnapshots)
        where TSerializer : TypeSerializer<T>
    {
        if (legacyNestedSnapshots.Length == 0)
        {
            throw new ArgumentException(null, nameof(legacyNestedSnapshots));
        }
        return newCompositeSnapshot.InternalResolveSchemaCompatibility(
            legacySerializerSnapshot, legacyNestedSnapshots);
    }

    /// <summary>
    /// Overrides the existing nested serializer's snapshots with the provided
    /// <paramref name="nestedSnapshots"/>.
    /// </summary>
    /// <param name="compositeSnapshot">the composite snapshot to overwrite its nested
    /// serializers.</param>
    /// <param name="nestedSnapshots">the nested snapshots to overwrite with.</param>
    public static void SetNestedSerializersSnapshots<T, TSerializer>(
        CompositeTypeSerializerSnapshot<T, TSerializer> compositeSnapshot,
        params TypeSerializerSnapshot[] nestedSnapshots)
        where TSerializer : TypeSerializer<T>
    {
        var del = new NestedSerializersSnapshotDelegate(nestedSnapshots);
        compositeSnapshot.SetNestedSerializersSnapshotDelegate(del);
    }

    /// <summary>
    /// Constructs an <see cref="IntermediateCompatibilityResult{T}"/> with the given array of
    /// nested serializers and their corresponding serializer snapshots.
    ///
    /// <para>This result is considered "intermediate", because the actual final result is not
    /// yet built if it isn't defined. This is the case if the final result is supposed to be
    /// compatible-with-a-reconfigured-serializer, where construction of the reconfigured
    /// serializer instance should be done by the caller.</para>
    /// </summary>
    /// <param name="newNestedSerializerSnapshots">the new nested serializer snapshots to check
    /// for compatibility.</param>
    /// <param name="oldNestedSerializerSnapshots">the associated previous nested serializers'
    /// snapshots.</param>
    public static IntermediateCompatibilityResult<T> ConstructIntermediateCompatibilityResult<T>(
        TypeSerializerSnapshot[] newNestedSerializerSnapshots,
        TypeSerializerSnapshot[] oldNestedSerializerSnapshots)
    {
        if (newNestedSerializerSnapshots.Length != oldNestedSerializerSnapshots.Length)
        {
            throw new ArgumentException(
                "Different number of new serializer snapshots and existing serializer snapshots.");
        }

        var nestedSerializers = new TypeSerializer[newNestedSerializerSnapshots.Length];

        // check nested serializers for compatibility
        bool nestedSerializerRequiresMigration = false;
        bool hasReconfiguredNestedSerializers = false;
        for (int i = 0; i < oldNestedSerializerSnapshots.Length; i++)
        {
            TypeSerializerSchemaCompatibility compatibility =
                newNestedSerializerSnapshots[i].ResolveSchemaCompatibilityUntyped(
                    oldNestedSerializerSnapshots[i]);

            // if any one of the new nested serializers is incompatible, we can just short
            // circuit the result
            if (compatibility.IsIncompatible())
            {
                return IntermediateCompatibilityResult<T>.DefinedIncompatibleResult();
            }

            if (compatibility.IsCompatibleAfterMigration())
            {
                nestedSerializerRequiresMigration = true;
            }
            else if (compatibility.IsCompatibleWithReconfiguredSerializer())
            {
                hasReconfiguredNestedSerializers = true;
                nestedSerializers[i] = compatibility.ReconfiguredSerializerUntyped!;
            }
            else if (compatibility.IsCompatibleAsIs())
            {
                nestedSerializers[i] = newNestedSerializerSnapshots[i].RestoreSerializerUntyped();
            }
            else
            {
                throw new InvalidOperationException("Undefined compatibility type.");
            }
        }

        if (nestedSerializerRequiresMigration)
        {
            return IntermediateCompatibilityResult<T>.DefinedCompatibleAfterMigrationResult();
        }

        if (hasReconfiguredNestedSerializers)
        {
            return IntermediateCompatibilityResult<T>.UndefinedReconfigureResult(nestedSerializers);
        }

        // ends up here if everything is compatible as is
        return IntermediateCompatibilityResult<T>.DefinedCompatibleAsIsResult(nestedSerializers);
    }

    /// <summary>The intermediate nested-serializer compatibility result; see
    /// <see cref="ConstructIntermediateCompatibilityResult{T}"/>.</summary>
    public sealed class IntermediateCompatibilityResult<T>
    {
        private readonly TypeSerializerSchemaCompatibility.CompatibilityType _compatibilityType;
        private readonly TypeSerializer[]? _nestedSerializers;

        internal static IntermediateCompatibilityResult<T> DefinedCompatibleAsIsResult(
            TypeSerializer[] originalSerializers) =>
            new(
                TypeSerializerSchemaCompatibility.CompatibilityType.CompatibleAsIs,
                originalSerializers);

        internal static IntermediateCompatibilityResult<T> DefinedIncompatibleResult() =>
            new(TypeSerializerSchemaCompatibility.CompatibilityType.Incompatible, null);

        internal static IntermediateCompatibilityResult<T> DefinedCompatibleAfterMigrationResult() =>
            new(TypeSerializerSchemaCompatibility.CompatibilityType.CompatibleAfterMigration, null);

        internal static IntermediateCompatibilityResult<T> UndefinedReconfigureResult(
            TypeSerializer[] reconfiguredNestedSerializers) =>
            new(
                TypeSerializerSchemaCompatibility.CompatibilityType.CompatibleWithReconfiguredSerializer,
                reconfiguredNestedSerializers);

        private IntermediateCompatibilityResult(
            TypeSerializerSchemaCompatibility.CompatibilityType compatibilityType,
            TypeSerializer[]? nestedSerializers)
        {
            _compatibilityType = compatibilityType;
            _nestedSerializers = nestedSerializers;
        }

        public bool IsCompatibleWithReconfiguredSerializer() =>
            _compatibilityType
                == TypeSerializerSchemaCompatibility.CompatibilityType.CompatibleWithReconfiguredSerializer;

        public bool IsCompatibleAsIs() =>
            _compatibilityType == TypeSerializerSchemaCompatibility.CompatibilityType.CompatibleAsIs;

        public bool IsCompatibleAfterMigration() =>
            _compatibilityType
                == TypeSerializerSchemaCompatibility.CompatibilityType.CompatibleAfterMigration;

        public bool IsIncompatible() =>
            _compatibilityType == TypeSerializerSchemaCompatibility.CompatibilityType.Incompatible;

        public TypeSerializerSchemaCompatibility<T> GetFinalResult()
        {
            return _compatibilityType switch
            {
                TypeSerializerSchemaCompatibility.CompatibilityType.CompatibleAsIs =>
                    TypeSerializerSchemaCompatibility<T>.CompatibleAsIs(),
                TypeSerializerSchemaCompatibility.CompatibilityType.CompatibleAfterMigration =>
                    TypeSerializerSchemaCompatibility<T>.CompatibleAfterMigration(),
                TypeSerializerSchemaCompatibility.CompatibilityType.Incompatible =>
                    TypeSerializerSchemaCompatibility<T>.Incompatible(),
                _ => throw new InvalidOperationException(
                    "unable to build final result if intermediate compatibility type is "
                        + "COMPATIBLE_WITH_RECONFIGURED_SERIALIZER."),
            };
        }

        public TypeSerializer[] GetNestedSerializers()
        {
            if (_compatibilityType
                    != TypeSerializerSchemaCompatibility.CompatibilityType.CompatibleAsIs
                && _compatibilityType
                    != TypeSerializerSchemaCompatibility.CompatibilityType.CompatibleWithReconfiguredSerializer)
            {
                throw new InvalidOperationException(
                    "only intermediate compatibility types COMPATIBLE_AS_IS and "
                        + "COMPATIBLE_WITH_RECONFIGURED_SERIALIZER have nested serializers.");
            }
            return _nestedSerializers!;
        }
    }
}
