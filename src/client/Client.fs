module Client

open Browser.Dom
open Elmish
open Elmish.React
open Elmish.Navigation
open Fable.FontAwesome
open Fable.FontAwesome.Free
open Fable.React
open Fable.Core
open Fable.React.Props
open Fetch.Types
open Thoth.Fetch
open Fulma
open Thoth.Json

open ServerProtocol.V1
open Router

type Model = {
    currentPage: Page
    token: string

    data: string
    login: Login.Types.Model
}

type Msg =
    | LoginMsg of Login.Types.Msg
    | WhoResultReady of Result<User,string>
    | LoginResultReady of Result<LoginResult,string>
    | ProcessLogin of Login.Types.ParentMsg

let whoAmI token =
    promise {
        let headers = [ ContentType "application/json"; Authorization ("Bearer " + token) ]
        try return! Fetch.tryFetchAs<User> ("/api/v1/me", [Fetch.requestHeaders headers ])
        with e -> return (Error (string e)) }

let loginServer (login, pwd) : JS.Promise<Result<LoginResult,string>> =
    promise {        
        let request = { login = login; pwd = pwd }
        try return! Fetch.tryPost("/api/login", request, isCamelCase = false)
        with e -> return (Error (string e)) }

let urlUpdate (result: Option<Page>) model =
    console.log ("urlupdate", result)
    match result with
    | None ->
        model, Navigation.modifyUrl (toPath model.currentPage)
    | Some page ->
        { model with currentPage = page }, Cmd.none

// defines the initial state and initial command (= side-effect) of the application
let init result : Model * Cmd<Msg> =
    let loginModel, _ = Login.State.init()
    // TODO retrieve token from store
    let initialModel = { currentPage = Page.Home; token = ""; data = "loading"; login = loginModel }
    let requestWhoCmd =
        console.log "sending who"
        Cmd.OfPromise.perform whoAmI initialModel.token WhoResultReady
    initialModel, requestWhoCmd

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    console.log("got update", msg)
    match msg with
    | LoginMsg loginMsg ->
        let nextModel, _ = Login.State.update loginMsg model.login
        { model with login = nextModel }, Cmd.none
    | ProcessLogin (Login.Types.ParentMsg.Login (x,y)) ->
        let loginCmd = Cmd.OfPromise.perform loginServer (x, y) LoginResultReady
        model, loginCmd
        
    | WhoResultReady (Ok user) ->
        let nextModel = { model with data = "I am a " + user.name }
        nextModel, toPath Page.Home |> Navigation.newUrl
    | WhoResultReady (Error e) ->
        model, toPath LoginScreen |> Navigation.newUrl

    | LoginResultReady (Ok data) ->
        let nextModel = { model with token = data.token; login = Login.State.init() |> fst }
        nextModel, Cmd.OfPromise.perform whoAmI data.token WhoResultReady

    | LoginResultReady (Error e) ->
        console.log("login failed", e) // TODO display error on login screen
        model, toPath LoginScreen |> Navigation.newUrl

    | _ -> model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    let page =
        match model.currentPage with
        | Page.LoginScreen -> 
            Login.view model.login (LoginMsg >> dispatch) (ProcessLogin >> dispatch)
        | Page.Home ->
            span [] [ str "Home "; strong [ ] [ str model.data ] ]
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
