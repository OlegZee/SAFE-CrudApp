module ManageUsers.State

open Elmish

open Components
open ServerProtocol.V1

open ManageUsers.Types

let init token : Model * Cmd<Msg> =
    TabularForms.init token

let (|NoneOrBlank|_|) =
    function |None -> Some "" | Some x when System.String.IsNullOrWhiteSpace x -> Some "" | _ -> None
    
let private validateEntry (map: Map<string,string>) =
    match Map.tryFind "login" map, Map.tryFind "name" map, Map.tryFind "pwd" map with
    | NoneOrBlank _, _, _ -> Error "login is not specified"
    | _, NoneOrBlank _, _ -> Error "name is not specified"
    | _, _, NoneOrBlank _ -> Error "password is not specified"
    | Some login, Some name, Some pwd ->
        let role = map |> Map.tryFind "role" |> Option.defaultValue "user"
        Ok ({ login = login; pwd = pwd; role = role; name = name; targetCalories = 0.0 }:CreateUserPayload)
    | _ ->
        Error "Some data is missing"

let private retrieveUsers (model: Model) = ServerComm.retrieveUsers (model.customData)
let private addNewUser (model: Model, data: CreateUserPayload) = ServerComm.addNewUser (model.customData, data)
let private removeUser (model: Model, userId) = ServerComm.removeUser (model.customData, userId)

let update: Msg -> Model -> Model * Cmd<Msg> =
    TabularForms.update (retrieveUsers, validateEntry, addNewUser, removeUser)
