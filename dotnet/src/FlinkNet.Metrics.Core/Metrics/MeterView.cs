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

namespace FlinkNet.Metrics;

/// <summary>
/// A MeterView provides an average rate of events per second over a given time period.
///
/// <para>The primary advantage of this class is that the rate is neither updated by the computing
/// thread nor for every event. Instead, a history of counts is maintained that is updated in
/// regular intervals by a background thread. From this history a rate is derived on demand, which
/// represents the average rate of events over the given time span.</para>
///
/// <para>Setting the time span to a low value reduces memory-consumption and will more accurately
/// report short-term changes. The minimum value possible is
/// <see cref="IView.UpdateIntervalSeconds"/>. A high value in turn increases memory-consumption,
/// since a longer history has to be maintained, but will result in smoother transitions between
/// rates.</para>
///
/// <para>The events are counted by a <see cref="ICounter"/>.</para>
/// </summary>
[Internal]
public class MeterView : IMeter, IView
{
    private const int DefaultTimeSpanInSeconds = 60;

    /// <summary>The underlying counter maintaining the count.</summary>
    private readonly ICounter _counter;

    /// <summary>The time-span over which the average is calculated.</summary>
    private readonly int _timeSpanInSeconds;

    /// <summary>Circular array containing the history of values.</summary>
    private readonly long[] _values;

    /// <summary>The index in the array for the current time.</summary>
    private int _time;

    /// <summary>The last rate we computed.</summary>
    private double _currentRate;

    public MeterView(int timeSpanInSeconds)
        : this(new SimpleCounter(), timeSpanInSeconds)
    {
    }

    public MeterView(ICounter counter)
        : this(counter, DefaultTimeSpanInSeconds)
    {
    }

    public MeterView(ICounter counter, int timeSpanInSeconds)
    {
        _counter = counter;
        // the time-span must be larger than the update-interval as otherwise the array has a
        // size of 1, for which no rate can be computed as no distinct before/after measurement
        // exists.
        _timeSpanInSeconds =
            Math.Max(
                timeSpanInSeconds - (timeSpanInSeconds % IView.UpdateIntervalSeconds),
                IView.UpdateIntervalSeconds);
        _values = new long[_timeSpanInSeconds / IView.UpdateIntervalSeconds + 1];
    }

    /// <summary>
    /// Creates a meter view backed by a number-valued gauge (port of Java's
    /// <c>MeterView(Gauge&lt;? extends Number&gt;)</c> constructor; C# constructors cannot be
    /// generic).
    /// </summary>
    public static MeterView ForGauge<T>(IGauge<T> numberGauge)
        where T : IConvertible =>
        new(new GaugeWrapper<T>(numberGauge));

    public void MarkEvent() => _counter.Inc();

    public void MarkEvent(long n) => _counter.Inc(n);

    public long Count => _counter.Count;

    public double Rate => _currentRate;

    public void Update()
    {
        _time = (_time + 1) % _values.Length;
        _values[_time] = _counter.Count;
        _currentRate =
            (double)(_values[_time] - _values[(_time + 1) % _values.Length]) / _timeSpanInSeconds;
    }

    /// <summary>Simple wrapper to expose number gauges as counters.</summary>
    private sealed class GaugeWrapper<T>(IGauge<T> numberGauge) : ICounter
        where T : IConvertible
    {
        public void Inc() => throw new NotSupportedException();

        public void Inc(long n) => throw new NotSupportedException();

        public void Dec() => throw new NotSupportedException();

        public void Dec(long n) => throw new NotSupportedException();

        // Java's Number.longValue() truncates floating values toward zero, where
        // IConvertible.ToInt64 would round to the nearest integer
        public long Count
        {
            get
            {
                T value = numberGauge.GetValue();
                return value.GetTypeCode() switch
                {
                    TypeCode.Single or TypeCode.Double or TypeCode.Decimal =>
                        (long)value.ToDouble(System.Globalization.CultureInfo.InvariantCulture),
                    _ => value.ToInt64(System.Globalization.CultureInfo.InvariantCulture),
                };
            }
        }
    }
}
