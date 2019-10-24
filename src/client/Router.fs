module Router

open Elmish.UrlParser

type Page =
    | Home      // routes to overview page
    | CaloriesInput of System.DateTime
    | LoginScreen
    | ManageUsers
    | UserOverview of int
    | UserCaloriesInput of int * System.DateTime

let toPath = function
    | Home -> "#/"
    | CaloriesInput day -> "#/data/" + day.ToString("yyyy-mm-dd")
    | LoginScreen -> "#/login"
    | ManageUsers -> "#/users"
    | UserOverview userId -> sprintf "#/users/%i" userId
    | UserCaloriesInput (userId, day) -> sprintf "#/users/%i/data/%s" userId (day.ToString("yyyy-mm-dd"))

let dt state =
    let parseDate (s: string) =
        match s.Split('-') |> Array.map(System.Int32.TryParse) with
        | [| true, y; true, m; true, d |] -> Ok (System.DateTime(y,m,d))
        | _ -> Error "cannot parse date"
    custom "dt" parseDate state

let pageParser : Parser<Page->_,Page> =
    oneOf [
        map Home (s "")
        map CaloriesInput (s "data" </> dt)
        map LoginScreen (s "login")
        map ManageUsers (s "users")
        map UserOverview (s "users" </> i32)
        map (fun u d -> UserCaloriesInput(u,d)) (s "users" </> i32 </> s "data" </> dt ) ]

// Browser.Dom.console.log (parse pageParser "/" Map.empty)
// Browser.Dom.console.log (parse pageParser "/data/2019-10-25" Map.empty)
// Browser.Dom.console.log (parse pageParser "/login" Map.empty)
// Browser.Dom.console.log (parse pageParser "/login" Map.empty)
// Browser.Dom.console.log (parse pageParser "/users" Map.empty)
// Browser.Dom.console.log (parse pageParser "/users/111" Map.empty)
// Browser.Dom.console.log (parse pageParser "/users/111/data/2019-10-26" Map.empty)