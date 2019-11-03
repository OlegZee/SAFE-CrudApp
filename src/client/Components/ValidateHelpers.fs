namespace Components

module ValidateHelpers =
    // helper pattern for entry validation
    let (|NoneOrBlank|_|) =
        function |None -> Some "" | Some x when System.String.IsNullOrWhiteSpace x -> Some "" | _ -> None
