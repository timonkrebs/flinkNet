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
using System.Text.RegularExpressions;
using FlinkNet.Util;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace FlinkNet.Configuration;

/// <summary>
/// This class contains utility methods to load standard yaml files and convert objects to
/// standard yaml syntax.
///
/// <para>PORT NOTE: Java builds on SnakeYAML Engine. The port parses with YamlDotNet's
/// representation model (so the quoted-vs-plain distinction survives) and applies the YAML 1.2
/// core schema scalar resolution that SnakeYAML's <c>CoreSchema</c> provides; dumping is
/// implemented directly to match SnakeYAML's flow/block output (including its single-quote
/// preference). YamlDotNet error messages do not embed document snippets, so Java's
/// sensitive-data sanitization wrapper is unnecessary.</para>
/// </summary>
public static partial class YamlParserUtils
{
    /// <summary>
    /// Loads the contents of the given YAML file into a map.
    /// </summary>
    /// <param name="file">the YAML file to load.</param>
    /// <returns>a non-null map representing the YAML content. If the file is empty or only
    /// contains comments, an empty map is returned.</returns>
    /// <exception cref="FileNotFoundException">if the YAML file is not found.</exception>
    /// <exception cref="YamlException">if the file cannot be parsed.</exception>
    /// <exception cref="IOException">if an I/O error occurs while reading from the file.</exception>
    public static IDictionary<string, object?> LoadYamlFile(string file)
    {
        using StreamReader reader = File.OpenText(file);
        var stream = new YamlStream();
        stream.Load(reader);

        object? root =
            stream.Documents.Count == 0 ? null : NodeToObject(stream.Documents[0].RootNode);
        if (root is null)
        {
            return new Dictionary<string, object?>();
        }

        // a non-mapping document is invalid; the cast throws like Java's (Map) cast does
        var map = (System.Collections.IDictionary)root;

        var result = new Dictionary<string, object?>();
        foreach (System.Collections.DictionaryEntry entry in map)
        {
            result[Convert.ToString(entry.Key, CultureInfo.InvariantCulture)!] = entry.Value;
        }
        return result;
    }

    /// <summary>
    /// Converts the given value to a string representation in the YAML syntax (single-line flow
    /// style).
    ///
    /// <para>Note: This method may perform escaping on certain characters in the value to ensure
    /// proper YAML syntax.</para>
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    /// <returns>The string representation of the value in YAML syntax.</returns>
    public static string ToYamlString(object? value)
    {
        var builder = new StringBuilder();
        EmitFlow(value, builder);
        return builder.ToString();
    }

    /// <summary>
    /// Converts a flat map into a nested map structure and outputs the result as a list of
    /// YAML-formatted strings (block style). Each item in the list represents a single line of
    /// the YAML data.
    /// </summary>
    /// <param name="flattenMap">A map containing flattened keys (e.g., "parent.child.key")
    /// associated with their values.</param>
    /// <returns>A list of strings that represents the YAML data, where each item corresponds to a
    /// line of the data.</returns>
    public static IList<string> ConvertAndDumpYamlFromFlatMap(
        IDictionary<string, object> flattenMap)
    {
        var nestedMap = new Dictionary<string, object?>();
        foreach (KeyValuePair<string, object> entry in flattenMap)
        {
            string[] keys = entry.Key.Split('.');
            Dictionary<string, object?> currentMap = nestedMap;
            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (!currentMap.TryGetValue(keys[i], out object? child)
                    || child is not Dictionary<string, object?> childMap)
                {
                    childMap = new Dictionary<string, object?>();
                    currentMap[keys[i]] = childMap;
                }
                currentMap = childMap;
            }
            currentMap[keys[^1]] = entry.Value;
        }

