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
/// A factory for <see cref="IMetricReporter"/>s.
///
/// <para>PORT NOTE: Java factories receive <c>java.util.Properties</c> and are discovered via
/// service loading; the port passes a <see cref="MetricConfig"/> and registration happens
/// explicitly until the plugin mechanism lands.</para>
/// </summary>
[Public]
public interface IMetricReporterFactory
{
    /// <summary>Creates a new metric reporter.</summary>
    /// <param name="properties">the properties for the reporter</param>
    /// <returns>the created metric reporter</returns>
    IMetricReporter CreateMetricReporter(MetricConfig properties);
}
