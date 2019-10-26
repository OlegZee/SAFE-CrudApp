module ServerComm

open Fable.Core
open Fetch.Types
open Thoth.Fetch

open ServerProtocol.V1

[<Erase>]
type Token = Token of string

let mkRestRequestProps (Token token) =
    [Fetch.requestHeaders [ ContentType "application/json"; Authorization ("Bearer " + token) ] ]

let loginServer (login, pwd) : JS.Promise<Result<Token*User,string>> =
    promise {        
        let request = { login = login; pwd = pwd }
        try
            let! (loginResponse: Result<LoginResult,string>) = Fetch.tryPost("/api/login", request, isCamelCase = false)
            match loginResponse with
            | Ok { token = token } ->
                let! user = Fetch.tryFetchAs<User> ("/api/v1/me", mkRestRequestProps (Token token))
                return user |> Result.map(fun u -> (Token token, u))
            | Error e ->
                return Error e
        with e -> return (Error (string e)) }    

let retrieveSummary token : JS.Promise<Result<SummaryData list,string>> =
    promise {
        try return! Fetch.tryFetchAs<SummaryData list>("/api/v1/data", isCamelCase = false, properties = mkRestRequestProps token)
        with e -> return Error (string e) }

let retrieveDailyData (token, apiUrl) : JS.Promise<Result<UserData list,string>> =
    promise {
        try return! Fetch.tryFetchAs<UserData list>(apiUrl, isCamelCase = false, properties = mkRestRequestProps token)
        with e -> return Error (string e) }

let addNewEntry (token, apiUrl, data: CreateUserData) : JS.Promise<Result<UserCreated,string>> =
    promise {
        try return! Fetch.tryPost(apiUrl, data, isCamelCase = false, properties = mkRestRequestProps token)
        with e -> return Error (string e) }

let removeEntry (token, apiUrl, record_id: int) : JS.Promise<Result<unit,string>> =
    promise {
        try let! result = Fetch.tryDelete(sprintf "%s/%i" apiUrl record_id, "", isCamelCase = false, properties = mkRestRequestProps token)
            return result |> Result.map ignore
        with e -> return Error (string e) }

let saveSettings (token, settings: UserSettings) : JS.Promise<Result<unit,string>> =
    promise {
        try let! result = Fetch.tryPut("/api/v1/settings", settings, isCamelCase = false, properties = mkRestRequestProps token)
            return result |> Result.map ignore
        with e -> return Error (string e) }
        