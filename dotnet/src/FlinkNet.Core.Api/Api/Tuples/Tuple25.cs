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
/// A tuple with 25 fields. Tuples are strongly typed; each field may be of a separate type. The
/// fields of the tuple can be accessed directly as public fields (F0, F1, ...) or via their
/// position through the <see cref="GetField{T}(int)"/> method. The tuple field positions start
/// at zero.
///
/// <para>Tuples are mutable types, meaning that their fields can be re-assigned. This allows
/// functions that work with Tuples to reuse objects in order to reduce pressure on the garbage
/// collector.</para>
///
/// <para>Warning: If you subclass Tuple25, then be sure to either
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
/// <typeparam name="T13">The type of field 13</typeparam>
/// <typeparam name="T14">The type of field 14</typeparam>
/// <typeparam name="T15">The type of field 15</typeparam>
/// <typeparam name="T16">The type of field 16</typeparam>
/// <typeparam name="T17">The type of field 17</typeparam>
/// <typeparam name="T18">The type of field 18</typeparam>
/// <typeparam name="T19">The type of field 19</typeparam>
/// <typeparam name="T20">The type of field 20</typeparam>
/// <typeparam name="T21">The type of field 21</typeparam>
/// <typeparam name="T22">The type of field 22</typeparam>
/// <typeparam name="T23">The type of field 23</typeparam>
/// <typeparam name="T24">The type of field 24</typeparam>
[Public]
public class Tuple25<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> : Tuple
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

    /// <summary>Field 13 of the tuple.</summary>
    public T13? F13;

    /// <summary>Field 14 of the tuple.</summary>
    public T14? F14;

    /// <summary>Field 15 of the tuple.</summary>
    public T15? F15;

    /// <summary>Field 16 of the tuple.</summary>
    public T16? F16;

    /// <summary>Field 17 of the tuple.</summary>
    public T17? F17;

    /// <summary>Field 18 of the tuple.</summary>
    public T18? F18;

    /// <summary>Field 19 of the tuple.</summary>
    public T19? F19;

    /// <summary>Field 20 of the tuple.</summary>
    public T20? F20;

    /// <summary>Field 21 of the tuple.</summary>
    public T21? F21;

    /// <summary>Field 22 of the tuple.</summary>
    public T22? F22;

    /// <summary>Field 23 of the tuple.</summary>
    public T23? F23;

    /// <summary>Field 24 of the tuple.</summary>
    public T24? F24;

    /// <summary>Creates a new tuple where all fields are null.</summary>
    public Tuple25() { }

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
    /// <param name="f13">The value for field 13</param>
    /// <param name="f14">The value for field 14</param>
    /// <param name="f15">The value for field 15</param>
    /// <param name="f16">The value for field 16</param>
    /// <param name="f17">The value for field 17</param>
    /// <param name="f18">The value for field 18</param>
    /// <param name="f19">The value for field 19</param>
    /// <param name="f20">The value for field 20</param>
    /// <param name="f21">The value for field 21</param>
    /// <param name="f22">The value for field 22</param>
    /// <param name="f23">The value for field 23</param>
    /// <param name="f24">The value for field 24</param>
    public Tuple25(T0? f0, T1? f1, T2? f2, T3? f3, T4? f4, T5? f5, T6? f6, T7? f7, T8? f8, T9? f9, T10? f10, T11? f11, T12? f12, T13? f13, T14? f14, T15? f15, T16? f16, T17? f17, T18? f18, T19? f19, T20? f20, T21? f21, T22? f22, T23? f23, T24? f24)
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
        F13 = f13;
        F14 = f14;
        F15 = f15;
        F16 = f16;
        F17 = f17;
        F18 = f18;
        F19 = f19;
        F20 = f20;
        F21 = f21;
        F22 = f22;
        F23 = f23;
        F24 = f24;
    }

    public override int Arity => 25;

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
            13 => (T?)(object?)F13,
            14 => (T?)(object?)F14,
            15 => (T?)(object?)F15,
            16 => (T?)(object?)F16,
            17 => (T?)(object?)F17,
            18 => (T?)(object?)F18,
            19 => (T?)(object?)F19,
            20 => (T?)(object?)F20,
            21 => (T?)(object?)F21,
            22 => (T?)(object?)F22,
            23 => (T?)(object?)F23,
            24 => (T?)(object?)F24,
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
            case 8:
                F8 = (T8?)(object?)value;
                break;
            case 9:
                F9 = (T9?)(object?)value;
                break;
            case 10:
                F10 = (T10?)(object?)value;
                break;
            case 11:
                F11 = (T11?)(object?)value;
                break;
            case 12:
                F12 = (T12?)(object?)value;
                break;
            case 13:
                F13 = (T13?)(object?)value;
                break;
            case 14:
                F14 = (T14?)(object?)value;
                break;
            case 15:
                F15 = (T15?)(object?)value;
                break;
            case 16:
                F16 = (T16?)(object?)value;
                break;
            case 17:
                F17 = (T17?)(object?)value;
                break;
            case 18:
                F18 = (T18?)(object?)value;
                break;
            case 19:
                F19 = (T19?)(object?)value;
                break;
            case 20:
                F20 = (T20?)(object?)value;
                break;
            case 21:
                F21 = (T21?)(object?)value;
                break;
            case 22:
                F22 = (T22?)(object?)value;
                break;
            case 23:
                F23 = (T23?)(object?)value;
                break;
            case 24:
                F24 = (T24?)(object?)value;
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
    /// <param name="f13">The value for field 13</param>
    /// <param name="f14">The value for field 14</param>
    /// <param name="f15">The value for field 15</param>
    /// <param name="f16">The value for field 16</param>
    /// <param name="f17">The value for field 17</param>
    /// <param name="f18">The value for field 18</param>
    /// <param name="f19">The value for field 19</param>
    /// <param name="f20">The value for field 20</param>
    /// <param name="f21">The value for field 21</param>
    /// <param name="f22">The value for field 22</param>
    /// <param name="f23">The value for field 23</param>
    /// <param name="f24">The value for field 24</param>
    public void SetFields(T0? f0, T1? f1, T2? f2, T3? f3, T4? f4, T5? f5, T6? f6, T7? f7, T8? f8, T9? f9, T10? f10, T11? f11, T12? f12, T13? f13, T14? f14, T15? f15, T16? f16, T17? f17, T18? f18, T19? f19, T20? f20, T21? f21, T22? f22, T23? f23, T24? f24)
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
        F13 = f13;
        F14 = f14;
        F15 = f15;
        F16 = f16;
        F17 = f17;
        F18 = f18;
        F19 = f19;
        F20 = f20;
        F21 = f21;
        F22 = f22;
        F23 = f23;
        F24 = f24;
    }

    // -------------------------------------------------------------------------------------------------
    // standard utilities
    // -------------------------------------------------------------------------------------------------

    /// <summary>
    /// Creates a string representation of the tuple in the form (f0, f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14, f15, f16, f17, f18, f19, f20, f21, f22, f23, f24), where the
    /// individual fields are the value returned by calling <see cref="object.ToString"/> on that
    /// field.
    /// </summary>
    public override string ToString() =>
        "(" + ArrayAwareToString(F0) + "," + ArrayAwareToString(F1) + "," + ArrayAwareToString(F2) + "," + ArrayAwareToString(F3) + "," + ArrayAwareToString(F4) + "," + ArrayAwareToString(F5) + "," + ArrayAwareToString(F6) + "," + ArrayAwareToString(F7) + "," + ArrayAwareToString(F8) + "," + ArrayAwareToString(F9) + "," + ArrayAwareToString(F10) + "," + ArrayAwareToString(F11) + "," + ArrayAwareToString(F12) + "," + ArrayAwareToString(F13) + "," + ArrayAwareToString(F14) + "," + ArrayAwareToString(F15) + "," + ArrayAwareToString(F16) + "," + ArrayAwareToString(F17) + "," + ArrayAwareToString(F18) + "," + ArrayAwareToString(F19) + "," + ArrayAwareToString(F20) + "," + ArrayAwareToString(F21) + "," + ArrayAwareToString(F22) + "," + ArrayAwareToString(F23) + "," + ArrayAwareToString(F24) + ")";

    /// <summary>Deep equality for tuples by calling Equals() on the tuple members.</summary>
    /// <param name="obj">the object checked for equality</param>
    /// <returns>true if this is equal to <paramref name="obj"/>.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj is not Tuple25<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> tuple)
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
        if (!Equals(F8, tuple.F8))
        {
            return false;
        }
        if (!Equals(F9, tuple.F9))
        {
            return false;
        }
        if (!Equals(F10, tuple.F10))
        {
            return false;
        }
        if (!Equals(F11, tuple.F11))
        {
            return false;
        }
        if (!Equals(F12, tuple.F12))
        {
            return false;
        }
        if (!Equals(F13, tuple.F13))
        {
            return false;
        }
        if (!Equals(F14, tuple.F14))
        {
            return false;
        }
        if (!Equals(F15, tuple.F15))
        {
            return false;
        }
        if (!Equals(F16, tuple.F16))
        {
            return false;
        }
        if (!Equals(F17, tuple.F17))
        {
            return false;
        }
        if (!Equals(F18, tuple.F18))
        {
            return false;
        }
        if (!Equals(F19, tuple.F19))
        {
            return false;
        }
        if (!Equals(F20, tuple.F20))
        {
            return false;
        }
        if (!Equals(F21, tuple.F21))
        {
            return false;
        }
        if (!Equals(F22, tuple.F22))
        {
            return false;
        }
        if (!Equals(F23, tuple.F23))
        {
            return false;
        }
        if (!Equals(F24, tuple.F24))
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
        result = 31 * result + (F8?.GetHashCode() ?? 0);
        result = 31 * result + (F9?.GetHashCode() ?? 0);
        result = 31 * result + (F10?.GetHashCode() ?? 0);
        result = 31 * result + (F11?.GetHashCode() ?? 0);
        result = 31 * result + (F12?.GetHashCode() ?? 0);
        result = 31 * result + (F13?.GetHashCode() ?? 0);
        result = 31 * result + (F14?.GetHashCode() ?? 0);
        result = 31 * result + (F15?.GetHashCode() ?? 0);
        result = 31 * result + (F16?.GetHashCode() ?? 0);
        result = 31 * result + (F17?.GetHashCode() ?? 0);
        result = 31 * result + (F18?.GetHashCode() ?? 0);
        result = 31 * result + (F19?.GetHashCode() ?? 0);
        result = 31 * result + (F20?.GetHashCode() ?? 0);
        result = 31 * result + (F21?.GetHashCode() ?? 0);
        result = 31 * result + (F22?.GetHashCode() ?? 0);
        result = 31 * result + (F23?.GetHashCode() ?? 0);
        result = 31 * result + (F24?.GetHashCode() ?? 0);
        return result;
    }

    /// <summary>Shallow tuple copy.</summary>
    /// <returns>A new Tuple with the same fields as this.</returns>
    public override Tuple25<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> Copy() => new(F0, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, F17, F18, F19, F20, F21, F22, F23, F24);
}

/// <summary>
/// Factory for <see cref="Tuple25{T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24}"/> allowing the compiler to infer the generic type
/// arguments implicitly. For example: <c>Tuple25.Of(...)</c> instead of
/// <c>new Tuple25&lt;...&gt;(...)</c>.
/// </summary>
[Public]
public static class Tuple25
{
    /// <summary>
    /// Creates a new tuple and assigns the given values to the tuple's fields. This is more
    /// convenient than using the constructor, because the compiler can infer the generic type
    /// arguments implicitly.
    /// </summary>
    public static Tuple25<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> Of<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24>(T0? f0, T1? f1, T2? f2, T3? f3, T4? f4, T5? f5, T6? f6, T7? f7, T8? f8, T9? f9, T10? f10, T11? f11, T12? f12, T13? f13, T14? f14, T15? f15, T16? f16, T17? f17, T18? f18, T19? f19, T20? f20, T21? f21, T22? f22, T23? f23, T24? f24) => new(f0, f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14, f15, f16, f17, f18, f19, f20, f21, f22, f23, f24);
}
