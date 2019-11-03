module EntryForm.State

open System

open Fable.Validation.Core
open Elmish

open Components
open ServerProtocol.V1
open EntryForm.Types

let validateEntry (map: Map<string,string>) =
    all <| fun t ->
        let fromMap name = (map |> Map.tryFind name |> Option.defaultValue "") |> t.Test name in
        { rtime = fromMap "time"
            |> t.To TimeSpan.Parse "time should be a valid time value"
            |> t.To string ""
            |> t.End
          meal = fromMap "meal"
            |> t.Trim
            |> t.NotBlank "name cannot be blank"
            |> t.MaxLen 60 "maxlen is {len}"
            |> t.MinLen 3 "minlen is {len}"
            |> t.End
          amount = fromMap "amount"
            |> t.To double "should be a number"
            |> t.Gt 0. "should greater then {min}"
            |> t.Lt 10000. "shoudld less then {max}"
            |> t.End }

let init (apiUrl, token): Model * Cmd<Msg> =
    TabularForms.init { token = token; api = apiUrl }

let private retrieveData (model: Model) = ServerComm.retrieveDailyData (model.customData.token, model.customData.api)
let private addNewEntry (model: Model, data) = ServerComm.addNewEntry (model.customData.token, model.customData.api, data)
let private removeEntry (model: Model, recId) = ServerComm.removeEntry (model.customData.token, model.customData.api, recId)

let update: Msg -> Model -> Model * Cmd<Msg> =
    TabularForms.update (retrieveData, validateEntry, addNewEntry, removeEntry)
