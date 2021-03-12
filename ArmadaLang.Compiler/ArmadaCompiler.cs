using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ArmadaLang.Compiler
{
    public class ArmadaCompiler 
    {
        public static CodeCompileUnit CompileCodeDomFromFile(string fileName)
        {
            ArmadaParser parser = new();
            using var file = File.OpenRead(fileName);
            using StreamReader fileReader = new(file);
            var codeDom = parser.Parse(fileReader);
            return codeDom;
        }

        public static string GenerateCSharpCodeFromCodeDom(CodeCompileUnit codeDom)
        {
            using var csharpWriter = new StringWriter();
            CodeGeneratorOptions options = new()
            {
                ElseOnClosing = true,
                BracingStyle = "C",
            };
            CSharpCodeProvider csp = new();

            csp.GenerateCodeFromCompileUnit(codeDom, csharpWriter, options);
            var csharpCode = csharpWriter.GetStringBuilder().ToString();
            return csharpCode;
        }

        public static void CompileExeFromArmadaFile(string fileName)
        {
            var codeDom = CompileCodeDomFromFile(fileName);
            var csharpCode = GenerateCSharpCodeFromCodeDom(codeDom);

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Console.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll"))
            };

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);
            CSharpCompilation csc = CSharpCompilation.Create(
                codeDom.Namespaces[0].Name,
                new[] { syntaxTree },
                references,
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
             );

            EmitResult result = csc.Emit($@"C:\Users\josem\Downloads\{codeDom.Namespaces[0].Name}\{codeDom.Namespaces[0].Name}.exe");

            Console.WriteLine($"Finished. Success: {result.Success};");
        }
    }
}
