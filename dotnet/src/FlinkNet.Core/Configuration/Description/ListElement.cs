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

/// <summary>Represents a list in the <see cref="Description"/>.</summary>
[PublicEvolving]
public class ListElement : IBlockElement
{
    private readonly IReadOnlyList<IInlineElement> _entries;

    /// <summary>Creates a list with blocks of text. For example:
    /// <code>
    /// ListElement.List(
    ///     TextElement.Text("this is first element of list"),
    ///     TextElement.Text("this is second element of list with a %s", LinkElement.Link("https://link")))
    /// </code>
    /// </summary>
    /// <param name="elements">list of this list's entries</param>
    /// <returns>list representation</returns>
    public static ListElement List(params IInlineElement[] elements) => new(elements);

    public IReadOnlyList<IInlineElement> GetEntries() => _entries;

    private ListElement(IReadOnlyList<IInlineElement> entries)
    {
        _entries = entries;
    }

    public void Format(Formatter formatter) => formatter.Format(this);
}
