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
/// The class <c>LocalFileStatus</c> provides an implementation of the <see cref="IFileStatus"/>
/// interface for the local file system.
/// </summary>
[Internal]
public class LocalFileStatus : IFileStatus
{
    /// <summary>The file/directory this status belongs to.</summary>
    private readonly FileSystemInfo _file;

    /// <summary>The path of this status.</summary>
    private readonly Path _path;

    /// <summary>
    /// Creates a <c>LocalFileStatus</c> object from a given <see cref="FileSystemInfo"/> object.
    /// </summary>
    /// <param name="f">the file/directory object this status belongs to</param>
    /// <param name="fs">the file system of the corresponding file</param>
    public LocalFileStatus(FileSystemInfo f, FileSystem fs)
    {
        _file = f;
        _path = new Path(fs.GetUri().Scheme + ":" + f.FullName.Replace('\\', '/'));
    }

    public long AccessTime => new DateTimeOffset(_file.LastAccessTimeUtc).ToUnixTimeMilliseconds();

    public long BlockSize => Len;

    public long Len => _file is FileInfo info ? info.Length : 0;

    public long ModificationTime =>
        new DateTimeOffset(_file.LastWriteTimeUtc).ToUnixTimeMilliseconds();

    public short Replication => 1;

    public bool IsDir => _file is DirectoryInfo;

    public Path Path => _path;

    public FileSystemInfo File => _file;

    public override string ToString() => "LocalFileStatus{file=" + _file.FullName + "}";
}
