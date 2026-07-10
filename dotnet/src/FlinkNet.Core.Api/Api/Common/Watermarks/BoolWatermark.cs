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

namespace FlinkNet.Api.Common.Watermarks;

/// <summary>
/// The <see cref="BoolWatermark"/> represents a watermark with a boolean value and an associated
/// identifier.
/// </summary>
[Experimental]
public class BoolWatermark : IWatermark
{
    public BoolWatermark(bool value, string identifier)
    {
        Value = value;
        Identifier = identifier;
    }

    public bool Value { get; }

    public string Identifier { get; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        BoolWatermark that = (BoolWatermark)obj;
        return Value == that.Value && Identifier == that.Identifier;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, Identifier);
    }

    public override string ToString()
    {
        return $"BoolWatermark{{value={(Value ? "true" : "false")}, identifier='{Identifier}'}}";
    }
}
