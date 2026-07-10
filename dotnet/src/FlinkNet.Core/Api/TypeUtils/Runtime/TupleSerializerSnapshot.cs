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
using FlinkNet.Api.Common.TypeUtils;
using FlinkNet.Core.Memory;
using Tuple = FlinkNet.Api.Tuples.Tuple;

namespace FlinkNet.Api.TypeUtils.Runtime;

/// <summary>
/// Snapshot of a tuple serializer's configuration.
///
/// <para>PORT NOTE: Java's erased snapshot persists the tuple class in the outer snapshot and
/// needs it to recreate the serializer. The port's snapshot class is a closed generic (the type
/// name written by <c>WriteVersionedSnapshot</c> pins <typeparamref name="T"/> already), so the
/// persisted tuple class name doubles as an integrity check on read. The nested serializers are
/// the unwrapped typed field serializers, not the object-boxing adapters.</para>
/// </summary>
[Internal]
public sealed class TupleSerializerSnapshot<T> : CompositeTypeSerializerSnapshot<T, TupleSerializer<T>>
    where T : Tuple, new()
{
    private const int SnapshotVersion = 2;

    /// <summary>Constructor for read instantiation.</summary>
    public TupleSerializerSnapshot()
    {
    }

    /// <summary>Constructor to create the snapshot for writing.</summary>
    internal TupleSerializerSnapshot(TupleSerializer<T> serializerInstance)
        : base(serializerInstance)
    {
    }

    protected override int CurrentOuterSnapshotVersion => SnapshotVersion;

    protected override TypeSerializer[] GetNestedSerializers(TupleSerializer<T> outerSerializer) =>
        outerSerializer.FieldSerializers.Select(ObjectSerializerAdapter.Unwrap).ToArray();

    protected override TupleSerializer<T> CreateOuterSerializerWithNestedSerializers(
        TypeSerializer[] nestedSerializers) =>
        TupleSerializer<T>.ForFields(nestedSerializers.Cast<object>().ToArray());

    protected override void WriteOuterSnapshot(IDataOutputView output) =>
        output.WriteUTF(typeof(T).AssemblyQualifiedName!);

    protected override void ReadOuterSnapshot(int readOuterSnapshotVersion, IDataInputView input)
    {
        string tupleClassName = input.ReadUTF();
        Type tupleClass;
        try
        {
            tupleClass = Type.GetType(tupleClassName, throwOnError: true)!;
        }
        catch (Exception e)
        {
            throw new IOException(
                "Could not find the tuple class '" + tupleClassName + "'.", e);
        }
        if (tupleClass != typeof(T))
        {
            throw new IOException(
                "The tuple class in the snapshot (" + tupleClassName
                    + ") does not match the snapshot's tuple type " + typeof(T) + ".");
        }
    }
}
