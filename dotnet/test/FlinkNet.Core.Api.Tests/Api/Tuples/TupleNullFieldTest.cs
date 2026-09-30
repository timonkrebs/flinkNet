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

using FlinkNet.Api.Tuples;
using Xunit;

namespace FlinkNet.Tests.Api.Tuples;

/// <summary>
/// Null handling of tuple fields. PORT NOTE: Java's boxed fields (<c>Integer</c>) map to nullable
/// types (<c>int?</c>); a non-nullable value-type field holds <c>default(T)</c> and rejects null.
/// </summary>
public class TupleNullFieldTest
{
    [Fact]
    public void TestNullableFieldsBehaveLikeJavaBoxedFields()
    {
        var tuple = new Tuple2<int?, string>();
        Assert.Null(tuple.F0);
        Assert.Null(tuple.F1);

        tuple.SetField<object>(5, 0);
        Assert.Equal(5, tuple.F0);
        tuple.SetField<object>(null, 0);
        Assert.Null(tuple.F0);
    }

    [Fact]
    public void TestNonNullableValueTypeFieldRejectsNull()
    {
        var tuple = new Tuple1<int>();
        Assert.Equal(0, tuple.F0);

        ArgumentNullException e =
            Assert.Throws<ArgumentNullException>(() => tuple.SetField<object>(null, 0));
        Assert.Contains("int?", e.Message);

        tuple.SetField<object>(7, 0);
        Assert.Equal(7, tuple.F0);
    }
}
