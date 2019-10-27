module Client

open Browser.Dom
open Elmish
open Elmish.React
open Elmish.Navigation
open Fable.React
open Fulma

open ServerProtocol.V1
open ServerComm
open Router

type Model =
    | Initializing
    | LoggingIn of Login.Types.Model
    | Connected of Token * App.Types.Model

type Msg =
    | LoginMsg of Login.Types.Msg
    | AppMsg of App.Types.Msg
    | ProcessLogin of Login.Types.ParentMsg
    | LoggedIn of Result<Token*User,string>

let urlUpdate (page: Option<Page>) (model: Model) =
    match model, page with
    | _, Some LoginScreen | _, Some LoginScreen ->
        let loginModel, cmd = Login.State.init()
        LoggingIn loginModel, Cmd.map LoginMsg cmd
    | Connected (session, appModel), _ ->
        let appState, cmd = App.State.urlUpdate page appModel
        Connected (session, appState), Cmd.map AppMsg cmd
    | _ ->
        // otherwise redirect to login
        Initializing, toPath LoginScreen |> Navigation.newUrl

// defines the initial state and initial command (= side-effect) of the application
let init result : Model * Cmd<Msg> =
    Initializing, toPath LoginScreen |> Navigation.newUrl

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    // console.log("got update", msg)
    match model, msg with
    | LoggingIn model, LoginMsg msg ->
        let nextModel, cmd = Login.State.update msg model
        LoggingIn nextModel, Cmd.map LoginMsg cmd

    | LoggingIn _ as model, ProcessLogin (Login.Types.ParentMsg.Login (x,y)) ->
        model, Cmd.OfPromise.perform loginServer (x, y) LoggedIn
    | _, LoggedIn (Ok (Token token, user)) ->
        let appModel, cmd = App.State.init (user, token)
        Connected (Token token, appModel),
            Cmd.batch [
                toPath Home |> Navigation.newUrl
                cmd |> Cmd.map AppMsg ]
    | _, LoggedIn (Error e) ->
        model, toPath LoginScreen |> Navigation.newUrl

    | Connected (session, appModel), AppMsg msg ->
        let nextModel, cmd = App.State.update msg appModel
        Connected (session, nextModel), Cmd.map AppMsg cmd

    | _ ->
        console.log("unhandled message", msg)
        model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =

    let wrapPage page =
        Hero.hero
            [ Hero.Color IsSuccess
              Hero.IsFullHeight ]
            [ Hero.body [ ]
                [ Container.container
                    [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ page ] ] ]

    match model with
    | Initializing ->
        span [] [ str "initializing..." ] |> wrapPage
    | LoggingIn model -> 
        Login.view model (LoginMsg >> dispatch) (ProcessLogin >> dispatch) |> wrapPage
    | Connected (_, appModel) ->
        App.View.view appModel (AppMsg >> dispatch)
    | other ->
        span [] [ str "Other state "; strong [ ] [ str (sprintf "%A" other) ] ] |> wrapPage

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
