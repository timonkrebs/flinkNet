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
using System.Numerics;

namespace FlinkNet.Util;

/// <summary>
/// Collection of utilities about time intervals.
///
/// <para>PORT NOTE: Java's <c>java.time.Duration</c> maps to <see cref="TimeSpan"/>. TimeSpan
/// ticks are 100 nanoseconds, so parsing values with sub-tick precision truncates toward zero
/// (e.g. <c>"1ns"</c> parses to <see cref="TimeSpan.Zero"/>), and the representable range is
/// smaller than Java's (overflow is reported against the TimeSpan range).</para>
/// </summary>
public static class TimeUtils
{
    private static readonly IReadOnlyDictionary<string, TimeUnit> LabelToUnitMap = InitMap();

    private static readonly BigInteger NanosPerTick = new(100);

    /// <summary>
    /// Parse the given string to a <see cref="TimeSpan"/>. The string is in format "{length
    /// value}{time unit label}", e.g. "123ms", "321 s". If no time unit label is specified, it
    /// will be considered as milliseconds. If above rules are not matched, it will fall back to
    /// parsing the ISO-8601 duration format.
    ///
    /// <para>Supported time unit labels are:</para>
    /// <list type="bullet">
    ///   <item><description>DAYS: "d", "day"</description></item>
    ///   <item><description>HOURS: "h", "hour"</description></item>
    ///   <item><description>MINUTES: "m", "min", "minute"</description></item>
    ///   <item><description>SECONDS: "s", "sec", "second"</description></item>
    ///   <item><description>MILLISECONDS: "ms", "milli", "millisecond"</description></item>
    ///   <item><description>MICROSECONDS: "µs", "micro", "microsecond"</description></item>
    ///   <item><description>NANOSECONDS: "ns", "nano", "nanosecond"</description></item>
    /// </list>
    /// </summary>
    /// <param name="text">string to parse.</param>
    public static TimeSpan ParseDuration(string text)
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
        string unitLabel = trimmed[pos..].Trim().ToLowerInvariant();

        if (number.Length == 0)
        {
            // Fall back to parse ISO-8601 duration format
            TimeSpan parsedDuration;
            try
            {
                parsedDuration = System.Xml.XmlConvert.ToTimeSpan(trimmed);
            }
            catch (FormatException)
            {
                throw new FormatException(
                    "text does not start with a number, and is not a valid ISO-8601 duration format: "
                        + trimmed);
            }
            if (parsedDuration < TimeSpan.Zero)
            {
                // Don't support negative duration which is consistent with before format
                throw new FormatException("negative duration is not supported");
            }
            return parsedDuration;
        }

        BigInteger value = BigInteger.Parse(number, CultureInfo.InvariantCulture);

        TimeUnit? unit;
        if (unitLabel.Length == 0)
        {
            unit = TimeUnit.Milliseconds;
        }
        else
        {
            LabelToUnitMap.TryGetValue(unitLabel, out unit);
        }
        if (unit is null)
        {
            throw new ArgumentException(
                "Time interval unit label '"
                    + unitLabel
                    + "' does not match any of the recognized units: "
                    + TimeUnit.GetAllUnits());
        }

        BigInteger ticks = value * unit.UnitAsNanos / NanosPerTick;
        if (ticks > long.MaxValue)
        {
            throw new ArgumentException(
                "The value '" + number + "' cannot be represented as Duration (numeric overflow).");
        }

