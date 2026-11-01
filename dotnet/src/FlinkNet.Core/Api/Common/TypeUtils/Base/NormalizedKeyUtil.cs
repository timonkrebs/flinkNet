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

namespace FlinkNet.Api.Common.TypeUtils.Base;

/// <summary>
/// Utilities related to <see cref="TypeComparator{T}"/>: they write the normalized keys of the
/// primitive types, i.e. big-endian prefixes that compare byte-wise (as unsigned bytes) in the
/// order of the values. A key shorter than the full width is the value's leading bytes, a longer
/// one is padded with zeros, and a non-positive width writes nothing.
/// </summary>
[Internal]
public static class NormalizedKeyUtil
{
    public static void PutByteNormalizedKey(sbyte value, MemorySegment target, int offset, int numBytes)
    {
        if (numBytes <= 0)
        {
            return;
        }
        // convert to int explicitly before subtracting the min value, which shifts the signed
        // range onto the unsigned one
        int highByte = (value & 0xff) - sbyte.MinValue;
        target.Put(offset, unchecked((byte)highByte));
        for (int i = 1; i < numBytes; i++)
        {
            target.Put(offset + i, 0);
        }
    }

    public static void PutCharNormalizedKey(char value, MemorySegment target, int offset, int numBytes)
    {
        // char is unsigned, so its bytes need no transformation to compare correctly
        if (numBytes <= 0)
        {
            return;
        }
        target.Put(offset, (byte)((value >> 8) & 0xff));
        if (numBytes == 1)
        {
            return;
        }
        target.Put(offset + 1, (byte)(value & 0xff));
        for (int i = 2; i < numBytes; i++)
        {
            target.Put(offset + i, 0);
        }
    }

    public static void PutBooleanNormalizedKey(bool value, MemorySegment target, int offset, int numBytes)
    {
        if (numBytes <= 0)
        {
            return;
        }
        target.Put(offset, (byte)(value ? 1 : 0));
        for (int i = 1; i < numBytes; i++)
        {
            target.Put(offset + i, 0);
        }
    }

    public static void PutShortNormalizedKey(short value, MemorySegment target, int offset, int numBytes)
    {
        if (numBytes <= 0)
        {
            return;
        }
        int highByte = ((value >> 8) & 0xff) - sbyte.MinValue;
        target.Put(offset, unchecked((byte)highByte));
        if (numBytes == 1)
        {
            return;
        }
        target.Put(offset + 1, (byte)(value & 0xff));
        for (int i = 2; i < numBytes; i++)
        {
            target.Put(offset + i, 0);
        }
    }

    public static void PutIntNormalizedKey(int iValue, MemorySegment target, int offset, int numBytes) =>
        PutUnsignedIntegerNormalizedKey(unchecked(iValue - int.MinValue), target, offset, numBytes);

    /// <summary>Writes the normalized key of an int whose bits are to be ordered as an unsigned
    /// value.</summary>
    public static void PutUnsignedIntegerNormalizedKey(
        int value, MemorySegment target, int offset, int numBytes)
    {
        if (numBytes == 4)
        {
            // default case, full normalized key
            target.PutIntBigEndian(offset, value);
        }
        else if (numBytes > 0)
        {
            if (numBytes < 4)
            {
                for (int i = 0; numBytes > 0; numBytes--, i++)
                {
                    target.Put(offset + i, unchecked((byte)(value >>> ((3 - i) << 3))));
                }
            }
            else
            {
                target.PutIntBigEndian(offset, value);
                for (int i = 4; i < numBytes; i++)
                {
                    target.Put(offset + i, 0);
                }
            }
        }
    }

    public static void PutLongNormalizedKey(long lValue, MemorySegment target, int offset, int numBytes) =>
        PutUnsignedLongNormalizedKey(unchecked(lValue - long.MinValue), target, offset, numBytes);

    /// <summary>Writes the normalized key of a long whose bits are to be ordered as an unsigned
    /// value.</summary>
    public static void PutUnsignedLongNormalizedKey(
        long value, MemorySegment target, int offset, int numBytes)
    {
        if (numBytes == 8)
        {
            // default case, full normalized key
            target.PutLongBigEndian(offset, value);
        }
        else if (numBytes > 0)
        {
            if (numBytes < 8)
            {
                for (int i = 0; numBytes > 0; numBytes--, i++)
                {
                    target.Put(offset + i, unchecked((byte)(value >>> ((7 - i) << 3))));
                }
            }
            else
            {
                target.PutLongBigEndian(offset, value);
                for (int i = 8; i < numBytes; i++)
                {
                    target.Put(offset + i, 0);
                }
            }
        }
    }
}
