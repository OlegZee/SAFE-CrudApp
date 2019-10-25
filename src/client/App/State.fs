module App.State

open Elmish
open Browser.Dom

open Types
open ServerProtocol.V1

let init (user: User, token: string) =
    let userInfo: UserInfo = {
        token = token
        userName = user.name; userRole = user.role; target = user.targetCalories }
    Model (userInfo, NoView), Cmd.none

let update (msg: Msg) (Model (user, appView) as model) =
    console.log("app msg", msg)
    match msg with
    | RefreshUserData ->
        let retrieveSummaryCmd = Cmd.OfPromise.perform ServerComm.retrieveSummary user.token ReceivedUserSummary
        Model (user, NoView), retrieveSummaryCmd

    | ReceivedUserSummary (Ok summary) ->
        Model (user, SummaryData summary), Cmd.none
    | ReceivedUserSummary (Error e) ->
        Model (user, OverviewMode e), Cmd.none

    | DayViewMsg msg ->
        match appView with
        | DayView viewModel ->
            let nextModel, cmd = EntryForm.State.update msg viewModel
            Model (user, DayView nextModel), Cmd.map DayViewMsg cmd
        | _ ->
            console.warn("the message is unexpected in this model state", msg)
            model, Cmd.none

let urlUpdate (page: Option<Router.Page>) (Model (user, appView) as model) =
    console.log("app url updata", page)
    match page with
    | Some Router.Home ->
        let retrieveSummaryCmd = Cmd.OfPromise.perform ServerComm.retrieveSummary user.token ReceivedUserSummary
        Model (user, NoView), retrieveSummaryCmd
    | Some (Router.DailyView d) ->
        let apiUrl = "/api/v1/data/" + d.ToString("yyyy-MM-dd")
        let dayViewModel, cmd = EntryForm.State.init (apiUrl, d, user.token)
        Model (user, DayView dayViewModel), Cmd.map DayViewMsg cmd
    | Some page ->
        Model (user, OverviewMode (page.ToString())), Cmd.none
    
    | _ -> model, Cmd.none
