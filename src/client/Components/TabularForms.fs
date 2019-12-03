namespace Components

module TabularForms =
    open Elmish
    open Browser.Dom

    type ValidationStatus = Map<string,string list>

    type EntryData<'record> = {
        rawFields: Map<string,string>
        validated: Result<'record, ValidationStatus>
    }

    type TableData<'t> =
        | Init
        | Loading
        | DataLoaded of 't list
    type Model<'trec,'tnewrec,'tcustom> = {
        customData: 'tcustom    // inheritor specific data
        data: TableData<'trec>
        edited: EntryData<'tnewrec> option
        newrec:  EntryData<'tnewrec>
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
    let initEntry () = { rawFields = Map.empty; validated = initValidationError () }

    let validate validateFields record =
        { record with validated = validateFields record.rawFields }

    let init (customData): Model<'tr,_,_> *  Cmd<Msg<'tr,'recordKey>> =
        { customData = customData
          data = Init
          newrec = initEntry ()
          edited = None
          lastError = None }, Cmd.ofMsg RefreshData

    let update (retrieveData,validateFields,addNewEntry,removeEntry) (msg: Msg<'tr,'recordKey>) (model: Model<'tr,'tnew,_>) =
        
        let respondRestApiResult = function |Ok _ -> RefreshData | Error e -> SetLastError (Some e)
        match msg with
        | RefreshData ->
            { model with data = Loading; newrec = initEntry () },
            Cmd.OfPromise.perform retrieveData model ReceivedData
        | ReceivedData (Ok data) ->
            { model with data = DataLoaded data; lastError = None }, Cmd.none

        | SetNewField (field, value) ->        
            { model with newrec = { model.newrec with rawFields = model.newrec.rawFields |> Map.add field value} }, Cmd.ofMsg ValidateNewEntry
        | ValidateNewEntry ->
            { model with newrec = model.newrec |> validate validateFields }, Cmd.none

        | SetLastError x ->
            { model with lastError = x }, Cmd.none

        | SaveNewEntry ->
            match model.newrec.validated with
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
