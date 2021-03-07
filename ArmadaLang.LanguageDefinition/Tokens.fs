namespace ArmadaLang.LanguageDefinition

open System.Collections.Generic

module Tokens = 
    type Token =
    | CompilerError = -1
    | AssignmentOperator = 0
    | StringLiteral = 1
    | WriteFunction = 2
    | IntegerLiteral = 3
    | FloatLiteral = 4
    | Reference = 5
    | VariableDefinition = 6
    | NamespaceImport = 7
    | EndOfStatement = 8
    | NamespaceDeclaration = 9

    let IgnoreSpacesBetween = [|
        '"'
    |]


