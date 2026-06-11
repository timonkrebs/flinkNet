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

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FlinkNet.Annotations;

namespace FlinkNet.Core.Memory;

/// <summary>
/// This class represents a piece of memory managed by Flink. The segment may be backed by heap
/// memory (byte array) or by off-heap (native) memory.
///
/// <para>The methods for individual memory access specify the byte order in their name (or use
/// the native byte order for the unsuffixed methods, matching Java's <c>Unsafe</c>-based access).
/// All indexes are zero-based and checked against the segment size.</para>
///
/// <para>PORT NOTE: Java implements this over <c>sun.misc.Unsafe</c> with heap/direct
/// <c>ByteBuffer</c> interop. The port uses a <c>byte[]</c> or native allocation
/// (<see cref="NativeMemory"/>) addressed through <see cref="Span{T}"/>/
/// <see cref="Unsafe"/>. The <c>ByteBuffer</c>-specific members (<c>wrap</c>,
/// <c>processAsByteBuffer</c>, <c>get/put(DataInput/DataOutput)</c>, ...) are replaced by
/// <see cref="AsSpan(int, int)"/>; accesses to a freed segment throw
/// <see cref="InvalidOperationException"/>.</para>
/// </summary>
[Internal]
public sealed unsafe class MemorySegment
{
    /// <summary>The heap byte array object relative to which we access the memory, or null for
    /// off-heap segments.</summary>
    private readonly byte[]? _heapMemory;

    /// <summary>The native memory address, for off-heap segments.</summary>
    private byte* _address;

    /// <summary>The size in bytes of the memory segment.</summary>
    private readonly int _size;

    /// <summary>Optional owner of the memory segment.</summary>
    private readonly object? _owner;

    /// <summary>Whether this segment has been freed.</summary>
    private bool _freed;

    /// <summary>Whether this segment owns native memory that must be released on free.</summary>
    private readonly bool _ownsNativeMemory;

    // -------------------------------------------------------------------------------------------

