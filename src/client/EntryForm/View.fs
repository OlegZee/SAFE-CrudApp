module EntryForm.View

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open ServerProtocol.V1

let recordEntry =
    function
    | Unchanged (r: UserData)
    | Dirty (r,_) ->
        tr  [ ]
            [ td [ ] [ str r.rtime ]
              td [ ] [ str r.meal ]
              td [ ] [ str <| sprintf "%f" r.amount ] ]

let view (model: Model) dispatch =
    match model.data with
    | ModelState.Data entries ->
        Table.table [ Table.IsBordered
                      Table.IsNarrow
                      Table.IsStriped ]
            [ thead [ ]
                [ tr [ ]
                     [ th [ ] [ str "Time" ]
                       th [ ] [ str "Meal" ]
                       th [ ] [ str "Amount" ] ] ]
              tbody [ ]
                (entries |> List.map recordEntry) ]
    | _ ->
        div [] [ str "Loading data"]
