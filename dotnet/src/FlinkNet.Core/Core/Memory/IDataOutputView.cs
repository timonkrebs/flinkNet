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
/// This interface defines a view over some memory that can be used to sequentially write
/// contents to the memory. The view is typically backed by one or more
/// <see cref="MemorySegment"/>.
///
/// <para>PORT NOTE: Java's <c>DataOutputView</c> extends <c>java.io.DataOutput</c>, which has no
/// .NET counterpart, so the full contract is declared here. All multi-byte writes are big-endian
/// (the Java <c>DataOutput</c> wire format).</para>
/// </summary>
[Public]
public interface IDataOutputView
{
    /// <summary>Writes the low 8 bits of the given value.</summary>
    void Write(int b);

    void Write(byte[] b);

    void Write(byte[] b, int off, int len);

    void WriteBoolean(bool v);

    /// <summary>Writes the low 8 bits of the given value.</summary>
    void WriteByte(int v);

    /// <summary>Writes the low 16 bits of the given value, big-endian.</summary>
    void WriteShort(int v);

    /// <summary>Writes the low 16 bits of the given value, big-endian.</summary>
    void WriteChar(int v);

    void WriteInt(int v);

    void WriteLong(long v);

    void WriteFloat(float v);

    void WriteDouble(double v);

    /// <summary>Writes the low 8 bits of every character of the string.</summary>
    void WriteBytes(string s);

    /// <summary>Writes every character of the string as a 2-byte big-endian value.</summary>
    void WriteChars(string s);

    /// <summary>Writes a string in Java's modified UTF-8 format with a 2-byte length prefix.</summary>
    void WriteUTF(string str);

    /// <summary>
    /// Skips <paramref name="numBytes"/> bytes of memory. In contrast to skip-on-read, the
    /// skipped memory may contain arbitrary data.
    /// </summary>
    /// <param name="numBytes">The number of bytes to skip.</param>
    /// <exception cref="EndOfStreamException">Thrown, if too few bytes remained.</exception>
    void SkipBytesToWrite(int numBytes);

    /// <summary>
    /// Copies <paramref name="numBytes"/> bytes from the source to this view.
    /// </summary>
    /// <param name="source">The source to copy the bytes from.</param>
    /// <param name="numBytes">The number of bytes to copy.</param>
    void Write(IDataInputView source, int numBytes);
}
