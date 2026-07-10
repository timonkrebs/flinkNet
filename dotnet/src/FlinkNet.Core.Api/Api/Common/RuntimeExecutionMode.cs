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

namespace FlinkNet.Api.Common;

/// <summary>
/// Runtime execution mode of DataStream programs. Among other things, this controls task
/// scheduling, network shuffle behavior, and time semantics. Some operations will also change
/// their record emission behaviour based on the configured execution mode.
/// </summary>
/// <seealso href="https://cwiki.apache.org/confluence/display/FLINK/FLIP-134%3A+Batch+execution+for+the+DataStream+API">
/// FLIP-134: Batch execution for the DataStream API</seealso>
[PublicEvolving]
public enum RuntimeExecutionMode
{
    /// <summary>
    /// The Pipeline will be executed with Streaming Semantics. All tasks will be deployed before
    /// execution starts, checkpoints will be enabled, and both processing and event time will be
    /// fully supported.
    /// </summary>
    Streaming,

    /// <summary>
    /// The Pipeline will be executed with Batch Semantics. Tasks will be scheduled gradually based
    /// on the scheduling region they belong, shuffles between regions will be blocking, watermarks
    /// are assumed to be "perfect" i.e. no late data, and processing time is assumed to not advance
    /// during execution.
    /// </summary>
    Batch,

    /// <summary>
    /// Flink will set the execution mode to <see cref="Batch"/> if all sources are bounded, or
    /// <see cref="Streaming"/> if there is at least one source which is unbounded.
    /// </summary>
    Automatic
}
