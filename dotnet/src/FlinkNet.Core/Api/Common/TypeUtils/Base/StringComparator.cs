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
using FlinkNet.Types;
using FlinkNet.Util;

namespace FlinkNet.Api.Common.TypeUtils.Base;

/// <summary>
/// Comparator for <c>string</c> values, ordered as Java's <c>String.compareTo</c>: ordinally by
/// UTF-16 code unit, never by culture. The normalized key is an order-preserving variable-length
/// encoding of the leading characters, so it is always only a prefix.
/// </summary>
[Internal]
public sealed class StringComparator : BasicTypeComparator<string>
{
    private const int HighBit = 0x1 << 7;

    private const int HighBit2 = 0x1 << 13;

    private const int HighBit2Mask = 0x3 << 6;

    public StringComparator(bool ascending)
        : base(ascending)
    {
    }

    protected override int CompareValues(string first, string second) =>
        string.CompareOrdinal(first, second);

    protected override int JavaHashCode(string value) => JavaPrimitives.HashCode(value);

    public override int CompareSerialized(IDataInputView firstSource, IDataInputView secondSource)
    {
        string s1 = ReadNonNullString(firstSource);
        string s2 = ReadNonNullString(secondSource);
        int comp = string.CompareOrdinal(s1, s2);
        return AscendingComparison ? comp : -comp;
    }

    public override bool SupportsNormalizedKey => true;

    public override bool SupportsSerializationWithKeyNormalization => false;

    public override int NormalizeKeyLen => int.MaxValue;

    public override bool IsNormalizedKeyPrefixOnly(int keyBytes) => true;

    public override void PutNormalizedKey(string record, MemorySegment target, int offset, int numBytes)
    {
        ArgumentNullException.ThrowIfNull(record);
        int limit = offset + numBytes;
        int end = record.Length;
        int pos = 0;

        while (pos < end && offset < limit)
        {
            char c = record[pos++];
            if (c < HighBit)
            {
                target.Put(offset++, (byte)c);
            }
            else if (c < HighBit2)
            {
                target.Put(offset++, unchecked((byte)((c >>> 7) | HighBit)));
                if (offset < limit)
                {
                    target.Put(offset++, unchecked((byte)c));
                }
            }
            else
            {
                target.Put(offset++, unchecked((byte)((c >>> 10) | HighBit2Mask)));
                if (offset < limit)
                {
                    target.Put(offset++, unchecked((byte)(c >>> 2)));
                }
                if (offset < limit)
                {
                    target.Put(offset++, unchecked((byte)c));
                }
            }
        }
        while (offset < limit)
        {
            target.Put(offset++, 0);
        }
    }

    public override StringComparator Duplicate() => new(AscendingComparison);

    // Java fails with a NullPointerException when comparing a serialized null string
    private static string ReadNonNullString(IDataInputView source) =>
        StringValue.ReadString(source)
            ?? throw new InvalidOperationException("Cannot compare a null string.");
}
