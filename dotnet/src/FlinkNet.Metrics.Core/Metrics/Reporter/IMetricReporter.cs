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
/// Metric reporters are used to export <see cref="IMetric"/>s to an external backend.
///
/// <para>Reporters are instantiated generically and must have a public, parameterless
/// constructor.</para>
/// </summary>
[Public]
public interface IMetricReporter : IReporter
{
    // ------------------------------------------------------------------------
    //  adding / removing metrics
    // ------------------------------------------------------------------------

    /// <summary>Called when a new <see cref="IMetric"/> was added.</summary>
    /// <param name="metric">the metric that was added</param>
    /// <param name="metricName">the name of the metric</param>
    /// <param name="group">the group that contains the metric</param>
    void NotifyOfAddedMetric(IMetric metric, string metricName, IMetricGroup group);

    /// <summary>Called when a <see cref="IMetric"/> was removed.</summary>
    /// <param name="metric">the metric that should be removed</param>
    /// <param name="metricName">the name of the metric</param>
    /// <param name="group">the group that contains the metric</param>
    void NotifyOfRemovedMetric(IMetric metric, string metricName, IMetricGroup group);
}
