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

using FlinkNet.Api.Common.TypeInfo;
using FlinkNet.Api.Common.TypeUtils.Base;
using Xunit;

namespace FlinkNet.Tests.Api.Common.TypeInfo;

/// <summary>Tests for <see cref="BasicTypeInfo"/>.</summary>
public class BasicTypeInfoTest
{
    [Fact]
    public void TestProperties()
    {
        BasicTypeInfo<int> intInfo = BasicTypeInfo.IntTypeInfo;
        Assert.True(intInfo.IsBasicType);
        Assert.False(intInfo.IsTupleType);
        Assert.Equal(1, intInfo.Arity);
        Assert.Equal(1, intInfo.TotalFields);
        Assert.Equal(typeof(int), intInfo.TypeClass);
        Assert.True(intInfo.IsKeyType);
        Assert.Equal("Int32", intInfo.ToString());
    }

    [Fact]
    public void TestCreateSerializer()
    {
        Assert.Same(IntSerializer.Instance, BasicTypeInfo.IntTypeInfo.CreateSerializer(null));
        Assert.Same(StringSerializer.Instance, BasicTypeInfo.StringTypeInfo.CreateSerializer(null));
        Assert.Same(DoubleSerializer.Instance, BasicTypeInfo.DoubleTypeInfo.CreateSerializer(null));
    }

    [Fact]
    public void TestGetInfoFor()
    {
        Assert.Same(BasicTypeInfo.IntTypeInfo, BasicTypeInfo.GetInfoFor<int>());
        Assert.Same(BasicTypeInfo.StringTypeInfo, BasicTypeInfo.GetInfoFor<string>());
        Assert.Same(BasicTypeInfo.CharTypeInfo, BasicTypeInfo.GetInfoFor<char>());
        Assert.Null(BasicTypeInfo.GetInfoFor<decimal>());
    }

    [Fact]
    public void TestShouldAutocastTo()
    {
        Assert.True(BasicTypeInfo.ByteTypeInfo.ShouldAutocastTo(BasicTypeInfo.ShortTypeInfo));
        Assert.True(BasicTypeInfo.ByteTypeInfo.ShouldAutocastTo(BasicTypeInfo.DoubleTypeInfo));
        Assert.True(BasicTypeInfo.IntTypeInfo.ShouldAutocastTo(BasicTypeInfo.LongTypeInfo));
        Assert.True(BasicTypeInfo.LongTypeInfo.ShouldAutocastTo(BasicTypeInfo.FloatTypeInfo));
        Assert.True(BasicTypeInfo.FloatTypeInfo.ShouldAutocastTo(BasicTypeInfo.DoubleTypeInfo));

        Assert.False(BasicTypeInfo.DoubleTypeInfo.ShouldAutocastTo(BasicTypeInfo.FloatTypeInfo));
        Assert.False(BasicTypeInfo.IntTypeInfo.ShouldAutocastTo(BasicTypeInfo.ShortTypeInfo));
        Assert.False(BasicTypeInfo.StringTypeInfo.ShouldAutocastTo(BasicTypeInfo.CharTypeInfo));
    }

    [Fact]
    public void TestEquality()
    {
        Assert.Equal(BasicTypeInfo.IntTypeInfo, BasicTypeInfo.GetInfoFor<int>());
        Assert.NotEqual<object>(BasicTypeInfo.IntTypeInfo, BasicTypeInfo.LongTypeInfo);
        Assert.Equal(
            BasicTypeInfo.IntTypeInfo.GetHashCode(),
            BasicTypeInfo.GetInfoFor<int>()!.GetHashCode());
    }
}
