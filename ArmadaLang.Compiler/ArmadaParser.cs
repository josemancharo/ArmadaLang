using System;
using System.Linq;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using ArmadaLang.LanguageDefinition;
using static ArmadaLang.LanguageDefinition.Tokens;
using static ArmadaLang.LanguageDefinition.CodeTransformer;
using System.Data;

namespace ArmadaLang.Compiler
{
    public class ArmadaParser : ICodeParser
    {
        private CodeCompileUnit CompileUnit = new();
        private CodeNamespace CurrentNamespace = new();
        private CodeTypeDeclaration CurrentClass = new();
        private CodeMemberMethod CurrentMethod = new();

        public CodeCompileUnit Parse(TextReader codeStream)
        {
            var codeText = codeStream.ReadToEnd();
            var parsedTopLevel = IdentifyTopLevelElements(codeText);
            foreach (var node in parsedTopLevel)
            {
               ParseStatement(node);
            }
            return CompileUnit;
        }

        private IdentifiedExpression IdentifyExpression(string rawExpression)
        {
            var expression = new IdentifiedExpression(expressionType: TokenMatcher.MatchExpression(rawExpression), value: rawExpression);
            return expression;
        }


        #region GeneralParsers
        private void ParseStatement(IdentifiedStatement statement)
        {
            var s = statement.Value.Trim();
            switch (statement.StatementType)
            {
                case Statement.NamespaceDeclaration:
                    ParseNamespaceDeclaration(s);
                    break;
                case Statement.ClassDeclaration:
                    ParseClassDeclaration(s);
                    break;
                case Statement.FieldDeclaration:
                    break;
                case Statement.MethodDeclaration:
                    ParseMethodDeclaration(s);
                    break;
                case Statement.EntrypointMethodDeclaration:
                    ParseEntrypointMethodDeclaration(s);
                    break;
                case Statement.NamespaceImport:
                    ParseNamespaceImport(s);
                    break;
                case Statement.VariableDeclaration:
                    ParseVariableDeclaration(s);
                    break;
                case Statement.WriteLn:
                    ParseWriteLn(s);
                    break;
                case Statement.MethodInvocation:
                    ParseMethodInvocation(s);
                    break;
                case Statement.CompilerError:
                    throw new Exception("Compiler Error at ParseStatement");

            }
        }
        private CodeExpressionCollection ParseExpression(string rawExpression)
        {
            var expression = IdentifyExpression(rawExpression);
            CodeExpressionCollection codeExpressions = new();
            switch (expression.ExpressionType)
            {
                case Expression.StringLiteral:
                    codeExpressions.Add(ParsePrimitive(expression));
                    break;
                case Expression.NumericLiteral:
                    codeExpressions.Add(ParsePrimitive(expression));
                    break;
                case Expression.VariableReference:
                    codeExpressions.Add(ParseVariableReference(expression.Value));
                    break;
                case Expression.BooleanLiteral:
                    codeExpressions.Add(ParsePrimitive(expression));
                    break;
                case Expression.ObjectCreation:
                    codeExpressions.Add(ParseObjectCreate(expression.Value));
                    break;
                case Expression.SelfReference:
                    codeExpressions.Add(new CodeThisReferenceExpression());
                    break;
                case Expression.FieldReference:
                    codeExpressions.Add(ParseFieldReference(expression.Value));
                    break;
                case Expression.CompilerError:
                    throw new Exception("Compiler Error at ParseExpression");
            }
            return codeExpressions;
        }
        #endregion


        #region SubParsers
        private CodeFieldReferenceExpression ParseFieldReference(string expression)
        {
            var parts = expression.Split('.').ToList();
            var fieldName = parts.Last();
            parts.RemoveAt(parts.Count - 1);
            var reference = new CodeFieldReferenceExpression
            {
                FieldName = fieldName,
                TargetObject = ParseExpression(string.Join('.', parts))[0]
            };
            return reference;
        }

        private CodeObjectCreateExpression ParseObjectCreate(string rawExpression)
        {
            var expression = rawExpression
                .TrimStart()
                .Remove(0, "new".Length);
            var parts = expression.Split(' ').ToList();
            var constructor = new CodeObjectCreateExpression
            {
                CreateType = new CodeTypeReference(parts[0]),
            };
            parts.RemoveAt(0);
            ParseParameters(string.Join(' ', parts), ref constructor);
            return constructor;
        }

        private void ParseNamespaceImport(string expression)
        {
            var parts = expression.Split(' ');
            var namespaceToImport = new CodeNamespaceImport
            {
                Namespace = parts[1]
            };
            CurrentNamespace.Imports.Add(namespaceToImport);
        }

        private void ParseMethodInvocation(string expression)
        {
            var parts = expression.Split(' ');
            var nameString = parts[0];
            var valueString = parts[2];
            var nameParts = nameString.Split('.').ToList();
            var methodName = nameParts.Last();
            nameParts.RemoveAt(nameParts.Count - 1);
            var completeTargetObject = string.Join(".", nameParts);
            var function = new CodeMethodInvokeExpression
            {
                Method = new CodeMethodReferenceExpression
                {
                    MethodName = methodName,
                    TargetObject = ParseFieldReference(completeTargetObject)
                }
            };
            ParseParameters(valueString, ref function);
            CurrentMethod.Statements.Add(function);

        }

