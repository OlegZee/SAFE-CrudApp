module EntryForm.State

open Elmish
open ServerComm

let init (apiUrl, date, token): Model *  Cmd<Msg> =
    { apiUrl = apiUrl; token = token; date = date
      data = Init; lastUpdate = Ok() }, Cmd.ofMsg RefreshData
let update (msg: Msg) (model: Model) =
    match msg with
    | RefreshData ->
        { model with data = Loading },
            Cmd.OfPromise.perform retrieveDailyData (model.token, model.apiUrl) ReceivedData
    | ReceivedData (Ok data) ->
        let records = data |> List.map Unchanged
        { model with data = Data records }, Cmd.none

    | _ ->
        model, Cmd.none