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
/// Histogram statistics represent the current snapshot of elements recorded in the histogram.
///
/// <para>The histogram statistics allow calculating values for quantiles, the mean, the standard
/// deviation, the minimum and the maximum.</para>
/// </summary>
[Public]
public abstract class HistogramStatistics
{
    /// <summary>Returns the value for the given quantile based on the represented histogram
    /// statistics.</summary>
    /// <param name="quantile">Quantile to calculate the value for</param>
    /// <returns>Value for the given quantile</returns>
    public abstract double GetQuantile(double quantile);

    /// <summary>Returns the elements of the statistics' sample.</summary>
    public abstract long[] Values { get; }

    /// <summary>Returns the size of the statistics' sample.</summary>
    public abstract int Size { get; }

    /// <summary>Returns the mean of the histogram values.</summary>
    public abstract double Mean { get; }

    /// <summary>Returns the standard deviation of the distribution reflected by the histogram
    /// statistics.</summary>
    public abstract double StdDev { get; }

    /// <summary>Returns the maximum value of the histogram.</summary>
    public abstract long Max { get; }

    /// <summary>Returns the minimum value of the histogram.</summary>
    public abstract long Min { get; }
}
