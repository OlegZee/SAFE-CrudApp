module Signup

open Elmish
open Browser.Types
open Fable.React
open Fable.React.Props
open Fable.Core.JsInterop
open Fulma

open ServerProtocol.V1

module Types =
    type Model = { LastError: string option }
    type ParentMsg = | Signup of SignupPayload

open Types

module State =
    let init err = { LastError = err }

let view (model : Model) (dispatchParent: ParentMsg -> unit) =
    let loginRef, nameRef = (createRef None, createRef None)
    let pwdRef, repeatPwdRef = createRef None, createRef None

    let submitHandler (e: Event) =
        Browser.Dom.console.log ("handling form submit")

        match loginRef.current, pwdRef.current, nameRef.current, repeatPwdRef.current with
        | Some inputLogin, Some inputPwd, Some name, Some repeatPwd ->
            if inputPwd?value = repeatPwd?value then
                ParentMsg.Signup { login = inputLogin?value; pwd = inputPwd?value; name = name?value } |> dispatchParent
            else
                Browser.Dom.window.alert ("Passwords do not match")
        | _ ->
            ()

    let label text = Label.label [ Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left)] ] [ str text ]

    Column.column
        [ Column.Width (Screen.All, Column.Is4)
          Column.Offset (Screen.All, Column.Is4) ]
        [ Heading.h3
            [ Heading.Modifiers [ Modifier.TextColor IsGrey ] ]
            [ str "Please fill the form to signup." ]
          Box.box' [ ]
            [ form [ OnSubmit submitHandler ]
                [ yield Field.div [ ]
                    [ label "Login"
                      Control.div [ ]
                        [ Input.text
                            [ Input.Size IsLarge; Input.Placeholder "Login"
                              Input.Props [ AutoFocus true; Required true; RefValue loginRef ] ] ] ]
                  yield Field.div [ ]
                    [ label "Name"
                      Control.div [ ]
                        [ Input.text
                            [ Input.Size IsLarge; Input.Placeholder "Enter your name"
                              Input.Props [ Required true; RefValue nameRef ] ] ] ]
                  yield Field.div [ ]
                    [ label "Password"
                      Control.div [ ]
                        [ Input.password
                            [ Input.Size IsLarge; Input.Placeholder "Password"
                              Input.Props [ Required true; RefValue pwdRef] ] ] ]
                  yield Field.div [ ]
                    [ label "Repeat password"
                      Control.div [ ]
                        [ Input.password
                            [ Input.Size IsLarge; Input.Placeholder "Repeat Password"
                              Input.Props [ Required true; RefValue repeatPwdRef] ] ] ]
                  if Option.isSome model.LastError then
                    yield Notification.notification [ Notification.Color IsDanger ] [ str "Signup failed. Name is already taken." ]
                               
                  yield Button.Input.submit
                    [ Button.Color IsInfo
                      Button.IsFullWidth
                      Button.CustomClass "is-large is-block" ]
                ] ]
          Text.p [ Modifiers [ Modifier.TextColor IsGrey ] ]
            [ a [ Href <| Router.toPath Router.LoginScreen ] [ str "Login" ]
              str "\u00A0·\u00A0"
              a [ ] [ str "Forgot Password" ] ]
          br [ ]
          Text.div [ Modifiers [   Modifier.TextColor IsGrey ] ]
            [ span [ ]
                [ str "Version "
                  strong [ ] [ str Version.app ]
                ]    
             ] ]
    