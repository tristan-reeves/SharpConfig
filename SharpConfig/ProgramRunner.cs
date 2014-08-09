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
using SharpConfig.Config;

namespace SharpConfig
{
    class ProgramRunner
    {
        private readonly ProgramOptions m_Options;
        private readonly IConfigurationTopLevel m_ConfigurationTopLevel;

        private void Prepare(string inputFilename)
        {
            m_Options.CreateOutputDirectory(inputFilename);

            foreach (var environment in m_ConfigurationTopLevel.EnumerateEnvironments())
            {
                var outputFile = m_Options.GetOutputFileForInputFile(inputFilename, environment);
                File.Delete(outputFile);
            }
        }

        public ProgramRunner(ProgramOptions options, IConfigurationTopLevel configurationTopLevel)
        {
            m_Options = options;
            m_ConfigurationTopLevel = configurationTopLevel;
        }

        public void Execute(string inputFilename)
        {
            Prepare(inputFilename);
            var inputContents = File.ReadAllText(inputFilename);
            Action copybackContinuation = null;

            foreach (var environment in m_ConfigurationTopLevel.EnumerateEnvironments())
            {
                var config = m_ConfigurationTopLevel.GetEnvironment(environment);
                var transformed = config.TransformInput(inputContents);

                var outputFile = m_Options.GetOutputFileForInputFile(inputFilename, environment);
                File.WriteAllText(outputFile, transformed);

                if (copybackContinuation == null)
                {
                    var copybackFile = m_Options.GetCopybackOutputFileForInputFile(inputFilename);
                    copybackContinuation = GetCopyBackDefaultEnvironmentContinuationIfAppropriate(environment, outputFile, copybackFile);
                }
            }

            if (copybackContinuation != null) copybackContinuation();
        }

        private Action GetCopyBackDefaultEnvironmentContinuationIfAppropriate(string environment, string sourceFile, string targetFile)
        {
            if (m_Options.DefaultEnvironment.Trim().ToLower() != environment.Trim().ToLower()) return null;
            return () => File.Copy(sourceFile, targetFile, true);
        }
    }
}