// The MIT License (MIT)
// 
// Copyright (c) 2014 Tristan Reeves
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// 
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharpConfig.Config.Implementation
{
    public class DefaultTokenisingAlgorithm : ITokenisingAlgorithm
    {
        //private readonly Regex m_TokenRegex = new Regex(@"(?<!\$)\$\{[^\{\}]+\}");
        private readonly Regex m_TokenRegex = new Regex(@"(?<!\$)\$\{[^\}]+\}");
        public IEnumerable<IToken> Tokenise(string source)
        {
            var matches = m_TokenRegex.Matches(source)
                .OfType<Match>()
                .Where(g => g.Success)
                .OrderBy(g => g.Index)
                .ToArray();

            var startIndex = 0;
            foreach (var match in matches)
            {
                var sub = source.Substring(startIndex, match.Index - startIndex);
                if (sub.Length != 0)
                {
                    yield return new LiteralToken(sub);
                }
                yield return new ConfigurationKeyToken(match.Value);

                startIndex = match.Index + match.Length;
            }

            //get the tail...
            var tail = source.Substring(startIndex);
            if (tail.Length != 0)
            {
                yield return new LiteralToken(tail);
            }
        }
    }
}