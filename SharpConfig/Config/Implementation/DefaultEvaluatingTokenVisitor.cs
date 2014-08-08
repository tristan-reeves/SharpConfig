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

namespace SharpConfig.Config.Implementation
{
    public class DefaultEvaluatingTokenVisitor : IEvaluatingTokenVisitor
    {
        private readonly IConfigurationMapper m_Mapper;
        private string m_EvaluatedValue = "";

        public DefaultEvaluatingTokenVisitor(IConfigurationMapper mapper)
        {
            m_Mapper = mapper;
        }

        public string GetEvaluatedText()
        {
            return m_EvaluatedValue;
        }

        public void Visit(IToken token)
        {
            var type = token.GetType();
            if (typeof(LiteralToken).IsAssignableFrom(type)) this.Visit((LiteralToken)token);
            else if (typeof(ConfigurationKeyToken).IsAssignableFrom(type)) this.Visit((ConfigurationKeyToken)token);
            else throw new ArgumentException("Unknown token [" + token + "]");
        }

        public void Visit(LiteralToken token)
        {
            m_EvaluatedValue = token.UnescapedSource;
        }

        public void Visit(ConfigurationKeyToken token)
        {
            var key = token.ConfigurationKey;
            m_EvaluatedValue = m_Mapper.GetConfigurationValue(key) ?? token.Source;
        }
    }
}