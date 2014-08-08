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
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SharpConfig.Csv;

namespace SharpConfig.UnitTests
{
    [TestFixture]
    public class CsvSingleValueTests
    {
        private class DelimiterQuotePair
        {
            public char Quote;
            public char Delimiter;

            public DelimiterQuotePair(char quote, char delimiter)
            {
                Quote = quote;
                Delimiter = delimiter;
            }
        }

        private IEnumerable<DelimiterQuotePair> GetAllCombinations()
        {
            foreach (var quote in new[] { '"', '#' })
            {
                foreach (var delimiter in new[] { ',', '|' })
                {
                    yield return new DelimiterQuotePair(quote, delimiter);
                }
            }
        }

        private FieldForTest GetFieldForTest(string raw, DelimiterQuotePair pair)
        {
            var modified = raw
                .Replace("\"", pair.Quote.ToString())
                .Replace(",", pair.Delimiter.ToString());
            return new FieldForTest(modified, pair.Quote, pair.Delimiter);
        }

        private IEnumerable<string> GetProperFieldValues()
        {
            yield return "";
            yield return " ";
            yield return "a";
            yield return ".";
            yield return " \"";
            yield return " \"123\"";
            yield return "\r";
            yield return "\t";
        }
        private IEnumerable<string> GetImproperFieldValues()
        {
            yield return ",";
            yield return "\n";
            yield return "\r\n";
            yield return "\",";
            yield return "\"";
            yield return " ,";
            yield return " \n";
            yield return " \r\n";
        }
        private IEnumerable<string> GetAllFieldValues()
        {
            return GetProperFieldValues().Concat(GetImproperFieldValues());
        }
        private IEnumerable<FieldForTest> GetProperFields()
        {
            var fieldValues = GetProperFieldValues().ToArray();
            foreach (var pair in GetAllCombinations().ToArray())
            {
                foreach (var fv in fieldValues)
                {
                    yield return GetFieldForTest(fv, pair);
                }
            }
        }
        private IEnumerable<FieldForTest> GetImproperFields()
        {
            var fieldValues = GetImproperFieldValues().ToArray();
            foreach (var pair in GetAllCombinations().ToArray())
            {
                foreach (var fv in fieldValues)
                {
                    yield return GetFieldForTest(fv, pair);
                }
            }
        }
        private IEnumerable<FieldForTest> GetAllFields()
        {
            var fieldValues = GetAllFieldValues().ToArray();
            foreach (var pair in GetAllCombinations().ToArray())
            {
                foreach (var fv in fieldValues)
                {
                    yield return GetFieldForTest(fv, pair);
                }
            }
        }
        private IEnumerable<FieldForTest> GetQuotedFields()
        {
            foreach (var f in GetAllFields())
            {
                yield return f.Quote();
            }
        }
        private IEnumerable<FieldForTest[]> GetAllFieldPairs()
        {
            var fieldValues = GetAllFieldValues().ToArray();
            foreach (var pair in GetAllCombinations().ToArray())
            {
                foreach (var fv1 in fieldValues)
                {
                    foreach (var fv2 in fieldValues)
                    {
                        yield return new FieldForTest[] { GetFieldForTest(fv1, pair), GetFieldForTest(fv2, pair) };
                    }
                }
            }
        }

        [TestCaseSource("GetQuotedFields")]
        public void CheckQuotedFields(FieldForTest field)
        {
            Assert.True(field.IsProperlyQuoted);
        }
        [TestCaseSource("GetImproperFields")]
        public void CheckImproperFields(FieldForTest field)
        {
            Assert.False(field.IsProperlyQuoted);
            Assert.True(field.NeedsQuote);
            Assert.That(field.NormalisedValue, Is.EqualTo(field.RawValue));
            Assert.That(field.QuoteIfNecessary().RawValue, Is.EqualTo(field.Quote().RawValue));
            Assert.That(field.NormalisedValue, Is.Not.EqualTo(field.Quote().RawValue));
        }
        [TestCaseSource("GetProperFields")]
        public void CheckProperFields(FieldForTest field)
        {
            Assert.False(field.IsProperlyQuoted);
            Assert.False(field.NeedsQuote);
            Assert.That(field.NormalisedValue, Is.EqualTo(field.RawValue));
            Assert.That(field.QuoteIfNecessary().RawValue, Is.EqualTo(field.RawValue));
            Assert.That(field.NormalisedValue, Is.Not.EqualTo(field.Quote().RawValue));
        }

