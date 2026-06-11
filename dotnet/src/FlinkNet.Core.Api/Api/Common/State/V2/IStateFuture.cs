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
using FlinkNet.Api.Tuples;

namespace FlinkNet.Api.Common.State.V2;

/// <summary>
/// StateFuture is a future that act as a return value for async state interfaces. Note: All these
/// methods of this interface can ONLY be called within task thread.
/// </summary>
/// <typeparam name="T">The return type of this future.</typeparam>
[Experimental]
public interface IStateFuture<T>
{
    /// <summary>
    /// Returns a new StateFuture that, when this future completes normally, is executed with this
    /// future's result as the argument to the supplied function.
    /// </summary>
    /// <typeparam name="TResult">The function's return type.</typeparam>
    /// <param name="fn">The function to use to compute the value of the returned StateFuture.</param>
    /// <returns>The new StateFuture.</returns>
    IStateFuture<TResult> ThenApply<TResult>(Func<T, TResult> fn);

    /// <summary>
    /// Returns a new StateFuture that, when this future completes normally, is executed with this
    /// future's result as the argument to the supplied action.
    /// </summary>
    /// <param name="action">The action to perform before completing the returned StateFuture.</param>
    /// <returns>The new StateFuture.</returns>
    IStateFuture<object?> ThenAccept(Action<T> action);

    /// <summary>
    /// Returns a new future that, when this future completes normally, is executed with this
    /// future as the argument to the supplied function.
    /// </summary>
    /// <typeparam name="TResult">The type of the returned StateFuture's result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <returns>The new StateFuture.</returns>
    IStateFuture<TResult> ThenCompose<TResult>(Func<T, IStateFuture<TResult>> action);

    /// <summary>
    /// Returns a new StateFuture that, when this and the other given future both complete
    /// normally, is executed with the two results as arguments to the supplied function.
    /// </summary>
    /// <typeparam name="TOther">The type of the other StateFuture's result.</typeparam>
    /// <typeparam name="TResult">The function's return type.</typeparam>
    /// <param name="other">The other StateFuture.</param>
    /// <param name="fn">The function to use to compute the value of the returned StateFuture.</param>
    /// <returns>The new StateFuture.</returns>
    IStateFuture<TResult> ThenCombine<TOther, TResult>(
        IStateFuture<TOther> other, Func<T, TOther, TResult> fn);

    /// <summary>
    /// Apply a condition test on the result of this StateFuture, and try to perform one action out
    /// of two based on the result. Gather the results of the condition test and the selected
    /// action into a StateFuture of tuple. The relationship between the action result and the
    /// returned new StateFuture are just like the <see cref="ThenApply{TResult}"/>.
    /// </summary>
    /// <typeparam name="TTrueResult">The type of the output from actionIfTrue.</typeparam>
    /// <typeparam name="TFalseResult">The type of the output from actionIfFalse.</typeparam>
    /// <param name="condition">The condition test.</param>
    /// <param name="actionIfTrue">The function to apply if the condition returns true.</param>
    /// <param name="actionIfFalse">The function to apply if the condition returns false.</param>
    /// <returns>The new StateFuture with the result of condition test, and result of action.</returns>
    IStateFuture<Tuple2<bool, object?>> ThenConditionallyApply<TTrueResult, TFalseResult>(
        Func<T, bool> condition,
        Func<T, TTrueResult> actionIfTrue,
        Func<T, TFalseResult> actionIfFalse);

    /// <summary>
    /// Apply a condition test on the result of this StateFuture, and try to perform the action if
    /// test result is true. Gather the results of the condition test and the action (if applied)
    /// into a StateFuture of tuple. The relationship between the action result and the returned
    /// new StateFuture are just like the <see cref="ThenApply{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the output from actionIfTrue.</typeparam>
    /// <param name="condition">The condition test.</param>
    /// <param name="actionIfTrue">The function to apply if the condition returns true.</param>
    /// <returns>The new StateFuture with the result of condition test, and result of action.</returns>
    IStateFuture<Tuple2<bool, TResult?>> ThenConditionallyApply<TResult>(
        Func<T, bool> condition, Func<T, TResult> actionIfTrue);

    /// <summary>
    /// Apply a condition test on the result of this StateFuture, and try to perform one action out
    /// of two based on the result. Gather the results of the condition test StateFuture.
    /// </summary>
    /// <param name="condition">The condition test.</param>
    /// <param name="actionIfTrue">The function to apply if the condition returns true.</param>
    /// <param name="actionIfFalse">The function to apply if the condition returns false.</param>
    /// <returns>The new StateFuture.</returns>
    IStateFuture<bool> ThenConditionallyAccept(
        Func<T, bool> condition, Action<T> actionIfTrue, Action<T> actionIfFalse);

    /// <summary>
    /// Apply a condition test on the result of this StateFuture, and try to perform the action if
    /// test result is true. Gather the results of the condition test StateFuture.
    /// </summary>
    /// <param name="condition">The condition test.</param>
    /// <param name="actionIfTrue">The function to apply if the condition returns true.</param>
    /// <returns>The new StateFuture.</returns>
    IStateFuture<bool> ThenConditionallyAccept(Func<T, bool> condition, Action<T> actionIfTrue);

    /// <summary>
    /// Apply a condition test on the result of this StateFuture, and try to perform one action out
    /// of two based on the result. Gather the results of the condition test and the selected
    /// action into a StateFuture of tuple. The relationship between the action result and the
    /// returned new StateFuture are just like the <see cref="ThenCompose{TResult}"/>.
    /// </summary>
    /// <typeparam name="TTrueResult">The type of the output from actionIfTrue.</typeparam>
    /// <typeparam name="TFalseResult">The type of the output from actionIfFalse.</typeparam>
    /// <param name="condition">The condition test.</param>
    /// <param name="actionIfTrue">The function to apply if the condition returns true.</param>
    /// <param name="actionIfFalse">The function to apply if the condition returns false.</param>
    /// <returns>The new StateFuture with the result of condition test, and result of action.</returns>
    IStateFuture<Tuple2<bool, object?>> ThenConditionallyCompose<TTrueResult, TFalseResult>(
        Func<T, bool> condition,
        Func<T, IStateFuture<TTrueResult>> actionIfTrue,
        Func<T, IStateFuture<TFalseResult>> actionIfFalse);

    /// <summary>
    /// Apply a condition test on the result of this StateFuture, and try to perform the action if
    /// test result is true. Gather the results of the condition test and the action (if applied)
    /// into a StateFuture of tuple. The relationship between the action result and the returned
    /// new StateFuture are just like the <see cref="ThenCompose{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the output from actionIfTrue.</typeparam>
    /// <param name="condition">The condition test.</param>
    /// <param name="actionIfTrue">The function to apply if the condition returns true.</param>
    /// <returns>The new StateFuture with the result of condition test, and result of action.</returns>
    IStateFuture<Tuple2<bool, TResult?>> ThenConditionallyCompose<TResult>(
        Func<T, bool> condition, Func<T, IStateFuture<TResult>> actionIfTrue);
}
