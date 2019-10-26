module EntryForm.View

open Elmish
open Fable.React
open Fable.Core.JsInterop
open Fable.React.Props
open Browser.Dom
open Fulma
open ServerProtocol.V1
open Fable.FontAwesome

let recordEntry =
    function
    | Unchanged (r: UserData)
    | Dirty (r,_) ->
        tr  [ OnClick (fun _ -> console.log("click")) ]
            [ td [ Style [ TextAlign TextAlignOptions.Center ] ]
                 [ str r.rtime ]
              td [ ] [ str r.meal ]
              td [ Style [ TextAlign TextAlignOptions.Right] ]
                 [ str <| r.amount.ToString() ]
              td [] [] ]

let inputEntry (e: NewEntry, v: Result<CreateUserData,string>) dispatch =
    let handleChange (msg: string -> Msg) =
        Input.OnChange (fun e -> !!e.target?value |> msg |> dispatch)
    tr  [ ]
        [   td [ ] [ Input.time [ Input.Placeholder "time"; Input.DefaultValue e.time; handleChange SetNewTime ] ]
            td [ ] [ Input.text [ Input.Placeholder "meal"; Input.DefaultValue e.meal;  handleChange SetNewMeal ] ]
            td [ ] [ Input.number [ Input.Placeholder "amount"; Input.DefaultValue e.amount; handleChange SetNewAmount ] ]
            td [ ] [
                v |> function
                | Ok _ -> Button.button [ Button.Color IsPrimary; Button.OnClick(fun _ -> dispatch SaveNewEntry) ] [ str "add"]
                | Error e ->
                    Icon.icon [ Icon.Size IsMedium; Icon.Props [ Title e ] ]
                     [ Fa.i [ Fa.Solid.ExclamationTriangle ] [] ]
                ]
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
                    yield! (entries |> List.map recordEntry)
                    yield inputEntry (model.newEntry, model.newEntryValid) dispatch
                ]
            ]
    | _ ->
        div [] [ str "Loading data"]
