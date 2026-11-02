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

namespace FlinkNet.Types;

/// <summary>
/// An exception specifying that a required field was not set in a record, i.e. was <c>null</c>.
/// </summary>
[Public]
public class NullFieldException : Exception
{
    private readonly int _fieldPos;

    /// <summary>
    /// Constructs a <see cref="NullFieldException"/> with <c>null</c> as its error detail message.
    /// </summary>
    public NullFieldException()
    {
        _fieldPos = -1;
    }

    /// <summary>
    /// Constructs a <see cref="NullFieldException"/> with the specified detail message.
    /// </summary>
    /// <param name="message">The detail message.</param>
    public NullFieldException(string message)
        : base(message)
    {
        _fieldPos = -1;
    }

    /// <summary>
    /// Constructs a <see cref="NullFieldException"/> with a default message, referring to the
    /// given field number as the null field.
    /// </summary>
    /// <param name="fieldIdx">The index of the field that was null, but expected to hold a value.</param>
    public NullFieldException(int fieldIdx)
        : base($"Field {fieldIdx} is null, but expected to hold a value.")
    {
        _fieldPos = fieldIdx;
    }

    /// <summary>
    /// Constructs a <see cref="NullFieldException"/> with a default message, referring to the
    /// given field number as the null field and a cause.
    /// </summary>
    /// <param name="fieldIdx">The index of the field that was null, but expected to hold a value.</param>
    /// <param name="cause">Pass the root cause of the error.</param>
    public NullFieldException(int fieldIdx, Exception cause)
        : base($"Field {fieldIdx} is null, but expected to hold a value.", cause)
    {
        _fieldPos = fieldIdx;
    }

    /// <summary>
    /// Gets the index of the field that was null, but expected to hold a value, or -1 if unknown.
    /// </summary>
    public int FieldPos => _fieldPos;
}
