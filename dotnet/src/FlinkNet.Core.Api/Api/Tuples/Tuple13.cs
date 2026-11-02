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
using FlinkNet.Util;

namespace FlinkNet.Api.Tuples;

/// <summary>
/// A tuple with 13 fields. Tuples are strongly typed; each field may be of a separate type. The
/// fields of the tuple can be accessed directly as public fields (F0, F1, ...) or via their
/// position through the <see cref="GetField{T}(int)"/> method. The tuple field positions start
/// at zero.
///
/// <para>Tuples are mutable types, meaning that their fields can be re-assigned. This allows
/// functions that work with Tuples to reuse objects in order to reduce pressure on the garbage
/// collector.</para>
///
/// <para>Warning: If you subclass Tuple13, then be sure to either
/// <list type="bullet">
///   <item><description>not add any new fields, or</description></item>
///   <item><description>make it a POJO, and always declare the element type of your
///   DataStreams to your descendant type.</description></item>
/// </list></para>
/// </summary>
/// <seealso cref="Tuple"/>
/// <typeparam name="T0">The type of field 0</typeparam>
/// <typeparam name="T1">The type of field 1</typeparam>
/// <typeparam name="T2">The type of field 2</typeparam>
/// <typeparam name="T3">The type of field 3</typeparam>
/// <typeparam name="T4">The type of field 4</typeparam>
/// <typeparam name="T5">The type of field 5</typeparam>
/// <typeparam name="T6">The type of field 6</typeparam>
/// <typeparam name="T7">The type of field 7</typeparam>
/// <typeparam name="T8">The type of field 8</typeparam>
/// <typeparam name="T9">The type of field 9</typeparam>
/// <typeparam name="T10">The type of field 10</typeparam>
/// <typeparam name="T11">The type of field 11</typeparam>
/// <typeparam name="T12">The type of field 12</typeparam>
[Public]
public class Tuple13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : Tuple
{
    /// <summary>Field 0 of the tuple.</summary>
    public T0? F0;

    /// <summary>Field 1 of the tuple.</summary>
    public T1? F1;

    /// <summary>Field 2 of the tuple.</summary>
    public T2? F2;

    /// <summary>Field 3 of the tuple.</summary>
    public T3? F3;

    /// <summary>Field 4 of the tuple.</summary>
    public T4? F4;

    /// <summary>Field 5 of the tuple.</summary>
    public T5? F5;

    /// <summary>Field 6 of the tuple.</summary>
    public T6? F6;

    /// <summary>Field 7 of the tuple.</summary>
    public T7? F7;

    /// <summary>Field 8 of the tuple.</summary>
    public T8? F8;

    /// <summary>Field 9 of the tuple.</summary>
    public T9? F9;

    /// <summary>Field 10 of the tuple.</summary>
    public T10? F10;

    /// <summary>Field 11 of the tuple.</summary>
    public T11? F11;

    /// <summary>Field 12 of the tuple.</summary>
    public T12? F12;

    /// <summary>
    /// Creates a new tuple where all fields have their default value: null for reference and
    /// nullable types. PORT NOTE: a field of a non-nullable value type (e.g. <c>int</c>) holds
    /// <c>default(T)</c> and cannot be null; use the nullable form (<c>int?</c>) for Java's
    /// nullable boxed fields such as <c>Integer</c>.
    /// </summary>
    public Tuple13() { }

    /// <summary>Creates a new tuple and assigns the given values to the tuple's fields.</summary>
    /// <param name="f0">The value for field 0</param>
    /// <param name="f1">The value for field 1</param>
    /// <param name="f2">The value for field 2</param>
    /// <param name="f3">The value for field 3</param>
    /// <param name="f4">The value for field 4</param>
    /// <param name="f5">The value for field 5</param>
    /// <param name="f6">The value for field 6</param>
    /// <param name="f7">The value for field 7</param>
    /// <param name="f8">The value for field 8</param>
    /// <param name="f9">The value for field 9</param>
    /// <param name="f10">The value for field 10</param>
    /// <param name="f11">The value for field 11</param>
    /// <param name="f12">The value for field 12</param>
    public Tuple13(T0? f0, T1? f1, T2? f2, T3? f3, T4? f4, T5? f5, T6? f6, T7? f7, T8? f8, T9? f9, T10? f10, T11? f11, T12? f12)
    {
        F0 = f0;
        F1 = f1;
        F2 = f2;
        F3 = f3;
        F4 = f4;
        F5 = f5;
        F6 = f6;
        F7 = f7;
        F8 = f8;
        F9 = f9;
        F10 = f10;
        F11 = f11;
        F12 = f12;
    }

