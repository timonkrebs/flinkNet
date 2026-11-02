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
/// Extension for metric groups that support logical scopes.
/// </summary>
[Public]
public interface ILogicalScopeProvider
{
    /// <summary>Returns the logical scope for the metric group, for example
    /// <c>"taskmanager.job.task"</c>, with the given filter applied to all scope components.</summary>
    string GetLogicalScope(ICharacterFilter filter);

    /// <summary>Returns the logical scope for the metric group with the given filter and
    /// delimiter.</summary>
    string GetLogicalScope(ICharacterFilter filter, char delimiter);

    /// <summary>Returns the underlying metric group.</summary>
    IMetricGroup GetWrappedMetricGroup();

    /// <summary>
    /// Casts the given metric group to a <see cref="ILogicalScopeProvider"/>, if it implements
    /// the interface.
    /// </summary>
    /// <param name="metricGroup">metric group to cast</param>
    static ILogicalScopeProvider CastFrom(IMetricGroup metricGroup) =>
        metricGroup as ILogicalScopeProvider
            ?? throw new InvalidOperationException(
                "The given metric group does not implement the LogicalScopeProvider interface.");
}
