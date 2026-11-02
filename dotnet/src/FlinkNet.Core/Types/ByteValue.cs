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
using FlinkNet.Api.Common.TypeUtils.Base;
using FlinkNet.Core.Memory;

namespace FlinkNet.Types;

/// <summary>Boxed serializable and comparable byte type, representing Java's signed primitive
/// type <c>byte</c>, which maps to <c>sbyte</c> (see <see cref="ByteSerializer"/>).</summary>
[Public]
public class ByteValue : INormalizableKey<ByteValue>, IResettableValue<ByteValue>, ICopyableValue<ByteValue>
{
    private sbyte _value;

    /// <summary>Initializes the encapsulated sbyte with 0.</summary>
    public ByteValue()
    {
    }

    /// <summary>Initializes the encapsulated sbyte with the provided value.</summary>
    /// <param name="value">Initial value of the encapsulated sbyte.</param>
    public ByteValue(sbyte value)
    {
        _value = value;
    }

    /// <summary>Returns the value of the encapsulated sbyte.</summary>
    public sbyte GetValue() => _value;

    /// <summary>Sets the encapsulated sbyte to the specified value.</summary>
    /// <param name="value">the new value of the encapsulated sbyte.</param>
    public void SetValue(sbyte value) => _value = value;

    public void SetValue(ByteValue value) => _value = value._value;

    public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);

    // --------------------------------------------------------------------------------------------

    public void Read(IDataInputView input) => _value = unchecked((sbyte)input.ReadByte());

    public void Write(IDataOutputView output) => output.WriteByte(_value);

    // --------------------------------------------------------------------------------------------

    public int CompareTo(ByteValue? other)
    {
        sbyte otherValue = other!._value;
        return _value < otherValue ? -1 : _value > otherValue ? 1 : 0;
    }

    // Java's hashCode: the value itself
    public override int GetHashCode() => _value;

    public override bool Equals(object? obj) => obj is ByteValue other && other._value == _value;

    // --------------------------------------------------------------------------------------------

    public int MaxNormalizedKeyLen => 1;

    public void CopyNormalizedKey(MemorySegment memory, int offset, int len) =>
        NormalizedKeyUtil.PutByteNormalizedKey(_value, memory, offset, len);

    // --------------------------------------------------------------------------------------------

    public int BinaryLength => 1;

    public void CopyTo(ByteValue target) => target._value = _value;

    public ByteValue Copy() => new(_value);

    public void Copy(IDataInputView source, IDataOutputView target) => target.Write(source, 1);
}
