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
using System.Globalization;

namespace FlinkNet.Core.Memory;

/// <summary>
/// A simple and efficient serializer, growing a byte buffer and writing in the Java
/// <c>DataOutput</c> big-endian wire format.
/// </summary>
public class DataOutputSerializer : IDataOutputView, IMemorySegmentWritable
{
    private byte[] _buffer;

    private int _position;

    // ------------------------------------------------------------------------

    public DataOutputSerializer(int startSize)
    {
        if (startSize < 1)
        {
            throw new ArgumentException(null, nameof(startSize));
        }

        _buffer = new byte[startSize];
    }

    /// <summary>
    /// Returns the written bytes as a read-only memory over the shared buffer (the .NET
    /// replacement for Java's <c>wrapAsByteBuffer</c>).
    /// </summary>
    public ReadOnlyMemory<byte> WrapAsMemory() => new(_buffer, 0, _position);

    /// <summary>
    /// Gets a reference to the internal byte buffer. This buffer may be larger than the actual
    /// serialized data. Only the bytes from zero to <see cref="Length"/> are valid.
    /// </summary>
    public byte[] GetSharedBuffer() => _buffer;

    /// <summary>Gets a copy of the buffer that has the right length for the data serialized so far.</summary>
    public byte[] GetCopyOfBuffer() => _buffer.AsSpan(0, _position).ToArray();

    public void Clear() => _position = 0;

    public int Length => _position;

    public override string ToString() =>
        string.Format(CultureInfo.InvariantCulture, "[pos={0} cap={1}]", _position, _buffer.Length);

    // ----------------------------------------------------------------------------------------
    //                               Data Output
    // ----------------------------------------------------------------------------------------

    public void Write(int b)
    {
        if (_position >= _buffer.Length)
        {
            Resize(1);
        }
        _buffer[_position++] = (byte)(b & 0xff);
    }

    public void Write(byte[] b) => Write(b, 0, b.Length);

    public void Write(byte[] b, int off, int len)
    {
        if (len < 0 || off > b.Length - len)
        {
            throw new IndexOutOfRangeException();
        }
        if (_position > _buffer.Length - len)
        {
            Resize(len);
        }
        b.AsSpan(off, len).CopyTo(_buffer.AsSpan(_position));
        _position += len;
    }

