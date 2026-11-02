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
using FlinkNet.Api.Common.Watermarks;
using FlinkNet.DataStream.Api.Common;
using FlinkNet.DataStream.Api.Context;

namespace FlinkNet.DataStream.Api.Function;

/// <summary>This contains all logic related to processing records from a single input.</summary>
[Experimental]
public interface IOneInputStreamProcessFunction<in TIn, TOut> : IProcessFunction
{
    /// <summary>
    /// Initialization method for the function. It is called before the actual working methods.
    /// </summary>
    void Open(INonPartitionedContext<TOut> ctx)
    {
    }

    /// <summary>Process the record and emit data through <see cref="ICollector{TOut}"/>.</summary>
    /// <param name="record">to process.</param>
    /// <param name="output">to emit processed records.</param>
    /// <param name="ctx">runtime context in which this function is executed.</param>
    void ProcessRecord(TIn record, ICollector<TOut> output, IPartitionedContext<TOut> ctx);

    /// <summary>
    /// This is a life-cycle method indicates that this function will no longer receive any input
    /// data.
    /// </summary>
    /// <param name="ctx">the context in which this function is executed.</param>
    void EndInput(INonPartitionedContext<TOut> ctx)
    {
    }

    /// <summary>Callback for processing timer.</summary>
    /// <param name="timestamp">when this callback is triggered.</param>
    /// <param name="output">to emit records.</param>
    /// <param name="ctx">runtime context in which this function is executed.</param>
    void OnProcessingTimer(long timestamp, ICollector<TOut> output, IPartitionedContext<TOut> ctx)
    {
    }

    /// <summary>Callback function when receiving a watermark.</summary>
    WatermarkHandlingResult OnWatermark(
        IWatermark watermark, ICollector<TOut> output, INonPartitionedContext<TOut> ctx) =>
        WatermarkHandlingResult.Peek;
}
