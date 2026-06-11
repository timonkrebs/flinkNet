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
using System.Text;

namespace FlinkNet.Core.Memory;

/// <summary>
/// A simple and efficient deserializer reading the Java <c>DataInput</c> big-endian wire format
/// from a byte array.
/// </summary>
public class DataInputDeserializer : IDataInputView
{
    private static readonly byte[] Empty = [];

    // ------------------------------------------------------------------------

    private byte[] _buffer;

    private int _end;

    private int _position;

    // ------------------------------------------------------------------------

    public DataInputDeserializer()
    {
        _buffer = Empty;
    }

    public DataInputDeserializer(byte[] buffer)
    {
        _buffer = buffer;
        SetBufferInternal(buffer, 0, buffer.Length);
    }

    public DataInputDeserializer(byte[] buffer, int start, int len)
    {
        _buffer = buffer;
        SetBuffer(buffer, start, len);
    }

    // ------------------------------------------------------------------------
    //  Changing buffers
    // ------------------------------------------------------------------------

    public void SetBuffer(byte[] buffer, int start, int len)
    {
        if (start < 0 || len < 0 || start + len > buffer.Length)
        {
            throw new ArgumentException("Invalid bounds.");
        }

        SetBufferInternal(buffer, start, len);
    }

    public void SetBuffer(byte[] buffer) => SetBufferInternal(buffer, 0, buffer.Length);

    private void SetBufferInternal(byte[] buffer, int start, int len)
    {
        _buffer = buffer;
        _position = start;
        _end = start + len;
    }

    public void ReleaseArrays() => _buffer = Empty;

    // ----------------------------------------------------------------------------------------
    //                               Data Input
    // ----------------------------------------------------------------------------------------

    public int Available => _position < _end ? _end - _position : 0;

    public int Position => _position;

    public bool ReadBoolean()
    {
        if (_position < _end)
        {
            return _buffer[_position++] != 0;
        }
        throw new EndOfStreamException();
    }

    public byte ReadByte()
    {
        if (_position < _end)
        {
            return _buffer[_position++];
        }
        throw new EndOfStreamException();
    }

    public int ReadUnsignedByte() => ReadByte();

    public char ReadChar()
    {
        if (_position < _end - 1)
        {
            return (char)(((_buffer[_position++] & 0xff) << 8) | (_buffer[_position++] & 0xff));
        }
        throw new EndOfStreamException();
    }

    public double ReadDouble() => BitConverter.Int64BitsToDouble(ReadLong());

    public float ReadFloat() => BitConverter.Int32BitsToSingle(ReadInt());

    public void ReadFully(byte[] b) => ReadFully(b, 0, b.Length);

    public void ReadFully(byte[] b, int off, int len)
    {
        if (len < 0)
        {
            throw new ArgumentException("Length may not be negative.");
        }
        if (off > b.Length - len)
        {
            throw new IndexOutOfRangeException();
        }
        if (_position > _end - len)
        {
            throw new EndOfStreamException();
        }
        _buffer.AsSpan(_position, len).CopyTo(b.AsSpan(off, len));
        _position += len;
    }

    public int ReadInt()
    {
        if (_position >= 0 && _position < _end - 3)
        {
            int value = BinaryPrimitives.ReadInt32BigEndian(_buffer.AsSpan(_position));
            _position += 4;
            return value;
        }
        throw new EndOfStreamException();
    }

    public string? ReadLine()
    {
        if (_position < _end)
        {
            // read until a newline is found
            var bld = new StringBuilder();
            char curr = (char)ReadUnsignedByte();
            while (_position < _end && curr != '\n')
            {
                bld.Append(curr);
                curr = (char)ReadUnsignedByte();
            }
            // trim a trailing carriage return
            int len = bld.Length;
            if (len > 0 && bld[len - 1] == '\r')
            {
                bld.Length = len - 1;
            }
            return bld.ToString();
        }
        return null;
    }

    public long ReadLong()
    {
        if (_position >= 0 && _position < _end - 7)
        {
            long value = BinaryPrimitives.ReadInt64BigEndian(_buffer.AsSpan(_position));
            _position += 8;
            return value;
        }
        throw new EndOfStreamException();
    }