        return TimeSpan.FromTicks((long)ticks);
    }

    /// <param name="duration">to convert to string</param>
    /// <returns>duration string in millis</returns>
    public static string GetStringInMillis(TimeSpan duration) =>
        (duration.Ticks / TimeSpan.TicksPerMillisecond).ToString(CultureInfo.InvariantCulture)
            + TimeUnit.Milliseconds.Labels[0];

    /// <summary>
    /// Pretty prints the duration as a lowest granularity unit that does not lose precision.
    ///
    /// <para>Examples:</para>
    /// <code>
    /// TimeSpan.FromMilliseconds(60000) will be printed as 1 min
    /// TimeSpan.FromHours(1) + TimeSpan.FromSeconds(1) will be printed as 3601 s
    /// </code>
    /// </summary>
    public static string FormatWithHighestUnit(TimeSpan duration)
    {
        BigInteger nanos = new BigInteger(duration.Ticks) * NanosPerTick;

        TimeUnit highestIntegerUnit = GetHighestIntegerUnit(nanos);
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} {1}",
            nanos / highestIntegerUnit.UnitAsNanos,
            highestIntegerUnit.Labels[0]);
    }

    private static IReadOnlyDictionary<string, TimeUnit> InitMap()
    {
        var labelToUnit = new Dictionary<string, TimeUnit>();
        foreach (TimeUnit timeUnit in TimeUnit.Values)
        {
            foreach (string label in timeUnit.Labels)
            {
                labelToUnit[label] = timeUnit;
            }
        }
        return labelToUnit;
    }

    private static TimeUnit GetHighestIntegerUnit(BigInteger nanos)
    {
        if (nanos.IsZero)
        {
            return TimeUnit.Milliseconds;
        }

        TimeUnit[] orderedUnits =
        [
            TimeUnit.Nanoseconds,
            TimeUnit.Microseconds,
            TimeUnit.Milliseconds,
            TimeUnit.Seconds,
            TimeUnit.Minutes,
            TimeUnit.Hours,
            TimeUnit.Days,
        ];

        TimeUnit? highestIntegerUnit = null;
        foreach (TimeUnit timeUnit in orderedUnits)
        {
            if (!(nanos % timeUnit.UnitAsNanos).IsZero)
            {
                break;
            }
            highestIntegerUnit = timeUnit;
        }

        return highestIntegerUnit
            ?? throw new InvalidOperationException("Should find a highestIntegerUnit.");
    }

    /// <summary>
    /// Defines a time unit, mostly used to parse value from configuration file.
    ///
    /// <para>PORT NOTE: a Java enum with fields; ported as a sealed class with static readonly
    /// instances because C# enums cannot carry data. <c>Name</c> keeps the Java enum constant
    /// names so error messages stay identical.</para>
    /// </summary>
    private sealed class TimeUnit
    {
        public static readonly TimeUnit Days = new("DAYS", 86_400_000_000_000L, Singular("d"), Plural("day"));
        public static readonly TimeUnit Hours = new("HOURS", 3_600_000_000_000L, Singular("h"), Plural("hour"));
        public static readonly TimeUnit Minutes = new("MINUTES", 60_000_000_000L, Singular("min"), Singular("m"), Plural("minute"));
        public static readonly TimeUnit Seconds = new("SECONDS", 1_000_000_000L, Singular("s"), Plural("sec"), Plural("second"));
        public static readonly TimeUnit Milliseconds = new("MILLISECONDS", 1_000_000L, Singular("ms"), Plural("milli"), Plural("millisecond"));
        public static readonly TimeUnit Microseconds = new("MICROSECONDS", 1_000L, Singular("µs"), Plural("micro"), Plural("microsecond"));
        public static readonly TimeUnit Nanoseconds = new("NANOSECONDS", 1L, Singular("ns"), Plural("nano"), Plural("nanosecond"));

        public static readonly TimeUnit[] Values =
        [
            Days, Hours, Minutes, Seconds, Milliseconds, Microseconds, Nanoseconds,
        ];

        private const string PluralSuffix = "s";

        private TimeUnit(string name, long unitAsNanos, params string[][] labels)
        {
            Name = name;
            UnitAsNanos = new BigInteger(unitAsNanos);
            Labels = labels.SelectMany(l => l).ToList();
        }

        public string Name { get; }

        public IReadOnlyList<string> Labels { get; }

        public BigInteger UnitAsNanos { get; }

        public static string GetAllUnits() =>
            string.Join(", ", Values.Select(CreateTimeUnitString));

        private static string CreateTimeUnitString(TimeUnit timeUnit) =>
            timeUnit.Name + ": (" + string.Join(" | ", timeUnit.Labels) + ")";

        private static string[] Singular(string label) => [label];

        private static string[] Plural(string label) => [label, label + PluralSuffix];
    }
}
