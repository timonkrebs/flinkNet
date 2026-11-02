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

namespace FlinkNet.DataStream.Api.Common;

/// <summary>This interface is responsible for collecting data to the output stream.</summary>
[Experimental]
public interface ICollector<in TOut>
{
    /// <summary>Collect record to output stream.</summary>
    /// <param name="record">to be collected.</param>
    void Collect(TOut record);

    /// <summary>Overwrite the timestamp of this record and collect it to the output stream.</summary>
    /// <param name="record">to be collected.</param>
    /// <param name="timestamp">of the processed data.</param>
    void CollectAndOverwriteTimestamp(TOut record, long timestamp);
}
