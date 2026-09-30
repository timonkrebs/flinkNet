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

namespace FlinkNet.DataStream.Api.Extension.Window.Strategy;

/// <summary>
/// The base class of window strategies, which defines how to divide the records into finite
/// buckets (windows).
/// </summary>
[Experimental]
public class WindowStrategy
{
    public const TimeType ProcessingTime = TimeType.Processing;

    public const TimeType EventTime = TimeType.Event;

    /// <summary>The types of time used in window operations.</summary>
    [Experimental]
    public enum TimeType
    {
        Processing,
        Event,
    }

    // ============== global window ================

    /// <summary>Creates a WindowStrategy that assigns all records to the same global window.</summary>
    public static WindowStrategy Global() => new GlobalWindowStrategy();

    // ============== tumbling time window ================

    /// <summary>Creates a WindowStrategy that generates tumbling event-time windows.</summary>
    public static WindowStrategy Tumbling(TimeSpan windowSize) =>
        new TumblingTimeWindowStrategy(windowSize);

    /// <summary>Creates a WindowStrategy that generates tumbling windows of the given time type.</summary>
    public static WindowStrategy Tumbling(TimeSpan windowSize, TimeType timeType) =>
        new TumblingTimeWindowStrategy(windowSize, timeType);

    /// <summary>Creates a WindowStrategy that generates tumbling windows with allowed lateness.</summary>
    public static WindowStrategy Tumbling(
        TimeSpan windowSize, TimeType timeType, TimeSpan allowedLateness) =>
        new TumblingTimeWindowStrategy(windowSize, timeType, allowedLateness);

    // ============== sliding time window ================

    /// <summary>Creates a WindowStrategy that generates sliding event-time windows.</summary>
    public static WindowStrategy Sliding(TimeSpan windowSize, TimeSpan windowSlideInterval) =>
        new SlidingTimeWindowStrategy(windowSize, windowSlideInterval);

    /// <summary>Creates a WindowStrategy that generates sliding windows of the given time type.</summary>
    public static WindowStrategy Sliding(
        TimeSpan windowSize, TimeSpan windowSlideInterval, TimeType timeType) =>
        new SlidingTimeWindowStrategy(windowSize, windowSlideInterval, timeType);

    /// <summary>Creates a WindowStrategy that generates sliding windows with allowed lateness.</summary>
    public static WindowStrategy Sliding(
        TimeSpan windowSize,
        TimeSpan windowSlideInterval,
        TimeType timeType,
        TimeSpan allowedLateness) =>
        new SlidingTimeWindowStrategy(windowSize, windowSlideInterval, timeType, allowedLateness);

    // ============== session window ================

    /// <summary>Creates a WindowStrategy that generates event-time session windows.</summary>
    public static WindowStrategy Session(TimeSpan sessionGap) =>
        new SessionWindowStrategy(sessionGap);

    /// <summary>Creates a WindowStrategy that generates session windows of the given time type.</summary>
    public static WindowStrategy Session(TimeSpan sessionGap, TimeType timeType) =>
        new SessionWindowStrategy(sessionGap, timeType);
}
