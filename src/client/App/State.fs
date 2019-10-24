module App.State

open Elmish
open Browser.Dom
open Fable.Core
open Fetch.Types
open Thoth.Fetch

open Types
open ServerProtocol.V1

module ServerComm =
    let mkRestRequestProps (token) =
        [Fetch.requestHeaders [ ContentType "application/json"; Authorization ("Bearer " + token) ] ]

    let retrieveMyData token : JS.Promise<Result<SummaryData list,string>> =
        promise {
            try return! Fetch.tryFetchAs<SummaryData list>("/api/v1/data", isCamelCase = false, properties = mkRestRequestProps token)
            with e -> return Error (string e) }

let init (user: User, token: string) =
    let userInfo: UserInfo = { token = token; userName = user.name; userRole = user.role; target = user.targetCalories }
    Model (userInfo, NoView), Cmd.none

let update (msg: Msg) (Model (user, appView)) =
    console.log("app msg", msg)
    match msg with
    | RefreshUserData ->
        let retrieveSummaryCmd = Cmd.OfPromise.perform ServerComm.retrieveMyData user.token ReceivedUserSummary
        Model (user, NoView), retrieveSummaryCmd

    | ReceivedUserSummary (Ok summary) ->
        Model (user, SummaryData summary), Cmd.none
    | ReceivedUserSummary (Error e) ->
        Model (user, OverviewMode e), Cmd.none

let urlUpdate (page: Option<Router.Page>) (Model (user, appView) as model) =
    console.log("app url updata", page)
    match page with
    | Some Router.Home ->
        let retrieveSummaryCmd = Cmd.OfPromise.perform ServerComm.retrieveMyData user.token ReceivedUserSummary
        Model (user, NoView), retrieveSummaryCmd
    | Some (Router.DailyView d) ->
        Model (user, DayView d), Cmd.none
    | Some page ->
        Model (user, OverviewMode (page.ToString())), Cmd.none
    
    | _ -> model, Cmd.none
