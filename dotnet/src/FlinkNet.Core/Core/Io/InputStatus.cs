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
/// An <see cref="InputStatus"/> indicates the availability of data from an asynchronous input.
/// When asking an asynchronous input to produce data, it returns this status to indicate how to
/// proceed.
/// </summary>
[PublicEvolving]
public enum InputStatus
{
    /// <summary>Indicator that more data is available and the input can be called immediately
    /// again to produce more data.</summary>
    MoreAvailable,

    /// <summary>Indicator that no data is currently available, but more data will be available
    /// in the future again.</summary>
    NothingAvailable,

    /// <summary>Indicator that the input has reached the end of data.</summary>
    EndOfInput,
}
