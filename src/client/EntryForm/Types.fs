module EntryForm.Types

open CommonTypes

type DataRecord = {
    rtime: string
    meal: string
    amount: float
}

type ApiUri = ApiUri of string
type Model = Components.TabularForms.Model<EntryId, DataRecord, ApiUri>
type Msg = Components.TabularForms.Msg<EntryId,DataRecord>
    