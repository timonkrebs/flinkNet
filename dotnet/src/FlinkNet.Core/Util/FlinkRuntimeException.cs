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

namespace FlinkNet.Util;

/// <summary>
/// Base class of all Flink-specific unchecked exceptions.
///
/// <para>PORT NOTE: C# has no checked/unchecked exception distinction; the type is kept as the
/// semantic base for non-recoverable Flink failures. Java's <c>WrappingRuntimeException</c> is
/// not ported — it exists only to tunnel checked exceptions through lambdas.</para>
/// </summary>
public class FlinkRuntimeException : Exception
{
    /// <summary>Creates a new Exception with the given message and null as the cause.</summary>
    /// <param name="message">The exception message</param>
    public FlinkRuntimeException(string message)
        : base(message)
    {
    }

    /// <summary>Creates a new exception with a null message and the given cause.</summary>
    /// <param name="cause">The exception that caused this exception</param>
    public FlinkRuntimeException(Exception cause)
        : base(cause.Message, cause)
    {
    }

    /// <summary>Creates a new exception with the given message and cause.</summary>
    /// <param name="message">The exception message</param>
    /// <param name="cause">The exception that caused this exception</param>
    public FlinkRuntimeException(string message, Exception cause)
        : base(message, cause)
    {
    }
}
