using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Net.Http.Headers;

namespace ArmadaLang.Compiler
{
    public class ArmadaCodeDomAdapter
    {
        public static CodeAssignStatement AssignPrimitiveValue<T>(string variable, T value)
        {
            return new CodeAssignStatement(new CodeVariableReferenceExpression(variable), new CodePrimitiveExpression(value));
        }

        public static CodeVariableDeclarationStatement DeclarePrimitiveVariable<T>(string name, T value)
        {
            return new CodeVariableDeclarationStatement(
                typeof(T),
                name,
                new CodePrimitiveExpression(value)
                );
        }
        public static CodeMethodInvokeExpression ConsoleWrite(CodeExpression value)
        {
            return new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression("System.Console"),
                "WriteLine",
                value
                );
        }
    }
}
