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

let private monthNames = [|
    "January"; "February"; "March"; "April"; "May"; "June"; "July"
    "August"; "September"; "October"; "November"; "December"
  |]

let topNav =
    let homePath = Router.toPath Router.Home
    Navbar.navbar [ Navbar.HasShadow ]
        [ Container.container []
            [ Navbar.Brand.div []
                [ Navbar.Item.a [ Navbar.Item.Props [ Href homePath] ]
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
                        [ Navbar.Item.a [ Navbar.Item.Props [ Href homePath] ] [ str "Home" ]
                          Navbar.divider [] []
                          Navbar.Item.a [ Navbar.Item.Props [ Href <| Router.toPath Router.LoginScreen ] ] [ str "Logout" ]
                          ] ] ] ] ]
                          
let view (Model (user, appview) as model) (dispatch : Msg -> unit) =

    let content =
        match appview with
        | NoView -> 
            strong [ ] [ str "Loading data..." ]
        | DayView (date, user, x) ->
            let currentCalories = collectUserCalories x

            div [ Class "app-screen-title" ]
              [ Heading.h2 [] [ str user.userName ]     // TODO display other user
                Heading.h3 [] [ str <| date.ToShortDateString() ]
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
        | SummaryData data ->
            let now = System.DateTime.Now
    
            div [ Class "app-screen-title" ]
                [   Heading.h2 [] [ str <| user.userName ]
                    Heading.h3 [] [ str monthNames.[now.Month - 1]; str " "; str (string now.Year) ]
                    SummaryView.View.view data (SummaryViewMsg >> dispatch) ]
            
        | UserSummaryData (otherUser, data) ->
            let now = System.DateTime.Now
    
            div [ Class "app-screen-title" ]
                [   Heading.h2 [] [ str <| otherUser.userName ]
                    Heading.h3 [] [ str monthNames.[now.Month - 1]; str " "; str (string now.Year) ]
                    SummaryView.View.view data (SummaryViewMsg >> dispatch) ]  // TODO the other message user here
            
        | ManageUsers data ->
            div [ Class "app-screen-title" ]
                [   Heading.h2 [] [ str "Manage users" ]
                    ManageUsers.View.view (data, user.userRole) (ManageUsersMsg >> dispatch) ]
        | ReportView data ->
            div [ Class "app-screen-title" ]
                [   Heading.h2 [] [ str "Reports" ]
                    ReportsForm.View.view (data) (ReportViewMsg >> dispatch) ]
        | other ->
            div [ Class "app-screen-title" ]
              [ Heading.h2 [] [ str "Unknown state" ]
                Heading.h4 [] [ str (sprintf "%A" other) ] ]
    

    div [] [
        topNav
        Columns.columns [] [
            Column.column [ Column.Width (Screen.All, Column.Is3); Column.CustomClass "aside hero is-fullheight" ] [
                let hrefToday = Router.toPath (Router.DailyView <| System.DateTime.Now)
                yield div [ Class "main" ] [
                      Label.label [] [
                        str <| user.userName
                      ]
                      a [ Href <| Router.toPath Router.Home; Class "item active" ]
                            [ Icon.icon [] [ Fa.i [ Fa.Solid.CalendarAlt ] [] ]
                              span [ Class "name" ] [ str "Summary" ] ]
                      a [ Href hrefToday; Class "item active" ]
                            [ Icon.icon [] [ Fa.i [ Fa.Solid.CalendarDay ] [] ]
                              span [ Class "name" ] [ str "Today" ] ]
                      a [ Href <| Router.toPath Router.Report; Class "item active" ]
                            [ Icon.icon [] [ Fa.i [ Fa.Solid.BookOpen ] [] ]
                              span [ Class "name" ] [ str "Reports" ] ]
                      ]
                yield div [ Class "today" ]
                        [ Button.a [ Button.Color IsDanger; Button.IsFullWidth; Button.Props [ Href hrefToday] ] [ str "Today" ] ]
  
                // admin portion of site follows
                if user.userRole = "admin" || user.userRole = "manager" then
                    yield div [ Class "main" ] [
                        Label.label [] [ str "Admin tools"]
                        a [ Href (Router.toPath Router.ManageUsers); Class "item active" ]
                            [ Icon.icon [] [ Fa.i [ Fa.Solid.UserFriends ] [] ]
                              span [ Class "name" ] [ str "Users" ] ]
                    ]
              ]
            Column.column [ Column.Width(Screen.All, Column.Is9); Column.CustomClass "app-content hero is-fullheight"]
                [ content ] ] ]
