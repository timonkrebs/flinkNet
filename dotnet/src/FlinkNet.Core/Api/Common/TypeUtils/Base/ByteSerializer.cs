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

/// <summary>Type serializer for <see cref="byte"/>.</summary>
[Internal]
public sealed class ByteSerializer : TypeSerializerSingleton<byte>
{
    /// <summary>Sharable instance of the ByteSerializer.</summary>
    public static readonly ByteSerializer Instance = new();

    public override bool IsImmutableType => true;

    public override byte CreateInstance() => 0;

    public override byte Copy(byte from) => from;

    public override byte Copy(byte from, byte reuse) => from;

    public override int Length => 1;

    public override void Serialize(byte record, IDataOutputView target) =>
        target.WriteByte(record);

    public override byte Deserialize(IDataInputView source) => source.ReadByte();

    public override byte Deserialize(byte reuse, IDataInputView source) =>
        Deserialize(source);

    public override void Copy(IDataInputView source, IDataOutputView target) =>
        target.WriteByte(source.ReadByte());
}
