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

/// <summary>Type serializer for <see cref="float"/>.</summary>
[Internal]
public sealed class FloatSerializer : TypeSerializerSingleton<float>
{
    /// <summary>Sharable instance of the FloatSerializer.</summary>
    public static readonly FloatSerializer Instance = new();

    public override bool IsImmutableType => true;

    public override float CreateInstance() => 0f;

    public override float Copy(float from) => from;

    public override float Copy(float from, float reuse) => from;

    public override int Length => 4;

    public override void Serialize(float record, IDataOutputView target) =>
        target.WriteFloat(record);

    public override float Deserialize(IDataInputView source) => source.ReadFloat();

    public override float Deserialize(float reuse, IDataInputView source) =>
        Deserialize(source);

    public override void Copy(IDataInputView source, IDataOutputView target) =>
        target.WriteFloat(source.ReadFloat());
}
