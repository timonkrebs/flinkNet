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
using FlinkNet.Core.Memory;

namespace FlinkNet.Core.Io;

/// <summary>
/// This is the abstract base class for <see cref="IIOReadableWritable"/> which allows to
/// differentiate between serialization versions. Concrete subclasses should typically override
/// the <see cref="Write"/> and <see cref="Read"/> methods, thereby calling base to ensure
/// version checking.
/// </summary>
[Internal]
public abstract class VersionedIOReadableWritable : IIOReadableWritable, IVersioned
{
    private int _readVersion = int.MinValue;

    public abstract int Version { get; }

    public virtual void Write(IDataOutputView output) => output.WriteInt(Version);

    public virtual void Read(IDataInputView input)
    {
        _readVersion = input.ReadInt();
        ResolveVersionRead(_readVersion);
    }

    /// <summary>
    /// Returns the found serialization version. If this instance was not read from serialized
    /// bytes but simply instantiated, then the current version is returned.
    /// </summary>
    public int GetReadVersion() => _readVersion == int.MinValue ? Version : _readVersion;

    /// <summary>
    /// Returns the compatible version values.
    ///
    /// <para>By default, the base implementation recognizes only the current version (identified
    /// by <see cref="Version"/>) as compatible. This method can be used as a hook and may be
    /// overridden to identify more compatible versions.</para>
    /// </summary>
    public virtual int[] GetCompatibleVersions() => [Version];

    /// <summary>Additional error detail appended when the read version is incompatible.
    /// PORT NOTE: Java's <c>Optional&lt;String&gt;</c> maps to a nullable string.</summary>
    public virtual string? GetAdditionalDetailsForIncompatibleVersion(int readVersion) => null;

    private void ResolveVersionRead(int readVersion)
    {
        int[] compatibleVersions = GetCompatibleVersions();
        foreach (int compatibleVersion in compatibleVersions)
        {
            if (compatibleVersion == readVersion)
            {
                return;
            }
        }

        string error =
            "Incompatible version: found " + readVersion + ", compatible versions are ["
                + string.Join(", ", compatibleVersions) + "]";

        string? incompatibleVersionError = GetAdditionalDetailsForIncompatibleVersion(readVersion);
        if (incompatibleVersionError != null)
        {
            error += ". " + incompatibleVersionError;
        }

        throw new VersionMismatchException(error);
    }
}
