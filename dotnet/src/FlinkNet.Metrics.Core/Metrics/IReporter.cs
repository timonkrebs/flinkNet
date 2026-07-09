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

/// <summary>Parent interface to all reporters, defining their common lifecycle.</summary>
[Public]
public interface IReporter
{
    /// <summary>
    /// Configures this reporter.
    ///
    /// <para>If the reporter was instantiated generically and hence parameter-less, this method
    /// is the place where the reporter sets its basic fields based on configuration values.
    /// This method is always called first on a newly instantiated reporter.</para>
    /// </summary>
    /// <param name="config">A properties object that contains all parameters set for this
    /// reporter.</param>
    void Open(MetricConfig config);

    /// <summary>Closes this reporter. Should be used to close channels, streams and release
    /// resources.</summary>
    void Close();
}
