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
using FlinkNet.DataStream.Api.Function;

namespace FlinkNet.DataStream.Api.Extension.EventTime.Strategy;

/// <summary>
/// A builder to create a process function responsible for extracting event time and generating
/// event time watermarks.
/// </summary>
[Experimental]
public class EventTimeWatermarkGeneratorBuilder<T>
{
    // how to extract event time from event
    private readonly IEventTimeExtractor<T> _eventTimeExtractor;

    // what frequency to generate event time watermark
    private EventTimeWatermarkGenerateMode _generateMode =
        EventTimeWatermarkGenerateMode.Periodic;

    // if not set, it will default to the value of the "pipeline.auto-watermark-interval"
    // configuration.
    private TimeSpan _periodicWatermarkInterval = TimeSpan.Zero;

    // if set to zero, it will not generate idle status watermark
    private TimeSpan _idleTimeout = TimeSpan.Zero;

    // max out-of-order time
    private TimeSpan _maxOutOfOrderTime = TimeSpan.Zero;

    public EventTimeWatermarkGeneratorBuilder(IEventTimeExtractor<T> eventTimeExtractor)
    {
        _eventTimeExtractor = eventTimeExtractor;
    }

    public EventTimeWatermarkGeneratorBuilder<T> WithIdleness(TimeSpan idleTimeout)
    {
        _idleTimeout = idleTimeout;
        return this;
    }

    public EventTimeWatermarkGeneratorBuilder<T> WithMaxOutOfOrderTime(TimeSpan maxOutOfOrderTime)
    {
        _maxOutOfOrderTime = maxOutOfOrderTime;
        return this;
    }

    public EventTimeWatermarkGeneratorBuilder<T> NoWatermark()
    {
        _generateMode = EventTimeWatermarkGenerateMode.NoWatermark;
        return this;
    }

    public EventTimeWatermarkGeneratorBuilder<T> PeriodicWatermark()
    {
        _generateMode = EventTimeWatermarkGenerateMode.Periodic;
        return this;
    }

    public EventTimeWatermarkGeneratorBuilder<T> PeriodicWatermark(TimeSpan periodicWatermarkInterval)
    {
        _generateMode = EventTimeWatermarkGenerateMode.Periodic;
        _periodicWatermarkInterval = periodicWatermarkInterval;
        return this;
    }

    public EventTimeWatermarkGeneratorBuilder<T> PerEventWatermark()
    {
        _generateMode = EventTimeWatermarkGenerateMode.PerEvent;
        return this;
    }

    /// <summary>Builds the watermark generator as a process function.</summary>
    public IOneInputStreamProcessFunction<T, T> BuildAsProcessFunction()
    {
        var watermarkStrategy =
            new EventTimeWatermarkStrategy<T>(
                _eventTimeExtractor,
                _generateMode,
                _periodicWatermarkInterval,
                _idleTimeout,
                _maxOutOfOrderTime);
        return EventTimeExtension.BuildAsProcessFunction(watermarkStrategy);
    }
}
