namespace ArmadaLang.LanguageDefinition

module Tokens = 
    type Statement =
    | CompilerError = -1
    | NamespaceDeclaration = 0
    | NamespaceImport = 1
    | ClassDeclaration = 2
    | MethodDeclaration = 3
    | FieldDeclaration = 4
    | VariableDeclaration = 5
    | WriteLn = 6
    | MethodInvocation = 7
    | CommentLine = 8
    | EntrypointMethodDeclaration = 9

    type Expression =
    | CompilerError = -1
    | NumericLiteral = 0
    | FloatLiteral = 1
    | StringLiteral = 2
    | BooleanLiteral = 3
    | FieldReference = 4
    | SelfReference = 5
    | VariableReference = 6
    | ObjectCreation = 7
    | NullLiteral = 8

    let StringDelimeters = [|
        '"'
    |]

    let FunctionOpen = '{'

    let FunctionClose = '}'

    let StatementEnd = ';'

    let ExpressionEnd = ','

    let ParameterSeparator = '|'




