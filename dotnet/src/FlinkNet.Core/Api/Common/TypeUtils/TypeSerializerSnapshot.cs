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
/// Non-generic base of <see cref="TypeSerializerSnapshot{T}"/>.
///
/// <para>PORT NOTE: Java declares <c>TypeSerializerSnapshot&lt;T&gt;</c> as an interface and
/// plumbs heterogeneous snapshots through wildcard arrays (<c>TypeSerializerSnapshot&lt;?&gt;[]</c>)
/// with unchecked casts. The port uses an abstract class pair instead: this base carries the
/// untyped members and internal bridges that replace the wildcard plumbing.</para>
/// </summary>
[PublicEvolving]
public abstract class TypeSerializerSnapshot
{
    private protected TypeSerializerSnapshot()
    {
    }

    /// <summary>Returns the version of the current snapshot's written binary format.</summary>
    public abstract int CurrentVersion { get; }

    /// <summary>
    /// Writes the serializer snapshot to the provided <see cref="IDataOutputView"/>. The current
    /// version of the written serializer snapshot's binary format is specified by
    /// <see cref="CurrentVersion"/>.
    ///
    /// <para>One should always use <see cref="WriteVersionedSnapshot"/> to write snapshots out,
    /// rather than directly calling this method.</para>
    /// </summary>
    public abstract void WriteSnapshot(IDataOutputView output);

    /// <summary>
    /// Reads the serializer snapshot from the provided <see cref="IDataInputView"/>. The version
    /// of the binary format that the serializer snapshot was written with is provided. This
    /// version can be used to determine how the serializer snapshot should be read.
    ///
    /// <para>PORT NOTE: Java's user-code ClassLoader parameter is dropped; .NET resolves types
    /// through the runtime.</para>
    /// </summary>
    public abstract void ReadSnapshot(int readVersion, IDataInputView input);

    /// <summary>Untyped variant of <see cref="TypeSerializerSnapshot{T}.RestoreSerializer"/>
    /// (see the class PORT NOTE).</summary>
    internal abstract TypeSerializer RestoreSerializerUntyped();

    /// <summary>Untyped variant of
    /// <see cref="TypeSerializerSnapshot{T}.ResolveSchemaCompatibility"/> (see the class
    /// PORT NOTE; Java resolves wildcard snapshot pairs with unchecked casts).</summary>
    internal abstract TypeSerializerSchemaCompatibility ResolveSchemaCompatibilityUntyped(
        TypeSerializerSnapshot oldSerializerSnapshot);

    // ------------------------------------------------------------------------
    //  read / write utilities
    // ------------------------------------------------------------------------

    /// <summary>
    /// Writes the given snapshot to the output stream. One should always use this method to write
    /// snapshots out, rather than directly calling <see cref="WriteSnapshot"/>.
    ///
    /// <para>The snapshot written with this method can be read via
    /// <see cref="ReadVersionedSnapshot"/>.</para>
    ///
    /// <para>PORT NOTE: Java writes the snapshot's Java class name; the port writes the CLR
    /// assembly-qualified type name, so snapshot streams are not portable across the two
    /// runtimes (they are structurally identical otherwise).</para>
    /// </summary>
    public static void WriteVersionedSnapshot(IDataOutputView output, TypeSerializerSnapshot snapshot)
    {
        output.WriteUTF(snapshot.GetType().AssemblyQualifiedName!);
        output.WriteInt(snapshot.CurrentVersion);
        snapshot.WriteSnapshot(output);
    }

    /// <summary>
    /// Reads a snapshot from the stream, performing resolving.
    ///
    /// <para>This method reads snapshots written by <see cref="WriteVersionedSnapshot"/>.</para>
    /// </summary>
    public static TypeSerializerSnapshot ReadVersionedSnapshot(IDataInputView input)
    {
        TypeSerializerSnapshot snapshot =
            TypeSerializerSnapshotSerializationUtil.ReadAndInstantiateSnapshotClass(input);

        int version = input.ReadInt();
        snapshot.ReadSnapshot(version, input);
        return snapshot;
    }
}

/// <summary>
/// A <c>TypeSerializerSnapshot</c> is a point-in-time view of a <see cref="TypeSerializer{T}"/>'s
/// configuration. The configuration snapshot of a serializer is persisted within checkpoints as a
/// single source of meta information about the schema of serialized data in the checkpoint. This
/// serves three purposes:
///
/// <list type="bullet">
/// <item><description><b>Capturing serializer parameters and schema:</b> a serializer's
/// configuration snapshot represents information about the parameters, state, and schema of a
/// serializer.</description></item>
/// <item><description><b>Compatibility checks for new serializers:</b> when new serializers are
/// available, they need to be checked whether or not they are compatible to read the data written
/// by the previous serializer.</description></item>
/// <item><description><b>Factory for a read serializer when schema conversion is required:</b> in
/// the case that new serializers are not compatible to read previous data, a schema conversion
/// process executed across all data is required before the new serializer can be continued to be
/// used. In this scenario, the serializer configuration snapshots in checkpoints double as a
/// factory for the read serializer of the conversion process.</description></item>
/// </list>
///
/// <para>NOTE: Implementations must contain the default empty nullary constructor. This is
/// required to be able to deserialize the configuration snapshot from its binary form.</para>
/// </summary>
/// <typeparam name="T">The data type that the originating serializer of this configuration
/// serializes.</typeparam>
[PublicEvolving]
public abstract class TypeSerializerSnapshot<T> : TypeSerializerSnapshot
{
    /// <summary>
    /// Recreates a serializer instance from this snapshot. The returned serializer can be safely
    /// used to read data written by the prior serializer (i.e., the serializer that created this
    /// snapshot).
    /// </summary>
    public abstract TypeSerializer<T> RestoreSerializer();

    /// <summary>
    /// Checks current serializer's compatibility to read data written by the prior serializer.
    ///
    /// <para>When a checkpoint/savepoint is restored, this method checks whether the
    /// serialization format of the data in the checkpoint/savepoint is compatible for the format
    /// of the serializer used by the program that restores the checkpoint/savepoint. The outcome
    /// can be that the serialization format is compatible, that the program's serializer needs to
    /// reconfigure itself (meaning to incorporate some information from the snapshot to be
    /// compatible), that the format is outright incompatible, or that a migration is needed.</para>
    /// </summary>
    /// <param name="oldSerializerSnapshot">the old serializer snapshot to check.</param>
    public abstract TypeSerializerSchemaCompatibility<T> ResolveSchemaCompatibility(
        TypeSerializerSnapshot<T> oldSerializerSnapshot);

    internal sealed override TypeSerializer RestoreSerializerUntyped() => RestoreSerializer();

    // PORT NOTE: Java's erased cast lets snapshots of a different type reach the typed resolve,
    // which then reports incompatible; reified generics surface that mismatch here instead.
    internal sealed override TypeSerializerSchemaCompatibility ResolveSchemaCompatibilityUntyped(
        TypeSerializerSnapshot oldSerializerSnapshot) =>
        oldSerializerSnapshot is TypeSerializerSnapshot<T> typedOldSnapshot
            ? ResolveSchemaCompatibility(typedOldSnapshot)
            : TypeSerializerSchemaCompatibility<T>.Incompatible();
}
