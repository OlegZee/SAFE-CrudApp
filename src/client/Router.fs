module Router

open Elmish.UrlParser

type Page =
    | Home
    | Login
    | ManageUsers
    | UserData of int

let toPath = function
    | Home -> "/"
    | Login -> "#/login"
    | ManageUsers -> "#/users"
    | UserData userId -> sprintf "#/users/%i" userId

let pageParser : Parser<Page->_,Page> =
    oneOf [
        map Home (s "overview")
        map Login (s "login")
        map Home (s "overview")
        map UserData (s "users" </> i32 ) ]
