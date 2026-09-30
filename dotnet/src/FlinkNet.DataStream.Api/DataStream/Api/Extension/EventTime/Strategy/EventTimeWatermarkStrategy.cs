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

namespace FlinkNet.DataStream.Api.Extension.EventTime.Strategy;

/// <summary>Component which encapsulates the logic of how and when to extract event time and
/// watermarks.</summary>
[Experimental]
public class EventTimeWatermarkStrategy<T>
{
    // how to extract event time from event
    private readonly IEventTimeExtractor<T> _eventTimeExtractor;

    // what frequency to generate event time watermark
    private readonly EventTimeWatermarkGenerateMode _generateMode;

    // if not set, it will default to the value of the "pipeline.auto-watermark-interval"
    // configuration.
    private readonly TimeSpan _periodicWatermarkInterval;

    // if set to zero, it will not generate idle status watermark
    private readonly TimeSpan _idleTimeout;

    // max out-of-order time
    private readonly TimeSpan _maxOutOfOrderTime;

    public EventTimeWatermarkStrategy(IEventTimeExtractor<T> eventTimeExtractor)
        : this(
            eventTimeExtractor,
            EventTimeWatermarkGenerateMode.Periodic,
            TimeSpan.Zero,
            TimeSpan.Zero,
            TimeSpan.Zero)
    {
    }

    public EventTimeWatermarkStrategy(
        IEventTimeExtractor<T> eventTimeExtractor,
        EventTimeWatermarkGenerateMode generateMode,
        TimeSpan periodicWatermarkInterval,
        TimeSpan idleTimeout,
        TimeSpan maxOutOfOrderTime)
    {
        _eventTimeExtractor = eventTimeExtractor;
        _generateMode = generateMode;
        _periodicWatermarkInterval = periodicWatermarkInterval;
        _idleTimeout = idleTimeout;
        _maxOutOfOrderTime = maxOutOfOrderTime;
    }

    public IEventTimeExtractor<T> EventTimeExtractor => _eventTimeExtractor;

    public EventTimeWatermarkGenerateMode GenerateMode => _generateMode;

    public TimeSpan PeriodicWatermarkInterval => _periodicWatermarkInterval;

    public TimeSpan IdleTimeout => _idleTimeout;

    public TimeSpan MaxOutOfOrderTime => _maxOutOfOrderTime;

}

/// <summary>
/// The generate mode of event time watermark. (PORT NOTE: hoisted from Java's nested enum — a
/// type nested in a C# generic class would be instantiated per type argument.)
/// </summary>
[Internal]
public enum EventTimeWatermarkGenerateMode
{
    NoWatermark,
    Periodic,
    PerEvent,
}
