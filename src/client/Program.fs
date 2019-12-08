module Client

open Browser.Dom
open Elmish
open Elmish.React
open Elmish.Navigation
open Fable.React
open Fulma

open Fable.React.Props

open ServerProtocol.V1
open ServerComm
open Router

type Model =
    | Initializing
    | LoggingIn of Login.Types.Model
    | Signup of Signup.Types.Model
    | Connected of App.Types.Model

type Msg =
    | AppMsg of App.Types.Msg
    | ProcessLogin of Login.Types.ParentMsg
    | ProcessSignup of Signup.Types.ParentMsg
    | LoggedIn of Result<User,string>
    | DisplayLoginScreen
    | SignOut
    | StartInitializing

let urlUpdate (page: Option<Page>) (model: Model) =
    console.log("urlUpdate", page)
    match model, page with
    | _, Some LoginScreen ->
        LoggingIn (Login.State.init None), Cmd.none

    | _, Some SignupScreen ->
        Signup (Signup.State.init None), Cmd.none

    | Connected appModel, _ ->
        let appState, cmd = App.State.urlUpdate page appModel
        Connected (appState), Cmd.map AppMsg cmd

    | _ -> model, Cmd.none

// defines the initial state and initial command (= side-effect) of the application
let init _ : Model * Cmd<Msg> =
    console.log("Program.init", document.location.toString())
    Initializing, Cmd.ofMsg StartInitializing

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    console.log("Program.update", msg)
    match model, msg with

    | _, StartInitializing ->
        Initializing, Cmd.OfPromise.perform checkConnection () (function
            |Error _ -> DisplayLoginScreen
            |user -> LoggedIn user)

    | LoggingIn _ as model, ProcessLogin (Login.Types.ParentMsg.Login (x,y)) ->
        model, Cmd.OfPromise.perform loginServer (x, y) LoggedIn

    | Signup _ as model, ProcessSignup (Signup.Types.ParentMsg.Signup req) ->
        model, Cmd.OfPromise.perform signup req LoggedIn

    | _, LoggedIn (Ok user) ->
        let appModel, _ = App.State.init (user)
        console.log("Logged in", document.location.toString())
        Connected appModel,
        Cmd.batch [
                toPath Home |> Navigation.newUrl // FIXME workaround, urlUpdate is not invoked after login
                document.location.toString() |> Navigation.newUrl   // let urlUpdate to navigate to correct page
        ]

    | _, LoggedIn (Error e) ->
        LoggingIn (Login.State.init <| Some e), Cmd.none
    | _, DisplayLoginScreen ->
        LoggingIn (Login.State.init None), Cmd.none

    | Connected appModel, AppMsg msg ->
        let nextModel, cmd = App.State.update msg appModel
        Connected nextModel, Cmd.map AppMsg cmd

    | model, SignOut ->
        model,
            Cmd.batch [
                Cmd.OfPromise.perform signOut () (fun _ -> StartInitializing)
                toPath Home |> Navigation.newUrl
            ]

    | _ ->
        console.log("unhandled message", msg)
        model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =

    let topNav dispatch =
        let homePath = Router.toPath Router.Home
        Navbar.navbar [ Navbar.HasShadow ]
            [ Container.container []
                [ Navbar.Brand.div []
                    [ Navbar.Item.a [ Navbar.Item.Props [ Href homePath] ]
                        [ img [ Src "http://bulma.io/images/bulma-logo.png"
                                Alt "Bulma: a modern CSS framework based on Flexbox" ] ]
                      Navbar.burger []
                        [ span [ ] [ ]
                          span [ ] [ ]
                          span [ ] [ ] ] ]
                  
                  Navbar.End.div []
                    [ Navbar.Item.div [ Navbar.Item.HasDropdown; Navbar.Item.IsHoverable ]
                        [ Navbar.Link.a [] [ str "Account" ]
                          Navbar.Dropdown.div []
                            [ Navbar.Item.a [ Navbar.Item.Props [ Href homePath] ] [ str "Home" ]
                              Navbar.divider [] []
                              Navbar.Item.a [ Navbar.Item.Props [ OnClick (fun _ -> dispatch SignOut) ]] [ str "Logout" ]
                              ] ] ] ] ]
                              
    let wrapPage page =
        Hero.hero
            [ Hero.Color IsSuccess
              Hero.IsFullHeight ]
            [ Hero.body [ ]
                [ Container.container
                    [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ page ] ] ]

    match model with
    | Initializing ->         span [] [ str "initializing..." ] |> wrapPage
    | LoggingIn model ->      Login.view model (ProcessLogin >> dispatch) |> wrapPage
    | Signup model ->         Signup.view model (ProcessSignup >> dispatch) |> wrapPage
    | Connected appModel ->
        div [] [
            topNav dispatch
            App.View.view appModel (AppMsg >> dispatch)
        ]

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
