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
namespace SharpConfig.Csv
{
    public class TabularCsvOptions
    {
        private readonly bool m_HasHeaders;
        private readonly LongLineOptions m_LongLineOptions;
        private readonly ShortLineOptions m_ShortLineOptions;

        public TabularCsvOptions(bool hasHeaders = false, LongLineOptions longLineOptions = LongLineOptions.Truncate, ShortLineOptions shortLineOptions = ShortLineOptions.ExtendWithNulls)
        {
            m_HasHeaders = hasHeaders;
            m_LongLineOptions = longLineOptions;
            m_ShortLineOptions = shortLineOptions;
        }

        public bool HasHeaders
        {
            get { return m_HasHeaders; }
        }

        public LongLineOptions LongLineOptions
        {
            get { return m_LongLineOptions; }
        }

        public ShortLineOptions ShortLineOptions
        {
            get { return m_ShortLineOptions; }
        }
    }
}