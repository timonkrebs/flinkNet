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

namespace FlinkNet.Core.Io;

/// <summary>
/// An input split assigner distributes the <see cref="IInputSplit"/>s among the instances on
/// which a data source exists.
/// </summary>
[PublicEvolving]
public interface IInputSplitAssigner
{
    /// <summary>Returns the next input split that shall be consumed, or null if no split remains.</summary>
    /// <param name="host">the host address of the split requester</param>
    /// <param name="taskId">the id of the requesting task</param>
    IInputSplit? GetNextInputSplit(string host, int taskId);

    /// <summary>Return the splits to the assigner. This happens when a task failed and the splits
    /// it was assigned have to be processed by another task.</summary>
    /// <param name="splits">the splits to return</param>
    /// <param name="taskId">the id of the task that failed</param>
    void ReturnInputSplit(IList<IInputSplit> splits, int taskId);
}
