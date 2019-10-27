module ManageUsers.Types

module TabularFormTypes =
    type ModelState<'trec> =
        | Init
        | Loading
        | Data of 'trec list
    type Model<'trec,'tnewrec> = {
        token: ServerComm.Token
        apiUrl: string
        data: 'trec ModelState
        newEntry: Map<string,string>
        newEntryValid: Result<'tnewrec,string>
    }
    type Msg<'trec> =
        | RefreshData
        | SaveChanges
        | SetNewField of string * string
        | ValidateNewEntry
        | SaveNewEntry
        | DeleteEntry of record_id: int
        | ReceivedData of Result<'trec list,string>

open ServerProtocol.V1

type Model = TabularFormTypes.Model<User, CreateUserInfo>
type Msg = TabularFormTypes.Msg<User>
