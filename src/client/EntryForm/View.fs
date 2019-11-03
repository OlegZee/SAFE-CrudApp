module EntryForm.View

open Fable.React
open Fable.Core.JsInterop
open Fable.React.Props
open Browser.Dom
open Fulma
open Fable.FontAwesome

open Components
open CommonTypes
open ServerProtocol.V1
open EntryForm.Types

let recordEntry (r: UserData) dispatch =
    tr  []
        [ td [ Style [ TextAlign TextAlignOptions.Center] ] [ str r.rtime ]
          td [ ] [ str r.meal ]
          td [ Style [ TextAlign TextAlignOptions.Right ] ] [ str <| r.amount.ToString() ]
          td [ Style [ TextAlign TextAlignOptions.Center ] ] [
            Button.button [ Button.OnClick (fun _ -> window.alert "Editing is not implemented yet" ) ] [    // FIXME
                Icon.icon [ Icon.Props [ Title "Edit" ] ] [ Fa.i [ Fa.Solid.Pen ] [] ] ]
            Button.button [ Button.IsOutlined; Button.Color IsDanger; Button.OnClick (fun _ -> TabularForms.DeleteEntry (EntryId r.record_id) |> dispatch) ] [
                Icon.icon [ Icon.Props [ Title "Remove" ] ] [ Fa.i [ Fa.Solid.Trash ] [] ] ]
          ] ]

let inputEntry (map: Map<string,string>, v: Result<PostDataPayload,string>) dispatch =
    let pickField name = map |> Map.tryFind name |> Option.defaultValue ""

    let handleChange (field: string) =
        Input.OnChange (fun e -> TabularForms.SetNewField (field, !!e.target?value) |> dispatch)
    tr  [ ]
        [   td [ ] [ Input.time [ Input.Placeholder "time"; Input.DefaultValue <| pickField "time"; handleChange "time" ] ]
            td [ ] [ Input.text [ Input.Placeholder "meal"; Input.DefaultValue <| pickField "meal";  handleChange "meal" ] ]
            td [ ] [ Input.number [ Input.Placeholder "amount"; Input.DefaultValue <| pickField "amount"; handleChange "amount" ] ]
            td [ Style [ TextAlign TextAlignOptions.Center ] ] [
                let disabled, title = v |> function | Ok _ -> false, "" | Error e -> true, e
                yield Button.button [
                    Button.IsFullWidth; Button.Disabled disabled
                    Button.Props [ Title title ]; Button.Color IsSuccess
                    Button.OnClick(fun _ -> dispatch TabularForms.SaveNewEntry) ] [ str "Add" ] ]
            ]
  
let view (model: Model) (dispatch: Msg -> unit) =
    match model.data with
    | TabularForms.DataLoaded entries ->
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
