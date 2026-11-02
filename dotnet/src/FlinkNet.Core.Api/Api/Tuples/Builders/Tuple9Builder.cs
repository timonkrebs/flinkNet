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
/// A builder class for <see cref="Tuple9{T0, T1, T2, T3, T4, T5, T6, T7, T8}"/>.
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
[Public]
public class Tuple9Builder<T0, T1, T2, T3, T4, T5, T6, T7, T8>
{
    private readonly List<Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>> _tuples = [];

    public Tuple9Builder<T0, T1, T2, T3, T4, T5, T6, T7, T8> Add(T0? f0, T1? f1, T2? f2, T3? f3, T4? f4, T5? f5, T6? f6, T7? f7, T8? f8)
    {
        _tuples.Add(new Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(f0, f1, f2, f3, f4, f5, f6, f7, f8));
        return this;
    }

    public Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>[] Build() => _tuples.ToArray();
}
