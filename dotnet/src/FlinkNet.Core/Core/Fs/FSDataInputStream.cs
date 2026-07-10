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
/// Interface for a data input stream to a file on a <see cref="FileSystem"/>.
///
/// <para>This extends <see cref="Stream"/> with the Flink seek contract; implementations must
/// support seeking.</para>
/// </summary>
[Public]
public abstract class FSDataInputStream : Stream
{
    /// <summary>
    /// Seek to the given offset from the start of the file. The next read() will be from that
    /// location.
    /// </summary>
    /// <param name="desired">the desired offset</param>
    public abstract void Seek(long desired);

    /// <summary>Gets the current position in the input stream.</summary>
    /// <returns>current position in the input stream</returns>
    public abstract long GetPos();
}
