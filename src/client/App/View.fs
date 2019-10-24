module App.View

open Fable.React
open Fulma

open App.Types

let view (Model (user, appview)) (dispatch : Msg -> unit) =
    
    let page =
        match appview with
        | OverviewMode x ->
            span [] [ str "User is logged in "; strong [ ] [ str user.userName ] ]
        | other ->
            span [] [ str "Other state "; strong [ ] [ str (sprintf "%A" other) ] ]

    Hero.hero
        [ Hero.Color IsSuccess
          Hero.IsFullHeight ]
        [ Hero.body [ ]
            [ Container.container
                [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                [ page ] ] ]
