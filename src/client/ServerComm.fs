module ServerComm

open Fable.Core
open Thoth.Fetch

open CommonTypes
open ServerProtocol.V1

let checkConnection () =
    Fetch.tryGet<unit,User> "/api/v1/me"

let signOut () =
    Fetch.post<_,unit> ("/api/signout", "")

let loginServer (login, pwd) =
    promise {
        let request = { login = login; pwd = pwd }
        match! Fetch.tryPost("/api/login", request, isCamelCase = false) with
        | Ok _ -> return! Fetch.tryGet<unit,User> "/api/v1/me"
        | Error e -> return Error e
    }

let signup (request: SignupPayload) =
    promise {        
        match! Fetch.tryPost("/api/signup", request, isCamelCase = false) with
        | Ok _ ->    return! loginServer (request.login, request.pwd)
        | Error e -> return Error e
    }

let retrieveSummary () =
    Fetch.tryGet<unit, SummaryData list>("/api/v1/data", isCamelCase = false)

let retrieveDailyData (apiUrl) =
    Fetch.tryGet<_,UserData list>(apiUrl, isCamelCase = false)

let addNewEntry (apiUrl, data: PostDataPayload) =
    Fetch.tryPost<_,PostDataResponse>(apiUrl, data, isCamelCase = false)

let updateEntry (apiUrl, EntryId recordId, data) =
    Fetch.tryPut<PostDataPayload,unit>(sprintf "%s/%i" apiUrl recordId, data, isCamelCase = false)

let removeEntry (apiUrl, EntryId recordId) : JS.Promise<Result<unit,FetchError>> =
    Fetch.tryDelete<unit,unit>(sprintf "%s/%i" apiUrl recordId, (), isCamelCase = false)

let saveSettings settings =
    Fetch.tryPut<UserSettings,unit>("/api/v1/settings", settings, isCamelCase = false)

let retrieveUsers () =
    Fetch.tryGet<unit,User list>("/api/v1/users", isCamelCase = false)

let addNewUser data =
    Fetch.tryPost<CreateUserPayload,CreateUserResponse>("/api/v1/users", data, isCamelCase = false)

let updateUser (UserId userId, data) =
    Fetch.tryPut<UpdateUserPayload, unit>(sprintf "/api/v1/users/%i" userId, data, isCamelCase = false)

let removeUser (UserId userId) =
    Fetch.tryDelete<unit,unit>(sprintf "/api/v1/users/%i" userId, (), isCamelCase = false)

let retrieveUserSummary (UserId userId) =
    promise {
        let! userInfo = Fetch.tryGet<unit,User>(sprintf "/api/v1/users/%i" userId, isCamelCase = false)
        let! summary = Fetch.tryGet<unit,SummaryData list>(sprintf "/api/v1/users/%i/data" userId, isCamelCase = false)
        return
            match userInfo, summary with
            | Ok user, Ok summary      -> Ok (user, summary)
            | Error e, _ | _, Error e  -> Error (string e)
    }

open System        
type QueryDataParams = {
    from: DateTime option
    to': DateTime option

    tfrom: TimeSpan option
    tto: TimeSpan option }
        
let queryData (query: QueryDataParams) : JS.Promise<Result<SummaryData list,string>> =
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

    Fetch.get("/api/v1/data?" + queryStr, isCamelCase = false)
