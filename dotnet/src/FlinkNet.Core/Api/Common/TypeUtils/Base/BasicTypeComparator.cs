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

using System.Runtime.CompilerServices;
using FlinkNet.Annotations;
using FlinkNet.Core.Memory;

namespace FlinkNet.Api.Common.TypeUtils.Base;

/// <summary>
/// Base class for the comparators of the basic types.
///
/// <para>PORT NOTE: Java bounds <c>T</c> by <c>Comparable&lt;T&gt;</c> and uses the boxed
/// type's <c>compareTo</c>, <c>equals</c> and <c>hashCode</c>. The .NET counterparts differ
/// (culture-sensitive string comparison, a per-process randomized string hash, other
/// primitive hashes, an IEEE-style floating-point order), so subclasses supply Java's natural
/// order in <see cref="CompareValues"/> and Java's hash in <see cref="JavaHashCode"/>. Java's
/// <c>equals</c> agrees with its <c>compareTo</c> for every basic type, so
/// <see cref="EqualToReference"/> is <c>CompareValues(..) == 0</c>. The Java
/// <c>Serializable</c> marker is dropped (no Java serialization in the port).</para>
/// </summary>
/// <typeparam name="T">The basic type.</typeparam>
[Internal]
public abstract class BasicTypeComparator<T> : TypeComparator<T>
    where T : notnull
{
    private T? _reference;

    // For use by GetFlatComparators
    private readonly TypeComparator[] _comparators;

    protected BasicTypeComparator(bool ascending)
    {
        AscendingComparison = ascending;
        _comparators = [this];
    }

    /// <summary>Whether this comparator orders ascending.</summary>
    protected bool AscendingComparison { get; }

    /// <summary>Compares two non-null values in Java's natural order of the type.</summary>
    protected abstract int CompareValues(T first, T second);

    /// <summary>Java's <c>hashCode</c> of a non-null value of the type.</summary>
    protected abstract int JavaHashCode(T value);

    public override int Hash(T record)
    {
        RequireNonNull(record);
        return JavaHashCode(record);
    }

    public override void SetReference(T toCompare) => _reference = toCompare;

    public override bool EqualToReference(T candidate)
    {
        // Java: candidate.equals(reference), which fails on a null candidate and is false for a
        // null reference
        RequireNonNull(candidate);
        return _reference is not null && CompareValues(candidate, _reference) == 0;
    }

    public override int CompareToReference(TypeComparator<T> referencedComparator)
    {
        T? otherReference = ((BasicTypeComparator<T>)referencedComparator)._reference;
        int comp = CompareValues(RequireReference(otherReference), RequireReference(_reference));
        return AscendingComparison ? comp : -comp;
    }

    public override int Compare(T first, T second)
    {
        RequireNonNull(first);
        RequireNonNull(second);
        int cmp = CompareValues(first, second);
        return AscendingComparison ? cmp : -cmp;
    }

    public override bool InvertNormalizedKey => !AscendingComparison;

    public override bool SupportsSerializationWithKeyNormalization => false;

    public override void WriteWithKeyNormalization(T record, IDataOutputView target) =>
        throw new NotSupportedException();

    public override int ExtractKeys(object record, object?[] target, int index)
    {
        target[index] = record;
        return 1;
    }

    public override TypeComparator[] GetFlatComparators() => _comparators;

    public override T ReadWithKeyDenormalization(T reuse, IDataInputView source) =>
        throw new NotSupportedException();

    // A no-op for value types: the JIT folds the null test away.
    private static void RequireNonNull(
        T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    private static T RequireReference(T? reference) =>
        reference ?? throw new InvalidOperationException("No reference has been set.");
}
