module ServerComm

open Fable.Core
open Fetch.Types
open Thoth.Fetch

open ServerProtocol.V1

let mkRestRequestProps (token) =
    [Fetch.requestHeaders [ ContentType "application/json"; Authorization ("Bearer " + token) ] ]

let retrieveSummary token : JS.Promise<Result<SummaryData list,string>> =
    promise {
        try return! Fetch.tryFetchAs<SummaryData list>("/api/v1/data", isCamelCase = false, properties = mkRestRequestProps token)
        with e -> return Error (string e) }

let retrieveDailyData (token, apiUrl) : JS.Promise<Result<UserData list,string>> =
    promise {
        try return! Fetch.tryFetchAs<UserData list>(apiUrl, isCamelCase = false, properties = mkRestRequestProps token)
        with e -> return Error (string e) }
