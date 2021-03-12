using System;
using ArmadaLang.Compiler;

namespace ArmadaLang
{
    class Program
    {
        static void Main(string[] args)
        {
            ArmadaCompiler.CompileExeFromArmadaFile(args[0]);
        }
    }
}
