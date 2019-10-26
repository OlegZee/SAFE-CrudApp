namespace EntryForm

open System
open ServerProtocol.V1

[<AutoOpen>]
module Types =
    type RecordState<'t> =
        | Unchanged of 't
        | Dirty of 't * 't
    type ModelState =
        | Init
        | Loading
        | Data of RecordState<UserData> list

    // FIXME consider new component
    type NewEntry = {
        time: string
        meal: string
        amount: string
    }

    type Model = {
        token: ServerComm.Token
        apiUrl: string
        date: DateTime
        data: ModelState
        newEntry: NewEntry
        newEntryValid: Result<CreateUserData,string>
        lastUpdate: Result<unit,string>
    }
    type Msg =
        | RefreshData
        | SaveChanges
        | SetNewTime of string
        | SetNewMeal of string
        | SetNewAmount of string
        | ValidateNewEntry
        | SaveNewEntry
        | ReceivedData of Result<UserData list,string>