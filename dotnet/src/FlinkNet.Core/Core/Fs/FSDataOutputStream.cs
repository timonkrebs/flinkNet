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

namespace FlinkNet.Core.Fs;

/// <summary>
/// An output stream to a file that is created via a <see cref="FileSystem"/>. The data written
/// is only visible and durable after the stream is closed (and possibly <see cref="Sync"/>-ed).
/// </summary>
[Public]
public abstract class FSDataOutputStream : Stream
{
    /// <summary>Gets the position of the stream (non-negative), defined as the number of bytes
    /// from the beginning of the file to the current writing position.</summary>
    public abstract long GetPos();

    /// <summary>
    /// Flushes the data all the way to the persistent non-volatile storage (for example disks).
    /// The method behaves similar to the <i>fsync</i> function, forcing all data to be persistent
    /// on the devices.
    /// </summary>
    public abstract void Sync();
}
