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
using FlinkNet.DataStream.Api.Function;

namespace FlinkNet.DataStream.Api.Context;

/// <summary>
/// This interface represents the context associated with all operations that must be applied to
/// all partitions with two outputs.
/// </summary>
[Experimental]
public interface ITwoOutputNonPartitionedContext<TOut1, TOut2> : IRuntimeContext
{
    /// <summary>
    /// Apply a function to all partitions. For keyed stream, it will apply to all keys. For
    /// non-keyed stream, it will apply to the single partition.
    /// </summary>
    void ApplyToAllPartitions(
        ITwoOutputApplyPartitionFunction<TOut1, TOut2> applyPartitionFunction);

    /// <summary>Get the watermark manager of this process function.</summary>
    IWatermarkManager GetWatermarkManager();
}
