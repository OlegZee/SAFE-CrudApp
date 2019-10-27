module App.SummaryView

open Browser.Dom
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Fulma

open App.Types
open ServerProtocol.V1

module private Internals =
    let collectUserCalories (m: EntryForm.Types.Model): float =
        
        match m.data with
        | EntryForm.Types.Data records -> records |> List.sumBy (fun { amount = a } -> a)
        | _ -> 0.0

    let weekDayNames = [| "Sun"; "Mon"; "Tue"; "Wed"; "Thu"; "Fri"; "Sat" |]
    let monthNames = [|
        "January"; "February"; "March"; "April"; "May"; "June"; "July"
        "August"; "September"; "October"; "November"; "December"
      |]


    let itemView exceedTarget (date: System.DateTime, d: SummaryData option) =
        let exceed f = function
            | Some (x: SummaryData) -> exceedTarget x.amount |> f
            | None -> false

        div [ classList [
                "card", true
                "exceed-target", exceed id d
                "within-target", exceed not d
                ] ] [
            a [ Href <| Router.toPath (Router.DailyView date) ] [
                div [ Class "card-content" ] [
                    yield span [ Class "summary-date" ] [ str (string date.Day) ]
                    match d with
                    | Some x ->
                        yield span [ Class "summary-amount" ] [
                            str <| x.amount.ToString()
                            str " calories" ]
                    | None -> yield str ""
                ] ]
        ]

open Internals
open Fable.FontAwesome

let view ({user = user; data = data } as model) dispatch =

    // console.log("data", data)

    let now = System.DateTime.Now
    let firstDay = System.DateTime(now.Year, now.Month, 1)
    let lastDay = (firstDay.AddMonths 1).AddDays(-1.0)
    let days =
        [   for day in 1..lastDay.Day do
            let date = System.DateTime(now.Year, now.Month, day).Date
            let dayData = data |> List.tryFind (fun d -> d.rdate.Year = date.Year && d.rdate.Month = date.Month && d.rdate.Day = date.Day)
            // console.log("daydata", date, dayData)
            yield date, dayData ]

    let blanks x = List.init x (fun _ -> None)
    let calendar = blanks (int firstDay.DayOfWeek) @ (List.map Some days) @ blanks (6 - int lastDay.DayOfWeek)
    let splitBy n =
        List.mapi (fun i d -> (i / n, d))
        >> List.groupBy fst
        >> List.map (snd >> List.map snd)

    let inputTargetRef = createRef None

    div [ Class "summary-items" ]
        [ yield Columns.columns [ Columns.IsGap (Screen.All, Columns.Is1) ]
                    (weekDayNames |> List.ofArray |> List.map (fun d -> Column.column [] [ strong [] [str d] ]))
          yield!
              calendar |> splitBy 7 |>
              List.map (fun week ->
                  Columns.columns [ Columns.IsGap (Screen.All, Columns.Is1) ]
                      (week |> List.map (function
                          | Some d -> Column.column [ ] [ itemView (fun x -> x > user.target) d ]
                          | None -> Column.column [ ] [ str "" ] ))
                  )
          let targetValueControl =
                match model.editedTarget with
                | Some x -> span [] [
                    Input.number [ Input.Props [ RefValue inputTargetRef; Style [ Width "130px"] ]; Input.DefaultValue <| x.ToString() ]
                    Button.button [ Button.OnClick (fun _ -> dispatch (SaveValue inputTargetRef.current.Value?value)) ]
                        [ Icon.icon [] [ Fa.i [ Fa.Solid.Upload ] [] ] ]
                    Button.button [ Button.OnClick (fun _ -> dispatch CancelEdit) ]
                        [ Icon.icon [ Icon.Size IsSmall ] [ Fa.i [ Fa.Solid.WindowClose ] [] ] ]
                     ]
                | None -> span [] [
                    str <| user.target.ToString()
                    Button.button [ Button.Size IsSmall; Button.OnClick (fun _ -> dispatch EditTarget) ]
                        [ Icon.icon [ Icon.Size IsSmall ] [ Fa.i [ Fa.Regular.Edit ] [] ] ]
                    ]

          yield Heading.h4 [] [
              span [] [ str "Target Calories: " ]
              targetValueControl
          ]
        ]
