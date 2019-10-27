module Login

open Elmish
open Fable.React
open Fable.React.Props
open Fable.Core.JsInterop
open Fulma

module Types =
    type Model = { LastError: string option }
    type ParentMsg = | Login of string * string

open Types

module State =
    let init err = { LastError = err }

let view (model : Model) (dispatchParent: ParentMsg -> unit) =
    let loginRef = createRef None
    let pwdRef = createRef None
    let loginBtnHandler _ =
        match loginRef.current, pwdRef.current with
        | Some inputLogin, Some inputPwd ->
            dispatchParent <| Login (inputLogin?value, inputPwd?value)
        | _ ->
            ()

    Column.column
        [ Column.Width (Screen.All, Column.Is4)
          Column.Offset (Screen.All, Column.Is4) ]
        [ Heading.h3
            [ Heading.Modifiers [ Modifier.TextColor IsGrey ] ]
            [ str "Please login to proceed." ]
          Box.box' [ ]
            [ figure [ Class "avatar" ]
                [ img [ Src "https://placehold.it/128x128" ] ]
              form [ ]
                [ yield Field.div [ ]
                    [ Control.div [ ]
                        [ Input.text
                            [ Input.Size IsLarge
                              Input.Placeholder "Your Login"
                              Input.Props [ AutoFocus true; Required true; RefValue loginRef ] ] ] ]
                  yield Field.div [ ]
                    [ Control.div [ ]
                        [ Input.password
                            [ Input.Size IsLarge
                              Input.Placeholder "Your Password"
                              Input.Props [ Required true; RefValue pwdRef] ] ] ]
                  if Option.isSome model.LastError then
                    yield Notification.notification [ Notification.Color IsDanger ] [ str "Login failed. Check user/password." ]
                                    
                  yield Button.button
                    [ Button.Color IsInfo
                      Button.IsFullWidth
                      Button.CustomClass "is-large is-block"
                      Button.OnClick loginBtnHandler ]
                    [ str "Login" ] ] ]
          Text.p [ Modifiers [ Modifier.TextColor IsGrey ] ]
            [ a [ ] [ str "Sign Up" ]
              str "\u00A0·\u00A0"
              a [ ] [ str "Forgot Password" ] ]
          br [ ]
          Text.div [ Modifiers [   Modifier.TextColor IsGrey ] ]
            [ span [ ]
                [ str "Version "
                  strong [ ] [ str Version.app ]
                ]    
             ] ]
    