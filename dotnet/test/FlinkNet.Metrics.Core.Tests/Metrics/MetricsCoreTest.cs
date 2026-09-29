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

using FlinkNet.Metrics;
using FlinkNet.Metrics.Groups;
using FlinkNet.Metrics.Reporter;
using Xunit;

namespace FlinkNet.Tests.Metrics;

/// <summary>Tests for counters, MetricConfig, metric types and the AbstractReporter routing.</summary>
public class MetricsCoreTest
{
    [Fact]
    public void TestSimpleCounter()
    {
        var counter = new SimpleCounter();
        Assert.Equal(0, counter.Count);
        counter.Inc();
        counter.Inc(5);
        Assert.Equal(6, counter.Count);
        counter.Dec();
        counter.Dec(2);
        Assert.Equal(3, counter.Count);
        Assert.Equal(MetricType.Counter, ((IMetric)counter).GetMetricType());
    }

    [Fact]
    public async Task TestThreadSafeSimpleCounter()
    {
        var counter = new ThreadSafeSimpleCounter();
        Task[] tasks =
            Enumerable.Range(0, 8)
                .Select(_ =>
                    Task.Run(() =>
                    {
                        for (int i = 0; i < 10_000; i++)
                        {
                            counter.Inc();
                        }
                        counter.Dec(5000);
                    }))
                .ToArray();
        await Task.WhenAll(tasks);
        Assert.Equal(8 * (10_000 - 5000), counter.Count);
    }

    [Fact]
    public void TestMetricConfigTypedGetters()
    {
        var config = new MetricConfig();
        config.SetProperty("int", "42");
        config.SetProperty("long", "123456789012345");
        config.SetProperty("float", "0.5");
        config.SetProperty("double", "3.14");
        config.SetProperty("bool", "TRUE");
        config.SetProperty("string", "hello");

        Assert.Equal(42, config.GetInteger("int", 0));
        Assert.Equal(123456789012345L, config.GetLong("long", 0L));
        Assert.Equal(0.5f, config.GetFloat("float", 0f));
        Assert.Equal(3.14, config.GetDouble("double", 0.0));
        Assert.True(config.GetBoolean("bool", false));
        Assert.Equal("hello", config.GetString("string", "default"));

        // native and cross-type values
        config["nativeInt"] = 7;
        config["crossType"] = 12L;
        Assert.Equal(7, config.GetInteger("nativeInt", 0));
        Assert.Equal(7L, config.GetLong("nativeInt", 0L));
        Assert.Equal(12, config.GetInteger("crossType", 0));
        Assert.Equal(12.0, config.GetDouble("crossType", 0.0));

        // defaults for missing keys; non-"true" strings are false (Boolean.parseBoolean)
        Assert.Equal(9, config.GetInteger("missing", 9));
        Assert.Equal("d", config.GetString("missing", "d"));
        Assert.False(config.GetBoolean("missing", false));
        config.SetProperty("notBool", "yes");
        Assert.False(config.GetBoolean("notBool", true));
    }

    /// <summary>Native numbers convert like Java's Number.intValue()/longValue(): floating
    /// values truncate toward zero and saturate (NaN to 0), longs narrow to int by wrapping.</summary>
    [Fact]
    public void TestMetricConfigNumericConversionsFollowJava()
    {
        var config = new MetricConfig
        {
            ["pos"] = 1.9,
            ["neg"] = -1.9,
            ["half"] = 3.5,
            ["float"] = 2.7f,
            ["big"] = 1e10,
            ["huge"] = 1e30,
            ["nan"] = double.NaN,
            ["wide"] = 4294967297L,
        };

        Assert.Equal(1, config.GetInteger("pos", 0));
        Assert.Equal(-1, config.GetInteger("neg", 0));
        Assert.Equal(3, config.GetInteger("half", 0));
        Assert.Equal(2, config.GetInteger("float", 0));
        Assert.Equal(int.MaxValue, config.GetInteger("big", 0));
        Assert.Equal(0, config.GetInteger("nan", 7));
        Assert.Equal(1, config.GetInteger("wide", 0));

        Assert.Equal(1L, config.GetLong("pos", 0L));
        Assert.Equal(-1L, config.GetLong("neg", 0L));
        Assert.Equal(10_000_000_000L, config.GetLong("big", 0L));
        Assert.Equal(long.MaxValue, config.GetLong("huge", 0L));
        Assert.Equal(0L, config.GetLong("nan", 7L));
    }

