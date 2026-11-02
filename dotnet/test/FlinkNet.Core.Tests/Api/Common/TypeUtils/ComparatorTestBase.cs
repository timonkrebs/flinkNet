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

using FlinkNet.Api.Common.TypeUtils;
using FlinkNet.Core.Memory;
using Xunit;

namespace FlinkNet.Tests.Api.Common.TypeUtils;

/// <summary>
/// Abstract test base for comparators (port of Flink's <c>ComparatorTestBase</c>).
///
/// <para>PORT NOTE: Java's <c>Order</c> enum is not ported yet, so the tested orders are
/// ascending flags. <see cref="TestCompare"/> is added: the Java base only reaches
/// <c>compare</c> through key extraction. <see cref="TestNormalizedKeyReadWriter"/> reads back
/// the value it just wrote; the Java version re-reads the stream from its start on every
/// iteration (no basic comparator reaches that code).</para>
/// </summary>
public abstract class ComparatorTestBase<T>
{
    // Same as in the NormalizedKeySorter
    private const int DefaultMaxNormalizedKeyLen = 8;

    protected virtual bool[] TestedOrders => [true, false];

    protected abstract TypeComparator<T> CreateComparator(bool ascending);

    protected abstract TypeSerializer<T> CreateSerializer();

    /// <summary>
    /// Returns the sorted data set. Note: every element needs to be <em>strictly greater</em>
    /// than the previous element.
    /// </summary>
    protected abstract T[] GetSortedTestData();

    protected virtual bool SupportsNullKeys => false;

    // -------------------------------- test duplication ------------------------------------------

    [Fact]
    public void TestDuplicate()
    {
        bool ascending = TestedOrders[0];
        TypeComparator<T> comparator = GetComparator(ascending);
        TypeComparator<T> clone = comparator.Duplicate();

        T[] data = GetSortedData();
        comparator.SetReference(data[0]);
        clone.SetReference(data[1]);

        Assert.True(
            comparator.EqualToReference(data[0]) && clone.EqualToReference(data[1]),
            "Comparator duplication does not work: Altering the reference in a duplicated "
                + "comparator alters the original comparator's reference.");
    }

    // --------------------------------- equality tests -------------------------------------------

    [Fact]
    public void TestEquality()
    {
        foreach (bool ascending in TestedOrders)
        {
            TestEquals(ascending);
        }
    }

    private void TestEquals(bool ascending)
    {
        // Just setup two identical output/input views and go over their data to see if compare
        // works
        TypeComparator<T> comparator = GetComparator(ascending);
        foreach (T d in GetSortedData())
        {
            IDataInputView in1 = WriteSortedData(d);
            IDataInputView in2 = WriteSortedData(d);
            Assert.Equal(0, comparator.CompareSerialized(in1, in2));
        }
    }

    [Fact]
    public void TestEqualityWithReference()
    {
        TypeSerializer<T> serializer = GetSerializer();
        bool ascending = TestedOrders[0];
        TypeComparator<T> comparator = GetComparator(ascending);
        TypeComparator<T> comparator2 = GetComparator(ascending);
        foreach (T d in GetSortedData())
        {
            comparator.SetReference(d);
            // Make a copy to compare
            T copy = serializer.Copy(d, serializer.CreateInstance());

            // And then test equalTo and compareToReference method of comparator
            Assert.True(comparator.EqualToReference(d));
            comparator2.SetReference(copy);
            Assert.Equal(0, comparator.CompareToReference(comparator2));
        }
    }

    // --------------------------------- inequality tests ----------------------------------------

    [Fact]
    public void TestInequality()
    {
        foreach (bool ascending in TestedOrders)
        {
            TestGreatSmallAscDesc(ascending, true);
            TestGreatSmallAscDesc(ascending, false);
        }
    }

    private void TestGreatSmallAscDesc(bool ascending, bool greater)
    {
        T[] data = GetSortedData();
        TypeComparator<T> comparator = GetComparator(ascending);

        // compares every element in high with every element in low
        for (int x = 0; x < data.Length - 1; x++)
        {
            for (int y = x + 1; y < data.Length; y++)
            {
                IDataInputView in1 = WriteSortedData(data[x]);
                IDataInputView in2 = WriteSortedData(data[y]);

                if (greater && ascending)
                {
                    Assert.True(comparator.CompareSerialized(in1, in2) < 0);
                }
                if (greater && !ascending)
                {
                    Assert.True(comparator.CompareSerialized(in1, in2) > 0);
                }
                if (!greater && ascending)
                {
                    Assert.True(comparator.CompareSerialized(in2, in1) > 0);
                }
                if (!greater && !ascending)
                {
                    Assert.True(comparator.CompareSerialized(in2, in1) < 0);
                }
            }
        }
    }

    [Fact]
    public void TestInequalityWithReference()
    {
        foreach (bool ascending in TestedOrders)
        {
            TestGreatSmallAscDescWithReference(ascending, true);
            TestGreatSmallAscDescWithReference(ascending, false);
        }
    }

