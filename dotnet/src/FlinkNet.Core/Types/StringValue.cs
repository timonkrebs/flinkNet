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
using FlinkNet.Core.Memory;

namespace FlinkNet.Types;

/// <summary>
/// Static helpers for Flink's variable-length string wire format: the length plus one is written
/// as a 7-bit varint (so length 0 marks a null string), followed by each char as a 1-3 byte
/// varint.
///
/// <para>PORT NOTE: in Java these are static members of the mutable <c>StringValue</c> record
/// type; the Value class itself is deferred to the types increment.</para>
/// </summary>
[Public]
public static class StringValue
{
    private const int HighBit = 0x1 << 7;

    private const int HighBit14 = 0x1 << 14;

    private const int HighBit21 = 0x1 << 21;

    private const int HighBit28 = 0x1 << 28;

    public static string? ReadString(IDataInputView input)
    {
        // the length we read is offset by one, because a length of zero indicates a null value
        int len = input.ReadUnsignedByte();

        if (len == 0)
        {
            return null;
        }

        if (len >= HighBit)
        {
            int shift = 7;
            int curr;
            len &= 0x7f;
            while ((curr = input.ReadUnsignedByte()) >= HighBit)
            {
                len |= (curr & 0x7f) << shift;
                shift += 7;
            }
            len |= curr << shift;
        }

        // subtract one for the null length
        len -= 1;

        char[] data = new char[len];

        for (int i = 0; i < len; i++)
        {
            int c = input.ReadUnsignedByte();
            if (c >= HighBit)
            {
                int shift = 7;
                int curr;
                c &= 0x7f;
                while ((curr = input.ReadUnsignedByte()) >= HighBit)
                {
                    c |= (curr & 0x7f) << shift;
                    shift += 7;
                }
                c |= curr << shift;
            }
            data[i] = (char)c;
        }

        return new string(data, 0, len);
    }

    public static void WriteString(string? cs, IDataOutputView output)
    {
        if (cs != null)
        {
            int strlen = cs.Length;

            // the length we write is offset by one, because a length of zero indicates a null
            // value
            int lenToWrite = strlen + 1;
            if (lenToWrite < 0)
            {
                throw new ArgumentException("CharSequence is too long.");
            }

            // string is prefixed by its variable length encoded size, which can take 1-5 bytes.
            if (lenToWrite < HighBit)
            {
                output.Write((byte)lenToWrite);
            }
            else if (lenToWrite < HighBit14)
            {
                output.Write(lenToWrite | HighBit);
                output.Write(lenToWrite >>> 7);
            }
            else if (lenToWrite < HighBit21)
            {
                output.Write(lenToWrite | HighBit);
                output.Write((lenToWrite >>> 7) | HighBit);
                output.Write(lenToWrite >>> 14);
            }
            else if (lenToWrite < HighBit28)
            {
                output.Write(lenToWrite | HighBit);
                output.Write((lenToWrite >>> 7) | HighBit);
                output.Write((lenToWrite >>> 14) | HighBit);
                output.Write(lenToWrite >>> 21);
            }
            else
            {
                output.Write(lenToWrite | HighBit);
                output.Write((lenToWrite >>> 7) | HighBit);
                output.Write((lenToWrite >>> 14) | HighBit);
                output.Write((lenToWrite >>> 21) | HighBit);
                output.Write(lenToWrite >>> 28);
            }

            // write the char data, variable length encoded
            for (int i = 0; i < strlen; i++)
            {
                int c = cs[i];

                if (c < HighBit)
                {
                    output.Write(c);
                }
                else if (c < HighBit14)
                {
                    output.Write(c | HighBit);
                    output.Write(c >>> 7);
                }
                else
                {
                    output.Write(c | HighBit);
                    output.Write((c >>> 7) | HighBit);
                    output.Write(c >>> 14);
                }
            }
        }
        else
        {
            output.Write(0);
        }
    }

    public static void CopyString(IDataInputView input, IDataOutputView output)
    {
        int len = input.ReadUnsignedByte();
        output.WriteByte(len);

        if (len >= HighBit)
        {
            int shift = 7;
            int curr;
            len &= 0x7f;
            while ((curr = input.ReadUnsignedByte()) >= HighBit)
            {
                output.WriteByte(curr);
                len |= (curr & 0x7f) << shift;
                shift += 7;
            }
            output.WriteByte(curr);
            len |= curr << shift;
        }

        // note that the length is one larger than the actual length (length 0 is a null string,
        // not a zero length string)
        len--;

        for (int i = 0; i < len; i++)
        {
            int c = input.ReadUnsignedByte();
            output.WriteByte(c);
            while (c >= HighBit)
            {
                c = input.ReadUnsignedByte();
                output.WriteByte(c);
            }
        }
    }
}
