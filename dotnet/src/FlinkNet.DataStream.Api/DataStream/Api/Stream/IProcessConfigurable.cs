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
using FlinkNet.Api.Common;

namespace FlinkNet.DataStream.Api.Stream;

/// <summary>
/// The non-generic base of <see cref="IProcessConfigurable{T}"/>.
///
/// <para>PORT NOTE: stands in for Java's <c>ProcessConfigurable&lt;?&gt;</c> wildcard, which C#
/// cannot express for a self-referential generic.</para>
/// </summary>
[Experimental]
public interface IProcessConfigurable
{
    IProcessConfigurable WithUid(string uid);

    IProcessConfigurable WithName(string name);

    IProcessConfigurable WithParallelism(int parallelism);

    IProcessConfigurable WithMaxParallelism(int maxParallelism);

    IProcessConfigurable WithSlotSharingGroup(SlotSharingGroup slotSharingGroup);
}

/// <summary>
/// This represents the handle of a processing operation, which can be used to configure the
/// operation, such as its parallelism.
/// </summary>
/// <typeparam name="T">the concrete configurable type, for fluent chaining.</typeparam>
[Experimental]
public interface IProcessConfigurable<T> : IProcessConfigurable
    where T : IProcessConfigurable<T>
{
    /// <summary>
    /// Sets an ID for this operator. The specified ID is used to assign the same operator ID
    /// across job submissions (for example when starting a job from a savepoint). Important: this
    /// ID needs to be unique per transformation and job. Otherwise, job submission will fail.
    /// </summary>
    /// <param name="uid">The unique user-specified ID of this transformation.</param>
    /// <returns>The operator with the specified ID.</returns>
    new T WithUid(string uid);

    /// <summary>Sets the name of the current data stream. This name is used by the visualization
    /// and logging during runtime.</summary>
    /// <returns>The named operator.</returns>
    new T WithName(string name);

    /// <summary>Sets the parallelism for this operator.</summary>
    /// <param name="parallelism">The parallelism for this operator.</param>
    /// <returns>The operator with set parallelism.</returns>
    new T WithParallelism(int parallelism);

    /// <summary>Sets the maximum parallelism of this operator.</summary>
    /// <param name="maxParallelism">Maximum parallelism.</param>
    /// <returns>The operator with set maximum parallelism.</returns>
    new T WithMaxParallelism(int maxParallelism);

    /// <summary>Sets the slot sharing group of this operation. Parallel instances of operations
    /// that are in the same slot sharing group will be co-located in the same TaskManager slot,
    /// if possible.</summary>
    /// <param name="slotSharingGroup">Which contains name and its resource spec.</param>
    new T WithSlotSharingGroup(SlotSharingGroup slotSharingGroup);

    IProcessConfigurable IProcessConfigurable.WithUid(string uid) => WithUid(uid);

    IProcessConfigurable IProcessConfigurable.WithName(string name) => WithName(name);

    IProcessConfigurable IProcessConfigurable.WithParallelism(int parallelism) =>
        WithParallelism(parallelism);

    IProcessConfigurable IProcessConfigurable.WithMaxParallelism(int maxParallelism) =>
        WithMaxParallelism(maxParallelism);

    IProcessConfigurable IProcessConfigurable.WithSlotSharingGroup(
        SlotSharingGroup slotSharingGroup) =>
        WithSlotSharingGroup(slotSharingGroup);
}