        private void ParseParameters(string valueString, ref CodeMethodInvokeExpression codeMethod)
        {
            var parameters = valueString
                .Split(ParameterSeparator, StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => ParseExpression(x)[0]);

            foreach (var param in parameters)
            {
                codeMethod.Parameters.Add(param);
            }
        }
        private void ParseParameters(string valueString, ref CodeObjectCreateExpression codeMethod)
        {
            var parameters = valueString
                .Split(ParameterSeparator, StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => ParseExpression(x)[0]);

            foreach (var param in parameters)
            {
                codeMethod.Parameters.Add(param);
            }
        }

        private void ParseNamespaceDeclaration(string statement)
        {
            var namespaceName = statement
                .Split(' ', StringSplitOptions.TrimEntries)
                [1];
            var newNamespace = new CodeNamespace(namespaceName);
            CompileUnit.Namespaces.Add(newNamespace);
            CurrentNamespace = newNamespace;
        }

        private void ParseClassDeclaration(string statement)
        {
            var pieces = statement
                .Split(' ', StringSplitOptions.TrimEntries);
            var newClass = new CodeTypeDeclaration { IsClass = true, Name = pieces[1] };
            CurrentNamespace.Types.Add(newClass);
            CurrentClass = newClass;
        }

        private void ParseWriteLn(string statement)
        {
            var length = "writeln".Length;
            var value = statement.Remove(0, length);
            var logFunction = new CodeMethodInvokeExpression
            {
                Method = new CodeMethodReferenceExpression
                {
                    MethodName = "WriteLine",
                    TargetObject = new CodeTypeReferenceExpression("System.Console")
                },
            };
            ParseParameters(value, ref logFunction);
            CurrentMethod.Statements.Add(logFunction);
        }

        private static CodePrimitiveExpression ParsePrimitive(IdentifiedExpression expression)
        {
            var insideString = expression.Value.Trim();

            if (expression.ExpressionType == Expression.StringLiteral)
            {
                insideString = insideString.Remove(0, 1).Remove(insideString.Length - 2, 1);
            }
            return new CodePrimitiveExpression(insideString);
        }

        private CodeVariableReferenceExpression ParseVariableReference(string expression)
        {
            var name = expression.Replace("@", null);
            var reference = new CodeVariableReferenceExpression
            {
                VariableName = name,
            };
            return reference;
        }

        private string GetTypeOfPrimitive(string rawPrimitive)
        {
            var primitive = IdentifyExpression(rawPrimitive);
            if (primitive.ExpressionType == Expression.StringLiteral)
            {
                return "System.String";
            }
            else if (primitive.ExpressionType == Expression.NumericLiteral)
            {
                if (primitive.Value.Contains('.'))
                {
                    return "System.Double";
                }
                else
                {
                    return "System.Int32";
                }
            }
            else if (primitive.ExpressionType == Expression.BooleanLiteral)
            {
                return "System.Boolean";
            }
            else throw new Exception("Primitive type not valid");
        }

        private void ParseVariableDeclaration(string statement)
        {
            var trimmedStatement = statement
                .Remove(0, "let".Length)
                .Split('=', StringSplitOptions.TrimEntries);
            var variableName = trimmedStatement[0];
            var expressionValue = trimmedStatement[1];
            var initExpression = ParseExpression(expressionValue)[0];
            var newExpression = new CodeVariableDeclarationStatement 
            { 
                Name = variableName, 
                InitExpression = initExpression,
                Type = new CodeTypeReference(GetTypeOfPrimitive(expressionValue))
                
            };
            CurrentMethod.Statements.Add(newExpression);
        }

        private void ParseFunctionBody(string functionBody)
        {
            var identifiedExpressions = IdentifyFunctionBodyElements(functionBody);
            foreach (var expr in identifiedExpressions)
            {
                ParseStatement(expr);
            }
        }

        private void ParseEntrypointMethodDeclaration(string statement)
        {
            var methodName = statement
                .Remove(0, "#entrypoint fn".Length)
                .TrimStart()
                .Split('{')
                .First();
            CurrentMethod = new CodeEntryPointMethod { Name = methodName };
            var functionBody = statement.Split('{', '}')[1];
            ParseFunctionBody(functionBody);
            CurrentClass.Members.Add(CurrentMethod);
        }

        private void ParseMethodDeclaration(string statement)
        {
            var methodName = statement
                .Remove(0, "fn".Length)
                .TrimStart()
                .Split('{')
                .First();
            CurrentMethod = new CodeMemberMethod { Name = methodName };
            var functionBody = statement.Split('{', '}')[1];
            ParseFunctionBody(functionBody);
            CurrentClass.Members.Add(CurrentMethod);
        }
        #endregion
    }
}
