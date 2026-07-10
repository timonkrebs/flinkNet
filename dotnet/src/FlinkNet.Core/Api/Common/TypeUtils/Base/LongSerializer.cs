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

namespace FlinkNet.Api.Common.TypeUtils.Base;

/// <summary>Type serializer for <see cref="long"/>.</summary>
[Internal]
public sealed class LongSerializer : TypeSerializerSingleton<long>
{
    /// <summary>Sharable instance of the LongSerializer.</summary>
    public static readonly LongSerializer Instance = new();

    public override bool IsImmutableType => true;

    public override long CreateInstance() => 0L;

    public override long Copy(long from) => from;

    public override long Copy(long from, long reuse) => from;

    public override int Length => 8;

    public override void Serialize(long record, IDataOutputView target) =>
        target.WriteLong(record);

    public override long Deserialize(IDataInputView source) => source.ReadLong();

    public override long Deserialize(long reuse, IDataInputView source) =>
        Deserialize(source);

    public override void Copy(IDataInputView source, IDataOutputView target) =>
        target.WriteLong(source.ReadLong());

    public override TypeSerializerSnapshot<long> SnapshotConfiguration() =>
        new LongSerializerSnapshot();

    // ------------------------------------------------------------------------

    /// <summary>Serializer configuration snapshot for compatibility and format evolution.</summary>
    public sealed class LongSerializerSnapshot : SimpleTypeSerializerSnapshot<long>
    {
        public LongSerializerSnapshot()
            : base(() => Instance)
        {
        }
    }
}
