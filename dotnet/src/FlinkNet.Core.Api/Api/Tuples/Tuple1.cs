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

using FlinkNet.Annotations;

namespace FlinkNet.Api.Tuples;

/// <summary>
/// A tuple with 1 field. Tuples are strongly typed; each field may be of a separate type. The
/// fields of the tuple can be accessed directly as public fields (F0, F1, ...) or via their
/// position through the <see cref="GetField{T}(int)"/> method. The tuple field positions start
/// at zero.
///
/// <para>Tuples are mutable types, meaning that their fields can be re-assigned. This allows
/// functions that work with Tuples to reuse objects in order to reduce pressure on the garbage
/// collector.</para>
///
/// <para>Warning: If you subclass Tuple1, then be sure to either
/// <list type="bullet">
///   <item><description>not add any new fields, or</description></item>
///   <item><description>make it a POJO, and always declare the element type of your
///   DataStreams to your descendant type.</description></item>
/// </list></para>
/// </summary>
/// <seealso cref="Tuple"/>
/// <typeparam name="T0">The type of field 0</typeparam>
[Public]
public class Tuple1<T0> : Tuple
{
    /// <summary>Field 0 of the tuple.</summary>
    public T0? F0;

    /// <summary>Creates a new tuple where all fields are null.</summary>
    public Tuple1() { }

    /// <summary>Creates a new tuple and assigns the given values to the tuple's fields.</summary>
    /// <param name="f0">The value for field 0</param>
    public Tuple1(T0? f0)
    {
        F0 = f0;
    }

    public override int Arity => 1;

    public override T? GetField<T>(int pos) where T : default =>
        pos switch
        {
            0 => (T?)(object?)F0,
            _ => throw new IndexOutOfRangeException(pos.ToString()),
        };

    public override void SetField<T>(T? value, int pos) where T : default
    {
        switch (pos)
        {
            case 0:
                F0 = (T0?)(object?)value;
                break;
            default:
                throw new IndexOutOfRangeException(pos.ToString());
        }
    }

    /// <summary>Sets new values to all fields of the tuple.</summary>
    /// <param name="f0">The value for field 0</param>
    public void SetFields(T0? f0)
    {
        F0 = f0;
    }

    // -------------------------------------------------------------------------------------------------
    // standard utilities
    // -------------------------------------------------------------------------------------------------

    /// <summary>
    /// Creates a string representation of the tuple in the form (f0), where the
    /// individual fields are the value returned by calling <see cref="object.ToString"/> on that
    /// field.
    /// </summary>
    public override string ToString() =>
        "(" + ArrayAwareToString(F0) + ")";

    /// <summary>Deep equality for tuples by calling Equals() on the tuple members. Like Java's
    /// raw <c>instanceof</c> check, tuples of the same arity class compare structurally across
    /// generic instantiations.</summary>
    /// <param name="obj">the object checked for equality</param>
    /// <returns>true if this is equal to <paramref name="obj"/>.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (!IsSameTupleClass(obj, typeof(Tuple1<>)))
        {
            return false;
        }
        var tuple = (Tuple)obj!;
        if (!Equals(F0, tuple.GetField<object>(0)))
        {
            return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        int result = F0?.GetHashCode() ?? 0;
        return result;
    }

    /// <summary>Shallow tuple copy.</summary>
    /// <returns>A new Tuple with the same fields as this.</returns>
    public override Tuple1<T0> Copy() => new(F0);
}

/// <summary>
/// Factory for <see cref="Tuple1{T0}"/> allowing the compiler to infer the generic type
/// arguments implicitly. For example: <c>Tuple1.Of(...)</c> instead of
/// <c>new Tuple1&lt;...&gt;(...)</c>.
/// </summary>
[Public]
public static class Tuple1
{
    /// <summary>
    /// Creates a new tuple and assigns the given values to the tuple's fields. This is more
    /// convenient than using the constructor, because the compiler can infer the generic type
    /// arguments implicitly.
    /// </summary>
    public static Tuple1<T0> Of<T0>(T0? f0) => new(f0);
}
