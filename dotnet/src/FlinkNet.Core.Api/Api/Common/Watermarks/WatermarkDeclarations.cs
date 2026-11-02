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
/// The Utils class is used to create <see cref="IWatermarkDeclaration"/>.
/// </summary>
[Experimental]
public static class WatermarkDeclarations
{
    public static WatermarkDeclarationBuilder NewBuilder(string identifier)
    {
        return new WatermarkDeclarationBuilder(identifier);
    }

    /// <summary>
    /// Builder class for <see cref="IWatermarkDeclaration"/>s.
    /// </summary>
    [Experimental]
    public class WatermarkDeclarationBuilder
    {
        private readonly string _identifier;

        internal WatermarkDeclarationBuilder(string identifier)
        {
            _identifier = identifier;
        }

        public LongWatermarkDeclarationBuilder TypeLong()
        {
            return new LongWatermarkDeclarationBuilder(_identifier);
        }

        public BoolWatermarkDeclarationBuilder TypeBool()
        {
            return new BoolWatermarkDeclarationBuilder(_identifier);
        }

        [Experimental]
        public class LongWatermarkDeclarationBuilder
        {
            private readonly string _identifier;
            private bool _combineWaitForAllChannels;
            private IWatermarkCombinationFunction _combinationFunction =
                IWatermarkCombinationFunction.NumericWatermarkCombinationFunction.Min;
            private WatermarkHandlingStrategy _defaultHandlingStrategy =
                WatermarkHandlingStrategy.Forward;

            public LongWatermarkDeclarationBuilder(string identifier)
            {
                _identifier = identifier;
            }

            /// <summary>Combine and propagate the maximum watermark to downstream.</summary>
            public LongWatermarkDeclarationBuilder CombineFunctionMax()
            {
                _combinationFunction =
                    IWatermarkCombinationFunction.NumericWatermarkCombinationFunction.Max;
                return this;
            }

            /// <summary>Combine and propagate the minimum watermark to downstream.</summary>
            public LongWatermarkDeclarationBuilder CombineFunctionMin()
            {
                _combinationFunction =
                    IWatermarkCombinationFunction.NumericWatermarkCombinationFunction.Min;
                return this;
            }

            /// <summary>
            /// Define whether the framework should send the <see cref="IWatermark"/> to downstream
            /// tasks when the user-defined <see cref="IWatermark"/> process method returns
            /// <see cref="WatermarkHandlingResult.Peek"/>. If set to
            /// <see cref="WatermarkHandlingStrategy.Forward"/>, the framework will send the
            /// watermark to downstream tasks. If set to
            /// <see cref="WatermarkHandlingStrategy.Ignore"/>, the framework will not take any
            /// action.
            /// </summary>
            public LongWatermarkDeclarationBuilder DefaultHandlingStrategy(
                WatermarkHandlingStrategy strategy)
            {
                _defaultHandlingStrategy = strategy;
                return this;
            }

            public LongWatermarkDeclarationBuilder DefaultHandlingStrategyForward()
            {
                _defaultHandlingStrategy = WatermarkHandlingStrategy.Forward;
                return this;
            }

            public LongWatermarkDeclarationBuilder DefaultHandlingStrategyIgnore()
            {
                _defaultHandlingStrategy = WatermarkHandlingStrategy.Ignore;
                return this;
            }

            /// <summary>
            /// Whether the combine process should be executed after the process function receives
            /// watermarks from both upstream channels.
            /// </summary>
            public LongWatermarkDeclarationBuilder CombineWaitForAllChannels(
                bool combineWaitForAllChannels)
            {
                _combineWaitForAllChannels = combineWaitForAllChannels;
                return this;
            }

            public LongWatermarkDeclaration Build()
            {
                return new LongWatermarkDeclaration(
                    _identifier,
                    new WatermarkCombinationPolicy(
                        _combinationFunction, _combineWaitForAllChannels),
                    _defaultHandlingStrategy);
            }
        }

        [Experimental]
        public class BoolWatermarkDeclarationBuilder
        {
            private readonly string _identifier;
            private bool _combineWaitForAllChannels;
            private IWatermarkCombinationFunction _combinationFunction =
                IWatermarkCombinationFunction.BoolWatermarkCombinationFunction.And;
            private WatermarkHandlingStrategy _defaultHandlingStrategy =
                WatermarkHandlingStrategy.Forward;

            public BoolWatermarkDeclarationBuilder(string identifier)
            {
                _identifier = identifier;
            }

            /// <summary>
            /// Propagate the logical OR combination result of boolean watermarks downstream.
            /// </summary>
            public BoolWatermarkDeclarationBuilder CombineFunctionOR()
            {
                _combinationFunction =
                    IWatermarkCombinationFunction.BoolWatermarkCombinationFunction.Or;
                return this;
            }

            /// <summary>
            /// Propagate the logical AND combination result of boolean watermarks downstream.
            /// </summary>
            public BoolWatermarkDeclarationBuilder CombineFunctionAND()
            {
                _combinationFunction =
                    IWatermarkCombinationFunction.BoolWatermarkCombinationFunction.And;
                return this;
            }

            /// <summary>
            /// Define whether the framework should send the <see cref="IWatermark"/> to downstream
            /// tasks when the user-defined <see cref="IWatermark"/> process method returns
            /// <see cref="WatermarkHandlingResult.Peek"/>. If set to
            /// <see cref="WatermarkHandlingStrategy.Forward"/>, the framework will send the
            /// watermark to downstream tasks. If set to
            /// <see cref="WatermarkHandlingStrategy.Ignore"/>, the framework will not take any
            /// action.
            /// </summary>
            public BoolWatermarkDeclarationBuilder DefaultHandlingStrategy(
                WatermarkHandlingStrategy strategy)
            {
                _defaultHandlingStrategy = strategy;
                return this;
            }

            public BoolWatermarkDeclarationBuilder DefaultHandlingStrategyForward()
            {
                _defaultHandlingStrategy = WatermarkHandlingStrategy.Forward;
                return this;
            }

            public BoolWatermarkDeclarationBuilder DefaultHandlingStrategyIgnore()
            {
                _defaultHandlingStrategy = WatermarkHandlingStrategy.Ignore;
                return this;
            }

            /// <summary>
            /// Whether the combine process should be executed after the process function receives
            /// watermarks from both upstream channels.
            /// </summary>
            public BoolWatermarkDeclarationBuilder CombineWaitForAllChannels(
                bool combineWaitForAllChannels)
            {
                _combineWaitForAllChannels = combineWaitForAllChannels;
                return this;
            }

            public BoolWatermarkDeclaration Build()
            {
                return new BoolWatermarkDeclaration(
                    _identifier,
                    new WatermarkCombinationPolicy(
                        _combinationFunction, _combineWaitForAllChannels),
                    _defaultHandlingStrategy);
            }
        }
    }
}