    [Fact]
    public void TestUnregisteredMetricsGroup()
    {
        IMetricGroup group = new UnregisteredMetricsGroup();

        ICounter counter = group.Counter("c");
        Assert.IsType<SimpleCounter>(counter);

        var myCounter = new ThreadSafeSimpleCounter();
        Assert.Same(myCounter, group.Counter("c2", myCounter));

        var gauge = new ConstantGauge<string>("v");
        Assert.Same(gauge, group.Gauge<string>("g", gauge));

        var meter = new MeterView(10);
        Assert.Same(meter, group.Meter("m", meter));

        Assert.NotNull(group.AddGroup("sub"));
        Assert.NotNull(group.AddGroup("key", "value"));
        Assert.Empty(group.GetScopeComponents());
        Assert.Empty(group.GetAllVariables());
        Assert.Equal("metric", group.GetMetricIdentifier("metric"));
        Assert.Equal("metric", group.GetMetricIdentifier("metric", ICharacterFilter.NoOpFilter));
    }

    [Fact]
    public void TestAbstractReporterRoutesByMetricType()
    {
        var reporter = new TestReporter();
        IMetricGroup group = new UnregisteredMetricsGroup();

        var counter = new SimpleCounter();
        var gauge = new ConstantGauge<int>(1);
        var meter = new MeterView(counter);
        var histogram = new TestHistogram();

        reporter.NotifyOfAddedMetric(counter, "c", group);
        reporter.NotifyOfAddedMetric(gauge, "g", group);
        reporter.NotifyOfAddedMetric(meter, "m", group);
        reporter.NotifyOfAddedMetric(histogram, "h", group);

        // UnregisteredMetricsGroup ignores the character filter, like in Java
        Assert.Equal("c", Assert.Single(reporter.CounterNames));
        Assert.Equal("g", Assert.Single(reporter.GaugeNames));
        Assert.Equal("m", Assert.Single(reporter.MeterNames));
        Assert.Equal("h", Assert.Single(reporter.HistogramNames));

        reporter.NotifyOfRemovedMetric(counter, "c", group);
        reporter.NotifyOfRemovedMetric(gauge, "g", group);
        reporter.NotifyOfRemovedMetric(meter, "m", group);
        reporter.NotifyOfRemovedMetric(histogram, "h", group);

        Assert.Empty(reporter.CounterNames);
        Assert.Empty(reporter.GaugeNames);
        Assert.Empty(reporter.MeterNames);
        Assert.Empty(reporter.HistogramNames);
    }

    /// <summary>Metric registration synchronizes on the lock that subclasses use while
    /// enumerating, like Java's synchronized (this) on the reporter instance.</summary>
    [Fact]
    public async Task TestSubclassesShareTheMetricsLock()
    {
        var reporter = new TestReporter();
        IMetricGroup group = new UnregisteredMetricsGroup();
        using var locked = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();

        var holder = new Thread(() =>
        {
            lock (reporter.Lock)
            {
                locked.Set();
                release.Wait();
            }
        });
        holder.Start();
        locked.Wait();

        Task add = Task.Run(() => reporter.NotifyOfAddedMetric(new SimpleCounter(), "c", group));
        Task first = await Task.WhenAny(add, Task.Delay(TimeSpan.FromMilliseconds(200)));
        Assert.NotSame(add, first);

        release.Set();
        await add.WaitAsync(TimeSpan.FromSeconds(10));
        holder.Join();
        Assert.Equal("c", Assert.Single(reporter.CounterNames));
    }

    private sealed class ConstantGauge<T>(T value) : IGauge<T>
    {
        public T GetValue() => value;
    }

    private sealed class TestReporter : AbstractReporter
    {
        public IEnumerable<string> CounterNames => Counters.Values;

        public IEnumerable<string> GaugeNames => Gauges.Values;

        public IEnumerable<string> MeterNames => Meters.Values;

        public IEnumerable<string> HistogramNames => Histograms.Values;

        public object Lock => SyncRoot;

        public override void Open(MetricConfig config)
        {
        }

        public override void Close()
        {
        }

        public override string FilterCharacters(string input) => input;
    }

    private sealed class TestHistogram : IHistogram
    {
        private long _count;

        public void Update(long value) => _count++;

        public long Count => _count;

        public HistogramStatistics GetStatistics() => throw new NotSupportedException();
    }
}
