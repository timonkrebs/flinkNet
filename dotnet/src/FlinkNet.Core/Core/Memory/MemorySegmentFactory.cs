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

using System.Runtime.InteropServices;
using FlinkNet.Annotations;

namespace FlinkNet.Core.Memory;

/// <summary>
/// A factory for (hybrid) memory segments. Use this factory in all cases where memory segments
/// need to be created, to make sure the code remains uniform.
///
/// <para>PORT NOTE: Java distinguishes direct-ByteBuffer off-heap memory from "unsafe" native
/// memory; in .NET both map to <see cref="NativeMemory"/> allocations, so
/// <c>allocateUnpooledOffHeapMemory</c> and <c>allocateOffHeapUnsafeMemory</c> collapse into
/// <see cref="AllocateUnpooledOffHeapMemory(int, object?)"/>. Off-heap segments release their
/// native memory in <see cref="MemorySegment.Free"/>.</para>
/// </summary>
[Internal]
public static class MemorySegmentFactory
{
    /// <summary>
    /// Creates a new memory segment that targets the given heap memory region.
    ///
    /// <para>This method should be used to turn short lived byte arrays into memory segments.</para>
    /// </summary>
    /// <param name="buffer">The heap memory region.</param>
    /// <returns>A new memory segment that targets the given heap memory region.</returns>
    public static MemorySegment Wrap(byte[] buffer) => new(buffer, null);

    /// <summary>
    /// Copies the given heap memory region and creates a new memory segment wrapping it.
    /// </summary>
    /// <param name="bytes">The heap memory region.</param>
    /// <param name="start">starting position, inclusive</param>
    /// <param name="end">end position, exclusive</param>
    /// <returns>A new memory segment that targets a copy of the given heap memory region.</returns>
    public static MemorySegment WrapCopy(byte[] bytes, int start, int end)
    {
        if (start < 0 || end < start || end > bytes.Length)
        {
            throw new ArgumentException($"start: {start}, end: {end}, length: {bytes.Length}");
        }
        var copy = new byte[end - start];
        bytes.AsSpan(start, copy.Length).CopyTo(copy);
        return Wrap(copy);
    }

    /// <summary>
    /// Wraps the four bytes representing the given number with a <see cref="MemorySegment"/>
    /// (big-endian, like Java's ByteBuffer default).
    /// </summary>
    /// <seealso cref="MemorySegment.PutIntBigEndian"/>
    public static MemorySegment WrapInt(int value)
    {
        var segment = AllocateUnpooledSegment(4);
        segment.PutIntBigEndian(0, value);
        return segment;
    }

    /// <summary>
    /// Allocates some unpooled memory and creates a new memory segment that represents that
    /// memory.
    ///
    /// <para>This method is similar to <see cref="AllocateUnpooledSegment(int, object?)"/>, but
    /// the memory segment will have no owner.</para>
    /// </summary>
    /// <param name="size">The size of the memory segment to allocate.</param>
    /// <returns>A new memory segment, backed by unpooled heap memory.</returns>
    public static MemorySegment AllocateUnpooledSegment(int size) =>
        AllocateUnpooledSegment(size, null);

    /// <summary>
    /// Allocates some unpooled memory and creates a new memory segment that represents that
    /// memory.
    /// </summary>
    /// <param name="size">The size of the memory segment to allocate.</param>
    /// <param name="owner">The owner to associate with the memory segment.</param>
    /// <returns>A new memory segment, backed by unpooled heap memory.</returns>
    public static MemorySegment AllocateUnpooledSegment(int size, object? owner) =>
        new(new byte[size], owner);

    /// <summary>
    /// Allocates some unpooled off-heap memory and creates a new memory segment that represents
    /// that memory.
    /// </summary>
    /// <param name="size">The size of the off-heap memory segment to allocate.</param>
    /// <returns>A new memory segment, backed by unpooled off-heap memory.</returns>
    public static MemorySegment AllocateUnpooledOffHeapMemory(int size) =>
        AllocateUnpooledOffHeapMemory(size, null);

    /// <summary>
    /// Allocates some unpooled off-heap memory and creates a new memory segment that represents
    /// that memory.
    /// </summary>
    /// <param name="size">The size of the off-heap memory segment to allocate.</param>
    /// <param name="owner">The owner to associate with the off-heap memory segment.</param>
    /// <returns>A new memory segment, backed by unpooled off-heap memory.</returns>
    public static unsafe MemorySegment AllocateUnpooledOffHeapMemory(int size, object? owner)
    {
        byte* address = (byte*)NativeMemory.AllocZeroed((nuint)size);
        return new MemorySegment(address, size, ownsNativeMemory: true, owner);
    }
}
