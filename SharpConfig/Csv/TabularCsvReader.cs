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
using System.IO;

namespace SharpConfig.Csv
{
    public class TabularCsvReader : IDisposable
    {
        private readonly BasicCsvReader m_BasicCsvReader;
        private readonly TabularCsvOptions m_Options;
        private string[] m_FirstLine;
        private string[] m_HeaderLine;
        private bool m_HasReadFirstLine;

        private void ReadFirstLine()
        {
            if (m_Options.HasHeaders)
            {
                m_BasicCsvReader.ReadLine();
                m_HeaderLine = m_BasicCsvReader.GetValues();
                m_BasicCsvReader.ReadLine();
                m_FirstLine = m_BasicCsvReader.GetValues();
            }
            else
            {
                m_BasicCsvReader.ReadLine();
                m_FirstLine = m_BasicCsvReader.GetValues();
                m_HeaderLine = new string[m_FirstLine.Length];
            }
        }
        public TabularCsvReader(BasicCsvReader basicCsvReader, TabularCsvOptions options = null)
        {
            m_BasicCsvReader = basicCsvReader;
            m_Options = options ?? new TabularCsvOptions();

            ReadFirstLine();
        }
        public TabularCsvReader(TextReader textReader, BasicCsvOptions basicCsvOptions = null, TabularCsvOptions tabularCsvOptions = null) 
            : this(new BasicCsvReader(textReader, basicCsvOptions), tabularCsvOptions)
        {
            
        }

        public int GetFieldCount()
        {
            if (!m_HasReadFirstLine) throw new InvalidOperationException("Must call ReadLine() before accessing FieldCount");
            var result = m_FirstLine.Length;
            if (result == ColumnCount) return ColumnCount;
            else if (result < ColumnCount)
            {
                switch (m_Options.ShortLineOptions)
                {
                    case ShortLineOptions.DoNothing:
                        return result;
                    case ShortLineOptions.ExtendWithNulls:
                        return ColumnCount;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                switch (m_Options.LongLineOptions)
                {
                    case LongLineOptions.DoNothing:
                        return result;
                    case LongLineOptions.Truncate:
                        return ColumnCount;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public int ColumnCount
        {
            get { return m_HeaderLine.Length; }
        }
        public bool ReadLine()
        {
            if (!m_HasReadFirstLine)
            {
                m_HasReadFirstLine = true;
                return (m_FirstLine.Length > 0);
            }
            else
            {
                var result = m_BasicCsvReader.ReadLine();
                m_FirstLine = m_BasicCsvReader.GetValues();
                return result;
            }
        }
        public int GetColumnOrdinal(string columnName)
        {
            if (columnName == null) throw new ArgumentNullException("columnName");
            if (!m_Options.HasHeaders) return -1;
            for (int i = 0; i < m_HeaderLine.Length; i++)
            {
                if (m_HeaderLine[i].Equals(columnName)) return i;
            }
            return -1;
        }
        public string GetColumnName(int index)
        {
            return m_HeaderLine[index];
        }
        public string GetFieldValue(int index)
        {
            if (!m_HasReadFirstLine) throw new InvalidOperationException("Must call ReadLine() before accessing elements");
            if (index >= ColumnCount)
            {
                switch (m_Options.LongLineOptions)
                {
                    case LongLineOptions.DoNothing:
                        return m_FirstLine[index];
                    case LongLineOptions.Truncate:
                        throw new IndexOutOfRangeException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else if (index >= m_FirstLine.Length)
            {
                switch (m_Options.ShortLineOptions)
                {
                    case ShortLineOptions.DoNothing:
                        return m_FirstLine[index];
                    case ShortLineOptions.ExtendWithNulls:
                        return null;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                return m_FirstLine[index];
            }
        }
        public void Dispose()
        {
            if (m_BasicCsvReader == null) return;
            m_BasicCsvReader.Dispose();
        }
    }
}