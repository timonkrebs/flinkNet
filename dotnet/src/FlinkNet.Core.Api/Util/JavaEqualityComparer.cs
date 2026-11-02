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
/// Key equality with Java's <c>equals</c> semantics, for the dictionaries that stand in for
/// Java's <c>HashMap</c>.
///
/// <para>PORT NOTE: .NET treats -0.0 and 0.0 as equal keys, while Java's <c>Double</c> and
/// <c>Float</c> keys are distinct, so a dictionary with the default comparer would merge entries
/// that a Java map keeps apart. For <c>double</c>, <c>float</c> (also nullable) and
/// <c>object</c> keys this comparer uses <see cref="JavaPrimitives.ObjectEquals"/>; for every
/// other type it is <see cref="EqualityComparer{T}.Default"/>, whose equality already agrees with
/// Java's.</para>
/// </summary>
/// <typeparam name="T">The key type.</typeparam>
[Internal]
public static class JavaEqualityComparer<T>
{
    /// <summary>The comparer for keys of type <typeparamref name="T"/>.</summary>
    public static IEqualityComparer<T> Default { get; } = Create();

    private static IEqualityComparer<T> Create()
    {
        Type type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        if (type == typeof(double))
        {
            return (IEqualityComparer<T>)(object)(typeof(T) == typeof(double)
                ? new DoubleComparer()
                : new BoxedComparer<double?>());
        }
        if (type == typeof(float))
        {
            return (IEqualityComparer<T>)(object)(typeof(T) == typeof(float)
                ? new FloatComparer()
                : new BoxedComparer<float?>());
        }
        if (type == typeof(object))
        {
            return (IEqualityComparer<T>)(object)new BoxedComparer<object>();
        }
        return EqualityComparer<T>.Default;
    }

    private sealed class DoubleComparer : IEqualityComparer<double>
    {
        public bool Equals(double x, double y) =>
            JavaPrimitives.DoubleToLongBits(x) == JavaPrimitives.DoubleToLongBits(y);

        public int GetHashCode(double obj) => JavaPrimitives.HashCode(obj);
    }

    private sealed class FloatComparer : IEqualityComparer<float>
    {
        public bool Equals(float x, float y) =>
            JavaPrimitives.FloatToIntBits(x) == JavaPrimitives.FloatToIntBits(y);

        public int GetHashCode(float obj) => JavaPrimitives.HashCode(obj);
    }

    private sealed class BoxedComparer<TBoxed> : IEqualityComparer<TBoxed>
    {
        public bool Equals(TBoxed? x, TBoxed? y) => JavaPrimitives.ObjectEquals(x, y);

        public int GetHashCode(TBoxed obj) => JavaPrimitives.ObjectHashCode(obj);
    }
}
