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

namespace FlinkNet.Util;

/// <summary>
/// Utility class for implementing CoGroupedStream in DataStream V1, as well as two-input window
/// operations in DataStream V2.
/// </summary>
// PORT NOTE: Like the Java original, the union discriminates its sides by null-ness. With
// value-type type arguments IsOne/IsTwo cannot distinguish an unset side from default(T); use
// reference types or nullable value types (e.g. TaggedUnion<int?, ...>) where that distinction
// matters.
[Internal]
public class TaggedUnion<T1, T2>
{
    private readonly T1? _one;
    private readonly T2? _two;

    private TaggedUnion(T1? one, T2? two)
    {
        _one = one;
        _two = two;
    }

    public bool IsOne => _one != null;

    public bool IsTwo => _two != null;

    public T1 GetOne()
    {
        return _one!;
    }

    public T2 GetTwo()
    {
        return _two!;
    }

    public static TaggedUnion<T1, T2> One(T1 one)
    {
        return new TaggedUnion<T1, T2>(one, default);
    }

    public static TaggedUnion<T1, T2> Two(T2 two)
    {
        return new TaggedUnion<T1, T2>(default, two);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not TaggedUnion<T1, T2> other)
        {
            return false;
        }

        return Equals(_one, other._one) && Equals(_two, other._two);
    }

    // PORT NOTE: The Java class overrides equals without hashCode; C# requires the pair, so the
    // matching hash code is provided here.
    public override int GetHashCode()
    {
        return HashCode.Combine(_one, _two);
    }
}