    public short ReadShort()
    {
        if (_position >= 0 && _position < _end - 1)
        {
            return (short)(((_buffer[_position++] & 0xff) << 8) | (_buffer[_position++] & 0xff));
        }
        throw new EndOfStreamException();
    }

    public string ReadUTF() => ReadString(ReadUnsignedShort());

    /// <summary>Counterpart of <see cref="DataOutputSerializer.WriteLongUTF"/>.</summary>
    public string ReadLongUTF() => ReadString(ReadInt());

    private string ReadString(int utflen)
    {
        byte[] bytearr = new byte[utflen];
        char[] chararr = new char[utflen];

        int count = 0;
        int chararrCount = 0;

        ReadFully(bytearr, 0, utflen);

        while (count < utflen)
        {
            int c = bytearr[count] & 0xff;
            if (c > 127)
            {
                break;
            }
            count++;
            chararr[chararrCount++] = (char)c;
        }

        while (count < utflen)
        {
            int c = bytearr[count] & 0xff;
            int char2;
            int char3;
            switch (c >> 4)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    /* 0xxxxxxx */
                    count++;
                    chararr[chararrCount++] = (char)c;
                    break;
                case 12:
                case 13:
                    /* 110x xxxx 10xx xxxx */
                    count += 2;
                    if (count > utflen)
                    {
                        throw new IOException("malformed input: partial character at end");
                    }
                    char2 = bytearr[count - 1];
                    if ((char2 & 0xC0) != 0x80)
                    {
                        throw new IOException("malformed input around byte " + count);
                    }
                    chararr[chararrCount++] = (char)(((c & 0x1F) << 6) | (char2 & 0x3F));
                    break;
                case 14:
                    /* 1110 xxxx 10xx xxxx 10xx xxxx */
                    count += 3;
                    if (count > utflen)
                    {
                        throw new IOException("malformed input: partial character at end");
                    }
                    char2 = bytearr[count - 2];
                    char3 = bytearr[count - 1];
                    if ((char2 & 0xC0) != 0x80 || (char3 & 0xC0) != 0x80)
                    {
                        throw new IOException("malformed input around byte " + (count - 1));
                    }
                    chararr[chararrCount++] =
                        (char)(((c & 0x0F) << 12) | ((char2 & 0x3F) << 6) | (char3 & 0x3F));
                    break;
                default:
                    /* 10xx xxxx, 1111 xxxx */
                    throw new IOException("malformed input around byte " + count);
            }
        }
        // The number of chars produced may be less than utflen
        return new string(chararr, 0, chararrCount);
    }

    public int ReadUnsignedShort()
    {
        if (_position < _end - 1)
        {
            return ((_buffer[_position++] & 0xff) << 8) | (_buffer[_position++] & 0xff);
        }
        throw new EndOfStreamException();
    }

    public int SkipBytes(int n)
    {
        if (_position <= _end - n)
        {
            _position += n;
            return n;
        }
        n = _end - _position;
        _position = _end;
        return n;
    }

    public void SkipBytesToRead(int numBytes)
    {
        int skippedBytes = SkipBytes(numBytes);

        if (skippedBytes < numBytes)
        {
            throw new EndOfStreamException("Could not skip " + numBytes + " bytes.");
        }
    }

    public int Read(byte[] b, int off, int len)
    {
        if (b == null)
        {
            throw new ArgumentNullException(nameof(b), "Byte array b cannot be null.");
        }

        if (off < 0)
        {
            throw new ArgumentException("The offset off cannot be negative.");
        }

        if (len < 0)
        {
            throw new ArgumentException("The length len cannot be negative.");
        }

        if (len == 0)
        {
            return 0;
        }
        if (_position >= _end)
        {
            return -1;
        }

        int toRead = Math.Min(_end - _position, len);
        _buffer.AsSpan(_position, toRead).CopyTo(b.AsSpan(off, toRead));
        _position += toRead;

        return toRead;
    }

    public int Read(byte[] b) => Read(b, 0, b.Length);
}
