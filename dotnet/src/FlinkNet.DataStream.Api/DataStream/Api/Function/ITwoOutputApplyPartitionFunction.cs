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
using FlinkNet.Api.Common.Functions;
using FlinkNet.DataStream.Api.Common;
using FlinkNet.DataStream.Api.Context;

namespace FlinkNet.DataStream.Api.Function;

/// <summary>A function to be applied to all partitions with two outputs.</summary>
[Experimental]
public interface ITwoOutputApplyPartitionFunction<TOut1, TOut2> : IFunction
{
    /// <summary>The actual method to be applied to each partition.</summary>
    /// <param name="firstOutput">to emit record to first output.</param>
    /// <param name="secondOutput">to emit record to second output.</param>
    /// <param name="ctx">runtime context in which this function is executed.</param>
    void Apply(
        ICollector<TOut1> firstOutput,
        ICollector<TOut2> secondOutput,
        ITwoOutputPartitionedContext<TOut1, TOut2> ctx);
}
