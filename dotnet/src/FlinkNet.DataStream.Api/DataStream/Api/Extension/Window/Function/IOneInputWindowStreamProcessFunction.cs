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
using FlinkNet.DataStream.Api.Extension.Window.Context;

namespace FlinkNet.DataStream.Api.Extension.Window.Function;

/// <summary>A type of <see cref="IWindowProcessFunction"/> for one-input window processing.</summary>
[Experimental]
public interface IOneInputWindowStreamProcessFunction<TIn, TOut> : IWindowProcessFunction
{
    /// <summary>
    /// This method will be invoked when a record is received. Its default behavior is to store
    /// data in the built-in window state.
    /// </summary>
    void OnRecord(
        TIn record,
        ICollector<TOut> output,
        IPartitionedContext<TOut> ctx,
        IOneInputWindowContext<TIn> windowContext) =>
        windowContext.PutRecord(record);

    /// <summary>
    /// This method will be invoked when the Window is triggered. You can obtain all the records
    /// in the Window through the <paramref name="windowContext"/>.
    /// </summary>
    void OnTrigger(
        ICollector<TOut> output,
        IPartitionedContext<TOut> ctx,
        IOneInputWindowContext<TIn> windowContext);

    /// <summary>
    /// Callback when a window is about to be cleaned up. It is the time to deletes any state in
    /// the <paramref name="windowContext"/> when the Window expires.
    /// </summary>
    void OnClear(
        ICollector<TOut> output,
        IPartitionedContext<TOut> ctx,
        IOneInputWindowContext<TIn> windowContext)
    {
    }

    /// <summary>This method will be invoked when a record is received after the window has been
    /// cleaned.</summary>
    void OnLateRecord(TIn record, ICollector<TOut> output, IPartitionedContext<TOut> ctx)
    {
    }
}
