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
using System.Configuration;
using System.IO;
using System.Linq;
using SharpConfig.Common;
using SharpConfig.Config;
using SharpConfig.Config.Implementation;
using SharpConfig.Csv;
using SharpConfig.Exceptions;

namespace SharpConfig
{
    class Program
    {
        private static int MainInternal(string[] args)
        {
            var options = ProgramOptions.ParseCommandLine(args);
            if (options == null)
            {
                var usage = ProgramOptions.ShortUsage();
                Console.Write(usage);
                return 1;
            }
            ILog log = new Log(options.Verbosity.Trim().ToLower() == "verbose");

            var ok = options.PathsOk();
            if (!ok)
            {
                var msg = @"There is a problem with the paths supplied. Please check the options below.
                {0}";
                msg = string.Format(msg, options);
                log.Error(msg);
                return 1;
            }

            log.Debug("Command line = [" + Environment.CommandLine + "]");
            log.Debug("Raw Options = \r\n" + options.ToString(false) + "\r\n");
            log.Debug("Computed Options = \r\n" + options.ToString(true) + "\r\n");

            var configFile = options.GetFullConfigurationSource();
            if (!File.Exists(configFile))
            {
                log.Error("It looks like the configuration source file [" + configFile + "] does not exist. This file is mandatory.");
                return 1;
            }

            string configContents;
            try
            {
                configContents = File.ReadAllText(configFile);
            }
            catch (Exception)
            {
                log.Error("It looks like something went wrong trying to read from the configuration source file [" + configFile + "]");
                return 1;
            }

            var csvOptions = new BasicCsvOptions(ownsTextReader: false, delimiter: options.CsvDelimiter, quote: options.CsvQuoteChar);
            IConfigurationTopLevel configurationTopLevel = null;

            try
            {
                configurationTopLevel = new DefaultConfigurationTopLevel(configContents, csvOptions);
            }
            catch (ConfigurationLoadingException ex)
            {
                var msg = "An error occurred while reading the configuration file [" + configFile + "]";
                msg += "\r\n" + "The error is: " + ex.Message;
                log.Error(msg);
                return 1;
            }
            catch (Exception ex)
            {
                var msg = "An unexpected error occurred while reading the configuration file [" + configFile + "]";
                msg += "\r\n" + "The error is: " + ex.Message;
                log.Error(msg);
                return 1;
            }
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
                msg += "\r\nThe command line was [{1}]";
                Console.Error.WriteLine(msg, ex.Message, Environment.CommandLine);
                throw;
            }
        }
    }
}
