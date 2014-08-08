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
using Moq;
using NUnit.Framework;
using SharpConfig.Config;
using SharpConfig.Config.Implementation;
using SharpConfig.Csv;

namespace SharpConfig.UnitTests
{
    [TestFixture]
    public class ConfigurationEnvironmentsTests
    {
        private IConfigurationEnvironments GetDefaultConfigSystem(char quote, char delimiter)
        {
            var csv =
    @",dev,uat,""prod""
""key1"",dev-key1,uat-key1,prod-key1
key2,""dev-key2"",uat-key2,prod-key2
qualified.key3,""dev-qualified.key3"",uat-qualified.key3,prod-qualified.key3";

            csv = csv
                .Replace('"', quote)
                .Replace(',', delimiter);
            return new DefaultConfigurationEnvironments(new StringReader(csv), new BasicCsvOptions(false, delimiter, quote));
        }

        private string GetDefaultText()
        {
            return @"key1 key2 qualified.key3
${key1} ${key2} ${qualified.key3}
${key1}${key2}${qualified.key3}
${key1 } ${key2 } ${qualified.key3 }
${ key1} ${ key2} ${ qualified.key3}
$${key1} $${key2} $${qualified.key3}

${key1{} ${key2{} ${qualified.key3{}
$${key1{} $${key2{} $${qualified.key3{}

${-key1} ${-key2} ${-qualified.key3}
$${-key1} $${-key2} $${-qualified.key3}
";
        }

        [TestCase("hello", "hello")]
        [TestCase("hel lo", "hel lo")]
        [TestCase("hello ", "hello ")]
        [TestCase(" hello", " hello")]
        [TestCase("", "")]
        [TestCase(" ", " ")]
        [TestCase("$${}", "${}")]
        [TestCase("${}", "${}")]
        [TestCase("$${something}", "${something}")]
        [TestCase("${something}", "something")]
        [TestCase("${something", "${something")]
        [TestCase("${something{", "${something{")]
        [TestCase("${something{}", "something{")]
        [TestCase("${something{ }", "something{ ")]
        [TestCase("${something}}", "something}")]
        [TestCase("$${something", "${something")]
        [TestCase("this was ${something} very ${unusual} ${sir{}", "this was something very unusual sir{")]
        [TestCase("this was ${something} very \r\n ${unusual} !!!", "this was something very \r\n unusual !!!")]
        public void SimpleExperiments(string input, string expected)
        {
            var m = new Mock<IConfigurationMapper>();
            m.Setup(x => x.GetConfigurationValue(It.IsAny<string>())).Returns<string>(s => s);

            IConfigurationEnvironment env = new DefaultConfigurationEnvironment("x", new DefaultTokenisingAlgorithm(), new DefaultTokenVisitorFactory(), m.Object);
            var actual = env.TransformInput(input);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void TestEnvironments(
            [Values('"', '#')]char quote,
            [Values(',', '|')]char delimiter)
        {
            var configSystem = GetDefaultConfigSystem(quote, delimiter);
            var environments = configSystem.EnumerateEnvironments().ToArray();
            Assert.That(environments, Has.Length.EqualTo(3));
            Assert.That(environments[0], Is.EqualTo("dev"));
            Assert.That(environments[1], Is.EqualTo("uat"));
            Assert.That(environments[2], Is.EqualTo("prod"));
        }

        [Test]
        public void TestEnvironmentNames(
            [Values("dev", "uat", "prod")] string environmentName,            
            [Values('"', '#')]char quote,
            [Values(',', '|')]char delimiter)
        {
            var configSystem = GetDefaultConfigSystem(quote, delimiter);
            var ce = configSystem.GetEnvironment(environmentName);

            Assert.That(ce.EnvironmentName, Is.EqualTo(environmentName));
        }

        [Test]
        public void EnvironmentsAreCaseSensitive(
            [Values("DEV", "UAT", "PROD")]string environmentName,
            [Values('"', '#')]char quote,
            [Values(',', '|')]char delimiter)
        {
            var configSystem = GetDefaultConfigSystem(quote, delimiter);
            var ce = configSystem.GetEnvironment(environmentName);
            Assert.That(ce, Is.Null);

            environmentName = environmentName.ToLower();
            ce = configSystem.GetEnvironment(environmentName);
            Assert.That(ce.EnvironmentName, Is.EqualTo(environmentName));
        }

        [Test]
        public void TestSubstitutionsHappyPath(
            [Values("${key1}", "${key2}", "${qualified.key3}")]
            string input,

            [Values("dev", "uat", "prod")]            
            string environmentName,

            [Values('"', '#')]char quote,
            [Values(',', '|')]char delimiter)
        {
            var configSystem = GetDefaultConfigSystem(quote, delimiter);
            var ce = configSystem.GetEnvironment(environmentName);

            var transformedInput = ce.TransformInput(input);
            Assert.That(transformedInput, Is.EqualTo(environmentName + "-" + input.Trim('$').Trim('{').Trim('}')));
        }

        [Test]
        public void TestSubstitutionsNoMatch(
            [Values("${-key1}", "${-key2}", "${-key3}")]
            string input,

            [Values("dev", "uat", "prod")]
            string environmentName,

            [Values('"', '#')]char quote,
            [Values(',', '|')]char delimiter)
        {
            var configSystem = GetDefaultConfigSystem(quote, delimiter);
            var ce = configSystem.GetEnvironment(environmentName);

            var transformedInput = ce.TransformInput(input);
            Assert.That(transformedInput, Is.EqualTo(input));
        }


        [Test]
        public void NonExistantEnvironmentGivesNull( 
            [Values('"', '#')]char quote,
            [Values(',', '|')]char delimiter)
        {
            var configSystem = GetDefaultConfigSystem(quote, delimiter);
            var ce = configSystem.GetEnvironment("jdghjgh");
            Assert.That(ce, Is.Null);
        }

        [Test]
        public void Integration([Values("dev", "uat", "prod")] string environmentName,
            [Values('"', '#')]char quote,
            [Values(',', '|')]char delimiter)
        {
            var configSystem = GetDefaultConfigSystem(quote, delimiter);
            var ce = configSystem.GetEnvironment(environmentName);
            var input = GetDefaultText();

            var tmp = Guid.NewGuid().ToString();
            var expected = input
                .Replace("$${", tmp)
                .Replace("${key1}", environmentName + "-key1")
                .Replace("${key2}", environmentName + "-key2")
                .Replace("${qualified.key3}", environmentName + "-qualified.key3")
                .Replace(tmp, "${");

            var actual = ce.TransformInput(input);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
