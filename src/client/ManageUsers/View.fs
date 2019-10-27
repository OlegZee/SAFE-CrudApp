module ManageUsers.View

open Fable.React
open Fable.React.Props
open Browser.Dom
open Fulma

open ManageUsers.Types
open ServerProtocol.V1
open Fable.FontAwesome

// let inputEntry (e: NewEntry, v: Result<CreateUserData,string>) dispatch =
//     let handleChange (msg: string -> Msg) =
//         Input.OnChange (fun e -> !!e.target?value |> msg |> dispatch)
//     tr  [ ]
//         [   td [ ] [ Input.time [ Input.Placeholder "time"; Input.DefaultValue e.time; handleChange SetNewTime ] ]
//             td [ ] [ Input.text [ Input.Placeholder "meal"; Input.DefaultValue e.meal;  handleChange SetNewMeal ] ]
//             td [ ] [ Input.number [ Input.Placeholder "amount"; Input.DefaultValue e.amount; handleChange SetNewAmount ] ]
//             td [ Style [ TextAlign TextAlignOptions.Center ] ] [
//                 let disabled, title = v |> function | Ok _ -> false, "" | Error e -> true, e
//                 yield Button.button [
//                     Button.IsFullWidth; Button.Disabled disabled
//                     Button.Props [ Title title ]; Button.Color IsSuccess
//                     Button.OnClick(fun _ -> dispatch SaveNewEntry) ] [ str "Add" ] ]
//             ]

let private recordEntry (r: User) dispatch =
    tr  []
        [ td [ Style [ ] ] [ str <| r.user_id.ToString() ]
          td [ ] [ str r.login ]
          td [ Style [ ] ] [ str r.name ]
          td [ Style [ TextAlign TextAlignOptions.Center ] ] [
            Button.button [ Button.OnClick (fun _ -> window.alert "Editing is not implemented yet" ) ] [    // FIXME
                Icon.icon [ Icon.Props [ Title "Edit" ] ] [ Fa.i [ Fa.Solid.Pen ] [] ] ]
            Button.button [ Button.IsOutlined; Button.Color IsDanger; Button.OnClick (fun _ -> TabularFormTypes.DeleteEntry r.user_id |> dispatch) ] [
                Icon.icon [ Icon.Props [ Title "Remove" ] ] [ Fa.i [ Fa.Solid.Trash ] [] ] ]
          ] ]

let view (model: Model) (dispatch: Msg -> unit) =
    match model.data with
    | TabularFormTypes.ModelState.Data entries ->
        Table.table [ Table.IsBordered
                      Table.IsFullWidth
                      Table.IsStriped ]
            [ thead [ ]
                [ tr [ ]
                     [ th [ ] [ str "Id" ]
                       th [ ] [ str "Login" ]
                       th [ ] [ str "Name" ]
                       th [ ] [ str "Actions" ] ] ]
              tbody [ ]
                [
                    yield! (entries |> List.map (fun r -> recordEntry r dispatch))
                    // yield inputEntry (model.newEntry, model.newEntryValid) dispatch
                ]
            ]
    | _ ->
        div [] [ str "Loading data"]
