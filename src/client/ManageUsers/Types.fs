module ManageUsers.Types

open CommonTypes

module TabularFormTypes =
    type ModelState<'trec> =
        | Init
        | Loading
        | Data of 'trec list
    type Model<'trec,'tnewrec> = {
        token: Token
        apiUrl: string
        data: 'trec ModelState
        newEntry: Map<string,string>
        newEntryValid: Result<'tnewrec,string>
        lastError: string option
    }
    type Msg<'trec> =
        | RefreshData
        | SetLastError of string option
        | SaveChanges
        | SetNewField of string * string
        | ValidateNewEntry
        | SaveNewEntry
        | DeleteEntry of record_id: int
        | ReceivedData of Result<'trec list,string>

open ServerProtocol.V1

type Model = TabularFormTypes.Model<User, CreateUserPayload>
type Msg = TabularFormTypes.Msg<User>
