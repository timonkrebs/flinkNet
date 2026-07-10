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

namespace FlinkNet.Configuration.Description;

/// <summary>
/// Represents a text block in the <see cref="Description"/>. The <c>%s</c> placeholders in the
/// format are substituted with the given inline elements when formatting.
/// </summary>
[PublicEvolving]
public class TextElement : IBlockElement, IInlineElement
{
    private readonly string _format;
    private readonly IReadOnlyList<IInlineElement> _elements;
    private readonly HashSet<TextStyle> _textStyles = [];

    /// <summary>
    /// Creates a block of text with placeholders ("%s") that will be replaced with proper string
    /// representation of given <see cref="IInlineElement"/>. For example:
    ///
    /// <para><c>Text("This is a text with a link %s", Link("https://somepage", "to here"))</c></para>
    /// </summary>
    /// <param name="format">text with placeholders for elements</param>
    /// <param name="elements">elements to be put in the text</param>
    /// <returns>block of text</returns>
    public static TextElement Text(string format, params IInlineElement[] elements) =>
        new(format, elements);

    /// <summary>Creates a simple block of text.</summary>
    /// <param name="text">a simple block of text</param>
    /// <returns>block of text</returns>
    public static TextElement Text(string text) => new(text, []);

    /// <summary>Wraps a list of <see cref="IInlineElement"/>s into a single one.</summary>
    public static IInlineElement Wrap(params IInlineElement[] elements) =>
        Text(string.Concat(Enumerable.Repeat("%s", elements.Length)), elements);

    /// <summary>Creates a block of text formatted as code.</summary>
    /// <param name="text">a block of text that will be formatted as code</param>
    /// <returns>block of text formatted as code</returns>
    public static TextElement Code(string text)
    {
        TextElement element = Text(text);
        element._textStyles.Add(TextStyle.Code);
        return element;
    }

    public string GetFormat() => _format;

    public IReadOnlyList<IInlineElement> GetElements() => _elements;

    public ISet<TextStyle> GetStyles() => _textStyles;

    private TextElement(string format, IReadOnlyList<IInlineElement> elements)
    {
        _format = format;
        _elements = elements;
    }

    public void Format(Formatter formatter) => formatter.Format(this);

    /// <summary>Styles that can be applied to <see cref="TextElement"/> e.g. code, bold etc.</summary>
    [PublicEvolving]
    public enum TextStyle
    {
        Code,
    }
}
