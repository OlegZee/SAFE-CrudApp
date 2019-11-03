module EntryForm.Types

open CommonTypes
open ServerProtocol.V1

type CustomData = { token: Token; api: string }
type Model = Components.TabularForms.Model<UserData, PostDataPayload, CustomData>
type Msg = Components.TabularForms.Msg<UserData, EntryId>
    