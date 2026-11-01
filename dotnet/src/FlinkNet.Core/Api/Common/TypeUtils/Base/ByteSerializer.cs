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

/// <summary>
/// Type serializer for Java's <c>byte</c>/<c>Byte</c>, which is signed.
///
/// <para>PORT NOTE: Java's byte maps to <see cref="sbyte"/> (-128..127), not the unsigned CLR
/// <see cref="byte"/>, so values, ordering and arithmetic keep Flink's semantics (Java's -1 is
/// -1, not 255). The wire byte is unchanged; raw binary data stays <c>byte[]</c>.</para>
/// </summary>
[Internal]
public sealed class ByteSerializer : TypeSerializerSingleton<sbyte>
{
    /// <summary>Sharable instance of the ByteSerializer.</summary>
    public static readonly ByteSerializer Instance = new();

    public override bool IsImmutableType => true;

    public override sbyte CreateInstance() => 0;

    public override sbyte Copy(sbyte from) => from;

    public override sbyte Copy(sbyte from, sbyte reuse) => from;

    public override int Length => 1;

    public override void Serialize(sbyte record, IDataOutputView target) =>
        target.WriteByte(record);

    public override sbyte Deserialize(IDataInputView source) => unchecked((sbyte)source.ReadByte());

    public override sbyte Deserialize(sbyte reuse, IDataInputView source) =>
        Deserialize(source);

    public override void Copy(IDataInputView source, IDataOutputView target) =>
        target.WriteByte(source.ReadByte());

    public override TypeSerializerSnapshot<sbyte> SnapshotConfiguration() =>
        new ByteSerializerSnapshot();

    // ------------------------------------------------------------------------

    /// <summary>Serializer configuration snapshot for compatibility and format evolution.</summary>
    public sealed class ByteSerializerSnapshot : SimpleTypeSerializerSnapshot<sbyte>
    {
        public ByteSerializerSnapshot()
            : base(() => Instance)
        {
        }
    }
}
