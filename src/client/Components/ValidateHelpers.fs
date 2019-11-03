namespace Components

open Fulma
open Fable.React.Helpers

module ValidateHelpers =
    // helper pattern for entry validation
    let (|NoneOrBlank|_|) =
        function |None -> Some "" | Some x when System.String.IsNullOrWhiteSpace x -> Some "" | _ -> None

    let errors result fieldName =
        match result with
        | Ok _ -> []
        | Error map ->
            let errlist = map |> Map.tryFind fieldName |> Option.defaultValue []
            errlist |> List.map (fun error -> Help.help [ Help.Color IsDanger ] [ str error ])
    