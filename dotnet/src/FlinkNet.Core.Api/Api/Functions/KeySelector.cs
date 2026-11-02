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

namespace FlinkNet.Api.Functions;

/// <summary>
/// The <see cref="IKeySelector{TIn,TKey}"/> allows to use deterministic objects for operations
/// such as reduce, reduceGroup, join, coGroup, etc. If invoked multiple times on the same object,
/// the returned key must be the same.
///
/// <para>The extractor takes an object and returns the deterministic key for that object.</para>
/// </summary>
/// <typeparam name="TIn">Type of objects to extract the key from.</typeparam>
/// <typeparam name="TKey">Type of key.</typeparam>
[Public]
public interface IKeySelector<TIn, TKey> : IFunction
{
    /// <summary>
    /// User-defined function that deterministically extracts the key from an object.
    ///
    /// <para>For example for a class:</para>
    ///
    /// <code>
    /// public class Word
    /// {
    ///     string word;
    ///     int count;
    /// }
    /// </code>
    ///
    /// <para>The key extractor could return the word as a key to group all Word objects by the
    /// string they contain.</para>
    ///
    /// <para>The code would look like this</para>
    ///
    /// <code>
    /// public string GetKey(Word w)
    /// {
    ///     return w.word;
    /// }
    /// </code>
    /// </summary>
    /// <param name="value">The object to get the key from.</param>
    /// <returns>The extracted key.</returns>
    /// <exception cref="Exception">Throwing an exception will cause the execution of the
    /// respective task to fail, and trigger recovery or cancellation of the program.</exception>
    TKey GetKey(TIn value);
}
