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

namespace FlinkNet.Api.Common.Functions;

/// <summary>
/// The <c>AggregateFunction</c> is a flexible aggregation function, characterized by the following
/// features:
///
/// <list type="bullet">
///   <item><description>The aggregates may use different types for input values, intermediate
///       aggregates, and result type, to support a wide range of aggregation types.</description></item>
///   <item><description>Support for distributive aggregations: Different intermediate aggregates
///       can be merged together, to allow for pre-aggregation/final-aggregation
///       optimizations.</description></item>
/// </list>
///
/// <para>The <c>AggregateFunction</c>'s intermediate aggregate (in-progress aggregation state) is
/// called the <i>accumulator</i>. Values are added to the accumulator, and final aggregates are
/// obtained by finalizing the accumulator state. This supports aggregation functions where the
/// intermediate state needs to be different than the aggregated values and the final result type,
/// such as for example <i>average</i> (which typically keeps a count and sum). Merging intermediate
/// aggregates (partial aggregates) means merging the accumulators.</para>
///
/// <para>The AggregationFunction itself is stateless. To allow a single AggregationFunction
/// instance to maintain multiple aggregates (such as one aggregate per key), the
/// AggregationFunction creates a new accumulator whenever a new aggregation is started.</para>
///
/// <para>Aggregation functions are sent around between distributed processes during distributed
/// execution; Flink takes care of their serialization.</para>
///
/// <para><b>Example: Average and Weighted Average</b></para>
///
/// <code><![CDATA[
/// // the accumulator, which holds the state of the in-flight aggregate
/// public class AverageAccumulator
/// {
///     public long Count;
///     public long Sum;
/// }
///
/// // implementation of an aggregation function for an 'average'
/// public class Average : IAggregateFunction<int, AverageAccumulator, double>
/// {
///     public AverageAccumulator CreateAccumulator()
///     {
///         return new AverageAccumulator();
///     }
///
///     public AverageAccumulator Merge(AverageAccumulator a, AverageAccumulator b)
///     {
///         a.Count += b.Count;
///         a.Sum += b.Sum;
///         return a;
///     }
///
///     public AverageAccumulator Add(int value, AverageAccumulator acc)
///     {
///         acc.Sum += value;
///         acc.Count++;
///         return acc;
///     }
///
///     public double GetResult(AverageAccumulator acc)
///     {
///         return acc.Sum / (double)acc.Count;
///     }
/// }
///
/// // implementation of a weighted average
/// // this reuses the same accumulator type as the aggregate function for 'average'
/// public class WeightedAverage : IAggregateFunction<Datum, AverageAccumulator, double>
/// {
///     public AverageAccumulator CreateAccumulator()
///     {
///         return new AverageAccumulator();
///     }
///
///     public AverageAccumulator Merge(AverageAccumulator a, AverageAccumulator b)
///     {
///         a.Count += b.Count;
///         a.Sum += b.Sum;
///         return a;
///     }
///
///     public AverageAccumulator Add(Datum value, AverageAccumulator acc)
///     {
///         acc.Count += value.Weight;
///         acc.Sum += value.Value;
///         return acc;
///     }
///
///     public double GetResult(AverageAccumulator acc)
///     {
///         return acc.Sum / (double)acc.Count;
///     }
/// }
/// ]]></code>
/// </summary>
/// <typeparam name="TIn">The type of the values that are aggregated (input values)</typeparam>
/// <typeparam name="TAcc">The type of the accumulator (intermediate aggregate state).</typeparam>
/// <typeparam name="TOut">The type of the aggregated result</typeparam>
[PublicEvolving]
public interface IAggregateFunction<TIn, TAcc, TOut> : IFunction
{
    /// <summary>
    /// Creates a new accumulator, starting a new aggregate.
    ///
    /// <para>The new accumulator is typically meaningless unless a value is added via
    /// <see cref="Add"/>.</para>
    ///
    /// <para>The accumulator is the state of a running aggregation. When a program has multiple
    /// aggregates in progress (such as per key and window), the state (per key and window) is the
    /// size of the accumulator.</para>
    /// </summary>
    /// <returns>A new accumulator, corresponding to an empty aggregate.</returns>
    TAcc CreateAccumulator();

    /// <summary>
    /// Adds the given input value to the given accumulator, returning the new accumulator value.
    ///
    /// <para>For efficiency, the input accumulator may be modified and returned.</para>
    /// </summary>
    /// <param name="value">The value to add</param>
    /// <param name="accumulator">The accumulator to add the value to</param>
    /// <returns>The accumulator with the updated state</returns>
    TAcc Add(TIn value, TAcc accumulator);

    /// <summary>
    /// Gets the result of the aggregation from the accumulator.
    /// </summary>
    /// <param name="accumulator">The accumulator of the aggregation</param>
    /// <returns>The final aggregation result.</returns>
    TOut GetResult(TAcc accumulator);

    /// <summary>
    /// Merges two accumulators, returning an accumulator with the merged state.
    ///
    /// <para>This function may reuse any of the given accumulators as the target for the merge and
    /// return that. The assumption is that the given accumulators will not be used any more after
    /// having been passed to this function.</para>
    /// </summary>
    /// <param name="a">An accumulator to merge</param>
    /// <param name="b">Another accumulator to merge</param>
    /// <returns>The accumulator with the merged state</returns>
    TAcc Merge(TAcc a, TAcc b);
}
