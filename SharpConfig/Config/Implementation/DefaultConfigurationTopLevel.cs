﻿// The MIT License (MIT)
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
using System.IO;
using System.Linq;
using System.Threading;
using SharpConfig.Csv;
using SharpConfig.Exceptions;

namespace SharpConfig.Config.Implementation
{
    public class DefaultConfigurationTopLevel : IConfigurationTopLevel
    {
        private readonly Dictionary<string, IConfigurationEnvironment> m_Environments = new Dictionary<string, IConfigurationEnvironment>();
        private void VerifyEnvironments(string[] environmentNames)
        {
            var lowerCase = environmentNames.Select(x => x.Trim().ToLower()).ToArray();
            if (lowerCase.Any(s => s == ""))
            {
                throw new BlankEnvironmentException();
            }

            var duplicates = lowerCase.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
            if (duplicates.Any())
            {
                throw new DuplicateEnvironmentException(duplicates);
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var invalidEnvironments = lowerCase.Where(s => s.Intersect(invalidChars).Any()).ToArray();
            if (invalidEnvironments.Any())
            {
                throw new InvalidCharEnvironmentException(invalidEnvironments);
            }
        }
        private void VerifyKeys(List<string> allKeys)
        {
            var lowerCase = allKeys.Select(x => x.Trim().ToLower()).ToArray();
            if (lowerCase.Any(s => s == ""))
            {
                throw new BlankKeyException();
            }

            var duplicates = lowerCase.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
            if (duplicates.Any())
            {
                throw new DuplicateKeyException(duplicates);
            }

            var invalidChars = new[] { '}' };
            var invalidKeys = lowerCase.Where(s => s.Intersect(invalidChars).Any()).ToArray();
            if (invalidKeys.Any())
            {
                throw new InvalidCharKeyException(invalidKeys);
            }
        }
        private void ReadCsv(TextReader csvContents, BasicCsvOptions csvOptions)
        {
            using (var r = new BasicCsvReader(csvContents, csvOptions))
            {
                if (!r.ReadLine()) return;
                var environmentNames = r.GetValues().Skip(1).ToArray();
                VerifyEnvironments(environmentNames);

                var dicts = environmentNames.ToDictionary(x => x, x => new Dictionary<string, string>());

                var allKeys = new List<string>();
                var lineCounter = 1;
                while (r.ReadLine())
                {
                    lineCounter++;
                    var values = r.GetValues();
                    var key = values[0].Trim().ToLower();
                    allKeys.Add(key);
                    if (values.Length != environmentNames.Length + 1)
                    {
                        throw new CsvWrongFieldCountException(lineCounter, values.Length, environmentNames.Length + 1);
                    }

                    for (int i = 0; i < environmentNames.Length; i++)
                    {
                        var environment = environmentNames[i];
                        var keyLookupValue = values[i + 1];
                        dicts[environment].Add(key, keyLookupValue);
                    }
                }
                VerifyKeys(allKeys);

                foreach (var kv in dicts)
                {
                    var envName = kv.Key;
                    var dict = kv.Value;
                    var env = new DefaultConfigurationEnvironment(
                        envName,
                        new DefaultTokenisingAlgorithm(),
                        new DefaultTokenVisitorFactory(),
                        new DefaultConfigurationMapper(dict));
                    m_Environments.Add(envName, env);
                }
            }
        }

        public DefaultConfigurationTopLevel(TextReader csvContents, BasicCsvOptions csvOptions = null)
        {
            ReadCsv(csvContents, csvOptions);
        }
        public DefaultConfigurationTopLevel(string csvContents, BasicCsvOptions csvOptions = null)
        {
            using (var sr = new StringReader(csvContents))
            {
                ReadCsv(sr, csvOptions);
            }
        }

        public IConfigurationEnvironment GetEnvironment(string environmentName)
        {
            IConfigurationEnvironment result;
            if (!m_Environments.TryGetValue(environmentName, out result)) return null;

            return result;
        }

        public IEnumerable<string> EnumerateEnvironments()
        {
            return m_Environments.Keys;
        }
    }
}