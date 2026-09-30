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
using System.Text;
using FlinkNet.Annotations;

namespace FlinkNet.Configuration;

/// <summary>
/// MemorySize is a representation of a number of bytes, viewable in different units.
///
/// <para><b>Parsing</b></para>
///
/// <para>The size can be parsed from a text expression. If the expression is a pure number, the
/// value will be interpreted as bytes.</para>
/// </summary>
[PublicEvolving]
public class MemorySize : IComparable<MemorySize>
{
    public static readonly MemorySize Zero = new(0L);

    public static readonly MemorySize MaxValue = new(long.MaxValue);

    private static readonly IReadOnlyList<MemoryUnit> OrderedUnits =
    [
        MemoryUnit.Bytes,
        MemoryUnit.KiloBytes,
        MemoryUnit.MegaBytes,
        MemoryUnit.GigaBytes,
        MemoryUnit.TeraBytes,
    ];

    // ------------------------------------------------------------------------

    /// <summary>The memory size, in bytes.</summary>
    private readonly long _bytes;

    /// <summary>The memoized value returned by ToString().</summary>
    private string? _stringified;

    /// <summary>The memoized value returned by ToHumanReadableString().</summary>
    private string? _humanReadableStr;

    /// <summary>Constructs a new MemorySize.</summary>
    /// <param name="bytes">The size, in bytes. Must be zero or larger.</param>
    public MemorySize(long bytes)
    {
        if (bytes < 0)
        {
            throw new ArgumentException("bytes must be >= 0");
        }
        _bytes = bytes;
    }

    public static MemorySize OfMebiBytes(long mebiBytes) => new(mebiBytes << 20);

    // ------------------------------------------------------------------------

    /// <summary>Gets the memory size in bytes.</summary>
    public long Bytes => _bytes;

    /// <summary>Gets the memory size in Kibibytes (= 1024 bytes).</summary>
    public long KibiBytes => _bytes >> 10;

    /// <summary>Gets the memory size in Mebibytes (= 1024 Kibibytes).</summary>
    public int MebiBytes => (int)(_bytes >> 20);

    /// <summary>Gets the memory size in Gibibytes (= 1024 Mebibytes).</summary>
    public long GibiBytes => _bytes >> 30;

    /// <summary>Gets the memory size in Tebibytes (= 1024 Gibibytes).</summary>
    public long TebiBytes => _bytes >> 40;

    // ------------------------------------------------------------------------

