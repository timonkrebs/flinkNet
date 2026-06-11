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

/// <summary>Type serializer for <see cref="double"/>.</summary>
[Internal]
public sealed class DoubleSerializer : TypeSerializerSingleton<double>
{
    /// <summary>Sharable instance of the DoubleSerializer.</summary>
    public static readonly DoubleSerializer Instance = new();

    public override bool IsImmutableType => true;

    public override double CreateInstance() => 0.0;

    public override double Copy(double from) => from;

    public override double Copy(double from, double reuse) => from;

    public override int Length => 8;

    public override void Serialize(double record, IDataOutputView target) =>
        target.WriteDouble(record);

    public override double Deserialize(IDataInputView source) => source.ReadDouble();

    public override double Deserialize(double reuse, IDataInputView source) =>
        Deserialize(source);

    public override void Copy(IDataInputView source, IDataOutputView target) =>
        target.WriteDouble(source.ReadDouble());
}
