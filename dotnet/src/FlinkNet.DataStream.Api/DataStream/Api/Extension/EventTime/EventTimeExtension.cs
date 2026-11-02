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
using FlinkNet.Api.Common.Watermarks;
using FlinkNet.DataStream.Api.Extension.EventTime.Function;
using FlinkNet.DataStream.Api.Extension.EventTime.Strategy;
using FlinkNet.DataStream.Api.Function;

namespace FlinkNet.DataStream.Api.Extension.EventTime;

/// <summary>
/// The entry point of the event-time extension: the built-in event-time/idle-status watermark
/// declarations, identifier checks, the watermark-generator builder, and the wrapping of
/// user-defined <see cref="IEventTimeProcessFunction"/>s.
///
/// <para>PORT NOTE: Java delegates the wrap/build operations to
/// <c>org.apache.flink.datastream.impl...EventTimeExtensionImpl</c> via reflection; the port
/// uses an explicit <see cref="IImplProvider"/> registration performed by the implementation
/// module.</para>
/// </summary>
[Experimental]
public static class EventTimeExtension
{
    // =============== Event Time related Watermark Declarations ===============

    public static readonly LongWatermarkDeclaration EventTimeWatermarkDeclaration =
        WatermarkDeclarations.NewBuilder("BUILTIN_API_EVENT_TIME")
            .TypeLong()
            .CombineFunctionMin()
            .CombineWaitForAllChannels(true)
            .DefaultHandlingStrategyForward()
            .Build();

    public static readonly BoolWatermarkDeclaration IdleStatusWatermarkDeclaration =
        WatermarkDeclarations.NewBuilder("BUILTIN_API_EVENT_TIME_IDLE")
            .TypeBool()
            .CombineFunctionAND()
            .CombineWaitForAllChannels(true)
            .DefaultHandlingStrategyForward()
            .Build();

    public static bool IsEventTimeWatermark(IWatermark watermark) =>
        IsEventTimeWatermark(watermark.Identifier);

    public static bool IsEventTimeWatermark(string watermarkIdentifier) =>
        watermarkIdentifier == EventTimeWatermarkDeclaration.Identifier;

    public static bool IsIdleStatusWatermark(IWatermark watermark) =>
        IsIdleStatusWatermark(watermark.Identifier);

    public static bool IsIdleStatusWatermark(string watermarkIdentifier) =>
        watermarkIdentifier == IdleStatusWatermarkDeclaration.Identifier;

    // ======== EventTimeWatermarkGeneratorBuilder to generate event time watermarks =========

    /// <summary>
    /// Creates the builder to build the event time watermark generator as a process function.
    /// </summary>
    public static EventTimeWatermarkGeneratorBuilder<T> NewWatermarkGeneratorBuilder<T>(
        IEventTimeExtractor<T> eventTimeExtractor) =>
        new(eventTimeExtractor);

    // ======== Wrap user-defined event-time process functions =========

    public static IOneInputStreamProcessFunction<TIn, TOut> WrapProcessFunction<TIn, TOut>(
        IOneInputEventTimeStreamProcessFunction<TIn, TOut> processFunction) =>
        RequireProvider().WrapProcessFunction(processFunction);

    public static ITwoOutputStreamProcessFunction<TIn, TOut1, TOut2> WrapProcessFunction<TIn, TOut1, TOut2>(
        ITwoOutputEventTimeStreamProcessFunction<TIn, TOut1, TOut2> processFunction) =>
        RequireProvider().WrapProcessFunction(processFunction);

    public static ITwoInputNonBroadcastStreamProcessFunction<TIn1, TIn2, TOut> WrapProcessFunction<TIn1, TIn2, TOut>(
        ITwoInputNonBroadcastEventTimeStreamProcessFunction<TIn1, TIn2, TOut> processFunction) =>
        RequireProvider().WrapProcessFunction(processFunction);

    public static ITwoInputBroadcastStreamProcessFunction<TIn1, TIn2, TOut> WrapProcessFunction<TIn1, TIn2, TOut>(
        ITwoInputBroadcastEventTimeStreamProcessFunction<TIn1, TIn2, TOut> processFunction) =>
        RequireProvider().WrapProcessFunction(processFunction);

    /// <summary>Builds the watermark generator process function for the given strategy (used by
    /// <see cref="EventTimeWatermarkGeneratorBuilder{T}"/>).</summary>
    public static IOneInputStreamProcessFunction<T, T> BuildAsProcessFunction<T>(
        EventTimeWatermarkStrategy<T> watermarkStrategy) =>
        RequireProvider().BuildAsProcessFunction(watermarkStrategy);

    // --------------------------------------------------------------------------------------------

    private static IImplProvider? implProvider;

    /// <summary>Registers the implementation provider (called by the implementation module).</summary>
    public static void SetImplProvider(IImplProvider provider) => implProvider = provider;

    private static IImplProvider RequireProvider() =>
        implProvider
            ?? throw new InvalidOperationException(
                "Please ensure that the FlinkNet DataStream implementation module is available "
                    + "and has registered the EventTimeExtension implementation provider.");

    /// <summary>
    /// The operations the implementation module supplies for the event-time extension.
    /// </summary>
    [Internal]
    public interface IImplProvider
    {
        IOneInputStreamProcessFunction<TIn, TOut> WrapProcessFunction<TIn, TOut>(
            IOneInputEventTimeStreamProcessFunction<TIn, TOut> processFunction);

        ITwoOutputStreamProcessFunction<TIn, TOut1, TOut2> WrapProcessFunction<TIn, TOut1, TOut2>(
            ITwoOutputEventTimeStreamProcessFunction<TIn, TOut1, TOut2> processFunction);

        ITwoInputNonBroadcastStreamProcessFunction<TIn1, TIn2, TOut> WrapProcessFunction<TIn1, TIn2, TOut>(
            ITwoInputNonBroadcastEventTimeStreamProcessFunction<TIn1, TIn2, TOut> processFunction);

        ITwoInputBroadcastStreamProcessFunction<TIn1, TIn2, TOut> WrapProcessFunction<TIn1, TIn2, TOut>(
            ITwoInputBroadcastEventTimeStreamProcessFunction<TIn1, TIn2, TOut> processFunction);

        IOneInputStreamProcessFunction<T, T> BuildAsProcessFunction<T>(
            EventTimeWatermarkStrategy<T> watermarkStrategy);
    }
}
