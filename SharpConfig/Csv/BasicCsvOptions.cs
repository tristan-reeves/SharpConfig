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

namespace SharpConfig.Csv
{
    public class BasicCsvOptions
    {
        private readonly bool m_OwnsTextReader;
        private readonly char m_Delimiter;
        private readonly char m_Quote;
        private readonly bool m_WindowsLineEndings;
        private readonly bool m_UnixLineEndings;
        private readonly bool m_StrictQuotes;

        public BasicCsvOptions(bool ownsTextReader = false, char delimiter = ',', char quote = '"', bool strictQuotes = false)
        {
            if (delimiter == quote) throw new ArgumentException("deimiter and quote must be different from each other");
            if (delimiter == char.MaxValue) throw new ArgumentException("deimiter must not be equal to -1");
            if (delimiter == '\r') throw new ArgumentException("deimiter must not be equal to '\\r'");
            if (delimiter == '\n') throw new ArgumentException("deimiter must not be equal to '\\n'");
            if (quote == char.MaxValue) throw new ArgumentException("quote must not be equal to -1");
            if (quote == '\r') throw new ArgumentException("quote must not be equal to '\\r'");
            if (quote == '\n') throw new ArgumentException("quote must not be equal to '\\n'");

            m_OwnsTextReader = ownsTextReader;
            m_Delimiter = delimiter;
            m_Quote = quote;
            m_StrictQuotes = strictQuotes;

            m_UnixLineEndings = true;
            m_WindowsLineEndings = true;
        }
        public bool OwnsTextReader
        {
            get { return m_OwnsTextReader; }
        }
        public char Delimiter
        {
            get { return m_Delimiter; }
        }
        public char Quote
        {
            get { return m_Quote; }
        }
        public bool WindowsLineEndings
        {
            get { return m_WindowsLineEndings; }
        }
        public bool UnixLineEndings
        {
            get { return m_UnixLineEndings; }
        }

        public bool StrictQuotes
        {
            get { return m_StrictQuotes; }
        }
    }
}