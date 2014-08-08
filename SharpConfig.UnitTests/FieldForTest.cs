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

namespace SharpConfig.UnitTests
{
    //self describing fields
    public class FieldForTest
    {
        private readonly string m_QuoteChar;
        private readonly string m_Delimiter;
        private readonly bool m_NeedsQuote;
        private readonly string m_RawValue;
        private readonly string m_NormalisedValue;
        private readonly bool m_IsProperlyQuoted;

        private bool CalculateIsProperlyQuoted(string str)
        {
            if (!str.StartsWith(QuoteChar)) return false;
            if (!str.EndsWith(QuoteChar)) return false;
            if (str.Length == 1) return false;
            var hasOddQuotes = (str.Substring(1, str.Length - 2))
                .Replace(QuoteChar + QuoteChar, "x")
                .Contains(QuoteChar);

            return !hasOddQuotes;
        }
        private string QuoteStr(string str)
        {
            return QuoteChar + str.Replace(QuoteChar, QuoteChar + QuoteChar) + QuoteChar;
        }
        private string DequoteStr(string str)
        {
            var uniquePadding = Guid.NewGuid().ToString();
            while (str.Contains(uniquePadding))
            {
                uniquePadding = Guid.NewGuid().ToString();
            }

            return (uniquePadding + str + uniquePadding)
                .Replace(uniquePadding + QuoteChar, "")
                .Replace(QuoteChar + uniquePadding, "")
                .Replace(uniquePadding, "")
                .Replace(QuoteChar+QuoteChar, QuoteChar);
        }

        public FieldForTest QuoteIfNecessary()
        {
            if (NeedsQuote) return Quote();
            else return this;
        }
        public FieldForTest Quote()
        {
            return new FieldForTest(QuoteStr(RawValue), QuoteChar[0], Delimiter[0]);
        }
        public FieldForTest(string rawValue, char quoteChar = '"', char delimiter = ',')
        {
            m_RawValue = rawValue;
            m_QuoteChar = quoteChar.ToString();
            m_Delimiter = delimiter.ToString();
            m_IsProperlyQuoted = CalculateIsProperlyQuoted(RawValue);
            m_NormalisedValue = IsProperlyQuoted ? DequoteStr(RawValue) : RawValue;

            if (IsProperlyQuoted) m_NeedsQuote = false;
            else
            {
                m_NeedsQuote = (rawValue.Contains(Delimiter) || rawValue.Contains("\n")) || rawValue.StartsWith(QuoteChar);
            }
        }
        public string RawValue
        {
            get { return m_RawValue; }
        }
        public string NormalisedValue
        {
            get { return m_NormalisedValue; }
        }
        public bool IsProperlyQuoted
        {
            get { return m_IsProperlyQuoted; }
        }
        public bool NeedsQuote
        {
            get { return m_NeedsQuote; }
        }

        public string QuoteChar
        {
            get { return m_QuoteChar; }
        }

        public string Delimiter
        {
            get { return m_Delimiter; }
        }
    }
}