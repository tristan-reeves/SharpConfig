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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpConfig.Csv
{
    public class BasicCsvReader : IDisposable
    {
        private enum CsvState
        {
            FileStart,
            LineStart,
            ValueStart,
            ReadingSimpleValue,
            ReadingQuotedValue,
            DoneReadingQuotedValue,
            Done
        }
        private readonly StringBuilder m_Buffer = new StringBuilder();
        private readonly List<string> m_CurrentLine = new List<string>();
        private readonly TextReader m_TextReader;
        private readonly BasicCsvOptions m_Options;
        private CsvState m_CurrentCsvState = CsvState.FileStart;
        private string ReadNextField()
        {
            m_Buffer.Clear();
            var thisVal = m_TextReader.Read();

            if (thisVal == -1)
            {
                switch (m_CurrentCsvState)
                {
                    case CsvState.LineStart:
                    case CsvState.ValueStart:
                        m_CurrentCsvState = CsvState.Done;
                        return "";
                }

                m_CurrentCsvState = CsvState.Done;
                return null;
            }

            while (thisVal != -1)
            {
                switch (m_CurrentCsvState)
                {
                    case CsvState.FileStart:
                    case CsvState.LineStart:
                    case CsvState.ValueStart:
                        if (thisVal == m_Options.Delimiter)
                        {
                            m_CurrentCsvState = CsvState.ValueStart;
                            return "";
                        }
                        else if (thisVal == m_Options.Quote)
                        {
                            m_CurrentCsvState = CsvState.ReadingQuotedValue;
                        }
                        else
                        {
                            if ((thisVal == '\n') && m_Options.UnixLineEndings)
                            {
                                m_CurrentCsvState = CsvState.LineStart;
                                return "";
                            }
                            else if ((thisVal == '\r') && (m_TextReader.Peek() == '\n') && m_Options.WindowsLineEndings)
                            {
                                m_CurrentCsvState = CsvState.LineStart;
                                m_TextReader.Read();
                                return "";
                            }
                            else
                            {
                                m_CurrentCsvState = CsvState.ReadingSimpleValue;
                                m_Buffer.Append((char)thisVal);
                            }
                        }
                        break;
                    case CsvState.ReadingSimpleValue:
                        if (thisVal == m_Options.Delimiter)
                        {
                            m_CurrentCsvState = CsvState.ValueStart;
                            return m_Buffer.ToString();
                        }
                        else if ((thisVal == '\n') && m_Options.UnixLineEndings)
                        {
                            m_CurrentCsvState = CsvState.LineStart;
                            return m_Buffer.ToString();
                        }
                        else if ((thisVal == '\r') && (m_TextReader.Peek() == '\n') && m_Options.WindowsLineEndings)
                        {
                            m_CurrentCsvState = CsvState.LineStart;
                            m_TextReader.Read();
                            return m_Buffer.ToString();
                        }
                        else
                        {
                            m_Buffer.Append((char)thisVal);
                        }
                        break;
                    case CsvState.ReadingQuotedValue:
                        if (thisVal == m_Options.Quote)
                        {
                            if (m_TextReader.Peek() == m_Options.Quote)
                            {
                                m_TextReader.Read();
                                m_Buffer.Append((char)thisVal);
                            }
                            else
                            {
                                m_CurrentCsvState = CsvState.DoneReadingQuotedValue;
                            }
                        }
                        else
                        {
                            m_Buffer.Append((char)thisVal);
                        }
                        break;
                    case CsvState.DoneReadingQuotedValue:
                        if (thisVal == m_Options.Delimiter)
                        {
                            m_CurrentCsvState = CsvState.ValueStart;
                            return m_Buffer.ToString();
                        }
                        else if ((thisVal == '\n') && m_Options.UnixLineEndings)
                        {
                            m_CurrentCsvState = CsvState.LineStart;
                            return m_Buffer.ToString();
                        }
                        else if ((thisVal == '\r') && (m_TextReader.Peek() == '\n') && m_Options.WindowsLineEndings)
                        {
                            m_CurrentCsvState = CsvState.LineStart;
                            m_TextReader.Read();
                            return m_Buffer.ToString();
                        }
                        else
                        {
                            //technically should be an error, but we should be lenient
                            if (m_Options.StrictQuotes)
                            {
                                throw new InvalidOperationException("Invalid Quoted String");
                            }
                            m_CurrentCsvState = CsvState.ReadingSimpleValue;
                            m_Buffer.Append((char)thisVal);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                thisVal = m_TextReader.Read();
            }

            if ((m_Options.StrictQuotes) && (m_CurrentCsvState == CsvState.ReadingQuotedValue))
            {
                throw new InvalidOperationException("Unmatched Opening Quote");
            }
            return m_Buffer.ToString();
        }

        public BasicCsvReader(TextReader textReader, BasicCsvOptions options = null)
        {
            m_TextReader = textReader;
            m_Options = options ?? new BasicCsvOptions();
        }
        public string[] GetValues()
        {
            return m_CurrentLine.ToArray();
        }
        public string GetValue(int index)
        {
            return m_CurrentLine[index];
        }
        public int FieldCount
        {
            get { return m_CurrentLine.Count; }
        }
        public bool ReadLine()
        {
            m_CurrentLine.Clear();

            var nextField = ReadNextField();
            if (nextField == null) return false;

            m_CurrentLine.Add(nextField);
            while ((m_CurrentCsvState != CsvState.LineStart) && (nextField != null))
            {
                nextField = ReadNextField();
                if (nextField != null)
                {
                    m_CurrentLine.Add(nextField);
                }
            }

            return true;
        }
        public void Dispose()
        {
            if (!m_Options.OwnsTextReader) return;
            if (m_TextReader == null) return;
            ((IDisposable)m_TextReader).Dispose();
        }
    }
}