    public override int Arity => 13;

    public override T? GetField<T>(int pos) where T : default =>
        pos switch
        {
            0 => (T?)(object?)F0,
            1 => (T?)(object?)F1,
            2 => (T?)(object?)F2,
            3 => (T?)(object?)F3,
            4 => (T?)(object?)F4,
            5 => (T?)(object?)F5,
            6 => (T?)(object?)F6,
            7 => (T?)(object?)F7,
            8 => (T?)(object?)F8,
            9 => (T?)(object?)F9,
            10 => (T?)(object?)F10,
            11 => (T?)(object?)F11,
            12 => (T?)(object?)F12,
            _ => throw new IndexOutOfRangeException(pos.ToString()),
        };

    public override void SetField<T>(T? value, int pos) where T : default
    {
        switch (pos)
        {
            case 0:
                F0 = CastField<T0>(value, 0);
                break;
            case 1:
                F1 = CastField<T1>(value, 1);
                break;
            case 2:
                F2 = CastField<T2>(value, 2);
                break;
            case 3:
                F3 = CastField<T3>(value, 3);
                break;
            case 4:
                F4 = CastField<T4>(value, 4);
                break;
            case 5:
                F5 = CastField<T5>(value, 5);
                break;
            case 6:
                F6 = CastField<T6>(value, 6);
                break;
            case 7:
                F7 = CastField<T7>(value, 7);
                break;
            case 8:
                F8 = CastField<T8>(value, 8);
                break;
            case 9:
                F9 = CastField<T9>(value, 9);
                break;
            case 10:
                F10 = CastField<T10>(value, 10);
                break;
            case 11:
                F11 = CastField<T11>(value, 11);
                break;
            case 12:
                F12 = CastField<T12>(value, 12);
                break;
            default:
                throw new IndexOutOfRangeException(pos.ToString());
        }
    }

    /// <summary>Sets new values to all fields of the tuple.</summary>
    /// <param name="f0">The value for field 0</param>
    /// <param name="f1">The value for field 1</param>
    /// <param name="f2">The value for field 2</param>
    /// <param name="f3">The value for field 3</param>
    /// <param name="f4">The value for field 4</param>
    /// <param name="f5">The value for field 5</param>
    /// <param name="f6">The value for field 6</param>
    /// <param name="f7">The value for field 7</param>
    /// <param name="f8">The value for field 8</param>
    /// <param name="f9">The value for field 9</param>
    /// <param name="f10">The value for field 10</param>
    /// <param name="f11">The value for field 11</param>
    /// <param name="f12">The value for field 12</param>
    public void SetFields(T0? f0, T1? f1, T2? f2, T3? f3, T4? f4, T5? f5, T6? f6, T7? f7, T8? f8, T9? f9, T10? f10, T11? f11, T12? f12)
    {
        F0 = f0;
        F1 = f1;
        F2 = f2;
        F3 = f3;
        F4 = f4;
        F5 = f5;
        F6 = f6;
        F7 = f7;
        F8 = f8;
        F9 = f9;
        F10 = f10;
        F11 = f11;
        F12 = f12;
    }

    // -------------------------------------------------------------------------------------------------
    // standard utilities
    // -------------------------------------------------------------------------------------------------

    /// <summary>
    /// Creates a string representation of the tuple in the form (f0, f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12), where the
    /// individual fields are the value returned by calling <see cref="object.ToString"/> on that
    /// field.
    /// </summary>
    public override string ToString() =>
        "(" + JavaPrimitives.DeepToString(F0) + "," + JavaPrimitives.DeepToString(F1) + "," + JavaPrimitives.DeepToString(F2) + "," + JavaPrimitives.DeepToString(F3) + "," + JavaPrimitives.DeepToString(F4) + "," + JavaPrimitives.DeepToString(F5) + "," + JavaPrimitives.DeepToString(F6) + "," + JavaPrimitives.DeepToString(F7) + "," + JavaPrimitives.DeepToString(F8) + "," + JavaPrimitives.DeepToString(F9) + "," + JavaPrimitives.DeepToString(F10) + "," + JavaPrimitives.DeepToString(F11) + "," + JavaPrimitives.DeepToString(F12) + ")";

