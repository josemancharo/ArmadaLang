using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ArmadaLang.LanguageDefinition;
using static ArmadaLang.LanguageDefinition.Tokens;
namespace ArmadaLang.Tests
{
    public partial class Tests
    {
        private const string helloWorldProgram = @"
namespace ArmadaApp1;

let hello = ""Hello World"";
writeln @hello;
";

        [Test]
        public void NamespaceTokenReturnsCorrectType()
        {
            var lines = helloWorldProgram.Split('\n');
            var namespaceDeclaration = lines[1].Split(' ')[0];
            var token = TokenMatcher.MatchToken(namespaceDeclaration);
            Assert.That(token == Token.NamespaceDeclaration);
        }
    }
}
