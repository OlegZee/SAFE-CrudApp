module EntryForm.State

open System
open Browser.Dom

open Elmish
open ServerComm
open ServerProtocol.V1

let initNewEntry ( ): NewEntry =
      { time = DateTime.Now.TimeOfDay.ToString()
        meal = ""
        amount = "" }

let validateEntry (d: NewEntry) : Result<PostDataPayload,string> =
    // TODO fable-validate for nicer validation rules
    match TimeSpan.TryParse d.time, d.meal, Double.TryParse d.amount with
    | (true, time), meal, (true, amount) when (not <| String.IsNullOrWhiteSpace meal) && amount > 0.0 && amount < 1000.0 ->
        Ok { rtime = time.ToString(); meal = meal; amount = amount }

    | (false,_),_,_ -> Error "invalid time value"
    | _,m,_ when String.IsNullOrWhiteSpace m -> Error "meal cannot be empty"
    | _,_,(false,_) -> Error "invalid amount value, must be a number"
    | _,_,(true,v) when not (v > 0.0 && v < 1000.0) -> Error "invalid amount value, must be between 0 and 1000"
    | _ -> Error "some data is not valid"

let init (apiUrl, token): Model *  Cmd<Msg> =
    { apiUrl = apiUrl; token = token
      data = Init
      newEntry = initNewEntry()
      newEntryValid = Error "input incomplete" }, Cmd.ofMsg RefreshData

let update (msg: Msg) (model: Model) =
    match msg with
    | RefreshData ->
        { model with data = Loading; newEntry = initNewEntry(); newEntryValid = Error "input incomplete" },
        Cmd.OfPromise.perform retrieveDailyData (model.token, model.apiUrl) ReceivedData
    | ReceivedData (Ok data) ->
        { model with data = Data data }, Cmd.none

    | SetNewTime t -> { model with newEntry = { model.newEntry with time = t}}, Cmd.ofMsg ValidateNewEntry
    | SetNewMeal m -> { model with newEntry = { model.newEntry with meal = m}}, Cmd.ofMsg ValidateNewEntry
    | SetNewAmount a -> { model with newEntry = { model.newEntry with amount = a}}, Cmd.ofMsg ValidateNewEntry
    | ValidateNewEntry -> { model with newEntryValid = validateEntry model.newEntry }, Cmd.none

    | SaveNewEntry ->
        match model.newEntryValid with
        | Ok newEntry ->
            model, Cmd.OfPromise.perform addNewEntry (model.token, model.apiUrl, newEntry) (fun _ -> RefreshData)   // ignoring create result
        | _ ->
            console.warn ("should not get here as the data is not validated")
            model, Cmd.none

    | DeleteEntry recordId ->
        console.log("delete", recordId)
        model, Cmd.OfPromise.perform removeEntry (model.token, model.apiUrl, recordId) (fun _ -> RefreshData)

    | _ ->
        model, Cmd.none