        var lines = new List<string>();
        EmitBlockMapping(nestedMap, 0, lines);
        return lines;
    }

    /// <summary>Parses the given YAML string and casts the result to <typeparamref name="T"/>.</summary>
    public static T? ConvertToObject<T>(string value)
    {
        var stream = new YamlStream();
        stream.Load(new StringReader(value));
        if (stream.Documents.Count == 0)
        {
            return default;
        }
        return (T?)NodeToObject(stream.Documents[0].RootNode);
    }

    // --------------------------------------------------------------------------------------------
    //  Parsing: YAML node -> object with YAML 1.2 core schema resolution
    // --------------------------------------------------------------------------------------------

    private static object? NodeToObject(YamlNode node)
    {
        switch (node)
        {
            case YamlScalarNode scalar:
                return ScalarToObject(scalar);
            case YamlSequenceNode sequence:
                {
                    var list = new List<object?>();
                    foreach (YamlNode child in sequence.Children)
                    {
                        list.Add(NodeToObject(child));
                    }
                    return list;
                }
            case YamlMappingNode mapping:
                {
                    var map = new Dictionary<object, object?>();
                    foreach (KeyValuePair<YamlNode, YamlNode> child in mapping.Children)
                    {
                        object key = NodeToObject(child.Key)!;
                        if (!map.TryAdd(key, NodeToObject(child.Value)))
                        {
                            // matches SnakeYAML's duplicate-key error, without leaking values
                            throw new YamlException("found duplicate key " + key);
                        }
                    }
                    return map;
                }
            default:
                throw new YamlException($"Unsupported YAML node type: {node.GetType()}");
        }
    }

    private static object? ScalarToObject(YamlScalarNode scalar)
    {
        string value = scalar.Value ?? "";

        // explicitly tagged scalars: only the core schema tags are supported
        if (!scalar.Tag.IsEmpty && !scalar.Tag.IsNonSpecific)
        {
            return scalar.Tag.Value switch
            {
                "tag:yaml.org,2002:str" => value,
                "tag:yaml.org,2002:bool" => ResolveBool(value)
                    ?? throw new YamlException($"Invalid boolean scalar: {value}"),
                "tag:yaml.org,2002:int" => ResolveInt(value)
                    ?? throw new YamlException($"Invalid integer scalar: {value}"),
                "tag:yaml.org,2002:float" => ResolveFloat(value)
                    ?? throw new YamlException($"Invalid float scalar: {value}"),
                "tag:yaml.org,2002:null" => null,
                _ =>
                    throw new YamlException(
                        "could not determine a constructor for the tag " + scalar.Tag.Value),
            };
        }

        // quoted scalars are always strings; plain scalars resolve per the core schema
        return scalar.Style == ScalarStyle.Plain ? ResolveScalar(value) : value;
    }

    /// <summary>YAML 1.2 core schema resolution for plain scalars.</summary>
    private static object? ResolveScalar(string value)
    {
        switch (value)
        {
            case "":
            case "~":
            case "null":
            case "Null":
            case "NULL":
                return null;
        }

        object? boolValue = ResolveBool(value);
        if (boolValue is not null)
        {
            return boolValue;
        }

        object? intValue = ResolveInt(value);
        if (intValue is not null)
        {
            return intValue;
        }

        object? floatValue = ResolveFloat(value);
        if (floatValue is not null)
        {
            return floatValue;
        }

        return value;
    }

    private static object? ResolveBool(string value) =>
        value switch
        {
            "true" or "True" or "TRUE" => true,
            "false" or "False" or "FALSE" => false,
            _ => null,
        };

    private static object? ResolveInt(string value)
    {
        if (DecimalIntRegex().IsMatch(value))
        {
            // values exceeding long stay strings; the conversion engine reports them later
            return long.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long l)
                ? l is >= int.MinValue and <= int.MaxValue ? (object)(int)l : (object)l
                : null;
        }
        if (value.StartsWith("0x", StringComparison.Ordinal) && HexIntRegex().IsMatch(value))
        {
            return long.TryParse(
                value[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long h)
                ? h is >= int.MinValue and <= int.MaxValue ? (object)(int)h : (object)h
                : null;
        }
        if (value.StartsWith("0o", StringComparison.Ordinal) && OctalIntRegex().IsMatch(value))
        {
            try
            {
                long o = Convert.ToInt64(value[2..], 8);
                return o is >= int.MinValue and <= int.MaxValue ? (object)(int)o : (object)o;
            }
            catch (OverflowException)
            {
                return null;
            }
        }
        return null;
    }

    private static object? ResolveFloat(string value)
    {
        switch (value)
        {
            case ".inf" or ".Inf" or ".INF" or "+.inf" or "+.Inf" or "+.INF":
                return double.PositiveInfinity;
            case "-.inf" or "-.Inf" or "-.INF":
                return double.NegativeInfinity;
            case ".nan" or ".NaN" or ".NAN":
                return double.NaN;
        }
        if (FloatRegex().IsMatch(value))
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double d)
                ? d
                : null;
        }
        return null;
    }

    [GeneratedRegex("^[-+]?[0-9]+$")]
    private static partial Regex DecimalIntRegex();

    [GeneratedRegex("^0x[0-9a-fA-F]+$")]
    private static partial Regex HexIntRegex();

    [GeneratedRegex("^0o[0-7]+$")]
    private static partial Regex OctalIntRegex();

    [GeneratedRegex(@"^[-+]?(\.[0-9]+|[0-9]+(\.[0-9]*)?)([eE][-+]?[0-9]+)?$")]
    private static partial Regex FloatRegex();

    // --------------------------------------------------------------------------------------------
    //  Dumping
    // --------------------------------------------------------------------------------------------

    private static void EmitFlow(object? value, StringBuilder builder, bool inFlowCollection = false)
    {
        switch (value)
        {
            case null:
                builder.Append("null");
                break;
            case string s:
                builder.Append(FormatScalar(s, inFlowCollection));
                break;
            case bool b:
                builder.Append(b ? "true" : "false");
                break;
            case TimeSpan duration:
                builder.Append(FormatScalar(TimeUtils.FormatWithHighestUnit(duration), inFlowCollection));
                break;
            case MemorySize memorySize:
                builder.Append(FormatScalar(memorySize.ToString(), inFlowCollection));
                break;
            case Enum e:
                builder.Append(FormatScalar(e.ToString(), inFlowCollection));
                break;
            case System.Collections.IDictionary rawMap:
                EmitFlowMapEntries(EnumerateEntries(rawMap), builder);
                break;
            case IEnumerable<KeyValuePair<string, string>> stringMap:
                EmitFlowMapEntries(
                    stringMap.Select(e => ((object)e.Key, (object?)e.Value)), builder);
                break;
            case System.Collections.IEnumerable enumerable:
                {
                    builder.Append('[');
                    bool first = true;
                    foreach (object? element in enumerable)
                    {
                        if (!first)
                        {
                            builder.Append(", ");
                        }
                        first = false;
                        EmitFlow(element, builder, inFlowCollection: true);
                    }
                    builder.Append(']');
                    break;
                }
            default:
                builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                break;
        }
    }

    private static void EmitFlowMapEntries(
        IEnumerable<(object Key, object? Value)> entries, StringBuilder builder)
    {
        builder.Append('{');
        bool first = true;
        foreach ((object key, object? entryValue) in entries)
        {
            if (!first)
            {
                builder.Append(", ");
            }
            first = false;
            EmitFlow(key, builder, inFlowCollection: true);
            builder.Append(": ");
            EmitFlow(entryValue, builder, inFlowCollection: true);
        }
        builder.Append('}');
    }

    private static IEnumerable<(object Key, object? Value)> EnumerateEntries(
        System.Collections.IDictionary map)
    {
        foreach (System.Collections.DictionaryEntry entry in map)
        {
            yield return (entry.Key, entry.Value);
        }
    }

    private static void EmitBlockMapping(
        Dictionary<string, object?> map, int indent, List<string> lines)
    {
        string padding = new(' ', indent);
        foreach (KeyValuePair<string, object?> entry in map)
        {
            string key = FormatScalar(entry.Key);
            object? value = entry.Value;
            if (TryAsNestedMap(value, out Dictionary<string, object?>? nested))
            {
                // an empty block mapping has no representation; a bare "key:" reloads as null
                if (nested.Count == 0)
                {
                    lines.Add(padding + key + ": {}");
                    continue;
                }
                lines.Add(padding + key + ":");
                EmitBlockMapping(nested, indent + 2, lines);
            }
            else if (value is System.Collections.IEnumerable sequence and not string)
            {
                var elements = sequence.Cast<object?>().ToList();
                // an empty block sequence has no representation; a bare "key:" reloads as null
                if (elements.Count == 0)
                {
                    lines.Add(padding + key + ": []");
                    continue;
                }
                lines.Add(padding + key + ":");
                foreach (object? element in elements)
                {
                    // SnakeYAML's block style puts the dash at the key's indentation
                    if (TryAsNestedMap(element, out Dictionary<string, object?>? elementMap))
                    {
                        var sub = new List<string>();
                        EmitBlockMapping(elementMap, indent + 2, sub);
                        // the first entry of a map item shares the dash line; the dash plus
                        // space exactly replaces the two indent spaces, keeping alignment
                        sub[0] = padding + "- " + sub[0][(indent + 2)..];
                        lines.AddRange(sub);
                    }
                    else
                    {
                        lines.Add(padding + "- " + ToYamlString(element));
                    }
                }
            }
            else
            {
                lines.Add(padding + key + ": " + ToYamlString(value));
            }
        }
    }

    private static bool TryAsNestedMap(
        object? value, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Dictionary<string, object?>? nested)
    {
        switch (value)
        {
            case Dictionary<string, object?> direct:
                nested = direct;
                return true;
            case string:
                nested = null;
                return false;
            case System.Collections.IDictionary raw:
                nested = new Dictionary<string, object?>();
                foreach (System.Collections.DictionaryEntry entry in raw)
                {
                    nested[Convert.ToString(entry.Key, CultureInfo.InvariantCulture)!] = entry.Value;
                }
                return true;
            case IEnumerable<KeyValuePair<string, string>> stringMap:
                nested = new Dictionary<string, object?>();
                foreach (KeyValuePair<string, string> entry in stringMap)
                {
                    nested[entry.Key] = entry.Value;
                }
                return true;
            default:
                nested = null;
                return false;
        }
    }

    /// <summary>
    /// Quotes the scalar when required for YAML syntax or to preserve its string type. Single
    /// quotes are preferred (matching SnakeYAML); double quotes are used when the value contains
    /// characters that single quoting cannot represent.
    /// </summary>
    private static string FormatScalar(string value, bool inFlowCollection = false)
    {
        if (value.Any(c => char.IsControl(c) && c != '\t'))
        {
            var escaped = new StringBuilder("\"");
            foreach (char c in value)
            {
                switch (c)
                {
                    case '\\':
                        escaped.Append("\\\\");
                        break;
                    case '"':
                        escaped.Append("\\\"");
                        break;
                    case '\n':
                        escaped.Append("\\n");
                        break;
                    case '\r':
                        escaped.Append("\\r");
                        break;
                    case '\t':
                        escaped.Append("\\t");
                        break;
                    default:
                        if (char.IsControl(c))
                        {
                            escaped.Append("\\x").Append(((int)c).ToString("x2", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            escaped.Append(c);
                        }
                        break;
                }
            }
            return escaped.Append('"').ToString();
        }

        return NeedsQuoting(value, inFlowCollection) ? "'" + value.Replace("'", "''") + "'" : value;
    }

    private static bool NeedsQuoting(string value, bool inFlowCollection)
    {
        if (value.Length == 0)
        {
            return true;
        }
        // values that would resolve to a non-string type must be quoted to stay strings
        if (ResolveScalar(value) is not string)
        {
            return true;
        }
        if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]))
        {
            return true;
        }
        // leading YAML indicator characters
        if ("[]{}#&*!|>'\"%@`,-?:".Contains(value[0]))
        {
            return true;
        }
        // inside a flow collection the flow indicators terminate a plain scalar anywhere,
        // so "a,b" would otherwise be read back as two elements
        if (inFlowCollection && value.IndexOfAny([',', '[', ']', '{', '}']) >= 0)
        {
            return true;
        }
        return value.Contains(": ") || value.EndsWith(':') || value.Contains(" #") || value.Contains('\t');
    }
}
