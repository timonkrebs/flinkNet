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

// --------------------------------------------------------------
//  THIS IS A GENERATED SOURCE FILE. DO NOT EDIT!
//  GENERATED FROM tools/generate_tuples.py
//  (C# port of org.apache.flink.api.java.tuple.TupleGenerator)
// --------------------------------------------------------------

using System.Text;
using FlinkNet.Annotations;
using FlinkNet.Types;

namespace FlinkNet.Api.Tuples;

/// <summary>
/// The base class of all tuples. Tuples have a fix length and contain a set of fields, which may
/// all be of different types. Because Tuples are strongly typed, each distinct tuple length is
/// represented by its own class. Tuples exists with up to 25 fields and are described in the
/// classes <see cref="Tuple1{T0}"/> to Tuple25.
///
/// <para>The fields in the tuples may be accessed directly as public fields, or via position
/// (zero indexed) <see cref="GetField{T}(int)"/>.</para>
/// </summary>
[Public]
public abstract class Tuple
{
    public const int MaxArity = 25;

    /// <summary>Gets the field at the specified position.</summary>
    /// <param name="pos">The position of the field, zero indexed.</param>
    /// <returns>The field at the specified position.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown, if the position is negative, or equal
    /// to, or larger than the number of fields.</exception>
    public abstract T? GetField<T>(int pos);

    /// <summary>
    /// Gets the field at the specified position, throws <see cref="NullFieldException"/> if the
    /// field is null. Used for comparing key fields.
    /// </summary>
    /// <param name="pos">The position of the field, zero indexed.</param>
    /// <returns>The field at the specified position.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown, if the position is negative, or equal
    /// to, or larger than the number of fields.</exception>
    /// <exception cref="NullFieldException">Thrown, if the field at pos is null.</exception>
    public T GetFieldNotNull<T>(int pos)
    {
        T? field = GetField<T>(pos);
        return field is not null ? field : throw new NullFieldException(pos);
    }

    /// <summary>Sets the field at the specified position.</summary>
    /// <param name="value">The value to be assigned to the field at the specified position.</param>
    /// <param name="pos">The position of the field, zero indexed.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown, if the position is negative, or equal
    /// to, or larger than the number of fields.</exception>
    public abstract void SetField<T>(T? value, int pos);

    /// <summary>Gets the number of fields in the tuple (the tuple arity).</summary>
    public abstract int Arity { get; }

    /// <summary>Shallow tuple copy.</summary>
    /// <returns>A new Tuple with the same fields as this.</returns>
    public abstract Tuple Copy();

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Gets the type corresponding to the tuple of the given arity (dimensions). For example,
    /// <c>GetTupleType(3)</c> will return the open generic <c>Tuple3&lt;,,&gt;</c> type.
    /// </summary>
    /// <param name="arity">The arity of the tuple type to get.</param>
    /// <returns>The tuple type with the given arity.</returns>
    public static Type GetTupleType(int arity)
    {
        if (arity < 0 || arity > MaxArity)
        {
            throw new ArgumentException($"The tuple arity must be in [0, {MaxArity}].");
        }
        return Types[arity];
    }

