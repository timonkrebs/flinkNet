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

/// <summary>Null base type for programs that implements the key interfaces.</summary>
[Public]
public sealed class NullValue : INormalizableKey<NullValue>, ICopyableValue<NullValue>
{
    /// <summary>The singleton NullValue instance.</summary>
    private static readonly NullValue Singleton = new();

    /// <summary>Returns the NullValue singleton instance.</summary>
    public static NullValue GetInstance() => Singleton;

    /// <summary>Creates a NullValue object.</summary>
    public NullValue()
    {
    }

    public override string ToString() => "(null)";

    // --------------------------------------------------------------------------------------------

    public void Read(IDataInputView input) => input.ReadBoolean();

    public void Write(IDataOutputView output) => output.WriteBoolean(false);

    // --------------------------------------------------------------------------------------------

    public int CompareTo(NullValue? other) => 0;

    public override bool Equals(object? obj) => obj != null && obj.GetType() == typeof(NullValue);

    public override int GetHashCode() => 53;

    // --------------------------------------------------------------------------------------------

    public int MaxNormalizedKeyLen => 0;

    public void CopyNormalizedKey(MemorySegment memory, int offset, int len)
    {
        for (int i = offset; i < offset + len; i++)
        {
            memory.Put(i, 0);
        }
    }

    // --------------------------------------------------------------------------------------------

    public int BinaryLength => 1;

    public void CopyTo(NullValue target)
    {
    }

    public NullValue Copy() => GetInstance();

    public void Copy(IDataInputView source, IDataOutputView target)
    {
        source.ReadBoolean();
        target.WriteBoolean(false);
    }
}
