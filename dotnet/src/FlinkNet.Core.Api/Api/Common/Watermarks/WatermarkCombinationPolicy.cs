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
/// The <see cref="WatermarkCombinationPolicy"/> defines when and how to the combine
/// <see cref="IWatermark"/>s.
///
/// <para>The watermark combination process will first check the setting of
/// <see cref="IsCombineWaitForAllChannels"/>. If it is set to true, the ProcessFunction must
/// receive watermarks from all input channels before it can proceed with the combination.</para>
///
/// <para>The actual combination of watermarks will then be executed using the specified
/// <see cref="WatermarkCombinationFunction"/>.</para>
/// </summary>
[Experimental]
public class WatermarkCombinationPolicy
{
    public WatermarkCombinationPolicy(
        IWatermarkCombinationFunction watermarkCombinationFunction,
        bool combineWaitForAllChannels)
    {
        WatermarkCombinationFunction = watermarkCombinationFunction;
        IsCombineWaitForAllChannels = combineWaitForAllChannels;
    }

    public IWatermarkCombinationFunction WatermarkCombinationFunction { get; }

    /// <summary>
    /// Whether the combine process should be executed after the process function receives
    /// watermarks from both upstream channels.
    /// </summary>
    public bool IsCombineWaitForAllChannels { get; }

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
        WatermarkCombinationPolicy that = (WatermarkCombinationPolicy)obj;
        return IsCombineWaitForAllChannels == that.IsCombineWaitForAllChannels
            && Equals(WatermarkCombinationFunction, that.WatermarkCombinationFunction);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(WatermarkCombinationFunction, IsCombineWaitForAllChannels);
    }
}
