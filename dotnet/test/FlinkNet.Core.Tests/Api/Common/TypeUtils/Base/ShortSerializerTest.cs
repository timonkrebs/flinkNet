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

using FlinkNet.Api.Common.TypeUtils;
using FlinkNet.Api.Common.TypeUtils.Base;

namespace FlinkNet.Tests.Api.Common.TypeUtils.Base;

/// <summary>A test for the <see cref="ShortSerializer"/>.</summary>
public class ShortSerializerTest : SerializerTestBase<short>
{
    protected override TypeSerializer<short> CreateSerializer() => ShortSerializer.Instance;

    protected override int ExpectedLength => 2;

    protected override short[] GetTestData() =>
    [
        0,
        1,
        -1,
        short.MaxValue,
        short.MinValue,
        12345,
        -9876,
    ];
}
