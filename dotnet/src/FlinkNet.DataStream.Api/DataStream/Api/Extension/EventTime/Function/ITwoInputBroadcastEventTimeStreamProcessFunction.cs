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
using FlinkNet.DataStream.Api.Common;
using FlinkNet.DataStream.Api.Context;
using FlinkNet.DataStream.Api.Function;

namespace FlinkNet.DataStream.Api.Extension.EventTime.Function;

/// <summary>The <see cref="ITwoInputBroadcastStreamProcessFunction{TIn1,TIn2,TOut}"/> that
/// extends with event time support.</summary>
[Experimental]
public interface ITwoInputBroadcastEventTimeStreamProcessFunction<in TIn1, in TIn2, TOut>
    : IEventTimeProcessFunction, ITwoInputBroadcastStreamProcessFunction<TIn1, TIn2, TOut>
{
    /// <summary>The callback invoked when the process function receives an event time
    /// watermark.</summary>
    void OnEventTimeWatermark(
        long watermarkTimestamp, ICollector<TOut> output, INonPartitionedContext<TOut> ctx)
    {
    }

    /// <summary>The callback invoked when an event timer fires.</summary>
    void OnEventTimer(long timestamp, ICollector<TOut> output, IPartitionedContext<TOut> ctx)
    {
    }
}
