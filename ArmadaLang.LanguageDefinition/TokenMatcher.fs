namespace ArmadaLang.LanguageDefinition

module TokenMatcher =
    open System.Text.RegularExpressions
    open Tokens

    let MatchToken (token : string) : Token = 
        match token with
        | "namespace" -> Token.NamespaceDeclaration
        | "summon" -> Token.NamespaceImport
        | ";" -> Token.EndOfStatement
        | "let" -> Token.VariableDefinition
        | "=" -> Token.AssignmentOperator
        | "writeln" -> Token.WriteFunction
        | token when Regex("[0-9]").IsMatch(token) -> Token.IntegerLiteral
        | token when Regex("[0-9]\..{0,10}").IsMatch(token) -> Token.FloatLiteral
        | token when Regex("([\"])(?:(?=(\\?))\2.)*?\1").IsMatch(token) -> Token.StringLiteral
        | token when Regex("@[A-z]{1}[A-z0-9_-]").IsMatch(token) -> Token.Reference
        | _ -> Token.CompilerError