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

    type Model<'rowkey,'trec,'tcustom> = {
        customData: 'tcustom    // inheritor specific data
        data: TableData<'rowkey * 'trec>
        edited: ('rowkey * EntryData<'trec>) option
        newrec:  EntryData<'trec>
        lastError: string option
    }

    type Msg<'rkey,'trec> =
        | RefreshData
        | SetLastError of string option
        | SaveChanges
        | SetNewField of string * string
        | SaveNewEntry
        | DeleteEntry of 'rkey
        | ReceivedData of Result<('rkey * 'trec) list,string>

        | StartEdit of 'rkey
        | SetEditField of string * string
        | SaveEditEntry


    let private initValidationError () = Error <| Map.add "" ["input incomplete"] Map.empty
    let initEntry () = { rawFields = Map.empty; validated = initValidationError () }

    let mapPromiseResult f p = promise {
        let! res = p
        return res |> Result.map f
    }
    
    let validate validateFields record =
        { record with validated = validateFields record.rawFields }

    let init (customData): Model<'rkey,'tr,_> *  Cmd<Msg<'rkey,'tr>> =
        { customData = customData
          data = Init
          newrec = initEntry ()
          edited = None
          lastError = None }, Cmd.ofMsg RefreshData

    let update (retrieveData,getFields,validateFields,addNewEntry,updateEntry,removeEntry) (msg: Msg<'rkey,'tr>) (model: Model<'rkey,'tr,_>) =
        
        let respondRestApiResult = function |Ok _ -> RefreshData | Error e -> SetLastError (Some e)
        match msg with
        | RefreshData ->
            { model with data = Loading; newrec = initEntry (); edited = None },
            Cmd.OfPromise.perform retrieveData model.customData ReceivedData
        | ReceivedData (Ok data) ->
            { model with data = DataLoaded data; lastError = None }, Cmd.none

        | SetNewField (field, value) ->        
            let fields = model.newrec.rawFields |> Map.add field value
            { model with newrec = { model.newrec with rawFields = fields; validated = validateFields fields} }, Cmd.none

        | SetLastError x ->
            { model with lastError = x }, Cmd.none

        | SaveNewEntry ->
            match model.newrec.validated with
            | Ok newEntry ->
                model, Cmd.OfPromise.perform addNewEntry (model.customData, newEntry) respondRestApiResult
            | _ ->
                console.warn ("should not get here as the data is not validated")
                model, Cmd.none

        | DeleteEntry recordId ->
            model, Cmd.OfPromise.perform removeEntry (model.customData, recordId) (
                function |Ok _ -> RefreshData | Error e -> SetLastError (Some <| sprintf "Failed to remove user (%s)" e)
            )

        | StartEdit recordId ->
            let editEntry =
                function
                | DataLoaded records -> records |> List.tryFind (fst >> ((=) recordId))
                | _ -> None
                >> Option.map (fun (key,record) -> key, { rawFields = getFields record; validated = Ok record })

            { model with edited = editEntry model.data }, Cmd.none

        | SetEditField (field, value) ->
            match model with
            | { edited = Some (rkey, record)} ->
                let updFields = record.rawFields |> Map.add field value
                let updated = { record with rawFields = updFields; validated = validateFields updFields }
                { model with edited = Some (rkey, updated)}, Cmd.none
            | _ ->
                console.warn("model is not being edited")
                model, Cmd.none

        | SaveEditEntry ->
            match model with
            | { edited = Some (rkey, {validated = Ok updEntry})} ->
                model, Cmd.OfPromise.perform updateEntry (model.customData, rkey, updEntry) (
                    function |Ok _ -> RefreshData | Error e -> SetLastError (Some <| sprintf "Failed to remove user (%s)" e)
                )
            | _ ->
                console.warn("record is not being edited or not valid")
                model, Cmd.none

        | _ ->
            model, Cmd.none
