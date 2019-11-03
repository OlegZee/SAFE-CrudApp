namespace Components

module TabularForms =
    open Elmish
    open Browser.Dom

    type ModelState<'trec> =
        | Init
        | Loading
        | DataLoaded of 'trec list
    type Model<'trec,'tnewrec,'tcustom> = {
        customData: 'tcustom    // inheritor specific data
        data: 'trec ModelState
        newEntry: Map<string,string>
        newEntryValid: Result<'tnewrec, Map<string,string list>>    // 'tfield: string list
        lastError: string option
    }
    type Msg<'trec,'recordKey> =
        | RefreshData
        | SetLastError of string option
        | SaveChanges
        | SetNewField of string * string
        | ValidateNewEntry
        | SaveNewEntry
        | DeleteEntry of 'recordKey
        | ReceivedData of Result<'trec list,string>

    let private initValidationError () = Error <| Map.add "" ["input incomplete"] Map.empty
    let initNewEntry () = Map.empty
    let init (customData): Model<'tr,_,_> *  Cmd<Msg<'tr,'recordKey>> =
        { customData = customData
          data = Init
          newEntry = initNewEntry ()
          newEntryValid = initValidationError()
          lastError = None }, Cmd.ofMsg RefreshData

    let update (retrieveData,validateEntry,addNewEntry,removeEntry) (msg: Msg<'tr,'recordKey>) (model: Model<'tr,'tnew,_>) =
        
        let respondRestApiResult = function |Ok _ -> RefreshData | Error e -> SetLastError (Some e)
        match msg with
        | RefreshData ->
            { model with data = Loading; newEntry = initNewEntry(); newEntryValid = initValidationError() },
            Cmd.OfPromise.perform retrieveData model ReceivedData
        | ReceivedData (Ok data) ->
            { model with data = DataLoaded data; lastError = None }, Cmd.none

        | SetNewField (field, value) ->        
            { model with newEntry = model.newEntry |> Map.add field value }, Cmd.ofMsg ValidateNewEntry
        | ValidateNewEntry ->
            { model with newEntryValid = validateEntry model.newEntry }, Cmd.none

        | SetLastError x ->
            { model with lastError = x }, Cmd.none

        | SaveNewEntry ->
            match model.newEntryValid with
            | Ok newEntry ->
                model, Cmd.OfPromise.perform addNewEntry (model, newEntry) respondRestApiResult
            | _ ->
                console.warn ("should not get here as the data is not validated")
                model, Cmd.none

        | DeleteEntry recordId ->
            model, Cmd.OfPromise.perform removeEntry (model, recordId) (
                function |Ok _ -> RefreshData | Error e -> SetLastError (Some <| sprintf "Failed to remove user (%s)" e)
            )

        | _ ->
            model, Cmd.none
