module App.View

open Fable.React
open Fable.React.Props
open Fulma

open App.Types
open ServerProtocol.V1
open Fable.FontAwesome

let collectUserCalories (m: EntryForm.Types.Model): float =
    
    match m.data with
    | EntryForm.Types.Data records -> records |> List.sumBy (fun { amount = a } -> a)
    | _ -> 0.0

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
                          
let view (Model (user, appview) as model) (dispatch : Msg -> unit) =

    let content =
        match appview with
        | NoView -> 
            strong [ ] [ str "Loading data..." ]
        | DayView x ->
            let currentCalories = collectUserCalories x

            div [ Class "app-screen-title" ]
              [ Heading.h2 [] [ str user.userName ]
                Heading.h3 [] [ str <| x.date.ToShortDateString() ]
                EntryForm.View.view x (DayViewMsg >> dispatch)
                Level.level [ ]
                    [ Level.item [ Level.Item.HasTextCentered ]
                        [ div [ ]
                            [ Level.heading [ ] [ str "Target" ]
                              Level.title [ ] [ str <| user.target.ToString() ] ] ]
                      Level.item [ Level.Item.HasTextCentered ]
                        [ div [ classList [ "exceed-target", currentCalories >= user.target] ]
                            [ Level.heading [ ] [ str "Current" ]
                              Level.title [ ] [ str <| currentCalories.ToString() ] ] ]
                    ]
                ]
        | SummaryData data -> SummaryView.view data (SummaryViewMsg >> dispatch)
        | other ->
            span [] [ str "Other state "; strong [ ] [ str (sprintf "%A" other) ] ]

    let hrefToday = Router.toPath (Router.DailyView <| System.DateTime.Now)
    div [] [
        topNav
        Columns.columns [] [
            Column.column [ Column.Width (Screen.All, Column.Is3); Column.CustomClass "aside hero is-fullheight" ] [
                div [ Class "main" ] [
                      a [ Href "/#/"; Class "item active" ]
                            [ Icon.icon [] [ Fa.i [ Fa.Solid.CalendarAlt ] [] ]
                              span [ Class "name" ] [ str "Summary" ] ]
                      a [ Href "/#/"; Class "item active" ]
                            [ Icon.icon [] [ Fa.i [ Fa.Solid.UserFriends ] [] ]
                              span [ Class "name" ] [ str "Users" ] ]
                      ]
                div [ Class "today" ]
                        [ Button.a [ Button.Color IsDanger; Button.IsFullWidth; Button.Props [ Href hrefToday] ] [ str "Today" ] ]
                ]
            Column.column [ Column.Width(Screen.All, Column.Is9); Column.CustomClass "app-content hero is-fullheight"]
                [ content ] ] ]
