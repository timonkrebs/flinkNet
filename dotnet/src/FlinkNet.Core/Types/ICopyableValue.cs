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
using FlinkNet.Core.Memory;

namespace FlinkNet.Types;

/// <summary>Interface to be implemented by basic types that support to be copied efficiently.</summary>
[Public]
public interface ICopyableValue<T> : IValue
{
    /// <summary>
    /// Gets the length of the data type when it is serialized, in bytes.
    /// </summary>
    /// <value>The length of the data type, or <c>-1</c>, if variable length.</value>
    int BinaryLength { get; }

    /// <summary>Performs a deep copy of this object into the <paramref name="target"/>
    /// instance.</summary>
    /// <param name="target">Object to copy into.</param>
    void CopyTo(T target);

    /// <summary>
    /// Performs a deep copy of this object into a new instance.
    ///
    /// <para>This method is useful for generic user-defined functions to clone a
    /// <see cref="ICopyableValue{T}"/> when storing multiple objects. With object reuse a deep
    /// copy must be created.</para>
    /// </summary>
    /// <returns>New object with copied fields.</returns>
    T Copy();

    /// <summary>
    /// Copies the next serialized instance from <paramref name="source"/> to
    /// <paramref name="target"/>.
    ///
    /// <para>This method is equivalent to calling <c>Read(source)</c> followed by
    /// <c>Write(target)</c> but does not require intermediate deserialization.</para>
    /// </summary>
    /// <param name="source">Data source for serialized instance.</param>
    /// <param name="target">Data target for serialized instance.</param>
    void Copy(IDataInputView source, IDataOutputView target);
}