    /// <summary>Creates a new memory segment that represents the memory of the byte array.</summary>
    internal MemorySegment(byte[] buffer, object? owner)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        _heapMemory = buffer;
        _size = buffer.Length;
        _owner = owner;
    }

    /// <summary>Creates a new memory segment that represents unmanaged native memory.</summary>
    internal MemorySegment(byte* address, int size, bool ownsNativeMemory, object? owner)
    {
        _address = address;
        _size = size;
        _ownsNativeMemory = ownsNativeMemory;
        _owner = owner;
    }

    // -------------------------------------------------------------------------------------------
    //  Memory Segment Operations
    // -------------------------------------------------------------------------------------------

    /// <summary>Gets the size of the memory segment, in bytes.</summary>
    public int Size => _size;

    /// <summary>Checks whether the memory segment was freed.</summary>
    public bool IsFreed => _freed;

    /// <summary>
    /// Frees this memory segment. After this operation has been called, no further operations are
    /// possible on the memory segment and will fail. The actual memory (heap or off-heap) will
    /// only be released after this memory segment object has become garbage collected (heap), or
    /// is released immediately (owned native memory).
    /// </summary>
    public void Free()
    {
        if (_freed)
        {
            throw new InvalidOperationException("MemorySegment can be freed only once!");
        }
        _freed = true;
        if (_ownsNativeMemory && _address != null)
        {
            NativeMemory.Free(_address);
            _address = null;
        }
    }

    /// <summary>Checks whether this memory segment is backed by off-heap memory.</summary>
    public bool IsOffHeap => _heapMemory is null;

    /// <summary>
    /// Returns the byte array of on-heap memory segments.
    /// </summary>
    /// <returns>underlying byte array</returns>
    /// <exception cref="InvalidOperationException">if the memory segment does not represent
    /// on-heap memory</exception>
    public byte[] GetArray()
    {
        CheckNotFreed();
        return _heapMemory ?? throw new InvalidOperationException("Memory segment does not represent heap memory");
    }

    /// <summary>
    /// Returns the native memory address of off-heap memory segments.
    /// </summary>
    /// <exception cref="InvalidOperationException">if the memory segment does not represent
    /// off-heap memory</exception>
    public nint GetAddress()
    {
        CheckNotFreed();
        if (_heapMemory is null)
        {
            return (nint)_address;
        }
        throw new InvalidOperationException("Memory segment does not represent off-heap memory");
    }

    /// <summary>Gets the owner of this memory segment, or null if there is none.</summary>
    public object? GetOwner() => _owner;

    /// <summary>
    /// Returns a <see cref="Span{T}"/> over the given sub-range of this segment. This is the
    /// .NET replacement for Java's <c>wrap(offset, length)</c> ByteBuffer view.
    /// </summary>
    public Span<byte> AsSpan(int offset, int length)
    {
        CheckNotFreed();
        if (offset < 0 || length < 0 || offset > _size - length)
        {
            throw new IndexOutOfRangeException($"offset: {offset}, length: {length}, size: {_size}");
        }
        return _heapMemory is not null
            ? _heapMemory.AsSpan(offset, length)
            : new Span<byte>(_address + offset, length);
    }

    // ------------------------------------------------------------------------
    //  Random Access get() and put() methods
    // ------------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref byte GetRef(int index, int length)
    {
        if (_freed)
        {
            throw new InvalidOperationException("MemorySegment has been freed.");
        }
        if (index < 0 || index > _size - length)
        {
            throw new IndexOutOfRangeException($"index: {index}, length: {length}, size: {_size}");
        }
        return ref _heapMemory is not null
            ? ref _heapMemory[index]
            : ref Unsafe.AsRef<byte>(_address + index);
    }

    private void CheckNotFreed()
    {
        if (_freed)
        {
            throw new InvalidOperationException("MemorySegment has been freed.");
        }
    }

    /// <summary>Reads the byte at the given position.</summary>
    public byte Get(int index) => GetRef(index, 1);

    /// <summary>Writes the given byte into this buffer at the given position.</summary>
    public void Put(int index, byte b) => GetRef(index, 1) = b;

    /// <summary>Bulk get method. Copies dst.Length memory from the specified position to the
    /// destination memory.</summary>
    public void Get(int index, byte[] dst) => Get(index, dst, 0, dst.Length);

    /// <summary>Bulk put method. Copies src.Length memory from the source memory into the memory
    /// segment beginning at the specified position.</summary>
    public void Put(int index, byte[] src) => Put(index, src, 0, src.Length);

    /// <summary>
    /// Bulk get method. Copies <paramref name="length"/> bytes from this memory segment,
    /// starting at position <paramref name="index"/> to the destination array.
    /// </summary>
    public void Get(int index, byte[] dst, int offset, int length)
    {
        if (offset < 0 || length < 0 || offset > dst.Length - length)
        {
            throw new IndexOutOfRangeException();
        }
        AsSpan(index, length).CopyTo(dst.AsSpan(offset, length));
    }

    /// <summary>
    /// Bulk put method. Copies <paramref name="length"/> bytes from the source array into this
    /// memory segment, starting at position <paramref name="index"/>.
    /// </summary>
    public void Put(int index, byte[] src, int offset, int length)
    {
        if (offset < 0 || length < 0 || offset > src.Length - length)
        {
            throw new IndexOutOfRangeException();
        }
        src.AsSpan(offset, length).CopyTo(AsSpan(index, length));
    }

    /// <summary>Reads one byte at the given position and returns its boolean representation.</summary>
    public bool GetBoolean(int index) => Get(index) != 0;

    /// <summary>Writes one byte containing the byte value into this buffer at the given position.</summary>
    public void PutBoolean(int index, bool value) => Put(index, (byte)(value ? 1 : 0));

    // -------------------------------------------------------------------------------------------
    //  Native-order accessors (matching Java's Unsafe access), plus explicit BE/LE variants
    // -------------------------------------------------------------------------------------------

    public char GetChar(int index) => (char)GetShortNative(index);

    public char GetCharLittleEndian(int index) =>
        (char)BinaryPrimitives.ReadUInt16LittleEndian(AsSpan(index, 2));

    public char GetCharBigEndian(int index) =>
        (char)BinaryPrimitives.ReadUInt16BigEndian(AsSpan(index, 2));

    public void PutChar(int index, char value) => PutShortNative(index, (short)value);

    public void PutCharLittleEndian(int index, char value) =>
        BinaryPrimitives.WriteUInt16LittleEndian(AsSpan(index, 2), value);

    public void PutCharBigEndian(int index, char value) =>
        BinaryPrimitives.WriteUInt16BigEndian(AsSpan(index, 2), value);

    public short GetShort(int index) => GetShortNative(index);

    public short GetShortLittleEndian(int index) =>
        BinaryPrimitives.ReadInt16LittleEndian(AsSpan(index, 2));

    public short GetShortBigEndian(int index) =>
        BinaryPrimitives.ReadInt16BigEndian(AsSpan(index, 2));

    public void PutShort(int index, short value) => PutShortNative(index, value);

    public void PutShortLittleEndian(int index, short value) =>
        BinaryPrimitives.WriteInt16LittleEndian(AsSpan(index, 2), value);

    public void PutShortBigEndian(int index, short value) =>
        BinaryPrimitives.WriteInt16BigEndian(AsSpan(index, 2), value);

    public int GetInt(int index) => Unsafe.ReadUnaligned<int>(ref GetRef(index, 4));

    public int GetIntLittleEndian(int index) =>
        BinaryPrimitives.ReadInt32LittleEndian(AsSpan(index, 4));

    public int GetIntBigEndian(int index) =>
        BinaryPrimitives.ReadInt32BigEndian(AsSpan(index, 4));

    public void PutInt(int index, int value) =>
        Unsafe.WriteUnaligned(ref GetRef(index, 4), value);

    public void PutIntLittleEndian(int index, int value) =>
        BinaryPrimitives.WriteInt32LittleEndian(AsSpan(index, 4), value);

    public void PutIntBigEndian(int index, int value) =>
        BinaryPrimitives.WriteInt32BigEndian(AsSpan(index, 4), value);

    public long GetLong(int index) => Unsafe.ReadUnaligned<long>(ref GetRef(index, 8));

    public long GetLongLittleEndian(int index) =>
        BinaryPrimitives.ReadInt64LittleEndian(AsSpan(index, 8));

    public long GetLongBigEndian(int index) =>
        BinaryPrimitives.ReadInt64BigEndian(AsSpan(index, 8));

    public void PutLong(int index, long value) =>
        Unsafe.WriteUnaligned(ref GetRef(index, 8), value);

    public void PutLongLittleEndian(int index, long value) =>
        BinaryPrimitives.WriteInt64LittleEndian(AsSpan(index, 8), value);

    public void PutLongBigEndian(int index, long value) =>
        BinaryPrimitives.WriteInt64BigEndian(AsSpan(index, 8), value);

    public float GetFloat(int index) => BitConverter.Int32BitsToSingle(GetInt(index));

    public float GetFloatLittleEndian(int index) =>
        BitConverter.Int32BitsToSingle(GetIntLittleEndian(index));

    public float GetFloatBigEndian(int index) =>
        BitConverter.Int32BitsToSingle(GetIntBigEndian(index));

    public void PutFloat(int index, float value) =>
        PutInt(index, BitConverter.SingleToInt32Bits(value));

    public void PutFloatLittleEndian(int index, float value) =>
        PutIntLittleEndian(index, BitConverter.SingleToInt32Bits(value));

    public void PutFloatBigEndian(int index, float value) =>
        PutIntBigEndian(index, BitConverter.SingleToInt32Bits(value));

    public double GetDouble(int index) => BitConverter.Int64BitsToDouble(GetLong(index));

    public double GetDoubleLittleEndian(int index) =>
        BitConverter.Int64BitsToDouble(GetLongLittleEndian(index));

    public double GetDoubleBigEndian(int index) =>
        BitConverter.Int64BitsToDouble(GetLongBigEndian(index));

    public void PutDouble(int index, double value) =>
        PutLong(index, BitConverter.DoubleToInt64Bits(value));

    public void PutDoubleLittleEndian(int index, double value) =>
        PutLongLittleEndian(index, BitConverter.DoubleToInt64Bits(value));

    public void PutDoubleBigEndian(int index, double value) =>
        PutLongBigEndian(index, BitConverter.DoubleToInt64Bits(value));

    private short GetShortNative(int index) => Unsafe.ReadUnaligned<short>(ref GetRef(index, 2));

    private void PutShortNative(int index, short value) =>
        Unsafe.WriteUnaligned(ref GetRef(index, 2), value);

    // -------------------------------------------------------------------------------------------
    //  Bulk Read and Write Methods
    // -------------------------------------------------------------------------------------------

    /// <summary>
    /// Bulk copy method. Copies <paramref name="numBytes"/> bytes from this memory segment,
    /// starting at position <paramref name="offset"/> to the target memory segment.
    /// </summary>
    public void CopyTo(int offset, MemorySegment target, int targetOffset, int numBytes) =>
        AsSpan(offset, numBytes).CopyTo(target.AsSpan(targetOffset, numBytes));

    /// <summary>
    /// Compares two memory segment regions byte-wise (unsigned).
    /// </summary>
    /// <returns>0 if equal, -1 if seg1 &lt; seg2, 1 otherwise</returns>
    public int Compare(MemorySegment seg2, int offset1, int offset2, int len) =>
        Math.Sign(AsSpan(offset1, len).SequenceCompareTo(seg2.AsSpan(offset2, len)));

    /// <summary>
    /// Compares two memory segment regions with different lengths.
    /// </summary>
    /// <returns>0 if equal, -1 if seg1 &lt; seg2, 1 otherwise</returns>
    public int Compare(MemorySegment seg2, int offset1, int offset2, int len1, int len2)
    {
        int minLength = Math.Min(len1, len2);
        int cmp = Compare(seg2, offset1, offset2, minLength);
        return cmp == 0 ? len1 - len2 : cmp;
    }

    /// <summary>
    /// Swaps bytes between two memory segments, using the given auxiliary buffer.
    /// </summary>
    public void SwapBytes(
        byte[] tempBuffer, MemorySegment seg2, int offset1, int offset2, int len)
    {
        if ((uint)len > (uint)tempBuffer.Length)
        {
            throw new IndexOutOfRangeException(
                $"swap length {len} exceeds temp buffer length {tempBuffer.Length}");
        }
        Span<byte> thisSpan = AsSpan(offset1, len);
        Span<byte> otherSpan = seg2.AsSpan(offset2, len);
        thisSpan.CopyTo(tempBuffer.AsSpan(0, len));
        otherSpan.CopyTo(thisSpan);
        tempBuffer.AsSpan(0, len).CopyTo(otherSpan);
    }

    /// <summary>
    /// Equals two memory segment regions.
    /// </summary>
    /// <returns>true if equal, false otherwise</returns>
    public bool EqualTo(MemorySegment seg2, int offset1, int offset2, int length) =>
        AsSpan(offset1, length).SequenceEqual(seg2.AsSpan(offset2, length));

    /// <summary>
    /// Gets the heap byte array object, or null if the segment is off-heap.
    /// </summary>
    public byte[]? GetHeapMemory() => _heapMemory;
}
