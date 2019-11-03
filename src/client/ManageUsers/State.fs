module ManageUsers.State

open Elmish
open Fable.Validation.Core

open Components
open Components.ValidateHelpers
open ServerProtocol.V1

open ManageUsers.Types

let init token : Model * Cmd<Msg> =
    TabularForms.init token

let private validateEntry (map: Map<string,string>) =
    all <| fun t ->
        let fromMap name = (map |> Map.tryFind name |> Option.defaultValue "") |> t.Test name in
        { login = fromMap "login"
            |> t.Trim
            |> t.NotBlank "cannot be blank"
            |> t.MaxLen 30 "maxlen is {len}"
            |> t.MinLen 3 "minlen is {len}"
            |> t.End
          name = fromMap "name"
              |> t.Trim
              |> t.NotBlank "cannot be blank"
              |> t.MaxLen 30 "maxlen is {len}"
              |> t.MinLen 3 "minlen is {len}"
              |> t.End
          role = map |> Map.tryFind "role" |> Option.defaultValue "user"
          pwd = fromMap "pwd"
              |> t.Trim
              |> t.NotBlank "cannot be blank"
              |> t.MaxLen 100 "maxlen is {len}"
              |> t.MinLen 3 "minlen is {len}"
              |> t.End
          targetCalories = 0.
        }

let private retrieveUsers (model: Model) = ServerComm.retrieveUsers (model.customData)
let private addNewUser (model: Model, data: CreateUserPayload) = ServerComm.addNewUser (model.customData, data)
let private removeUser (model: Model, userId) = ServerComm.removeUser (model.customData, userId)

let update: Msg -> Model -> Model * Cmd<Msg> =
    TabularForms.update (retrieveUsers, validateEntry, addNewUser, removeUser)
