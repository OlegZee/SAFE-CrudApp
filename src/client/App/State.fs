module App.State

open Elmish
open Browser.Dom

open Types
open ServerProtocol.V1
open ServerComm

let mapUser (user: User, Token token): UserInfo =
    { token = Token token
      userName = user.name; userRole = user.role; target = user.targetCalories }

let init (user: User, token: string) =
    let userInfo = mapUser (user, Token token)
    Model (userInfo, NoView), Cmd.none

let initSummaryModel user data : SummaryViewModel =
    { user = user; data = data; editedTarget = None; otherUser = None }
    
let updateSummaryViewModel (msg: SummaryViewMsg) (model: SummaryViewModel) =
    match msg with
    | EditTarget -> { model with editedTarget = model.user.target |> Some } , Cmd.none
    | CancelEdit -> { model with editedTarget = None }, Cmd.none
    | SavedTargetValue _ -> model, Cmd.ofMsg CancelEdit
    | SaveValue newValue ->
        match System.Double.TryParse newValue with
        | true, value when value >= 0. && value < 10000. ->
             { model with editedTarget = None }, Cmd.OfPromise.perform saveSettings (model.user.token, { targetCalories = value })
                (function |Ok () -> SavedTargetValue value |Error e -> EditTarget)  // FIXME improve parent notification, validation/error
        | _ ->
            model, Cmd.none

let update (msg: Msg) (model: Model) =
    // console.log("app msg", msg)
    match msg, model with
    | RefreshUserData, Model (user, _) ->
        Model (user, NoView),
        Cmd.OfPromise.perform retrieveSummary user.token (function
            | Ok data -> DisplayMySummary data
            | Error e -> DisplayError e)

    | DisplayError e, Model (user, _) ->
        Model (user, ErrorView e), Cmd.none

    | DisplayMySummary summary, Model (user, _) ->
        Model (user, SummaryData (initSummaryModel user summary)), Cmd.none

    | DisplayUserSummary (otherUserId, otherUser, summary), Model (user, _) ->
        let summaryModel = { initSummaryModel otherUser summary with otherUser = Some otherUserId }
        Model (user, UserSummaryData (otherUser, summaryModel)), Cmd.none

    | DayViewMsg msg, Model (user, DayView (date, otherUser, viewModel)) ->
        let nextModel, cmd = EntryForm.State.update msg viewModel
        Model (user, DayView (date, otherUser, nextModel)), Cmd.map DayViewMsg cmd

    | SummaryViewMsg (SavedTargetValue value), Model (user, SummaryData viewModel) ->
        // FIXME trick to update app data
        let newUser = { user with target = value }
        Model (newUser, SummaryData { viewModel with user = newUser }), Cmd.none

    | SummaryViewMsg msg, Model (user, SummaryData viewModel) ->
        let nextModel, cmd = updateSummaryViewModel msg viewModel
        Model (user, SummaryData nextModel), Cmd.map SummaryViewMsg cmd

    | ManageUsersMsg msg, Model (user, ManageUsers usersModel) ->
        let nextModel, cmd = ManageUsers.State.update msg usersModel
        Model (user, ManageUsers nextModel), Cmd.map ManageUsersMsg cmd

    | _ ->
        console.warn("the message is unexpected in this model state", msg, model)
        model, Cmd.none


let urlUpdate (page: Option<Router.Page>) (Model (user, appView) as model) =
    //  console.log("app url update", page)
    match page with
    | Some Router.Home ->
        Model (user, NoView), Cmd.ofMsg RefreshUserData

    | Some (Router.DailyView d) ->
        let apiUrl = "/api/v1/data/" + d.ToString("yyyy-MM-dd")
        let dayViewModel, cmd = EntryForm.State.init (apiUrl, user.token)
        Model (user, DayView (d, user, dayViewModel)), Cmd.map DayViewMsg cmd

    | Some Router.ManageUsers ->
        let usersModel, cmd = ManageUsers.State.init ("", user.token)
        Model (user, ManageUsers usersModel), Cmd.map ManageUsersMsg cmd

    | Some (Router.UserOverview userId) ->
        Model (user, NoView),
            Cmd.OfPromise.perform retrieveUserSummary (user.token, userId) (function
            | Ok (otherUser,data) ->
                let userInfo = mapUser (otherUser, user.token)
                DisplayUserSummary (otherUser.user_id, userInfo, data) 
            | Error e -> DisplayError e )

    | Some (Router.UserDailyView (userId, date)) ->
        let apiUrl = sprintf "/api/v1/users/%i/data/%s" userId (date.ToString("yyyy-MM-dd"))
        let dayViewModel, cmd = EntryForm.State.init (apiUrl, user.token)
        // HACK to get the user name
        let otherUser = appView |> function
            | UserSummaryData (otherUser, _)
            | DayView (_, otherUser, _) -> otherUser
            | _ -> user
        Model (user, DayView (date, otherUser, dayViewModel)), Cmd.map DayViewMsg cmd

    | Some page ->
        Model (user, ErrorView (page.ToString())), Cmd.none
    
    | _ -> model, Cmd.none