    /// <summary>Deep equality for tuples with Java's <c>equals</c> on the tuple members (see
    /// <see cref="Tuple.FieldEquals"/>). Like Java's raw <c>instanceof</c> check, tuples of the
    /// same arity class compare structurally across generic instantiations.</summary>
    /// <param name="obj">the object checked for equality</param>
    /// <returns>true if this is equal to <paramref name="obj"/>.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        // fast path for the common same-instantiation case (no reflection, no boxing)
        if (obj is Tuple13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> same)
        {
            return FieldEquals(F0, same.F0)
                && FieldEquals(F1, same.F1)
                && FieldEquals(F2, same.F2)
                && FieldEquals(F3, same.F3)
                && FieldEquals(F4, same.F4)
                && FieldEquals(F5, same.F5)
                && FieldEquals(F6, same.F6)
                && FieldEquals(F7, same.F7)
                && FieldEquals(F8, same.F8)
                && FieldEquals(F9, same.F9)
                && FieldEquals(F10, same.F10)
                && FieldEquals(F11, same.F11)
                && FieldEquals(F12, same.F12);
        }
        if (!IsSameTupleClass(obj, typeof(Tuple13<,,,,,,,,,,,,>)))
        {
            return false;
        }
        var tuple = (Tuple)obj!;
        if (!JavaPrimitives.ObjectEquals(F0, tuple.GetField<object>(0)))
        {
            return false;
        }
        if (!JavaPrimitives.ObjectEquals(F1, tuple.GetField<object>(1)))
        {
            return false;
        }
        if (!JavaPrimitives.ObjectEquals(F2, tuple.GetField<object>(2)))
        {
            return false;
        }
        if (!JavaPrimitives.ObjectEquals(F3, tuple.GetField<object>(3)))
        {
            return false;
        }
        if (!JavaPrimitives.ObjectEquals(F4, tuple.GetField<object>(4)))
        {
            return false;
        }
        if (!JavaPrimitives.ObjectEquals(F5, tuple.GetField<object>(5)))
        {
            return false;
        }
        if (!JavaPrimitives.ObjectEquals(F6, tuple.GetField<object>(6)))
        {
            return false;
        }
        if (!JavaPrimitives.ObjectEquals(F7, tuple.GetField<object>(7)))
        {
            return false;
        }
        if (!JavaPrimitives.ObjectEquals(F8, tuple.GetField<object>(8)))
        {
            return false;
        }
        if (!JavaPrimitives.ObjectEquals(F9, tuple.GetField<object>(9)))
        {
            return false;
        }
        if (!JavaPrimitives.ObjectEquals(F10, tuple.GetField<object>(10)))
        {
            return false;
        }
        if (!JavaPrimitives.ObjectEquals(F11, tuple.GetField<object>(11)))
        {
            return false;
        }
        if (!JavaPrimitives.ObjectEquals(F12, tuple.GetField<object>(12)))
        {
            return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        int result = FieldHashCode(F0);
        result = 31 * result + FieldHashCode(F1);
        result = 31 * result + FieldHashCode(F2);
        result = 31 * result + FieldHashCode(F3);
        result = 31 * result + FieldHashCode(F4);
        result = 31 * result + FieldHashCode(F5);
        result = 31 * result + FieldHashCode(F6);
        result = 31 * result + FieldHashCode(F7);
        result = 31 * result + FieldHashCode(F8);
        result = 31 * result + FieldHashCode(F9);
        result = 31 * result + FieldHashCode(F10);
        result = 31 * result + FieldHashCode(F11);
        result = 31 * result + FieldHashCode(F12);
        return result;
    }

    /// <summary>Shallow tuple copy.</summary>
    /// <returns>A new Tuple with the same fields as this.</returns>
    public override Tuple13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Copy() => new(F0, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12);
}

/// <summary>
/// Factory for <see cref="Tuple13{T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12}"/> allowing the compiler to infer the generic type
/// arguments implicitly. For example: <c>Tuple13.Of(...)</c> instead of
/// <c>new Tuple13&lt;...&gt;(...)</c>.
/// </summary>
[Public]
public static class Tuple13
{
    /// <summary>
    /// Creates a new tuple and assigns the given values to the tuple's fields. This is more
    /// convenient than using the constructor, because the compiler can infer the generic type
    /// arguments implicitly.
    /// </summary>
    public static Tuple13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Of<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T0? f0, T1? f1, T2? f2, T3? f3, T4? f4, T5? f5, T6? f6, T7? f7, T8? f8, T9? f9, T10? f10, T11? f11, T12? f12) => new(f0, f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12);
}
