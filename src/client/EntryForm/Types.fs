namespace EntryForm

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
    type Model = {
        token: string
        apiUrl: string
        date: System.DateTime
        data: ModelState
        lastUpdate: Result<unit,string>
    }
    type Msg =
        | RefreshData
        | SaveChanges
        | ReceivedData of Result<UserData list,string>