    private void TestGreatSmallAscDescWithReference(bool ascending, bool greater)
    {
        T[] data = GetSortedData();

        TypeComparator<T> comparatorLow = GetComparator(ascending);
        TypeComparator<T> comparatorHigh = GetComparator(ascending);

        // compares every element in high with every element in low
        for (int x = 0; x < data.Length - 1; x++)
        {
            for (int y = x + 1; y < data.Length; y++)
            {
                comparatorLow.SetReference(data[x]);
                comparatorHigh.SetReference(data[y]);

                if (greater && ascending)
                {
                    Assert.True(comparatorLow.CompareToReference(comparatorHigh) > 0);
                }
                if (greater && !ascending)
                {
                    Assert.True(comparatorLow.CompareToReference(comparatorHigh) < 0);
                }
                if (!greater && ascending)
                {
                    Assert.True(comparatorHigh.CompareToReference(comparatorLow) < 0);
                }
                if (!greater && !ascending)
                {
                    Assert.True(comparatorHigh.CompareToReference(comparatorLow) > 0);
                }
            }
        }
    }

    [Fact]
    public void TestCompare()
    {
        T[] data = GetSortedData();
        foreach (bool ascending in TestedOrders)
        {
            TypeComparator<T> comparator = GetComparator(ascending);
            for (int x = 0; x < data.Length; x++)
            {
                Assert.Equal(0, comparator.Compare(data[x], data[x]));
                for (int y = x + 1; y < data.Length; y++)
                {
                    Assert.Equal(ascending ? -1 : 1, Math.Sign(comparator.Compare(data[x], data[y])));
                    Assert.Equal(ascending ? 1 : -1, Math.Sign(comparator.Compare(data[y], data[x])));
                }
            }
        }
    }

    // --------------------------------- Normalized key tests -------------------------------------

    // Sets up a memory segment and normalizes the keys of the data array in it
    private static MemorySegment SetupNormalizedKeysMemSegment(
        T[] data, int normKeyLen, TypeComparator<T> comparator)
    {
        MemorySegment memSeg = MemorySegmentFactory.AllocateUnpooledSegment(2048);

        // Setup normalized Keys in the memory segment
        int offset = 0;
        foreach (T e in data)
        {
            comparator.PutNormalizedKey(e, memSeg, offset, normKeyLen);
            offset += normKeyLen;
        }
        return memSeg;
    }

    // Returns a normalized key length, either as done in the NormalizedKeySorter or its half
    private static int GetNormKeyLen(bool halfLength, TypeComparator<T> comparator)
    {
        // Same as in the NormalizedKeySorter
        int keyLen = Math.Min(comparator.NormalizeKeyLen, DefaultMaxNormalizedKeyLen);
        if (keyLen < comparator.NormalizeKeyLen)
        {
            Assert.True(comparator.IsNormalizedKeyPrefixOnly(keyLen));
        }

        if (halfLength)
        {
            keyLen /= 2;
            Assert.True(comparator.IsNormalizedKeyPrefixOnly(keyLen));
        }
        return keyLen;
    }

    [Fact]
    public void TestNormalizedKeysEqualsFullLength()
    {
        // Ascending or descending does not matter in this case
        if (!GetComparator(TestedOrders[0]).SupportsNormalizedKey)
        {
            return;
        }
        TestNormalizedKeysEquals(false);
    }

    [Fact]
    public void TestNormalizedKeysEqualsHalfLength()
    {
        if (!GetComparator(TestedOrders[0]).SupportsNormalizedKey)
        {
            return;
        }
        TestNormalizedKeysEquals(true);
    }

    private void TestNormalizedKeysEquals(bool halfLength)
    {
        TypeComparator<T> comparator = GetComparator(TestedOrders[0]);
        T[] data = GetSortedData();
        int normKeyLen = GetNormKeyLen(halfLength, comparator);

        MemorySegment memSeg1 = SetupNormalizedKeysMemSegment(data, normKeyLen, comparator);
        MemorySegment memSeg2 = SetupNormalizedKeysMemSegment(data, normKeyLen, comparator);

        for (int i = 0; i < data.Length; i++)
        {
            Assert.Equal(0, memSeg1.Compare(memSeg2, i * normKeyLen, i * normKeyLen, normKeyLen));
        }
    }

    [Fact]
    public void TestNormalizedKeysGreatSmallFullLength()
    {
        // ascending/descending in comparator doesn't matter for normalized keys
        TypeComparator<T> comparator = GetComparator(TestedOrders[0]);
        if (!comparator.SupportsNormalizedKey)
        {
            return;
        }
        TestNormalizedKeysGreatSmall(true, comparator, false);
        TestNormalizedKeysGreatSmall(false, comparator, false);
    }

