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

/// <summary>This contains all logic related to processing records from two non-broadcast
/// inputs.</summary>
[Experimental]
public interface ITwoInputNonBroadcastStreamProcessFunction<in TIn1, in TIn2, TOut> : IProcessFunction
{
    /// <summary>
    /// Initialization method for the function. It is called before the actual working methods.
    /// </summary>
    void Open(INonPartitionedContext<TOut> ctx)
    {
    }

    /// <summary>Process the record from the first input.</summary>
    void ProcessRecordFromFirstInput(
        TIn1 record, ICollector<TOut> output, IPartitionedContext<TOut> ctx);

    /// <summary>Process the record from the second input.</summary>
    void ProcessRecordFromSecondInput(
        TIn2 record, ICollector<TOut> output, IPartitionedContext<TOut> ctx);

    /// <summary>Indicates the first input has ended.</summary>
    void EndFirstInput(INonPartitionedContext<TOut> ctx)
    {
    }

    /// <summary>Indicates the second input has ended.</summary>
    void EndSecondInput(INonPartitionedContext<TOut> ctx)
    {
    }

    /// <summary>Callback for processing timer.</summary>
    void OnProcessingTimer(long timestamp, ICollector<TOut> output, IPartitionedContext<TOut> ctx)
    {
    }

    /// <summary>Callback when receiving a watermark from the first input.</summary>
    WatermarkHandlingResult OnWatermarkFromFirstInput(
        IWatermark watermark, ICollector<TOut> output, INonPartitionedContext<TOut> ctx) =>
        WatermarkHandlingResult.Peek;

    /// <summary>Callback when receiving a watermark from the second input.</summary>
    WatermarkHandlingResult OnWatermarkFromSecondInput(
        IWatermark watermark, ICollector<TOut> output, INonPartitionedContext<TOut> ctx) =>
        WatermarkHandlingResult.Peek;
}
