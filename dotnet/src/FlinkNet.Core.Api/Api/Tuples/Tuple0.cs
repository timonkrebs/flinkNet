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
/// A tuple with 0 fields.
///
/// <para>The Tuple0 is a soft singleton, i.e., there is a "singleton" instance, but it does not
/// prevent creation of additional instances.</para>
/// </summary>
/// <seealso cref="Tuple"/>
[Public]
public class Tuple0 : Tuple
{
    /// <summary>An immutable reusable Tuple0 instance.</summary>
    public static readonly Tuple0 Instance = new();

    // ------------------------------------------------------------------------

    public override int Arity => 0;

    public override T? GetField<T>(int pos) where T : default =>
        throw new IndexOutOfRangeException(pos.ToString());

    public override void SetField<T>(T? value, int pos) where T : default =>
        throw new IndexOutOfRangeException(pos.ToString());

    /// <summary>Shallow tuple copy.</summary>
    /// <returns>A new Tuple with the same fields as this.</returns>
    public override Tuple0 Copy() => new();

    // -------------------------------------------------------------------------------------------------
    // standard utilities
    // -------------------------------------------------------------------------------------------------

    /// <summary>Creates a string representation of the tuple in the form "()".</summary>
    public override string ToString() => "()";

    /// <summary>Deep equality for tuples by calling Equals() on the tuple members.</summary>
    /// <param name="obj">the object checked for equality</param>
    /// <returns>true if this is equal to <paramref name="obj"/>.</returns>
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Tuple0;

    public override int GetHashCode() => 0;
}
