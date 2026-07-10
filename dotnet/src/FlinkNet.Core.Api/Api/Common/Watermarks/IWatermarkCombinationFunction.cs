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
using FlinkNet.Api.Common.Functions;

namespace FlinkNet.Api.Common.Watermarks;

/// <summary>
/// The <see cref="IWatermarkCombinationFunction"/> defines the watermark's comparison/combination
/// semantics among multiple input channels.
/// </summary>
[Experimental]
public interface IWatermarkCombinationFunction : IFunction
{
    // PORT NOTE: In Java these nested types are enums that implement the
    // WatermarkCombinationFunction interface. C# enums cannot implement interfaces, so they are
    // ported as sealed classes exposing singleton instances; reference equality of the singletons
    // matches the Java enum equality semantics.

    /// <summary>
    /// The <see cref="BoolWatermarkCombinationFunction"/> defines the combination semantics for
    /// boolean watermarks. It includes logical operations such as <c>Or</c> and <c>And</c>.
    /// </summary>
    [Experimental]
    public sealed class BoolWatermarkCombinationFunction : IWatermarkCombinationFunction
    {
        /// <summary>Logical OR combination for boolean watermarks.</summary>
        public static readonly BoolWatermarkCombinationFunction Or =
            new BoolWatermarkCombinationFunction("Or");

        /// <summary>Logical AND combination for boolean watermarks.</summary>
        public static readonly BoolWatermarkCombinationFunction And =
            new BoolWatermarkCombinationFunction("And");

        private readonly string _name;

        private BoolWatermarkCombinationFunction(string name)
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }

    /// <summary>
    /// The <see cref="NumericWatermarkCombinationFunction"/> defines the combination semantics for
    /// numeric watermarks. It includes operations such as <c>Min</c> and <c>Max</c>.
    /// </summary>
    [Experimental]
    public sealed class NumericWatermarkCombinationFunction : IWatermarkCombinationFunction
    {
        /// <summary>Minimum value combination for numeric watermarks.</summary>
        public static readonly NumericWatermarkCombinationFunction Min =
            new NumericWatermarkCombinationFunction("Min");

        /// <summary>Maximum value combination for numeric watermarks.</summary>
        public static readonly NumericWatermarkCombinationFunction Max =
            new NumericWatermarkCombinationFunction("Max");

        private readonly string _name;

        private NumericWatermarkCombinationFunction(string name)
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
