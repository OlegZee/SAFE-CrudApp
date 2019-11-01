module Router

open CommonTypes
open Elmish.UrlParser

type Page =
    | Home      // routes to overview page
    | DailyView of System.DateTime
    | LoginScreen
    | SignupScreen
    | ManageUsers
    | UserOverview of UserId
    | UserDailyView of UserId * System.DateTime
    | Report

let toPath = function
    | Home -> "#/"
    | DailyView day -> "#/data/" + day.ToString("yyyy-MM-dd")
    | LoginScreen -> "#/login"
    | SignupScreen -> "#/signup"
    | ManageUsers -> "#/users"
    | UserOverview (UserId userId) -> sprintf "#/users/%i" userId
    | UserDailyView (UserId userId, day) -> sprintf "#/users/%i/data/%s" userId (day.ToString("yyyy-MM-dd"))
    | Report -> "#/report"

let dt state =
    let parseDate (s: string) =
        match s.Split('-') |> Array.map(System.Int32.TryParse) with
        | [| true, y; true, m; true, d |] -> Ok (System.DateTime(y,m,d))
        | _ -> Error "cannot parse date"
    custom "dt" parseDate state

let pageParser : Parser<Page->_,Page> =
    oneOf [
        map Home (s "")
        map DailyView (s "data" </> dt)
        map LoginScreen (s "login")
        map SignupScreen (s "signup")
        map ManageUsers (s "users")
        map (UserId >> UserOverview) (s "users" </> i32)
        map (fun u d -> UserDailyView(UserId u,d)) (s "users" </> i32 </> s "data" </> dt )
        map Report (s "report") ]

// Browser.Dom.console.log (parse pageParser "/" Map.empty)
// Browser.Dom.console.log (parse pageParser "/data/2019-10-25" Map.empty)
// Browser.Dom.console.log (parse pageParser "/login" Map.empty)
// Browser.Dom.console.log (parse pageParser "/login" Map.empty)
// Browser.Dom.console.log (parse pageParser "/users" Map.empty)
// Browser.Dom.console.log (parse pageParser "/users/111" Map.empty)
// Browser.Dom.console.log (parse pageParser "/users/111/data/2019-10-26" Map.empty)