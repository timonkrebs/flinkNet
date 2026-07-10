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

/// <summary>This contains all logic related to processing and emitting records to two output
/// streams.</summary>
[Experimental]
public interface ITwoOutputStreamProcessFunction<in TIn, TOut1, TOut2> : IProcessFunction
{
    /// <summary>
    /// Initialization method for the function. It is called before the actual working methods.
    /// </summary>
    void Open(ITwoOutputNonPartitionedContext<TOut1, TOut2> ctx)
    {
    }

    /// <summary>
    /// Process and emit record to the first/second output through <see cref="ICollector{TOut}"/>s.
    /// </summary>
    /// <param name="record">to process.</param>
    /// <param name="output1">to emit processed records to the first output.</param>
    /// <param name="output2">to emit processed records to the second output.</param>
    /// <param name="ctx">runtime context in which this function is executed.</param>
    void ProcessRecord(
        TIn record,
        ICollector<TOut1> output1,
        ICollector<TOut2> output2,
        ITwoOutputPartitionedContext<TOut1, TOut2> ctx);

    /// <summary>
    /// This is a life-cycle method indicates that this function will no longer receive any input
    /// data.
    /// </summary>
    void EndInput(ITwoOutputNonPartitionedContext<TOut1, TOut2> ctx)
    {
    }

    /// <summary>Callback for processing timer.</summary>
    void OnProcessingTimer(
        long timestamp,
        ICollector<TOut1> output1,
        ICollector<TOut2> output2,
        ITwoOutputPartitionedContext<TOut1, TOut2> ctx)
    {
    }

    /// <summary>Callback function when receiving a watermark.</summary>
    WatermarkHandlingResult OnWatermark(
        IWatermark watermark,
        ICollector<TOut1> output1,
        ICollector<TOut2> output2,
        ITwoOutputNonPartitionedContext<TOut1, TOut2> ctx) =>
        WatermarkHandlingResult.Peek;
}
