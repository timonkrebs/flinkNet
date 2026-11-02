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

using FlinkNet.Api.Common.Watermarks;
using FlinkNet.DataStream.Api.Extension.EventTime;
using FlinkNet.DataStream.Api.Extension.EventTime.Strategy;
using FlinkNet.DataStream.Api.Extension.Window.Strategy;
using Xunit;

namespace FlinkNet.Tests.DataStream.Api;

/// <summary>Smoke tests for the DataStream API extensions (event time and windows).</summary>
public class ExtensionsSmokeTest
{
    [Fact]
    public void TestEventTimeWatermarkDeclarations()
    {
        Assert.Equal(
            "BUILTIN_API_EVENT_TIME", EventTimeExtension.EventTimeWatermarkDeclaration.Identifier);
        Assert.Equal(
            "BUILTIN_API_EVENT_TIME_IDLE",
            EventTimeExtension.IdleStatusWatermarkDeclaration.Identifier);

        // combination policies match Java: MIN + wait-for-all for event time, AND for idleness
        Assert.Same(
            IWatermarkCombinationFunction.NumericWatermarkCombinationFunction.Min,
            EventTimeExtension.EventTimeWatermarkDeclaration.CombinationPolicy
                .WatermarkCombinationFunction);
        Assert.True(
            EventTimeExtension.EventTimeWatermarkDeclaration.CombinationPolicy
                .IsCombineWaitForAllChannels);
        Assert.Same(
            IWatermarkCombinationFunction.BoolWatermarkCombinationFunction.And,
            EventTimeExtension.IdleStatusWatermarkDeclaration.CombinationPolicy
                .WatermarkCombinationFunction);

        IWatermark eventTimeWatermark =
            EventTimeExtension.EventTimeWatermarkDeclaration.NewWatermark(42);
        Assert.True(EventTimeExtension.IsEventTimeWatermark(eventTimeWatermark));
        Assert.False(EventTimeExtension.IsIdleStatusWatermark(eventTimeWatermark));
        Assert.True(EventTimeExtension.IsIdleStatusWatermark("BUILTIN_API_EVENT_TIME_IDLE"));
    }

    [Fact]
    public void TestWatermarkGeneratorBuilderRequiresImplProvider()
    {
        EventTimeWatermarkGeneratorBuilder<string> builder =
            EventTimeExtension.NewWatermarkGeneratorBuilder(new LengthExtractor())
                .WithIdleness(TimeSpan.FromSeconds(5))
                .WithMaxOutOfOrderTime(TimeSpan.FromSeconds(1))
                .PeriodicWatermark(TimeSpan.FromMilliseconds(200));

        // no implementation module registered in this test assembly
        Assert.Throws<InvalidOperationException>(() => builder.BuildAsProcessFunction());
    }

    [Fact]
    public void TestWindowStrategyFactories()
    {
        WindowStrategy global = WindowStrategy.Global();
        Assert.IsType<GlobalWindowStrategy>(global);

        var tumbling =
            Assert.IsType<TumblingTimeWindowStrategy>(
                WindowStrategy.Tumbling(TimeSpan.FromMinutes(5)));
        Assert.Equal(TimeSpan.FromMinutes(5), tumbling.WindowSize);
        Assert.Equal(WindowStrategy.TimeType.Event, tumbling.WindowTimeType);
        Assert.Equal(TimeSpan.Zero, tumbling.AllowedLateness);

        var sliding =
            Assert.IsType<SlidingTimeWindowStrategy>(
                WindowStrategy.Sliding(
                    TimeSpan.FromMinutes(10),
                    TimeSpan.FromMinutes(1),
                    WindowStrategy.ProcessingTime,
                    TimeSpan.FromSeconds(30)));
        Assert.Equal(TimeSpan.FromMinutes(10), sliding.WindowSize);
        Assert.Equal(TimeSpan.FromMinutes(1), sliding.WindowSlideInterval);
        Assert.Equal(WindowStrategy.TimeType.Processing, sliding.WindowTimeType);
        Assert.Equal(TimeSpan.FromSeconds(30), sliding.AllowedLateness);

        var session =
            Assert.IsType<SessionWindowStrategy>(WindowStrategy.Session(TimeSpan.FromSeconds(45)));
        Assert.Equal(TimeSpan.FromSeconds(45), session.SessionGap);
        Assert.Equal(WindowStrategy.TimeType.Event, session.WindowTimeType);
    }

    [Fact]
    public void TestEventTimeWatermarkStrategyDefaults()
    {
        var strategy = new EventTimeWatermarkStrategy<string>(new LengthExtractor());
        Assert.Equal(EventTimeWatermarkGenerateMode.Periodic, strategy.GenerateMode);
        Assert.Equal(TimeSpan.Zero, strategy.PeriodicWatermarkInterval);
        Assert.Equal(TimeSpan.Zero, strategy.IdleTimeout);
        Assert.Equal(TimeSpan.Zero, strategy.MaxOutOfOrderTime);
        Assert.Equal(5, strategy.EventTimeExtractor.ExtractTimestamp("hello"));
    }

    private sealed class LengthExtractor : IEventTimeExtractor<string>
    {
        public long ExtractTimestamp(string @event) => @event.Length;
    }
}
