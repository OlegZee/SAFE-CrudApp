module Login

open Elmish
open Fable.React
open Fable.React.Props
open Fable.Core.JsInterop
open Fulma

module Types =
    type Model = { Login: string; Pwd: string }
    type Msg = | SetLogin of string | SetPwd of string
    type ParentMsg = | Login of string * string

module State =
    open Types
    let init () = { Login = ""; Pwd = ""}, Cmd.none
    let update msg model =
        match msg with
        | SetLogin login -> { model with Login = login }, Cmd.none
        | SetPwd passwrd -> { model with Pwd = passwrd }, Cmd.none

open Types

let view (model : Model) (dispatch : Msg -> unit) (dispatchParent: ParentMsg -> unit) =
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
                [ Field.div [ ]
                    [ Control.div [ ]
                        [ Input.text
                            [ Input.Size IsLarge
                              Input.Placeholder "Your Login"
                              Input.DefaultValue model.Login
                              Input.OnChange (fun ev -> !!ev.target?value |> (SetLogin >> dispatch))
                              // Input.Props [ OnChange (fun event -> !!event.target?value |> (SetLogin >> dispatch)) ]
                              Input.Props [ AutoFocus true ] ] ] ]
                  Field.div [ ]
                    [ Control.div [ ]
                        [ Input.password
                            [ Input.Size IsLarge
                              Input.Placeholder "Your Password"
                              Input.OnChange (fun ev -> !!ev.target?value |> (SetPwd >> dispatch))
                              Input.DefaultValue model.Pwd ] ] ]
                  Button.button
                    [ Button.Color IsInfo
                      Button.IsFullWidth
                      Button.CustomClass "is-large is-block"
                      Button.OnClick (fun _ -> dispatchParent <| Login (model.Login, model.Pwd)) ]
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
    