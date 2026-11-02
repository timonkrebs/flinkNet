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

namespace FlinkNet.DataStream.Api.Extension.Window.Context;

/// <summary>The <see cref="IWindowContext"/> for two input window processing.</summary>
[Experimental]
public interface ITwoInputWindowContext<TIn1, TIn2> : IWindowContext
{
    /// <summary>Puts the record from the first input into the window's internal record
    /// storage.</summary>
    void PutRecord1(TIn1 record);

    /// <summary>Retrieves all first-input records from the window's internal record
    /// storage.</summary>
    IEnumerable<TIn1> GetAllRecords1();

    /// <summary>Puts the record from the second input into the window's internal record
    /// storage.</summary>
    void PutRecord2(TIn2 record);

    /// <summary>Retrieves all second-input records from the window's internal record
    /// storage.</summary>
    IEnumerable<TIn2> GetAllRecords2();
}