    [Fact]
    public void TestNormalizedKeysGreatSmallAscDescHalfLength()
    {
        TypeComparator<T> comparator = GetComparator(TestedOrders[0]);
        if (!comparator.SupportsNormalizedKey)
        {
            return;
        }
        TestNormalizedKeysGreatSmall(true, comparator, true);
        TestNormalizedKeysGreatSmall(false, comparator, true);
    }

    private void TestNormalizedKeysGreatSmall(bool greater, TypeComparator<T> comparator, bool halfLength)
    {
        T[] data = GetSortedData();
        // Get the normKeyLen on which we are testing
        int normKeyLen = GetNormKeyLen(halfLength, comparator);

        // Write the data into 2 different memory segments
        MemorySegment memSegLow = SetupNormalizedKeysMemSegment(data, normKeyLen, comparator);
        MemorySegment memSegHigh = SetupNormalizedKeysMemSegment(data, normKeyLen, comparator);

        bool fullyDetermines = !comparator.IsNormalizedKeyPrefixOnly(normKeyLen);

        // Compare every element with every bigger element
        for (int l = 0; l < data.Length - 1; l++)
        {
            for (int h = l + 1; h < data.Length; h++)
            {
                if (greater)
                {
                    int cmp = memSegLow.Compare(memSegHigh, l * normKeyLen, h * normKeyLen, normKeyLen);
                    Assert.True(fullyDetermines ? cmp < 0 : cmp <= 0);
                }
                else
                {
                    int cmp = memSegHigh.Compare(memSegLow, h * normKeyLen, l * normKeyLen, normKeyLen);
                    Assert.True(fullyDetermines ? cmp > 0 : cmp >= 0);
                }
            }
        }
    }

    [Fact]
    public void TestNormalizedKeyReadWriter()
    {
        T[] data = GetSortedData();
        T reuse = GetSortedData()[0];

        TypeComparator<T> comp1 = GetComparator(TestedOrders[0]);
        if (!comp1.SupportsSerializationWithKeyNormalization)
        {
            return;
        }
        TypeComparator<T> comp2 = comp1.Duplicate();

        foreach (T value in data)
        {
            comp1.SetReference(value);
            var output = new DataOutputSerializer(64);
            comp1.WriteWithKeyNormalization(value, output);
            var input = new DataInputDeserializer(output.GetCopyOfBuffer());
            reuse = comp1.ReadWithKeyDenormalization(reuse, input);
            comp2.SetReference(reuse);

            Assert.Equal(0, comp1.CompareToReference(comp2));
        }
    }

    // -------------------------------- Key extraction tests --------------------------------------

    [Fact]
    public void TestKeyExtraction()
    {
        TypeComparator<T> comparator = GetComparator(TestedOrders[0]);

        foreach (T value in GetSortedData())
        {
            TypeComparator[] comparators = comparator.GetFlatComparators();
            object?[] extractedKeys = new object?[comparators.Length];
            int insertedKeys = comparator.ExtractKeys(value!, extractedKeys, 0);
            Assert.Equal(comparators.Length, insertedKeys);

            for (int i = 0; i < insertedKeys; i++)
            {
                // check if some keys are null, although this is not supported
                if (!SupportsNullKeys)
                {
                    Assert.NotNull(extractedKeys[i]);
                }
                // compare the extracted key with itself as a basic check if the extracted key
                // corresponds to the comparator
                Assert.Equal(0, comparators[i].UntypedCompare(extractedKeys[i]!, extractedKeys[i]!));
            }
        }
    }

    // --------------------------------------------------------------------------------------------

    protected virtual void DeepEquals(string message, T should, T actual) =>
        Assert.True(EqualityComparer<T>.Default.Equals(should, actual), message);

    private TypeComparator<T> GetComparator(bool ascending) =>
        CreateComparator(ascending)
            ?? throw new InvalidOperationException("Test case corrupt. Returns null as comparator.");

    private T[] GetSortedData()
    {
        T[] data = GetSortedTestData()
            ?? throw new InvalidOperationException("Test case corrupt. Returns null as test data.");
        if (data.Length < 2)
        {
            throw new InvalidOperationException("Test case does not provide enough sorted test data.");
        }
        return data;
    }

    private TypeSerializer<T> GetSerializer() =>
        CreateSerializer()
            ?? throw new InvalidOperationException("Test case corrupt. Returns null as serializer.");

    // Serializes the value, checks that it deserializes to itself, and returns a view over it
    private IDataInputView WriteSortedData(T value)
    {
        TypeSerializer<T> serializer = GetSerializer();

        var output = new DataOutputSerializer(64);
        serializer.Serialize(value, output);
        byte[] bytes = output.GetCopyOfBuffer();

        // These are the same tests as in the serializer: check that the data is really there
        // after serialization, before testing the comparator on it
        var check = new DataInputDeserializer(bytes);
        Assert.True(check.Available > 0, "No data available during deserialization.");
        T deserialized = serializer.Deserialize(serializer.CreateInstance(), check);
        DeepEquals("Deserialized value is wrong.", value, deserialized);

        return new DataInputDeserializer(bytes);
    }
}
