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
/// InputSplitSources create <see cref="IInputSplit"/>s that define portions of data to be
/// produced by input formats.
/// </summary>
/// <typeparam name="T">The type of the input splits created by the source.</typeparam>
[Public]
public interface IInputSplitSource<T>
    where T : IInputSplit
{
    /// <summary>Computes the input splits. The given minimum number of splits is a hint as to
    /// how many splits are desired.</summary>
    /// <param name="minNumSplits">Number of minimal input splits, as a hint.</param>
    T[] CreateInputSplits(int minNumSplits);

    /// <summary>Returns the assigner for the input splits.</summary>
    IInputSplitAssigner GetInputSplitAssigner(T[] inputSplits);
}
