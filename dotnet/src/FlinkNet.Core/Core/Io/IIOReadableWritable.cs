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

namespace FlinkNet.Core.Io;

/// <summary>
/// This interface must be implemented by every class whose objects have to be serialized to
/// their binary representation and vice-versa.
/// </summary>
[Public]
public interface IIOReadableWritable
{
    /// <summary>Writes the object's internal data to the given data output view.</summary>
    /// <param name="output">the output view to receive the data.</param>
    void Write(IDataOutputView output);

    /// <summary>Reads the object's internal data from the given data input view.</summary>
    /// <param name="input">the input view to read the data from</param>
    void Read(IDataInputView input);
}
