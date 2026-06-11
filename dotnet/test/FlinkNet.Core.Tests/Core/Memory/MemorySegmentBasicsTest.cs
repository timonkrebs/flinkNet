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

using FlinkNet.Core.Memory;
using Xunit;

namespace FlinkNet.Tests.Core.Memory;

/// <summary>
/// Tests for <see cref="MemorySegment"/> and <see cref="MemorySegmentFactory"/> over both heap
/// and off-heap segments.
///
/// <para>PORT NOTE: condensed from Java's <c>MemorySegmentTestBase</c> hierarchy.</para>
/// </summary>
public class MemorySegmentBasicsTest
{
    private const int PageSize = 128;

    public static TheoryData<bool> Segments() => new() { false, true };

    private static MemorySegment Allocate(bool offHeap) =>
        offHeap
            ? MemorySegmentFactory.AllocateUnpooledOffHeapMemory(PageSize, owner: "test")
            : MemorySegmentFactory.AllocateUnpooledSegment(PageSize, owner: "test");

    [Theory]
    [MemberData(nameof(Segments))]
    public void TestByteAccess(bool offHeap)
    {
        MemorySegment segment = Allocate(offHeap);
        try
        {
            Assert.Equal(PageSize, segment.Size);
            Assert.Equal(offHeap, segment.IsOffHeap);
            Assert.Equal("test", segment.GetOwner());

            for (int i = 0; i < PageSize; i++)
            {
                segment.Put(i, (byte)(i * 31));
            }
            for (int i = 0; i < PageSize; i++)
            {
                Assert.Equal((byte)(i * 31), segment.Get(i));
            }

            segment.PutBoolean(7, true);
            Assert.True(segment.GetBoolean(7));
            segment.PutBoolean(7, false);
            Assert.False(segment.GetBoolean(7));

            Assert.Throws<IndexOutOfRangeException>(() => segment.Get(-1));
            Assert.Throws<IndexOutOfRangeException>(() => segment.Get(PageSize));
            Assert.Throws<IndexOutOfRangeException>(() => segment.PutLong(PageSize - 7, 0L));
        }
        finally
        {
            segment.Free();
        }
        Assert.True(segment.IsFreed);
        Assert.Throws<InvalidOperationException>(() => segment.Get(0));
        Assert.Throws<InvalidOperationException>(() => segment.Free());
    }

    [Theory]
    [MemberData(nameof(Segments))]
    public void TestEndiannessOfExplicitAccessors(bool offHeap)
    {
        MemorySegment segment = Allocate(offHeap);
        try
        {
            segment.PutIntBigEndian(0, 0x0A0B0C0D);
            Assert.Equal(0x0A, segment.Get(0));
            Assert.Equal(0x0D, segment.Get(3));
            Assert.Equal(0x0A0B0C0D, segment.GetIntBigEndian(0));

            segment.PutIntLittleEndian(4, 0x0A0B0C0D);
            Assert.Equal(0x0D, segment.Get(4));
            Assert.Equal(0x0A, segment.Get(7));
            Assert.Equal(0x0A0B0C0D, segment.GetIntLittleEndian(4));

            segment.PutLongBigEndian(8, 0x0102030405060708L);
            Assert.Equal(0x01, segment.Get(8));
            Assert.Equal(0x08, segment.Get(15));
            Assert.Equal(0x0102030405060708L, segment.GetLongBigEndian(8));

            segment.PutShortBigEndian(16, 0x0102);
            Assert.Equal(0x01, segment.Get(16));
            segment.PutCharBigEndian(18, 'ሴ'); // U+1234
            Assert.Equal(0x12, segment.Get(18));
            Assert.Equal(0x34, segment.Get(19));
            Assert.Equal('ሴ', segment.GetCharBigEndian(18));

            // native order round-trips through the unsuffixed accessors
            segment.PutLong(24, long.MinValue + 12345);
            Assert.Equal(long.MinValue + 12345, segment.GetLong(24));
            segment.PutInt(32, -987654321);
            Assert.Equal(-987654321, segment.GetInt(32));
            segment.PutShort(36, short.MinValue);
            Assert.Equal(short.MinValue, segment.GetShort(36));
            segment.PutChar(38, '뻯');
            Assert.Equal('뻯', segment.GetChar(38));
            segment.PutFloat(40, 2.71828f);
            Assert.Equal(2.71828f, segment.GetFloat(40));
            segment.PutDouble(44, -3.14159265358979);
            Assert.Equal(-3.14159265358979, segment.GetDouble(44));
            segment.PutDoubleBigEndian(52, 1.0);
            Assert.Equal(1.0, segment.GetDoubleBigEndian(52));
            Assert.Equal(0x3FF0000000000000L, segment.GetLongBigEndian(52));
            segment.PutFloatLittleEndian(60, 1.0f);
            Assert.Equal(0x3F800000, segment.GetIntLittleEndian(60));
        }
        finally
        {
            segment.Free();
        }
    }

