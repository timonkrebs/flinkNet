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
using FlinkNet.Api.Common.TypeUtils.Base;
using FlinkNet.Core.Memory;

namespace FlinkNet.Types;

/// <summary>Boxed serializable and comparable character type, representing the primitive type
/// <c>char</c>.</summary>
[Public]
public class CharValue : INormalizableKey<CharValue>, IResettableValue<CharValue>, ICopyableValue<CharValue>
{
    private char _value;

    /// <summary>Initializes the encapsulated char with 0.</summary>
    public CharValue()
    {
    }

    /// <summary>Initializes the encapsulated char with the provided value.</summary>
    /// <param name="value">Initial value of the encapsulated char.</param>
    public CharValue(char value)
    {
        _value = value;
    }

    /// <summary>Returns the value of the encapsulated char.</summary>
    public char GetValue() => _value;

    /// <summary>Sets the encapsulated char to the specified value.</summary>
    /// <param name="value">the new value of the encapsulated char.</param>
    public void SetValue(char value) => _value = value;

    public void SetValue(CharValue value) => _value = value._value;

    public override string ToString() => _value.ToString();

    // --------------------------------------------------------------------------------------------

    public void Read(IDataInputView input) => _value = input.ReadChar();

    public void Write(IDataOutputView output) => output.WriteChar(_value);

    // --------------------------------------------------------------------------------------------

    public int CompareTo(CharValue? other)
    {
        char otherValue = other!._value;
        return _value < otherValue ? -1 : _value > otherValue ? 1 : 0;
    }

    // Java's hashCode: the value itself
    public override int GetHashCode() => _value;

    public override bool Equals(object? obj) => obj is CharValue other && other._value == _value;

    // --------------------------------------------------------------------------------------------

    public int MaxNormalizedKeyLen => 2;

    public void CopyNormalizedKey(MemorySegment memory, int offset, int len) =>
        NormalizedKeyUtil.PutCharNormalizedKey(_value, memory, offset, len);

    // --------------------------------------------------------------------------------------------

    public int BinaryLength => 2;

    public void CopyTo(CharValue target) => target._value = _value;

    public CharValue Copy() => new(_value);

    public void Copy(IDataInputView source, IDataOutputView target) => target.Write(source, 2);
}
