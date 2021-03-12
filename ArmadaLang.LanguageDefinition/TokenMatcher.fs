namespace ArmadaLang.LanguageDefinition

module TokenMatcher =
    open System.Text.RegularExpressions
    open Tokens

    let MatchStatment (statementRaw : string)=
        let statement = statementRaw.Trim().Replace('\n', ' ')

        let startsWith (value : string) = 
            statement
                .StartsWith(value)

        let matches (regex: string) =
            Regex.IsMatch(statement, regex)

        match statement with 
        | _ when startsWith "namespace" -> Statement.NamespaceDeclaration
        | _ when startsWith "class" -> Statement.ClassDeclaration
        | _ when startsWith "apply" -> Statement.NamespaceImport
        | _ when startsWith "writeln" -> Statement.WriteLn
        | _ when startsWith "let" -> Statement.VariableDeclaration
        | _ when startsWith "#entrypoint" -> Statement.EntrypointMethodDeclaration
        | _ when startsWith "fn" -> Statement.MethodDeclaration
        | _ when matches @"\:.*" -> Statement.MethodInvocation
        | _ -> Statement.CompilerError


    let MatchExpression (expressionRaw : string) = 
        let expression = expressionRaw.Trim().Replace('\n', ' ')
        
        let startsWith (value : string) = 
            expression.StartsWith(value)

        let matches (regex: string) =
            Regex.IsMatch(expression, regex)

        match expression with 
        | _ when matches @"(?<="")(?:\\.|[^""\\])*(?="")" -> Expression.StringLiteral
        | _ when matches @"[+-]?(\d*\.)?\d+" -> Expression.NumericLiteral
        | _ when matches @"@[0-9A-z\-_]*" -> Expression.VariableReference
        | _ when startsWith "$" -> Expression.SelfReference
        | _ when startsWith "new" -> Expression.ObjectCreation
        | _ when matches @"true|false" -> Expression.BooleanLiteral
        | _ when matches @"[A-z0-9\-_\.]*" -> Expression.FieldReference
        | e when e = "null" -> Expression.NullLiteral
        | _ -> Expression.CompilerError