    public void Write(MemorySegment segment, int off, int len)
    {
        if (len < 0 || off < 0 || off > segment.Size - len)
        {
            throw new IndexOutOfRangeException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "offset: {0}, length: {1}, size: {2}",
                    off,
                    len,
                    segment.Size));
        }
        if (_position > _buffer.Length - len)
        {
            Resize(len);
        }
        segment.Get(off, _buffer, _position, len);
        _position += len;
    }

    public void WriteBoolean(bool v) => Write(v ? 1 : 0);

    public void WriteByte(int v) => Write(v);

    /// <summary>
    /// Writes the low 8 bits of every character.
    ///
    /// <para>PORT NOTE: the Java implementation advances the position twice (an upstream bug that
    /// leaves a gap of garbage bytes); the port writes the expected, contiguous bytes.</para>
    /// </summary>
    public void WriteBytes(string s)
    {
        foreach (char c in s)
        {
            WriteByte(c);
        }
    }

    public void WriteChar(int v)
    {
        if (_position >= _buffer.Length - 1)
        {
            Resize(2);
        }
        _buffer[_position++] = (byte)(v >> 8);
        _buffer[_position++] = (byte)v;
    }

    public void WriteChars(string s)
    {
        int sLen = s.Length;
        if (_position >= _buffer.Length - 2 * sLen)
        {
            Resize(2 * sLen);
        }
        foreach (char c in s)
        {
            WriteChar(c);
        }
    }

    public void WriteDouble(double v) => WriteLong(BitConverter.DoubleToInt64Bits(v));

    public void WriteFloat(float v) => WriteInt(BitConverter.SingleToInt32Bits(v));

    public void WriteInt(int v)
    {
        if (_position >= _buffer.Length - 3)
        {
            Resize(4);
        }
        BinaryPrimitives.WriteInt32BigEndian(_buffer.AsSpan(_position), v);
        _position += 4;
    }

    /// <summary>Writes the given int at the given position without moving the position (used for
    /// back-patching length fields).</summary>
    public void WriteIntUnsafe(int v, int pos) =>
        BinaryPrimitives.WriteInt32BigEndian(_buffer.AsSpan(pos), v);

    public void WriteLong(long v)
    {
        if (_position >= _buffer.Length - 7)
        {
            Resize(8);
        }
        BinaryPrimitives.WriteInt64BigEndian(_buffer.AsSpan(_position), v);
        _position += 8;
    }

    public void WriteShort(int v)
    {
        if (_position >= _buffer.Length - 1)
        {
            Resize(2);
        }
        _buffer[_position++] = (byte)((v >>> 8) & 0xff);
        _buffer[_position++] = (byte)(v & 0xff);
    }

    public void WriteUTF(string str)
    {
        int strlen = str.Length;
        int utflen = 0;

        foreach (char c in str)
        {
            utflen += GetUtfBytesSize(c);
        }

        if (utflen > 65535)
        {
            throw new IOException("Encoded string is too long: " + utflen);
        }
        else if (_position > _buffer.Length - utflen - 2)
        {
            Resize(utflen + 2);
        }

        _buffer[_position++] = (byte)((utflen >>> 8) & 0xFF);
        _buffer[_position++] = (byte)(utflen & 0xFF);

        WriteUtfBytes(str, strlen);
    }

    /// <summary>Like <see cref="WriteUTF"/> but with a 4-byte length prefix for long strings.</summary>
    public void WriteLongUTF(string str)
    {
        int strlen = str.Length;
        long utflen = 0;

        foreach (char c in str)
        {
            utflen += GetUtfBytesSize(c);

            if (utflen > int.MaxValue)
            {
                throw new IOException("Encoded string reached maximum length: " + utflen);
            }
        }

        if (utflen > int.MaxValue - 4)
        {
            throw new IOException("Encoded string is too long: " + utflen);
        }

        WriteInt((int)utflen);

        // PORT NOTE: Java reserves utflen+2 here but needs 4+utflen, which can overrun the
        // buffer; the port sizes the reservation correctly after the length prefix.
        if (_position > _buffer.Length - (int)utflen)
        {
            Resize((int)utflen);
        }

        WriteUtfBytes(str, strlen);
    }

    private void WriteUtfBytes(string str, int strlen)
    {
        byte[] bytearr = _buffer;
        int count = _position;

        int i;
        for (i = 0; i < strlen; i++)
        {
            char c = str[i];
            if (!(c >= 0x0001 && c <= 0x007F))
            {
                break;
            }
            bytearr[count++] = (byte)c;
        }

        for (; i < strlen; i++)
        {
            char c = str[i];
            if (c >= 0x0001 && c <= 0x007F)
            {
                bytearr[count++] = (byte)c;
            }
            else if (c > 0x07FF)
            {
                bytearr[count++] = (byte)(0xE0 | ((c >> 12) & 0x0F));
                bytearr[count++] = (byte)(0x80 | ((c >> 6) & 0x3F));
                bytearr[count++] = (byte)(0x80 | (c & 0x3F));
            }
            else
            {
                bytearr[count++] = (byte)(0xC0 | ((c >> 6) & 0x1F));
                bytearr[count++] = (byte)(0x80 | (c & 0x3F));
            }
        }

        _position = count;
    }

    private static int GetUtfBytesSize(int c)
    {
        if (c >= 0x0001 && c <= 0x007F)
        {
            return 1;
        }
        else if (c > 0x07FF)
        {
            return 3;
        }
        else
        {
            return 2;
        }
    }

    private void Resize(int minCapacityAdd)
    {
        long desired = Math.Max((long)_buffer.Length * 2, (long)_buffer.Length + minCapacityAdd);
        if (desired > int.MaxValue)
        {
            desired = (long)_buffer.Length + minCapacityAdd;
            if (desired > int.MaxValue)
            {
                throw new IOException(
                    "Serialization failed because the record length would exceed 2GB (max addressable array size).");
            }
        }

        byte[] nb;
        try
        {
            nb = new byte[(int)desired];
        }
        catch (OutOfMemoryException e)
        {
            // this was too large to allocate, try the smaller size (if possible)
            long fallback = (long)_buffer.Length + minCapacityAdd;
            if (desired > fallback && fallback <= int.MaxValue)
            {
                try
                {
                    nb = new byte[(int)fallback];
                }
                catch (OutOfMemoryException ee)
                {
                    throw new IOException(
                        "Failed to serialize element. Serialized size (> "
                            + fallback
                            + " bytes) exceeds heap space",
                        ee);
                }
            }
            else
            {
                throw new IOException(
                    "Failed to serialize element. Serialized size (> "
                        + desired
                        + " bytes) exceeds heap space",
                    e);
            }
        }

        _buffer.AsSpan(0, _position).CopyTo(nb);
        _buffer = nb;
    }

    public void SkipBytesToWrite(int numBytes)
    {
        if (_buffer.Length - _position < numBytes)
        {
            throw new EndOfStreamException("Could not skip " + numBytes + " bytes.");
        }

        _position += numBytes;
    }

    public void Write(IDataInputView source, int numBytes)
    {
        if (_buffer.Length - _position < numBytes)
        {
            throw new EndOfStreamException(
                "Could not write " + numBytes + " bytes. Buffer overflow.");
        }

        source.ReadFully(_buffer, _position, numBytes);
        _position += numBytes;
    }

    public void SetPosition(int position)
    {
        if (position < 0 || position > _position)
        {
            throw new ArgumentException("Position out of bounds.");
        }
        _position = position;
    }

    public void SetPositionUnsafe(int position) => _position = position;
}
