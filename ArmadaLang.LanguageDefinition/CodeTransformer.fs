namespace ArmadaLang.LanguageDefinition

open System
open System.Linq
open FSharp.Linq
open Tokens

type IdentifiedStatement = { StatementType : Statement; Value: string }
type IdentifiedExpression = { ExpressionType : Expression; Value: string }

module CodeTransformer =
    let private RemoveNewlinesAndTabs (code : string) : string = 
        code.Trim()
        |> String.map (fun x -> if x = '\n' || x = '\t' || x = '\r' then ' ' else x)

    let SplitCodeIntoStatements (code : string) : string[] = 
        code.Split(StatementEnd)
            |> Array.map (fun x -> x.Trim())

    let ParseStatements (statements: string[]) =
       [| 
       for statement in statements -> 
            { 
                StatementType = TokenMatcher.MatchStatment statement 
                Value = statement 
            } 
       |]
       
    let IdentifyTopLevelElements (code : string)  = 
        code 
        |> RemoveNewlinesAndTabs
        |> SplitCodeIntoStatements
        |> ParseStatements

    let SplitFunctionBodyIntoStatements (block : string) =
        block.Split ExpressionEnd

    let IdentifyFunctionBodyElements (code: string) =
        code 
        |> RemoveNewlinesAndTabs
        |> SplitFunctionBodyIntoStatements
        |> ParseStatements
       