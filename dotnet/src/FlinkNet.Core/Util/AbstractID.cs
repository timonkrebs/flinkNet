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

namespace FlinkNet.Util;

/// <summary>A statistically unique identification number.</summary>
[PublicEvolving]
public class AbstractID : IComparable<AbstractID>
{
    /// <summary>The size of a long in bytes.</summary>
    private const int SizeOfLong = 8;

    /// <summary>The size of the ID in bytes.</summary>
    public const int Size = 2 * SizeOfLong;

    // ------------------------------------------------------------------------

    /// <summary>The upper part of the actual ID.</summary>
    protected readonly long UpperPartValue;

    /// <summary>The lower part of the actual ID.</summary>
    protected readonly long LowerPartValue;

    /// <summary>The memoized value returned by ToString().</summary>
    private string? _hexString;

    // --------------------------------------------------------------------------------------------

    /// <summary>Constructs a new ID with a specific bytes value.</summary>
    public AbstractID(byte[] bytes)
    {
        if (bytes == null || bytes.Length != Size)
        {
            throw new ArgumentException("Argument bytes must by an array of " + Size + " bytes");
        }

        LowerPartValue = ByteArrayToLong(bytes, 0);
        UpperPartValue = ByteArrayToLong(bytes, SizeOfLong);
    }

    /// <summary>
    /// Constructs a new abstract ID.
    /// </summary>
    /// <param name="lowerPart">the lower bytes of the ID</param>
    /// <param name="upperPart">the higher bytes of the ID</param>
    public AbstractID(long lowerPart, long upperPart)
    {
        LowerPartValue = lowerPart;
        UpperPartValue = upperPart;
    }

    /// <summary>
    /// Copy constructor: Creates a new abstract ID from the given one.
    /// </summary>
    /// <param name="id">the abstract ID to copy</param>
    public AbstractID(AbstractID id)
    {
        if (id == null)
        {
            throw new ArgumentException("Id must not be null.");
        }
        LowerPartValue = id.LowerPartValue;
        UpperPartValue = id.UpperPartValue;
    }

    /// <summary>Constructs a new random ID from a uniform distribution.</summary>
    public AbstractID()
    {
        Span<byte> randomBytes = stackalloc byte[Size];
        Random.Shared.NextBytes(randomBytes);
        LowerPartValue = BitConverter.ToInt64(randomBytes[..SizeOfLong]);
        UpperPartValue = BitConverter.ToInt64(randomBytes[SizeOfLong..]);
    }

    // --------------------------------------------------------------------------------------------

    /// <summary>Gets the lower 64 bits of the ID.</summary>
    public long LowerPart => LowerPartValue;

    /// <summary>Gets the upper 64 bits of the ID.</summary>
    public long UpperPart => UpperPartValue;

    /// <summary>Gets the bytes underlying this ID.</summary>
    public byte[] GetBytes()
    {
        byte[] bytes = new byte[Size];
        LongToByteArray(LowerPartValue, bytes, 0);
        LongToByteArray(UpperPartValue, bytes, SizeOfLong);
        return bytes;
    }

    /// <summary>Returns pure String representation of the ID in hexadecimal.</summary>
    public string ToHexString()
    {
        if (_hexString == null)
        {
            byte[] ba = new byte[Size];
            LongToByteArray(LowerPartValue, ba, 0);
            LongToByteArray(UpperPartValue, ba, SizeOfLong);

            _hexString = StringUtils.ByteToHexString(ba);
        }

        return _hexString;
    }

    // --------------------------------------------------------------------------------------------
    //  Standard Utilities
    // --------------------------------------------------------------------------------------------

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(obj, this))
        {
            return true;
        }
        if (obj is not null && obj.GetType() == GetType())
        {
            var that = (AbstractID)obj;
            return that.LowerPartValue == LowerPartValue && that.UpperPartValue == UpperPartValue;
        }
        return false;
    }

    public override int GetHashCode() =>
        ((int)LowerPartValue)
            ^ ((int)(LowerPartValue >>> 32))
            ^ ((int)UpperPartValue)
            ^ ((int)(UpperPartValue >>> 32));

    public override string ToString() => ToHexString();

    public int CompareTo(AbstractID? other)
    {
        if (other is null)
        {
            return 1;
        }
        int diff1 = UpperPartValue.CompareTo(other.UpperPartValue);
        int diff2 = LowerPartValue.CompareTo(other.LowerPartValue);
        return diff1 == 0 ? diff2 : diff1;
    }

    // --------------------------------------------------------------------------------------------
    //  Conversion Utilities
    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Converts the given byte array to a long (big-endian, like the Java implementation).
    /// </summary>
    /// <param name="ba">the byte array to be converted</param>
    /// <param name="offset">the offset indicating at which byte inside the array the conversion
    /// shall begin</param>
    /// <returns>the long variable</returns>
    private static long ByteArrayToLong(byte[] ba, int offset)
    {
        long l = 0;

        for (int i = 0; i < SizeOfLong; ++i)
        {
            l |= (long)ba[offset + SizeOfLong - 1 - i] << (i << 3);
        }

        return l;
    }

    /// <summary>
    /// Converts a long to a byte array (big-endian, like the Java implementation).
    /// </summary>
    /// <param name="l">the long variable to be converted</param>
    /// <param name="ba">the byte array to store the result</param>
    /// <param name="offset">the offset indicating at what position inside the byte array the
    /// result shall be stored</param>
    private static void LongToByteArray(long l, byte[] ba, int offset)
    {
        for (int i = 0; i < SizeOfLong; ++i)
        {
            int shift = i << 3; // i * 8
            ba[offset + SizeOfLong - 1 - i] = (byte)((l >>> shift) & 0xff);
        }
    }
}
