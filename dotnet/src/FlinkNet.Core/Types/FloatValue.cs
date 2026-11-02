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
using FlinkNet.Util;

namespace FlinkNet.Types;

/// <summary>
/// Boxed serializable and comparable float type, representing the primitive type
/// <c>float</c>.
///
/// <para>As in Java, <see cref="CompareTo"/> uses the IEEE comparison operators (-0.0 and 0.0
/// compare as equal, and so does NaN with any value), while <see cref="Equals"/> and
/// <see cref="GetHashCode"/> use the canonical bits (-0.0 differs from 0.0, NaN equals NaN).
/// <see cref="ToString"/> prints like Java's <c>Float.toString</c>.</para>
/// </summary>
[Public]
public class FloatValue : IComparable<FloatValue>, IResettableValue<FloatValue>, ICopyableValue<FloatValue>
{
    private float _value;

    /// <summary>Initializes the encapsulated float with 0.0.</summary>
    public FloatValue()
    {
    }

    /// <summary>Initializes the encapsulated float with the provided value.</summary>
    /// <param name="value">Initial value of the encapsulated float.</param>
    public FloatValue(float value)
    {
        _value = value;
    }

    /// <summary>Returns the value of the encapsulated primitive float.</summary>
    public float GetValue() => _value;

    /// <summary>Sets the value of the encapsulated primitive float.</summary>
    /// <param name="value">the new value of the encapsulated float.</param>
    public void SetValue(float value) => _value = value;

    public void SetValue(FloatValue value) => _value = value._value;

    // --------------------------------------------------------------------------------------------

    public void Read(IDataInputView input) => _value = input.ReadFloat();

    public void Write(IDataOutputView output) => output.WriteFloat(_value);

    // --------------------------------------------------------------------------------------------

    public override string ToString() => JavaPrimitives.ToString(_value);

    public int CompareTo(FloatValue? other)
    {
        double otherValue = other!._value;
        return _value < otherValue ? -1 : _value > otherValue ? 1 : 0;
    }

    public override int GetHashCode() => JavaPrimitives.FloatToIntBits(_value);

    public override bool Equals(object? obj) =>
        obj is FloatValue other && JavaPrimitives.FloatToIntBits(_value) == JavaPrimitives.FloatToIntBits(other._value);

    // --------------------------------------------------------------------------------------------

    public int BinaryLength => 4;

    public void CopyTo(FloatValue target) => target._value = _value;

    public FloatValue Copy() => new(_value);

    public void Copy(IDataInputView source, IDataOutputView target) => target.Write(source, 4);
}
