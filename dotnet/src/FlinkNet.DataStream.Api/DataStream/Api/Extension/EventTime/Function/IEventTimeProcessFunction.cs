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
using FlinkNet.DataStream.Api.Extension.EventTime.Timer;
using FlinkNet.DataStream.Api.Function;

namespace FlinkNet.DataStream.Api.Extension.EventTime.Function;

/// <summary>
/// The base interface of all event-time process functions. Note that the event-time process
/// function should be used in conjunction with <see cref="EventTimeExtension"/>.
/// </summary>
[Experimental]
public interface IEventTimeProcessFunction : IProcessFunction
{
    /// <summary>
    /// Initialize the event-time process function, this method should be invoked before
    /// open method of process function. The <see cref="IEventTimeManager"/> can be used to
    /// register event timers and query the current event time.
    /// </summary>
    void InitEventTimeProcessFunction(IEventTimeManager eventTimeManager);
}
