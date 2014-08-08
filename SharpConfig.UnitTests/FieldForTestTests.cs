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
using System;
using NUnit.Framework;

namespace SharpConfig.UnitTests
{
    [TestFixture]
    public class FieldForTestTests
    {
        private string DequoteStr(string str, string quoteChar)
        {
            var uniquePadding = Guid.NewGuid().ToString();
            while (str.Contains(uniquePadding))
            {
                uniquePadding = Guid.NewGuid().ToString();
            }

            return (uniquePadding + str + uniquePadding)
                .Replace(uniquePadding + quoteChar, "")
                .Replace(quoteChar + uniquePadding, "")
                .Replace(uniquePadding, "")
                .Replace(quoteChar + quoteChar, quoteChar);
        }

        [TestCase("", false, false)]
        [TestCase(" ", false, false)]
        [TestCase("x", false, false)]
        [TestCase(",", true, false)]
        [TestCase("x,", true, false)]
        [TestCase("x,y", true, false)]
        [TestCase(",y", true, false)]
        [TestCase("\r\n", true, false)]
        [TestCase("\n", true, false)]
        [TestCase("x\"", false, false)]
        [TestCase("x\" ", false, false)]
        [TestCase("\"x", true, false)]
        [TestCase(" \"x", false, false)]
        [TestCase("\"\"", false, true)]
        [TestCase("\"123, \r\n \n\"", false, true)]
        [TestCase("\"123, \"\r\n \n\"", true, false)]
        [TestCase("\"123, \"\"\r\n \n\"", false, true)]
        [TestCase("\"123, \r\n \n", true, false)]
        [TestCase("\"123, \"\r\n \n", true, false)]
        [TestCase("\"123, \"\"\r\n \n", true, false)]
        public void TestEm(string rawValue, bool needsQuote, bool isProperlyQuoted)
        {
            foreach (var quoteChar in new[] { "\"", "Q" })
            {
                foreach (var delimiterChar in new[] { ",", "|" })
                {
                    var value = rawValue.Replace("\"", quoteChar).Replace(",", delimiterChar);
                    TestEmInternl(value, needsQuote, isProperlyQuoted, quoteChar, delimiterChar);
                }
            }
        }

        private void TestEmInternl(string rawValue, bool needsQuote, bool isProperlyQuoted, string quoteChar, string delimiterChar)
        {
            var field = new FieldForTest(rawValue, quoteChar[0], delimiterChar[0]);

            Assert.That(field.RawValue, Is.EqualTo(rawValue));
            Assert.That(field.NeedsQuote, Is.EqualTo(needsQuote));
            Assert.That(field.IsProperlyQuoted, Is.EqualTo(isProperlyQuoted));

            var expectedNormlisedValue = isProperlyQuoted
                ? DequoteStr(rawValue, quoteChar)
                : rawValue;

            Assert.That(field.NormalisedValue, Is.EqualTo(expectedNormlisedValue));
            Assert.That(field.Quote().NormalisedValue, Is.EqualTo(rawValue));
            Assert.That(field.Quote().RawValue, Is.EqualTo(quoteChar + rawValue.Replace(quoteChar, quoteChar + quoteChar) + quoteChar));

            if (field.NeedsQuote)
            {
                Assert.That(field.QuoteIfNecessary().RawValue, Is.EqualTo(field.Quote().RawValue));
            }
            else
            {
                Assert.That(field.QuoteIfNecessary().RawValue, Is.EqualTo(field.RawValue));
            }
        }
    }
}