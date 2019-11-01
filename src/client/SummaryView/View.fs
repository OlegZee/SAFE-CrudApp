module SummaryView.View

open Browser.Dom
open Fable.Core.JsInterop
open Fable.FontAwesome
open Fable.React
open Fable.React.Props
open Fulma

open Types
open CommonTypes
open ServerProtocol.V1

module private Internals =
    let weekDayNames = [| "Sun"; "Mon"; "Tue"; "Wed"; "Thu"; "Fri"; "Sat" |]

    let itemView href exceedTarget (date: System.DateTime, d: SummaryData option) =
        let exceed f = function
            | Some (x: SummaryData) -> exceedTarget x.amount |> f
            | None -> false

        let now = System.DateTime.Now
        let isFuture date = date > now
        let hrefIfEditable item = if isFuture date then item else a [ Href href ] [item]

        div [ classList [
                "card", true
                "exceed-target", exceed id d
                "within-target", exceed not d
                ] ] [
            hrefIfEditable(
                div [ Class "card-content" ] [
                    yield span [ classList ["summary-date", true; "summary-date-ro", isFuture date ] ] [ str (string date.Day) ]
                    match d with
                    | Some x ->
                        yield span [ Class "summary-amount" ] [
                            str <| x.amount.ToString()
                            str " calories" ]
                    | None -> yield str ""
                ])
        ]

open Internals

let view ({user = user; data = data; otherUser = otherUserId } as model) dispatch =

    let now = System.DateTime.Now
    let firstDay = System.DateTime(now.Year, now.Month, 1)
    let lastDay = (firstDay.AddMonths 1).AddDays(-1.0)
    let days =
        [   for day in 1..lastDay.Day do
            let date = System.DateTime(now.Year, now.Month, day)
            let dayData = data |> List.tryFind (fun d -> d.rdate.ToLocalTime() = date)
            yield date, dayData ]

    let blanks x = List.init x (fun _ -> None)
    let calendarCells = blanks (int firstDay.DayOfWeek) @ (List.map Some days) @ blanks (6 - int lastDay.DayOfWeek)
    let splitBy n =
        List.mapi (fun i d -> (i / n, d))
        >> List.groupBy fst
        >> List.map (snd >> List.map snd)

    let inputTargetRef = createRef None

    div [ Class "summary-items" ]
        [ yield Columns.columns [ Columns.IsGap (Screen.All, Columns.Is1) ]
                    (weekDayNames |> List.ofArray |> List.map (fun d -> Column.column [] [ strong [] [str d] ]))
          yield!
              calendarCells |> splitBy 7 |>
              List.map (fun week ->
                  Columns.columns [ Columns.IsGap (Screen.All, Columns.Is1) ]
                      (week |> List.map (function
                          | Some ((date, _) as d) ->
                            let href = function
                                | Some userId -> Router.UserDailyView (userId, date)
                                | None -> Router.DailyView date

                            Column.column [ ] [ itemView (href otherUserId |> Router.toPath) (fun x -> x > user.target) d ]
                          | None -> Column.column [ ] [ str "" ] ))
                  )
          let targetValueControl =
                match model.editedTarget, otherUserId with
                | Some x, None -> span [] [
                    Input.number [ Input.Props [ RefValue inputTargetRef; Style [ Width "130px"] ]; Input.DefaultValue <| x.ToString() ]
                    Button.button [ Button.OnClick (fun _ -> dispatch (SaveValue inputTargetRef.current.Value?value)) ]
                        [ Icon.icon [] [ Fa.i [ Fa.Solid.Upload ] [] ] ]
                    Button.button [ Button.OnClick (fun _ -> dispatch CancelEdit) ]
                        [ Icon.icon [ Icon.Size IsSmall ] [ Fa.i [ Fa.Solid.WindowClose ] [] ] ]
                     ]
                | None, None -> span [] [
                    str <| user.target.ToString()
                    Button.button [ Button.Size IsSmall; Button.OnClick (fun _ -> dispatch EditTarget) ]
                        [ Icon.icon [ Icon.Size IsSmall ] [ Fa.i [ Fa.Regular.Edit ] [] ] ]
                    ]
                | _ -> span [] [ str <| user.target.ToString() ]

          yield Heading.h4 [] [
              span [] [ str "Target Calories: " ]
              targetValueControl
          ]
        ]
