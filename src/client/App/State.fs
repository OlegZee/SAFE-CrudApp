module App.State

open Elmish
open Browser.Dom

open Types
open CommonTypes
open ServerProtocol.V1
open ServerComm

let mapUser (user: User): UserInfo =
    { userName = user.name; userRole = user.role; target = user.expenseLimit }

let init (user: User) =
    Model (mapUser user, NoView), Cmd.ofMsg RefreshUserData

let update (msg: Msg) (model: Model) =
    // console.log("app msg", msg)
    match msg, model with
    | RefreshUserData, Model (user, _) ->
        Model (user, NoView),
        Cmd.OfPromise.perform retrieveSummary () (function
            | Ok data -> DisplayMySummary data
            | Error e -> DisplayError e)

    | DisplayError e, Model (user, _) ->
        Model (user, ErrorView e), Cmd.none

    | DisplayMySummary summary, Model (user, _) ->
        Model (user, SummaryData (SummaryView.State.init user summary)), Cmd.none

    | DisplayUserSummary (otherUserId, otherUser, summary), Model (user, _) ->
        let summaryModel = { SummaryView.State.init otherUser summary with otherUser = Some otherUserId }
        Model (user, UserSummaryData (otherUser, summaryModel)), Cmd.none

    | DayViewMsg msg, Model (user, DayView (date, otherUser, viewModel)) ->
        let nextModel, cmd = EntryForm.State.update msg viewModel
        Model (user, DayView (date, otherUser, nextModel)), Cmd.map DayViewMsg cmd

    | SummaryViewMsg (SummaryView.Types.SavedTargetValue value), Model (user, SummaryData viewModel) ->
        // FIXME trick to update app data
        let newUser = { user with target = value }
        Model (newUser, SummaryData { viewModel with user = newUser }), Cmd.none

    | SummaryViewMsg msg, Model (user, SummaryData viewModel) ->
        let nextModel, cmd = SummaryView.State.update msg viewModel
        Model (user, SummaryData nextModel), Cmd.map SummaryViewMsg cmd

    | ManageUsersMsg msg, Model (user, ManageUsers usersModel) ->
        let nextModel, cmd = ManageUsers.State.update msg usersModel
        Model (user, ManageUsers nextModel), Cmd.map ManageUsersMsg cmd

    | ReportViewMsg msg, Model (user, ReportView reportModel) ->
        let nextModel, cmd = ReportsForm.State.update msg reportModel
        Model (user, ReportView nextModel), Cmd.map ReportViewMsg cmd

    | _ ->
        console.warn("the message is unexpected in this model state", msg, model)
        model, Cmd.none


let urlUpdate (page: Option<Router.Page>) (Model (user, appView) as model) =
    console.log("app url update", page)
    match page with
    | None
    | Some Router.Home ->
        Model (user, NoView), Cmd.ofMsg RefreshUserData

    | Some (Router.DailyView d) ->
        let apiUrl = "/api/v1/data/" + d.ToString("yyyy-MM-dd")
        let dayViewModel, cmd = EntryForm.State.init apiUrl
        Model (user, DayView (d, user, dayViewModel)), Cmd.map DayViewMsg cmd

    | Some Router.ManageUsers ->
        let usersModel, cmd = ManageUsers.State.init ()
        Model (user, ManageUsers usersModel), Cmd.map ManageUsersMsg cmd

    | Some (Router.UserOverview userId) ->
        Model (user, NoView),
            Cmd.OfPromise.perform retrieveUserSummary userId (function
            | Ok (otherUser,data) ->
                DisplayUserSummary (UserId otherUser.user_id, mapUser otherUser, data) 
            | Error e -> DisplayError e )

    | Some (Router.UserDailyView (UserId userId, date)) ->
        let apiUrl = sprintf "/api/v1/users/%i/data/%s" userId (date.ToString("yyyy-MM-dd"))
        let dayViewModel, cmd = EntryForm.State.init apiUrl
        // HACK to get the user name
        let otherUser = appView |> function
            | UserSummaryData (otherUser, _)
            | DayView (_, otherUser, _) -> otherUser
            | _ -> user
        Model (user, DayView (date, otherUser, dayViewModel)), Cmd.map DayViewMsg cmd

    | Some Router.Report ->
        let reportModel, cmd = ReportsForm.State.init ()
        Model (user, ReportView reportModel), Cmd.map ReportViewMsg cmd


    | Some page ->
        Model (user, ErrorView (page.ToString())), Cmd.none
