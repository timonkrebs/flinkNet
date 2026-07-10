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

/// <summary>Type serializer for <see cref="int"/>.</summary>
[Internal]
public sealed class IntSerializer : TypeSerializerSingleton<int>
{
    /// <summary>Sharable instance of the IntSerializer.</summary>
    public static readonly IntSerializer Instance = new();

    public override bool IsImmutableType => true;

    public override int CreateInstance() => 0;

    public override int Copy(int from) => from;

    public override int Copy(int from, int reuse) => from;

    public override int Length => 4;

    public override void Serialize(int record, IDataOutputView target) =>
        target.WriteInt(record);

    public override int Deserialize(IDataInputView source) => source.ReadInt();

    public override int Deserialize(int reuse, IDataInputView source) =>
        Deserialize(source);

    public override void Copy(IDataInputView source, IDataOutputView target) =>
        target.WriteInt(source.ReadInt());

    public override TypeSerializerSnapshot<int> SnapshotConfiguration() =>
        new IntSerializerSnapshot();

    // ------------------------------------------------------------------------

    /// <summary>Serializer configuration snapshot for compatibility and format evolution.</summary>
    public sealed class IntSerializerSnapshot : SimpleTypeSerializerSnapshot<int>
    {
        public IntSerializerSnapshot()
            : base(() => Instance)
        {
        }
    }
}
