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

using FlinkNet.Core.Memory;
using FlinkNet.Types;
using Xunit;

namespace FlinkNet.Tests.Types;

/// <summary>
/// Port of Flink's <c>PrimitiveDataTypeTest</c>: value retrieval, comparison and stream
/// round trips through the data-view stream wrappers.
///
/// <para>PORT NOTE: a <see cref="MemoryStream"/> replaces Java's piped streams, and the case
/// of the not yet ported mutable <c>StringValue</c> class is left out.</para>
/// </summary>
public sealed class PrimitiveDataTypeTest : IDisposable
{
    private readonly MemoryStream _buffer = new();

    private readonly DataOutputViewStreamWrapper _out;

    public PrimitiveDataTypeTest()
    {
        _out = new DataOutputViewStreamWrapper(_buffer, leaveOpen: true);
    }

    public void Dispose()
    {
        _out.Dispose();
        _buffer.Dispose();
    }

    // reads what has been written so far
    private DataInputViewStreamWrapper In() => new(new MemoryStream(_buffer.ToArray()));

    [Fact]
    public void TestIntValue()
    {
        var int0 = new IntValue(10);
        // test value retrieval
        Assert.Equal(10, int0.GetValue());
        // test value comparison
        var int1 = new IntValue(10);
        var int2 = new IntValue(-10);
        var int3 = new IntValue(20);
        Assert.Equal(0, int0.CompareTo(int0));
        Assert.Equal(0, int0.CompareTo(int1));
        Assert.Equal(1, int0.CompareTo(int2));
        Assert.Equal(-1, int0.CompareTo(int3));
        // test stream output and retrieval
        int0.Write(_out);
        int2.Write(_out);
        int3.Write(_out);
        DataInputViewStreamWrapper mIn = In();
        var int1n = new IntValue();
        var int2n = new IntValue();
        var int3n = new IntValue();
        int1n.Read(mIn);
        int2n.Read(mIn);
        int3n.Read(mIn);
        Assert.Equal(0, int0.CompareTo(int1n));
        Assert.Equal(int0.GetValue(), int1n.GetValue());
        Assert.Equal(0, int2.CompareTo(int2n));
        Assert.Equal(int2.GetValue(), int2n.GetValue());
        Assert.Equal(0, int3.CompareTo(int3n));
        Assert.Equal(int3.GetValue(), int3n.GetValue());
    }

    [Fact]
    public void TestDoubleValue()
    {
        var double0 = new DoubleValue(10.2);
        // test value retrieval
        Assert.Equal(10.2, double0.GetValue(), 4);
        // test value comparison
        var double1 = new DoubleValue(10.2);
        var double2 = new DoubleValue(-10.5);
        var double3 = new DoubleValue(20.2);
        Assert.Equal(0, double0.CompareTo(double0));
        Assert.Equal(0, double0.CompareTo(double1));
        Assert.Equal(1, double0.CompareTo(double2));
        Assert.Equal(-1, double0.CompareTo(double3));
        // test stream output and retrieval
        double0.Write(_out);
        double2.Write(_out);
        double3.Write(_out);
        DataInputViewStreamWrapper mIn = In();
        var double1n = new DoubleValue();
        var double2n = new DoubleValue();
        var double3n = new DoubleValue();
        double1n.Read(mIn);
        double2n.Read(mIn);
        double3n.Read(mIn);
        Assert.Equal(0, double0.CompareTo(double1n));
        Assert.Equal(double0.GetValue(), double1n.GetValue(), 4);
        Assert.Equal(0, double2.CompareTo(double2n));
        Assert.Equal(double2.GetValue(), double2n.GetValue(), 4);
        Assert.Equal(0, double3.CompareTo(double3n));
        Assert.Equal(double3.GetValue(), double3n.GetValue(), 4);
    }

    [Fact]
    public void TestPactNull()
    {
        var pn1 = new NullValue();
        var pn2 = new NullValue();
        Assert.Equal(pn1, pn2);
        Assert.Equal(pn2, pn1);

        // test serialization
        var pn = new NullValue();
        const int numWrites = 13;
        // write it multiple times
        for (int i = 0; i < numWrites; i++)
        {
            pn.Write(_out);
        }
        // read it multiple times
        DataInputViewStreamWrapper mIn = In();
        for (int i = 0; i < numWrites; i++)
        {
            pn.Read(mIn);
        }
        Assert.Equal(-1, mIn.Read(new byte[1]));
    }
}
