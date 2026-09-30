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

/// <summary>Element that represents a link in the <see cref="Description"/>.</summary>
[PublicEvolving]
public class LinkElement : IInlineElement
{
    private readonly string _link;
    private readonly string _text;

    /// <summary>Creates a link with a given url and description.</summary>
    /// <param name="link">address that this link should point to</param>
    /// <param name="text">a description for that link, that should be used in text</param>
    /// <returns>link representation</returns>
    public static LinkElement Link(string link, string text) => new(link, text);

    /// <summary>Creates a link with a given url. This url will be used as a description for that link.</summary>
    /// <param name="link">address that this link should point to</param>
    /// <returns>link representation</returns>
    public static LinkElement Link(string link) => new(link, link);

    public string GetLink() => _link;

    public string GetText() => _text;

    private LinkElement(string link, string text)
    {
        _link = link;
        _text = text;
    }

    public void Format(Formatter formatter) => formatter.Format(this);
}
