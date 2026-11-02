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
/// Boxed serializable and comparable double type, representing the primitive type
/// <c>double</c>.
///
/// <para>As in Java, <see cref="CompareTo"/> uses the IEEE comparison operators (-0.0 and 0.0
/// compare as equal, and so does NaN with any value), while <see cref="Equals"/> and
/// <see cref="GetHashCode"/> use the canonical bits (-0.0 differs from 0.0, NaN equals NaN).
/// <see cref="ToString"/> prints like Java's <c>Double.toString</c>.</para>
/// </summary>
[Public]
public class DoubleValue : IComparable<DoubleValue>, IResettableValue<DoubleValue>, ICopyableValue<DoubleValue>
{
    private double _value;

    /// <summary>Initializes the encapsulated double with 0.0.</summary>
    public DoubleValue()
    {
    }

    /// <summary>Initializes the encapsulated double with the provided value.</summary>
    /// <param name="value">Initial value of the encapsulated double.</param>
    public DoubleValue(double value)
    {
        _value = value;
    }

    /// <summary>Returns the value of the encapsulated primitive double.</summary>
    public double GetValue() => _value;

    /// <summary>Sets the value of the encapsulated primitive double.</summary>
    /// <param name="value">the new value of the encapsulated double.</param>
    public void SetValue(double value) => _value = value;

    public void SetValue(DoubleValue value) => _value = value._value;

    // --------------------------------------------------------------------------------------------

    public void Read(IDataInputView input) => _value = input.ReadDouble();

    public void Write(IDataOutputView output) => output.WriteDouble(_value);

    // --------------------------------------------------------------------------------------------

    public override string ToString() => JavaPrimitives.ToString(_value);

    public int CompareTo(DoubleValue? other)
    {
        double otherValue = other!._value;
        return _value < otherValue ? -1 : _value > otherValue ? 1 : 0;
    }

    public override int GetHashCode() => unchecked(31 + JavaPrimitives.HashCode(_value));

    public override bool Equals(object? obj) =>
        obj is DoubleValue other && JavaPrimitives.DoubleToLongBits(_value) == JavaPrimitives.DoubleToLongBits(other._value);

    // --------------------------------------------------------------------------------------------

    public int BinaryLength => 8;

    public void CopyTo(DoubleValue target) => target._value = _value;

    public DoubleValue Copy() => new(_value);

    public void Copy(IDataInputView source, IDataOutputView target) => target.Write(source, 8);
}
