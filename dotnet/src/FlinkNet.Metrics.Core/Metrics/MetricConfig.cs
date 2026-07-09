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

using System.Globalization;
using FlinkNet.Annotations;

namespace FlinkNet.Metrics;

/// <summary>
/// A properties class with typed getters.
///
/// <para>PORT NOTE: Java extends <c>java.util.Properties</c>; the port derives from
/// <c>Dictionary&lt;string, object&gt;</c> with a <see cref="SetProperty"/> convenience.</para>
/// </summary>
[Public]
public class MetricConfig : Dictionary<string, object>
{
    /// <summary>Stores a string property.</summary>
    public void SetProperty(string key, string value) => this[key] = value;

    /// <summary>
    /// Searches for the property with the specified key in this property list, returning the
    /// default value argument if the property is not found.
    /// </summary>
    public string GetString(string key, string? defaultValue)
    {
        return TryGetValue(key, out object? value) ? value.ToString()! : defaultValue!;
    }

    public int GetInteger(string key, int defaultValue)
    {
        if (!TryGetValue(key, out object? value))
        {
            return defaultValue;
        }
        return value switch
        {
            int i => i,
            IConvertible c and (long or short or byte or float or double or decimal) =>
                c.ToInt32(CultureInfo.InvariantCulture),
            _ => int.Parse(value.ToString()!, CultureInfo.InvariantCulture),
        };
    }

    public long GetLong(string key, long defaultValue)
    {
        if (!TryGetValue(key, out object? value))
        {
            return defaultValue;
        }
        return value switch
        {
            long l => l,
            IConvertible c and (int or short or byte or float or double or decimal) =>
                c.ToInt64(CultureInfo.InvariantCulture),
            _ => long.Parse(value.ToString()!, CultureInfo.InvariantCulture),
        };
    }

    public float GetFloat(string key, float defaultValue)
    {
        if (!TryGetValue(key, out object? value))
        {
            return defaultValue;
        }
        return value switch
        {
            float f => f,
            IConvertible c and (int or long or short or byte or double or decimal) =>
                c.ToSingle(CultureInfo.InvariantCulture),
            _ => float.Parse(value.ToString()!, CultureInfo.InvariantCulture),
        };
    }

    public double GetDouble(string key, double defaultValue)
    {
        if (!TryGetValue(key, out object? value))
        {
            return defaultValue;
        }
        return value switch
        {
            double d => d,
            IConvertible c and (int or long or short or byte or float or decimal) =>
                c.ToDouble(CultureInfo.InvariantCulture),
            _ => double.Parse(value.ToString()!, CultureInfo.InvariantCulture),
        };
    }

    /// <summary>
    /// Like Java's <c>Boolean.parseBoolean</c>: returns true iff the stored value's string form
    /// is "true" (case-insensitive); missing keys return the default.
    /// </summary>
    public bool GetBoolean(string key, bool defaultValue)
    {
        if (!TryGetValue(key, out object? value))
        {
            return defaultValue;
        }
        if (value is bool b)
        {
            return b;
        }
        return string.Equals(value.ToString(), "true", StringComparison.OrdinalIgnoreCase);
    }
}
