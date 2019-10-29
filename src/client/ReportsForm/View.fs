module ReportsForm.View

open System
open Types
open Fable.React
open Fable.React.Props
open Fable.Core.JsInterop

open Browser.Dom
open Fulma

open ServerProtocol.V1
open Fable.FontAwesome

let reportLine (data: SummaryData) =
    tr [] [
        td [ ] [ str <| data.rdate.ToShortDateString() ]
        td [ ] [ str <| data.count.ToString() ]
        td [ Style [ TextAlign TextAlignOptions.Right ] ] [ str <| sprintf "%.2f" data.amount ]
    ]

let queryParameters (dispatch) =
    let dat1Ref, dat2Ref = (createRef None, createRef None)
    let time1Ref, time2Ref = createRef None, createRef None

    let runButtonHandler _ =
        let take (name: string) parse (r: IRefValue<_>) =
            r.current |> Option.bind (fun e -> if e?value = "" then None else Some (parse e?value))
            |> function
            | Some(true, date) -> Ok (Some date)
            | None -> Ok None
            | _ -> Error ("invalid value: " + name)
        let query =
            Ok (State.initQuery())
            |> Result.bind (fun query -> 
                match take "start date" DateTime.TryParse dat1Ref, take "end date" DateTime.TryParse dat2Ref with
                | Ok dat1, Ok dat2 -> Ok { query with dateStart = dat1; dateEnd = dat2 }
                | Error e, _ | _, Error e -> Error e)
            |> Result.bind (fun query -> 
                match take "time start" TimeSpan.TryParse time1Ref, take "time end" TimeSpan.TryParse time2Ref with
                | Ok start, Ok end' -> Ok { query with timeStart = start; timeEnd = end' }
                | Error e, _ | _, Error e -> Error e)

        match query with
        | Ok query -> dispatch query
        | Error e -> window.alert e

    div [] [
        Field.div [ ]
            [ Label.label [ ] [ str "Start date" ]
              Input.date [ Input.Placeholder "Start from"; Input.Props [RefValue dat1Ref] ] ]
        Field.div [ ]
            [ Label.label [ ] [ str "End date" ]
              Input.date [ Input.Placeholder "To"; Input.Props [RefValue dat2Ref] ] ]
        Field.div [ ]
            [ Label.label [ ] [ str "Time start" ]
              Input.time [ Input.Placeholder "Start time"; Input.Props [RefValue time1Ref] ] ]
        Field.div [ ]
            [ Label.label [ ] [ str "End time" ]
              Input.time [ Input.Placeholder "End time"; Input.Props [RefValue time2Ref] ] ]
        Button.button [ Button.IsFullWidth; Button.Color IsSuccess; Button.OnClick runButtonHandler ] [ str "Run"]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Columns.columns [ ]
        [ Column.column [ Column.Width (Screen.All, Column.Is3) ]
            [ queryParameters (QueryData >> dispatch) ]
          Column.column [ Column.Width (Screen.All, Column.Is9); Column.Modifiers [ Modifier.BackgroundColor IsLight] ]
            [
                yield Label.label [ ] [ str "Report results" ]
                match model.data with
                | [] ->
                    yield strong [] [ str "No data matches the query"]
                | records ->
                    yield Table.table [ Table.IsBordered; Table.IsStriped ] [
                        thead [ ]
                            [ tr [ ]
                                 [ th [ ] [ str "Date" ]
                                   th [ ] [ str "Entries Count" ]
                                   th [ ] [ str "Amount (calories)" ] ] ]
                        tbody [ ] (records |> List.map reportLine) ]
                        ]
        ]
    