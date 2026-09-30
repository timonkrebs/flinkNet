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

namespace FlinkNet.Core.Fs.Local;

/// <summary>
/// Implementation of the <see cref="IBlockLocation"/> interface for a local file system.
///
/// <para>Local files have only one block that represents the entire file. The block has no
/// location information, because it is not accessible where the files (or their block) actually
/// reside, especially in cases where the files are on a mounted file system.</para>
/// </summary>
[Internal]
public class LocalBlockLocation : IBlockLocation
{
    private readonly long _length;

    public LocalBlockLocation(long length)
    {
        _length = length;
    }

    public string[] GetHosts() => [];

    public long Length => _length;

    public long Offset => 0;

    public int CompareTo(IBlockLocation? other) => 0;
}
