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

using FlinkNet.Api.Common.Watermarks;
using Xunit;

namespace FlinkNet.Tests.Api.Common.Watermarks;

/// <summary>Tests for <see cref="WatermarkDeclarations"/>.</summary>
public class WatermarkDeclarationsTest
{
    private const string DefaultWatermarkIdentifier = "default";

    [Fact]
    public void TestCreatedLongWatermarkDeclarationDefaultValue()
    {
        LongWatermarkDeclaration watermarkDeclaration =
            WatermarkDeclarations.NewBuilder(DefaultWatermarkIdentifier).TypeLong().Build();
        Assert.Equal(DefaultWatermarkIdentifier, watermarkDeclaration.Identifier);
        Assert.Same(
            IWatermarkCombinationFunction.NumericWatermarkCombinationFunction.Min,
            watermarkDeclaration.CombinationPolicy.WatermarkCombinationFunction);
        Assert.False(watermarkDeclaration.CombinationPolicy.IsCombineWaitForAllChannels);
        Assert.Equal(WatermarkHandlingStrategy.Forward, watermarkDeclaration.DefaultHandlingStrategy);
    }

    [Fact]
    public void TestCreatedBoolWatermarkDeclarationDefaultValue()
    {
        BoolWatermarkDeclaration watermarkDeclaration =
            WatermarkDeclarations.NewBuilder(DefaultWatermarkIdentifier).TypeBool().Build();
        Assert.Equal(DefaultWatermarkIdentifier, watermarkDeclaration.Identifier);
        Assert.Same(
            IWatermarkCombinationFunction.BoolWatermarkCombinationFunction.And,
            watermarkDeclaration.CombinationPolicy.WatermarkCombinationFunction);
        Assert.False(watermarkDeclaration.CombinationPolicy.IsCombineWaitForAllChannels);
        Assert.Equal(WatermarkHandlingStrategy.Forward, watermarkDeclaration.DefaultHandlingStrategy);
    }

    [Fact]
    public void TestBuildLongWatermarkDeclaration()
    {
        LongWatermarkDeclaration watermarkDeclaration =
            WatermarkDeclarations.NewBuilder(DefaultWatermarkIdentifier)
                .TypeLong()
                .CombineFunctionMax()
                .CombineWaitForAllChannels(true)
                .DefaultHandlingStrategyIgnore()
                .Build();
        Assert.Equal(DefaultWatermarkIdentifier, watermarkDeclaration.Identifier);
        Assert.Same(
            IWatermarkCombinationFunction.NumericWatermarkCombinationFunction.Max,
            watermarkDeclaration.CombinationPolicy.WatermarkCombinationFunction);
        Assert.True(watermarkDeclaration.CombinationPolicy.IsCombineWaitForAllChannels);
        Assert.Equal(WatermarkHandlingStrategy.Ignore, watermarkDeclaration.DefaultHandlingStrategy);
    }

    [Fact]
    public void TestBuildBoolWatermarkDeclaration()
    {
        BoolWatermarkDeclaration watermarkDeclaration =
            WatermarkDeclarations.NewBuilder(DefaultWatermarkIdentifier)
                .TypeBool()
                .CombineFunctionOR()
                .CombineWaitForAllChannels(true)
                .DefaultHandlingStrategyIgnore()
                .Build();
        Assert.Equal(DefaultWatermarkIdentifier, watermarkDeclaration.Identifier);
        Assert.Same(
            IWatermarkCombinationFunction.BoolWatermarkCombinationFunction.Or,
            watermarkDeclaration.CombinationPolicy.WatermarkCombinationFunction);
        Assert.True(watermarkDeclaration.CombinationPolicy.IsCombineWaitForAllChannels);
        Assert.Equal(WatermarkHandlingStrategy.Ignore, watermarkDeclaration.DefaultHandlingStrategy);
    }
}
