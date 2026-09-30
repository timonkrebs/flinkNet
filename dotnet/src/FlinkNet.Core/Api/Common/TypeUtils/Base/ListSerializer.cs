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
/// A serializer for lists. The serializer relies on an element serializer for the serialization
/// of the list's elements.
///
/// <para>The serialization format for the list is as follows: four bytes for the length of the
/// list, followed by the serialized representation of each element.</para>
/// </summary>
/// <typeparam name="T">The type of element in the list.</typeparam>
[Internal]
public sealed class ListSerializer<T> : TypeSerializer<IList<T>>
{
    /// <summary>The serializer for the elements of the list.</summary>
    private readonly TypeSerializer<T> _elementSerializer;

    /// <summary>
    /// Creates a list serializer that uses the given serializer to serialize the list's elements.
    /// </summary>
    /// <param name="elementSerializer">The serializer for the elements of the list</param>
    public ListSerializer(TypeSerializer<T> elementSerializer)
    {
        ArgumentNullException.ThrowIfNull(elementSerializer);
        _elementSerializer = elementSerializer;
    }

    // ------------------------------------------------------------------------
    //  ListSerializer specific properties
    // ------------------------------------------------------------------------

    /// <summary>Gets the serializer for the elements of the list.</summary>
    public TypeSerializer<T> ElementSerializer => _elementSerializer;

    // ------------------------------------------------------------------------
    //  Type Serializer implementation
    // ------------------------------------------------------------------------

    public override bool IsImmutableType => false;

    public override TypeSerializer<IList<T>> Duplicate()
    {
        TypeSerializer<T> duplicateElement = _elementSerializer.Duplicate();
        return ReferenceEquals(duplicateElement, _elementSerializer)
            ? this
            : new ListSerializer<T>(duplicateElement);
    }

    public override IList<T> CreateInstance() => new List<T>(0);

    public override IList<T> Copy(IList<T> from)
    {
        var newList = new List<T>(from.Count);
        foreach (T element in from)
        {
            newList.Add(_elementSerializer.Copy(element));
        }
        return newList;
    }

    public override IList<T> Copy(IList<T> from, IList<T> reuse) => Copy(from);

    public override int Length => -1; // var length

    public override void Serialize(IList<T> record, IDataOutputView target)
    {
        int size = record.Count;
        target.WriteInt(size);

        foreach (T element in record)
        {
            _elementSerializer.Serialize(element, target);
        }
    }

    public override IList<T> Deserialize(IDataInputView source)
    {
        int size = source.ReadInt();
        // create new list with (size + 1) capacity to prevent expensive growth when a single
        // element is added
        var list = new List<T>(size + 1);
        for (int i = 0; i < size; i++)
        {
            list.Add(_elementSerializer.Deserialize(source));
        }
        return list;
    }

    public override IList<T> Deserialize(IList<T> reuse, IDataInputView source) =>
        Deserialize(source);

    public override void Copy(IDataInputView source, IDataOutputView target)
    {
        // copy number of elements
        int num = source.ReadInt();
        target.WriteInt(num);
        for (int i = 0; i < num; i++)
        {
            _elementSerializer.Copy(source, target);
        }
    }

    // --------------------------------------------------------------------

    public override bool Equals(object? obj) =>
        ReferenceEquals(obj, this)
            || (obj is not null
                && obj.GetType() == GetType()
                && _elementSerializer.Equals(((ListSerializer<T>)obj)._elementSerializer));

    public override int GetHashCode() => _elementSerializer.GetHashCode();

    public override TypeSerializerSnapshot<IList<T>> SnapshotConfiguration() =>
        new ListSerializerSnapshot<T>(this);
}
