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

/// <summary>Boxed serializable and comparable short integer type, representing the primitive type
/// <c>short</c>.</summary>
[Public]
public class ShortValue : INormalizableKey<ShortValue>, IResettableValue<ShortValue>, ICopyableValue<ShortValue>
{
    private short _value;

    /// <summary>Initializes the encapsulated short with 0.</summary>
    public ShortValue()
    {
    }

    /// <summary>Initializes the encapsulated short with the provided value.</summary>
    /// <param name="value">Initial value of the encapsulated short.</param>
    public ShortValue(short value)
    {
        _value = value;
    }

    /// <summary>Returns the value of the encapsulated short.</summary>
    public short GetValue() => _value;

    /// <summary>Sets the encapsulated short to the specified value.</summary>
    /// <param name="value">the new value of the encapsulated short.</param>
    public void SetValue(short value) => _value = value;

    public void SetValue(ShortValue value) => _value = value._value;

    public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);

    // --------------------------------------------------------------------------------------------

    public void Read(IDataInputView input) => _value = input.ReadShort();

    public void Write(IDataOutputView output) => output.WriteShort(_value);

    // --------------------------------------------------------------------------------------------

    public int CompareTo(ShortValue? other)
    {
        short otherValue = other!._value;
        return _value < otherValue ? -1 : _value > otherValue ? 1 : 0;
    }

    // Java's hashCode: the value itself
    public override int GetHashCode() => _value;

    public override bool Equals(object? obj) => obj is ShortValue other && other._value == _value;

    // --------------------------------------------------------------------------------------------

    public int MaxNormalizedKeyLen => 2;

    public void CopyNormalizedKey(MemorySegment memory, int offset, int len) =>
        NormalizedKeyUtil.PutShortNormalizedKey(_value, memory, offset, len);

    // --------------------------------------------------------------------------------------------

    public int BinaryLength => 2;

    public void CopyTo(ShortValue target) => target._value = _value;

    public ShortValue Copy() => new(_value);

    public void Copy(IDataInputView source, IDataOutputView target) => target.Write(source, 2);
}
