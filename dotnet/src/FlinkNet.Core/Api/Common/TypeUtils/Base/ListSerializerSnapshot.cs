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

namespace FlinkNet.Api.Common.TypeUtils.Base;

/// <summary>Snapshot class for the <see cref="ListSerializer{T}"/>.</summary>
public sealed class ListSerializerSnapshot<T>
    : CompositeTypeSerializerSnapshot<IList<T>, ListSerializer<T>>
{
    private const int SnapshotVersion = 1;

    /// <summary>Constructor for read instantiation.</summary>
    public ListSerializerSnapshot()
    {
    }

    /// <summary>Constructor to create the snapshot for writing.</summary>
    public ListSerializerSnapshot(ListSerializer<T> listSerializer)
        : base(listSerializer)
    {
    }

    protected override int CurrentOuterSnapshotVersion => SnapshotVersion;

    protected override ListSerializer<T> CreateOuterSerializerWithNestedSerializers(
        TypeSerializer[] nestedSerializers)
    {
        var elementSerializer = (TypeSerializer<T>)nestedSerializers[0];
        return new ListSerializer<T>(elementSerializer);
    }

    protected override TypeSerializer[] GetNestedSerializers(ListSerializer<T> outerSerializer) =>
        [outerSerializer.ElementSerializer];

    public TypeSerializerSnapshot<T> GetElementSerializerSnapshot() =>
        (TypeSerializerSnapshot<T>)GetNestedSerializerSnapshots()[0];
}
