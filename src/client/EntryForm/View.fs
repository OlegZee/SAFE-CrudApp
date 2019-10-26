module EntryForm.View

open Fable.React
open Fable.Core.JsInterop
open Fable.React.Props
open Browser.Dom
open Fulma
open ServerProtocol.V1
open Fable.FontAwesome

let recordEntry (r: UserData) dispatch =
    tr  []
        [ td [ Style [ TextAlign TextAlignOptions.Center] ] [ str r.rtime ]
          td [ ] [ str r.meal ]
          td [ Style [ TextAlign TextAlignOptions.Right ] ] [ str <| r.amount.ToString() ]
          td [ Style [ TextAlign TextAlignOptions.Center ] ] [
            Button.button [ Button.OnClick (fun _ -> window.alert "Editing is not implemented yet" ) ] [    // FIXME
                Icon.icon [ Icon.Props [ Title "Edit" ] ] [ Fa.i [ Fa.Solid.Pen ] [] ] ]
            Button.button [ Button.IsOutlined; Button.Color IsDanger; Button.OnClick (fun _ -> DeleteEntry r.record_id |> dispatch) ] [
                Icon.icon [ Icon.Props [ Title "Remove" ] ] [ Fa.i [ Fa.Solid.Trash ] [] ] ]
          ] ]

let inputEntry (e: NewEntry, v: Result<CreateUserData,string>) dispatch =
    let handleChange (msg: string -> Msg) =
        Input.OnChange (fun e -> !!e.target?value |> msg |> dispatch)
    tr  [ ]
        [   td [ ] [ Input.time [ Input.Placeholder "time"; Input.DefaultValue e.time; handleChange SetNewTime ] ]
            td [ ] [ Input.text [ Input.Placeholder "meal"; Input.DefaultValue e.meal;  handleChange SetNewMeal ] ]
            td [ ] [ Input.number [ Input.Placeholder "amount"; Input.DefaultValue e.amount; handleChange SetNewAmount ] ]
            td [ Style [ TextAlign TextAlignOptions.Center ] ] [
                let disabled, title = v |> function | Ok _ -> false, "" | Error e -> true, e
                yield Button.button [
                    Button.IsFullWidth; Button.Disabled disabled
                    Button.Props [ Title title ]; Button.Color IsSuccess
                    Button.OnClick(fun _ -> dispatch SaveNewEntry) ] [ str "Add" ] ]
            ]
  
let view (model: Model) (dispatch: Msg -> unit) =
    match model.data with
    | ModelState.Data entries ->
        Table.table [ Table.IsBordered
                      Table.IsFullWidth
                      Table.IsStriped ]
            [ thead [ ]
                [ tr [ ]
                     [ th [ ] [ str "Time" ]
                       th [ ] [ str "Meal" ]
                       th [ ] [ str "Amount" ]
                       th [ ] [ str "" ] ] ]
              tbody [ ]
                [
                    yield! (entries |> List.map (fun r -> recordEntry r dispatch))
                    yield inputEntry (model.newEntry, model.newEntryValid) dispatch
                ]
            ]
    | _ ->
        div [] [ str "Loading data"]
