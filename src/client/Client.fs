module Client

open Elmish
open Elmish.React
open Elmish.Navigation
open Fable.FontAwesome
open Fable.FontAwesome.Free
open Fable.React
open Fable.React.Props
open Fetch.Types
open Thoth.Fetch
open Fulma
open Thoth.Json

open ServerProtocol.V1
open Router

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server

type Model = {
    currentPage: Page
    token: string
    data: string
    login: unit
}

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
    | LoginMsg of Login.Types.Msg
    | WhoResultReady of Result<User,string>

let whoAmI () =
    promise {
        try
            return! Fetch.tryFetchAs<User> "/api/v1/me"
        with e ->
            return (Error (string e))
    }

let urlUpdate (result: Option<Page>) model =
    match result with
    | None ->
        model, Navigation.modifyUrl (toPath model.currentPage)
    | Some page ->
        { model with currentPage = page }, Cmd.none

// defines the initial state and initial command (= side-effect) of the application
let init result : Model * Cmd<Msg> =
    let initialModel = { currentPage = Page.Home; token = ""; data = "loading"; login = () }
    let requestWhoCmd =
        Browser.Dom.console.log "sending who"
        Cmd.OfPromise.perform whoAmI () WhoResultReady
    initialModel, requestWhoCmd

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    Browser.Dom.console.log("got update", msg)
    match currentModel, msg with
    | _, WhoResultReady (Ok user) ->
        let nextModel = { currentModel with data = "I am a " + user.name }
        nextModel, Cmd.none
    | _, WhoResultReady (Error e) ->
        currentModel, toPath Login |> Navigation.newUrl
    | _ -> currentModel, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    let page =
        match model.currentPage with
        | Login -> 
            Login.view model.login (Msg.LoginMsg >> dispatch)
        | Home ->
            span [] [ str "Home "; strong [ ] [ str model.data ] ]
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
