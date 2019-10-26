module App.State

open Elmish
open Browser.Dom

open Types
open ServerProtocol.V1
open ServerComm

let init (user: User, token: string) =
    let userInfo: UserInfo = {
        token = Token token
        userName = user.name; userRole = user.role; target = user.targetCalories }
    Model (userInfo, NoView), Cmd.none

let initSummaryModel user data : SummaryViewModel =
    { user = user; data = data; editedTarget = None}
    
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
        Model (user, NoView), Cmd.OfPromise.perform retrieveSummary user.token ReceivedUserSummary

    | ReceivedUserSummary (Ok summary), Model (user, _) ->
        Model (user, SummaryData (initSummaryModel user summary)), Cmd.none

    | ReceivedUserSummary (Error e), Model (user, _) ->
        Model (user, ErrorView e), Cmd.none

    | DayViewMsg msg, Model (user, DayView viewModel) ->
        let nextModel, cmd = EntryForm.State.update msg viewModel
        Model (user, DayView nextModel), Cmd.map DayViewMsg cmd

    | SummaryViewMsg (SavedTargetValue value), Model (user, SummaryData viewModel) ->
        // FIXME
        let newUser = { user with target = value }
        Model (newUser, SummaryData { viewModel with user = newUser }), Cmd.none

    | SummaryViewMsg msg, Model (user, SummaryData viewModel) ->
        let nextModel, cmd = updateSummaryViewModel msg viewModel
        Model (user, SummaryData nextModel), Cmd.map SummaryViewMsg cmd
    | _ ->
        console.warn("the message is unexpected in this model state", msg, model)
        model, Cmd.none


let urlUpdate (page: Option<Router.Page>) (Model (user, appView) as model) =
    //  console.log("app url update", page)
    match page with
    | Some Router.Home ->
        let retrieveSummaryCmd = Cmd.OfPromise.perform ServerComm.retrieveSummary user.token ReceivedUserSummary
        Model (user, NoView), retrieveSummaryCmd
    | Some (Router.DailyView d) ->
        let apiUrl = "/api/v1/data/" + d.ToString("yyyy-MM-dd")
        let dayViewModel, cmd = EntryForm.State.init (apiUrl, d, user.token)
        Model (user, DayView dayViewModel), Cmd.map DayViewMsg cmd
    | Some page ->
        Model (user, ErrorView (page.ToString())), Cmd.none
    
    | _ -> model, Cmd.none
