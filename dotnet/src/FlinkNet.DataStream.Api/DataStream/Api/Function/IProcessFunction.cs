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
using FlinkNet.Api.Common.State;
using FlinkNet.Api.Common.Watermarks;

namespace FlinkNet.DataStream.Api.Function;

/// <summary>Base class for all user defined process functions.</summary>
[Experimental]
public interface IProcessFunction : IFunction
{
    /// <summary>
    /// Explicitly declares states upfront. Each specific state must be declared in this method
    /// before it can be used.
    /// </summary>
    /// <returns>all declared states used by this process function.</returns>
    IReadOnlySet<IStateDeclaration> UsesStates() => new HashSet<IStateDeclaration>();

    /// <summary>
    /// Explicitly declares watermarks upfront. Each watermark must be declared in this method
    /// before it can be used.
    /// </summary>
    /// <returns>all declared watermarks used by this process function.</returns>
    IReadOnlySet<IWatermarkDeclaration> DeclareWatermarks() => new HashSet<IWatermarkDeclaration>();

    /// <summary>
    /// This method is invoked at the end of the lifecycle of this process function. It can be
    /// used for clean up work.
    /// </summary>
    void Close()
    {
    }
}