    public override int GetHashCode() => (int)(_bytes ^ (_bytes >>> 32));

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj)
            || (obj is not null
                && obj.GetType() == GetType()
                && ((MemorySize)obj)._bytes == _bytes);

    public override string ToString() => _stringified ??= FormatToString();

    private string FormatToString()
    {
        MemoryUnit highestIntegerUnit = MemoryUnit.Bytes;
        for (int idx = 0; idx < OrderedUnits.Count; idx++)
        {
            if (_bytes % OrderedUnits[idx].Multiplier != 0)
            {
                highestIntegerUnit = idx == 0 ? OrderedUnits[0] : OrderedUnits[idx - 1];
                break;
            }
        }

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} {1}",
            _bytes / highestIntegerUnit.Multiplier,
            highestIntegerUnit.Units[1]);
    }

    public string ToHumanReadableString() => _humanReadableStr ??= FormatToHumanReadableString();

    private string FormatToHumanReadableString()
    {
        MemoryUnit highestUnit = MemoryUnit.Bytes;
        for (int idx = 0; idx < OrderedUnits.Count; idx++)
        {
            if (_bytes > OrderedUnits[idx].Multiplier)
            {
                highestUnit = OrderedUnits[idx];
            }
        }

        if (ReferenceEquals(highestUnit, MemoryUnit.Bytes))
        {
            return string.Format(
                CultureInfo.InvariantCulture, "{0} {1}", _bytes, MemoryUnit.Bytes.Units[1]);
        }

        double approximate = 1.0 * _bytes / highestUnit.Multiplier;
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:0.000}{1} ({2} bytes)",
            approximate,
            highestUnit.Units[1],
            _bytes);
    }

    public int CompareTo(MemorySize? other) =>
        other is null ? 1 : _bytes.CompareTo(other._bytes);

    // ------------------------------------------------------------------------
    //  Calculations
    // ------------------------------------------------------------------------

    public MemorySize Add(MemorySize that) => new(checked(_bytes + that._bytes));

    public MemorySize Subtract(MemorySize that) => new(checked(_bytes - that._bytes));

    public MemorySize Multiply(double multiplier)
    {
        if (multiplier < 0)
        {
            throw new ArgumentException("multiplier must be >= 0");
        }

        decimal product = _bytes * (decimal)multiplier;
        if (product > long.MaxValue)
        {
            throw new OverflowException("long overflow");
        }
        return new MemorySize((long)product);
    }

    public MemorySize Divide(long by)
    {
        if (by < 0)
        {
            throw new ArgumentException("divisor must be != 0");
        }
        return new MemorySize(_bytes / by);
    }

    // ------------------------------------------------------------------------
    //  Parsing
    // ------------------------------------------------------------------------

    /// <summary>Parses the given string as a MemorySize.</summary>
    /// <param name="text">The string to parse</param>
    /// <returns>The parsed MemorySize</returns>
    /// <exception cref="ArgumentException">Thrown, if the expression cannot be parsed.</exception>
    public static MemorySize Parse(string text) => new(ParseBytes(text));

    /// <summary>Parses the given string with a default unit.</summary>
    /// <param name="text">The string to parse.</param>
    /// <param name="defaultUnit">specify the default unit.</param>
    /// <returns>The parsed MemorySize.</returns>
    /// <exception cref="ArgumentException">Thrown, if the expression cannot be parsed.</exception>
    public static MemorySize Parse(string text, MemoryUnit defaultUnit)
    {
        if (!MemoryUnit.HasUnit(text))
        {
            return Parse(text + defaultUnit.Units[0]);
        }

        return Parse(text);
    }

    /// <summary>
    /// Parses the given string as bytes. The supported expressions are listed under
    /// <see cref="MemorySize"/>.
    /// </summary>
    /// <param name="text">The string to parse</param>
    /// <returns>The parsed size, in bytes.</returns>
    /// <exception cref="ArgumentException">Thrown, if the expression cannot be parsed.</exception>
    public static long ParseBytes(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string trimmed = text.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("argument is an empty- or whitespace-only string");
        }

        int len = trimmed.Length;
        int pos = 0;

        while (pos < len && trimmed[pos] >= '0' && trimmed[pos] <= '9')
        {
            pos++;
        }

        string number = trimmed[..pos];
        string unit = trimmed[pos..].Trim().ToLowerInvariant();

        if (number.Length == 0)
        {
            throw new FormatException("text does not start with a number");
        }

        if (!long.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out long value))
        {
            throw new ArgumentException(
                $"The value '{number}' cannot be represented as 64bit number (numeric overflow).");
        }

        long multiplier = ParseUnit(unit)?.Multiplier ?? 1L;
        long result = value * multiplier;

        // check for overflow
        if (result / multiplier != value)
        {
            throw new ArgumentException(
                $"The value '{text}' cannot be represented as 64bit number of bytes (numeric overflow).");
        }

        return result;
    }

    private static MemoryUnit? ParseUnit(string unit)
    {
        if (MatchesAny(unit, MemoryUnit.Bytes))
        {
            return MemoryUnit.Bytes;
        }
        else if (MatchesAny(unit, MemoryUnit.KiloBytes))
        {
            return MemoryUnit.KiloBytes;
        }
        else if (MatchesAny(unit, MemoryUnit.MegaBytes))
        {
            return MemoryUnit.MegaBytes;
        }
        else if (MatchesAny(unit, MemoryUnit.GigaBytes))
        {
            return MemoryUnit.GigaBytes;
        }
        else if (MatchesAny(unit, MemoryUnit.TeraBytes))
        {
            return MemoryUnit.TeraBytes;
        }
        else if (unit.Length != 0)
        {
            throw new ArgumentException(
                $"Memory size unit '{unit}' does not match any of the recognized units: "
                    + MemoryUnit.GetAllUnits());
        }

        return null;
    }

    private static bool MatchesAny(string str, MemoryUnit unit)
    {
        foreach (string s in unit.Units)
        {
            if (s == str)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Defines a memory unit, mostly used to parse values from configuration files.
    ///
    /// <para>To make larger values more compact, the common size suffixes are supported:</para>
    ///
    /// <list type="bullet">
    ///   <item><description>1b or 1bytes (bytes)</description></item>
    ///   <item><description>1k or 1kb or 1kibibytes (interpreted as kibibytes = 1024 bytes)</description></item>
    ///   <item><description>1m or 1mb or 1mebibytes (interpreted as mebibytes = 1024 kibibytes)</description></item>
    ///   <item><description>1g or 1gb or 1gibibytes (interpreted as gibibytes = 1024 mebibytes)</description></item>
    ///   <item><description>1t or 1tb or 1tebibytes (interpreted as tebibytes = 1024 gibibytes)</description></item>
    /// </list>
    ///
    /// <para>PORT NOTE: a Java enum with fields; ported as a sealed class with static readonly
    /// instances because C# enums cannot carry data.</para>
    /// </summary>
    public sealed class MemoryUnit
    {
        public static readonly MemoryUnit Bytes = new(["b", "bytes"], 1L);
        public static readonly MemoryUnit KiloBytes = new(["k", "kb", "kibibytes"], 1024L);
        public static readonly MemoryUnit MegaBytes = new(["m", "mb", "mebibytes"], 1024L * 1024L);
        public static readonly MemoryUnit GigaBytes = new(["g", "gb", "gibibytes"], 1024L * 1024L * 1024L);
        public static readonly MemoryUnit TeraBytes = new(["t", "tb", "tebibytes"], 1024L * 1024L * 1024L * 1024L);

        private MemoryUnit(string[] units, long multiplier)
        {
            Units = units;
            Multiplier = multiplier;
        }

        public string[] Units { get; }

        public long Multiplier { get; }

        public static string GetAllUnits() =>
            ConcatenateUnits(
                Bytes.Units, KiloBytes.Units, MegaBytes.Units, GigaBytes.Units, TeraBytes.Units);

        public static bool HasUnit(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            string trimmed = text.Trim();
            if (trimmed.Length == 0)
            {
                throw new ArgumentException("argument is an empty- or whitespace-only string");
            }

            int len = trimmed.Length;
            int pos = 0;

            while (pos < len && trimmed[pos] >= '0' && trimmed[pos] <= '9')
            {
                pos++;
            }

            string unit = trimmed[pos..].Trim().ToLowerInvariant();

            return unit.Length > 0;
        }

        private static string ConcatenateUnits(params string[][] allUnits)
        {
            var builder = new StringBuilder(128);

            foreach (string[] units in allUnits)
            {
                builder.Append('(');

                foreach (string unit in units)
                {
                    builder.Append(unit);
                    builder.Append(" | ");
                }

                builder.Length -= 3;
                builder.Append(") / ");
            }

            builder.Length -= 3;
            return builder.ToString();
        }
    }
}
