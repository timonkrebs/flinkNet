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

using System.Reflection;
using FlinkNet.Annotations;
using Xunit;

namespace FlinkNet.Tests;

/// <summary>
/// Java's stability annotations are not <c>@Inherited</c>: an unannotated subclass of a
/// <c>@Public</c> type makes no stability commitment. The attributes must behave the same.
/// </summary>
public class StabilityAttributesTest
{
    [Theory]
    [InlineData(typeof(PublicAttribute))]
    [InlineData(typeof(PublicEvolvingAttribute))]
    [InlineData(typeof(ExperimentalAttribute))]
    [InlineData(typeof(InternalAttribute))]
    [InlineData(typeof(VisibleForTestingAttribute))]
    public void TestAttributeIsNotInherited(Type attributeType)
    {
        AttributeUsageAttribute usage = attributeType.GetCustomAttribute<AttributeUsageAttribute>()!;
        Assert.False(usage.Inherited);
    }

    [Fact]
    public void TestUnannotatedSubclassIsNotPublic()
    {
        Assert.True(Attribute.IsDefined(typeof(AnnotatedBase), typeof(PublicAttribute), inherit: true));
        Assert.False(Attribute.IsDefined(typeof(UnannotatedDerived), typeof(PublicAttribute), inherit: true));
    }

    [Public]
    private class AnnotatedBase
    {
    }

    private sealed class UnannotatedDerived : AnnotatedBase
    {
    }
}
