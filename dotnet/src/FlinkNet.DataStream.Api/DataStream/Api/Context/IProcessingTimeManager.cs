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

namespace FlinkNet.DataStream.Api.Context;

/// <summary>
/// This is responsibility for managing runtime information related to processing time of the
/// process function.
/// </summary>
[Experimental]
public interface IProcessingTimeManager
{
    /// <summary>Register a processing timer for this process function. onProcessingTimer method
    /// of this function will be invoked as timer goes off.</summary>
    /// <param name="timestamp">to trigger timer callback.</param>
    void RegisterTimer(long timestamp);

    /// <summary>Deletes the processing-time timer with the given trigger timestamp. This method
    /// has only an effect if such a timer was previously registered and did not already expire.</summary>
    /// <param name="timestamp">indicates the timestamp of the timer to delete.</param>
    void DeleteTimer(long timestamp);

    /// <summary>Get the current processing time.</summary>
    /// <returns>current processing time.</returns>
    long CurrentTime();
}
