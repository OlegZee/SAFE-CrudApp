namespace EntryForm

open CommonTypes
open ServerProtocol.V1

[<AutoOpen>]
module Types =
    type ModelState =
        | Init
        | Loading
        | Data of UserData list

    type NewEntry = {
        time: string
        meal: string
        amount: string
    }

    type Model = {
        token: Token
        apiUrl: string
        data: ModelState
        newEntry: NewEntry
        newEntryValid: Result<PostDataPayload,string>
    }
    type Msg =
        | RefreshData
        | SaveChanges
        | SetNewTime of string
        | SetNewMeal of string
        | SetNewAmount of string
        | ValidateNewEntry
        | SaveNewEntry
        | DeleteEntry of record_id: int
        | ReceivedData of Result<UserData list,string>