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

/// <summary>A BlockLocation lists hosts, offset and length of block.</summary>
[Public]
public interface IBlockLocation : IComparable<IBlockLocation>
{
    /// <summary>Gets the list of hosts (hostname) hosting this block.</summary>
    string[] GetHosts();

    /// <summary>Gets the start offset of the file associated with this block.</summary>
    long Offset { get; }

    /// <summary>Gets the length of the block.</summary>
    long Length { get; }
}