        [TestCaseSource("GetAllFields")]
        [TestCaseSource("GetQuotedFields")]
        public void ReadQuotedCsvValue(FieldForTest field)
        {
            if (field.RawValue == "") return;
            var value = field.QuoteIfNecessary().RawValue;
            var csv = new BasicCsvReader(new StringReader(value), new BasicCsvOptions(false, field.Delimiter[0], field.QuoteChar[0]));

            var ok = csv.ReadLine();
            var length = csv.FieldCount;
            var readValues = csv.GetValues().ToArray();

            Assert.True(ok);
            Assert.That(length, Is.EqualTo(1));
            Assert.That(length, Is.EqualTo(readValues.Length));
            Assert.That(readValues[0], Is.EqualTo(field.NormalisedValue));

            ok = csv.ReadLine();
            Assert.False(ok);
        }

        [TestCaseSource("GetImproperFields")]
        public void ReadImproperCsvValue(FieldForTest field)
        {
            var value = field.RawValue;
            var csv = new BasicCsvReader(new StringReader(value), new BasicCsvOptions(false, field.Delimiter[0], field.QuoteChar[0]));

            var ok = csv.ReadLine();
            var readValues = csv.GetValues().ToArray();

            Assert.True(ok);
            Assert.That(readValues[0], Is.Not.EqualTo(field.NormalisedValue));
        }

        [TestCaseSource("GetProperFields")]
        [TestCaseSource("GetQuotedFields")]
        public void ReadProperCsvValue(FieldForTest field)
        {
            if (field.RawValue == "") return;
            var value = field.RawValue;
            var csv = new BasicCsvReader(new StringReader(value), new BasicCsvOptions(false, field.Delimiter[0], field.QuoteChar[0]));

            var ok = csv.ReadLine();
            var readValues = csv.GetValues().ToArray();

            Assert.True(ok);
            Assert.That(readValues[0], Is.EqualTo(field.NormalisedValue));
        }

        [Test]
        public void ReadEmptyCsvLine(
            [Values(',', '|')]char delimiter,
            [Values('"', '#')]char quote)
        {
            var line = "";
            var csv = new BasicCsvReader(new StringReader(line), new BasicCsvOptions(false, delimiter, quote));

            var ok = csv.ReadLine();
            var length = csv.FieldCount;
            var readValues = csv.GetValues();

            Assert.False(ok);
            Assert.That(length, Is.EqualTo(0));
            Assert.That(readValues, Has.Length.EqualTo(length));
        }

        [TestCaseSource("GetAllFieldPairs")]
        public void ReadValidLine(FieldForTest field1, FieldForTest field2)
        {
            var line = field1.QuoteIfNecessary().RawValue + field1.Delimiter + field2.QuoteIfNecessary().RawValue;
            var csv = new BasicCsvReader(new StringReader(line), new BasicCsvOptions(false, field1.Delimiter[0], field1.QuoteChar[0]));

            csv.ReadLine();
            var fieldCount = csv.FieldCount;

            Assert.That(fieldCount, Is.EqualTo(2));
            Assert.That(csv.GetValue(0), Is.EqualTo(field1.RawValue));
            Assert.That(csv.GetValue(1), Is.EqualTo(field2.RawValue));
            Assert.That(csv.ReadLine(), Is.False);
        }

        [TestCaseSource("GetAllFieldPairs")]
        public void ReadInvalidLine(FieldForTest field1, FieldForTest field2)
        {
            ReadInvalidLineInternal(field1, field2, "\r\n");
            ReadInvalidLineInternal(field1, field2, "");
        }
        private void ReadInvalidLineInternal(FieldForTest field1, FieldForTest field2, string eol)
        {
            var line = field1.RawValue + field1.Delimiter + field2.RawValue;
            var csv = new BasicCsvReader(new StringReader(line + eol), new BasicCsvOptions(false, field1.Delimiter[0], field1.QuoteChar[0]));

            csv.ReadLine();
            var fieldCount = csv.FieldCount;

            if (field1.NeedsQuote)
            {
                Assert.That(csv.GetValue(0), Is.Not.EqualTo(field1.RawValue));
            }
            else if (field2.NeedsQuote)
            {
                Assert.That(csv.GetValue(1), Is.Not.EqualTo(field2.RawValue));
            }
            else
            {
                Assert.That(csv.GetValue(0), Is.EqualTo(field1.RawValue));
                Assert.That(csv.GetValue(1), Is.EqualTo(field2.RawValue));
            }
        }

