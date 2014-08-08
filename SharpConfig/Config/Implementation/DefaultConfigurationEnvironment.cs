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
using System.Linq;
using System.Text;

namespace SharpConfig.Config.Implementation
{
    public class DefaultConfigurationEnvironment : IConfigurationEnvironment
    {
        private readonly string m_EnvironmentName;
        private readonly ITokenisingAlgorithm m_Tokeniser;
        private readonly ITokenVisitorFactory m_VisitorFactory;
        private readonly IConfigurationMapper m_Mapper;

        public DefaultConfigurationEnvironment(string environmentName, ITokenisingAlgorithm tokeniser, ITokenVisitorFactory visitorFactory, IConfigurationMapper mapper)
        {
            m_EnvironmentName = environmentName;
            m_Tokeniser = tokeniser;
            m_VisitorFactory = visitorFactory;
            m_Mapper = mapper;
        }

        public string EnvironmentName
        {
            get { return m_EnvironmentName; }
        }

        public string TransformInput(string source)
        {
            var tokens = m_Tokeniser.Tokenise(source).ToArray();

            var result = new StringBuilder();
            foreach (var token in tokens)
            {
                var visitor = m_VisitorFactory.CreatEvaluatingTokenVisitor(m_Mapper);
                token.Accept(visitor);
                var evaluation = visitor.GetEvaluatedText();
                result.Append(evaluation);
            }

            return result.ToString();
        }
    }
}