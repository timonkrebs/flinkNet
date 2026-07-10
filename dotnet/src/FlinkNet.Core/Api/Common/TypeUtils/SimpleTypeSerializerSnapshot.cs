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
/// A simple base class for TypeSerializerSnapshots, for serializers that have no parameters. The
/// serializer is defined solely by its class.
///
/// <para>Serializers that produce these snapshots must be public, have a public zero-argument
/// constructor and cannot be a non-static inner class.</para>
/// </summary>
[PublicEvolving]
public abstract class SimpleTypeSerializerSnapshot<T> : TypeSerializerSnapshot<T>
{
    /// <summary>
    /// This snapshot starts from version 2 (since Flink 1.7.x), so that version 1 is reserved for
    /// implementing backwards compatible code paths in case we decide to make this snapshot
    /// backwards compatible with the <c>ParameterlessTypeSerializerConfig</c>.
    /// </summary>
    private const int SnapshotVersion = 3;

    /// <summary>The supplier of the serializer for this snapshot.</summary>
    private readonly Func<TypeSerializer<T>> _serializerSupplier;

    /// <summary>Constructor to create snapshot from serializer (writing the snapshot).</summary>
    protected SimpleTypeSerializerSnapshot(Func<TypeSerializer<T>> serializerSupplier)
    {
        ArgumentNullException.ThrowIfNull(serializerSupplier);
        _serializerSupplier = serializerSupplier;
    }

    // ------------------------------------------------------------------------
    //  Serializer Snapshot Methods
    // ------------------------------------------------------------------------

    public override int CurrentVersion => SnapshotVersion;

    public override TypeSerializer<T> RestoreSerializer() => _serializerSupplier();

    public override TypeSerializerSchemaCompatibility<T> ResolveSchemaCompatibility(
        TypeSerializerSnapshot<T> oldSerializerSnapshot)
    {
        if (oldSerializerSnapshot is not SimpleTypeSerializerSnapshot<T> oldSimpleSnapshot)
        {
            return TypeSerializerSchemaCompatibility<T>.Incompatible();
        }
        return oldSimpleSnapshot.RestoreSerializer().GetType() == RestoreSerializer().GetType()
            ? TypeSerializerSchemaCompatibility<T>.CompatibleAsIs()
            : TypeSerializerSchemaCompatibility<T>.Incompatible();
    }

    public override void WriteSnapshot(IDataOutputView output)
    {
        //
    }

    public override void ReadSnapshot(int readVersion, IDataInputView input)
    {
        switch (readVersion)
        {
            case 3:
                break;
            case 2:
                // we don't need the classname any more; read and drop to maintain compatibility
                input.ReadUTF();
                break;
            default:
                throw new IOException("Unrecognized version: " + readVersion);
        }
    }

    // ------------------------------------------------------------------------
    //  standard utilities
    // ------------------------------------------------------------------------

    public sealed override bool Equals(object? obj) => obj != null && obj.GetType() == GetType();

    public sealed override int GetHashCode() => GetType().GetHashCode();

    public override string ToString() => GetType().Name;
}
