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

namespace FlinkNet.Configuration.Description;

/// <summary>Formatter that transforms <see cref="Description"/> into HTML representation.</summary>
public class HtmlFormatter : Formatter
{
    protected override void FormatLink(StringBuilder state, string link, string description) =>
        state.Append($"<a href=\"{link}\">{description}</a>");

    protected override void FormatLineBreak(StringBuilder state) => state.Append("<br />");

    protected override void FormatText(
        StringBuilder state,
        string format,
        string[] elements,
        ISet<TextElement.TextStyle> styles)
    {
        string escapedFormat = EscapeCharacters(format);

        string prefix = "";
        string suffix = "";
        if (styles.Contains(TextElement.TextStyle.Code))
        {
            prefix = "<code class=\"highlighter-rouge\">";
            suffix = "</code>";
        }
        state.Append(prefix);
        state.Append(SubstituteArguments(escapedFormat, elements));
        state.Append(suffix);
    }

    protected override void FormatList(StringBuilder state, string[] entries)
    {
        state.Append("<ul>");
        foreach (string entry in entries)
        {
            state.Append($"<li>{entry}</li>");
        }
        state.Append("</ul>");
    }

    protected override Formatter NewInstance() => new HtmlFormatter();

    private static string EscapeCharacters(string value) =>
        value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
