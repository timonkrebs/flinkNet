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
/// The <c>LocalDataOutputStream</c> class is a wrapper class for a data output stream to the
/// local file system.
/// </summary>
[Internal]
public class LocalDataOutputStream : FSDataOutputStream
{
    private readonly FileStream _fos;

    /// <summary>
    /// Constructs a new <c>LocalDataOutputStream</c> object from a given file.
    /// </summary>
    /// <param name="filePath">the file this data output stream writes to</param>
    public LocalDataOutputStream(string filePath)
    {
        _fos = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
    }

    public override long GetPos() => _fos.Position;

    public override void Sync() => _fos.Flush(flushToDisk: true);

    public override void Write(byte[] buffer, int offset, int count) =>
        _fos.Write(buffer, offset, count);

    public override void WriteByte(byte value) => _fos.WriteByte(value);

    public override void Flush() => _fos.Flush();

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => _fos.CanWrite;

    public override long Length => _fos.Length;

    public override long Position
    {
        get => _fos.Position;
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fos.Dispose();
        }
        base.Dispose(disposing);
    }
}
