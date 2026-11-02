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
using FlinkNet.Core.Memory;

namespace FlinkNet.Api.Common.TypeUtils;

/// <summary>
/// Non-generic base of <see cref="TypeComparator{T}"/>.
///
/// <para>PORT NOTE: stands in for Java's raw <c>TypeComparator</c>, which
/// <see cref="GetFlatComparators"/> returns for comparators of heterogeneous types.</para>
/// </summary>
[PublicEvolving]
public abstract class TypeComparator
{
    private protected TypeComparator()
    {
    }

    /// <summary>
    /// Extracts the key fields from a record. This is for use by the pair comparator: it
    /// extracts the keys from the given record into the target array, starting at the given
    /// index, and returns the number of keys extracted.
    /// </summary>
    /// <param name="record">The record that contains the key(s).</param>
    /// <param name="target">The array to write the key(s) into.</param>
    /// <param name="index">The offset of the target array to start writing into.</param>
    /// <returns>the number of keys added to target.</returns>
    public abstract int ExtractKeys(object record, object?[] target, int index);

    /// <summary>
    /// Get the field comparators. This is used together with <see cref="ExtractKeys"/> to
    /// provide interoperability between different record types. Note, that this should return
    /// at least one comparator, even for atomic types.
    /// </summary>
    public abstract TypeComparator[] GetFlatComparators();

    /// <summary>
    /// Untyped variant of <see cref="TypeComparator{T}.Compare"/> for keys obtained through
    /// <see cref="ExtractKeys"/> and compared with <see cref="GetFlatComparators"/>, which Java
    /// does with raw-typed calls (see the class PORT NOTE).
    /// </summary>
    /// <exception cref="InvalidCastException">If a key is not of the comparator's type.</exception>
    public abstract int UntypedCompare(object first, object second);
}

/// <summary>
/// This interface describes the methods that are required for a data type to be handled by the
/// pact runtime operations: hashing, comparing records (also in serialized form), and creating
/// normalized keys, i.e. binary prefixes that compare byte-wise in the record order.
///
/// <para>Comparators are stateful (they hold a reference record) and must not be used
/// concurrently; <see cref="Duplicate"/> creates an independent instance.</para>
///
/// <para>PORT NOTE: Java's boolean getters (<c>supportsNormalizedKey()</c>,
/// <c>getNormalizeKeyLen()</c>, ...) are properties; <c>invertNormalizedKey()</c> becomes
/// <see cref="InvertNormalizedKey"/>.</para>
/// </summary>
/// <typeparam name="T">The data type that the comparator works on.</typeparam>
[PublicEvolving]
public abstract class TypeComparator<T> : TypeComparator
{
    /// <summary>
    /// Computes a hash value for the given record. The hash value should include all fields in
    /// the record relevant to the comparison, and must be the same in every process (it decides
    /// the partition of a key).
    /// </summary>
    public abstract int Hash(T record);

    /// <summary>Sets the given element as the comparison reference for future calls to
    /// <see cref="EqualToReference"/> and <see cref="CompareToReference"/>.</summary>
    public abstract void SetReference(T toCompare);

    /// <summary>Checks whether the given element is equal to the element that has been set as
    /// the comparison reference in this comparator instance.</summary>
    public abstract bool EqualToReference(T candidate);

    /// <summary>
    /// This method compares the element that has been set as reference in this type accessor,
    /// to the element set as reference in the given type accessor. Similar to comparing two
    /// elements <c>e1</c> and <c>e2</c> via <c>Compare(e1, e2)</c>, this returns
    /// <c>Compare(referenced, this reference)</c>.
    /// </summary>
    public abstract int CompareToReference(TypeComparator<T> referencedComparator);

    /// <summary>A special case the runtime uses for special "PactRecord" support.</summary>
    public virtual bool SupportsCompareAgainstReference => false;

    /// <summary>Compares two records in object form. The return value indicates the order of
    /// the two in the same way as defined by <see cref="IComparer{T}.Compare"/>.</summary>
    public abstract int Compare(T first, T second);

    public sealed override int UntypedCompare(object first, object second) =>
        Compare((T)first, (T)second);

    /// <summary>
    /// Compares two records in serialized form. The return value indicates the order of the
    /// two in the same way as defined by <see cref="IComparer{T}.Compare"/>. This method
    /// usually deserializes as little of each record as needed to establish the order.
    /// </summary>
    public abstract int CompareSerialized(IDataInputView firstSource, IDataInputView secondSource);

    // --------------------------------------------------------------------------------------------

    /// <summary>Whether the data type supports the creation of a normalized key for
    /// comparison.</summary>
    public abstract bool SupportsNormalizedKey { get; }

    /// <summary>Whether the comparator supports serializing the record in a format that replaces
    /// its keys by a normalized key.</summary>
    public abstract bool SupportsSerializationWithKeyNormalization { get; }

    /// <summary>
    /// The number of bytes that the normalized key would maximally take. A value of
    /// <see cref="int.MaxValue"/> is interpreted as infinite.
    /// </summary>
    public abstract int NormalizeKeyLen { get; }

    /// <summary>
    /// Checks, whether the given number of bytes for a normalized is only a prefix to
    /// determine the order of elements of the data type for which this comparator provides the
    /// comparison methods. For example, if the data type is ordered with respect to an integer
    /// value it contains, then this method would return true, if the number of key bytes is
    /// smaller than four.
    /// </summary>
    public abstract bool IsNormalizedKeyPrefixOnly(int keyBytes);

    /// <summary>
    /// Writes a normalized key for the given record into the target byte array, starting at the
    /// specified position and writing exactly the given number of bytes. Note that the
    /// comparison of the bytes is treating the bytes as unsigned bytes. If the meaningful part
    /// of the normalized key takes less than the given number of bytes, then it must be padded.
    /// </summary>
    public abstract void PutNormalizedKey(T record, MemorySegment target, int offset, int numBytes);

    /// <summary>Writes the record in such a fashion that all keys are normalizing and at the
    /// beginning of the serialized data.</summary>
    public abstract void WriteWithKeyNormalization(T record, IDataOutputView target);

    /// <summary>Reads the record back while de-normalizing the key fields. This must only be
    /// used when <see cref="WriteWithKeyNormalization"/> was used for serialization.</summary>
    public abstract T ReadWithKeyDenormalization(T reuse, IDataInputView source);

    /// <summary>Flag whether normalized key comparisons should be inverted key should be
    /// interpreted inverted, i.e. descending.</summary>
    public abstract bool InvertNormalizedKey { get; }

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Creates a copy of this class. The copy must be deep such that no state set in the copy
    /// affects this instance of the comparator class.
    /// </summary>
    public abstract TypeComparator<T> Duplicate();

    /// <summary>A workaround hack of the Java runtime for the comparison against extracted
    /// keys.</summary>
    public virtual int CompareAgainstReference(IComparable[] keys) =>
        throw new NotSupportedException("Workaround hack.");
}
