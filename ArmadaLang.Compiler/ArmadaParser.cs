using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using ArmadaLang.LanguageDefinition;
using static ArmadaLang.LanguageDefinition.Tokens;

namespace ArmadaLang.Compiler
{
    public class ArmadaParser : ICodeParser
    {
        public string FileName { get; set; } = "stdin";
        public string Namespace { get; set; } = "MainNamespace";

        public CodeCompileUnit Parse(TextReader codeStream)
        {
            var unit = new CodeCompileUnit();

            var mainNamespace = new CodeNamespace(Namespace);
            unit.Namespaces.Add(mainNamespace); //TODO: Add namespacing
            var program = new CodeTypeDeclaration("Program");
            var start = new CodeEntryPointMethod();
            program.Members.Add(start);
            mainNamespace.Types.Add(program);

            string line;
            do
            {
                line = codeStream.ReadLine();
                var tokens = FormatCodeLineForParsing(line);
            } 
            while (line != null);
            return null;
        }
        private IEnumerable<string> FormatCodeLineForParsing(string line)
        {
            List<string> tokens = new ();
            bool ignoringSpaces = false;
            string token = "";
            foreach (char c in line)
            {
                if (IgnoreSpacesBetween.Contains(c))
                {
                    ignoringSpaces = !ignoringSpaces;
                    token += c;
                }
                else if (ignoringSpaces)
                {
                    token += c;
                }
                else if (c == ' ')
                {
                    tokens.Add(token);
                    token = "";
                }
            }
            return tokens;
        }
    }
}