        [TestCaseSource("GetAllFieldPairs")]
        public void ReadInvalidSecondLine(FieldForTest field1, FieldForTest field2)
        {
            var line = field1.RawValue + field1.Delimiter + field2.RawValue;
            var csv = new BasicCsvReader(new StringReader("\r\n" + line), new BasicCsvOptions(false, field1.Delimiter[0], field1.QuoteChar[0]));

            csv.ReadLine();
            var fieldCount = csv.FieldCount;
            Assert.That(fieldCount, Is.EqualTo(1));
            csv.ReadLine();

            if (field1.NeedsQuote)
            {
                Assert.That(csv.GetValue(0), Is.Not.EqualTo(field1.RawValue));
            }
            else if (field2.NeedsQuote)
            {
                Assert.That(csv.GetValue(1), Is.Not.EqualTo(field2.RawValue));
            }
            else
            {
                Assert.That(csv.GetValue(0), Is.EqualTo(field1.RawValue));
                Assert.That(csv.GetValue(1), Is.EqualTo(field2.RawValue));
            }
        }

        [TestCaseSource("GetAllFieldPairs")]
        public void ReadValid2Lines(FieldForTest field1, FieldForTest field2)
        {
            ReadValid2LinesInternal(field1, field2, "\n");
            ReadValid2LinesInternal(field1, field2, "\r\n");
        }
        public void ReadValid2LinesInternal(FieldForTest field1, FieldForTest field2, string lineSeparator)
        {
            if ((lineSeparator == "\n") && (field2.QuoteIfNecessary().RawValue.EndsWith("\r")))
            {
                return;
            }
            var line = field1.QuoteIfNecessary().RawValue + field1.Delimiter + field2.QuoteIfNecessary().RawValue;
            var csv = new BasicCsvReader(new StringReader(line + lineSeparator + line), new BasicCsvOptions(false, field1.Delimiter[0], field1.QuoteChar[0]));

            for (int i = 0; i < 2; i++)
            {
                csv.ReadLine();
                var fieldCount = csv.FieldCount;
                Assert.That(fieldCount, Is.EqualTo(2));
                Assert.That(csv.GetValue(0), Is.EqualTo(field1.RawValue));
                Assert.That(csv.GetValue(1), Is.EqualTo(field2.RawValue));
            }
            Assert.That(csv.ReadLine(), Is.False);
        }

        [Test]
        public void ReadSpecialLine([ValueSource("GetAllFields")] FieldForTest field1)
        {
            var field2 = new FieldForTest("\r");
            var line = field1.QuoteIfNecessary().RawValue + field1.Delimiter + field2.QuoteIfNecessary().RawValue;
            var csv = new BasicCsvReader(new StringReader(line + "\n" + line), new BasicCsvOptions(false, field1.Delimiter[0], field1.QuoteChar[0]));

            csv.ReadLine();
            var fieldCount = csv.FieldCount;
            Assert.That(fieldCount, Is.EqualTo(2));
            Assert.That(csv.GetValue(0), Is.EqualTo(field1.RawValue));
            Assert.That(csv.GetValue(1), Is.EqualTo(""));

            csv.ReadLine();
            fieldCount = csv.FieldCount;
            Assert.That(fieldCount, Is.EqualTo(2));
            Assert.That(csv.GetValue(0), Is.EqualTo(field1.RawValue));
            Assert.That(csv.GetValue(1), Is.EqualTo(field2.RawValue));

            Assert.That(csv.ReadLine(), Is.False);
        }

