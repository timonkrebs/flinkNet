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
/// A serializer for arrays of objects.
///
/// <para>PORT NOTE: Java's constructor takes the component <c>Class</c> explicitly (an erasure
/// artifact used for reflective array creation); reified generics make it <c>typeof(C)</c>.</para>
/// </summary>
/// <typeparam name="C">The component type.</typeparam>
[Internal]
public sealed class GenericArraySerializer<C> : TypeSerializer<C?[]>
{
    private readonly TypeSerializer<C> _componentSerializer;

    private C?[]? _empty;

    public GenericArraySerializer(TypeSerializer<C> componentSerializer)
    {
        ArgumentNullException.ThrowIfNull(componentSerializer);
        _componentSerializer = componentSerializer;
    }

    public Type ComponentClass => typeof(C);

    public TypeSerializer<C> ComponentSerializer => _componentSerializer;

    public override bool IsImmutableType => false;

    public override TypeSerializer<C?[]> Duplicate()
    {
        TypeSerializer<C> duplicateComponentSerializer = _componentSerializer.Duplicate();
        return ReferenceEquals(duplicateComponentSerializer, _componentSerializer)
            ? this // is not stateful, return ourselves
            : new GenericArraySerializer<C>(duplicateComponentSerializer);
    }

    public override C?[] CreateInstance() => _empty ??= [];

    public override C?[] Copy(C?[] from)
    {
        TypeSerializer<C> serializer = _componentSerializer;

        if (serializer.IsImmutableType)
        {
            return (C?[])from.Clone();
        }

        var copy = new C?[from.Length];
        for (int i = 0; i < copy.Length; i++)
        {
            C? val = from[i];
            if (val != null)
            {
                copy[i] = serializer.Copy(val);
            }
        }
        return copy;
    }

    public override C?[] Copy(C?[] from, C?[] reuse) => Copy(from);

    public override int Length => -1;

    public override void Serialize(C?[] record, IDataOutputView target)
    {
        target.WriteInt(record.Length);
        foreach (C? val in record)
        {
            if (val == null)
            {
                target.WriteBoolean(false);
            }
            else
            {
                target.WriteBoolean(true);
                _componentSerializer.Serialize(val, target);
            }
        }
    }

    public override C?[] Deserialize(IDataInputView source)
    {
        int len = source.ReadInt();

        var array = new C?[len];
        for (int i = 0; i < len; i++)
        {
            bool isNonNull = source.ReadBoolean();
            array[i] = isNonNull ? _componentSerializer.Deserialize(source) : default;
        }

        return array;
    }

    public override C?[] Deserialize(C?[] reuse, IDataInputView source) => Deserialize(source);

    public override void Copy(IDataInputView source, IDataOutputView target)
    {
        int len = source.ReadInt();
        target.WriteInt(len);

        for (int i = 0; i < len; i++)
        {
            bool isNonNull = source.ReadBoolean();
            target.WriteBoolean(isNonNull);

            if (isNonNull)
            {
                _componentSerializer.Copy(source, target);
            }
        }
    }

    // --------------------------------------------------------------------------------------------

    public override int GetHashCode() =>
        31 * typeof(C).GetHashCode() + _componentSerializer.GetHashCode();

    public override bool Equals(object? obj) =>
        obj is GenericArraySerializer<C> other
            && _componentSerializer.Equals(other._componentSerializer);

    public override string ToString() => "Serializer " + typeof(C).Name + "[]";

    // --------------------------------------------------------------------------------------------
    // Serializer configuration snapshotting & compatibility
    // --------------------------------------------------------------------------------------------

    public override TypeSerializerSnapshot<C?[]> SnapshotConfiguration() =>
        new GenericArraySerializerSnapshot<C>(this);
}
