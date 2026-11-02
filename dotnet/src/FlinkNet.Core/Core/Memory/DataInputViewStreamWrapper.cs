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
using FlinkNet.Annotations;

namespace FlinkNet.Core.Memory;

/// <summary>
/// Utility class that turns a <see cref="Stream"/> into a <see cref="IDataInputView"/>
/// (the port of Java's <c>DataInputStream</c>-based wrapper). Disposing the wrapper disposes
/// the underlying stream, like closing Java's <c>DataInputStream</c>, unless it was created with
/// <c>leaveOpen</c> (the .NET reader convention).
///
/// <para><see cref="ReadLine"/> follows Java's <c>DataInputStream.readLine</c>: a line ends at
/// LF, CR or CRLF. To recognize a lone CR it reads one byte ahead and keeps that byte for the
/// next read, as Java does by wrapping its stream in a <c>PushbackInputStream</c>.</para>
/// </summary>
[PublicEvolving]
public class DataInputViewStreamWrapper : IDataInputView, IDisposable
{
    private readonly Stream _in;

    private readonly bool _leaveOpen;

    private bool _disposed;

    // the byte read ahead after a lone '\r' by ReadLine, or -1; every read consumes it first
    private int _pushback = -1;

    public DataInputViewStreamWrapper(Stream inStream, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(inStream);
        _in = inStream;
        _leaveOpen = leaveOpen;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        if (disposing && !_leaveOpen)
        {
            _in.Dispose();
        }
    }

    public void SkipBytesToRead(int numBytes)
    {
        if (SkipBytes(numBytes) != numBytes)
        {
            throw new EndOfStreamException("Could not skip " + numBytes + " bytes.");
        }
    }

    public void ReadFully(byte[] b) => ReadFully(b, 0, b.Length);

    public void ReadFully(byte[] b, int off, int len)
    {
        ArgumentNullException.ThrowIfNull(b);
        ReadExactly(b.AsSpan(off, len));
    }

    public int SkipBytes(int n)
    {
        int total = 0;
        while (total < n)
        {
            int b = ReadRawByte();
            if (b < 0)
            {
                break;
            }
            total++;
        }
        return total;
    }

    public bool ReadBoolean() => ReadUnsignedByte() != 0;

    public byte ReadByte() => (byte)ReadUnsignedByte();

    public int ReadUnsignedByte()
    {
        int b = ReadRawByte();
        if (b < 0)
        {
            throw new EndOfStreamException();
        }
        return b;
    }

    public short ReadShort() => (short)ReadUnsignedShort();

    public int ReadUnsignedShort() => (ReadUnsignedByte() << 8) | ReadUnsignedByte();

    public char ReadChar() => (char)ReadUnsignedShort();

    public int ReadInt()
    {
        Span<byte> buf = stackalloc byte[4];
        ReadExactly(buf);
        return BinaryPrimitives.ReadInt32BigEndian(buf);
    }

    public long ReadLong()
    {
        Span<byte> buf = stackalloc byte[8];
        ReadExactly(buf);
        return BinaryPrimitives.ReadInt64BigEndian(buf);
    }

    public float ReadFloat() => BitConverter.Int32BitsToSingle(ReadInt());

    public double ReadDouble() => BitConverter.Int64BitsToDouble(ReadLong());

    public string? ReadLine()
    {
        var bld = new StringBuilder();
        int c;
        while (true)
        {
            c = ReadRawByte();
            if (c < 0 || c == '\n')
            {
                break;
            }
            if (c == '\r')
            {
                int c2 = ReadRawByte();
                if (c2 >= 0 && c2 != '\n')
                {
                    _pushback = c2;
                }
                break;
            }
            bld.Append((char)c);
        }
        return c < 0 && bld.Length == 0 ? null : bld.ToString();
    }

    public string ReadUTF()
    {
        int utflen = ReadUnsignedShort();
        byte[] data = new byte[utflen];
        ReadFully(data, 0, utflen);
        var deserializer = new DataInputDeserializer(BuildUtfRecord(utflen, data));
        return deserializer.ReadUTF();
    }

    private static byte[] BuildUtfRecord(int utflen, byte[] data)
    {
        byte[] record = new byte[utflen + 2];
        record[0] = (byte)((utflen >>> 8) & 0xFF);
        record[1] = (byte)(utflen & 0xFF);
        data.CopyTo(record, 2);
        return record;
    }

    public int Read(byte[] b, int off, int len)
    {
        ArgumentNullException.ThrowIfNull(b);
        Span<byte> target = b.AsSpan(off, len);
        if (target.IsEmpty)
        {
            return 0;
        }
        int pushedBack = 0;
        if (_pushback >= 0)
        {
            target[0] = (byte)_pushback;
            _pushback = -1;
            pushedBack = 1;
            target = target[1..];
            if (target.IsEmpty)
            {
                return 1;
            }
        }
        int read = _in.Read(target);
        return read == 0 ? (pushedBack == 0 ? -1 : pushedBack) : pushedBack + read;
    }

    public int Read(byte[] b) => Read(b, 0, b.Length);

    private int ReadRawByte()
    {
        if (_pushback < 0)
        {
            return _in.ReadByte();
        }
        int b = _pushback;
        _pushback = -1;
        return b;
    }

    private void ReadExactly(Span<byte> buffer)
    {
        if (!buffer.IsEmpty && _pushback >= 0)
        {
            buffer[0] = (byte)_pushback;
            _pushback = -1;
            buffer = buffer[1..];
        }
        _in.ReadExactly(buffer);
    }
}
