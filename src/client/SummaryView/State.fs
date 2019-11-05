module SummaryView.State

open Elmish
open Types

open ServerProtocol.V1
open ServerComm

let init user data : Model =
    { user = user; data = data; editedTarget = None; otherUser = None }
    
let update (msg: Msg) (model: Model) =
    match msg with
    | EditTarget -> { model with editedTarget = model.user.target |> Some } , Cmd.none
    | CancelEdit -> { model with editedTarget = None }, Cmd.none
    | SavedTargetValue _ -> model, Cmd.ofMsg CancelEdit
    | SaveValue newValue ->
        match System.Double.TryParse newValue with
        | true, value when value >= 0. && value < 10000. ->
             { model with editedTarget = None }, Cmd.OfPromise.perform saveSettings { targetCalories = value }
                (function |Ok () -> SavedTargetValue value |Error e -> EditTarget)  // FIXME improve parent notification, validation/error
        | _ ->
            model, Cmd.none