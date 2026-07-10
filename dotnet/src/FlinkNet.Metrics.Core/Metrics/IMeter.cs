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

/// <summary>Metric for measuring throughput.</summary>
[Public]
public interface IMeter : IMetric
{
    /// <summary>Mark occurrence of an event.</summary>
    void MarkEvent();

    /// <summary>Mark occurrence of multiple events.</summary>
    /// <param name="n">number of occurred events</param>
    void MarkEvent(long n);

    /// <summary>Returns the current rate of events per second.</summary>
    double Rate { get; }

    /// <summary>Get number of events marked on the meter.</summary>
    long Count { get; }

    MetricType IMetric.GetMetricType() => MetricType.Meter;
}
