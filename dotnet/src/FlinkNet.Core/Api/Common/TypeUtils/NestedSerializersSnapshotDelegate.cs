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
/// A NestedSerializersSnapshotDelegate represents the snapshots of multiple serializers that are
/// used by an outer serializer. Examples would be tuples, where the outer serializer is the tuple
/// format serializer, and the NestedSerializersSnapshotDelegate holds the serializers for the
/// different tuple fields.
///
/// <para>The NestedSerializersSnapshotDelegate does not extend
/// <see cref="TypeSerializerSnapshot{T}"/>. It is not meant to be inherited from, but to be
/// composed with a serializer snapshot implementation.</para>
///
/// <para>The NestedSerializersSnapshotDelegate has its own versioning internally, it does not
/// couple its versioning to the versioning of the TypeSerializerSnapshot that builds on top of
/// this class. That way, the NestedSerializersSnapshotDelegate and the enclosing
/// TypeSerializerSnapshot can evolve their formats independently.</para>
/// </summary>
[Internal]
public class NestedSerializersSnapshotDelegate
{
    /// <summary>Magic number for integrity checks during deserialization.</summary>
    private const int MagicNumber = 1333245;

    /// <summary>Current version of the serialization format.</summary>
    private const int Version = 1;

    /// <summary>The snapshots from the serializers that make up this composition.</summary>
    private readonly TypeSerializerSnapshot[] _nestedSnapshots;

    /// <summary>Constructor to create a snapshot for writing.</summary>
    public NestedSerializersSnapshotDelegate(params TypeSerializer[] serializers)
    {
        _nestedSnapshots = TypeSerializerUtils.Snapshot(serializers);
    }

    /// <summary>Constructor to create a snapshot during deserialization.</summary>
    internal NestedSerializersSnapshotDelegate(TypeSerializerSnapshot[] snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        _nestedSnapshots = snapshots;
    }

    // ------------------------------------------------------------------------
    //  Nested Serializers and Compatibility
    // ------------------------------------------------------------------------

    /// <summary>
    /// Produces a restore serializer from each contained serializer configuration snapshot. The
    /// serializers are returned in the same order as the snapshots are stored.
    /// </summary>
    public TypeSerializer[] GetRestoredNestedSerializers() =>
        Array.ConvertAll(_nestedSnapshots, s => s.RestoreSerializerUntyped());

    /// <summary>Creates the restore serializer from the pos-th config snapshot.</summary>
    public TypeSerializer<T> GetRestoredNestedSerializer<T>(int pos)
    {
        if (pos >= _nestedSnapshots.Length)
        {
            throw new ArgumentException(null, nameof(pos));
        }
        var snapshot = (TypeSerializerSnapshot<T>)_nestedSnapshots[pos];
        return snapshot.RestoreSerializer();
    }

    /// <summary>Returns the snapshots of the nested serializers.</summary>
    public TypeSerializerSnapshot[] GetNestedSerializerSnapshots() => _nestedSnapshots;

    // ------------------------------------------------------------------------
    //  Serialization
    // ------------------------------------------------------------------------

    /// <summary>Writes the composite snapshot of all the contained serializers.</summary>
    public void WriteNestedSerializerSnapshots(IDataOutputView output)
    {
        output.WriteInt(MagicNumber);
        output.WriteInt(Version);

        output.WriteInt(_nestedSnapshots.Length);
        foreach (TypeSerializerSnapshot snapshot in _nestedSnapshots)
        {
            TypeSerializerSnapshot.WriteVersionedSnapshot(output, snapshot);
        }
    }

    /// <summary>Reads the composite snapshot of all the contained serializers.</summary>
    public static NestedSerializersSnapshotDelegate ReadNestedSerializerSnapshots(
        IDataInputView input)
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

        int version = input.ReadInt();
        if (version != Version)
        {
            throw new IOException("Unrecognized version: " + version);
        }

        int numSnapshots = input.ReadInt();
        var nestedSnapshots = new TypeSerializerSnapshot[numSnapshots];
        for (int i = 0; i < numSnapshots; i++)
        {
            nestedSnapshots[i] = TypeSerializerSnapshot.ReadVersionedSnapshot(input);
        }

        return new NestedSerializersSnapshotDelegate(nestedSnapshots);
    }
}
