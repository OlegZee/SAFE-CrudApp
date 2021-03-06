module EntryForm.View

open Fable.React
open Fable.Core.JsInterop
open Fable.React.Props
open Browser.Dom
open Fulma
open Fable.FontAwesome

open Components
open CommonTypes
open EntryForm.Types

let recordEntry (recordId, r: DataRecord) dispatch =
    tr  []
        [ td [ Style [ TextAlign TextAlignOptions.Center] ] [ str r.rtime ]
          td [ ] [ str r.item ]
          td [ Style [ TextAlign TextAlignOptions.Right ] ] [ str <| r.amount.ToString() ]
          td [ Style [ TextAlign TextAlignOptions.Center ] ] [
            Button.button [ Button.OnClick (fun _ -> TabularForms.StartEdit recordId |> dispatch) ] [
                Icon.icon [ Icon.Props [ Title "Edit" ] ] [ Fa.i [ Fa.Solid.Pen ] [] ] ]
            Button.button [ Button.IsOutlined; Button.Color IsDanger; Button.OnClick (fun _ -> TabularForms.DeleteEntry recordId |> dispatch) ] [
                Icon.icon [ Icon.Props [ Title "Remove" ] ] [ Fa.i [ Fa.Solid.Trash ] [] ] ]
          ] ]

let editEntry (EntryId entryId, e: TabularForms.EntryData<DataRecord> ) dispatch =
    let pickField name = e.rawFields |> Map.tryFind name |> Option.defaultValue ""

    let handleChange (field: string) =
        Input.OnChange (fun e -> TabularForms.SetEditField (field, !!e.target?value) |> dispatch)
    let errors fieldName = ValidateHelpers.errors e.validated fieldName

    tr  [ ]
        [   td [ ] [
                yield Input.time [ Input.Placeholder "time"; Input.DefaultValue <| pickField "time"; handleChange "time" ]
                yield! errors "time" ]
            td [ ] [
                yield Input.text [ Input.Placeholder "expense item"; Input.DefaultValue <| pickField "item";  handleChange "item" ]
                yield! errors "item" ]
            td [ ] [
                yield Input.number [ Input.Placeholder "amount"; Input.DefaultValue <| pickField "amount"; handleChange "amount" ]
                yield! errors "amount" ]
            td [ Style [ TextAlign TextAlignOptions.Center ] ] [
                let disabled = e.validated |> function |Ok _ -> false | Error _ -> true
                yield Button.button [
                    Button.Disabled disabled
                    Button.Color (if disabled then IsGrey else IsSuccess)
                    Button.OnClick(fun _ -> dispatch TabularForms.SaveEditEntry) ] [ str "Apply" ]
                yield Button.button [
                    Button.OnClick(fun _ -> dispatch TabularForms.CancelEdit) ] [ str "Cancel" ] ]
            ]
        
let inputEntry (map: Map<string,string>, v: Result<DataRecord, Map<string, string list>>) dispatch =
    let pickField name = map |> Map.tryFind name |> Option.defaultValue ""

    let handleChange (field: string) =
        Input.OnChange (fun e -> TabularForms.SetNewField (field, !!e.target?value) |> dispatch)
    let errors fieldName = ValidateHelpers.errors v fieldName

    tr  [ ]
        [   td [ ] [
                yield Input.time [ Input.Placeholder "time"; Input.DefaultValue <| pickField "time"; handleChange "time" ]
                yield! errors "time" ]
            td [ ] [
                yield Input.text [ Input.Placeholder "item"; Input.DefaultValue <| pickField "item";  handleChange "item" ]
                yield! errors "item" ]
            td [ ] [
                yield Input.number [ Input.Placeholder "amount"; Input.DefaultValue <| pickField "amount"; handleChange "amount" ]
                yield! errors "amount" ]
            td [ Style [ TextAlign TextAlignOptions.Center ] ] [
                let disabled = v |> function | Ok _ -> false | Error e -> true
                yield Button.button [
                    Button.IsFullWidth; Button.Disabled disabled
                    Button.Color (if disabled then IsGrey else IsSuccess)
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
                       th [ ] [ str "item" ]
                       th [ ] [ str "Amount" ]
                       th [ ] [ str "" ] ] ]
              tbody [ ]
                [
                    yield! (entries |> List.map
                        (fun record ->
                            match model.edited with
                            | Some (rkey,edited) when rkey = fst record ->
                                editEntry (rkey, edited) dispatch
                            | _ ->
                                recordEntry record dispatch
                        ))
                    yield inputEntry (model.newrec.rawFields, model.newrec.validated) dispatch
                ]
            ]
    | _ ->
        div [] [ str "Loading data"]
