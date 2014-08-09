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
using System.Text.RegularExpressions;

namespace SharpConfig
{
    public class ProgramOptions
    {
        private static ProgramOptions SetValue(string optionName, string optionValue, ProgramOptions options)
        {
            switch (optionName.Trim().ToLower())
            {
                case "--output-transform":
                    options.m_OutputTransform = optionValue;
                    return options;
                case "--file-mask":
                    options.m_FileMask = optionValue;
                    return options;
                case "--base-directory":
                    options.m_BaseDirectory = optionValue;
                    return options;
                case "--config-source":
                    options.m_ConfigSource = optionValue;
                    return options;
                case "--csv-quote":
                    var quote = (char)ConvertToChar(optionValue);
                    if (quote == char.MaxValue) return null;
                    options.m_CsvQuoteChar = (char)quote;
                    return options;
                case "--csv-delimiter":
                    var delim = (char)ConvertToChar(optionValue);
                    if (delim == char.MaxValue) return null;
                    options.m_CsvDelimiter = (char)delim;
                    return options;
                case "--output-directory":
                    options.m_OutputDirectory = optionValue;
                    return options;
                case "--default-environment":
                    options.m_DefaultEnvironment = optionValue;
                    return options;
                case "--verbosity":
                    options.m_Verbosity = optionValue;
                    return options;
                default:
                    return null;
            }
        }
        private static int ConvertToChar(string optionValue)
        {
            if (optionValue.Length == 1) return optionValue[0];
            if (optionValue.Length == 0) return '\0';
            if (optionValue.Length > 2) return -1;

            //unescape, if possible to get a single char. For example the 2-character string '\t' should give us a tab
            var s = Regex.Unescape(optionValue);
            if (s.Length == 1) return s[0];
            if (s.Length == 0) return '\0';
            return -1;
        }

        private const string EnvToken = "<env>";
        private string m_OutputTransform = @"\.template\.=>." + EnvToken + ".";
        private string m_FileMask = @"*.template.*";
        private string m_BaseDirectory = @"";
        private string m_ConfigSource = @"ConfigValues.csv";
        private char m_CsvQuoteChar = '"';
        private char m_CsvDelimiter = ',';
        private string m_OutputDirectory = ".";
        private string m_DefaultEnvironment = "dev";
        private string m_Verbosity = "quiet";

        public string OutputTransform
        {
            get { return m_OutputTransform; }
        }
        public string FileMask
        {
            get { return m_FileMask; }
        }
        public string BaseDirectory
        {
            get { return m_BaseDirectory; }
        }
        public string ConfigSource
        {
            get { return m_ConfigSource; }
        }
        public char CsvQuoteChar
        {
            get { return m_CsvQuoteChar; }
        }
        public char CsvDelimiter
        {
            get { return m_CsvDelimiter; }
        }
        public string OutputDirectory
        {
            get { return m_OutputDirectory; }
        }
        public string DefaultEnvironment
        {
            get { return m_DefaultEnvironment; }
        }
        public string Verbosity
        {
            get { return m_Verbosity; }
        }

