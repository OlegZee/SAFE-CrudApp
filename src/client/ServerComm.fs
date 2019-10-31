module ServerComm

open Fable.Core
open Fetch.Types
open Thoth.Fetch

open CommonTypes
open ServerProtocol.V1

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

let signup (request: SignupPayload) : JS.Promise<Result<Token*User,string>> =
    promise {        
        try
            let! loginResponse = Fetch.tryPost("/api/signup", request, isCamelCase = false)
            match loginResponse with
            | Ok _ ->    return! loginServer (request.login, request.pwd)
            | Error e -> return Error e
        with e -> return (Error (string e)) }

let retrieveSummary token : JS.Promise<Result<SummaryData list,string>> =
    promise {
        try return! Fetch.tryFetchAs<SummaryData list>("/api/v1/data", isCamelCase = false, properties = mkRestRequestProps token)
        with e -> return Error (string e) }

let retrieveDailyData (token, apiUrl) : JS.Promise<Result<UserData list,string>> =
    promise {
        try return! Fetch.tryFetchAs<UserData list>(apiUrl, isCamelCase = false, properties = mkRestRequestProps token)
        with e -> return Error (string e) }

let addNewEntry (token, apiUrl, data: PostDataPayload) : JS.Promise<Result<PostDataResponse,string>> =
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

let retrieveUsers (token) : JS.Promise<Result<User list,string>> =
    promise {
        try return! Fetch.tryFetchAs<User list>("/api/v1/users", isCamelCase = false, properties = mkRestRequestProps token)
        with e -> return Error (string e) }
        
let addNewUser (token, data: CreateUserPayload) : JS.Promise<Result<CreateUserResponse,string>> =
    promise {
        try return! Fetch.tryPost("/api/v1/users", data, isCamelCase = false, properties = mkRestRequestProps token)
        with e -> return Error (string e) }

let removeUser (token, record_id: int) : JS.Promise<Result<unit,string>> =
    promise {
        try let! result = Fetch.tryDelete(sprintf "/api/v1/users/%i" record_id, "", isCamelCase = false, properties = mkRestRequestProps token)
            Browser.Dom.console.log("remove result", result)
            return result |> Result.map ignore
        with e ->
            Browser.Dom.console.log("remove result error", e)
            return Error (string e) }

let retrieveUserSummary (token, userId) : JS.Promise<Result<User * SummaryData list,string>> =
    promise {
        try
            let! userInfo = Fetch.tryFetchAs<User>(sprintf "/api/v1/users/%i" userId, isCamelCase = false, properties = mkRestRequestProps token)
            let! summary = Fetch.tryFetchAs<SummaryData list>(sprintf "/api/v1/users/%i/data" userId, isCamelCase = false, properties = mkRestRequestProps token)
            return
                match userInfo, summary with
                | Ok user, Ok summary      -> Ok (user, summary)
                | Error e, _ | _, Error e  -> Error e
        with e -> return Error (string e) }

open System        
type QueryDataParams = {
    from: DateTime option
    to': DateTime option

    tfrom: TimeSpan option
    tto: TimeSpan option }
        
let queryData (token, query: QueryDataParams) : JS.Promise<Result<SummaryData list,string>> =
    let d v = Option.map (fun (x: DateTime) -> x.ToString("yyyy-MM-dd")) v
    let t v = Option.map (fun (x: TimeSpan) -> sprintf "%i:%i" x.Hours x.Minutes) v
    let queryStr =
        [
            "from", d query.from
            "to", d query.to'
            "tfrom", t query.tfrom
            "tto", t query.tto
        ] |> List.choose( fun (key,value) -> value |> function |Some v -> sprintf "%s=%s" key v |> Some |_ -> None)
        |> Array.ofList |> (fun items -> String.Join("&", items))
    promise {
        try return! Fetch.tryFetchAs<SummaryData list>("/api/v1/data?" + queryStr, isCamelCase = false, properties = mkRestRequestProps token)
        with e -> return Error (string e) }