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

/// <summary>Type serializer for <see cref="char"/>.</summary>
[Internal]
public sealed class CharSerializer : TypeSerializerSingleton<char>
{
    /// <summary>Sharable instance of the CharSerializer.</summary>
    public static readonly CharSerializer Instance = new();

    public override bool IsImmutableType => true;

    public override char CreateInstance() => (char)0;

    public override char Copy(char from) => from;

    public override char Copy(char from, char reuse) => from;

    public override int Length => 2;

    public override void Serialize(char record, IDataOutputView target) =>
        target.WriteChar(record);

    public override char Deserialize(IDataInputView source) => source.ReadChar();

    public override char Deserialize(char reuse, IDataInputView source) =>
        Deserialize(source);

    public override void Copy(IDataInputView source, IDataOutputView target) =>
        target.WriteChar(source.ReadChar());

    public override TypeSerializerSnapshot<char> SnapshotConfiguration() =>
        new CharSerializerSnapshot();

    // ------------------------------------------------------------------------

    /// <summary>Serializer configuration snapshot for compatibility and format evolution.</summary>
    public sealed class CharSerializerSnapshot : SimpleTypeSerializerSnapshot<char>
    {
        public CharSerializerSnapshot()
            : base(() => Instance)
        {
        }
    }
}
