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

namespace FlinkNet.Core.Memory;

/// <summary>
/// This interface defines a view over some memory that can be used to sequentially read the
/// contents of the memory. The view is typically backed by one or more
/// <see cref="MemorySegment"/>.
///
/// <para>PORT NOTE: Java's <c>DataInputView</c> extends <c>java.io.DataInput</c>, which has no
/// .NET counterpart, so the full contract is declared here. All multi-byte reads are big-endian
/// (the Java <c>DataInput</c> wire format). <c>ReadByte</c> returns the unsigned .NET
/// <c>byte</c>; <c>ReadUnsignedByte</c> is kept for API parity.</para>
/// </summary>
[Public]
public interface IDataInputView
{
    /// <summary>
    /// Reads <c>b.Length</c> bytes into the given array, failing with
    /// <see cref="EndOfStreamException"/> if not enough data is available.
    /// </summary>
    void ReadFully(byte[] b);

    /// <summary>
    /// Reads <paramref name="len"/> bytes into <paramref name="b"/> starting at
    /// <paramref name="off"/>, failing with <see cref="EndOfStreamException"/> if not enough data
    /// is available.
    /// </summary>
    void ReadFully(byte[] b, int off, int len);

    /// <summary>Skips up to <paramref name="n"/> bytes and returns the number actually skipped.</summary>
    int SkipBytes(int n);

    bool ReadBoolean();

    byte ReadByte();

    /// <summary>Reads one byte and returns it as an int in the range 0..255.</summary>
    int ReadUnsignedByte();

    short ReadShort();

    /// <summary>Reads two bytes and returns them as an int in the range 0..65535.</summary>
    int ReadUnsignedShort();

    char ReadChar();

    int ReadInt();

    long ReadLong();

    float ReadFloat();

    double ReadDouble();

    /// <summary>
    /// Reads the next line of text (terminated by '\n', with a trailing '\r' trimmed), or null if
    /// no data is available.
    /// </summary>
    string? ReadLine();

    /// <summary>Reads a string encoded in Java's modified UTF-8 format with a 2-byte length prefix.</summary>
    string ReadUTF();

    /// <summary>
    /// Skips <paramref name="numBytes"/> bytes of memory. In contrast to
    /// <see cref="SkipBytes(int)"/>, this method always skips the desired number of bytes or
    /// throws an exception.
    /// </summary>
    /// <param name="numBytes">The number of bytes to skip.</param>
    /// <exception cref="EndOfStreamException">Thrown, if too few bytes remained.</exception>
    void SkipBytesToRead(int numBytes);

    /// <summary>
    /// Reads up to <paramref name="len"/> bytes of memory and stores it into <paramref name="b"/>
    /// starting at offset <paramref name="off"/>. It returns the number of read bytes or -1 if
    /// there is no more data left.
    /// </summary>
    /// <param name="b">byte array to store the data to</param>
    /// <param name="off">offset into byte array</param>
    /// <param name="len">byte length to read</param>
    /// <returns>the number of actually read bytes of -1 if there is no more data left</returns>
    int Read(byte[] b, int off, int len);

    /// <summary>
    /// Tries to fill the given byte array <paramref name="b"/>. Returns the actually number of
    /// read bytes or -1 if there is no more data.
    /// </summary>
    /// <param name="b">byte array to store the data to</param>
    /// <returns>the number of read bytes or -1 if there is no more data left</returns>
    int Read(byte[] b);
}
