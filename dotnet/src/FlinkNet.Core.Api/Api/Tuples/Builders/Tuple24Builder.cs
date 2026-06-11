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

namespace FlinkNet.Api.Tuples.Builders;

/// <summary>
/// A builder class for <see cref="Tuple24{T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23}"/>.
/// </summary>
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
[Public]
public class Tuple24Builder<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23>
{
    private readonly List<Tuple24<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23>> _tuples = [];

    public Tuple24Builder<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> Add(T0? f0, T1? f1, T2? f2, T3? f3, T4? f4, T5? f5, T6? f6, T7? f7, T8? f8, T9? f9, T10? f10, T11? f11, T12? f12, T13? f13, T14? f14, T15? f15, T16? f16, T17? f17, T18? f18, T19? f19, T20? f20, T21? f21, T22? f22, T23? f23)
    {
        _tuples.Add(new Tuple24<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23>(f0, f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14, f15, f16, f17, f18, f19, f20, f21, f22, f23));
        return this;
    }

    public Tuple24<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23>[] Build() => _tuples.ToArray();
}