        [Test]
        public void ReadThreeLines(
            [Values(',', '|')]char delimiter,
            [Values('"', '#')]char quote,
            [Values("\n", "\r\n")]string eol)
        {
            var lines = new[]
            {
                "1,2,3",
                "'4,5','6,7','8,9'",
                "'10\r\n11','12\n13'",
                " "
            };
            lines = lines
                .Select(x => x.Replace('\'', quote).Replace(',', delimiter))
                .ToArray();

            var file = string.Join(eol, lines);
            var csv = new BasicCsvReader(new StringReader(file), new BasicCsvOptions(false, delimiter, quote));

            Assert.True(csv.ReadLine());
            Assert.That(csv.FieldCount, Is.EqualTo(3));
            Assert.That(csv.GetValue(0), Is.EqualTo("1"));
            Assert.That(csv.GetValue(1), Is.EqualTo("2"));
            Assert.That(csv.GetValue(2), Is.EqualTo("3"));

            Assert.True(csv.ReadLine());
            Assert.That(csv.FieldCount, Is.EqualTo(3));
            Assert.That(csv.GetValue(0), Is.EqualTo("4,5".Replace(',', delimiter)));
            Assert.That(csv.GetValue(1), Is.EqualTo("6,7".Replace(',', delimiter)));
            Assert.That(csv.GetValue(2), Is.EqualTo("8,9".Replace(',', delimiter)));

            Assert.True(csv.ReadLine());
            Assert.That(csv.FieldCount, Is.EqualTo(2));
            Assert.That(csv.GetValue(0), Is.EqualTo("10\r\n11"));
            Assert.That(csv.GetValue(1), Is.EqualTo("12\n13"));

            Assert.True(csv.ReadLine());
            Assert.That(csv.FieldCount, Is.EqualTo(1));
            Assert.That(csv.GetValue(0), Is.EqualTo(" "));

            Assert.False(csv.ReadLine());
        }

        [Test]
        public void ReadThreeLines2(
            [Values(',', '|')]char delimiter,
            [Values('"', '#')]char quote,
            [Values("\n", "\r\n")]string eol)
        {
            var lines = new[]
            {
                "'1','2','3'",
                "'4,5' a,'6,7' b,'8,9' c",
                "'10\r\n11','12\n13'",
                "' '"
            };
            lines = lines
                .Select(x => x.Replace('\'', quote).Replace(',', delimiter))
                .ToArray();

            var file = string.Join(eol, lines);
            var csv = new BasicCsvReader(new StringReader(file), new BasicCsvOptions(false, delimiter, quote));

            Assert.True(csv.ReadLine());
            Assert.That(csv.FieldCount, Is.EqualTo(3));
            Assert.That(csv.GetValue(0), Is.EqualTo("1"));
            Assert.That(csv.GetValue(1), Is.EqualTo("2"));
            Assert.That(csv.GetValue(2), Is.EqualTo("3"));

            Assert.True(csv.ReadLine());
            Assert.That(csv.FieldCount, Is.EqualTo(3));
            Assert.That(csv.GetValue(0), Is.EqualTo("4,5 a".Replace(',', delimiter)));
            Assert.That(csv.GetValue(1), Is.EqualTo("6,7 b".Replace(',', delimiter)));
            Assert.That(csv.GetValue(2), Is.EqualTo("8,9 c".Replace(',', delimiter)));

            Assert.True(csv.ReadLine());
            Assert.That(csv.FieldCount, Is.EqualTo(2));
            Assert.That(csv.GetValue(0), Is.EqualTo("10\r\n11"));
            Assert.That(csv.GetValue(1), Is.EqualTo("12\n13"));

            Assert.True(csv.ReadLine());
            Assert.That(csv.FieldCount, Is.EqualTo(1));
            Assert.That(csv.GetValue(0), Is.EqualTo(" "));

            Assert.False(csv.ReadLine());
        }

        [Test]
        public void DisposesTextReaderIfInstructedTo([Values(true, false)]bool ownsReader)
        {
            var tr = new MyTextReader();

            var csv = new BasicCsvReader(tr, new BasicCsvOptions(ownsReader));
            csv.Dispose();

            Assert.That(tr.WasDisposed, Is.EqualTo(ownsReader));
        }

        [Test]
        public void StrictQuotesCausesExceptions(
            [Values("a,'df' ", "a,'df ", "a,'sdf\r\n")] string line,
            [Values(',', '|')]char delimiter,
            [Values('"', '#')]char quote,
            [Values(true, false)] bool strict)
        {
            var file = line.Replace(',', delimiter).Replace('\'', quote);
            var csv = new BasicCsvReader(new StringReader(file), new BasicCsvOptions(false, delimiter, quote, strict));

            TestDelegate td = () => csv.ReadLine();

            if(strict) Assert.That(td, Throws.Exception);
            else Assert.That(td, Throws.Nothing);
        }
    }
}