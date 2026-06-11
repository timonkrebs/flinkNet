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
using FlinkNet.Annotations;

namespace FlinkNet.Core.Memory;

/// <summary>
/// Utility class that turns a <see cref="Stream"/> into a <see cref="IDataOutputView"/>
/// (the port of Java's <c>DataOutputStream</c>-based wrapper).
/// </summary>
[PublicEvolving]
public class DataOutputViewStreamWrapper : IDataOutputView
{
    private readonly Stream _out;

    private byte[]? _tempBuffer;

    public DataOutputViewStreamWrapper(Stream outStream)
    {
        ArgumentNullException.ThrowIfNull(outStream);
        _out = outStream;
    }

    public void SkipBytesToWrite(int numBytes)
    {
        _tempBuffer ??= new byte[4096];

        while (numBytes > 0)
        {
            int toWrite = Math.Min(numBytes, _tempBuffer.Length);
            Write(_tempBuffer, 0, toWrite);
            numBytes -= toWrite;
        }
    }

    public void Write(IDataInputView source, int numBytes)
    {
        _tempBuffer ??= new byte[4096];

        while (numBytes > 0)
        {
            int toCopy = Math.Min(numBytes, _tempBuffer.Length);
            source.ReadFully(_tempBuffer, 0, toCopy);
            Write(_tempBuffer, 0, toCopy);
            numBytes -= toCopy;
        }
    }

    public void Write(int b) => _out.WriteByte((byte)(b & 0xff));

    public void Write(byte[] b) => _out.Write(b, 0, b.Length);

    public void Write(byte[] b, int off, int len) => _out.Write(b, off, len);

    public void WriteBoolean(bool v) => Write(v ? 1 : 0);

    public void WriteByte(int v) => Write(v);

    public void WriteShort(int v)
    {
        Write((v >>> 8) & 0xff);
        Write(v & 0xff);
    }

    public void WriteChar(int v) => WriteShort(v);

    public void WriteInt(int v)
    {
        Span<byte> buf = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(buf, v);
        _out.Write(buf);
    }

    public void WriteLong(long v)
    {
        Span<byte> buf = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(buf, v);
        _out.Write(buf);
    }

    public void WriteFloat(float v) => WriteInt(BitConverter.SingleToInt32Bits(v));

    public void WriteDouble(double v) => WriteLong(BitConverter.DoubleToInt64Bits(v));

    public void WriteBytes(string s)
    {
        foreach (char c in s)
        {
            Write(c);
        }
    }

    public void WriteChars(string s)
    {
        foreach (char c in s)
        {
            WriteChar(c);
        }
    }

    public void WriteUTF(string str)
    {
        var serializer = new DataOutputSerializer(Math.Max(16, str.Length + 2));
        serializer.WriteUTF(str);
        Write(serializer.GetSharedBuffer(), 0, serializer.Length);
    }
}
