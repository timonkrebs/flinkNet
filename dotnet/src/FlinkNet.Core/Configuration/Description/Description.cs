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
/// Description for <see cref="FlinkNet.Configuration.ConfigOption{T}"/>. Allows providing multiple
/// rich formats.
/// </summary>
[PublicEvolving]
public class Description
{
    private readonly IReadOnlyList<IBlockElement> _blocks;

    public static DescriptionBuilder Builder() => new();

    public IReadOnlyList<IBlockElement> GetBlocks() => _blocks;

    /// <summary>Builder for <see cref="Description"/>.</summary>
    [PublicEvolving]
    public class DescriptionBuilder
    {
        private readonly List<IBlockElement> _blocks = [];

        /// <summary>
        /// Adds a block of text with placeholders ("%s") that will be replaced with proper string
        /// representation of given <see cref="IInlineElement"/>. For example:
        ///
        /// <para><c>Text("This is a text with a link %s", Link("https://somepage", "to here"))</c></para>
        /// </summary>
        /// <param name="format">text with placeholders for elements</param>
        /// <param name="elements">elements to be put in the text</param>
        /// <returns>description with added block of text</returns>
        public DescriptionBuilder Text(string format, params IInlineElement[] elements)
        {
            _blocks.Add(TextElement.Text(format, elements));
            return this;
        }

        /// <summary>Creates a simple block of text.</summary>
        /// <param name="text">a simple block of text</param>
        /// <returns>block of text</returns>
        public DescriptionBuilder Text(string text)
        {
            _blocks.Add(TextElement.Text(text));
            return this;
        }

        /// <summary>Block of description add.</summary>
        /// <param name="block">block of description to add</param>
        /// <returns>block of description</returns>
        public DescriptionBuilder Add(IBlockElement block)
        {
            _blocks.Add(block);
            return this;
        }

        /// <summary>Creates a line break in the description.</summary>
        public DescriptionBuilder LineBreak()
        {
            _blocks.Add(LineBreakElement.LineBreak());
            return this;
        }

        /// <summary>Adds a bulleted list to the description.</summary>
        public DescriptionBuilder List(params IInlineElement[] elements)
        {
            _blocks.Add(ListElement.List(elements));
            return this;
        }

        /// <summary>Creates description representation.</summary>
        public Description Build() => new(_blocks);
    }

    private Description(IReadOnlyList<IBlockElement> blocks)
    {
        _blocks = blocks;
    }
}
