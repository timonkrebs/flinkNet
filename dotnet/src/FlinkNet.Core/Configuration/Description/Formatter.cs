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

using System.Text;
using FlinkNet.Annotations;

namespace FlinkNet.Configuration.Description;

/// <summary>
/// Allows providing multiple formatters for the description. E.g. HTML formatter, Markdown
/// formatter etc.
/// </summary>
[PublicEvolving]
public abstract class Formatter
{
    private readonly StringBuilder _state = new();

    /// <summary>Formats the description into a String using format specific tags.</summary>
    /// <param name="description">description to be formatted</param>
    /// <returns>string representation of the description</returns>
    public string Format(Description description)
    {
        foreach (IBlockElement blockElement in description.GetBlocks())
        {
            blockElement.Format(this);
        }
        return FinalizeFormatting();
    }

    public void Format(LinkElement element) =>
        FormatLink(_state, element.GetLink(), element.GetText());

    public void Format(TextElement element)
    {
        string[] inlineElements =
            element.GetElements()
                .Select(
                    el =>
                    {
                        Formatter formatter = NewInstance();
                        el.Format(formatter);
                        return formatter.FinalizeFormatting();
                    })
                .ToArray();
        FormatText(_state, element.GetFormat(), inlineElements, element.GetStyles());
    }

    public void Format(LineBreakElement element) => FormatLineBreak(_state);

    public void Format(ListElement element)
    {
        string[] inlineElements =
            element.GetEntries()
                .Select(
                    el =>
                    {
                        Formatter formatter = NewInstance();
                        el.Format(formatter);
                        return formatter.FinalizeFormatting();
                    })
                .ToArray();
        FormatList(_state, inlineElements);
    }

    private string FinalizeFormatting()
    {
        string result = _state.ToString();
        _state.Length = 0;
        return result;
    }

    protected abstract void FormatLink(StringBuilder state, string link, string description);

    protected abstract void FormatLineBreak(StringBuilder state);

    protected abstract void FormatText(
        StringBuilder state,
        string format,
        string[] elements,
        ISet<TextElement.TextStyle> styles);

    protected abstract void FormatList(StringBuilder state, string[] entries);

    protected abstract Formatter NewInstance();

    /// <summary>
    /// Substitutes the Java-style <c>%s</c> placeholders in the format with the given elements in
    /// order; <c>%%</c> collapses to a literal <c>%</c>.
    ///
    /// <para>PORT NOTE: Java routes this through <c>String.format</c> after escaping; the port
    /// substitutes directly with the same net behavior.</para>
    /// </summary>
    protected static string SubstituteArguments(string format, string[] elements)
    {
        var result = new StringBuilder(format.Length);
        int nextElement = 0;
        for (int i = 0; i < format.Length; i++)
        {
            char c = format[i];
            if (c == '%' && i + 1 < format.Length)
            {
                char next = format[i + 1];
                if (next == 's')
                {
                    result.Append(elements[nextElement++]);
                    i++;
                    continue;
                }
                if (next == '%')
                {
                    result.Append('%');
                    i++;
                    continue;
                }
            }
            result.Append(c);
        }
        return result.ToString();
    }
}
