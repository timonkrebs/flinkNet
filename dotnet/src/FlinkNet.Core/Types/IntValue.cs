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

/// <summary>Boxed serializable and comparable integer type, representing the primitive type
/// <c>int</c>.</summary>
[Public]
public class IntValue : INormalizableKey<IntValue>, IResettableValue<IntValue>, ICopyableValue<IntValue>
{
    private int _value;

    /// <summary>Initializes the encapsulated int with 0.</summary>
    public IntValue()
    {
        _value = 0;
    }

    /// <summary>Initializes the encapsulated int with the provided value.</summary>
    /// <param name="value">Initial value of the encapsulated int.</param>
    public IntValue(int value)
    {
        _value = value;
    }

    /// <summary>Returns the value of the encapsulated int.</summary>
    public int GetValue() => _value;

    /// <summary>Sets the encapsulated int to the specified value.</summary>
    /// <param name="value">the new value of the encapsulated int.</param>
    public void SetValue(int value) => _value = value;

    public void SetValue(IntValue value) => _value = value._value;

    public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);

    // --------------------------------------------------------------------------------------------

    public void Read(IDataInputView input) => _value = input.ReadInt();

    public void Write(IDataOutputView output) => output.WriteInt(_value);

    // --------------------------------------------------------------------------------------------

    public int CompareTo(IntValue? other)
    {
        int otherValue = other!._value;
        return _value < otherValue ? -1 : _value > otherValue ? 1 : 0;
    }

    public override int GetHashCode() => _value;

    public override bool Equals(object? obj) => obj is IntValue other && other._value == _value;

    // --------------------------------------------------------------------------------------------

    public int MaxNormalizedKeyLen => 4;

    public void CopyNormalizedKey(MemorySegment memory, int offset, int len)
    {
        // take out the value and add the integer min value. This gets an offset representation
        // when interpreted as an unsigned integer (as is the case with normalized keys). write
        // this value as big endian to ensure the most significant byte comes first.
        if (len == 4)
        {
            memory.PutIntBigEndian(offset, unchecked(_value - int.MinValue));
        }
        else if (len <= 0)
        {
        }
        else if (len < 4)
        {
            int value = unchecked(_value - int.MinValue);
            for (int i = 0; len > 0; len--, i++)
            {
                memory.Put(offset + i, (byte)((value >>> ((3 - i) << 3)) & 0xff));
            }
        }
        else
        {
            memory.PutIntBigEndian(offset, unchecked(_value - int.MinValue));
            for (int i = 4; i < len; i++)
            {
                memory.Put(offset + i, 0);
            }
        }
    }

    // --------------------------------------------------------------------------------------------

    public int BinaryLength => 4;

    public void CopyTo(IntValue target) => target._value = _value;

    public IntValue Copy() => new(_value);

    public void Copy(IDataInputView source, IDataOutputView target) => target.Write(source, 4);
}
