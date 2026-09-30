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
/// A MetricGroup is a named container for <see cref="IMetric"/>s and further metric subgroups.
///
/// <para>Instances of this class can be used to register new metrics with Flink and to create a
/// nested hierarchy based on the group names. A MetricGroup is uniquely identified by its place
/// in the hierarchy and name.</para>
/// </summary>
[Public]
public interface IMetricGroup
{
    // ------------------------------------------------------------------------
    //  Metrics
    // ------------------------------------------------------------------------

    /// <summary>Creates and registers a new <see cref="ICounter"/> with Flink.</summary>
    /// <param name="name">name of the counter</param>
    /// <returns>the created counter</returns>
    ICounter Counter(string name);

    /// <summary>Registers a <see cref="ICounter"/> with Flink.</summary>
    /// <param name="name">name of the counter</param>
    /// <param name="counter">counter to register</param>
    /// <typeparam name="TCounter">counter type</typeparam>
    /// <returns>the given counter</returns>
    TCounter Counter<TCounter>(string name, TCounter counter)
        where TCounter : ICounter;

    /// <summary>Registers a new <see cref="IGauge{T}"/> with Flink.</summary>
    /// <param name="name">name of the gauge</param>
    /// <param name="gauge">gauge to register</param>
    /// <typeparam name="T">return type of the gauge</typeparam>
    /// <returns>the given gauge</returns>
    IGauge<T> Gauge<T>(string name, IGauge<T> gauge);

    /// <summary>Registers a new <see cref="IHistogram"/> with Flink.</summary>
    /// <param name="name">name of the histogram</param>
    /// <param name="histogram">histogram to register</param>
    /// <typeparam name="THistogram">histogram type</typeparam>
    /// <returns>the given histogram</returns>
    THistogram Histogram<THistogram>(string name, THistogram histogram)
        where THistogram : IHistogram;

    /// <summary>Registers a new <see cref="IMeter"/> with Flink.</summary>
    /// <param name="name">name of the meter</param>
    /// <param name="meter">meter to register</param>
    /// <typeparam name="TMeter">meter type</typeparam>
    /// <returns>the given meter</returns>
    TMeter Meter<TMeter>(string name, TMeter meter)
        where TMeter : IMeter;

    // ------------------------------------------------------------------------
    //  Groups
    // ------------------------------------------------------------------------

    /// <summary>Creates a new MetricGroup and adds it to this group's sub-groups.</summary>
    /// <param name="name">name of the group</param>
    /// <returns>the created group</returns>
    IMetricGroup AddGroup(string name);

    /// <summary>
    /// Creates a new key-value MetricGroup pair. The key group is added to this group's
    /// sub-groups, while the value group is added to the key group's sub-groups. This method
    /// returns the value group.
    ///
    /// <para>The only difference between calling this method and
    /// <c>group.AddGroup(key).AddGroup(value)</c> is that <see cref="GetAllVariables"/> of the
    /// value group return an additional <c>"&lt;key&gt;"="value"</c> pair.</para>
    /// </summary>
    /// <param name="key">name of the first group</param>
    /// <param name="value">name of the second group</param>
    /// <returns>the second created group</returns>
    IMetricGroup AddGroup(string key, string value);

    // ------------------------------------------------------------------------
    //  Scope
    // ------------------------------------------------------------------------

    /// <summary>Gets the scope as an array of the scope components, for example
    /// <c>["host-7", "taskmanager-2", "window_word_count", "my-mapper"]</c>.</summary>
    string[] GetScopeComponents();

    /// <summary>Returns a map of all variables and their associated value, for example
    /// <c>{"&lt;host&gt;"="host-7", "&lt;tm_id&gt;"="taskmanager-2"}</c>.</summary>
    IDictionary<string, string> GetAllVariables();

    /// <summary>Returns the fully qualified metric name, for example
    /// <c>"host-7.taskmanager-2.window_word_count.my-mapper.metricName"</c>.</summary>
    /// <param name="metricName">metric name</param>
    /// <returns>fully qualified metric name</returns>
    string GetMetricIdentifier(string metricName);

    /// <summary>Returns the fully qualified metric name using the given character filter.</summary>
    /// <param name="metricName">metric name</param>
    /// <param name="filter">character filter which is applied to the scope components</param>
    /// <returns>fully qualified metric name</returns>
    string GetMetricIdentifier(string metricName, ICharacterFilter filter);
}
