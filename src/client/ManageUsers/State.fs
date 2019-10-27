module ManageUsers.State

open Browser.Dom

open Elmish

module TabularForms =
    open ManageUsers.Types.TabularFormTypes

    let initNewEntry () = Map.empty
    let init (apiUrl, token): Model<'tr,_> *  Cmd<Msg<'tr>> =
        { apiUrl = apiUrl; token = token
          data = Init
          newEntry = initNewEntry ()
          newEntryValid = Error "input incomplete"
          lastError = None }, Cmd.ofMsg RefreshData

    let update (retrieveData,validateEntry,addNewEntry,removeEntry) (msg: Msg<'tr>) (model: Model<'tr,'tnew>) =
        
        let respondRestApiResult = function |Ok _ -> RefreshData | Error e -> SetLastError (Some e)
        match msg with
        | RefreshData ->
            { model with data = Loading; newEntry = initNewEntry(); newEntryValid = Error "input incomplete" },
            Cmd.OfPromise.perform retrieveData model ReceivedData
        | ReceivedData (Ok data) ->
            { model with data = Data data; lastError = None }, Cmd.none
    
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

open ServerProtocol.V1
open ManageUsers.Types

let init (apiUrl, token) : Model * Cmd<Msg> =
    TabularForms.init (apiUrl, token)

let private validateEntry map =
    Error "validate not implemented"

let private retrieveUsers (model: Model) = ServerComm.retrieveUsers (model.token)
let private addNewUser (model: Model, data: CreateUserInfo) = ServerComm.addNewUser (model.token, data)
let private removeUser (model: Model, rec_id: int) = ServerComm.removeUser (model.token, rec_id)

let update: Msg -> Model -> Model * Cmd<Msg> =
    TabularForms.update (retrieveUsers, validateEntry, addNewUser, removeUser)
