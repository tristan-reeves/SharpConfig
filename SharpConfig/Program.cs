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
using System.Linq;
using SharpConfig.Config;
using SharpConfig.Config.Implementation;
using SharpConfig.Csv;

namespace SharpConfig
{
    class Program
    {
        static int MainInternal(string[] args)
        {
            var options = ProgramOptions.ParseCommandLine(args);
            if (options == null)
            {
                var usage = ProgramOptions.ShortUsage();
                Console.Write(usage);
                return 1;
            }

            var configFile = options.GetFullConfigurationSource();
            var configContents = File.ReadAllText(configFile);

            var csvOptions = new BasicCsvOptions(ownsTextReader: false, delimiter: options.CsvDelimiter, quote: options.CsvQuoteChar);
            IConfigurationTopLevel configurationTopLevel = new DefaultConfigurationTopLevel(configContents, csvOptions);
            var runner = new ProgramRunner(options, configurationTopLevel);

            var baseDir = options.GetFullBaseDirectory();
            var files = Directory.EnumerateFiles(baseDir, options.FileMask, SearchOption.AllDirectories).ToArray();
            foreach (var file in files)
            {
                runner.Execute(file);
            }

            return 0;
        }
        static int Main(string[] args)
        {            
            try
            {
                return MainInternal(args);
            }
            catch (Exception ex)
            {
                var msg = "An unhandled exception occurred. The exception's message was [{0}]. Please check further output for more information.";
                Console.Error.WriteLine(msg, ex.Message);
                throw;
            }
        }
    }
}
