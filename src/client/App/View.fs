module App.View

open Elmish
open Fable.React
open Fable.React.Props
open Fulma

open App.Types
open ServerProtocol.V1

let topNav =
    Navbar.navbar [ Navbar.HasShadow ]
        [ Container.container []
            [ Navbar.Brand.div []
                [ Navbar.Item.a [ Navbar.Item.Props [ Href "/#/"] ]
                    [ img [ Src "http://bulma.io/images/bulma-logo.png"
                            Alt "Bulma: a modern CSS framework based on Flexbox" ] ]
                  Navbar.burger []
                    [ span [ ] [ ]
                      span [ ] [ ]
                      span [ ] [ ] ] ]
              
              Navbar.End.div []
                [ Navbar.Item.div [ Navbar.Item.HasDropdown; Navbar.Item.IsHoverable ]
                    [ Navbar.Link.a [] [ str "Account" ]
                      Navbar.Dropdown.div []
                        [ Navbar.Item.a [ Navbar.Item.Props [ Href "/#/"] ] [ str "Home" ]
                          Navbar.divider [] []
                          Navbar.Item.a [ Navbar.Item.Props [ Href <| Router.toPath Router.LoginScreen ] ] [ str "Logout" ]
                          ] ] ] ] ]
                          
let summaryData (user: UserInfo) (data: SummaryData list) =
    let cmp f = function
        | Some (x: SummaryData) -> f x.amount
        | None -> false
    let itemView (date, d: SummaryData option) =
        div [ classList [
                "card", true
                "exceed-target", d |> cmp ((<) user.target)
                "within-target", d |> cmp ((>=) user.target)
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

    let weekDayNames = [| "Sun"; "Mon"; "Tue"; "Wed"; "Thu"; "Fri"; "Sat" |]
    let monthNames = [|
        "January"; "February"; "March"; "April"; "May"; "June"; "July"
        "August"; "September"; "October"; "November"; "December"
      |]

    let now = System.DateTime.Now
    let firstDay = System.DateTime(now.Year, now.Month, 1)
    let lastDay = (firstDay.AddMonths 1).AddDays(-1.0)
    let days =
        [   for day in 1..lastDay.Day do
            let date = System.DateTime(now.Year, now.Month, day)
            let dayData = data |> List.tryFind (fun d -> d.rdate = date)
            yield date, dayData ]

    let blanks x = List.init x (fun _ -> None)
    let calendar = blanks (int firstDay.DayOfWeek) @ (List.map Some days) @ blanks (6 - int lastDay.DayOfWeek)
    let splitBy n =
        List.mapi (fun i d -> (i / n, d))
        >> List.groupBy fst
        >> List.map (snd >> List.map snd)

    div [ Class "summary-items" ]
        [ yield Heading.h2 [] [ str user.userName ]
          yield Heading.h3 [] [ str monthNames.[now.Month - 1]; str " "; str (string now.Year) ]
          yield Columns.columns [ Columns.IsGap (Screen.All, Columns.Is1) ]
                    (weekDayNames |> List.ofArray |> List.map (fun d -> Column.column [] [ strong [] [str d] ]))
          yield!
              calendar |> splitBy 7 |>
              List.map (fun week ->
                  Columns.columns [ Columns.IsGap (Screen.All, Columns.Is1) ]
                      (week |> List.map (function
                          | Some d -> Column.column [ ] [ itemView d ]
                          | None -> Column.column [ ] [ str "" ] ))
                  )
        ]

let view (Model (user, appview) as model) (dispatch : Msg -> unit) =

    let content =
        match appview with
        | NoView -> 
            strong [ ] [ str "Loading data..." ]
        | DayView x -> 
            div [ Class "app-screen-title" ]
              [ Heading.h2 [] [ str user.userName ]
                Heading.h3 [] [ str <| x.date.ToShortDateString() ]
                EntryForm.View.view x dispatch ]
        | SummaryData data ->
            summaryData user data
        | other ->
            span [] [ str "Other state "; strong [ ] [ str (sprintf "%A" other) ] ]

    let hrefToday = Router.toPath (Router.DailyView <| System.DateTime.Now)
    div [] [
        topNav
        Columns.columns [] [
            Column.column [ Column.Width (Screen.All, Column.Is3); Column.CustomClass "aside hero is-fullheight" ] [
                div [ ] [
                    div [ Class "today has-text-centered" ]
                        [ Button.a [ Button.Color IsDanger; Button.IsFullWidth; Button.Props [ Href hrefToday] ] [ str "Today" ] ]
                    div [ Class "main" ]
                        [ a [ Href "/#/"; Class "item active" ]
                            [ Icon.icon []
                                [ i [ Class "fa fa-inbox" ] [ ] ]
                              span [ Class "name" ] [ str "Summary" ] ]
                        ] ] ]            
            Column.column [ Column.Width(Screen.All, Column.Is9); Column.CustomClass "messages hero is-fullheight"]
                [ content ] ] ]
