module ManageUsers.View

open Fable.React
open Fable.React.Props
open Fable.Core.JsInterop
open Fable.FontAwesome

open Browser.Dom
open Fulma

open Components
open ManageUsers.Types
open CommonTypes
open ServerProtocol.V1

let inputEntry (e: Map<string,string>, v: Result<CreateUserPayload, Map<string, string list>>) dispatch =
    let handleChange (field: string) =
        (fun (e: Browser.Types.Event) -> TabularForms.SetNewField (field, !!e.target?value) |> dispatch)
    let pickField name = e |> Map.tryFind name |> Option.defaultValue ""
    let errors fieldName = ValidateHelpers.errors v fieldName

    tr  [ ]
        [   td [ ] [ ]
            td [ ] [
                yield Input.text [ Input.Placeholder "login"; Input.DefaultValue <| pickField "login"; Input.OnChange (handleChange "login") ]
                yield! errors "login" ]
            td [ ] [
                yield Input.text [ Input.Placeholder "name"; Input.DefaultValue <| pickField "name"; Input.OnChange (handleChange "name") ]
                yield! errors "name" ]
            td [ ] [
                yield Select.select [ Select.Props [ OnChange (handleChange "role") ]] [
                    select [ DefaultValue "user" ] [
                        option [ Value "user" ] [ str "User" ]
                        option [ Value "manager" ] [ str "Manager" ]
                        option [ Value "admin" ] [ str "Admin" ] ]
                    ]
                yield! errors "role"
                ]
            td [ ] [
                yield Input.password [ Input.Placeholder "password"; Input.DefaultValue <| pickField "pwd"; Input.OnChange (handleChange "pwd") ]
                yield! errors "pwd" ]
            td [ Style [ TextAlign TextAlignOptions.Center ] ] [
                let disabled = v |> function | Ok _ -> false | Error _ -> true
                yield Button.button [
                    Button.IsFullWidth; Button.Disabled disabled
                    Button.Color (if disabled then IsGrey else IsSuccess)
                    Button.OnClick(fun _ -> dispatch TabularForms.SaveNewEntry) ] [ str "Add" ] ]
            ]

let private recordEntry (r: User, role) dispatch =
    tr [] [
        td [ Style [ ] ] [ str <| sprintf "%i" r.user_id ]
        td [ ] [ str r.login ]
        td [ Style [ ] ] [ str r.name ]
        td [ Style [ ] ] [ str r.role ]
        td [ ] [ ]
        td [ Style [ TextAlign TextAlignOptions.Center ] ] [
            if role = "admin" then
                yield Button.a [ Button.Props [ Href <| Router.toPath (Router.UserOverview <| UserId r.user_id) ] ] [
                    Icon.icon [ Icon.Props [ Title "View user summary" ] ] [ Fa.i [ Fa.Solid.CalendarAlt ] [] ] ]
            yield Button.button [ Button.OnClick (fun _ -> window.alert "Editing is not implemented yet" ) ] [    // FIXME
                Icon.icon [ Icon.Props [ Title "Edit" ] ] [ Fa.i [ Fa.Solid.Pen ] [] ] ]
            yield Button.button [ Button.IsOutlined; Button.Color IsDanger; Button.OnClick (fun _ -> TabularForms.DeleteEntry (UserId r.user_id) |> dispatch) ] [
                Icon.icon [ Icon.Props [ Title "Remove" ] ] [ Fa.i [ Fa.Solid.Trash ] [] ] ]
        ] ]

let view (model: Model, role: string) (dispatch: Msg -> unit) =
    match model.data with
    | TabularForms.ModelState.DataLoaded entries ->
        div [] [
            yield Table.table [ Table.IsBordered; Table.IsFullWidth; Table.IsStriped ] [
                thead [ ]
                    [ tr [ ]
                         [ th [ ] [ str "Id" ]
                           th [ ] [ str "Login" ]
                           th [ ] [ str "Name" ]
                           th [ ] [ str "Role" ]
                           th [ ] [ str "Pwd" ]
                           th [ ] [ str "Actions" ] ] ]
                tbody [ ]
                    [
                        yield! (entries |> List.map (fun r -> recordEntry (r, role) dispatch))
                        yield inputEntry (model.newEntry, model.newEntryValid) dispatch
                    ] ]

            match model.lastError with
            | Some e ->
                yield Notification.notification [ Notification.Color IsDanger ]
                        [ Notification.delete [ Props [ OnClick (fun _ -> dispatch (TabularForms.SetLastError None)) ] ] [ ]
                          str e ]
            | _ -> yield! []
        
        ]
    | _ ->
        div [] [ str "Loading data"]
