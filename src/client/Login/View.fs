module Login

open Fable.React
open Fable.React.Props
open Fulma

module Types =
    type Model = unit
    type Msg = unit

module State =
    ()

open Types

let view (model : Model) (dispatch : Msg -> unit) =
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
                              Input.Placeholder "Your Email"
                              Input.Props [ AutoFocus true ] ] ] ]
                  Field.div [ ]
                    [ Control.div [ ]
                        [ Input.password
                            [ Input.Size IsLarge
                              Input.Placeholder "Your Password" ] ] ]
                  Button.button
                    [ Button.Color IsInfo
                      Button.IsFullWidth
                      Button.CustomClass "is-large is-block" ]
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
    