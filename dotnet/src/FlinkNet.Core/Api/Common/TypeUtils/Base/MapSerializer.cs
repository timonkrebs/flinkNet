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

namespace FlinkNet.Api.Common.TypeUtils.Base;

/// <summary>
/// A serializer for maps. The serializer relies on a key serializer and a value serializer for
/// the serialization of the map's key-value pairs.
///
/// <para>The serialization format for the map is as follows: four bytes for the length of the
/// map, followed by the serialized representation of each key-value pair. To allow null values,
/// each value is prefixed by a null flag.</para>
/// </summary>
/// <typeparam name="TKey">The type of the keys in the map.</typeparam>
/// <typeparam name="TValue">The type of the values in the map.</typeparam>
[Internal]
public sealed class MapSerializer<TKey, TValue> : TypeSerializer<IDictionary<TKey, TValue>>
    where TKey : notnull
{
    /// <summary>The serializer for the keys in the map.</summary>
    private readonly TypeSerializer<TKey> _keySerializer;

    /// <summary>The serializer for the values in the map.</summary>
    private readonly TypeSerializer<TValue> _valueSerializer;

    /// <summary>
    /// Creates a map serializer that uses the given serializers to serialize the key-value pairs
    /// in the map.
    /// </summary>
    /// <param name="keySerializer">The serializer for the keys in the map</param>
    /// <param name="valueSerializer">The serializer for the values in the map</param>
    public MapSerializer(TypeSerializer<TKey> keySerializer, TypeSerializer<TValue> valueSerializer)
    {
        ArgumentNullException.ThrowIfNull(keySerializer, "The key serializer cannot be null");
        ArgumentNullException.ThrowIfNull(valueSerializer, "The value serializer cannot be null.");
        _keySerializer = keySerializer;
        _valueSerializer = valueSerializer;
    }

    // ------------------------------------------------------------------------
    //  MapSerializer specific properties
    // ------------------------------------------------------------------------

    public TypeSerializer<TKey> KeySerializer => _keySerializer;

    public TypeSerializer<TValue> ValueSerializer => _valueSerializer;

    // ------------------------------------------------------------------------
    //  Type Serializer implementation
    // ------------------------------------------------------------------------

    public override bool IsImmutableType => false;

    public override TypeSerializer<IDictionary<TKey, TValue>> Duplicate()
    {
        TypeSerializer<TKey> duplicateKeySerializer = _keySerializer.Duplicate();
        TypeSerializer<TValue> duplicateValueSerializer = _valueSerializer.Duplicate();

        return ReferenceEquals(duplicateKeySerializer, _keySerializer)
                && ReferenceEquals(duplicateValueSerializer, _valueSerializer)
            ? this
            : new MapSerializer<TKey, TValue>(duplicateKeySerializer, duplicateValueSerializer);
    }

    public override IDictionary<TKey, TValue> CreateInstance() => new Dictionary<TKey, TValue>();

    public override IDictionary<TKey, TValue> Copy(IDictionary<TKey, TValue> from)
    {
        var newMap = new Dictionary<TKey, TValue>(from.Count);

        foreach (KeyValuePair<TKey, TValue> entry in from)
        {
            TKey newKey = _keySerializer.Copy(entry.Key);
            TValue newValue = entry.Value is null ? entry.Value : _valueSerializer.Copy(entry.Value);

            newMap[newKey] = newValue;
        }

        return newMap;
    }

    public override IDictionary<TKey, TValue> Copy(
        IDictionary<TKey, TValue> from, IDictionary<TKey, TValue> reuse) =>
        Copy(from);

    public override int Length => -1; // var length

    public override void Serialize(IDictionary<TKey, TValue> record, IDataOutputView target)
    {
        int size = record.Count;
        target.WriteInt(size);

        foreach (KeyValuePair<TKey, TValue> entry in record)
        {
            _keySerializer.Serialize(entry.Key, target);

            if (entry.Value is null)
            {
                target.WriteBoolean(true);
            }
            else
            {
                target.WriteBoolean(false);
                _valueSerializer.Serialize(entry.Value, target);
            }
        }
    }

    public override IDictionary<TKey, TValue> Deserialize(IDataInputView source)
    {
        int size = source.ReadInt();

        var map = new Dictionary<TKey, TValue>(size);
        for (int i = 0; i < size; ++i)
        {
            TKey key = _keySerializer.Deserialize(source);

            bool isNull = source.ReadBoolean();
            TValue value = isNull ? default! : _valueSerializer.Deserialize(source);

            map[key] = value;
        }

        return map;
    }

    public override IDictionary<TKey, TValue> Deserialize(
        IDictionary<TKey, TValue> reuse, IDataInputView source) =>
        Deserialize(source);

    public override void Copy(IDataInputView source, IDataOutputView target)
    {
        int size = source.ReadInt();
        target.WriteInt(size);

        for (int i = 0; i < size; ++i)
        {
            _keySerializer.Copy(source, target);

            bool isNull = source.ReadBoolean();
            target.WriteBoolean(isNull);

            if (!isNull)
            {
                _valueSerializer.Copy(source, target);
            }
        }
    }

    // --------------------------------------------------------------------

    public override bool Equals(object? obj) =>
        ReferenceEquals(obj, this)
            || (obj is not null
                && obj.GetType() == GetType()
                && _keySerializer.Equals(((MapSerializer<TKey, TValue>)obj)._keySerializer)
                && _valueSerializer.Equals(((MapSerializer<TKey, TValue>)obj)._valueSerializer));

    public override int GetHashCode() => _keySerializer.GetHashCode() * 31 + _valueSerializer.GetHashCode();

    public override TypeSerializerSnapshot<IDictionary<TKey, TValue>> SnapshotConfiguration() =>
        new MapSerializerSnapshot<TKey, TValue>(this);
}
