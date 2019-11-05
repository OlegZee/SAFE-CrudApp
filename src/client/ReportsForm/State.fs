module ReportsForm.State

open Elmish
open Types
open ServerComm

let initQuery () =
    { dateStart = None; dateEnd = None; timeStart = None; timeEnd = None }

let init () =
    { data = []; lastError = None }, Cmd.ofMsg (QueryData (initQuery()))

let update (msg: Msg) (model: Model) =
    match msg with
    | QueryData query ->
        let q : ServerComm.QueryDataParams = { from = query.dateStart; to' = query.dateEnd; tfrom = query.timeStart; tto = query.timeEnd }
        { model with lastError = None }, Cmd.OfPromise.perform queryData q (
             function |Ok data -> UpdateData data | Error e -> DisplayError (Some e)
        )
    | UpdateData data -> { model with data = data }, Cmd.none
    | DisplayError e ->
        { model with lastError = e }, Cmd.none
