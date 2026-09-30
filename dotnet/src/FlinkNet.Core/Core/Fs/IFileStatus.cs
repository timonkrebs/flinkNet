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
/// Interface that represents the client side information for a file independent of the file
/// system.
/// </summary>
[Public]
public interface IFileStatus
{
    /// <summary>Returns the length of this file.</summary>
    long Len { get; }

    /// <summary>Gets the block size of the file.</summary>
    long BlockSize { get; }

    /// <summary>Gets the replication factor of a file.</summary>
    short Replication { get; }

    /// <summary>Gets the modification time of the file.</summary>
    long ModificationTime { get; }

    /// <summary>Gets the access time of the file.</summary>
    long AccessTime { get; }

    /// <summary>Checks if this object represents a directory.</summary>
    bool IsDir { get; }

    /// <summary>Returns the corresponding Path to the FileStatus.</summary>
    Path Path { get; }
}
