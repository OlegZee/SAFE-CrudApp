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

let init apiUrl: Model * Cmd<Msg> =
    TabularForms.init (ApiUri apiUrl)

let dataToPayload (data: DataRecord) : PostDataPayload =
    {   rtime = data.rtime
        meal = data.meal
        amount = data.amount }

let toData (payload: UserData) : (CommonTypes.EntryId * DataRecord) =
    CommonTypes.EntryId payload.record_id,
    {   rtime = payload.rtime
        meal = payload.meal
        amount = payload.amount }

let private getFields _ = Map.empty // TODO
let private retrieveData (ApiUri apiUrl) = ServerComm.retrieveDailyData apiUrl |> TabularForms.mapPromiseResult (List.map toData)
let private addNewEntry (ApiUri apiUrl, data: DataRecord) = ServerComm.addNewEntry (apiUrl, dataToPayload data)
let private removeEntry (ApiUri apiUrl, recId) = ServerComm.removeEntry (apiUrl, recId)
let private updateEntry (ApiUri apiUrl, recId, data: DataRecord) = ServerComm.updateEntry (apiUrl, recId, dataToPayload data)

let update: Msg -> Model -> Model * Cmd<Msg> =
    TabularForms.update (retrieveData, getFields, validateEntry, addNewEntry, updateEntry, removeEntry)
