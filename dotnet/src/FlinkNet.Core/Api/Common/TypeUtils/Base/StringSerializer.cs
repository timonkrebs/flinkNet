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
using FlinkNet.Types;

namespace FlinkNet.Api.Common.TypeUtils.Base;

/// <summary>
/// Type serializer for <see cref="string"/>, using Flink's variable-length string format
/// (see <see cref="StringValue"/>). Null records are supported on the wire (encoded as length
/// zero).
/// </summary>
[Internal]
public sealed class StringSerializer : TypeSerializerSingleton<string>
{
    /// <summary>Sharable instance of the StringSerializer.</summary>
    public static readonly StringSerializer Instance = new();

    private const string Empty = "";

    public override bool IsImmutableType => true;

    public override string CreateInstance() => Empty;

    public override string Copy(string from) => from;

    public override string Copy(string from, string reuse) => from;

    public override int Length => -1;

    public override void Serialize(string? record, IDataOutputView target) =>
        StringValue.WriteString(record, target);

    public override string Deserialize(IDataInputView source) => StringValue.ReadString(source)!;

    public override string Deserialize(string reuse, IDataInputView source) =>
        Deserialize(source);

    public override void Copy(IDataInputView source, IDataOutputView target) =>
        StringValue.CopyString(source, target);
}
