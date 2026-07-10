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
/// (the port of Java's <c>DataInputStream</c>-based wrapper).
/// </summary>
[PublicEvolving]
public class DataInputViewStreamWrapper : IDataInputView
{
    private readonly Stream _in;

    public DataInputViewStreamWrapper(Stream inStream)
    {
        ArgumentNullException.ThrowIfNull(inStream);
        _in = inStream;
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
        _in.ReadExactly(b, off, len);
    }

    public int SkipBytes(int n)
    {
        int total = 0;
        while (total < n)
        {
            int b = _in.ReadByte();
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
        int b = _in.ReadByte();
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
        _in.ReadExactly(buf);
        return BinaryPrimitives.ReadInt32BigEndian(buf);
    }

    public long ReadLong()
    {
        Span<byte> buf = stackalloc byte[8];
        _in.ReadExactly(buf);
        return BinaryPrimitives.ReadInt64BigEndian(buf);
    }

    public float ReadFloat() => BitConverter.Int32BitsToSingle(ReadInt());

    public double ReadDouble() => BitConverter.Int64BitsToDouble(ReadLong());

    public string? ReadLine()
    {
        int b = _in.ReadByte();
        if (b < 0)
        {
            return null;
        }
        var bld = new StringBuilder();
        while (b >= 0 && b != '\n')
        {
            bld.Append((char)b);
            b = _in.ReadByte();
        }
        if (bld.Length > 0 && bld[^1] == '\r')
        {
            bld.Length -= 1;
        }
        return bld.ToString();
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

    public int Read(byte[] b, int off, int len) =>
        _in.Read(b, off, len) is var read && read == 0 && len > 0 ? -1 : read;

    public int Read(byte[] b) => Read(b, 0, b.Length);
}
