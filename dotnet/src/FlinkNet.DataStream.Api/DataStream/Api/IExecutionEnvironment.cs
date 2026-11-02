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
using FlinkNet.Api.Connector.DsV2;
using FlinkNet.DataStream.Api.Stream;

namespace FlinkNet.DataStream.Api;

/// <summary>
/// This is the context in which a program is executed.
///
/// <para>The environment provides methods to create a DataStream and control the job execution.</para>
/// </summary>
[Experimental]
public interface IExecutionEnvironment
{
    private static Func<IExecutionEnvironment>? factory;

    /// <summary>
    /// Registers the factory that <see cref="GetInstance"/> uses.
    ///
    /// <para>PORT NOTE: Java loads
    /// <c>org.apache.flink.datastream.impl.ExecutionEnvironmentImpl</c> reflectively; the port
    /// uses an explicit factory registration performed by the implementation module.</para>
    /// </summary>
    static void SetFactory(Func<IExecutionEnvironment> environmentFactory) =>
        factory = environmentFactory;

    /// <summary>Get the execution environment instance.</summary>
    /// <returns>A new instance of the execution environment.</returns>
    static IExecutionEnvironment GetInstance() =>
        (factory ?? throw new InvalidOperationException(
            "No ExecutionEnvironment factory is registered. The implementation module must call "
                + "IExecutionEnvironment.SetFactory before GetInstance is used."))();

    /// <summary>Execute and submit the job attached to this environment.</summary>
    /// <param name="jobName">to name the job.</param>
    void Execute(string jobName);

    /// <summary>Get the execution mode of this environment.</summary>
    RuntimeExecutionMode GetExecutionMode();

    /// <summary>Set the execution mode for this environment.</summary>
    /// <param name="runtimeMode">the execution mode.</param>
    /// <returns>this environment.</returns>
    IExecutionEnvironment SetExecutionMode(RuntimeExecutionMode runtimeMode);

    /// <summary>Add a collection of data as a source to the environment.</summary>
    /// <param name="source">to attach.</param>
    /// <param name="sourceName">the name of the source.</param>
    /// <returns>a stream of the source data.</returns>
    IProcessConfigurableAndNonKeyedPartitionStream<TOut> FromSource<TOut>(
        ISource<TOut> source, string sourceName);
}
