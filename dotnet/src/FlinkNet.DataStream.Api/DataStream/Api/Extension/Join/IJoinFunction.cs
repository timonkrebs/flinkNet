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

namespace FlinkNet.DataStream.Api.Extension.Join;

/// <summary>
/// A function that processes two joined records and produces the join result.
/// </summary>
[Experimental]
public interface IJoinFunction<in TIn1, in TIn2, TOut> : IFunction
{
    /// <summary>Performs the join and emits results through the collector.</summary>
    /// <param name="leftRecord">the record from the left input.</param>
    /// <param name="rightRecord">the record from the right input.</param>
    /// <param name="output">to emit joined records.</param>
    /// <param name="ctx">runtime context in which this function is executed.</param>
    void ProcessRecord(
        TIn1 leftRecord, TIn2 rightRecord, ICollector<TOut> output, IRuntimeContext ctx);
}
