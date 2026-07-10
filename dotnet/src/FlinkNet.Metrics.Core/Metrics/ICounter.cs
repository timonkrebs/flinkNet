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

/// <summary>A Counter is a <see cref="IMetric"/> that measures a count.</summary>
[Public]
public interface ICounter : IMetric
{
    /// <summary>Increment the current count by 1.</summary>
    void Inc();

    /// <summary>Increment the current count by the given value.</summary>
    /// <param name="n">value to increment the current count by</param>
    void Inc(long n);

    /// <summary>Decrement the current count by 1.</summary>
    void Dec();

    /// <summary>Decrement the current count by the given value.</summary>
    /// <param name="n">value to decrement the current count by</param>
    void Dec(long n);

    /// <summary>Returns the current count.</summary>
    long Count { get; }

    MetricType IMetric.GetMetricType() => MetricType.Counter;
}
