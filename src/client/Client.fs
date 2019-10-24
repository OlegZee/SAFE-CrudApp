module Client

open Browser.Dom
open Elmish
open Elmish.React
open Elmish.Navigation
open Fable.React
open Fable.Core
open Fetch.Types
open Thoth.Fetch
open Fulma

open ServerProtocol.V1
open Router

type ScreenModel =
    | OverviewMode of string

[<Erase>]
type Token = Token of string

type SessionInfo = { token: Token; userName: string; userRole: string; target: float }

type Model =
    | Initializing
    | LoggingIn of Login.Types.Model
    | Connected of SessionInfo * ScreenModel

type Msg =
    | LoginMsg of Login.Types.Msg
    | ProcessLogin of Login.Types.ParentMsg
    | LoggedIn of Result<Token*User,string>

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

let urlUpdate (page: Option<Page>) (model: Model) =
    console.log ("urlupdate", page)
    match model, page with
    | _, Some LoginScreen | _, Some LoginScreen ->
        let loginModel, cmd = Login.State.init()
        LoggingIn loginModel, Cmd.map LoginMsg cmd
    | Connected (session, _), Some Home ->
        // TODO translate page to screen model
        Connected (session, OverviewMode session.userName), Cmd.none
    | Connected (session, _), page ->
        // TODO translate page to screen model
        Connected (session, OverviewMode (page.ToString())), Cmd.none
    | _ ->
        // otherwise redirect to login
        Initializing, toPath LoginScreen |> Navigation.newUrl

// defines the initial state and initial command (= side-effect) of the application
let init result : Model * Cmd<Msg> =
    Initializing, toPath LoginScreen |> Navigation.newUrl

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    console.log("got update", msg)
    match model, msg with
    | LoggingIn model, LoginMsg msg ->
        let nextModel, cmd = Login.State.update msg model
        LoggingIn nextModel, Cmd.map LoginMsg cmd

    | LoggingIn _ as model, ProcessLogin (Login.Types.ParentMsg.Login (x,y)) ->
        model, Cmd.OfPromise.perform loginServer (x, y) LoggedIn
    | _, LoggedIn (Ok (token, user)) ->
        let session: SessionInfo = { token = token; userName = user.name; userRole = user.role; target = user.targetCalories}
        Connected (session, OverviewMode ""), toPath Home |> Navigation.newUrl
    | _, LoggedIn (Error e) ->
        model, toPath LoginScreen |> Navigation.newUrl

    | _ -> model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    let page =
        match model with
        | Initializing ->
            span [] [ str "initializing..." ]
        | LoggingIn model -> 
            Login.view model (LoginMsg >> dispatch) (ProcessLogin >> dispatch)
        | Connected (session, OverviewMode x) ->
            span [] [ str "User is logged in "; strong [ ] [ str session.userName ] ]
        | other ->
            span [] [ str "Other state "; strong [ ] [ str (sprintf "%A" other) ] ]
    Hero.hero
        [ Hero.Color IsSuccess
          Hero.IsFullHeight ]
        [ Hero.body [ ]
            [ Container.container
                [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                [ page ] ] ]
    
#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
|> Program.toNavigable (UrlParser.parseHash Router.pageParser) urlUpdate
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
