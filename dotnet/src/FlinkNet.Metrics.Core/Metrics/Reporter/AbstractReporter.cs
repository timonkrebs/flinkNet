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

namespace FlinkNet.Metrics.Reporter;

/// <summary>
/// Base interface for custom metric reporters.
///
/// <para>PORT NOTE: Java logs a warning for unknown metric types via SLF4J; the port throws
/// nothing and ignores unknown types silently until a logging abstraction is introduced.</para>
/// </summary>
[Public]
public abstract class AbstractReporter : IMetricReporter, ICharacterFilter
{
    protected readonly Dictionary<IMetric, string> Gauges = new();
    protected readonly Dictionary<ICounter, string> Counters = new();
    protected readonly Dictionary<IHistogram, string> Histograms = new();
    protected readonly Dictionary<IMeter, string> Meters = new();

    private readonly object _lock = new();

    public abstract void Open(MetricConfig config);

    public abstract void Close();

    public abstract string FilterCharacters(string input);

    public void NotifyOfAddedMetric(IMetric metric, string metricName, IMetricGroup group)
    {
        string name = group.GetMetricIdentifier(metricName, this);
        lock (_lock)
        {
            switch (metric.GetMetricType())
            {
                case MetricType.Counter:
                    Counters[(ICounter)metric] = name;
                    break;
                case MetricType.Gauge:
                    Gauges[metric] = name;
                    break;
                case MetricType.Histogram:
                    Histograms[(IHistogram)metric] = name;
                    break;
                case MetricType.Meter:
                    Meters[(IMeter)metric] = name;
                    break;
                default:
                    // unknown metric type: ignored (Java logs a warning)
                    break;
            }
        }
    }

    public void NotifyOfRemovedMetric(IMetric metric, string metricName, IMetricGroup group)
    {
        lock (_lock)
        {
            switch (metric.GetMetricType())
            {
                case MetricType.Counter:
                    Counters.Remove((ICounter)metric);
                    break;
                case MetricType.Gauge:
                    Gauges.Remove(metric);
                    break;
                case MetricType.Histogram:
                    Histograms.Remove((IHistogram)metric);
                    break;
                case MetricType.Meter:
                    Meters.Remove((IMeter)metric);
                    break;
                default:
                    // unknown metric type: ignored (Java logs a warning)
                    break;
            }
        }
    }
}
