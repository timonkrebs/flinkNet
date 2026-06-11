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
/// A tuple with 8 fields. Tuples are strongly typed; each field may be of a separate type. The
/// fields of the tuple can be accessed directly as public fields (F0, F1, ...) or via their
/// position through the <see cref="GetField{T}(int)"/> method. The tuple field positions start
/// at zero.
///
/// <para>Tuples are mutable types, meaning that their fields can be re-assigned. This allows
/// functions that work with Tuples to reuse objects in order to reduce pressure on the garbage
/// collector.</para>
///
/// <para>Warning: If you subclass Tuple8, then be sure to either
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
[Public]
public class Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> : Tuple
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

    /// <summary>Creates a new tuple where all fields are null.</summary>
    public Tuple8() { }

    /// <summary>Creates a new tuple and assigns the given values to the tuple's fields.</summary>
    /// <param name="f0">The value for field 0</param>
    /// <param name="f1">The value for field 1</param>
    /// <param name="f2">The value for field 2</param>
    /// <param name="f3">The value for field 3</param>
    /// <param name="f4">The value for field 4</param>
    /// <param name="f5">The value for field 5</param>
    /// <param name="f6">The value for field 6</param>
    /// <param name="f7">The value for field 7</param>
    public Tuple8(T0? f0, T1? f1, T2? f2, T3? f3, T4? f4, T5? f5, T6? f6, T7? f7)
    {
        F0 = f0;
        F1 = f1;
        F2 = f2;
        F3 = f3;
        F4 = f4;
        F5 = f5;
        F6 = f6;
        F7 = f7;
    }

    public override int Arity => 8;

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
            _ => throw new IndexOutOfRangeException(pos.ToString()),
        };

    public override void SetField<T>(T? value, int pos) where T : default
    {
        switch (pos)
        {
            case 0:
                F0 = (T0?)(object?)value;
                break;
            case 1:
                F1 = (T1?)(object?)value;
                break;
            case 2:
                F2 = (T2?)(object?)value;
                break;
            case 3:
                F3 = (T3?)(object?)value;
                break;
            case 4:
                F4 = (T4?)(object?)value;
                break;
            case 5:
                F5 = (T5?)(object?)value;
                break;
            case 6:
                F6 = (T6?)(object?)value;
                break;
            case 7:
                F7 = (T7?)(object?)value;
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
    public void SetFields(T0? f0, T1? f1, T2? f2, T3? f3, T4? f4, T5? f5, T6? f6, T7? f7)
    {
        F0 = f0;
        F1 = f1;
        F2 = f2;
        F3 = f3;
        F4 = f4;
        F5 = f5;
        F6 = f6;
        F7 = f7;
    }

    // -------------------------------------------------------------------------------------------------
    // standard utilities
    // -------------------------------------------------------------------------------------------------

    /// <summary>
    /// Creates a string representation of the tuple in the form (f0, f1, f2, f3, f4, f5, f6, f7), where the
    /// individual fields are the value returned by calling <see cref="object.ToString"/> on that
    /// field.
    /// </summary>
    public override string ToString() =>
        "(" + ArrayAwareToString(F0) + "," + ArrayAwareToString(F1) + "," + ArrayAwareToString(F2) + "," + ArrayAwareToString(F3) + "," + ArrayAwareToString(F4) + "," + ArrayAwareToString(F5) + "," + ArrayAwareToString(F6) + "," + ArrayAwareToString(F7) + ")";

    /// <summary>Deep equality for tuples by calling Equals() on the tuple members.</summary>
    /// <param name="obj">the object checked for equality</param>
    /// <returns>true if this is equal to <paramref name="obj"/>.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj is not Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> tuple)
        {
            return false;
        }
        if (!Equals(F0, tuple.F0))
        {
            return false;
        }
        if (!Equals(F1, tuple.F1))
        {
            return false;
        }
        if (!Equals(F2, tuple.F2))
        {
            return false;
        }
        if (!Equals(F3, tuple.F3))
        {
            return false;
        }
        if (!Equals(F4, tuple.F4))
        {
            return false;
        }
        if (!Equals(F5, tuple.F5))
        {
            return false;
        }
        if (!Equals(F6, tuple.F6))
        {
            return false;
        }
        if (!Equals(F7, tuple.F7))
        {
            return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        int result = F0?.GetHashCode() ?? 0;
        result = 31 * result + (F1?.GetHashCode() ?? 0);
        result = 31 * result + (F2?.GetHashCode() ?? 0);
        result = 31 * result + (F3?.GetHashCode() ?? 0);
        result = 31 * result + (F4?.GetHashCode() ?? 0);
        result = 31 * result + (F5?.GetHashCode() ?? 0);
        result = 31 * result + (F6?.GetHashCode() ?? 0);
        result = 31 * result + (F7?.GetHashCode() ?? 0);
        return result;
    }

    /// <summary>Shallow tuple copy.</summary>
    /// <returns>A new Tuple with the same fields as this.</returns>
    public override Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> Copy() => new(F0, F1, F2, F3, F4, F5, F6, F7);
}

/// <summary>
/// Factory for <see cref="Tuple8{T0, T1, T2, T3, T4, T5, T6, T7}"/> allowing the compiler to infer the generic type
/// arguments implicitly. For example: <c>Tuple8.Of(...)</c> instead of
/// <c>new Tuple8&lt;...&gt;(...)</c>.
/// </summary>
[Public]
public static class Tuple8
{
    /// <summary>
    /// Creates a new tuple and assigns the given values to the tuple's fields. This is more
    /// convenient than using the constructor, because the compiler can infer the generic type
    /// arguments implicitly.
    /// </summary>
    public static Tuple8<T0, T1, T2, T3, T4, T5, T6, T7> Of<T0, T1, T2, T3, T4, T5, T6, T7>(T0? f0, T1? f1, T2? f2, T3? f3, T4? f4, T5? f5, T6? f6, T7? f7) => new(f0, f1, f2, f3, f4, f5, f6, f7);
}
