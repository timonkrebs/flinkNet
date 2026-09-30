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

using FlinkNet.Api.Common.State;
using FlinkNet.Api.Common.TypeInfo;
using FlinkNet.Api.Common.Watermarks;
using FlinkNet.DataStream.Api;
using FlinkNet.DataStream.Api.Common;
using FlinkNet.DataStream.Api.Context;
using FlinkNet.DataStream.Api.Function;
using Xunit;

namespace FlinkNet.Tests.DataStream.Api;

/// <summary>
/// Smoke tests proving the DataStream API surface is implementable and that the default
/// interface methods behave like Java's default methods.
/// </summary>
public class DataStreamApiSmokeTest
{
    [Fact]
    public void TestProcessFunctionDefaults()
    {
        var function = new CountingFunction();

        // default methods: no states/watermarks declared, close is a no-op
        IProcessFunction asBase = function;
        Assert.Empty(asBase.UsesStates());
        Assert.Empty(asBase.DeclareWatermarks());
        asBase.Close();

        // the watermark callback defaults to PEEK
        IOneInputStreamProcessFunction<string, int> asTyped = function;
        Assert.Equal(
            WatermarkHandlingResult.Peek, asTyped.OnWatermark(new LongWatermark(1, "wm"), null!, null!));
    }

    [Fact]
    public void TestProcessRecordAndCollector()
    {
        var function = new CountingFunction();
        var collector = new ListCollector<int>();

        function.ProcessRecord("ab", collector, null!);
        function.ProcessRecord("cde", collector, null!);

        Assert.Equal([2, 3], collector.Collected);
    }

    [Fact]
    public void TestStateDeclarationsOnFunction()
    {
        var function = new StatefulFunction();
        IReadOnlySet<IStateDeclaration> states = ((IProcessFunction)function).UsesStates();
        IStateDeclaration declaration = Assert.Single(states);
        Assert.Equal("seen-count", declaration.Name);
    }

    [Fact]
    public void TestExecutionEnvironmentFactoryRegistrationRequired()
    {
        // no factory registered in this test assembly
        Assert.Throws<InvalidOperationException>(() => IExecutionEnvironment.GetInstance());
    }

    // ------------------------------------------------------------------

    private sealed class CountingFunction : IOneInputStreamProcessFunction<string, int>
    {
        public void ProcessRecord(string record, ICollector<int> output, IPartitionedContext<int> ctx) =>
            output.Collect(record.Length);
    }

    private sealed class StatefulFunction : IOneInputStreamProcessFunction<string, string>
    {
        private static readonly IValueStateDeclaration<long> CountState =
            StateDeclarations.ValueState("seen-count", TypeDescriptors.LONG);

        public IReadOnlySet<IStateDeclaration> UsesStates() =>
            new HashSet<IStateDeclaration> { CountState };

        public void ProcessRecord(
            string record, ICollector<string> output, IPartitionedContext<string> ctx) =>
            output.Collect(record);
    }

    private sealed class ListCollector<T> : ICollector<T>
    {
        public List<T> Collected { get; } = [];

        public void Collect(T record) => Collected.Add(record);

        public void CollectAndOverwriteTimestamp(T record, long timestamp) => Collected.Add(record);
    }
}