    [Theory]
    [MemberData(nameof(Segments))]
    public void TestBulkArrayAccess(bool offHeap)
    {
        MemorySegment segment = Allocate(offHeap);
        try
        {
            byte[] expected = new byte[PageSize];
            new Random(7).NextBytes(expected);
            segment.Put(0, expected);

            byte[] actual = new byte[PageSize];
            segment.Get(0, actual);
            Assert.Equal(expected, actual);

            // ranged copy
            byte[] slice = new byte[16];
            segment.Get(32, slice, 4, 8);
            Assert.Equal(expected.AsSpan(32, 8).ToArray(), slice.AsSpan(4, 8).ToArray());

            Assert.Throws<IndexOutOfRangeException>(() => segment.Get(PageSize - 4, new byte[8]));
            Assert.Throws<IndexOutOfRangeException>(() => segment.Put(0, new byte[8], 4, 8));
        }
        finally
        {
            segment.Free();
        }
    }

    [Theory]
    [MemberData(nameof(Segments))]
    public void TestCopyCompareSwapEquality(bool offHeap)
    {
        MemorySegment seg1 = Allocate(offHeap);
        // cross-type: the second segment uses the opposite backing
        MemorySegment seg2 = Allocate(!offHeap);
        try
        {
            for (int i = 0; i < PageSize; i++)
            {
                seg1.Put(i, (byte)i);
            }

            seg1.CopyTo(0, seg2, 0, PageSize);
            Assert.True(seg1.EqualTo(seg2, 0, 0, PageSize));
            Assert.Equal(0, seg1.Compare(seg2, 0, 0, PageSize));

            seg2.Put(64, 0xFF);
            Assert.False(seg1.EqualTo(seg2, 0, 0, PageSize));
            // unsigned comparison: 0xFF > 0x40
            Assert.Equal(-1, seg1.Compare(seg2, 0, 0, PageSize));
            Assert.Equal(1, seg2.Compare(seg1, 0, 0, PageSize));

            // different-length compare falls back to length difference on common prefix
            Assert.Equal(-3, seg1.Compare(seg2, 0, 0, 5, 8));

            // swap restores equality
            byte[] temp = new byte[PageSize];
            seg1.SwapBytes(temp, seg2, 64, 64, 1);
            Assert.Equal(0xFF, seg1.Get(64));
            Assert.Equal(64, seg2.Get(64));
        }
        finally
        {
            seg1.Free();
            seg2.Free();
        }
    }

    [Fact]
    public void TestFactoryWrapAndCopy()
    {
        byte[] data = [1, 2, 3, 4, 5, 6];
        MemorySegment wrapped = MemorySegmentFactory.Wrap(data);
        Assert.False(wrapped.IsOffHeap);
        Assert.Same(data, wrapped.GetArray());
        // mutations are visible through the wrap
        wrapped.Put(0, 9);
        Assert.Equal(9, data[0]);

        MemorySegment copy = MemorySegmentFactory.WrapCopy(data, 2, 6);
        Assert.Equal(4, copy.Size);
        Assert.Equal(3, copy.Get(0));
        copy.Put(0, 42);
        Assert.Equal(3, data[2]); // copy does not alias

        MemorySegment intSegment = MemorySegmentFactory.WrapInt(0x01020304);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, intSegment.GetArray());

        MemorySegment offHeap = MemorySegmentFactory.AllocateUnpooledOffHeapMemory(8);
        Assert.True(offHeap.IsOffHeap);
        Assert.NotEqual(0, (long)offHeap.GetAddress());
        Assert.Throws<InvalidOperationException>(() => offHeap.GetArray());
        Assert.Null(offHeap.GetHeapMemory());
        offHeap.Free();

        Assert.Throws<InvalidOperationException>(() => wrapped.GetAddress());
    }

    [Theory]
    [MemberData(nameof(Segments))]
    public void TestAsSpanAndSerializerInterop(bool offHeap)
    {
        MemorySegment segment = Allocate(offHeap);
        try
        {
            segment.AsSpan(0, 4).Fill(0xAB);
            Assert.Equal(0xAB, segment.Get(3));
            Assert.Throws<IndexOutOfRangeException>(() => segment.AsSpan(PageSize - 2, 4));

            // DataOutputSerializer can consume segment contents via IMemorySegmentWritable
            for (int i = 0; i < 8; i++)
            {
                segment.Put(i, (byte)(i + 1));
            }
            var output = new DataOutputSerializer(4);
            output.Write(segment, 2, 4);
            Assert.Equal(new byte[] { 3, 4, 5, 6 }, output.GetCopyOfBuffer());
        }
        finally
        {
            segment.Free();
        }
    }
}
