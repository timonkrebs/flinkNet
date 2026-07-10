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

namespace FlinkNet.Api.Common.Watermarks;

/// <summary>
/// The <see cref="BoolWatermarkDeclaration"/> class implements the
/// <see cref="IWatermarkDeclaration"/> interface and provides additional functionality specific to
/// boolean-type watermarks. It includes methods for obtaining combination semantics and creating
/// new bool watermarks.
/// </summary>
[Experimental]
public class BoolWatermarkDeclaration : IWatermarkDeclaration
{
    public BoolWatermarkDeclaration(
        string identifier,
        WatermarkCombinationPolicy combinationPolicy,
        WatermarkHandlingStrategy defaultHandlingStrategy)
    {
        Identifier = identifier;
        CombinationPolicy = combinationPolicy;
        DefaultHandlingStrategy = defaultHandlingStrategy;
    }

    public string Identifier { get; }

    public WatermarkCombinationPolicy CombinationPolicy { get; }

    public WatermarkHandlingStrategy DefaultHandlingStrategy { get; }

    /// <summary>
    /// Creates a new <see cref="BoolWatermark"/> with the specified boolean value.
    /// </summary>
    public BoolWatermark NewWatermark(bool val)
    {
        return new BoolWatermark(val, Identifier);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        BoolWatermarkDeclaration that = (BoolWatermarkDeclaration)obj;
        return Identifier == that.Identifier
            && Equals(CombinationPolicy, that.CombinationPolicy)
            && DefaultHandlingStrategy == that.DefaultHandlingStrategy;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Identifier, CombinationPolicy, DefaultHandlingStrategy);
    }

    public override string ToString()
    {
        return $"BoolWatermarkDeclaration{{identifier='{Identifier}'"
            + $", combinationPolicy={CombinationPolicy}"
            + $", defaultHandlingStrategy={DefaultHandlingStrategy}}}";
    }
}
