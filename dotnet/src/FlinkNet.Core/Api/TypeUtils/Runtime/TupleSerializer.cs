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
/// A serializer for Flink tuples, delegating each field to a field serializer.
///
/// <para>PORT NOTE: Java's <c>TupleSerializerBase</c>/<c>TupleSerializer</c> pair is merged, and
/// Java's erased <c>TypeSerializer&lt;Object&gt;[]</c> field array maps to object-boxing adapter
/// serializers built reflectively from the typed field serializers (see
/// <see cref="ForFields"/>).</para>
/// </summary>
/// <typeparam name="T">The tuple type.</typeparam>
[Internal]
public sealed class TupleSerializer<T> : TypeSerializer<T>
    where T : Tuple, new()
{
    private readonly TypeSerializer<object?>[] _fieldSerializers;

    private readonly int _arity;

    private int _length = -2;

    private TupleSerializer(TypeSerializer<object?>[] fieldSerializers)
    {
        ArgumentNullException.ThrowIfNull(fieldSerializers);
        _fieldSerializers = fieldSerializers;
        _arity = fieldSerializers.Length;
    }

    /// <summary>
    /// Creates a tuple serializer from the typed field serializers (each must be a
    /// <c>TypeSerializer&lt;TField&gt;</c> for the corresponding tuple field).
    /// </summary>
    public static TupleSerializer<T> ForFields(params object[] fieldSerializers)
    {
        int arity = new T().Arity;
        if (fieldSerializers.Length != arity)
        {
            throw new ArgumentException(
                $"Tuple arity ({arity}) does not match the number of field serializers "
                    + $"({fieldSerializers.Length}).");
        }
        return new TupleSerializer<T>(
            fieldSerializers.Select(ObjectSerializerAdapter.Wrap).ToArray());
    }

    public IReadOnlyList<TypeSerializer<object?>> FieldSerializers => _fieldSerializers;

    public int Arity => _arity;

    // --------------------------------------------------------------------------------------------

    public override bool IsImmutableType => false;

    public override TypeSerializer<T> Duplicate()
    {
        bool stateful = false;
        var duplicateFieldSerializers = new TypeSerializer<object?>[_fieldSerializers.Length];

        for (int i = 0; i < _fieldSerializers.Length; i++)
        {
            duplicateFieldSerializers[i] = _fieldSerializers[i].Duplicate();
            if (!ReferenceEquals(duplicateFieldSerializers[i], _fieldSerializers[i]))
            {
                // at least one of the serializers is stateful
                stateful = true;
            }
        }

        return stateful ? new TupleSerializer<T>(duplicateFieldSerializers) : this;
    }

    public override T CreateInstance()
    {
        var tuple = new T();
        for (int i = 0; i < _arity; i++)
        {
            tuple.SetField(_fieldSerializers[i].CreateInstance(), i);
        }
        return tuple;
    }

    public override T Copy(T from)
    {
        var target = new T();
        for (int i = 0; i < _arity; i++)
        {
            object? copy = _fieldSerializers[i].Copy(from.GetField<object>(i));
            target.SetField(copy, i);
        }
        return target;
    }

    public override T Copy(T from, T reuse)
    {
        for (int i = 0; i < _arity; i++)
        {
            object? copy = _fieldSerializers[i].Copy(from.GetField<object>(i));
            reuse.SetField(copy, i);
        }
        return reuse;
    }

    public override int Length
    {
        get
        {
            if (_length == -2)
            {
                int sum = 0;
                foreach (TypeSerializer<object?> serializer in _fieldSerializers)
                {
                    if (serializer.Length > 0)
                    {
                        sum += serializer.Length;
                    }
                    else
                    {
                        _length = -1;
                        return _length;
                    }
                }
                _length = sum;
            }
            return _length;
        }
    }

    public override void Serialize(T record, IDataOutputView target)
    {
        for (int i = 0; i < _arity; i++)
        {
            object? value = record.GetField<object>(i);
            _fieldSerializers[i].Serialize(value, target);
        }
    }

    public override T Deserialize(IDataInputView source)
    {
        var tuple = new T();
        for (int i = 0; i < _arity; i++)
        {
            tuple.SetField(_fieldSerializers[i].Deserialize(source), i);
        }
        return tuple;
    }

    public override T Deserialize(T reuse, IDataInputView source)
    {
        for (int i = 0; i < _arity; i++)
        {
            reuse.SetField(_fieldSerializers[i].Deserialize(source), i);
        }
        return reuse;
    }

    public override void Copy(IDataInputView source, IDataOutputView target)
    {
        for (int i = 0; i < _arity; i++)
        {
            _fieldSerializers[i].Copy(source, target);
        }
    }

    // --------------------------------------------------------------------------------------------

    public override bool Equals(object? obj) =>
        ReferenceEquals(obj, this)
            || (obj is TupleSerializer<T> other
                && _arity == other._arity
                && _fieldSerializers.SequenceEqual(other._fieldSerializers));

    public override int GetHashCode() =>
        31 * typeof(T).GetHashCode()
            + _fieldSerializers.Aggregate(17, (hash, s) => 31 * hash + s.GetHashCode());
}

/// <summary>
/// Wraps a typed <c>TypeSerializer&lt;TField&gt;</c> as a <c>TypeSerializer&lt;object?&gt;</c>
/// (the C# stand-in for Java's erased <c>TypeSerializer&lt;Object&gt;</c>).
/// </summary>
[Internal]
public static class ObjectSerializerAdapter
{
    /// <summary>Wraps the given typed serializer into an object-typed serializer.</summary>
    public static TypeSerializer<object?> Wrap(object fieldSerializer)
    {
        ArgumentNullException.ThrowIfNull(fieldSerializer);
        if (fieldSerializer is TypeSerializer<object?> alreadyObject)
        {
            return alreadyObject;
        }

        Type? current = fieldSerializer.GetType();
        while (current is not null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(TypeSerializer<>))
            {
                Type fieldType = current.GetGenericArguments()[0];
                return (TypeSerializer<object?>)Activator.CreateInstance(
                    typeof(Adapter<>).MakeGenericType(fieldType), fieldSerializer)!;
            }
            current = current.BaseType;
        }

        throw new ArgumentException(
            $"The given object of type {fieldSerializer.GetType()} is not a TypeSerializer.");
    }

    private sealed class Adapter<TField>(TypeSerializer<TField> inner) : TypeSerializer<object?>
    {
        private readonly TypeSerializer<TField> _inner = inner;

        public override bool IsImmutableType => _inner.IsImmutableType;

        public override TypeSerializer<object?> Duplicate()
        {
            TypeSerializer<TField> duplicate = _inner.Duplicate();
            return ReferenceEquals(duplicate, _inner) ? this : new Adapter<TField>(duplicate);
        }

        public override object? CreateInstance() => _inner.CreateInstance();

        public override object? Copy(object? from) => _inner.Copy((TField)from!);

        public override object? Copy(object? from, object? reuse) => Copy(from);

        public override int Length => _inner.Length;

        public override void Serialize(object? record, IDataOutputView target) =>
            _inner.Serialize((TField)record!, target);

        public override object? Deserialize(IDataInputView source) => _inner.Deserialize(source);

        public override object? Deserialize(object? reuse, IDataInputView source) =>
            Deserialize(source);

        public override void Copy(IDataInputView source, IDataOutputView target) =>
            _inner.Copy(source, target);

        public override bool Equals(object? obj) =>
            obj is Adapter<TField> other && _inner.Equals(other._inner);

        public override int GetHashCode() => _inner.GetHashCode();
    }
}
