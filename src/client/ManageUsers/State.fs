module ManageUsers.State

open Elmish
open Fable.Validation.Core

open Components
open ServerProtocol.V1

open ManageUsers.Types

let init () : Model * Cmd<Msg> =
    TabularForms.init ()

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
          expenseLimit = 0.
        }

let private toCreatePayload (data: UserData) : CreateUserPayload =
    {   login = data.login
        name = data.name
        role = data.role
        expenseLimit = data.expenseLimit
        pwd = data.pwd }

let private toUpdatePayload (data: UserData) : UpdateUserPayload =
    {   login = data.login
        name = data.name
        role = data.role
        expenseLimit = data.expenseLimit }
        
let toData (payload: User) : (CommonTypes.UserId * UserData) =
    CommonTypes.UserId payload.user_id,
    {   login = payload.login
        name = payload.name
        role = payload.role
        expenseLimit = payload.expenseLimit
        pwd = "" }

        
let private getFields (d: UserData) =
    Map.empty
    |> Map.add "login" d.login
    |> Map.add "name" d.name
    |> Map.add "role" d.role
    |> Map.add "expenseLimit" (string d.expenseLimit)
    |> Map.add "pwd" "dummy value (never seen)"

let private retrieveUsers _ = ServerComm.retrieveUsers () |> TabularForms.mapPromiseResult (List.map toData)
let private addNewUser (_, data: UserData) = ServerComm.addNewUser (toCreatePayload data)
let private removeUser (_, userId) = ServerComm.removeUser userId
let private updateUser (_, userId, data: UserData) = ServerComm.updateUser (userId, (toUpdatePayload data))

let update: Msg -> Model -> Model * Cmd<Msg> =
    TabularForms.update (retrieveUsers, getFields, validateEntry, addNewUser, updateUser, removeUser)