    /// <summary>
    /// Creates a new tuple of the given arity, with all field types being <c>object</c> and all
    /// fields null (Tuple0 returns the shared <see cref="Tuple0.Instance"/>).
    /// </summary>
    public static Tuple NewInstance(int arity) =>
        arity switch
        {
            0 => Tuple0.Instance,
            1 => new Tuple1<object>(),
            2 => new Tuple2<object, object>(),
            3 => new Tuple3<object, object, object>(),
            4 => new Tuple4<object, object, object, object>(),
            5 => new Tuple5<object, object, object, object, object>(),
            6 => new Tuple6<object, object, object, object, object, object>(),
            7 => new Tuple7<object, object, object, object, object, object, object>(),
            8 => new Tuple8<object, object, object, object, object, object, object, object>(),
            9 => new Tuple9<object, object, object, object, object, object, object, object, object>(),
            10 => new Tuple10<object, object, object, object, object, object, object, object, object, object>(),
            11 => new Tuple11<object, object, object, object, object, object, object, object, object, object, object>(),
            12 => new Tuple12<object, object, object, object, object, object, object, object, object, object, object, object>(),
            13 => new Tuple13<object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            14 => new Tuple14<object, object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            15 => new Tuple15<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            16 => new Tuple16<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            17 => new Tuple17<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            18 => new Tuple18<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            19 => new Tuple19<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            20 => new Tuple20<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            21 => new Tuple21<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            22 => new Tuple22<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            23 => new Tuple23<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            24 => new Tuple24<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            25 => new Tuple25<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(),
            _ => throw new ArgumentException($"The tuple arity must be in [0, {MaxArity}]."),
        };

    private static readonly Type[] Types =
    [
        typeof(Tuple0),
        typeof(Tuple1<>),
        typeof(Tuple2<,>),
        typeof(Tuple3<,,>),
        typeof(Tuple4<,,,>),
        typeof(Tuple5<,,,,>),
        typeof(Tuple6<,,,,,>),
        typeof(Tuple7<,,,,,,>),
        typeof(Tuple8<,,,,,,,>),
        typeof(Tuple9<,,,,,,,,>),
        typeof(Tuple10<,,,,,,,,,>),
        typeof(Tuple11<,,,,,,,,,,>),
        typeof(Tuple12<,,,,,,,,,,,>),
        typeof(Tuple13<,,,,,,,,,,,,>),
        typeof(Tuple14<,,,,,,,,,,,,,>),
        typeof(Tuple15<,,,,,,,,,,,,,,>),
        typeof(Tuple16<,,,,,,,,,,,,,,,>),
        typeof(Tuple17<,,,,,,,,,,,,,,,,>),
        typeof(Tuple18<,,,,,,,,,,,,,,,,,>),
        typeof(Tuple19<,,,,,,,,,,,,,,,,,,>),
        typeof(Tuple20<,,,,,,,,,,,,,,,,,,,>),
        typeof(Tuple21<,,,,,,,,,,,,,,,,,,,,>),
        typeof(Tuple22<,,,,,,,,,,,,,,,,,,,,,>),
        typeof(Tuple23<,,,,,,,,,,,,,,,,,,,,,,>),
        typeof(Tuple24<,,,,,,,,,,,,,,,,,,,,,,,>),
        typeof(Tuple25<,,,,,,,,,,,,,,,,,,,,,,,,>),
    ];

    /// <summary>
    /// Converts the given object into a string representation by calling
    /// <see cref="object.ToString"/> and formatting (possibly nested) arrays and <c>null</c>.
    /// Mirrors the format of Java's <c>Arrays.deepToString</c>.
    /// </summary>
    internal static string ArrayAwareToString(object? o)
    {
        if (o is null)
        {
            return "null";
        }

        if (o is Array array)
        {
            var builder = new StringBuilder("[");
            bool first = true;
            foreach (object? element in array)
            {
                if (!first)
                {
                    builder.Append(", ");
                }
                first = false;
                builder.Append(ArrayAwareToString(element));
            }
            return builder.Append(']').ToString();
        }

        return o.ToString() ?? "null";
    }

    /// <summary>
    /// Whether the object is an instantiation (or a subclass of an instantiation) of the given
    /// open generic tuple class. PORT NOTE: replicates Java's raw <c>instanceof TupleN</c>
    /// checks, which compare tuples structurally across generic instantiations.
    /// </summary>
    private protected static bool IsSameTupleClass(object? obj, Type openTupleType)
    {
        for (Type? type = obj?.GetType(); type != null; type = type.BaseType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == openTupleType)
            {
                return true;
            }
        }
        return false;
    }
}
