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
/// The <c>LocalDataInputStream</c> class is a wrapper class for a data input stream to the local
/// file system.
/// </summary>
[Internal]
public class LocalDataInputStream : FSDataInputStream
{
    private readonly FileStream _fis;

    /// <summary>
    /// Constructs a new <c>LocalDataInputStream</c> object from a given file.
    /// </summary>
    /// <param name="filePath">the file this data input stream reads from</param>
    public LocalDataInputStream(string filePath)
    {
        _fis = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    }

    public override void Seek(long desired) => _fis.Seek(desired, SeekOrigin.Begin);

    public override long GetPos() => _fis.Position;

    public override int Read(byte[] buffer, int offset, int count) =>
        _fis.Read(buffer, offset, count);

    public override int ReadByte() => _fis.ReadByte();

    public override bool CanRead => _fis.CanRead;

    public override bool CanSeek => _fis.CanSeek;

    public override bool CanWrite => false;

    public override long Length => _fis.Length;

    public override long Position
    {
        get => _fis.Position;
        set => _fis.Position = value;
    }

    public override void Flush() => _fis.Flush();

    public override long Seek(long offset, SeekOrigin origin) => _fis.Seek(offset, origin);

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fis.Dispose();
        }
        base.Dispose(disposing);
    }
}