        public string GetFullConfigurationSource()
        {
            var baseDir = GetFullBaseDirectory();
            return Path.Combine(baseDir, ConfigSource);
        }
        public string GetFullBaseDirectory()
        {
            if (Path.IsPathRooted(BaseDirectory)) return BaseDirectory;
            return Path.Combine(Environment.CurrentDirectory, BaseDirectory);
        }
        public string GetOutputFileForInputFile(string inputFile, string environment)
        {
            var ndx = this.OutputTransform.IndexOf("=>");
            var regex = this.OutputTransform.Substring(0, ndx);
            var replace = this.OutputTransform.Substring(ndx + 2, this.OutputTransform.Length - (ndx + 2)).Replace(EnvToken, environment);

            var inputFilenameWithoutDirectory = Path.GetFileName(inputFile);
            var inputDirectory = Path.GetDirectoryName(inputFile);

            var outputFilenameWithoutDirectory = Regex.Replace(inputFilenameWithoutDirectory, regex, replace);
            var result = Path.Combine(inputDirectory, this.OutputDirectory, outputFilenameWithoutDirectory);
            return result;
        }
        public string GetCopybackOutputFileForInputFile(string inputFile)
        {
            var ndx = this.OutputTransform.IndexOf("=>");
            var regex = this.OutputTransform.Substring(0, ndx);
            var replace = this.OutputTransform.Substring(ndx + 2, this.OutputTransform.Length - (ndx + 2)).Replace(EnvToken, "");

            var inputFilenameWithoutDirectory = Path.GetFileName(inputFile);
            var inputDirectory = Path.GetDirectoryName(inputFile);

            var outputFilenameWithoutDirectory = Regex.Replace(inputFilenameWithoutDirectory, regex, replace);
            while (outputFilenameWithoutDirectory.Contains(".."))
            {
                outputFilenameWithoutDirectory = outputFilenameWithoutDirectory.Replace("..", ".");
            }

            var result = Path.Combine(inputDirectory, outputFilenameWithoutDirectory);
            return result;
        }
        public void CreateOutputDirectory(string inputFile)
        {
            var inputDirectory = Path.GetDirectoryName(inputFile);
            var outputDirectoy = Path.Combine(inputDirectory, this.OutputDirectory);
            Directory.CreateDirectory(outputDirectoy);
        }
        public new string ToString(bool calculated)
        {
            var result = string.Format(
@"--output-transform ""{0}""
--file-mask ""{1}""
--base-directory ""{2}"" 
--config-source ""{3}""
--csv-quote ""{4}""
--csv-delimiter ""{5}""
--output-directory ""{6}""
--default-environment ""{7}""
--verbosity ""{8}""
",
                this.OutputTransform,
                this.FileMask,
                calculated ? this.GetFullBaseDirectory() : this.BaseDirectory,
                calculated ? this.GetFullConfigurationSource() : this.ConfigSource,
                this.CsvQuoteChar,
                this.CsvDelimiter,
                this.OutputDirectory,
                this.DefaultEnvironment,
                this.Verbosity);
            return result;
        }
        public override string ToString()
        {
            return this.ToString(false);
        }

        public static string ShortUsage()
        {
            var result =
@"
SharpConfig.exe
Copyright (c) 2014 Tristan Reeves

usage: SharpConfig.exe [Options]
Options are as follows:

--help
Prints this message.

--output-transform [pattern] 
default = '\.template\.=>." + EnvToken + @".'
Specifies the form of output files.

--file-mask [pattern] 
default = '*.template.*'
Specifies what files to apply configuration to.

--base-directory [relative or absolute directory] 
default = ''
Specifies the directory from which other paths are calculated.

--config-source [relative or absolute file path] 
default = 'ConfigValues.csv'
Specifies the location of the csv file containing the configuration values.

--csv-quote [single character]
default = '""'
Specifies the character to be used as a quote character in the csv file.

--csv-delimiter [single character]                          
default = ','
Specifies the character to be used as a delimiter in the csv file.

--output-directory [relative directory]
default = '.'
Specifies the directory where the output files are written. The directory is relative to the directory containing the input file.

--default-environment [string]
default = 'dev'
If specified and valid, indicates that the output file corresponding to this environment should be copied to a corresponding file without such an environment in its name.
For example, web.dev.config->web.config. The directory of the destination file is the same as the directory of the initial input file.

--verbosity [quiet|detailed]
default = 'quiet'
How much info do you want to see?
";
            return result;
        }
        public static ProgramOptions ParseCommandLine(string[] args)
        {
            try
            {
                var result = new ProgramOptions();
                if (args.Any(x => new[] { "--help", "-help", "--?", "-?" }.Contains(x.ToLower())))
                {
                    return null;
                }

                if (args.Length % 2 != 0) return null;
                for (int i = 0; i < args.Length; i += 2)
                {
                    var optionName = args[i];
                    var optionValue = args[i + 1];
                    result = SetValue(optionName, optionValue, result);
                    if (result == null) return null;
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        public bool PathsOk()
        {
            try
            {
                var fullBase = this.GetFullBaseDirectory();
                var fullConfigSource = this.GetFullConfigurationSource();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}