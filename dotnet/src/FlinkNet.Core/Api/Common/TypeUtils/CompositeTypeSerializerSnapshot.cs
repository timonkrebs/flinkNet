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

using System.Globalization;
using FlinkNet.Annotations;
using FlinkNet.Core.Memory;

namespace FlinkNet.Api.Common.TypeUtils;

/// <summary>
/// Indicates schema compatibility of the serializer configuration persisted as the outer
/// snapshot. (PORT NOTE: hoisted from Java's protected nested enum in
/// <see cref="CompositeTypeSerializerSnapshot{T, TSerializer}"/> — nested types in C# generics
/// are per-instantiation.)
/// </summary>
public enum OuterSchemaCompatibility
{
    CompatibleAsIs,
    CompatibleAfterMigration,
    Incompatible,
}

/// <summary>
/// A <c>CompositeTypeSerializerSnapshot</c> is a convenient serializer snapshot class that can be
/// used by simple serializers which 1) delegate their serialization to multiple nested
/// serializers, and 2) may contain some extra static information that needs to be persisted as
/// part of their snapshot.
///
/// <para>Examples for this would be the <c>ListSerializer</c>, <c>MapSerializer</c>, etc., in
/// which case the serializer, called the "outer" serializer in this context, has only some nested
/// serializers that need to be persisted as its snapshot, and nothing else that needs to be
/// persisted as the "outer" snapshot. Serializers that do have some outer snapshot need to make
/// sure to implement the methods <see cref="WriteOuterSnapshot"/>, <see cref="ReadOuterSnapshot"/>,
/// and <see cref="ResolveOuterSchemaCompatibility"/> when using this class as the base for their
/// serializer snapshot class.</para>
///
/// <para><b>Snapshot Versioning</b>: this base class has its own versioning for the format in
/// which it writes the outer snapshot and the nested serializer snapshots, defined by
/// <see cref="CurrentVersion"/>. This is independent of the version in which subclasses write
/// their outer snapshot, defined by <see cref="CurrentOuterSnapshotVersion"/>.</para>
///
/// <para>PORT NOTE: Java's deprecated legacy hooks (<c>isOuterSnapshotCompatible</c> and the
/// serializer-typed <c>resolveOuterSchemaCompatibility</c> overload) are not ported; the
/// snapshot-based <see cref="ResolveOuterSchemaCompatibility"/> defaults to compatible-as-is,
/// which matches the observable behavior of Java's default hook chain. Java's raw
/// <c>instanceof CompositeTypeSerializerSnapshot</c> check maps to the closed generic here, so
/// two different composite snapshot classes over the same data type resolve as incompatible
/// directly instead of via the nested comparison.</para>
/// </summary>
/// <typeparam name="T">The data type that the originating serializer of this snapshot
/// serializes.</typeparam>
/// <typeparam name="TSerializer">The type of the originating serializer.</typeparam>
[PublicEvolving]
public abstract class CompositeTypeSerializerSnapshot<T, TSerializer> : TypeSerializerSnapshot<T>
    where TSerializer : TypeSerializer<T>
{
    /// <summary>Magic number for integrity checks during deserialization.</summary>
    private const int MagicNumber = 911108;

    /// <summary>
    /// Current version of the base serialization format.
    ///
    /// <para>NOTE: it starts from version 3: previously, <see cref="CurrentVersion"/> was used to
    /// represent the outer snapshot's version (now represented by
    /// <see cref="CurrentOuterSnapshotVersion"/>). The starting version of the base format is set
    /// to be larger than the highest version previously used by subclasses, so that legacy
    /// deserialization paths (which did not contain versioning for the base format) can be
    /// identified simply by the read version being smaller than or equal to
    /// <see cref="HighestLegacyReadVersion"/>.</para>
    /// </summary>
    private const int Version = 3;

    private const int HighestLegacyReadVersion = 2;

    private NestedSerializersSnapshotDelegate? _nestedSerializersSnapshotDelegate;

    /// <summary>Constructor to be used for read instantiation.</summary>
    protected CompositeTypeSerializerSnapshot()
    {
    }

    /// <summary>Constructor to be used for writing the snapshot.</summary>
    /// <param name="serializerInstance">an instance of the originating serializer of this
    /// snapshot.</param>
    protected CompositeTypeSerializerSnapshot(TSerializer serializerInstance)
    {
        ArgumentNullException.ThrowIfNull(serializerInstance);
        _nestedSerializersSnapshotDelegate =
            new NestedSerializersSnapshotDelegate(GetNestedSerializers(serializerInstance));
    }

    public sealed override int CurrentVersion => Version;

    public sealed override void WriteSnapshot(IDataOutputView output)
    {
        InternalWriteOuterSnapshot(output);
        _nestedSerializersSnapshotDelegate!.WriteNestedSerializerSnapshots(output);
    }

    public sealed override void ReadSnapshot(int readVersion, IDataInputView input)
    {
        if (readVersion > HighestLegacyReadVersion)
        {
            InternalReadOuterSnapshot(input);
        }
        else
        {
            // legacy versions did not contain the pre-fixed magic numbers; just read the outer
            // snapshot
            ReadOuterSnapshot(readVersion, input);
        }
        _nestedSerializersSnapshotDelegate =
            NestedSerializersSnapshotDelegate.ReadNestedSerializerSnapshots(input);
    }

    public TypeSerializerSnapshot[] GetNestedSerializerSnapshots() =>
        _nestedSerializersSnapshotDelegate!.GetNestedSerializerSnapshots();

    public override TypeSerializerSchemaCompatibility<T> ResolveSchemaCompatibility(
        TypeSerializerSnapshot<T> oldSerializerSnapshot)
    {
        if (oldSerializerSnapshot is not CompositeTypeSerializerSnapshot<T, TSerializer> oldComposite)
        {
            return TypeSerializerSchemaCompatibility<T>.Incompatible();
        }

        return InternalResolveSchemaCompatibility(
            oldSerializerSnapshot, oldComposite.GetNestedSerializerSnapshots());
    }

    internal TypeSerializerSchemaCompatibility<T> InternalResolveSchemaCompatibility(
        TypeSerializerSnapshot<T> oldSnapshot, TypeSerializerSnapshot[] oldNestedSnapshots)
    {
        TypeSerializerSnapshot[] newNestedSerializerSnapshots =
            _nestedSerializersSnapshotDelegate!.GetNestedSerializerSnapshots();

        // check that nested serializer arity remains identical; if not, short circuit result
        if (newNestedSerializerSnapshots.Length != oldNestedSnapshots.Length)
        {
            return TypeSerializerSchemaCompatibility<T>.Incompatible();
        }

        OuterSchemaCompatibility outerSchemaCompatibility =
            ResolveOuterSchemaCompatibility(oldSnapshot);

        return ConstructFinalSchemaCompatibilityResult(
            newNestedSerializerSnapshots, oldNestedSnapshots, outerSchemaCompatibility);
    }

    internal void SetNestedSerializersSnapshotDelegate(NestedSerializersSnapshotDelegate del)
    {
        ArgumentNullException.ThrowIfNull(del);
        _nestedSerializersSnapshotDelegate = del;
    }

    public sealed override TypeSerializer<T> RestoreSerializer() =>
        CreateOuterSerializerWithNestedSerializers(
            _nestedSerializersSnapshotDelegate!.GetRestoredNestedSerializers());

    // ------------------------------------------------------------------------------------------
    //  Outer serializer access methods
    // ------------------------------------------------------------------------------------------

    /// <summary>The version of the current outer snapshot's written binary format.</summary>
    protected abstract int CurrentOuterSnapshotVersion { get; }

    /// <summary>Gets the nested serializers from the outer serializer.</summary>
    /// <param name="outerSerializer">the outer serializer.</param>
    protected abstract TypeSerializer[] GetNestedSerializers(TSerializer outerSerializer);

    /// <summary>Creates an instance of the outer serializer with a given array of its nested
    /// serializers.</summary>
    /// <param name="nestedSerializers">array of nested serializers to create the outer serializer
    /// with.</param>
    protected abstract TSerializer CreateOuterSerializerWithNestedSerializers(
        TypeSerializer[] nestedSerializers);

    // ------------------------------------------------------------------------------------------
    //  Outer snapshot methods; need to be overridden if the outer snapshot is not empty, or in
    //  other words, if the outer serializer has extra configuration beyond its nested serializers.
    // ------------------------------------------------------------------------------------------

    /// <summary>
    /// Writes the outer snapshot, i.e. any information beyond the nested serializers of the outer
    /// serializer. The base implementation writes nothing.
    /// </summary>
    protected virtual void WriteOuterSnapshot(IDataOutputView output)
    {
    }

    /// <summary>
    /// Reads the outer snapshot, i.e. any information beyond the nested serializers of the outer
    /// serializer. The base implementation reads nothing.
    /// </summary>
    protected virtual void ReadOuterSnapshot(int readOuterSnapshotVersion, IDataInputView input)
    {
    }

    /// <summary>
    /// Checks the schema compatibility of the given old serializer snapshot based on the outer
    /// snapshot. The base implementation assumes that the outer serializer only has nested
    /// serializers and no extra information, and therefore the result of the check is
    /// <see cref="OuterSchemaCompatibility.CompatibleAsIs"/>.
    /// </summary>
    protected virtual OuterSchemaCompatibility ResolveOuterSchemaCompatibility(
        TypeSerializerSnapshot<T> oldSerializerSnapshot) =>
        OuterSchemaCompatibility.CompatibleAsIs;

    // ------------------------------------------------------------------------------------------
    //  Utilities
    // ------------------------------------------------------------------------------------------

    private void InternalWriteOuterSnapshot(IDataOutputView output)
    {
        output.WriteInt(MagicNumber);
        output.WriteInt(CurrentOuterSnapshotVersion);

        WriteOuterSnapshot(output);
    }

    private void InternalReadOuterSnapshot(IDataInputView input)
    {
        int magicNumber = input.ReadInt();
        if (magicNumber != MagicNumber)
        {
            throw new IOException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Corrupt data, magic number mismatch. Expected {0:x8}, found {1:x8}",
                    MagicNumber,
                    magicNumber));
        }

        int outerSnapshotVersion = input.ReadInt();
        ReadOuterSnapshot(outerSnapshotVersion, input);
    }

    private TypeSerializerSchemaCompatibility<T> ConstructFinalSchemaCompatibilityResult(
        TypeSerializerSnapshot[] newNestedSerializerSnapshots,
        TypeSerializerSnapshot[] oldNestedSerializerSnapshots,
        OuterSchemaCompatibility outerSchemaCompatibility)
    {
        CompositeTypeSerializerUtil.IntermediateCompatibilityResult<T> nestedResult =
            CompositeTypeSerializerUtil.ConstructIntermediateCompatibilityResult<T>(
                newNestedSerializerSnapshots, oldNestedSerializerSnapshots);

        if (outerSchemaCompatibility == OuterSchemaCompatibility.Incompatible
            || nestedResult.IsIncompatible())
        {
            return TypeSerializerSchemaCompatibility<T>.Incompatible();
        }

        if (outerSchemaCompatibility == OuterSchemaCompatibility.CompatibleAfterMigration
            || nestedResult.IsCompatibleAfterMigration())
        {
            return TypeSerializerSchemaCompatibility<T>.CompatibleAfterMigration();
        }

        if (nestedResult.IsCompatibleWithReconfiguredSerializer())
        {
            TypeSerializer<T> reconfiguredCompositeSerializer =
                CreateOuterSerializerWithNestedSerializers(nestedResult.GetNestedSerializers());
            return TypeSerializerSchemaCompatibility<T>.CompatibleWithReconfiguredSerializer(
                reconfiguredCompositeSerializer);
        }

        return TypeSerializerSchemaCompatibility<T>.CompatibleAsIs();
    }
}
