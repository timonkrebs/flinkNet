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

using System.Globalization;
using FlinkNet.Annotations;
using FlinkNet.Core.Memory;

namespace FlinkNet.Types;

/// <summary>Boxed serializable and comparable long integer type, representing the primitive type
/// <c>long</c>.</summary>
[Public]
public class LongValue : INormalizableKey<LongValue>, IResettableValue<LongValue>, ICopyableValue<LongValue>
{
    private long _value;

    /// <summary>Initializes the encapsulated long with 0.</summary>
    public LongValue()
    {
        _value = 0;
    }

    /// <summary>Initializes the encapsulated long with the specified value.</summary>
    /// <param name="value">Initial value of the encapsulated long.</param>
    public LongValue(long value)
    {
        _value = value;
    }

    /// <summary>Returns the value of the encapsulated long.</summary>
    public long GetValue() => _value;

    /// <summary>Sets the value of the encapsulated long to the specified value.</summary>
    /// <param name="value">The new value of the encapsulated long.</param>
    public void SetValue(long value) => _value = value;

    public void SetValue(LongValue value) => _value = value._value;

    public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);

    // --------------------------------------------------------------------------------------------

    public void Read(IDataInputView input) => _value = input.ReadLong();

    public void Write(IDataOutputView output) => output.WriteLong(_value);

    // --------------------------------------------------------------------------------------------

    public int CompareTo(LongValue? other)
    {
        long otherValue = other!._value;
        return _value < otherValue ? -1 : _value > otherValue ? 1 : 0;
    }

    public override int GetHashCode() => (int)(_value ^ (_value >>> 32));

    public override bool Equals(object? obj) => obj is LongValue other && other._value == _value;

    // --------------------------------------------------------------------------------------------

    public int MaxNormalizedKeyLen => 8;

    public void CopyNormalizedKey(MemorySegment memory, int offset, int len)
    {
        // see IntValue for an explanation of the logic
        if (len == 8)
        {
            // default case, full normalized key
            memory.PutLongBigEndian(offset, unchecked(_value - long.MinValue));
        }
        else if (len <= 0)
        {
        }
        else if (len < 8)
        {
            long value = unchecked(_value - long.MinValue);
            for (int i = 0; len > 0; len--, i++)
            {
                memory.Put(offset + i, (byte)(value >>> ((7 - i) << 3)));
            }
        }
        else
        {
            memory.PutLongBigEndian(offset, unchecked(_value - long.MinValue));
            for (int i = 8; i < len; i++)
            {
                memory.Put(offset + i, 0);
            }
        }
    }

    // --------------------------------------------------------------------------------------------

    public int BinaryLength => 8;

    public void CopyTo(LongValue target) => target._value = _value;

    public LongValue Copy() => new(_value);

    public void Copy(IDataInputView source, IDataOutputView target) => target.Write(source, 8);
}
