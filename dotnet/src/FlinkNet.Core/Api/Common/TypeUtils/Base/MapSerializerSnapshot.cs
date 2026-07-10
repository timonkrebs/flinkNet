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

/// <summary>Snapshot class for the <see cref="MapSerializer{TKey, TValue}"/>.</summary>
public sealed class MapSerializerSnapshot<TKey, TValue>
    : CompositeTypeSerializerSnapshot<IDictionary<TKey, TValue>, MapSerializer<TKey, TValue>>
    where TKey : notnull
{
    private const int SnapshotVersion = 1;

    /// <summary>Constructor for read instantiation.</summary>
    public MapSerializerSnapshot()
    {
    }

    /// <summary>Constructor to create the snapshot for writing.</summary>
    public MapSerializerSnapshot(MapSerializer<TKey, TValue> mapSerializer)
        : base(mapSerializer)
    {
    }

    protected override int CurrentOuterSnapshotVersion => SnapshotVersion;

    protected override MapSerializer<TKey, TValue> CreateOuterSerializerWithNestedSerializers(
        TypeSerializer[] nestedSerializers)
    {
        var keySerializer = (TypeSerializer<TKey>)nestedSerializers[0];
        var valueSerializer = (TypeSerializer<TValue>)nestedSerializers[1];
        return new MapSerializer<TKey, TValue>(keySerializer, valueSerializer);
    }

    protected override TypeSerializer[] GetNestedSerializers(
        MapSerializer<TKey, TValue> outerSerializer) =>
        [outerSerializer.KeySerializer, outerSerializer.ValueSerializer];

    public TypeSerializerSnapshot<TKey> GetKeySerializerSnapshot() =>
        (TypeSerializerSnapshot<TKey>)GetNestedSerializerSnapshots()[0];

    public TypeSerializerSnapshot<TValue> GetValueSerializerSnapshot() =>
        (TypeSerializerSnapshot<TValue>)GetNestedSerializerSnapshots()[1];
}
