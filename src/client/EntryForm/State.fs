module EntryForm.State

open System
open Components
open Components.ValidateHelpers

open Elmish
open ServerProtocol.V1

open EntryForm.Types

let validateEntry (map: Map<string,string>) : Result<PostDataPayload,string> =
    match Map.tryFind "time" map, Map.tryFind "meal" map, Map.tryFind "amount" map with
    | NoneOrBlank _, _, _ -> Error "time is not specified"
    | _, NoneOrBlank _, _ -> Error "meal is not specified"
    | _, _, NoneOrBlank _ -> Error "amount is not specified"
    | Some time, Some meal, Some amount ->
        // TODO fable-validate for nicer validation rules
        match TimeSpan.TryParse time, meal, Double.TryParse amount with
        | (true, time), meal, (true, amount) when amount > 0.0 && amount <= 9999.0 ->
            Ok { rtime = time.ToString(); meal = meal; amount = amount }

        | (false,_),_,_ -> Error "invalid time value"
        | _,_,(false,_) -> Error "invalid amount value, must be a number"
        | _,_,(true,v) when not (v > 0.0 && v <= 9999.0) -> Error "invalid amount value, must be between 0 and 1000"
        | _ -> Error "some data is not valid"
    | _ -> Error "Something went wrong (in fact impossible case)"

let init (apiUrl, token): Model * Cmd<Msg> =
    TabularForms.init { token = token; api = apiUrl }

let private retrieveData (model: Model) = ServerComm.retrieveDailyData (model.customData.token, model.customData.api)
let private addNewEntry (model: Model, data) = ServerComm.addNewEntry (model.customData.token, model.customData.api, data)
let private removeEntry (model: Model, recId) = ServerComm.removeEntry (model.customData.token, model.customData.api, recId)

let update: Msg -> Model -> Model * Cmd<Msg> =
    TabularForms.update (retrieveData, validateEntry, addNewEntry, removeEntry)
