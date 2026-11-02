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

namespace FlinkNet.Configuration;

/// <summary>
/// An exception signaling that a configuration is invalid, or that some parameters are missing or
/// otherwise invalid.
/// </summary>
[Public]
public class IllegalConfigurationException : Exception
{
    /// <summary>
    /// Constructs a new exception with the given error message.
    /// </summary>
    /// <param name="message">The error message for the exception.</param>
    public IllegalConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Constructs a new exception with the given error message format and arguments
    /// (composite-format string, port of the Java <c>String.format</c> constructor).
    /// </summary>
    /// <param name="format">The error message format for the exception.</param>
    /// <param name="arguments">The arguments for the format.</param>
    public IllegalConfigurationException(string format, params object?[] arguments)
        : base(string.Format(System.Globalization.CultureInfo.InvariantCulture, format, arguments))
    {
    }

    /// <summary>
    /// Constructs a new exception with the given error message and a given cause.
    /// </summary>
    /// <param name="message">The error message for the exception.</param>
    /// <param name="cause">The error that caused this exception.</param>
    public IllegalConfigurationException(string message, Exception cause)
        : base(message, cause)
    {
    }
}
