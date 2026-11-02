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

namespace FlinkNet.Types;

/// <summary>Boxed serializable and comparable boolean type, representing the primitive type
/// <c>bool</c>.</summary>
[Public]
public class BooleanValue
    : INormalizableKey<BooleanValue>, IResettableValue<BooleanValue>, ICopyableValue<BooleanValue>
{
    /// <summary>A shared true instance. Mutable, like the Java original — handle with care.</summary>
    public static readonly BooleanValue True = new(true);

    /// <summary>A shared false instance. Mutable, like the Java original — handle with care.</summary>
    public static readonly BooleanValue False = new(false);

    private bool _value;

    public BooleanValue()
    {
    }

    public BooleanValue(bool value)
    {
        _value = value;
    }

    public bool Get() => _value;

    public void Set(bool value) => _value = value;

    public bool GetValue() => _value;

    public void SetValue(bool value) => _value = value;

    public void SetValue(BooleanValue value) => _value = value._value;

    public override string ToString() => _value ? "true" : "false";

    // --------------------------------------------------------------------------------------------

    public void Read(IDataInputView input) => _value = input.ReadBoolean();

    public void Write(IDataOutputView output) => output.WriteBoolean(_value);

    // --------------------------------------------------------------------------------------------

    public int CompareTo(BooleanValue? other)
    {
        int otherValue = other!._value ? 1 : 0;
        int thisValue = _value ? 1 : 0;
        return thisValue - otherValue;
    }

    public override int GetHashCode() => _value ? 1 : 0;

    public override bool Equals(object? obj) => obj is BooleanValue other && other._value == _value;

    // --------------------------------------------------------------------------------------------

    public int MaxNormalizedKeyLen => 1;

    public void CopyNormalizedKey(MemorySegment memory, int offset, int len)
    {
        if (len > 0)
        {
            memory.Put(offset, (byte)(_value ? 1 : 0));

            for (offset += 1; len > 1; len--)
            {
                memory.Put(offset++, 0);
            }
        }
    }

    // --------------------------------------------------------------------------------------------

    public int BinaryLength => 1;

    public void CopyTo(BooleanValue target) => target._value = _value;

    public BooleanValue Copy() => new(_value);

    public void Copy(IDataInputView source, IDataOutputView target) => target.Write(source, 1);
}
