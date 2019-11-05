module EntryForm.Types

open CommonTypes
open ServerProtocol.V1

type ApiUri = ApiUri of string
type Model = Components.TabularForms.Model<UserData, PostDataPayload, ApiUri>
type Msg = Components.TabularForms.Msg<UserData, EntryId>
    